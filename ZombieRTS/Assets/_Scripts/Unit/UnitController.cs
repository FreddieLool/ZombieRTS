using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class UnitController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] NavMeshAgent agent;
    public LayerMask groundLayer;
    public bool isCommandedToMove;
    public UnityEvent<bool> onMovementStateChanged;
    public bool isSelected = false;  // Tracks if this unit is selected
    [SerializeField] GameObject selectionIndicator;
    public GameObject groundMarker;


    void Start()
    {
        mainCamera = Camera.main;
        if (!agent)
        {
            Debug.LogError("NavMeshAgent not assigned.", this);
        }
        isSelected = false; // player is not selected at start
        UpdateSelectionIndicator();
    }


    void Update()
    {
        ProcessInput();
        if (agent != null && agent.hasPath)
        {
            CheckAndAnimateMarker();
        }
        UpdateMovementState();
        UpdateSelectionIndicator();  // Ensure this is called every frame to adjust the ring.
    }


    private void MoveToLocation(Vector3 targetPosition)
    {
        if (agent != null)
        {
            agent.SetDestination(targetPosition);
            animationTriggered = false;
        }

        AudioManager.Instance.PlayRandomUnitMoveCommandSound();
    }

    private void ProcessInput()
    {
        if (Input.GetMouseButtonDown(1) && isSelected)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                MoveToLocation(hit.point);
                ShowGroundMarker(hit.point);
            }
        }
    }

    private void ShowGroundMarker(Vector3 position)
    {
        if (groundMarker != null)
        {
            // Stop any ongoing animations and reset the marker state
            LeanTween.cancel(groundMarker);
            groundMarker.transform.localScale = new Vector3(1f, 1f, 1f);  // Reset scale
            groundMarker.SetActive(true);

            // Adjust the position with an offset
            float heightOffset = 1f;
            Vector3 adjustedPosition = new Vector3(position.x, position.y + heightOffset, position.z);
            groundMarker.transform.position = adjustedPosition;

            // Start a new animation to show the marker
            LeanTween.moveLocalY(groundMarker, groundMarker.transform.position.y + 2.5f, 0.25f);
        }
    }


    private void UpdateMovementState()
    {
        if (agent && !agent.pathPending)
        {
            if (agent.remainingDistance > agent.stoppingDistance && agent.velocity.sqrMagnitude > 0f)
            {
                if (!isCommandedToMove)
                {
                    isCommandedToMove = true;
                    Debug.Log("Invoking");
                    onMovementStateChanged.Invoke(true);
                }
            }
            else if (isCommandedToMove && (agent.remainingDistance <= agent.stoppingDistance || agent.velocity.sqrMagnitude == 0f))
            {
                isCommandedToMove = false;
                Debug.Log("Invoking false");
                onMovementStateChanged.Invoke(false);
            }
        }
    }

    bool animationTriggered = false;
    private void CheckAndAnimateMarker()
    {
        float distanceToMarker = Vector3.Distance(agent.transform.position, groundMarker.transform.position);
        float triggerDistance = 2.5f;  // distance at which the marker should start moving up

        if (distanceToMarker <= triggerDistance && !animationTriggered)
        {
            animationTriggered = true; // prevent re-triggering
            LeanTween.moveLocalY(groundMarker, groundMarker.transform.position.y + 1.234f, 1.777f)
                .setOnComplete(() => {
                    LeanTween.scale(groundMarker, Vector3.zero, 0.133f)
                        .setOnComplete(() => {
                            groundMarker.SetActive(false);
                            groundMarker.transform.localScale = new Vector3(1f, 1f, 1f);
                            groundMarker.transform.position = new Vector3(groundMarker.transform.position.x, groundMarker.transform.position.y - 0.5f, groundMarker.transform.position.z);
                            animationTriggered = false; // flag reset once animation completes
                        });
                });
        }
    }


    // Toggle the selection state of this unit
    public void ToggleSelection(bool state)
    {
        isSelected = state;
        UpdateSelectionIndicator();
    }

    // Update based on the current state
    private void UpdateSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
            if (isSelected)
            {
                RaycastHit hit;
                // Raycast down from the unit's position to find the ground
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f, groundLayer))
                {
                    // Determine if the slope's angle warrants a scale change
                    float angleDifference = Vector3.Angle(Vector3.up, hit.normal);
                    if (angleDifference > 10 && !isScalingDown) // Threshold angle is 10 degrees
                    {
                        isScalingDown = true;
                        float targetScaleFactor = originalScale * 0.99f; // Slightly smaller than normal
                        Vector3 targetScale = new Vector3(targetScaleFactor, targetScaleFactor, targetScaleFactor);
                        LeanTween.scale(selectionIndicator, targetScale, 0.3f);
                    }
                    else if (angleDifference <= 10 && isScalingDown)
                    {
                        isScalingDown = false;
                        LeanTween.scale(selectionIndicator, new Vector3(originalScale, originalScale, originalScale), 0.3f);
                    }
                }
            }
        }
    }


    private float originalScale = 0.075f; // Initialized to one to maintain original scale when not defined
    private bool isScalingDown = false; // Flag to control the scaling state






}
