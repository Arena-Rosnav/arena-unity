using System.Collections.Generic;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.Core;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.Serialization;
using System;


public class LaserScanSensor : MonoBehaviour
{
    const string laserScanTopicName = "scan";
    public string topicNamespace = "";
    [FormerlySerializedAs("TimeBetweenScansSeconds")]
    public float RangeMetersMin = 0;

    // jackal
    public double PublishPeriodSeconds = 0.1;
    public float RangeMetersMax = 30f;
    public float ScanAngleStartDegrees = 135;
    public float ScanAngleEndDegrees = -135;
    public int NumMeasurementsPerScan = 720;

    // Change the scan start and end by this amount after every publish
    public float ScanOffsetAfterPublish = 0f;
    
    public string frameId = "base_scan";
    ROSConnection m_Ros;
    double m_TimeNextScanSeconds = -1;
    int m_NumMeasurementsTaken;
    List<float> ranges = new List<float>();

    bool isScanning = false;
    double m_TimeLastScanBeganSeconds = -1;

    string PublishTopic => topicNamespace+ "/" + laserScanTopicName;

    void Start()
    {
        m_Ros = FindObjectOfType<ROSConnection>();
        m_Ros.RegisterPublisher<LaserScanMsg>(PublishTopic);

        m_TimeNextScanSeconds = Clock.Now + PublishPeriodSeconds;
    }

    /// <summary>
    /// Configures the laser scan. The scan rate, range, number of scans per scan
    /// and start and end angle degrees of the scan are configured.
    /// </summary>
    /// <param name="laserConfig">Dictionary which should be extracted from ROBOT_NAME.model.yaml.</param>
    /// <returns>True if all values have been successfully configued, false otherwise.</returns>
    public bool ConfigureScan(Dictionary<string, object> laserConfig)
    {
        bool success = true;

        // configure range
        if (laserConfig.TryGetValue("range", out object range) && float.TryParse((string)range, out float rangeVal))
            RangeMetersMax = rangeVal;
        else
        {
            Debug.LogError("Laser config dictionary doesn't contain 'range' or value not a valid float.\nUsing default jackal values.");
            success = false;
        }

        // configure update rate
        if (laserConfig.TryGetValue("update_rate", out object updateRate) && float.TryParse((string)updateRate, out float updateRateValHz))
            PublishPeriodSeconds = 1f / updateRateValHz;
        else
        {
            Debug.LogError("Laser config dictionary doesn't contain 'update_rate' or value is not valid float.\nUsing default jackal values.");
            success = false;
        }
        
        // configure angles
        if (laserConfig.TryGetValue("angle", out object angleConfig) && angleConfig is Dictionary<object, object> angleConfigDict)
        {
            // configure scan start and end angles
            if (angleConfigDict.TryGetValue("min", out object minRad) && float.TryParse((string)minRad, out float minRadVal))
                ScanAngleStartDegrees = minRadVal * Mathf.Rad2Deg;
            else
            {
                Debug.LogError("Angle config in laser config dictionary doesn't contain 'min' or value not a valid float.\nUsing default jackal values.");
                // return directly -> NumMeasurements not messed up
                return false;
            }
            if (angleConfigDict.TryGetValue("max", out object maxRad) && float.TryParse((string)maxRad, out float maxRadVal))
                ScanAngleEndDegrees = maxRadVal * Mathf.Rad2Deg;
            else
            {
                Debug.LogError("Angle config in laser config dictionary doesn't contain 'max' or value not a valid float.\nUsing default jackal values.");
                // return directly -> NumMeasurements not messed up
                return false;
            }

            // configure number of measurements per scan
            if (angleConfigDict.TryGetValue("increment", out object incrementRad) && double.TryParse((string)incrementRad, out double incrementRadVal))
                NumMeasurementsPerScan = (int)Math.Round((maxRadVal - minRadVal) / incrementRadVal);
            else
            {
                Debug.LogError("Angle config in laser config dictionary doesn't contain 'increment' or value not a valid double.\nUsing default jackal values.");
                success = false;
            }
        } else
        {
            Debug.LogError("Laser config dictionary doesn't contain 'angle' key or value not a dictionary.\nUsing default jackal values.");
            return false;
        }

        return success;
    }

    private void BeginScan()
    {
        isScanning = true;
        m_TimeLastScanBeganSeconds = Clock.Now;
        m_TimeNextScanSeconds = m_TimeLastScanBeganSeconds + PublishPeriodSeconds;
        m_NumMeasurementsTaken = 0;
    }

    private void EndScan()
    {
        if (ranges.Count == 0)
        {
            Debug.LogWarning($"Took {m_NumMeasurementsTaken} measurements but found no valid ranges");
        }
        else if (ranges.Count != NumMeasurementsPerScan)
        {
            Debug.LogWarning($"Expected {NumMeasurementsPerScan} measurements. Actually took {m_NumMeasurementsTaken}" +
                             $"and recorded {ranges.Count} ranges.");
        }

        TimeStamp timestamp = new TimeStamp(Clock.time);
        // Invert the angle ranges when going from Unity to ROS
        var angleStartRos = -ScanAngleStartDegrees * Mathf.Deg2Rad;
        var angleEndRos = -ScanAngleEndDegrees * Mathf.Deg2Rad;

        var msg = new LaserScanMsg
        {
            header = new HeaderMsg
            {
                frame_id = frameId,
                stamp = new TimeMsg
                {
                    sec = (uint)timestamp.Seconds,
                    nanosec = timestamp.NanoSeconds,
                }
            },
            range_min = RangeMetersMin,
            range_max = RangeMetersMax,
            angle_min = angleStartRos,
            angle_max = angleEndRos,
            angle_increment = (angleEndRos - angleStartRos) / NumMeasurementsPerScan,
            time_increment = 0f,
            scan_time = (float)PublishPeriodSeconds,
            intensities = new float[0],
            ranges = ranges.ToArray(),
        };

        m_Ros.Publish(PublishTopic, msg);

        m_NumMeasurementsTaken = 0;
        ranges.Clear();
        isScanning = false;
        var now = (float)Clock.time;
        if (now > m_TimeNextScanSeconds)
        {
            Debug.LogWarning($"Failed to complete scan started at {m_TimeLastScanBeganSeconds:F} before next scan was " +
                             $"scheduled to start: {m_TimeNextScanSeconds:F}, rescheduling to now ({now:F})");
            m_TimeNextScanSeconds = now;
        }
    }

    private void DoScan() 
    {
        float yawBaseDegrees = transform.rotation.eulerAngles.y;
        float yawDegrees = ScanAngleStartDegrees + yawBaseDegrees + ScanOffsetAfterPublish;
        float angleIncrement = (ScanAngleEndDegrees - ScanAngleStartDegrees) / NumMeasurementsPerScan;

        for (int numMeasurements = 0; numMeasurements < NumMeasurementsPerScan; numMeasurements++)
        {
            var directionVector = Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.forward;
            var measurementStart = RangeMetersMin * directionVector + transform.position;
            var measurementRay = new Ray(measurementStart, directionVector);
            // Only record measurement if it's within the sensor's operating range
            var foundValidMeasurement = Physics.Raycast(measurementRay, out var hit, RangeMetersMax);
            // Even if Raycast didn't find a valid hit, we still count it as a measurement
            if (foundValidMeasurement)
            {
                ranges.Add(hit.distance);
            }
            else
            {
                ranges.Add(float.NaN);
            }

            // Update scan degree
            yawDegrees += angleIncrement;
            ++m_NumMeasurementsTaken;
        }
    }

    void Update()
    {
        if (!isScanning)
        {
            if (Clock.NowTimeInSeconds < m_TimeNextScanSeconds)
            {
                return;
            }

            BeginScan();
            DoScan();
            EndScan();
        }
    }
}
