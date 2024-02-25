using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Geometry;

public class DiffDrive : MonoBehaviour
{
    public string topicNamespace;


    // Temp for Burger
    public ArticulationBody wA1;
    public ArticulationBody wA2;
    public float maxLinearSpeed = 2; //  m/s
    public float maxRotationalSpeed = 1;//
    public float wheelRadius = 0.033f; //meters
    public float trackWidth = 0.288f; // meters Distance between tyres
    public float forceLimit = 10;
    public float damping = 10;

    string topic;

    Vector3 linearVelocity = new Vector3(0, 0, 0);
    Vector3 angularVelocity = new Vector3(0, 0, 0);

    void Start()
    {
        topic = topicNamespace + "/cmd_vel";

        ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>(topic, CmdVel);

        // Set the parameters for burger of the relevant joints (wheels)
        SetParameters(wA1);
        SetParameters(wA2);
    }

    void FixedUpdate()
    {
        RobotInput(linearVelocity.x, -angularVelocity.z);
    }

    /// <summary> assign the given joint the properties from this class's properties </summary>
    private void SetParameters(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.forceLimit = forceLimit;
        drive.damping = damping;
        joint.xDrive = drive;
    }

    /// <summary> Update move direction after receiving /cmd_vel request </summary>
    void CmdVel(TwistMsg message)
    {
        Vector3Msg linear = message.linear;
        Vector3Msg angular = message.angular;

        linearVelocity = new Vector3(
            (float)linear.x, // TODO: check if neg is correct
            (float)linear.z,
            (float)linear.y
        );

        angularVelocity = new Vector3(
            (float)angular.x,
            (float)angular.y,
            (float)angular.z
        );
    }

    private void RobotInput(float speed, float rotSpeed) // m/s and rad/s
    {
        // make sure values are not over max
        if (speed > maxLinearSpeed)
        {
            speed = maxLinearSpeed;
        }
        if (rotSpeed > maxRotationalSpeed)
        {
            rotSpeed = maxRotationalSpeed;
        }
        // calculate rot speeds from velocity for burger robot
        float wheel1Rotation = (speed / wheelRadius);
        float wheel2Rotation = wheel1Rotation;
        float wheelSpeedDiff = ((rotSpeed * trackWidth) / wheelRadius);
        if (rotSpeed != 0)
        {
            wheel1Rotation = (wheel1Rotation + (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
            wheel2Rotation = (wheel2Rotation - (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
        }
        else
        {
            wheel1Rotation *= Mathf.Rad2Deg;
            wheel2Rotation *= Mathf.Rad2Deg;
        }
        // update speeds
        SetSpeed(wA1, wheel1Rotation);
        SetSpeed(wA2, wheel2Rotation);
    }

    /// <summary> Set the speed of given Burger joint to the wheelSpeed </summary>
    private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
    {
        ArticulationDrive drive = joint.xDrive;
        if (float.IsNaN(wheelSpeed))
        {
            // drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
            drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)1;
        }
        else
        {
            drive.targetVelocity = wheelSpeed;
        }
        joint.xDrive = drive;
    }
}
