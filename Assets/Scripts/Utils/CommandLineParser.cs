using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CommandLineParser : MonoBehaviour
{
    public string arena_sim_setup_path;
    public string sim_namespace;
    public string tcp_ip;
    public string tcp_port;
    public string time_scale;
    public string headless;


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
    public void Initialize()
    {
        arena_sim_setup_path = GetValue("arena_sim_setup_path");
        sim_namespace = GetValue("namespace");
        tcp_ip = GetValue("tcp_ip");
        tcp_port = GetValue("tcp_port");
        time_scale = GetValue("time_scale");
        headless = GetValue("arena_headless");
    }
}
