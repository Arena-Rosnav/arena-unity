using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using RosMessageTypes.Std;

/// <summary>
/// Example demonstration of implementing a UnityService that receives a Request message from another ROS node and sends a Response back
/// </summary>
public class ServiceController : MonoBehaviour
{
    [SerializeField]
    string SpawnServiceName = "unity/spawn_model";
    [SerializeField]
    string DeleteServiceName = "unity/delete_model";
    [SerializeField]
    string MoveServiceName = "unity/set_model_state";
    [SerializeField]
    string GoalServiceName = "unity/set_goal";

    void Start()
    {
        // register the services with ROS
        ROSConnection.GetOrCreateInstance().ImplementService<EmptyRequest, EmptyResponse>(SpawnServiceName, HandleSpawn);
        ROSConnection.GetOrCreateInstance().ImplementService<EmptyRequest, EmptyResponse>(DeleteServiceName, Example);
        ROSConnection.GetOrCreateInstance().ImplementService<EmptyRequest, EmptyResponse>(MoveServiceName, Example);
        ROSConnection.GetOrCreateInstance().ImplementService<EmptyRequest, EmptyResponse>(GoalServiceName, Example);
    }

    /// HANDLER SECTION
    /// Put here all the functions on how top handle the service requests

    /// <summary>
    ///  Callback to respond to the request
    /// </summary>
    /// <param name="request">service request containing the object name</param>
    /// <returns>service response containing the object pose (or 0 if object not found)</returns>
    private EmptyResponse Example(EmptyRequest request)
    {
        // process the service request
        Debug.Log("Received request for object: " + request);

        // prepare a response
        EmptyResponse objectPoseResponse = new EmptyResponse();

        return objectPoseResponse;
    }

    private EmptyResponse HandleSpawn(EmptyRequest request)
    {
        // process the service request
        Debug.Log("SPAWN REQUEST of:" + request.ToString());

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(Random.Range(-10.0f, 10.0f), 5, Random.Range(-10.0f, 10.0f));

        return new EmptyResponse();
    }
}