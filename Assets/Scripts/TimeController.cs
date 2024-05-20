using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    CommandLineParser commandLineArgs;

    // Start is called before the first frame update
    void Start()
    {
        commandLineArgs = new CommandLineParser();
        commandLineArgs.Initialize();
        if (commandLineArgs.time_scale != null)
            Time.timeScale = float.Parse(commandLineArgs.time_scale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
