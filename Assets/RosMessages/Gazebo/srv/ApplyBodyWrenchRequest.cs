//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.BuiltinInterfaces;

namespace RosMessageTypes.Gazebo
{
    [Serializable]
    public class ApplyBodyWrenchRequest : Message
    {
        public const string k_RosMessageName = "gazebo_msgs/ApplyBodyWrench";
        public override string RosMessageName => k_RosMessageName;

        //  Apply Wrench to Gazebo Body.
        //  via the callback mechanism
        //  all Gazebo operations are made in world frame
        public string body_name;
        //  Gazebo body to apply wrench (linear force and torque)
        //  wrench is applied in the gazebo world by default
        //  body names are prefixed by model name, e.g. pr2::base_link
        public string reference_frame;
        //  wrench is defined in the reference frame of this entity
        //  use inertial frame if left empty
        //  frame names are bodies prefixed by model name, e.g. pr2::base_link
        public Geometry.PointMsg reference_point;
        //  wrench is defined at this location in the reference frame
        public Geometry.WrenchMsg wrench;
        //  wrench applied to the origin of the body
        public TimeMsg start_time;
        //  (optional) wrench application start time (seconds)
        //  if start_time is not specified, or
        //  start_time < current time, start as soon as possible
        public DurationMsg duration;
        //  optional duration of wrench application time (seconds)
        //  if duration < 0, apply wrench continuously without end
        //  if duration = 0, do nothing
        //  if duration < step size, apply wrench
        //  for one step size

        public ApplyBodyWrenchRequest()
        {
            this.body_name = "";
            this.reference_frame = "";
            this.reference_point = new Geometry.PointMsg();
            this.wrench = new Geometry.WrenchMsg();
            this.start_time = new TimeMsg();
            this.duration = new DurationMsg();
        }

        public ApplyBodyWrenchRequest(string body_name, string reference_frame, Geometry.PointMsg reference_point, Geometry.WrenchMsg wrench, TimeMsg start_time, DurationMsg duration)
        {
            this.body_name = body_name;
            this.reference_frame = reference_frame;
            this.reference_point = reference_point;
            this.wrench = wrench;
            this.start_time = start_time;
            this.duration = duration;
        }

        public static ApplyBodyWrenchRequest Deserialize(MessageDeserializer deserializer) => new ApplyBodyWrenchRequest(deserializer);

        private ApplyBodyWrenchRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.body_name);
            deserializer.Read(out this.reference_frame);
            this.reference_point = Geometry.PointMsg.Deserialize(deserializer);
            this.wrench = Geometry.WrenchMsg.Deserialize(deserializer);
            this.start_time = TimeMsg.Deserialize(deserializer);
            this.duration = DurationMsg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.body_name);
            serializer.Write(this.reference_frame);
            serializer.Write(this.reference_point);
            serializer.Write(this.wrench);
            serializer.Write(this.start_time);
            serializer.Write(this.duration);
        }

        public override string ToString()
        {
            return "ApplyBodyWrenchRequest: " +
            "\nbody_name: " + body_name.ToString() +
            "\nreference_frame: " + reference_frame.ToString() +
            "\nreference_point: " + reference_point.ToString() +
            "\nwrench: " + wrench.ToString() +
            "\nstart_time: " + start_time.ToString() +
            "\nduration: " + duration.ToString();
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
