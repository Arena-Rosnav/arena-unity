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
    public GameObject Cube;

    // Array for the different ped types; specific ped types are added in the PedController Object in the Unity Editor
    public GameObject[] PedTypes;

    /* RANDOMIZER variables
        string[] SOCIALSTATES = {"Texting", "TalkingOnPhone", "Idle", "Interested"};
        string[] socialStates;
    */

    // Start is called before the first frame update
    void Start()
    {
        peds = new Dictionary<string, GameObject>();
        //socialStates = new string[] {"Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking", "Walking"}; RANDOMIZER

        ROSConnection.GetOrCreateInstance().Subscribe<AgentStatesMsg>(pedFeedbackTopic, AgentCallback);
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

            /* RANDOMIZER
                string socialStateIn = agentState.social_state; // get pedsim social state
                string animationOverwrite = socialStateIn; // var that shows which fake animation is played
                int id = int.Parse(agentState.id);
                System.Random rnd = new System.Random();
                int proc = rnd.Next(0,2);
                if (socialStateIn.Equals("Talking") || socialStateIn.Equals("Listening")){
                    // Debug.Log("Animation State for ped: "+id+" is Blend Tree? "+animator.GetCurrentAnimatorStateInfo(0).IsName("Blend Tree")); DEBUG
                    if(animator.GetCurrentAnimatorStateInfo(0).IsName("Blend Tree") || animator.GetCurrentAnimatorStateInfo(0).IsName("WalkingMale") || animator.GetCurrentAnimatorStateInfo(0).IsName("WalkingFemale")){
                        if (proc == 0)    // 50/50 chance that incoming "Talking" social state is overwritten by a random non-moving animation
                            animationOverwrite = SOCIALSTATES[rnd.Next(0,SOCIALSTATES.Length)];
                        TriggerAnimation(animator, animationOverwrite);
                    }
                } else{
                    TriggerAnimation(animator, socialStateIn);
                }
            */ 

            // set social state in the animator component
            string newSocialState = agentState.social_state;
            TriggerAnimation(animator, newSocialState);            
        }
    }

    void TriggerAnimation(Animator animator, string newSocialState){
        int currentSocialState = animator.GetInteger("socialState");
        System.Random rnd = new System.Random();
        int state;
        switch(newSocialState){
            case "Idle":
                if(animator.GetFloat("velocity") >= 0.1){       // force walking when velocity is greater or equal to 0.1
                    state = 10;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < -1 || 9 < currentSocialState ){    //if the social state is not already in the idle social state int range
                    state = 0 + rnd.Next(0,5);
                    animator.SetInteger("socialState", state);
                }
                break;
            case "Walking":
                if(animator.GetFloat("velocity") < 0.1){        // force idling when velocity is smaller than 0.1
                    state = 0;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 10 || 19 < currentSocialState ){    //if the current social state is different than the (incoming) walking social state int range
                    state = 10 + rnd.Next(0,5);
                    animator.SetInteger("socialState", state);
                }
                break;
            case "Running":
                if(animator.GetFloat("velocity") < 0.1){
                    state = 0;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 10 || 19 < currentSocialState ){    //if the social state is not already in the walking social state int range
                    state = 10 + rnd.Next(0,5);
                    animator.SetInteger("socialState", state);              
                }                                                                // Walking Animation already lines up with running velocity
                break;
            case "Talking":
                if(animator.GetFloat("velocity") >= 0.1){
                    state = 10;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 20 || 29 < currentSocialState ){    //if the social state is not already in the talking social state int range
                    state = 20 + rnd.Next(0,6);
                    animator.SetInteger("socialState", state);              
                }
                /*
                if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Talking")) // only trigger starting animation if it was not already in the talking animation
                    animator.SetTrigger("startTalking");
                */
                break;
            case "Listening":   // currently not a dedicated animation
                if(animator.GetFloat("velocity") >= 0.1){
                    state = 10;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 20 || 29 < currentSocialState ){    //if the social state is not already in the talking social state int range
                    state = 20 + rnd.Next(0,6);
                    animator.SetInteger("socialState", state);              
                }
                break; 
            case "Texting":
                if(animator.GetFloat("velocity") >= 0.1){
                    state = 10;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 30 || 39 < currentSocialState ){    //if the social state is not already in the texting social state int range
                    state = 30 + rnd.Next(0,1);
                    animator.SetInteger("socialState", state);              
                }
                break;
            case "Interested":
                if(animator.GetFloat("velocity") >= 0.1){
                    state = 10;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 40 || 49 < currentSocialState ){    //if the social state is not already in the Interested social state int range
                    state = 40 + rnd.Next(0,4);
                    animator.SetInteger("socialState", state);              
                }
                break;
            case "TalkingOnPhone":
                if(animator.GetFloat("velocity") >= 0.1){
                    state = 10;
                    animator.SetInteger("socialState", state);
                } else if( currentSocialState < 50 || 59 < currentSocialState ){    //if the social state is not already in the TalkingOnPhone social state int range
                    state = 50 + rnd.Next(0,2);
                    animator.SetInteger("socialState", state);              
                }
                break;
            default:    // default to walking
                animator.SetInteger("socialState", -1);
                break;
        }
    }
}
