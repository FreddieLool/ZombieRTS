using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResourcePopupManager : MonoBehaviour
{
    public GameObject floatingTextPrefab;  // Your TextMeshPro prefab
    public Transform popupPoint;  // Assign a child GameObject as the popup point
    public Transform CanvasTransform;

    void Awake()
    {
        if (popupPoint == null)
        {
            popupPoint = new GameObject("PopupPoint").transform;
            popupPoint.SetParent(transform);
            popupPoint.localPosition = new Vector3(0, 5, 0);
        }

        // ignores raycasts
        Canvas canvasComponent = CanvasTransform.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.renderMode = RenderMode.WorldSpace;
            CanvasScaler canvasScaler = CanvasTransform.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.dynamicPixelsPerUnit = 10;
            }

            // ignore raycasts
            CanvasGroup canvasGroup = CanvasTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = CanvasTransform.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        foreach (Transform child in CanvasTransform)
        {
            if (child.GetComponent<TMP_Text>() != null) 
            {
                child.LookAt(child.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
            }
        }
    }

    public void PopResource(Vector3 position, string text, string resourceName)
    {
        Color textColor = GetColorForResource(resourceName);
        CreateFloatingText(position, text, textColor);
    }

    private Color GetColorForResource(string resourceName)
    {
        switch (resourceName)
        {
            case "Bone": return Color.white;
            case "Flesh": return Color.red;
            case "Biohazard": return Color.green;
            default: return Color.white;
        }
    }

    private void CreateFloatingText(Vector3 position, string text, Color color)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textGO = Instantiate(floatingTextPrefab, position, Quaternion.identity);
            textGO.transform.SetParent(CanvasTransform, false);
            textGO.GetComponent<RectTransform>().localScale = Vector3.one;
            textGO.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(position + new Vector3(0, 0.5f, 0));


            TMP_Text tmpText = textGO.GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = text;
                tmpText.color = color;
                tmpText.raycastTarget = false;  // Ensure it doesn't block raycasts
            }

            StartTextPopupEffect(textGO, position);
        }
        else
        {
            Debug.LogError("FloatingTextPrefab is not assigned in the ResourcePopupManager.");
        }
    }

    private void StartTextPopupEffect(GameObject textGameObject, Vector3 startPosition)
    {
        float randomHeight = Random.Range(1f, 3.5f);
        float randomHorizontal = Random.Range(-5f, 5f);
        Vector3 popupOffset = new Vector3(randomHorizontal, randomHeight, 0);

        Vector3 startPopupPosition = startPosition + popupOffset;

        textGameObject.transform.position = startPopupPosition;
        textGameObject.transform.localScale = Vector3.zero;

        // Random duration for the animation to add variety n' Juice
        float scaleUpDuration = Random.Range(0.55f, 0.95f);
        float stayDuration = Random.Range(0.7f, 1.5f); 
        float fadeDuration = Random.Range(0.3f, 0.50f); 

        // Animate the text to 'pop out' and then disappear
        LeanTween.scale(textGameObject, Vector3.one * 0.3f, scaleUpDuration)
                 .setEase(LeanTweenType.easeOutBack)
                 .setOnComplete(() => {
                     LeanTween.scale(textGameObject, Vector3.zero, fadeDuration)
                              .setEase(LeanTweenType.easeInBack)
                              .setDelay(stayDuration);  // Wait before starting to fade out
                     LeanTween.value(textGameObject, 1, 0, fadeDuration)
                              .setDelay(stayDuration)
                              .setOnUpdate((float alpha) => {
                                  TMP_Text tmpText = textGameObject.GetComponent<TMP_Text>();
                                  if (tmpText != null)
                                  {
                                      tmpText.color = new Color(tmpText.color.r, tmpText.color.g, tmpText.color.b, alpha);
                                  }
                              })
                              .setOnComplete(() => Destroy(textGameObject));
                 });
    }


}
