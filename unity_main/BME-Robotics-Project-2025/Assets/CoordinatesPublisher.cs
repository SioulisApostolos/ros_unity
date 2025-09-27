using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class CoordinatesPublisher : MonoBehaviour
{
    ROSConnection ros;
    public BiopsyClickManager biopsyManager;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PointMsg>("/entry_point");
        ros.RegisterPublisher<PointMsg>("/target_point");
    }

    public void PublishPoints()
    {
        if (biopsyManager.xClick.HasValue && biopsyManager.yClick.HasValue && biopsyManager.zClick.HasValue)
        {
            Vector3 target = new Vector3(
                biopsyManager.xClick.Value.x, 
                biopsyManager.yClick.Value.y, 
                biopsyManager.zClick.Value.z
            );
            PointMsg targetMsg = new PointMsg { x = target.x, y = target.y, z = target.z };
            ros.Publish("/target_point", targetMsg);
        }

        if (biopsyManager.entryPoint.HasValue)
        {
            Vector3 entry = biopsyManager.entryPoint.Value;
            PointMsg entryMsg = new PointMsg { x = entry.x, y = entry.y, z = entry.z };
            ros.Publish("/entry_point", entryMsg);
        }
    }
}
