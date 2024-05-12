using UnityEngine;
using System.Collections;

public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject resourceManagerPrefab;

    void Start()
    {
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        if (GameManager.Instance == null)
        {
            Instantiate(gameManagerPrefab);
        }
        if (AudioManager.Instance == null)
        {
            Instantiate(audioManagerPrefab);
        }
        if (ResourceManager.Instance == null)
        {
            Instantiate(resourceManagerPrefab);
        }
    }
}
