using UnityEngine;
using System.IO;
using System.Linq;
using Dicom;
using Dicom.Imaging;
using System;

public class Loader1 : MonoBehaviour
{
    // === Singleton Instance ===
    public static Loader1 Instance { get; private set; }

    // === DICOM Import Settings ===
    [Header("DICOM Folder Path (Editor only)")]
    public string dicomFolder = "Assets/Scripts/My Scripts/New Folder/ScalarVolume_18";

    // === Slice Data Storage ===
    public Texture2D[] axialSlices;
    public Texture2D[] sagittalSlices;
    public Texture2D[] coronalSlices;

    // === Volume Dimensions ===
    public int width, height, depth;

    // === Raw Volume Data ===
    private ushort[,,] volumeData;

    void Awake()
    {
        // Ensure Singleton Instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // === Main Load Sequence ===
        Debug.Log("Loader1: Loading DICOM volume...");
        LoadVolume();       // Step 1: Load raw voxel data from DICOM
        GenerateSlices();   // Step 2: Generate 2D slices from volume
    }

    // === Load Raw Volume Data from DICOM Files ===
    void LoadVolume()
    {
        // Collect and sort valid DICOM files with numeric indices
        var files = Directory.GetFiles(dicomFolder, "*.dcm")
            .Select(f => new {
                Path = f,
                Index = ParseFlexibleIndex(Path.GetFileNameWithoutExtension(f))
            })
            .Where(x => x.Index.HasValue)
            .OrderBy(x => x.Index.Value)
            .Select(x => x.Path)
            .ToArray();

        if (files.Length == 0)
        {
            Debug.LogError("Loader1: No valid DICOM files found.");
            return;
        }

        // Use first slice to determine width/height
        var firstImg = new DicomImage(files[0]);
        width = firstImg.Width;
        height = firstImg.Height;
        depth = files.Length;

        // Allocate 3D voxel array
        volumeData = new ushort[width, height, depth];

        // Load each DICOM slice into volumeData
        for (int z = 0; z < depth; z++)
        {
            var dicomImage = new DicomImage(files[z]);
            Texture2D tex = dicomImage.RenderImage().AsTexture2D();
            Color[] pixels = tex.GetPixels();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    volumeData[x, y, z] = (ushort)(pixels[y * width + x].grayscale * 65535);
        }
    }

    // === Generate 2D Slice Textures (Axial, Sagittal, Coronal) ===
    void GenerateSlices()
    {
        // Axial slices (XY planes stacked along Z)
        axialSlices = new Texture2D[depth];
        for (int z = 0; z < depth; z++)
            axialSlices[z] = CreateTexture2D(width, height, (x, y) => volumeData[x, y, z]);

        // Sagittal slices (YZ planes stacked along X)
        sagittalSlices = new Texture2D[width];
        for (int x = 0; x < width; x++)
            sagittalSlices[x] = CreateTexture2D(depth, height, (z, y) => volumeData[x, y, z]);

        // Coronal slices (XZ planes stacked along Y)
        coronalSlices = new Texture2D[height];
        for (int y = 0; y < height; y++)
            coronalSlices[y] = CreateTexture2D(width, depth, (x, z) => volumeData[x, y, z]);

        Debug.Log("Loader1: Slices generated.");
    }

    // === Create a Grayscale Texture2D from Voxel Values ===
    Texture2D CreateTexture2D(int w, int h, Func<int, int, ushort> getValue)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] colors = new Color[w * h];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                colors[y * w + x] = Color.Lerp(Color.black, Color.white, getValue(x, y) / 65535f);

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    // === Parse Index from Filename (Flexible Handling) ===
    private long? ParseFlexibleIndex(string name)
    {
        // If whole filename is a number, use it
        if (long.TryParse(name, out long full))
            return full;

        // Otherwise, extract digits from filename
        string digits = new string(name.Where(char.IsDigit).ToArray());
        return long.TryParse(digits, out long extracted) ? extracted : null;
    }
}

