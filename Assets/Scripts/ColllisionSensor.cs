using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Unity;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.Core;

public class ColllisionSensor : MonoBehaviour
{
    const string collsionTopicName = "collision";
    const double publishRateHz = 20f;
    public CapsuleCollider colliderComponent;
    public string topicNamespace;
    private bool colliding;
    private ROSConnection connection;
    double lastPublishTimeSeconds;
    private string PublishTopic => topicNamespace + "/" + colliderComponent;
    double PublishPeriodSeconds => 1.0f / publishRateHz;
    private bool ShouldPublishMessage => Clock.time - PublishPeriodSeconds > lastPublishTimeSeconds;

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

    void OnCollisionEnter(Collision collision)
    {
        colliding = true;
    }

    void OnCollisionExit(Collision collision)
    {
        colliding = false;
    }

    private void PublishMessage()
    {
        CollisionMsg message = new(colliding);
        lastPublishTimeSeconds = Clock.time;
        connection.Publish(PublishTopic, message);
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
