using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.Core;

// ROS messages
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class OdomPublisher : MonoBehaviour
{
    const string odomTopicName = "odom";
    double publishRateHz = 30f;
    double lastPublishTimeSeconds;
    ROSConnection rosConnection;
    public string robotName = "jackal";
    // Drive for reading velocity
    Drive robotDrive;

    public string topicNamespace = "";

    string PublishTopic => topicNamespace+ "/" + odomTopicName;
    double PublishPeriodSeconds => 1.0f / publishRateHz;

    bool ShouldPublishMessage => Clock.NowTimeInSeconds > lastPublishTimeSeconds + PublishPeriodSeconds;

    // Start is called before the first frame update
    void Start()
    {
        rosConnection = FindObjectOfType<ROSConnection>();
        rosConnection.RegisterPublisher<OdometryMsg>(PublishTopic);
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
        OdometryMsg odomMsg = new(
            new HeaderMsg(0, new TimeStamp(Clock.time), robotName + "/" + odomTopicName),
            robotName + "/" + name,
            pose,
            twist
        );
        rosConnection.Publish(PublishTopic, odomMsg);
        lastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
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
