using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Rosgraph;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class Clock : MonoBehaviour
{
    ROSConnection ros;
    string topicName = "/clock";

    float publishClockFrequency = 100;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ClockMsg>(topicName);

        InvokeRepeating("PublishClock", 0f, 1 / publishClockFrequency);
    }

    // Update is called once per frame // FixedUpdate
    void PublishClock()
    {
        ClockMsg clockMsg = new ClockMsg(
            Clock.GetTimeMsg()
        );

        ros.Publish(topicName, clockMsg);
    }

    public static TimeMsg GetTimeMsg() {
        double time = Time.time;

        uint seconds = (uint) Math.Floor(time);
        uint nanoSeconds = (uint) ((time - seconds) * 100_000_000);

        return new TimeMsg(
            seconds,
            nanoSeconds
        );
    }
}
