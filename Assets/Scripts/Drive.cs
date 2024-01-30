using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
 
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class Drive : MonoBehaviour
{
    public string topicNamespace;
    string topic;

    public Vector3 linearVelocity = new Vector3(0, 0, 0);
    public Vector3 angularVelocity = new Vector3(0, 0, 0); 
    private Rigidbody rb;

    void Start() {
        topic = topicNamespace + "/cmd_vel";
        rb = GetComponent<Rigidbody>();

        ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>(topic, CmdVel);
    }

    void Update() {
        // invert the rotation since the conversion doesn't work correctly
        Quaternion rotationChange = Quaternion.Euler(Mathf.Rad2Deg * Time.deltaTime * (-1) * angularVelocity);
        transform.rotation *= rotationChange;

        // TODO:
        // Adjust the linear velocity to be acting from the center of the robot.
        // The center of mass is specified in a URDF script which is currently
        // disabled when loading the robot. The center of mass conversion is done
        // in the diff_drive plugin of flatland. It will improve simulation accuracy.

        transform.position += linearVelocity * Time.deltaTime;
    }

    void CmdVel(TwistMsg message) {
        // linear velocity given in the local/body reference frame
        linearVelocity = message.linear.From<FLU>();
        // convert from local fram to global frame
        linearVelocity = transform.TransformDirection(linearVelocity);
        
        angularVelocity = message.angular.From<FLU>();
    }
}
