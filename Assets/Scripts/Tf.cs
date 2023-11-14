using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class Tf : MonoBehaviour
{
    // Class Properties
    public string childFrameId;
    public string frameId;
    ROSConnection ros;
    readonly float tfUpdateFrequency = 5f;
    uint seq = 0;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        // ros.RegisterPublisher<TFMessageMsg>("/tf");

        InvokeRepeating(nameof(PublishTf), 0f, 1 / tfUpdateFrequency);
    }

    private void PublishTf()
    {
        TFMessageMsg tf = new TFMessageMsg(
            new TransformStampedMsg[] {
                new TransformStampedMsg(
                    new HeaderMsg(
                        seq,
                        Clock.GetTimeMsg(),
                        frameId
                    ),
                    childFrameId,
                    new TransformMsg(
                        new Vector3Msg(
                            transform.position.x,
                            transform.position.z,
                            transform.position.y
                        ),
                        new QuaternionMsg(
                            transform.rotation.x,
                            transform.rotation.z,
                            transform.rotation.y,
                            transform.rotation.w
                        )
                    )
                )
            }
        );

        seq++;

        ros.Publish("/tf", tf);
    }
}
