using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using YamlDotNet.Serialization;
using System.IO;
using DataObjects;

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
    Dictionary<string, GameObject> activeModels;
    GameObject obstaclesParent;
    GameObject wallsParent;
    GameObject pedsParent;
    public PedController pedController;
    CommandLineParser commandLineArgs;
    public GameObject Cube;
    string simNamespace;

    void Start()
    {           
        // Init variables
        activeModels = new Dictionary<string, GameObject>();
        commandLineArgs = gameObject.AddComponent<CommandLineParser>();
        commandLineArgs.Initialize();

        simNamespace = commandLineArgs.sim_namespace != null ? "/" + commandLineArgs.sim_namespace : "";

        // configure and connect ROS connection
        ROSConnection ros_con = SetRosConnection();
        // register the services with ROS
        ros_con.ImplementService<SpawnModelRequest, SpawnModelResponse>("/" + commandLineArgs.sim_namespace + "/" + SpawnServiceName, HandleSpawn);
        ros_con.ImplementService<SpawnWallsRequest, SpawnWallsResponse>("/" + commandLineArgs.sim_namespace + "/" + SpawnWallsServiceName, HandleWalls);
        ros_con.ImplementService<DeleteModelRequest, DeleteModelResponse>("/" + commandLineArgs.sim_namespace + "/" + DeleteServiceName, HandleDelete);
        ros_con.ImplementService<SetModelStateRequest, SetModelStateResponse>("/" + commandLineArgs.sim_namespace + "/" + MoveServiceName, HandleState);
        // ros_con.Subscribe<PoseStampedMsg>(GoalServiceName, HandleGoal);
        Debug.LogError(simNamespace + " **** All ROS services implemented");

        // initialize empty parent game object of obstacles (dynamic and static) & walls
        obstaclesParent = new("Obstacles");
        wallsParent = new("Walls");
        pedsParent = new("Peds");
    }

    private ROSConnection SetRosConnection()
    {
        // get command line IP and port
        string ip = commandLineArgs.tcp_ip != null ? commandLineArgs.tcp_ip : "127.0.0.1";
        int port = commandLineArgs.tcp_port != null ? int.Parse(commandLineArgs.tcp_port) : 10000;

        Debug.LogError(simNamespace + " **** IP: " + ip);
        Debug.LogError(simNamespace + " **** Port: " + port);

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
        Debug.LogError(simNamespace + " **** HandleDelete");
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
        Debug.LogError(simNamespace + " **** HandleState" + request);
        Debug.Log(request);
        string entityName = request.model_name;

        // check if the model really exists
        if (!activeModels.ContainsKey(entityName))
            return new SetModelStateResponse(false, "Model with name " + entityName + " does not exist.");

        // Move the object
        PoseMsg pose = request.pose;
        GameObject objectToMove = activeModels[entityName];

        if (objectToMove.transform.parent != null && objectToMove.transform.parent.name == "Obstacles") 
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
        Debug.LogError(simNamespace + " **** HandleSpawn");
        GameObject entity;

        // decide between robots and peds and obstacles
        if (request.model_xml.Contains("<robot>") || request.model_xml.Contains("<robot "))
        {
            entity = SpawnRobot(request);
        }
        else if (request.model_xml.Contains("<actor>") || request.model_xml.Contains("<actor "))
        {
            entity = pedController.SpawnPed(request);
            entity.transform.SetParent(pedsParent.transform);
        }
        else
        {
            entity = Instantiate(Cube);
            entity.name = request.robot_namespace;

            // sort under obstacles parent
            entity.transform.SetParent(obstaclesParent.transform);

            Utils.SetCubePose(entity, request.initial_pose);

            Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
            rb.useGravity = true;
        }

        // add to active models to delete later
        activeModels.Add(entity.name, entity);

        return new SpawnModelResponse(true, "Received Spawn Request");
    }

    private RobotConfig LoadRobotModelYaml(string robotName)
    {
        // Construct the full path robot yaml path
        // Take command line arg if executable build is running
        string arenaSimSetupPath = commandLineArgs.arena_sim_setup_path;
        // Use relative path if running in Editor
        arenaSimSetupPath ??= Path.Combine(Application.dataPath, "../../arena-simulation-setup");
        string yamlPath = Path.Combine(arenaSimSetupPath, "robot", robotName, robotName + ".model.yaml");

        // Check if the file exists
        if (!File.Exists(yamlPath))
        {
            Debug.LogError("Robot Model YAML file for " + robotName + " not found at: " + yamlPath);
            return null;
        }

        // Read the YAML file
        string yamlContent = File.ReadAllText(yamlPath);

        // Initialize the deserializer
        var deserializer = new DeserializerBuilder().Build();

        // Deserialize the YAML content into a dynamic object
        RobotConfig config = deserializer.Deserialize<RobotConfig>(yamlContent);

        return config;
    }

    private static Dictionary<string, object> GetPluginDict(RobotConfig config, string pluginTypeName)
    {
        Dictionary<string, object> targetDict = null;

        // Find Laser Scan configuration in list of plugins
        foreach (Dictionary<string, object> dict in config.plugins)
        {
            // check if type is actually laser scan
            if (dict.TryGetValue("type", out object value))
            {
                if (value is string strValue && strValue.Equals(pluginTypeName))
                {
                    targetDict = dict;
                    break;
                }
            }
        }

        return targetDict;
    }

    private static GameObject GetLaserLinkJoint(GameObject robot, Dictionary<string, object> laserDict)
    {

        // check if laser configuration has fram/joint specified
        if (!laserDict.TryGetValue("frame", out object frameName))
        {
            Debug.LogError("Robot Model Config for Laser Scan has no frame specified!");
            return null;
        }

        // get laser scan frame joint game object
        string laserJointName = frameName as string;
        Transform laserScanFrameTf = Utils.FindChildGameObject(robot.transform, laserJointName);
        if (laserScanFrameTf == null)
        {
            Debug.LogError("Robot has no joint game object as specified in Model Config for laser scan!");
            return null;
        }

        return laserScanFrameTf.gameObject;
    }

    private void HandleLaserScan(GameObject robot, RobotConfig config, string robotNamespace)
    {
        if (config == null)
        {
            Debug.LogError("Given robot config was null (probably incorrect config path). Robot will be spawned without scan");
            return;
        }

        // get configuration of laser scan from robot configuration
        Dictionary<string, object> laserDict = GetPluginDict(config, "Laser");
        if (laserDict == null)
        {
            Debug.LogError("Robot Model Configuration has no Laser plugin. Robot will be spawned without scan");
            return;
        }

        // find frame join game object for laser scan
        GameObject laserLinkJoint = GetLaserLinkJoint(robot, laserDict);
        if (laserLinkJoint == null)
        {
            Debug.LogError("No laser link joint was found. Robot will be spawned without scan.");
            return;
        }

        // attach LaserScanSensor
        LaserScanSensor laserScan = laserLinkJoint.AddComponent(typeof(LaserScanSensor)) as LaserScanSensor;
        laserScan.topicNamespace = simNamespace + "/" + robotNamespace;
        laserScan.frameId = robotNamespace + "/" + laserLinkJoint.name;

        // TODO: this is missing the necessary configuration of all parameters according to the laser scan config
        laserScan.ConfigureScan(laserDict);
    }

    private GameObject SpawnRobot(SpawnModelRequest request)
    {
        // process spawn request for robot
        GameObject entity = Utils.CreateGameObjectFromUrdfFile(
            request.model_xml,
            request.robot_namespace,
            disableJoints: true,
            disableScripts: true,
            parent: null
        );

        // get base link which is the second child after Plugins
        Transform baseLinkTf = entity.transform.GetChild(1);

        // Set up TF by adding TF publisher to the base_footprint game object
        baseLinkTf.gameObject.AddComponent(typeof(ROSTransformTreePublisher));

        // Set up Drive
        Drive drive = entity.AddComponent(typeof(Drive)) as Drive;
        drive.topicNamespace = simNamespace + "/" + request.robot_namespace;

        // Set up Odom publishing (this relies on the Drive -> must be added after Drive)
        OdomPublisher odom = baseLinkTf.gameObject.AddComponent(typeof(OdomPublisher)) as OdomPublisher;
        odom.topicNamespace = simNamespace + "/" + request.robot_namespace;
        odom.robotName = request.robot_namespace;

        // transport to starting pose
        Utils.SetPose(entity, request.initial_pose);

        // add gravity to robot
        Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = true;

        // try to attach laser scan sensor
        RobotConfig config = LoadRobotModelYaml(request.model_name);
        HandleLaserScan(entity, config, request.robot_namespace);

        return entity;
    }

    private void HandleGoal(PoseStampedMsg msg)
    {
        Debug.Log(msg.ToString());
    }

    private SpawnWallsResponse HandleWalls(SpawnWallsRequest request)
    {
        Debug.LogError(simNamespace + " **** HandleWalls");
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

            entity.transform.position = corner_start;
            entity.transform.localScale = corner_end - corner_start;
            AdjustPivot(entity.transform);

            // organize game object in walls parent game object
            entity.transform.SetParent(wallsParent.transform);
        }


        return new SpawnWallsResponse(true, "Walls successfully created");
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