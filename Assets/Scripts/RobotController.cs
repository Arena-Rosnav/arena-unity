using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataObjects;
using YamlDotNet.Serialization;
using System.IO;

// Message Types
using RosMessageTypes.Gazebo;
using RosMessageTypes.Unity;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    private CommandLineParser commandLineArgs;
    private string simNamespace;
    private readonly string CollisionSensorName = "CollisionSensor";
    private readonly string PedSafeDistSensorName = "PedSafeDistSensor";
    private readonly string ObsSafeDistSensorName = "ObsSafeDistSensor";
    [Tooltip("If true, the fallback RGBD sensor will be used if no sensor is found in the robot model yaml. If false no RGBD will be used in this case.")]
    private bool useFallbackRGBD = false;

    void Start()
    {
        commandLineArgs = gameObject.AddComponent<CommandLineParser>();
        commandLineArgs.Initialize();

        simNamespace = commandLineArgs.sim_namespace != null ? "/" + commandLineArgs.sim_namespace : "";
    }

    private string GetConfigFileContent(string relativeArenaSimSetupPath)
    {
        // Construct the full path robot yaml path
        // Take command line arg if executable build is running
        string arenaSimSetupPath = commandLineArgs.arena_sim_setup_path;
        // Use relative path if running in Editor
        arenaSimSetupPath ??= Path.Combine(Application.dataPath, "../../simulation-setup");
        string configPath = Path.Combine(arenaSimSetupPath, relativeArenaSimSetupPath);

        // Check if the file exists
        if (!File.Exists(configPath))
        {
            Debug.LogError("Config file could not be found at: " + configPath);
            return null;
        }

        // Read the config file
        return File.ReadAllText(configPath);
    }

    private RobotConfig LoadRobotModelYaml(string robotName)
    {
        // Get yaml file content
        string relativeYamlPath = Path.Combine("entities", "robots", robotName, robotName + ".model.yaml");
        string yamlContent = GetConfigFileContent(relativeYamlPath);
        if (yamlContent == null)
        {
            Debug.LogError("Robot model yaml file could not be found at: " + relativeYamlPath);
            return null;
        }

        // Initialize the deserializer
        var deserializer = new DeserializerBuilder().Build();

        // Deserialize the YAML content into a dynamic object
        RobotConfig config = deserializer.Deserialize<RobotConfig>(yamlContent);

        return config;
    }

    private RobotUnityConfig LoadRobotUnityParamsYaml(string robotName)
    {
        // Get yaml file content
        string relativeYamlPath = Path.Combine("entities", "robots", robotName, "unity", "unity_params.yaml");
        string yamlContent = GetConfigFileContent(relativeYamlPath);
        if (yamlContent == null)
        {
            Debug.LogError("Unity specific params yaml file could not be found at: " + relativeYamlPath);
            return null;
        }

        // Initialize the deserializer
        var deserializer = new DeserializerBuilder().Build();

        // Deserialize the YAML content into a dynamic object
        RobotUnityConfig config = deserializer.Deserialize<RobotUnityConfig>(yamlContent);
        
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

    private static GameObject GetLinkJoint(GameObject robot, Dictionary<string, object> dict)
    {

        // check if laser configuration has fram/joint specified
        dict.TryGetValue("type", out object pluginType);
        if (!dict.TryGetValue("frame", out object frameName))
        {
            Debug.LogError($"Robot Model Config for {pluginType} has no frame specified!");
            return null;
        }

        // get laser scan frame joint game object
        string jointName = frameName as string;
        Transform frameTf = Utils.FindChildGameObject(robot.transform, jointName);
        if (frameTf == null)
        {
            Debug.LogError($"Robot has no joint game object as specified in Model Config for {pluginType}!");
            return null;
        }

        return frameTf.gameObject;
    }

    private void HandleLaserScan(GameObject robot, RobotConfig config, string robotNamespace)
    {
        // get configuration of laser scan from robot configuration
        Dictionary<string, object> laserDict = GetPluginDict(config, "Laser");
        if (laserDict == null)
        {
            Debug.LogError("Robot Model Configuration has no Laser plugin. Robot will be spawned without scan");
            return;
        }

        // find frame join game object for laser scan
        GameObject laserLinkJoint = GetLinkJoint(robot, laserDict);
        if (laserLinkJoint == null)
        {
            Debug.LogError("No laser link joint was found. Robot will be spawned without scan.");
            return;
        }

        // attach LaserScanSensor
        LaserScanSensor laserScan = laserLinkJoint.AddComponent<LaserScanSensor>();
        laserScan.topicNamespace = simNamespace + "/" + robotNamespace;
        laserScan.frameId = robotNamespace + "/" + laserLinkJoint.name;

        laserScan.ConfigureScan(laserDict);
    }

    private void HandleRGBDSensor(GameObject robot, RobotUnityConfig config, string robotNamespace)
    {
        GameObject cameraLinkJoint;
        bool fallback = false;
        if (!config.components.TryGetValue("RGBDCamera", out Dictionary<string, object> dict))
        {
            Debug.LogWarning("Unity-specific config does not specify RGBDCamera component.");
            if (!useFallbackRGBD)
                return;
            fallback = true;
            
            // use laser frame as fallback
            LaserScanSensor laserScan = robot.transform.GetComponentInChildren<LaserScanSensor>();
            if (laserScan == null)
            {
                Debug.LogError("Robot has no laser scan. Robot will be spawned without RGBDCamera.");
                return;
            }
            cameraLinkJoint = laserScan.gameObject;
        }
        else
        {
            cameraLinkJoint = GetLinkJoint(robot, dict);
        }

        if (cameraLinkJoint == null)
        {
            Debug.LogError("No link joint was found. Robot will be spawned without RGBDCamera.");
            return;
        }

        // attach LaserScanSensor
        RGBDSensor camera = cameraLinkJoint.AddComponent<RGBDSensor>();
        camera.topicNamespace = simNamespace + "/" + robotNamespace;
        camera.frameId = robotNamespace + "/" + cameraLinkJoint.name;

        if (!fallback)
            camera.ConfigureRGBDSensor(dict, robot.name, cameraLinkJoint.name);
        else
            camera.ConfigureDefaultRGBDSensor(robot.name, cameraLinkJoint.name);
    }

    private void HandleCollider(GameObject robot, RobotUnityConfig config, string robotNamespace)
    {
        if (!config.components.TryGetValue("collider", out Dictionary<string, object> colliderDict))
        {
            Debug.LogWarning("Unity-specific config does not specify collider component.");
            return;
        }

        GameObject collisionSensorObject = new(CollisionSensorName);

        // attach collider 
        CapsuleCollider collider = collisionSensorObject.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        
        // attach collider sensor
        CollisionSensor collisionSensor = collisionSensorObject.AddComponent<CollisionSensor>();
        collisionSensor.colliderComponent = collider;
        collisionSensor.topicNamespace = simNamespace + "/" + robotNamespace;
        collisionSensor.ConfigureCollider(colliderDict);

        // center child collision sensor
        collisionSensorObject.transform.SetParent(robot.transform);
        collisionSensorObject.transform.SetPositionAndRotation(
            robot.transform.position, 
            robot.transform.rotation
        );
    }

    public GameObject SpawnRobot(SpawnModelRequest request)
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
        if (config == null)
        {
            Debug.LogError("Given robot config was null (probably incorrect config path). Robot will be spawned without Sensors!");
            return entity;
        }
        else 
        {
            HandleLaserScan(entity, config, request.robot_namespace);
        }

        // try to attach collider sensor
        RobotUnityConfig unityConfig = LoadRobotUnityParamsYaml(request.model_name);
        if (unityConfig == null)
        {
            Debug.LogError("Given robot unity params config was null (probably incorrect config path). Robot will be spawned without unity-specific sensor!");
        }
        else
        {
            HandleRGBDSensor(entity, unityConfig, request.robot_namespace);
            HandleCollider(entity, unityConfig, request.robot_namespace);
        }
        return entity;
    }

    public bool AttachSafeDistSensor(GameObject robot, AttachSafeDistSensorRequest request)
    {
        string sensorName = "";
        if (request.ped_safe_dist)
            sensorName = PedSafeDistSensorName;
        else if (request.obs_safe_dist)
            sensorName = ObsSafeDistSensorName;
        else
            return false;  // invalid request;

        // Get main collision sensor for configurations
        GameObject collisionSensorObject = robot.transform.Find(CollisionSensorName).gameObject;
        CollisionSensor collisionSensor = collisionSensorObject.GetComponent<CollisionSensor>();
        if (collisionSensor == null)
            return false;

        // Replace old safe dist sensor if existent
        Transform old = robot.transform.Find(sensorName);
        if (old != null)
            Destroy(old.gameObject);

        // Configure Collider
        GameObject safeDistSensorObject = new(sensorName);
        CapsuleCollider collider = safeDistSensorObject.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        // Use main collider configurations
        collider.height = collisionSensor.colliderComponent.height;
        collider.center = collisionSensor.colliderComponent.center;
        collider.radius = collisionSensor.colliderComponent.radius + (float)request.safe_dist;

        // Configure new safe dist sensor
        CollisionSensor safeDistSensor = safeDistSensorObject.AddComponent<CollisionSensor>();
        safeDistSensor.colliderComponent = collider;
        safeDistSensor.topicNamespace = simNamespace + "/" + request.robot_name;
        safeDistSensor.collsionTopicName = request.safe_dist_topic;
        safeDistSensor.detectPed = request.ped_safe_dist;
        safeDistSensor.detectObs = request.obs_safe_dist;
        

        safeDistSensorObject.transform.SetParent(robot.transform);
        safeDistSensorObject.transform.SetPositionAndRotation(
            robot.transform.position, 
            robot.transform.rotation
        );
        return true;
    }
}
