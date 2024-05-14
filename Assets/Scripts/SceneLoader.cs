using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public CommandLineParser commandLineParser;
    void Start()
    {
        commandLineParser = CommandLineParser.Instance;

        if (commandLineParser.unityMap == null)
        {
            Debug.Log("No unity map specified loading default empty scene");
            SceneManager.LoadScene("empty");
        }
        else
        {
            Debug.Log($"Loading scene {commandLineParser.unityMap}");
            int index = SceneUtility.GetBuildIndexByScenePath(commandLineParser.unityMap);
            if (index != -1)
            {
                SceneManager.LoadScene(index);
            }
            else
            {
                Debug.Log("Scene not found loading default empty scene");
                SceneManager.LoadScene("empty");
            }
        }
    }
}
