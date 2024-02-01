using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.TF;
using UnityEngine;

public class ROSTransformTreePublisher : MonoBehaviour
{
    string k_TfTopic = "/tf";
    
    [SerializeField]
    double m_PublishRateHz = 20f;
    [SerializeField]
    string m_GlobalFrameId = "odom";
    [SerializeField]
    GameObject m_RootGameObject;
    string m_TopicId;
    
    double m_LastPublishTimeSeconds;

    TransformTreeNode m_TransformRoot;
    ROSConnection m_ROS;

    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;

    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;

    CommandLineParser commandLineArgs;

    // Start is called before the first frame update
    void Start()
    {
        if (m_RootGameObject == null)
        {
            m_RootGameObject = gameObject;
        }

        // Init command line args
        commandLineArgs = gameObject.AddComponent<CommandLineParser>();
        commandLineArgs.Initialize();
        // k_TfTopic = commandLineArgs.sim_namespace != null ? "/" + commandLineArgs.sim_namespace + k_TfTopic : k_TfTopic;

        m_TopicId = m_RootGameObject.transform.parent.name;
        m_ROS = FindObjectOfType<ROSConnection>();
        m_TransformRoot = new TransformTreeNode(m_RootGameObject);
        m_ROS.RegisterPublisher<TFMessageMsg>(k_TfTopic);
        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;
    }

    static void PopulateTFList(List<TransformStampedMsg> tfList, TransformTreeNode tfNode, string topicId)
    {
        // TODO: Some of this could be done once and cached rather than doing from scratch every time
        // Only generate transform messages from the children, because This node will be parented to the global frame
        foreach (var childTf in tfNode.Children)
        {
            tfList.Add(TransformTreeNode.ToTransformStamped(childTf, topicId));

            if (!childTf.IsALeafNode)
            {
                PopulateTFList(tfList, childTf, topicId);
            }
        }
    }

    void PublishMessage()
    {
        var tfMessageList = new List<TransformStampedMsg>();

        var tfRootToGlobal = new TransformStampedMsg(
            new HeaderMsg(0, new TimeStamp(Clock.time), m_TopicId + "/" + m_GlobalFrameId),
            m_TopicId + "/" + m_TransformRoot.name,
            m_TransformRoot.Transform.To<FLU>());
        tfMessageList.Add(tfRootToGlobal);

        PopulateTFList(tfMessageList, m_TransformRoot, m_TopicId);

        var tfMessage = new TFMessageMsg(tfMessageList.ToArray());
        m_ROS.Publish(k_TfTopic, tfMessage);
        m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }

    void Update()
    {
        if (ShouldPublishMessage)
        {
            PublishMessage();
        }

    }
}
