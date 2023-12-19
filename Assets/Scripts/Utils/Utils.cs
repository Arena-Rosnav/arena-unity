using UnityEngine;
using Unity.Robotics.UrdfImporter;
using System.IO;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using RosMessageTypes.Geometry;


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

    public static Vector3 RosToUnity(PointMsg msg) {
        return new Vector3(
            (float)(-msg.y),
            (float)(msg.z),
            (float)(msg.x)
        );
    }

    public static Quaternion RosToUnity(QuaternionMsg msg) {
        return new Quaternion(
            (float)(-msg.y),
            (float)(msg.z),
            (float)(msg.x),
            (float)(msg.w)
        );
    }

    public static PointMsg UnityToRos(Vector3 vec) {
        return vec.To<FLU>();
    }

    public static QuaternionMsg UnityToRos(Quaternion qua) {
        return qua.To<FLU>();
    }

    public static void SetPose(GameObject obj, PoseMsg pose) 
    {
        Debug.Log("Setting pose:" + pose.ToString());
        obj.transform.SetPositionAndRotation(pose.position.From<FLU>(), pose.orientation.From<FLU>());
    }
}
