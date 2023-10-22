using RestServer;
using System;
using System.IO;
using UnityEngine;

public class RoutingManager : MonoBehaviour
{
    // Server URL: http://localhost:8080

    // Reference to the RestServer instance 
    public RestServer.RestServer server;

    // Start is called before the first frame update
    void Start()
    {
        // Register the endpoint
        server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/ping", (request) =>
        {
            // Handle Request
            Debug.Log("Pong!");

            var fileName = "Debug.txt";
            if (File.Exists(fileName))
            {
                Debug.Log(fileName + " already exists.");
                return;
            }
            var sr = File.CreateText(fileName);
            sr.WriteLine("Pong!");
            sr.Close();
        });
    }
}
