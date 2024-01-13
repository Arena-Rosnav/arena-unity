using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Turtlebot3Example
{
    public class Turtlebot3Action : Action<Turtlebot3ActionGoal, Turtlebot3ActionResult, Turtlebot3ActionFeedback, Turtlebot3Goal, Turtlebot3Result, Turtlebot3Feedback>
    {
        public const string k_RosMessageName = "turtlebot3_example/Turtlebot3Action";
        public override string RosMessageName => k_RosMessageName;


        public Turtlebot3Action() : base()
        {
            this.action_goal = new Turtlebot3ActionGoal();
            this.action_result = new Turtlebot3ActionResult();
            this.action_feedback = new Turtlebot3ActionFeedback();
        }

        public static Turtlebot3Action Deserialize(MessageDeserializer deserializer) => new Turtlebot3Action(deserializer);

        Turtlebot3Action(MessageDeserializer deserializer)
        {
            this.action_goal = Turtlebot3ActionGoal.Deserialize(deserializer);
            this.action_result = Turtlebot3ActionResult.Deserialize(deserializer);
            this.action_feedback = Turtlebot3ActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
