using UnityEngine;
using UnityEditor;
using System.IO;

// ROS msgs
using RosMessageTypes.Gazebo;


public class ObstacleController : MonoBehaviour
{
    public GameObject Cube;

    public GameObject SpawnObstacle(SpawnModelRequest request)
    {
        string obstPath = GetObstaclePath(request.model_xml);
        GameObject obstacle;
        GameObject prefab = Resources.Load<GameObject>(obstPath);
        if (prefab != null){
            obstacle = Instantiate(prefab);

            Utils.SetPose(obstacle, request.initial_pose);
        }else{
            obstacle = Instantiate(Cube);
            obstacle.tag = "Cube";

            Utils.SetCubePose(obstacle, request.initial_pose);
        }
        obstacle.name = request.robot_namespace;

        

        // Rigidbody rb = obstacle.AddComponent(typeof(Rigidbody)) as Rigidbody;
        // rb.useGravity = true;
        // rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        return obstacle;
    }

    // Checks if there exists an model in the arena-simulation-setup folder
    private static string GetObstaclePath(string name)
    {
        string filePath = Path.Combine("Obstacles", name, name);
        Debug.Log(filePath);
        return filePath;
    }
}