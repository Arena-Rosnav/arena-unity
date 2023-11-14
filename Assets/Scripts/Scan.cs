using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class Scan : MonoBehaviour {
    public float minAngle;
    public float maxAngle;

    public int numBeams = 50;
    public float range;
    public int updateRate;

    public string frameId;

    public string topicNamespace;

    float increment;
    
    List<GameObject> laserPoints;
    float[] distances;

    ROSConnection ros;
    string topicName;
    
    uint seq; 

    int counter = 0;

    void Start() {

        /// TODO FRAME ID
        /// 'robot_ns'/base_scan

        topicName = topicNamespace + "/scan";

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<LaserScanMsg>(topicName);

        increment = (float) (maxAngle - minAngle) / ((float) numBeams);
        seq = 0;

        distances = new float[numBeams];
        laserPoints = new List<GameObject>();

        for(int i = 0; i < numBeams; i++ ) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Collider collider = sphere.GetComponent<Collider>();
            collider.enabled = false;
            sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            // sphere.GetComponent<Renderer>().material.color = new Color(255, 0, 0, 1);

            sphere.SetActive(false);
            
            laserPoints.Add(sphere);
        }
    }

    void Update() {
        for( int i = 0; i < numBeams; i++ ) {
            double currentAngle = minAngle + i * increment;

            double x = Math.Cos(currentAngle + Math.PI / 2);
            double z = Math.Sin(currentAngle + Math.PI / 2);

            Vector3 direction = transform.rotation * new Vector3((float) x, 0, (float) z);

            RaycastHit hit;

            Ray ray = new Ray(transform.position, direction);

            if ( Physics.Raycast(ray, out hit, range) ) {
                laserPoints[i].transform.position = hit.point;
                laserPoints[i].SetActive(true);

                distances[i] = (float) hit.distance;
            } else {
                laserPoints[i].SetActive(false);

                distances[i] = range;
            }
        }

        LaserScanMsg laserScanMsg = new LaserScanMsg(
            new HeaderMsg(
                seq,
                Clock.GetTimeMsg(),
                frameId
            ),
            minAngle,
            maxAngle,
            increment,
            Time.deltaTime,
            10,
            0,
            range,
            distances,
            distances
        );

        ros.Publish(topicName, laserScanMsg);

        seq++;

        Debug.Log("Pub Scan");

        counter = (counter + 1) % numBeams; 
    }
}
