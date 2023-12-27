using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.Core;

// ROS messages
using RosMessageTypes.Nav;
using RosMessageTypes.Gazebo;
using RosMessageTypes.Geometry;

public class OdomPublisher : MonoBehaviour
{
    const string odomTopicName = "odom";
    double publishRateHz = 20f;
    double lastPublishTimeSeconds;
    ROSConnection rosConnection;
    string robotName;
    // Drive for reading velocity
    Drive robotDrive;

    double PublishPeriodSeconds => 1.0f / publishRateHz;

    bool ShouldPublishMessage => Clock.NowTimeInSeconds > lastPublishTimeSeconds + PublishPeriodSeconds;

    // Start is called before the first frame update
    void Start()
    {
        robotName = gameObject.transform.parent.name;
        rosConnection = ROSConnection.GetOrCreateInstance();
        rosConnection.RegisterPublisher<OdometryMsg>("/" + robotName + "/" + odomTopicName);
        lastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;

        robotDrive = gameObject.transform.parent.GetComponent<Drive>();
        if (robotDrive == null)
        {
            Debug.LogError("No Drive component in robot found!");
        }
    }

    void PublishMessage()
    {
        PoseWithCovarianceMsg pose = new(
            new PoseMsg(
                transform.position.To<FLU>(),
                transform.rotation.To<FLU>()
            ),
            new double[36]
        );
        TwistWithCovarianceMsg twist = new(
            new TwistMsg(
                robotDrive.linearVelocity.To<FLU>(), 
                robotDrive.angularVelocity.To<FLU>()
            ),
            new double[36]
        );
        OdometryMsg msg = new(new RosMessageTypes.Std.HeaderMsg())
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldPublishMessage)
        {
            PublishMessage();
        }
    }
}
