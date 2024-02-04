using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderSensor : MonoBehaviour
{
    public CapsuleCollider colliderComponent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
