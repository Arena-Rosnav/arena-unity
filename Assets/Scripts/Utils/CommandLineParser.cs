using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CommandLineParser : MonoBehaviour
{
    public string arena_sim_setup_path;

    private string GetValue(string argName)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-" + argName && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    // Assigns all properties the matching command line argument value
    void Start()
    {
        arena_sim_setup_path = GetValue("arena_sim_setup_path");
        Debug.LogError(arena_sim_setup_path);
        Debug.LogError(Environment.GetCommandLineArgs());
    }
}
