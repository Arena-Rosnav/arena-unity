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
    string[] SOCIALSTATES = {"Texting", "TalkingOnPhone", "Idle", "Interested"};
    string[] socialStates;

    // Start is called before the first frame update
    void Start()
    {
        peds = new Dictionary<string, GameObject>();
        socialStates = new string[] {"Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking"};

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
            position.y = 0f;
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
            // string social_state = agentState.social_state;
            string socialStateIn = agentState.social_state; // get pedsim social state
            string animationOverwrite = socialStateIn; // var that shows which fake animation is played
            int id = int.Parse(agentState.id);
            System.Random rnd = new System.Random();
            int proc = rnd.Next(0,2);
            if (socialStateIn.Equals("Talking") || socialStateIn.Equals("Listening")){
                // Debug.Log("Animation State for ped: "+id+" is Blend Tree? "+animator.GetCurrentAnimatorStateInfo(0).IsName("Blend Tree")); DEBUG
                if(animator.GetCurrentAnimatorStateInfo(0).IsName("Blend Tree") || animator.GetCurrentAnimatorStateInfo(0).IsName("Blend Tree")){
                    if (proc == 0)    // 50/50 chance that incoming "Talking" social state is overwritten by a random non-moving animation
                        animationOverwrite = SOCIALSTATES[rnd.Next(0,SOCIALSTATES.Length)];
                    TriggerAnimation(animator, animationOverwrite);
                }
            } else{
                TriggerAnimation(animator, socialStateIn);
            }
            
            //TriggerAnimation(animator, socialStates[id]);            
        }
    }

    void TriggerAnimation(Animator animator, string social_state){
        switch(social_state){
            case "Idle":
                animator.SetInteger("socialState", 0);
                break;
            case "Walking":
                animator.SetInteger("socialState", 1);
                break;
            case "Talking":
                animator.SetInteger("socialState", 2);
                animator.SetTrigger("startTalking");
                break;
            case "Texting":
                animator.SetInteger("socialState", 3);
                break; 
            case "Interested":
                animator.SetInteger("socialState", 4);
                break;
            case "TalkingOnPhone":
                animator.SetInteger("socialState", 5);
                break;
            default:
                //animator.SetInteger("socialState", -1);
                break;
        }
    }
}
