using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.InputSystem;

// Message Types
using RosMessageTypes.Gazebo;
using RosMessageTypes.Geometry;
using RosMessageTypes.Unity;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class ServiceController : MonoBehaviour
{
    [SerializeField]
    string SpawnServiceName = "unity/spawn_model";
    [SerializeField]
    string SpawnWallsServiceName = "unity/spawn_walls";
    [SerializeField]
    string DeleteServiceName = "unity/delete_model";
    [SerializeField]
    string MoveServiceName = "unity/set_model_state";
    [SerializeField]
    string GoalServiceName = "unity/set_goal";
    [SerializeField]
    string AttachSafeDistSensorServiceName = "unity/attach_safe_dist_sensor";
    Dictionary<string, GameObject> activeModels;
    GameObject obstaclesParent;
    GameObject wallsParent;
    GameObject pedsParent;
    public PedController pedController;
    public RobotController robotController;
    public ObstacleController obsController;
    CommandLineParser commandLineArgs;
    public GameObject Cube;
    string simNamespace;
    private bool robotSpawned;
    ROSConnection connection;
    public GameObject cameraObject;

    void Start()
    {
        // Init variables
        activeModels = new Dictionary<string, GameObject>();
        commandLineArgs = gameObject.AddComponent<CommandLineParser>();
        commandLineArgs.Initialize();
        robotSpawned = false;

        simNamespace = commandLineArgs.sim_namespace != null ? "/" + commandLineArgs.sim_namespace : "";

        // configure and connect ROS connection
        connection = SetRosConnection();
        // register the services with ROS
        connection.ImplementService<SpawnModelRequest, SpawnModelResponse>(simNamespace + "/" + SpawnServiceName, HandleSpawn);
        connection.ImplementService<SpawnWallsRequest, SpawnWallsResponse>(simNamespace + "/" + SpawnWallsServiceName, HandleWalls);
        connection.ImplementService<DeleteModelRequest, DeleteModelResponse>(simNamespace + "/" + DeleteServiceName, HandleDelete);
        connection.ImplementService<SetModelStateRequest, SetModelStateResponse>(simNamespace + "/" + MoveServiceName, HandleState);
        // connection.Subscribe<PoseStampedMsg>(GoalServiceName, HandleGoal);

        // initialize empty parent game object of obstacles (dynamic and static) & walls
        obstaclesParent = new("Obstacles");
        wallsParent = new("Walls");
        pedsParent = new("Peds");

        // remove camera in headless mode
        if (commandLineArgs.headless != null && commandLineArgs.headless.Equals("True"))
        {
            Destroy(cameraObject);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private ROSConnection SetRosConnection()
    {
        // get command line IP and port
        string ip = commandLineArgs.tcp_ip != null ? commandLineArgs.tcp_ip : "127.0.0.1";
        int port = commandLineArgs.tcp_port != null ? int.Parse(commandLineArgs.tcp_port) : 10000;
        ROSConnection rosConnector = GetComponent<ROSConnection>();
        // configure IP and port
        rosConnector.RosIPAddress = ip;
        rosConnector.RosPort = port;
        // connect with configured IP and port
        rosConnector.Connect();

        return rosConnector;
    }

    /// HANDLER SECTION
    private DeleteModelResponse HandleDelete(DeleteModelRequest request)
    {
        // Delete object from active Models if exists
        string entityName = request.model_name;

        if (!activeModels.ContainsKey(entityName))
            return new DeleteModelResponse(false, "Model with name " + entityName + " does not exist.");

        Destroy(activeModels[entityName]);
        activeModels.Remove(entityName);

        if (int.TryParse(entityName, out _))
        {
            pedController.DeletePed(entityName);
        }

        return new DeleteModelResponse(true, "Model with name " + entityName + " deleted.");
    }

    private SetModelStateResponse HandleState(SetModelStateRequest request)
    {
        string entityName = request.model_name;

        // check if the model really exists
        if (!activeModels.ContainsKey(entityName))
            return new SetModelStateResponse(false, "Model with name " + entityName + " does not exist.");

        // Move the object
        PoseMsg pose = request.pose;
        GameObject objectToMove = activeModels[entityName];

        if (objectToMove.CompareTag("Cube")) 
        {
            // It's a cube
            Utils.SetCubePose(objectToMove, pose);
        } 
        else 
        {
            Utils.SetPose(objectToMove, pose);
        }

        return new SetModelStateResponse(true, "Model moved");
    }

    private SpawnModelResponse HandleSpawn(SpawnModelRequest request)
    {
        GameObject entity;

        // decide between robots and peds and obstacles
        if (request.model_xml.Contains("<robot>") || request.model_xml.Contains("<robot "))
        {
            entity = robotController.SpawnRobot(request);
            
            // expose robot-specific services if robot is spawned for the first time
            if (!robotSpawned)
            {
                connection.ImplementService<AttachSafeDistSensorRequest, AttachSafeDistSensorResponse>(
                    simNamespace + "/" + AttachSafeDistSensorServiceName, 
                    HandleAttachSafeDistSensor
                );
            }
            robotSpawned = true;
        }
        else if (request.model_xml.Contains("<actor>") || request.model_xml.Contains("<actor "))
        {
            entity = pedController.SpawnPed(request);
            entity.transform.SetParent(pedsParent.transform);
            entity.layer = LayerMask.NameToLayer("Ped");
        }
        else
        {
            entity = obsController.SpawnObstacle(request);
            entity.transform.SetParent(obstaclesParent.transform);
            entity.layer = LayerMask.NameToLayer("Obs");
        }

        // add to active models to delete later
        activeModels.Add(entity.name, entity);

        return new SpawnModelResponse(true, "Received Spawn Request");
    }

    private void HandleGoal(PoseStampedMsg msg)
    {
        Debug.Log(msg.ToString());
    }

    private SpawnWallsResponse HandleWalls(SpawnWallsRequest request)
    {
        // Constants (move later)
        const string WALL_TAG = "Wall";

        // remove previous walls
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.tag == WALL_TAG)
            {
                Destroy(obj);
            }
        }

        // Add new walls 
        WallMsg[] walls = request.walls;
        int counter = 0;

        foreach (WallMsg wall in walls)
        {
            counter += 1;
            Vector3 corner_start = wall.start.From<FLU>();
            Vector3 corner_end = wall.end.From<FLU>();

            // Standard Cube
            GameObject entity = Instantiate(Cube);
            entity.name = "__WALL" + counter;
            entity.tag = WALL_TAG;
            entity.layer = LayerMask.NameToLayer("Obs");

            entity.transform.position = corner_start;
            entity.transform.localScale = corner_end - corner_start;
            AdjustPivot(entity.transform);

            // organize game object in walls parent game object
            entity.transform.SetParent(wallsParent.transform);
        }


        return new SpawnWallsResponse(true, "Walls successfully created");
    }

    private AttachSafeDistSensorResponse HandleAttachSafeDistSensor(
        AttachSafeDistSensorRequest request)
    {
        // Get referenced robot
        GameObject robot = activeModels.GetValueOrDefault(request.robot_name, null);
        if (robot == null)
            return new(false, "Provided robot name does not match an active robot!");

        // Try to configure and attach sensor
        bool success = robotController.AttachSafeDistSensor(robot, request);        

        return new AttachSafeDistSensorResponse(success, "");
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

    void AdjustPivot(Transform targetTransform)
    {
        // Get the bounds of the mesh
        Bounds bounds = targetTransform.GetComponent<MeshRenderer>().bounds;

        // Calculate the offset needed to move the pivot to the bottom-left corner
        Vector3 pivotOffset = new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);

        // Apply the offset to the position of the targetTransform
        targetTransform.position += pivotOffset;
    }
}
