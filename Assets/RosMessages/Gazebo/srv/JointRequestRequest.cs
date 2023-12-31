//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Gazebo
{
    [Serializable]
    public class JointRequestRequest : Message
    {
        public const string k_RosMessageName = "gazebo_msgs/JointRequest";
        public override string RosMessageName => k_RosMessageName;

        public string joint_name;
        //  name of the joint requested

        public JointRequestRequest()
        {
            this.joint_name = "";
        }

        public JointRequestRequest(string joint_name)
        {
            this.joint_name = joint_name;
        }

        public static JointRequestRequest Deserialize(MessageDeserializer deserializer) => new JointRequestRequest(deserializer);

        private JointRequestRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.joint_name);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.joint_name);
        }

        public override string ToString()
        {
            return "JointRequestRequest: " +
            "\njoint_name: " + joint_name.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
