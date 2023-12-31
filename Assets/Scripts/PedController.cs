using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

// ROS msgs
using RosMessageTypes.Geometry;
using RosMessageTypes.Pedsim;
using RosMessageTypes.Gazebo;

public class PedController : MonoBehaviour
{

    Dictionary<string, GameObject> peds;
    string pedFeedbackTopic = "/pedsim_simulator/simulated_agents";

    // Start is called before the first frame update
    void Start()
    {
        peds = new Dictionary<string, GameObject>();

        ROSConnection.GetOrCreateInstance().Subscribe<AgentStatesMsg>(pedFeedbackTopic, AgentCallback);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject SpawnPed(SpawnModelRequest request)
    {
        GameObject entity = GameObject.CreatePrimitive(PrimitiveType.Cube);
        entity.name = request.model_name;

        // set initial pose
        Utils.SetPose(entity, request.initial_pose);

        // register in peds dict
        peds.Add(request.model_name, entity);

        Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = true;

        return entity;
    }

    void AgentCallback(AgentStatesMsg agentStates)
    {

    }
}
