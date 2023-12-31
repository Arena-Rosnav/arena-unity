using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

// ROS msgs
using RosMessageTypes.Geometry;
using RosMessageTypes.Pedsim;
using RosMessageTypes.Gazebo;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class PedController : MonoBehaviour
{

    Dictionary<string, GameObject> peds;
    string pedFeedbackTopic = "/pedsim_simulator/simulated_agents";
    public GameObject PedMale;

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

    public GameObject SpawnPed(SpawnModelRequest request)
    {
        GameObject entity = Instantiate(PedMale);
        entity.name = request.model_name;

        // set initial pose
        Utils.SetPose(entity, request.initial_pose);

        // register in peds dict
        peds.Add(request.model_name, entity);

        Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = true;

        return entity;
    }

    public void DeletePed(string pedID)
    {
        if (!peds.ContainsKey(pedID)) 
            Debug.LogWarning("Tried to delete ped with ID " + pedID + " but ped with such ID doesn't exist!");
        else
            peds.Remove(pedID);
    }

    void AgentCallback(AgentStatesMsg agentStates)
    {
        foreach (AgentStateMsg agentState in agentStates.agent_states)
        {
            if (!peds.ContainsKey(agentState.id))
            {
                Debug.LogWarning("Got Agent State for Agent with ID " + agentState.id + " which doesn't exist!");
                continue;
            }

            GameObject agent = peds[agentState.id];
            Rigidbody rb = agent.GetComponent<Rigidbody>();


            // update agent properties
            Vector3 position = agentState.pose.position.From<FLU>();
            // set y position (only required for cubes)
            position.y = 0.5f;
            agent.transform.SetPositionAndRotation(
                position,
                agentState.pose.orientation.From<FLU>()
            );
            // only the linear velocitypart since our pedsim agents don't have angular velocity
            rb.velocity = agentState.twist.linear.From<FLU>();
            // set velocity in the animator component for animations
            agent.GetComponent<Animator>().SetFloat("velocity", rb.velocity.magnitude);
        }
    }
}
