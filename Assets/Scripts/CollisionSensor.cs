using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Unity;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;

public class CollisionSensor : MonoBehaviour
{
    private int collisionCount = 0;
    public string collsionTopicName = "collision";
    const double publishRateHz = 20f;
    public CapsuleCollider colliderComponent;
    public string topicNamespace;
    private ROSConnection connection;
    double lastPublishTimeSeconds;
    double continuedContactTime = 0;
    double checkCollisionThresholdSeconds = 10;
    private string PublishTopic => topicNamespace + "/" + collsionTopicName;
    double PublishPeriodSeconds => 1.0f / publishRateHz;
    private bool ShouldPublishMessage => Clock.time - PublishPeriodSeconds > lastPublishTimeSeconds;
    private bool InContact => collisionCount > 0;

    public bool detectPed = true;
    public bool detectObs = true;

    // Start is called before the first frame update
    void Start()
    {
        connection = FindObjectOfType<ROSConnection>();
        connection.RegisterPublisher<CollisionMsg>(PublishTopic);
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldPublishMessage)
            PublishMessage();

    }

    void OnTriggerEnter(Collider collider)
    {
        if ((detectPed && collider.gameObject.layer == LayerMask.NameToLayer("Ped")) ||
            (detectObs && collider.gameObject.layer == LayerMask.NameToLayer("Obs")))
            collisionCount++;
    }

    void OnTriggerExit(Collider collider)
    {
        if ((detectPed && collider.gameObject.layer == LayerMask.NameToLayer("Ped")) ||
            (detectObs && collider.gameObject.layer == LayerMask.NameToLayer("Obs")))
            collisionCount--;
    }

    private void PublishMessage()
    {
        CollisionMsg message = new(InContact);
        lastPublishTimeSeconds = Clock.time;
        connection.Publish(PublishTopic, message);

        // Increase the continued contact time
        if (InContact)
            continuedContactTime += PublishPeriodSeconds;
        else
            continuedContactTime = 0;
        
        // If there is an unusually long contact time check correctness of counter
        if (continuedContactTime > checkCollisionThresholdSeconds)
        {
            if (!ActuallyInContact())
                // Reset counter if there is no collision
                collisionCount = 0;
            continuedContactTime = 0;
        }
    }

    /// <summary>
    /// Checks whetehr there is currently a valid collision. This can be periodically
    /// called, to check the correctness of the OnTrigger method counting. You should not call 
    /// this every update, since it is computationally more complex.
    /// </summary>
    /// <returns><Number of current collisions./returns>
    private bool ActuallyInContact()
    {
        // Allocate a Collider array of size 1
        Collider[] collisionColliders = new Collider[1];
        // Create appropriate layer mask
        int layerMask = 0;
        if (detectObs && detectPed)
            layerMask = LayerMask.GetMask("Ped", "Obs");
        else if (detectObs)
            layerMask = LayerMask.GetMask("Obs");
        else if (detectPed)
            layerMask = LayerMask.GetMask("Ped");

        int collision = Physics.OverlapSphereNonAlloc(
            transform.TransformPoint(colliderComponent.center),
            colliderComponent.radius,
            collisionColliders,
            layerMask:layerMask 
        );
    
        return collision == 1;
    }

    public bool ConfigureCollider(Dictionary<string, object> colliderConfig)
    {
        bool success = true;

        // height
        if (colliderConfig.TryGetValue("height", out object height) && float.TryParse((string)height, out float heightVal))
        {
            colliderComponent.height = heightVal;
        }
        else
        {
            Debug.LogError("Config for collider doesn't include height value or value not a float!");
            success = false;
        }
        // radius
        if (colliderConfig.TryGetValue("radius", out object radius) && float.TryParse((string)radius, out float radiusVal))
        {
            colliderComponent.radius = radiusVal;
        }
        else
        {
            Debug.LogError("Config for collider doesn't include radius value or value not a float!");
            success = false;
        }
        // position
        if (colliderConfig.TryGetValue("position", out object position) && position is List<object> positionList)
        {
            colliderComponent.center = new Vector3(
                float.Parse((string)positionList[0]),
                float.Parse((string)positionList[1]),
                float.Parse((string)positionList[2]));
        }
        else
        {
            Debug.LogError("Config for collider doesn't include position value or value not a float!");
            success = false;
        }

        return success;
    }
}
