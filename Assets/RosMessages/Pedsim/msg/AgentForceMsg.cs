//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Pedsim
{
    [Serializable]
    public class AgentForceMsg : Message
    {
        public const string k_RosMessageName = "pedsim_msgs/AgentForce";
        public override string RosMessageName => k_RosMessageName;

        //  Forces acting on an agent.
        //  Max Speed
        public double vmax;
        //  Force Factors
        public double desired_ffactor;
        public double obstacle_ffactor;
        public double social_ffactor;
        public double robot_ffactor;
        //  Basic SFM forces.
        public Geometry.Vector3Msg desired_force;
        public Geometry.Vector3Msg obstacle_force;
        public Geometry.Vector3Msg social_force;
        //  Additional Group Forces
        public Geometry.Vector3Msg group_coherence_force;
        public Geometry.Vector3Msg group_gaze_force;
        public Geometry.Vector3Msg group_repulsion_force;
        //  Extra stabilization/custom forces.
        public Geometry.Vector3Msg random_force;
        public Geometry.Vector3Msg keep_distance_force;
        public Geometry.Vector3Msg robot_force;
        //  Total forces
        public Geometry.Vector3Msg force;

        public AgentForceMsg()
        {
            this.vmax = 0.0;
            this.desired_ffactor = 0.0;
            this.obstacle_ffactor = 0.0;
            this.social_ffactor = 0.0;
            this.robot_ffactor = 0.0;
            this.desired_force = new Geometry.Vector3Msg();
            this.obstacle_force = new Geometry.Vector3Msg();
            this.social_force = new Geometry.Vector3Msg();
            this.group_coherence_force = new Geometry.Vector3Msg();
            this.group_gaze_force = new Geometry.Vector3Msg();
            this.group_repulsion_force = new Geometry.Vector3Msg();
            this.random_force = new Geometry.Vector3Msg();
            this.keep_distance_force = new Geometry.Vector3Msg();
            this.robot_force = new Geometry.Vector3Msg();
            this.force = new Geometry.Vector3Msg();
        }

        public AgentForceMsg(double vmax, double desired_ffactor, double obstacle_ffactor, double social_ffactor, double robot_ffactor, Geometry.Vector3Msg desired_force, Geometry.Vector3Msg obstacle_force, Geometry.Vector3Msg social_force, Geometry.Vector3Msg group_coherence_force, Geometry.Vector3Msg group_gaze_force, Geometry.Vector3Msg group_repulsion_force, Geometry.Vector3Msg random_force, Geometry.Vector3Msg keep_distance_force, Geometry.Vector3Msg robot_force, Geometry.Vector3Msg force)
        {
            this.vmax = vmax;
            this.desired_ffactor = desired_ffactor;
            this.obstacle_ffactor = obstacle_ffactor;
            this.social_ffactor = social_ffactor;
            this.robot_ffactor = robot_ffactor;
            this.desired_force = desired_force;
            this.obstacle_force = obstacle_force;
            this.social_force = social_force;
            this.group_coherence_force = group_coherence_force;
            this.group_gaze_force = group_gaze_force;
            this.group_repulsion_force = group_repulsion_force;
            this.random_force = random_force;
            this.keep_distance_force = keep_distance_force;
            this.robot_force = robot_force;
            this.force = force;
        }

        public static AgentForceMsg Deserialize(MessageDeserializer deserializer) => new AgentForceMsg(deserializer);

        private AgentForceMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.vmax);
            deserializer.Read(out this.desired_ffactor);
            deserializer.Read(out this.obstacle_ffactor);
            deserializer.Read(out this.social_ffactor);
            deserializer.Read(out this.robot_ffactor);
            this.desired_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.obstacle_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.social_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.group_coherence_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.group_gaze_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.group_repulsion_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.random_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.keep_distance_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.robot_force = Geometry.Vector3Msg.Deserialize(deserializer);
            this.force = Geometry.Vector3Msg.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.vmax);
            serializer.Write(this.desired_ffactor);
            serializer.Write(this.obstacle_ffactor);
            serializer.Write(this.social_ffactor);
            serializer.Write(this.robot_ffactor);
            serializer.Write(this.desired_force);
            serializer.Write(this.obstacle_force);
            serializer.Write(this.social_force);
            serializer.Write(this.group_coherence_force);
            serializer.Write(this.group_gaze_force);
            serializer.Write(this.group_repulsion_force);
            serializer.Write(this.random_force);
            serializer.Write(this.keep_distance_force);
            serializer.Write(this.robot_force);
            serializer.Write(this.force);
        }

        public override string ToString()
        {
            return "AgentForceMsg: " +
            "\nvmax: " + vmax.ToString() +
            "\ndesired_ffactor: " + desired_ffactor.ToString() +
            "\nobstacle_ffactor: " + obstacle_ffactor.ToString() +
            "\nsocial_ffactor: " + social_ffactor.ToString() +
            "\nrobot_ffactor: " + robot_ffactor.ToString() +
            "\ndesired_force: " + desired_force.ToString() +
            "\nobstacle_force: " + obstacle_force.ToString() +
            "\nsocial_force: " + social_force.ToString() +
            "\ngroup_coherence_force: " + group_coherence_force.ToString() +
            "\ngroup_gaze_force: " + group_gaze_force.ToString() +
            "\ngroup_repulsion_force: " + group_repulsion_force.ToString() +
            "\nrandom_force: " + random_force.ToString() +
            "\nkeep_distance_force: " + keep_distance_force.ToString() +
            "\nrobot_force: " + robot_force.ToString() +
            "\nforce: " + force.ToString();
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
