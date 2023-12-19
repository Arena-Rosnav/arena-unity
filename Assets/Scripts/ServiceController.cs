using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

// Message Types
using RosMessageTypes.Gazebo;
using RosMessageTypes.Geometry;

/// <summary>
/// Example demonstration of implementing a UnityService that receives a Request message from another ROS node and sends a Response back
/// </summary>
public class ServiceController : MonoBehaviour
{
    [SerializeField]
    string SpawnServiceName = "unity/spawn_model";
    [SerializeField]
    string DeleteServiceName = "unity/delete_model";
    [SerializeField]
    string MoveServiceName = "unity/set_model_state";
    [SerializeField]
    string GoalServiceName = "unity/set_goal";

    Dictionary<string, GameObject> activeModels;

    void Start()
    {
        // Init variables
        activeModels = new Dictionary<string, GameObject>();

        // register the services with ROS
        ROSConnection ros_con = ROSConnection.GetOrCreateInstance();
        ros_con.ImplementService<SpawnModelRequest, SpawnModelResponse>(SpawnServiceName, HandleSpawn);
        ros_con.ImplementService<DeleteModelRequest, DeleteModelResponse>(DeleteServiceName, HandleDelete);
        ros_con.ImplementService<SetModelStateRequest, SetModelStateResponse>(MoveServiceName, HandleState);
        ros_con.Subscribe<PoseStampedMsg>(GoalServiceName, HandleGoal);
    }

    /// HANDLER SECTION
    private DeleteModelResponse HandleDelete(DeleteModelRequest request)
    {
        // Delete object from active Models if exists
        string name = request.model_name;

        if (!activeModels.ContainsKey(name))
            return new DeleteModelResponse(false, "Model with name " + name + " does not exist.");

        Destroy(activeModels[name]);
        activeModels.Remove(name);

        return new DeleteModelResponse(true, "Model with name " + name + " deleted.");
    }

    private SetModelStateResponse HandleState(SetModelStateRequest request)
    {
        Debug.Log(request);
        string name = request.model_name;

        // check if the model really exists
        if (!activeModels.ContainsKey(name))
            return new SetModelStateResponse(false, "Model with name " + name + " does not exist.");

        // Move the object
        PoseMsg pose = request.pose;
        GameObject objectToMove = activeModels[name];

        Utils.SetPose(objectToMove, pose);

        return new SetModelStateResponse(true, "Model moved");
    }

    private SpawnModelResponse HandleSpawn(SpawnModelRequest request)
    {
        // process the service request
        GameObject entity = Utils.CreateGameObjectFromUrdfFile(
            request.model_xml,
            request.model_name,
            disableJoints:true,
            disableScripts:true,
            parent:null
        );

            // entity = Instantiate(robotModel,
            //     new Vector3(
            //     // (float)request.initial_pose.position.x,
            //     -20.0f,
            //     (float)request.initial_pose.position.y,
            //     // (float)request.initial_pose.position.z
            //     24.0f
            // ), new Quaternion(
            //     (float)request.initial_pose.orientation.x,
            //     (float)request.initial_pose.orientation.y,
            //     (float)request.initial_pose.orientation.z,
            //     (float)request.initial_pose.orientation.w
            // ));

            // // Set up TF
            // entity.transform.GetChild(1).gameObject.AddComponent(typeof(ROSTransformTreePublisher));

            // // Set up Drive
            // Drive drive = entity.AddComponent(typeof(Drive)) as Drive;
            // drive.topicNamespace = request.robot_namespace;
            // // temp manually only for burger
            // drive.wA1 = FindSubChild(entity, "burger/wheel_right_link").GetComponent<ArticulationBody>();
            // drive.wA2 = FindSubChild(entity, "burger/wheel_left_link").GetComponent<ArticulationBody>();

            // // Set up Scan
            // var scanComponentName = "burger/base_scan";
            // GameObject laserLink = FindSubChild(entity, scanComponentName);
            // LaserScanSensor laserScan = laserLink.AddComponent(typeof(LaserScanSensor)) as LaserScanSensor;
            // laserScan.topic = "/burger/scan";
            // laserScan.frameId = scanComponentName;


        Utils.SetPose(entity, request.initial_pose);

        // Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        // rb.useGravity = false;

        // add to active models to delete later
        activeModels.Add(request.model_name, entity);

        return new SpawnModelResponse(true, "Received Spawn Request");
    }

    private void HandleGoal(PoseStampedMsg msg)
    {
        Debug.Log(msg.ToString());
    }

    private GameObject FindSubChild(GameObject gameObject, string objName)
    {
        if (gameObject.name == objName)
            return gameObject;

        foreach (Transform t in gameObject.transform)
        {
            GameObject possibleLaserLink = FindSubChild(t.gameObject, objName);

            if (possibleLaserLink)
                return possibleLaserLink;

        }
        // nothing found
        return null;
    }
}