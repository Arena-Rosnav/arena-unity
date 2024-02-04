using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataObjects;
using YamlDotNet.Serialization;
using System.IO;

// Message Types
using RosMessageTypes.Gazebo;

public class RobotController : MonoBehaviour
{
    private CommandLineParser commandLineArgs;
    private string simNamespace;

    void Start()
    {
        commandLineArgs = gameObject.AddComponent<CommandLineParser>();
        commandLineArgs.Initialize();

        simNamespace = commandLineArgs.sim_namespace != null ? "/" + commandLineArgs.sim_namespace : "";
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
        HandleLaserScan(entity, config, request.robot_namespace);

        return entity;
    }
}
