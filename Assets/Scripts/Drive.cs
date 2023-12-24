using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace RobotComponent 
{
    public class Drive : MonoBehaviour
    {
        public string topicNamespace;
        string topic;

        Vector3 linearVelocity = new Vector3(0, 0, 0);
        Vector3 angularVelocity = new Vector3(0, 0, 0); 

        void Start() {
            topic = "/" + topicNamespace + "/cmd_vel";

            ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>(topic, CmdVel);
        }

        void Update() {
            Quaternion rotation = Quaternion.Euler(
                angularVelocity.x * Time.deltaTime * Mathf.Rad2Deg, 
                angularVelocity.y * Time.deltaTime * Mathf.Rad2Deg, 
                angularVelocity.z * Time.deltaTime * Mathf.Rad2Deg
            );

            transform.rotation *= rotation;
            // transform.rotation = rotation; 

            transform.position += transform.rotation * linearVelocity * Time.deltaTime;
            // transform.position += linearVelocity * Time.deltaTime;
        }

        void CmdVel(TwistMsg message) {
            // Vector3Msg linear = message.linear;
            // Vector3Msg angular = message.angular;

            linearVelocity = message.linear.From<FLU>();
            angularVelocity = message.angular.From<FLU>();

            /*
            // CHANGE Y AND Z AXIS
            linearVelocity = new Vector3(
                (float) -linear.y,
                (float) linear.z,
                (float) linear.x
            );

            angularVelocity = new Vector3(
                (float) angular.x,
                (float) -angular.z,
                (float) angular.y
            );
            */
        }

    }
}