using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;
    public GameObject selectionRing;
    private Collider collider;
    public bool Selectable { get; private set; } = false;

    void Start()
    {
        // Initially disable the selection ring
        if (selectionRing != null) selectionRing.SetActive(false);

        collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;  // Start with the collider disabled
        }
    }

    public void DisplayInfo()
    {
        UIManager.Instance.ShowBuildingInfo(this);
        // Activate the selection ring
        if (selectionRing != null) selectionRing.SetActive(true);
    }

    public void HideInfo()
    {
        UIManager.Instance.HideUI();
        // Deactivate the selection ring
        if (selectionRing != null) selectionRing.SetActive(false);
    }

    // enable interaction with the building
    public void EnableInteraction()
    {
        SetSelectable(true);
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    public void SetSelectable(bool selectable)
    {
        Selectable = selectable;
    }
}
