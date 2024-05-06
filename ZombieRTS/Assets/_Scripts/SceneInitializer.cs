using UnityEngine;


// Makes sure AudioManager & GameManager are on all scenes. (if not starting from menu)
public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    [SerializeField] private GameObject resourceManagerPrefab;

    void Awake()
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
