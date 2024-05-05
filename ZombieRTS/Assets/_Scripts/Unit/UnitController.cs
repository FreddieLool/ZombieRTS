using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


public enum FormationType
{
    Line,
    Triangle,
    Rectangle
}

[System.Serializable]
public class FormationPattern
{
    public FormationType type;
    public Vector2[] positions;
}



public class UnitController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Unit unit;
    public Transform target;
    public LayerMask groundLayer;
    public bool isCommandedToMove;
    public UnityEvent<bool> onMovementStateChanged;
    public bool isSelected = false;
    [SerializeField] GameObject selectionIndicator;
    public GameObject groundMarker;

    [SerializeField] private GameObject groundMarkerPrefab;



    // UNit data
    public UnitData unitData;
    public bool isEnemy = false;



    void Start()
    {
        mainCamera = Camera.main;
        if (!agent)
        {
            Debug.LogError("NavMeshAgent not assigned.", this);
        }
        isSelected = false; // player is not selected at start
        UpdateSelectionIndicator();

        if (unitData != null)
        {
            ApplyUnitData();
        }
        else
        {
            Debug.LogError("UnitData not set for " + gameObject.name);
        }

        if (agent != null)
        {
            agent.avoidancePriority = Random.Range(20, 50); // Randomize priority for unit crowding..
        }
    }


    void Update()
    {
        if (!isEnemy) // Only process input for non-enemy units
        {
            ProcessInput();
            if (agent != null && agent.hasPath)
            {
                MarkerManager.Instance.CheckAndAnimateMarker(transform.position);
            }

            UpdateSelectionIndicator();

            if (Input.GetKeyDown(KeyCode.F) && isSelected)
            {
                List<UnitController> selectedUnits = SelectionManager.Instance.GetSelectedUnitControllers();
                Vector3 centerPosition = CalculateCenterPosition(selectedUnits);
                CommandUnitsToMove(centerPosition, FormationType.Rectangle, selectedUnits);
            }
        }

        FindClosestTarget();
        
        if(targetUnit != null)
        {
            Debug.Log("Attacking Target");
            if (Vector3.Distance(transform.position, targetUnit.transform.position) <= unit.attackRange)
            {
                if (unit.CanAttack())
                {
                    unit.Attack(targetUnit);
                }
            }
        }
           
        


        UpdateMovementState();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void FindClosestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        float closestDistance = float.MaxValue;
        if(hits.Length <= 0)
        {
            targetUnit = null;
            return;
        }

        foreach (var hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetUnit = hit.transform.GetComponent<Unit>();
            }
        }
    }

    public void ApplyUnitData()
    {
        if (unitData != null)
        {
            var unitStats = GetComponent<Unit>();
            if (!unitStats)
                unitStats = gameObject.AddComponent<Unit>();

            // Apply data from the scriptable object
            unitStats.Initialize(unitData.unitName, unitData.health, unitData.attackDamage, unitData.movementSpeed);
        }
        else
        {
            Debug.LogError("No unit data provided!");
        }
    }

    private Vector3[] CalculateFormationPositions(FormationType formationType, int count, Vector3 center)
    {
        Vector3[] positions = new Vector3[count];
        float interval = 1.0f;  // Distance between units

        switch (formationType)
        {
            case FormationType.Line:
                for (int i = 0; i < count; i++)
                {
                    // Spread units out along the x-axis
                    positions[i] = new Vector3(center.x + (i - count / 2.0f) * interval, center.y, center.z);
                }
                break;
        }

        return positions;
    }

    private Vector3 CalculateCenterPosition(List<UnitController> units)
    {
        Vector3 sum = Vector3.zero;
        foreach (var unit in units)
        {
            sum += unit.transform.position;
        }
        return sum / units.Count; // Average position
    }

    public void MoveToLocation(Vector3 position)
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("NavMeshAgent not found on " + gameObject.name);
                return;
            }
        }

        // Dynamically adjust the stopping distance based on the number of selected units
        agent.stoppingDistance = Mathf.Clamp(SelectionManager.Instance.GetSelectedUnitControllers().Count * 0.1f, 1.0f, 5.0f);
        agent.radius = Mathf.Clamp(0.5f - SelectionManager.Instance.GetSelectedUnitControllers().Count * 0.01f, 0.1f, 0.5f);

        agent.destination = position;
        isCommandedToMove = true;
    }



    [SerializeField]
    private FormationPattern[] formationPatterns;

    private void CommandUnitsToMove(Vector3 center, FormationType formationType, List<UnitController> units)
    {
        Vector3[] positions = CalculateFormationPositions(formationType, units.Count, center);
        for (int i = 0; i < units.Count; i++)
        {
            units[i].MoveToLocation(positions[i]);
        }
    }



    private void ProcessInput()
    {
        if (!isEnemy && Input.GetMouseButtonDown(1) && isSelected)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                Vector3 targetPosition = hit.point;
                MarkerManager.Instance.ShowMarker(targetPosition);
                MoveToLocation(targetPosition);
            }
        }
    }

    private void UpdateMovementState()
    {
        if (agent && !agent.pathPending)
        {
            float proximityRadius = 1.5f; // Radius to check for other units
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, proximityRadius, 1 << LayerMask.NameToLayer("ClickableUnit")); // Assuming all units are on a "Unit" layer
            bool isCrowded = hitColliders.Length > 1; // More than 1 because the unit will detect itself

            // Adjust stopping distance based on crowd density
            agent.stoppingDistance = isCrowded ? 7.0f : 0.3f;

            if (agent.remainingDistance > agent.stoppingDistance && agent.velocity.sqrMagnitude > 0f)
            {
                if (!isCommandedToMove)
                {
                    isCommandedToMove = true;
                    onMovementStateChanged.Invoke(true);
                }
                if(hitColliders.Length >= 4 && agent.remainingDistance <= 10)
                {
                    isCommandedToMove = false;
                    onMovementStateChanged.Invoke(false);
                }
            }
            else if (isCommandedToMove && (agent.remainingDistance <= agent.stoppingDistance || agent.velocity.sqrMagnitude == 0f))
            {
                isCommandedToMove = false;
                onMovementStateChanged.Invoke(false);
            }
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
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f, groundLayer))
                {
                    float angleDifference = Vector3.Angle(Vector3.up, hit.normal);
                    if (angleDifference > 10 && !isScalingDown)
                    {
                        isScalingDown = true;
                        float targetScaleFactor = originalScale * 0.99f;
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


    private float originalScale = 0.075f; 
    private bool isScalingDown = false; // Flag to control the scaling state
    private float detectionRadius = 10;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Unit targetUnit;
}
