using UnityEngine;
using Unity.Robotics.UrdfImporter;
using System.IO;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.TF;

using RosMessageTypes.Geometry;
using System;
using Unity.VisualScripting;


public class Utils : MonoBehaviour
{
    public static GameObject CreateGameObjectFromUrdfFile(string urdfXml, string modelName, bool disableScripts = true, bool disableJoints = true, GameObject parent = null)
    {
        TextReader tr = new StringReader(urdfXml);
        GameObject newGameObject = UrdfRobotExtensions.CreateRuntime(tr,modelName+".urdf", new ImportSettings());

        newGameObject.name = modelName;
        newGameObject.transform.parent = null;

        if (parent)
            newGameObject.transform.parent = parent.transform;


        // Edit new created world object

        // Disable all script
        if (disableScripts)
        {
            MonoBehaviour[] mono = newGameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour m in mono)
            {
                m.enabled = false;
                Destroy(m);
            }
        }

        // Disable all Articulation Bodies
        if (disableJoints)
        {
            ArticulationBody[] articulationBodys = newGameObject.GetComponentsInChildren<ArticulationBody>();

            foreach (ArticulationBody body in articulationBodys)
            {
                body.enabled = false;
                Destroy(body);
            }
        }

        return newGameObject;
    }

    /// <summary>
    /// Converts ROS vector to Unity vector
    /// </summary>
    public static Vector3 RosToUnity(PointMsg msg) 
    {
        return msg.From<FLU>();
    }

    /// <summary>
    /// Converts ROS vector to Unity vector
    /// </summary>
    public static Quaternion RosToUnity(QuaternionMsg msg) 
    {
        return msg.From<FLU>();
    }

    /// <summary>
    /// Converts Unity vector to ROS vector
    /// </summary>
    public static PointMsg UnityToRos(Vector3 vec) 
    {
        return vec.To<FLU>();
    }

    /// <summary>
    /// Converts Unity vector to ROS vector
    /// </summary>
    public static QuaternionMsg UnityToRos(Quaternion qua) 
    {
        return qua.To<FLU>();
    }

    /// <summary>
    /// Sets the pose in the transform component for a game object.
    /// It converts the ROS vector convention (FLU) to Unity convention.
    /// </summary>
    /// <param name="obj">Game object to be moved</param>
    /// <param name="pose">Pose message for target pose</param>
    public static void SetPose(GameObject obj, PoseMsg pose) 
    {
        obj.transform.SetPositionAndRotation(pose.position.From<FLU>(), pose.orientation.From<FLU>());
    }

    /// <summary>
    /// Sets the pose for Cubes because there transform position is measured
    /// in the middle at height 0.5 and not at the Cube's base.
    /// </summary>
    /// <param name="obj">Cube</param>
    /// <param name="pose">Target pose</param>
    public static void SetCubePose(GameObject obj, PoseMsg pose)
    {
        Vector3 position = pose.position.From<FLU>();
        position.y = 0.5F;
        obj.transform.SetPositionAndRotation(position, pose.orientation.From<FLU>());
    }

    /// <summary>
    /// Searches recursively through the children of given transform for 
    /// a GameObject with the given name.
    /// </summary>
    /// <param name="parent">Transform of root GameObject for search</param>
    /// <param name="childName">Name of child GameObject to be found</param>
    /// <returns>Transform of child GameObject or null if not found</returns>
    public static Transform FindChildGameObject(Transform parent, string childName) 
    {
        foreach (Transform child in parent) 
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildGameObject(child, childName);
            if (found != null) 
                return found;
        }

        // no GameObject found on all paths
        return null;
    }
}
