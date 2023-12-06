using UnityEngine;
using Unity.Robotics.UrdfImporter;
using System.IO;


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

}
