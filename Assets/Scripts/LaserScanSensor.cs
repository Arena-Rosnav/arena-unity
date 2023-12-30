using System.Collections.Generic;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.Core;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.Serialization;


public class LaserScanSensor : MonoBehaviour
{
    public string topic;
    [FormerlySerializedAs("TimeBetweenScansSeconds")]
    public double PublishPeriodSeconds = 0.1;
    public float RangeMetersMin = 0;

    // burger
    // public float RangeMetersMax = 3.5f;
    // public float ScanAngleStartDegrees = 0;
    // public float ScanAngleEndDegrees = 360;
    // public int NumMeasurementsPerScan = 360;

    // jackal
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

    protected virtual void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<LaserScanMsg>(topic);

        m_TimeNextScanSeconds = Clock.Now + PublishPeriodSeconds;
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
        // var angleStartRos = -ScanAngleStartDegrees * Mathf.Deg2Rad;
        // var angleEndRos = -ScanAngleEndDegrees * Mathf.Deg2Rad;
        if (angleStartRos > angleEndRos)
        {
            Debug.LogWarning("LaserScan was performed in a clockwise direction but ROS expects a counter-clockwise scan, flipping the ranges...");
            var temp = angleEndRos;
            angleEndRos = angleStartRos;
            angleStartRos = temp;
            ranges.Reverse();
        }

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
            intensities = new float[ranges.Count],
            ranges = ranges.ToArray(),
        };

        m_Ros.Publish(topic, msg);

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
                ranges.Add(float.MaxValue);
            }

            // Update scan degree
            yawDegrees += angleIncrement;
            ++m_NumMeasurementsTaken;
        }
    }

    public void Update()
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
