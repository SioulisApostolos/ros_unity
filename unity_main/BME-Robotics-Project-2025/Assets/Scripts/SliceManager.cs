using UnityEngine;
using UnityEngine.UI;

public class SliceManager : MonoBehaviour
{
    // === References to Brain Model and UI Elements ===
    public Transform brainModel;

    public Slider sliderAxial;
    public Slider sliderSagittal;
    public Slider sliderCoronal;

    public SliceDisplay axialDisplay;
    public SliceDisplay sagittalDisplay;
    public SliceDisplay coronalDisplay;

    // === Internal State ===
    private Bounds bounds;     // Bounds of the brain model
    private Loader1 loader;    // Loader singleton instance

    void Start()
    {
        // === Initialize Loader and Validate ===
        loader = Loader1.Instance;
        if (!loader) { Debug.LogError("Loader1 not found!"); return; }

        // === Get Bounds of Brain Model ===
        bounds = brainModel.GetComponent<Renderer>().bounds;

        // === Initialize Slice Displays with Model Bounds ===
        axialDisplay.Init(bounds);
        sagittalDisplay.Init(bounds);
        coronalDisplay.Init(bounds);

        // === Setup Sliders with Slice Data and Callbacks ===
        SetupSlider(sliderAxial, loader.axialSlices.Length, UpdateAxial);
        SetupSlider(sliderSagittal, loader.sagittalSlices.Length, UpdateSagittal);
        SetupSlider(sliderCoronal, loader.coronalSlices.Length, UpdateCoronal);

        // == Disable arrow key navigation
        DisableSliderNavigation(sliderAxial);
        DisableSliderNavigation(sliderSagittal);
        DisableSliderNavigation(sliderCoronal);

        // === Initialize Displays with Current Slider Values ===
        UpdateAxial(sliderAxial.value);
        UpdateSagittal(sliderSagittal.value);
        UpdateCoronal(sliderCoronal.value);
    }

    // === Slider Setup Helper ===
    void SetupSlider(Slider slider, int count, UnityEngine.Events.UnityAction<float> callback)
    {
        slider.minValue = 0;
        slider.maxValue = count - 1;
        slider.wholeNumbers = true;
        slider.onValueChanged.AddListener(callback);
    }

    // Disable arrow keys navigation for sliders
    void  DisableSliderNavigation(Slider slider)
    {
        var nav = slider.navigation;
        nav.mode = Navigation.Mode.None; // prevenets arrow keys from moving the sliders
        slider.navigation = nav;
    }

    // === Axial Slice Update ===
    void UpdateAxial(float index)
    {
        int i = Mathf.Clamp((int)index, 0, loader.axialSlices.Length - 1);
        float normalized = i / (float)(loader.axialSlices.Length - 1);
        axialDisplay.SetSlice(loader.axialSlices[i], normalized);
    }

    // === Sagittal Slice Update ===
    void UpdateSagittal(float index)
    {
        int i = Mathf.Clamp((int)index, 0, loader.sagittalSlices.Length - 1);
        float normalized = i / (float)(loader.sagittalSlices.Length - 1);
        sagittalDisplay.SetSlice(loader.sagittalSlices[i], normalized);
    }

    // === Coronal Slice Update ===
    void UpdateCoronal(float index)
    {
        int i = Mathf.Clamp((int)index, 0, loader.coronalSlices.Length - 1);
        float normalized = i / (float)(loader.coronalSlices.Length - 1);
        coronalDisplay.SetSlice(loader.coronalSlices[i], normalized);
    }
}

