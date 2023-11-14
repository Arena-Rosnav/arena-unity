using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using RosMessageTypes.Rosgraph;
using RosMessageTypes.BuiltinInterfaces;

public class Odom : MonoBehaviour
{
    public string topicNamespace;

    public string frameId;
    public string childFrameId;

    ROSConnection ros;
    string topicName;

    uint seq;

    double[] poseCovariance;
    double[] twistCovariance;

    public Vector3 rotation;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        topicName = topicNamespace + "/odom";
        poseCovariance = new double[36];
        twistCovariance = new double[36];

        rb = GetComponent<Rigidbody>();

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OdometryMsg>(topicName);
        ros.RegisterPublisher<TFMessageMsg>("/tf");
    }

    // Update is called once per frame
    void Update()
    {
        // Odom Message
        PublishOdom();
    }

    private void PublishOdom()
    {
        PoseWithCovarianceMsg poseWithCovariance = new PoseWithCovarianceMsg(
            new PoseMsg(
                new PointMsg(
                    transform.position.x,
                    transform.position.z,
                    transform.position.y
                ),
                new QuaternionMsg(
                    transform.rotation.x,
                    transform.rotation.z,
                    transform.rotation.y,
                    transform.rotation.w
                )
            ),
            poseCovariance
        );

        TwistWithCovarianceMsg twistWithCovariance = new TwistWithCovarianceMsg(
            new TwistMsg(
                // TODO
                new Vector3Msg(
                    rb.velocity.x,
                    rb.velocity.z,
                    rb.velocity.y
                ),
                new Vector3Msg(
                    rb.angularVelocity.x,
                    rb.angularVelocity.z,
                    rb.angularVelocity.y
                )
            ),
            twistCovariance
        );

        HeaderMsg headerMsg = new HeaderMsg(
            seq,
            Clock.GetTimeMsg(),
            frameId // TODO
        );

        OdometryMsg odometryMsg = new OdometryMsg(
            headerMsg,
            childFrameId,
            poseWithCovariance,
            twistWithCovariance
        );

        ros.Publish(topicName, odometryMsg);

        seq++;
    }

}
