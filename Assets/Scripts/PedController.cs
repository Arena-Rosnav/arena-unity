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

    CommandLineParser commandLineArgs;

    // Start is called before the first frame update
    void Start()
    {
        // Init command line args
        commandLineArgs = gameObject.AddComponent<CommandLineParser>();
        commandLineArgs.Initialize();

        pedFeedbackTopic = commandLineArgs.sim_namespace != null ? "/" + commandLineArgs.sim_namespace + pedFeedbackTopic : pedFeedbackTopic;

        peds = new Dictionary<string, GameObject>();

        ROSConnection.GetOrCreateInstance().Subscribe<AgentStatesMsg>(pedFeedbackTopic, AgentCallback);
    }

    public GameObject SpawnPed(SpawnModelRequest request)
    {
        // spawn every ped with a random character model
        System.Random r = new();
        int pedType = r.Next(PedTypes.Length);
        GameObject entity = Instantiate(PedTypes[pedType]);
        entity.name = request.robot_namespace;

        // add rigidbody to this ped to use unity physics (e.g. physics)
        Rigidbody rb = entity.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // set initial pose
        Utils.SetPose(entity, request.initial_pose);

        // register in peds dict
        peds.Add(entity.name, entity);

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
                // Debug.LogWarning("Got Agent State for Agent with ID " + agentState.id + " which doesn't exist!");
                /*
                 * Removed this log line since arena rosnav is now constantly sending agent states for non-spawned agents.
                 * The constant logging of this was so much that it caused Arena Unity to completely freeze.
                 */
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
            string socialState = agentState.social_state;
            TriggerAnimation(animator, socialState);            
        }
    }

    void TriggerAnimation(Animator animator, string socialState){
        switch(socialState){
            case "Idle":
                animator.SetInteger("socialState", 0);
                break;
            case "Walking":
                animator.SetInteger("socialState", 1);
                break;
            case "Running":
                animator.SetInteger("socialState", 1);  // Walking Animation already lines up with running velocity
                break;
            case "Talking":
                animator.SetInteger("socialState", 2);
                if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Talking")) // only trigger starting animation if it was not already in the talking animation
                    animator.SetTrigger("startTalking");
                break;
            case "Listening":   // currently not a dedicated animation
                animator.SetInteger("socialState", 2);
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
            default:    // default to walking
                animator.SetInteger("socialState", -1);
                break;
        }
    }
}
