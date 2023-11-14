using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

// Message Types
using RosMessageTypes.Gazebo;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter;
using System;

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

    public GameObject robotModel;

    Dictionary<string, GameObject> activeModels;

    void Start()
    {
        // Init variables
        activeModels = new Dictionary<string, GameObject>();

        // register the services with ROS
        ROSConnection.GetOrCreateInstance().ImplementService<SpawnModelRequest, SpawnModelResponse>(SpawnServiceName, HandleSpawn);
        ROSConnection.GetOrCreateInstance().ImplementService<DeleteModelRequest, DeleteModelResponse>(DeleteServiceName, HandleDelete);
        ROSConnection.GetOrCreateInstance().ImplementService<SetModelStateRequest, SetModelStateResponse>(MoveServiceName, HandleState);
        ROSConnection.GetOrCreateInstance().Subscribe<PoseStampedMsg>(GoalServiceName, HandleGoal);
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

        objectToMove.transform.position = new Vector3(
            (float)pose.position.x,
            (float)pose.position.z,
            (float)pose.position.y
        );
        objectToMove.transform.rotation = new Quaternion(
            (float)pose.orientation.x,
            (float)pose.orientation.z,
            (float)pose.orientation.y,
            (float)pose.orientation.w
        );

        return new SetModelStateResponse(true, "Model moved");
    }

    private SpawnModelResponse HandleSpawn(SpawnModelRequest request)
    {
        // process the service request
        GameObject entity;

        if (request.model_name == "burger")
        {
            // entity = Utils.CreateGameObjectFromUrdfFile(
            //     "Assets/turtlebot3/turtlebot3_burger.urdf", // replace by urdf path
            //     request.model_name + "_manually_loaded"
            // );
            entity = Instantiate(robotModel);
            // Set up Odom
            Odom odom = entity.AddComponent(typeof(Odom)) as Odom;
            odom.topicNamespace = request.robot_namespace;
            odom.frameId = request.model_name + "/odom";
            odom.childFrameId = request.model_name + "/base_footprint";

            // Set up Drive
            Drive drive = entity.AddComponent(typeof(Drive)) as Drive;
            drive.topicNamespace = request.robot_namespace;

            // Set up Scan
            var scanComponentName = "base_scan";
            GameObject laserLink = FindSubChild(entity, scanComponentName);
            Scan laserScan = laserLink.AddComponent(typeof(Scan)) as Scan;
            laserScan.topicNamespace = request.robot_namespace;
            laserScan.frameId = request.model_name + "/" + scanComponentName;
            // temporarily set manually
            laserScan.minAngle = 0f;
            laserScan.maxAngle = 6.5f;
            laserScan.range = 3.5f;
            laserScan.numBeams = 100;
            laserScan.updateRate = 10;

            // Set up Tf for each UrdfLink
            AddTfToChildren(entity, request.model_name);
        }
        else
        {
            // Standard Object
            entity = GameObject.CreatePrimitive(PrimitiveType.Cube);
            entity.name = request.model_name;
        }

        PoseMsg initialPose = request.initial_pose;
        SetInitialPose(entity, initialPose);

        Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = false;

        // add to active models to delete later
        activeModels.Add(request.model_name, entity);

        return new SpawnModelResponse(true, "Received Spawn Request");
    }

    private void HandleGoal(PoseStampedMsg msg)
    {
        Debug.Log(msg.ToString());
    }

    /// <summary> Sets the position and rotation according to initPos </summary>
    private void SetInitialPose(GameObject obj, PoseMsg initPos)
    {
        obj.transform.position = new Vector3(
            (float)initPos.position.x,
            (float)initPos.position.y,
            (float)initPos.position.z
        );
        obj.transform.rotation = new Quaternion(
            (float)initPos.orientation.x,
            (float)initPos.orientation.y,
            (float)initPos.orientation.z,
            (float)initPos.orientation.w
        );
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

    private void AddTfToChildren(GameObject gameObject, String ns)
    {
        foreach (UrdfLink link in gameObject.GetComponentsInChildren<UrdfLink>())
        {
            Tf tf = link.gameObject.AddComponent(typeof(Tf)) as Tf;
            tf.childFrameId = ns + "/" + link.gameObject.name;
            var parentName = link.transform.parent.gameObject.name;
            tf.frameId = ns + "/" + parentName;
            if (parentName == gameObject.name)
            {
                tf.frameId = ns + "/odom";
            }
        }
    }
}