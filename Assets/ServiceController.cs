using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using RosMessageTypes.Ford;

/// <summary>
/// Example demonstration of implementing a UnityService that receives a Request message from another ROS node and sends a Response back
/// </summary>
public class ServiceController : MonoBehaviour
{
    [SerializeField]
    string m_ServiceName = "unity/spawn_model";

    void Start()
    {
        // register the service with ROS
        ROSConnection.GetOrCreateInstance().ImplementService<GetSafeActionsRequest, GetSafeActionsResponse>(m_ServiceName, GetObjectPose);
    }

    /// <summary>
    ///  Callback to respond to the request
    /// </summary>
    /// <param name="request">service request containing the object name</param>
    /// <returns>service response containing the object pose (or 0 if object not found)</returns>
    private GetSafeActionsResponse GetObjectPose(GetSafeActionsRequest request)
    {
        // process the service request
        Debug.Log("Received request for object: " + request);

        // prepare a response
        GetSafeActionsResponse objectPoseResponse = new GetSafeActionsResponse();

        return objectPoseResponse;
    }
}