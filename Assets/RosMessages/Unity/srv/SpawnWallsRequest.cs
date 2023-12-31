//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Unity
{
    [Serializable]
    public class SpawnWallsRequest : Message
    {
        public const string k_RosMessageName = "unity_msgs/SpawnWalls";
        public override string RosMessageName => k_RosMessageName;

        public WallMsg[] walls;

        public SpawnWallsRequest()
        {
            this.walls = new WallMsg[0];
        }

        public SpawnWallsRequest(WallMsg[] walls)
        {
            this.walls = walls;
        }

        public static SpawnWallsRequest Deserialize(MessageDeserializer deserializer) => new SpawnWallsRequest(deserializer);

        private SpawnWallsRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.walls, WallMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.walls);
            serializer.Write(this.walls);
        }

        public override string ToString()
        {
            return "SpawnWallsRequest: " +
            "\nwalls: " + System.String.Join(", ", walls.ToList());
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
