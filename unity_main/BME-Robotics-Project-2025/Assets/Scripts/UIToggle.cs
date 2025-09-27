using UnityEngine;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour
{
    // == UI Text component for updating the button label dynamically
    public Text buttonText;

    // == String identifiers for the canvases that will be toggled
    [SerializeField] private string mainCanvasName = "MainUICanvas";
    [SerializeField] private string biopsyCanvasName = "BiopsyCanvas";

    // == Cached references to the primary canvases
    private GameObject mainCanvas;
    private GameObject biopsyCanvas;

    // == Tracks the visibility state of the UI
    private bool uiVisible = true;

    void Start()
    {
        // == Attempt to locate the main UI canvas at initialization
        mainCanvas = GameObject.Find(mainCanvasName);

        // == Provide a warning if the specified canvas is not found in the scene
        if (mainCanvas == null)
            Debug.LogWarning($"Main canvas '{mainCanvasName}' not found!");
    }

    public void ToggleUI()
    {
        // == Ensure biopsy canvas reference is established before use
        if (biopsyCanvas == null)
        {
            biopsyCanvas = GameObject.Find(biopsyCanvasName);
            if (biopsyCanvas == null)
            {
                Debug.LogWarning($"Biopsy canvas '{biopsyCanvasName}' not found!");
            }
        }

        // == Invert the visibility state
        uiVisible = !uiVisible;

        // == Apply the new visibility state to each canvas, if available
        if (mainCanvas != null)
            mainCanvas.SetActive(uiVisible);

        if (biopsyCanvas != null)
            biopsyCanvas.SetActive(uiVisible);

        // == Update the button label to reflect the new state
        if (buttonText != null)
            buttonText.text = uiVisible ? "Hide UI" : "Show UI";
    }
}

