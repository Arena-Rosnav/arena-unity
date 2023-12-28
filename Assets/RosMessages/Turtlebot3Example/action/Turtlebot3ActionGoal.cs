using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Turtlebot3Example
{
    public class Turtlebot3ActionGoal : ActionGoal<Turtlebot3Goal>
    {
        public const string k_RosMessageName = "turtlebot3_example/Turtlebot3ActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public Turtlebot3ActionGoal() : base()
        {
            this.goal = new Turtlebot3Goal();
        }

        public Turtlebot3ActionGoal(HeaderMsg header, GoalIDMsg goal_id, Turtlebot3Goal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static Turtlebot3ActionGoal Deserialize(MessageDeserializer deserializer) => new Turtlebot3ActionGoal(deserializer);

        Turtlebot3ActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = Turtlebot3Goal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
