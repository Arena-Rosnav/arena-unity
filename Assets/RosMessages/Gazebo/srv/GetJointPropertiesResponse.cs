//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Gazebo
{
    [Serializable]
    public class GetJointPropertiesResponse : Message
    {
        public const string k_RosMessageName = "gazebo_msgs/GetJointProperties";
        public override string RosMessageName => k_RosMessageName;

        //  joint type
        public byte type;
        public const byte REVOLUTE = 0; //  single DOF
        public const byte CONTINUOUS = 1; //  single DOF (revolute w/o joints)
        public const byte PRISMATIC = 2; //  single DOF
        public const byte FIXED = 3; //  0 DOF
        public const byte BALL = 4; //  3 DOF
        public const byte UNIVERSAL = 5; //  2 DOF
        //  dynamics properties
        public double[] damping;
        //  joint state
        public double[] position;
        public double[] rate;
        //  service return status
        public bool success;
        //  return true if get successful
        public string status_message;
        //  comments if available

        public GetJointPropertiesResponse()
        {
            this.type = 0;
            this.damping = new double[0];
            this.position = new double[0];
            this.rate = new double[0];
            this.success = false;
            this.status_message = "";
        }

        public GetJointPropertiesResponse(byte type, double[] damping, double[] position, double[] rate, bool success, string status_message)
        {
            this.type = type;
            this.damping = damping;
            this.position = position;
            this.rate = rate;
            this.success = success;
            this.status_message = status_message;
        }

        public static GetJointPropertiesResponse Deserialize(MessageDeserializer deserializer) => new GetJointPropertiesResponse(deserializer);

        private GetJointPropertiesResponse(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.type);
            deserializer.Read(out this.damping, sizeof(double), deserializer.ReadLength());
            deserializer.Read(out this.position, sizeof(double), deserializer.ReadLength());
            deserializer.Read(out this.rate, sizeof(double), deserializer.ReadLength());
            deserializer.Read(out this.success);
            deserializer.Read(out this.status_message);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.type);
            serializer.WriteLength(this.damping);
            serializer.Write(this.damping);
            serializer.WriteLength(this.position);
            serializer.Write(this.position);
            serializer.WriteLength(this.rate);
            serializer.Write(this.rate);
            serializer.Write(this.success);
            serializer.Write(this.status_message);
        }

        public override string ToString()
        {
            return "GetJointPropertiesResponse: " +
            "\ntype: " + type.ToString() +
            "\ndamping: " + System.String.Join(", ", damping.ToList()) +
            "\nposition: " + System.String.Join(", ", position.ToList()) +
            "\nrate: " + System.String.Join(", ", rate.ToList()) +
            "\nsuccess: " + success.ToString() +
            "\nstatus_message: " + status_message.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize, MessageSubtopic.Response);
        }
    }
}
