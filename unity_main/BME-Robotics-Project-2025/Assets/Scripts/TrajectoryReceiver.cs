using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Trajectory;
using System.Collections.Generic;

public class TrajectoryReceiver : MonoBehaviour
{
    [Header("ROS Settings")]
    [SerializeField] private string trajectoryTopic = "/planned_trajectory";

    private ROSConnection ros;
    private List<float[]> jointTrajectory = new List<float[]>();
    private string[] jointNames;
    private bool trajectoryReceived = false;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<JointTrajectoryMsg>(trajectoryTopic, TrajectoryCallback);
        Debug.Log($"TrajectoryReceiver initialized and subscribed to {trajectoryTopic}");
        Debug.Log($"ROSConnection status: {(ros != null ? "Connected" : "Failed")}");
    }

    private void TrajectoryCallback(JointTrajectoryMsg msg)
    {
        Debug.Log("=== Trajectory Callback Triggered ===");
        Debug.Log($"Received trajectory message with {msg.points.Length} points");

        jointTrajectory.Clear();
        
        jointNames = msg.joint_names;
        Debug.Log($"Joint names: [{string.Join(", ", jointNames)}]");

        for (int pointIndex = 0; pointIndex < msg.points.Length; pointIndex++)
        {
            var point = msg.points[pointIndex];
            float[] joints = new float[point.positions.Length];
            
            for (int i = 0; i < point.positions.Length; i++)
            {
                joints[i] = (float)point.positions[i];
            }
            
            jointTrajectory.Add(joints);
            
            if (pointIndex == 0 || pointIndex == msg.points.Length - 1)
            {
                Debug.Log($"Point {pointIndex}: [{string.Join(", ", joints)}]");
            }
        }

        trajectoryReceived = true;
        Debug.Log($"Successfully received trajectory with {jointTrajectory.Count} waypoints for {jointNames.Length} joints");
        
        OnTrajectoryReceived();
    }

    private void OnTrajectoryReceived()
    {
        Debug.Log("Trajectory processing complete - ready for execution");
    }

    public List<float[]> GetTrajectory()
    {
        return new List<float[]>(jointTrajectory); 
    }

    public float[] GetWaypoint(int index)
    {
        if (index >= 0 && index < jointTrajectory.Count)
        {
            return jointTrajectory[index];
        }
        Debug.LogWarning($"Invalid waypoint index: {index}. Valid range: 0-{jointTrajectory.Count - 1}");
        return null;
    }

    public string[] GetJointNames()
    {
        return jointNames;
    }

    public bool HasTrajectory()
    {
        return trajectoryReceived && jointTrajectory.Count > 0;
    }

    public int GetTrajectoryLength()
    {
        return jointTrajectory.Count;
    }

    public int GetJointCount()
    {
        return jointNames?.Length ?? 0;
    }

    public void PrintFullTrajectory()
    {
        if (!HasTrajectory())
        {
            Debug.Log("No trajectory available to print");
            return;
        }

        Debug.Log($"=== Full Trajectory ({jointTrajectory.Count} points) ===");
        for (int i = 0; i < jointTrajectory.Count; i++)
        {
            Debug.Log($"Point {i}: [{string.Join(", ", jointTrajectory[i])}]");
        }
    }

    [ContextMenu("Test Connection")]
    public void TestConnection()
    {
        Debug.Log($"ROS Connection: {(ros != null ? "OK" : "Failed")}");
        Debug.Log($"Subscribed to: {trajectoryTopic}");
        Debug.Log($"Trajectory received: {trajectoryReceived}");
        Debug.Log($"Waypoints count: {jointTrajectory.Count}");
    }
}