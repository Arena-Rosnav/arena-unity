using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Geometry;

public class Drive : MonoBehaviour {
    public string topicNamespace;

    public Vector3 minLinearVelocity;
    public Vector3 maxLinearVelocity;

    public Vector3 minAngularVelocity;
    public Vector3 maxAngularVelocity;

    string topic;

    Vector3 linearVelocity = new Vector3(0, 0, 0);
    Vector3 angularVelocity = new Vector3(0, 0, 0); 

    void Start() {
        topic = topicNamespace + "/cmd_vel";

        ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>(topic, CmdVel);
    }

    void Update() {
        Quaternion rotation = Quaternion.Euler(
            angularVelocity.x * Time.deltaTime * Mathf.Rad2Deg, 
            angularVelocity.y * Time.deltaTime * Mathf.Rad2Deg, 
            angularVelocity.z * Time.deltaTime * Mathf.Rad2Deg
        );

        transform.rotation *= rotation;

        transform.position += transform.rotation * linearVelocity * Time.deltaTime;
    }

    void CmdVel(TwistMsg message) {
        Vector3Msg linear = message.linear;
        Vector3Msg angular = message.angular;

        // CHANGE Y AND Z AXIS
        linearVelocity = new Vector3(
            (float) linear.x,
            (float) linear.z,
            (float) linear.y
        );

        angularVelocity = new Vector3(
            (float) angular.x,
            (float) angular.z,
            (float) angular.y
        );
    }
}
