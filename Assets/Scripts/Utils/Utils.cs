using UnityEngine;
using Unity.Robotics.UrdfImporter;


public class Utils : MonoBehaviour
{
    public static GameObject CreateGameObjectFromUrdfFile(string urdfFilePath, string modelName, bool disableScripts = true, bool disableJoints = true, GameObject parent = null)
    {
        GameObject newGameObject = UrdfRobotExtensions.CreateRuntime(urdfFilePath, new ImportSettings());

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
