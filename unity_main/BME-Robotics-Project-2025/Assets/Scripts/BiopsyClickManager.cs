using UnityEngine;
using UnityEngine.UI;

public class BiopsyClickManager : MonoBehaviour
{
    public Camera cam;
    public Collider brainCollider;

    // == Quads for slices
    public GameObject xSliceQuad;
    public GameObject ySliceQuad;
    public GameObject zSliceQuad;

    public Vector3? xClick; // From X slice
    public Vector3? yClick; // From Y slice
    public Vector3? zClick; // From Z slice

    public Vector3? entryPoint;

    public CoordinatesPublisher coordinatesPublisher; 
    private GameObject currentBiopsyDot;
    private GameObject currentEntryDot;
    //private LineRenderer lineRenderer;

    private bool waitingForEntryPoint = false;

    // == UI
    private GameObject canvasGO;
    private Button resetEntryButton;
    private Button resetTargetButton;

    void Start()
    {
        if (brainCollider != null)
            brainCollider.enabled = false;
        else
            Debug.LogWarning("Brain collider not assigned!");

        // == Line Renderer setup
       // GameObject lineObj = new GameObject("BiopsyEntryLine");
        //lineRenderer = lineObj.AddComponent<LineRenderer>();
        ///lineRenderer.startWidth = 0.005f;
        //lineRenderer.endWidth = 0.005f;
        ///lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        //lineRenderer.positionCount = 0;
        //lineRenderer.startColor = Color.red;
        //lineRenderer.endColor = Color.red;

        CreateCanvasAndButtons();
        SetButtonsActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (!waitingForEntryPoint)
                {
                    string tag = hit.collider.tag;

                    switch (tag)
                    {
                        case "XSlice":
                            xClick = hit.point;
                            Debug.Log("X slice clicked at: " + xClick.Value);
                            break;
                        case "YSlice":
                            yClick = hit.point;
                            Debug.Log("Y slice clicked at: " + yClick.Value);
                            break;
                        case "ZSlice":
                            zClick = hit.point;
                            Debug.Log("Z slice clicked at: " + zClick.Value);
                            break;
                    }

                    TryCalculateBiopsyPoint();
                }
                else
                {
                    if (hit.collider == brainCollider)
                    {
                        entryPoint = hit.point;
                        Debug.Log("Entry point selected at: " + entryPoint.Value);
                        ShowEntryDot(entryPoint.Value);
                        //DrawLine();

                        waitingForEntryPoint = false;

                        if (brainCollider != null)
                            brainCollider.enabled = false;

                        SetButtonsActive(true);
                        PublishPointsToROS(); // publish to ROS 
                    }
                    else
                    {
                        Debug.Log("Fourth click must be on brain surface!");
                    }
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any collider.");
            }
        }
    }

    void TryCalculateBiopsyPoint()
    {
        if (xClick.HasValue && yClick.HasValue && zClick.HasValue)
        {
            Vector3 biopsyPoint = new Vector3(
                xClick.Value.x,
                yClick.Value.y,
                zClick.Value.z
            );

            Debug.Log("<color=green>Biopsy target calculated at:</color> " + biopsyPoint);

            ShowBiopsyDot(biopsyPoint);

            if (brainCollider != null)
                brainCollider.enabled = true;

            waitingForEntryPoint = true;

            if (currentEntryDot != null)
                Destroy(currentEntryDot);

            ///lineRenderer.positionCount = 0;

            // == Hide slice quads after target selection
            SetSlicesVisible(false);
        }
        else
        {
            Debug.Log("Waiting for clicks on all slices: " +
                      $"X:{(xClick.HasValue ? "Yes" : "No")} " +
                      $"Y:{(yClick.HasValue ? "Yes" : "No")} " +
                      $"Z:{(zClick.HasValue ? "Yes" : "No")}");
        }
    }

    void ShowBiopsyDot(Vector3 position)
    {
        if (currentBiopsyDot != null)
            Destroy(currentBiopsyDot);

        currentBiopsyDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentBiopsyDot.transform.position = position;
        currentBiopsyDot.transform.localScale = Vector3.one * 0.05f;
        Destroy(currentBiopsyDot.GetComponent<Collider>());

        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        currentBiopsyDot.GetComponent<Renderer>().material = redMat;
    }

    void ShowEntryDot(Vector3 position)
    {
        if (currentEntryDot != null)
            Destroy(currentEntryDot);

        currentEntryDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentEntryDot.transform.position = position;
        currentEntryDot.transform.localScale = Vector3.one * 0.05f;
        Destroy(currentEntryDot.GetComponent<Collider>());

        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = Color.red;
        currentEntryDot.GetComponent<Renderer>().material = redMat;
    }

    //void DrawLine()
    //{
      //  if (!entryPoint.HasValue || currentBiopsyDot == null) return;

        //lineRenderer.positionCount = 2;
        //lineRenderer.SetPosition(0, entryPoint.Value);
        //lineRenderer.SetPosition(1, currentBiopsyDot.transform.position);
    //}
        //function which handles the publish to ros
    void PublishPointsToROS()
    {
        if (coordinatesPublisher != null)
        {
            coordinatesPublisher.PublishPoints();
            Debug.Log("Points published to ROS!");
        }
        else
        {
            Debug.LogWarning("Coordinates publisher not set!");
        }
    }

    void CreateCanvasAndButtons()
    {
        // == Create Canvas
        canvasGO = new GameObject("BiopsyCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // == Create Reset Entry Button
        resetEntryButton = CreateButton("Reset Entry Point", new Vector2(-400,-100));
        resetEntryButton.onClick.AddListener(ResetEntryPoint);

        // == Create Reset Target Button
        resetTargetButton = CreateButton("Reset Target Point", new Vector2(-400,0));
        resetTargetButton.onClick.AddListener(ResetTargetPoint);
    }

    Button CreateButton(string buttonText, Vector2 anchoredPosition)
    {
        // == Create button GameObject
        GameObject buttonGO = new GameObject(buttonText + "Button");
        buttonGO.transform.SetParent(canvasGO.transform);

        // == Add Button and Image components
        Button button = buttonGO.AddComponent<Button>();
        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.8f, 0.8f, 0.8f); // light grey

        // == RectTransform setup
        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 40);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        // == Create Text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        Text text = textGO.AddComponent<Text>();
        text.text = buttonText;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.resizeTextForBestFit = true;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    void SetButtonsActive(bool active)
    {
        if (resetEntryButton != null)
            resetEntryButton.gameObject.SetActive(active);
        if (resetTargetButton != null)
            resetTargetButton.gameObject.SetActive(active);
    }

    void SetSlicesVisible(bool visible)
    {
        if (xSliceQuad != null) xSliceQuad.SetActive(visible);
        if (ySliceQuad != null) ySliceQuad.SetActive(visible);
        if (zSliceQuad != null) zSliceQuad.SetActive(visible);
    }

    // == Button Callbacks
    public void ResetEntryPoint()
    {
        Debug.Log("Reset Entry Point clicked.");
        if (currentEntryDot != null)
            Destroy(currentEntryDot);
        entryPoint = null;

        //lineRenderer.positionCount = 0;

        if (brainCollider != null)
            brainCollider.enabled = true;

        waitingForEntryPoint = true;

        SetButtonsActive(false);
    }

    public void ResetTargetPoint()
    {
        Debug.Log("Reset Target Point clicked.");

        xClick = null;
        yClick = null;
        zClick = null;

        if (currentBiopsyDot != null)
            Destroy(currentBiopsyDot);

        if (currentEntryDot != null)
            Destroy(currentEntryDot);
        entryPoint = null;

        //lineRenderer.positionCount = 0;

        if (brainCollider != null)
            brainCollider.enabled = false;

        waitingForEntryPoint = false;

        SetButtonsActive(false);

        // == Show slice quads again after reset
        SetSlicesVisible(true);
    }
}


