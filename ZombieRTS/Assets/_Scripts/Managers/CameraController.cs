using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{

    // If we want to select an item to follow, inside the item script add:
    // public void OnMouseDown(){
    //   CameraController.instance.followTransform = transform;
    // }
    [Range(0f,1f)]
    [SerializeField] private float zoomLevel = 1;





    [SerializeField] private CinemachineVirtualCamera vcam;

    [Header("General")]
    [SerializeField] Transform cameraTransform;
    public Transform followTransform;
    Vector3 newPosition;
    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;

    [Header("Map Limit")]
    public Vector2 minPos;
    public Vector2 maxPos;

    [Header("Camera Controls")]
    public Transform playerTransform;
    private Vector3 targetPosition;

    public Vector3 followOffset = new Vector3(0, 5, -10);
    public float smoothSpeed = 0.125f;

    [Header("Zoom Settings")]
    [Header("*values assigned inspector only")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private AnimationCurve angleCurve;


    [Header("UI for Zoom Level")]
    [SerializeField] private Slider zoomSlider; 
    [SerializeField] private CanvasGroup zoomSliderCanvasGroup; 
    private float fadeOutDelay = 2.0f; // before the slider fades out
    private float fadeDuration = 0.5f;
    private float initialFadeLevel = 0.1777f;
    private float targetFadeLevel = 0.773f;


    //// for zoom out/in on crossing maxfovs
    private float targetFov;
    private float smoothFov;
    private float currentFovVelocity;  // For SmoothDamp
    private float zoomDampTime = 0.3f;
    private float zoomReturnDelay = 1f;
    private bool isReturningToDefault = false;


    [Header("Optional Functionality")]
    [SerializeField] bool moveWithKeyboad;
    [SerializeField] bool moveWithEdgeScrolling;
    [SerializeField] bool moveWithMouseDrag;

    [Header("Keyboard Movement")]
    [SerializeField] float fastSpeed = 20f;
    [SerializeField] float normalSpeed = 1f;
    [SerializeField] float movementSensitivity = 10f; // Hardcoded Sensitivity
    float movementSpeed;

    [Header("Edge Scrolling Movement")]
    [SerializeField] float edgeSize = 50f;
    bool isCursorSet = false;
    public Texture2D cursorArrowUp;
    public Texture2D cursorArrowDown;
    public Texture2D cursorArrowLeft;
    public Texture2D cursorArrowRight;



    private Vector3 CameraPosition
    {
        get
        {
            return cameraTransform.position;
        }

        set
        {
            if (value.x < minPos.x)
                value.x = minPos.x;
            if(value.x > maxPos.x)
                value.x = maxPos.x;
            if(value.z < minPos.y)
                value.z = minPos.y;
            if(value.z > maxPos.y)
                value.z = maxPos.y;

            cameraTransform.position = value;
        }
    }

    CursorArrow currentCursor = CursorArrow.DEFAULT;
    enum CursorArrow
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        DEFAULT
    }

    void Start()
    {

        newPosition = transform.position;

        movementSpeed = normalSpeed;

        targetFov = maxFov;

        // In-Game UI
        UpdateZoomSlider(targetFov);
        SetSliderVisibility(initialFadeLevel, false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.O))
            SetCameraXZPosition(182, 154);

        // Allow Camera to follow Target
        if (followTransform != null)
        {
            transform.position = followTransform.position;
        }
        // Let us control Camera
        else
        {
            HandleCameraMovement();
            HandleZoomInput();
            SmoothlyUpdateFOV();
            UpdateAngle();
            UpdateZoomSlider(vcam.m_Lens.FieldOfView);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            followTransform = playerTransform;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTransform = null;
        }

        if (followTransform != null)
        {
            FollowPlayer();
        }
    }

    public void SetCameraXZPosition(float x, float z)
    {
        Vector3 pos = CameraPosition;
        pos.x = x;
        pos.z = z - (CameraPosition.y / Mathf.Tan(Mathf.Deg2Rad * transform.rotation.eulerAngles.x));

        CameraPosition = pos;
    }




    private void OnValidate()
    {
        vcam.m_Lens.FieldOfView = Mathf.Lerp(minFov, maxFov, zoomLevel);
        UpdateAngle();
    }

    private void UpdateAngle()
    {
        Vector3 pos = CameraPosition;
        Vector3 rotation = transform.rotation.eulerAngles;
      
        float tAngle = angleCurve.Evaluate(zoomLevel);
        pos.y = Mathf.Lerp(minHeight, maxHeight, tAngle);
        rotation.x = Mathf.Lerp(minAngle, maxAngle, tAngle);

        CameraPosition = pos;
        transform.rotation = Quaternion.Euler(rotation);



    }

    void FollowPlayer()
    {
        Vector3 desiredPosition = followTransform.position + followOffset;
        Vector3 smoothedPosition = Vector3.Lerp(CameraPosition, desiredPosition, smoothSpeed * Time.deltaTime);
        CameraPosition = smoothedPosition;

    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float previousFov = targetFov;
            targetFov -= scroll * zoomSpeed;
            targetFov = Mathf.Clamp(targetFov, minFov, maxFov);

            // whenever there is input - deal wit it!
            CancelInvoke(nameof(FadeOutSlider));
            Invoke(nameof(FadeOutSlider), fadeOutDelay);
            FadeSlider(targetFadeLevel); // Fade in when zooming
        }
    }

   
    private void ReturnToMaxFov()
    {
        targetFov = maxFov;
    }


    private void UpdateZoomSlider(float fov)
    {
        zoomSlider.value = (fov - minFov) / (maxFov - minFov);
    }

    private void FadeOutSlider()
    {
        FadeSlider(initialFadeLevel); // Fade out after inactivity
    }

    private void FadeSlider(float targetAlpha)
    {
        StartCoroutine(FadeSliderTo(targetAlpha));
    }

    private IEnumerator FadeSliderTo(float targetAlpha)
    {
        float startAlpha = zoomSliderCanvasGroup.alpha;
        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / fadeDuration);
            SetSliderVisibility(alpha, targetAlpha > 0);
            yield return null;
        }
    }

    private void SetSliderVisibility(float alpha, bool visible)
    {
        zoomSliderCanvasGroup.alpha = alpha;
        zoomSliderCanvasGroup.interactable = visible;
        zoomSliderCanvasGroup.blocksRaycasts = visible;
    }

    private void HideZoomSlider()
    {
        if (zoomSlider)
        {
            zoomSlider.gameObject.SetActive(false);
        }
    }

    private void SmoothlyUpdateFOV()
    {
        if (Mathf.Abs(vcam.m_Lens.FieldOfView - targetFov) > 0.1f)
        {
            // Simulate a bounce by temporarily setting a target beyond the final target
            float bounceTarget = targetFov + (targetFov - smoothFov) * 0.1f;
            smoothFov = Mathf.SmoothDamp(vcam.m_Lens.FieldOfView, bounceTarget, ref currentFovVelocity, zoomDampTime);
            vcam.m_Lens.FieldOfView = smoothFov;
        }
        else
        {
            vcam.m_Lens.FieldOfView = targetFov;
        }

        zoomLevel = Mathf.InverseLerp(minFov, maxFov, vcam.m_Lens.FieldOfView);
    }

    // 1. camera zoom in zoom out.
    // 2. button to appear when player is out of camera view, click to animate, and bring player to center.
    // 3. todo:
    // Scrolling Smooth Zoomin
    // Shift + wasd for faster movement
    // 


    void HandleCameraMovement()
    {
        // Mouse Drag
        if (moveWithMouseDrag)
        {
            HandleMouseDragInput();
        }

        Vector3 pos = CameraPosition;

        // Keyboard Control
        if (moveWithKeyboad)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movementSpeed = fastSpeed;
            }
            else
            {
                movementSpeed = normalSpeed;
            }

            

            if (Input.GetKey(KeyCode.W))
                pos.z += movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.S))
                pos.z -= movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
                pos.x -= movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.D))
                pos.x += movementSpeed * Time.deltaTime;


        }


        // Edge Scrolling
        if (moveWithEdgeScrolling)
        {

            // Move Right
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                pos.x += movementSpeed * Time.deltaTime;
                ChangeCursor(CursorArrow.RIGHT);
                isCursorSet = true;
            }

            // Move Left
            else if (Input.mousePosition.x < edgeSize)
            {
                pos.x -= movementSpeed * Time.deltaTime;
                ChangeCursor(CursorArrow.LEFT);
                isCursorSet = true;
            }

            // Move Up
            else if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                pos.z += movementSpeed * Time.deltaTime;
                ChangeCursor(CursorArrow.UP);
                isCursorSet = true;
            }

            // Move Down
            else if (Input.mousePosition.y < edgeSize)
            {
                pos.z -= movementSpeed * Time.deltaTime;
                ChangeCursor(CursorArrow.DOWN);
                isCursorSet = true;
            }
            else
            {
                if (isCursorSet)
                {
                    ChangeCursor(CursorArrow.DEFAULT);
                    isCursorSet = false;
                }
            }
        }

        CameraPosition = pos;

        Cursor.lockState = CursorLockMode.Confined; // If we have an extra monitor we don't want to exit screen bounds
    }

    private void ChangeCursor(CursorArrow newCursor)
    {
        // Only change cursor if its not the same cursor
        if (currentCursor != newCursor)
        {
            switch (newCursor)
            {
                case CursorArrow.UP:
                    Cursor.SetCursor(cursorArrowUp, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorArrow.DOWN:
                    Cursor.SetCursor(cursorArrowDown, new Vector2(cursorArrowDown.width, cursorArrowDown.height), CursorMode.Auto); // So the Cursor will stay inside view
                    break;
                case CursorArrow.LEFT:
                    Cursor.SetCursor(cursorArrowLeft, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorArrow.RIGHT:
                    Cursor.SetCursor(cursorArrowRight, new Vector2(cursorArrowRight.width, cursorArrowRight.height), CursorMode.Auto); // So the Cursor will stay inside view
                    break;
                case CursorArrow.DEFAULT:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
            }

            currentCursor = newCursor;
        }
    }



    private void HandleMouseDragInput()
    {

        if (Input.GetMouseButtonDown(2) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(2) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                CameraPosition += dragStartPosition - dragCurrentPosition;
            }
        }
    }
}
