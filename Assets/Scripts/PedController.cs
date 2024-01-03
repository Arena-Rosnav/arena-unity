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

    // Array for the different ped types; specific ped types are added in the PedController Object in the Unity Editor
    public GameObject[] PedTypes;

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
        // spawn every ped with a random character model
        System.Random r = new();
        int pedType = r.Next(PedTypes.Length);
        GameObject entity = Instantiate(PedTypes[pedType]);
        entity.name = request.model_name;

        // add rigidbody to this ped to use unity physics (e.g. physics)
        Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = true;

        // set initial pose
        Utils.SetPose(entity, request.initial_pose);

        // register in peds dict
        peds.Add(request.model_name, entity);

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
            Animator animator = agent.GetComponent<Animator>();
            animator.SetFloat("velocity", rb.velocity.magnitude);

            // set social state in the animator component
            string social_state = agentState.social_state;
            if(social_state=="Running" && animator.GetBool("isRunning")){ // if published social state is the same as the current animation state, skip setting of parameters (performance reasons)
                continue;
            }else if(social_state=="Walking" && animator.GetBool("isWalking")){
                continue;
            }else{
                SetSocialState(animator, social_state);
            }
            
        }
    }

    void SetSocialState(Animator animator, string social_state){
        switch(social_state){
                case "Walking":
                    animator.SetBool("isIdle", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                    break;
                case "Running":
                    animator.SetBool("isIdle", false);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", true);
                    break;
                default:
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isIdle", true);
                    break;
            }
        return;
    }
}
