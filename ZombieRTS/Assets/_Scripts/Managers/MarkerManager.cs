using UnityEngine;

public class MarkerManager : MonoBehaviour
{
    public static MarkerManager Instance;
    [SerializeField] private GameObject groundMarkerPrefab;
    private GameObject currentGroundMarker;
    private bool animationTriggered = false;



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ShowMarker(Vector3 position)
    {
        if (currentGroundMarker != null)
        {
            Destroy(currentGroundMarker); 
        }

        currentGroundMarker = Instantiate(groundMarkerPrefab, position + Vector3.up * 0.1f, Quaternion.Euler(0, 0, -90));
        currentGroundMarker.SetActive(true);
        currentGroundMarker.transform.position = position + Vector3.up * 2f;
        animationTriggered = false;
    }

    public void CheckAndAnimateMarker(Vector3 unitPosition)
    {
        if (currentGroundMarker == null || animationTriggered) return;

        float distance = Vector3.Distance(unitPosition, currentGroundMarker.transform.position);
        if (distance < 10.5f)  // distance threshold to start animation
        {
            if (!animationTriggered)
            {
                animationTriggered = true;
                LeanTween.scale(currentGroundMarker, Vector3.zero, 0.5f).setOnComplete(() =>
                {
                    Destroy(currentGroundMarker);
                    currentGroundMarker = null;
                });
            }
        }
    }

    public void HideMarker()
    {
        if (currentGroundMarker != null)
        {
            Destroy(currentGroundMarker);
            currentGroundMarker = null;
        }
    }
}
