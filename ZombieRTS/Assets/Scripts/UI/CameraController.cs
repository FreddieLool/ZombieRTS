using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    // If we want to select an item to follow, inside the item script add:
    // public void OnMouseDown(){
    //   CameraController.instance.followTransform = transform;
    // }

    [SerializeField] private Camera mainCamera;  // Ensure this is linked to your main camera in the Inspector

    [Header("General")]
    [SerializeField] Transform cameraTransform;
    public Transform followTransform;
    Vector3 newPosition;
    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;

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
    [SerializeField] private float defaultFov;
    [SerializeField] private float maxZoomOutFov;  // value 2 trigger a snapback to maxFov
    [SerializeField] private float zoomOutResetDelay;

    [Header("UI for Zoom Level")]
    [SerializeField] private Slider zoomSlider; 
    [SerializeField] private CanvasGroup zoomSliderCanvasGroup; 
    private float fadeOutDelay = 2.0f; // before the slider fades out
    private float fadeDuration = 0.5f;
    private float initialFadeLevel = 0.1777f;
    private float targetFadeLevel = 0.773f;


    // for zoom out/in on crossing maxfovs
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
        instance = this;

        newPosition = transform.position;

        movementSpeed = normalSpeed;

        targetFov = mainCamera.fieldOfView = defaultFov;

        // In-Game UI
        UpdateZoomSlider(targetFov);
        SetSliderVisibility(initialFadeLevel, false);
    }

    void Update()
    {
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
            UpdateZoomSlider(mainCamera.fieldOfView);
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

    void FollowPlayer()
    {
        Vector3 desiredPosition = followTransform.position + followOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float previousFov = targetFov;
            targetFov -= scroll * zoomSpeed;
            targetFov = Mathf.Clamp(targetFov, minFov, maxZoomOutFov);

            if (scroll > 0 && targetFov < defaultFov) // Zooming in
            {
                CancelInvoke(nameof(ReturnToDefaultFov));
                Invoke(nameof(ReturnToDefaultFov), 1f);
            }
            else if (scroll < 0 && targetFov > maxFov) // Zooming out
            {
                CancelInvoke(nameof(ReturnToMaxFov));
                Invoke(nameof(ReturnToMaxFov), zoomOutResetDelay);
            }

            // whenever there is input - deal wit it!
            CancelInvoke(nameof(FadeOutSlider));
            Invoke(nameof(FadeOutSlider), fadeOutDelay);
            FadeSlider(targetFadeLevel); // Fade in when zooming
        }
    }

    private void CheckZoomLimits(float scrollDirection)
    {
        if (scrollDirection > 0 && targetFov < defaultFov)
        {
            // Zooming in and FOV is less than default
            CancelInvoke(nameof(ReturnToDefaultFov));
            Invoke(nameof(ReturnToDefaultFov), 1f);
        }
        else if (scrollDirection < 0 && targetFov > maxFov)
        {
            // Zooming out and FOV is greater than maxFov
            CancelInvoke(nameof(ReturnToMaxFov));
            Invoke(nameof(ReturnToMaxFov), zoomOutResetDelay);
        }
    }


    private void ReturnToMaxFov()
    {
        targetFov = maxFov;
    }


    private void UpdateZoomSlider(float fov)
    {
        zoomSlider.value = (fov - minFov) / (maxZoomOutFov - minFov);
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

    private void ReturnToDefaultFov()
    {
        targetFov = defaultFov;
    }

    private void SmoothlyUpdateFOV()
    {
        if (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.1f)
        {
            // Simulate a bounce by temporarily setting a target beyond the final target
            float bounceTarget = targetFov + (targetFov - smoothFov) * 0.1f;
            smoothFov = Mathf.SmoothDamp(mainCamera.fieldOfView, bounceTarget, ref currentFovVelocity, zoomDampTime);
            mainCamera.fieldOfView = smoothFov;
        }
        else
        {
            mainCamera.fieldOfView = targetFov;
        }
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

        float currentSpeed = normalSpeed;

        // Shift adds speed multiplier
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed *= 1.5f;
        }

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

            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                newPosition += (transform.forward * movementSpeed) * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                newPosition += (transform.forward * -movementSpeed) * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                newPosition += (transform.right * movementSpeed) * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                newPosition += (transform.right * -movementSpeed) * Time.deltaTime;
            }

            newPosition += direction * currentSpeed * Time.deltaTime;
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementSensitivity);


        // Edge Scrolling
        if (moveWithEdgeScrolling)
        {

            // Move Right
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                newPosition += (transform.right * movementSpeed) * Time.deltaTime;
                ChangeCursor(CursorArrow.RIGHT);
                isCursorSet = true;
            }

            // Move Left
            else if (Input.mousePosition.x < edgeSize)
            {
                newPosition += (transform.right * -movementSpeed) * Time.deltaTime;
                ChangeCursor(CursorArrow.LEFT);
                isCursorSet = true;
            }

            // Move Up
            else if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                newPosition += (transform.forward * movementSpeed) * Time.deltaTime;
                ChangeCursor(CursorArrow.UP);
                isCursorSet = true;
            }

            // Move Down
            else if (Input.mousePosition.y < edgeSize)
            {
                newPosition += (transform.forward * -movementSpeed) * Time.deltaTime;
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

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementSensitivity);

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

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
    }
}
