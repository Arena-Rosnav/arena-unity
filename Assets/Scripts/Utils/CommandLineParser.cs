using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class CommandLineParser
{
    public string arena_sim_setup_path;
    public string unityMap;

    private static CommandLineParser instance;

    public static CommandLineParser Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CommandLineParser();
                instance.Initialize();
            }
            return instance;
        }
    }

    private CommandLineParser()
    {
        // Private constructor to prevent instantiation
    }

    private void Initialize()
    {
        string[] args = Environment.GetCommandLineArgs();
        Debug.Log($"Args: {string.Join(", ", args)}");

        arena_sim_setup_path = GetValue("arena_sim_setup_path");
        string unityMapFile = GetValue("map");
        if (unityMapFile != null && unityMapFile != "")
        {
            var i = unityMapFile.IndexOf("_unity");
            if (i != -1)
            {
                unityMap = unityMapFile.Substring(0, i);
            }
        }
    }

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
}
