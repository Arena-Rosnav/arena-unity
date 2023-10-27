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
        server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/init", (request) =>
        {
            // Handle Request
            request.CreateResponse().Body("Connected").SendAsync();
        });
    }
}
