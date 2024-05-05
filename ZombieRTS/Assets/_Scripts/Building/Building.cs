using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;
    public GameObject selectionRing;
    private Collider collider;

    // Juice
    public ParticleSystem constructionParticles;
    public ParticleSystem smokeParticles;
    public ParticleSystem explosionParticles;

    private List<Coroutine> productionCoroutines = new List<Coroutine>();
    public bool Selectable { get; private set; } = false;
    private bool isConstructed = false; // Flag to check if building is constructed


    void Start()
    {
        // Initially disable the selection ring
        if (selectionRing != null) selectionRing.SetActive(false);

        collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;  // Start with the collider disabled
        }

        // Initially disable particle effects
        if (constructionParticles) constructionParticles.Stop();
        if (smokeParticles) smokeParticles.Stop();
        if (explosionParticles) explosionParticles.Stop();
    }

    private void StartProduction()
    {
        if (!isConstructed)
            return;  // Only start production if the building is fully constructed

        StopProduction(); // Stop previous coroutines if any
        foreach (var production in data.resourceProductions)
        {
            var coroutine = StartCoroutine(ManageIndividualResourceProduction(production));
            productionCoroutines.Add(coroutine);
        }
    }

    public void ActivateParticles()
    {
        if (constructionParticles) constructionParticles.Play();
        if (smokeParticles) smokeParticles.Play();
    }

    public void FinishConstruction()
    {
        isConstructed = true;
        StartProduction();
        EnableInteraction();

        if (constructionParticles) constructionParticles.Stop();
        if (explosionParticles) explosionParticles.Play();
    }

    private void StopProduction()
    {
        foreach (var coroutine in productionCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        productionCoroutines.Clear();
    }

    private void OnDestroy()
    {
        StopProduction();
    }

    private IEnumerator ManageIndividualResourceProduction(ResourceProduction production)
    {
        while (true)
        {
            yield return new WaitForSeconds(production.cycleTime);
            if (production.amountPerCycle > 0 && isConstructed) // Check if constructed before adding resources
            {
                ResourceManager.Instance.AddResource(production.resourceName, production.amountPerCycle);
                GetComponent<ResourcePopupManager>().PopResource(transform.position, "+" + production.amountPerCycle + " " + production.resourceName, production.resourceName);
            }
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

    public void DisableInteraction()
    {
        SetSelectable(false);
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    public void SetSelectable(bool selectable)
    {
        Selectable = selectable;
    }
}
