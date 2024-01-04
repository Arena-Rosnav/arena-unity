using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Turtlebot3Example
{
    public class Turtlebot3ActionFeedback : ActionFeedback<Turtlebot3Feedback>
    {
        public const string k_RosMessageName = "turtlebot3_example/Turtlebot3ActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public Turtlebot3ActionFeedback() : base()
        {
            this.feedback = new Turtlebot3Feedback();
        }

        public Turtlebot3ActionFeedback(HeaderMsg header, GoalStatusMsg status, Turtlebot3Feedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static Turtlebot3ActionFeedback Deserialize(MessageDeserializer deserializer) => new Turtlebot3ActionFeedback(deserializer);

        Turtlebot3ActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = Turtlebot3Feedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
