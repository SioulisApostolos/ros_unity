using UnityEngine;

public class SliceDisplay : MonoBehaviour
{
    // === Enum for Slice Plane Type ===
    public enum SlicePlane { Axial, Sagittal, Coronal }
    public SlicePlane plane;

    // === Public Settings ===
    [Tooltip("Material to apply to this slice display (e.g. special shader for sagittal)")]
    public Material overrideMaterial;

    // === Private References and State ===
    private MeshRenderer meshRenderer;
    private Bounds bounds;

    void Awake()
    {
        // === Get Mesh Renderer ===
        meshRenderer = GetComponent<MeshRenderer>();

        // === Apply Override Material (Cloned Instance) ===
        if (overrideMaterial != null)
        {
            meshRenderer.material = new Material(overrideMaterial); // Clone to avoid shared changes
        }
    }

    // === Initialize with Brain Model Bounds ===
    public void Init(Bounds brainBounds)
    {
        bounds = brainBounds;
    }

    // === Set Slice Texture and Position ===
    public void SetSlice(Texture2D texture, float normalizedIndex)
    {
        // Ensure meshRenderer is valid
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        // Apply texture to material
        if (meshRenderer.material != null)
            meshRenderer.material.mainTexture = texture;

        // Calculate position and scaling based on slice plane
        Vector3 pos = bounds.center;
        Vector3 size = bounds.size;

        switch (plane)
        {
            case SlicePlane.Axial:
                // Slice along Y axis
                pos.y = Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedIndex);
                transform.rotation = Quaternion.Euler(90, 0, 0); // Looking down the Y-axis
                transform.localScale = new Vector3(size.x, size.z, 1);
                break;

            case SlicePlane.Sagittal:
                // Slice along X axis
                pos.x = Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedIndex);
                transform.rotation = Quaternion.Euler(0, 90, 0); // Facing X, upright
                transform.localScale = new Vector3(size.z, size.y, 1);
                break;

            case SlicePlane.Coronal:
                // Slice along Z axis
                pos.z = Mathf.Lerp(bounds.min.z, bounds.max.z, normalizedIndex);
                transform.rotation = Quaternion.identity; // Facing Z forward
                transform.localScale = new Vector3(size.x, size.y, 1);
                break;
        }

        // Apply calculated position
        transform.position = pos;
    }
}
