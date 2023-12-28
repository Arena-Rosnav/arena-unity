using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Turtlebot3Example
{
    public class Turtlebot3ActionResult : ActionResult<Turtlebot3Result>
    {
        public const string k_RosMessageName = "turtlebot3_example/Turtlebot3ActionResult";
        public override string RosMessageName => k_RosMessageName;


        public Turtlebot3ActionResult() : base()
        {
            this.result = new Turtlebot3Result();
        }

        public Turtlebot3ActionResult(HeaderMsg header, GoalStatusMsg status, Turtlebot3Result result) : base(header, status)
        {
            this.result = result;
        }
        public static Turtlebot3ActionResult Deserialize(MessageDeserializer deserializer) => new Turtlebot3ActionResult(deserializer);

        Turtlebot3ActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = Turtlebot3Result.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
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
