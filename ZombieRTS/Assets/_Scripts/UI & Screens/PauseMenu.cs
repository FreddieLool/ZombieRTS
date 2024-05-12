using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // Public references to UI elements
    public GameObject PausePanel;
    public GameObject PersonalLogos;
    public GameObject BGOverlay;
    public GameObject Logo;
    public Button continueButton;
    public Button mainMenuButton;
    public Button haxGameButton;
    public Button resetDataButton;

    // Animation durations
    private float fadeInDuration = 0.25f;
    private float moveDuration = 0.3f;
    private float buttonScaleDuration = 0.3f;
    private float animationSpeedMultiplier = 0.7f; // 30% faster for reverse

    // Pause state
    public static bool isPaused = false;
    // Cooldown time
    private float cooldownDuration = 1.5f;
    private float lastPauseTime = 0f;
    private bool isHovering = false;


    // Original positions and scales
    private Vector3 logoStartPosition;
    private Vector3 logoTargetPosition;
    private Vector3 buttonStartScale = Vector3.zero;
    private Vector3 buttonIntermediateScale = new Vector3(1.1f, 1.1f, 1.1f);
    private Vector3 buttonTargetScale = Vector3.one;

    // P Logos
    private Vector3 logo1StartPosition;
    private Vector3 logo1TargetPosition;
    private Vector3 logo2StartPosition;
    private Vector3 logo2TargetPosition;
    [SerializeField] private Transform logo1Transform;
    [SerializeField] private Transform logo2Transform;

    void Start()
    {
        // Store initial logo position and move it upwards for animation
        logoTargetPosition = Logo.transform.localPosition;
        logoStartPosition = logoTargetPosition + new Vector3(0, 400, 0);
        Logo.transform.localPosition = logoStartPosition;

        // Set initial button scales to zero for "pop-in" animation
        continueButton.transform.localScale = buttonStartScale;
        mainMenuButton.transform.localScale = buttonStartScale;
        haxGameButton.transform.localScale = buttonStartScale;
        resetDataButton.transform.localScale = buttonStartScale;

        logo1TargetPosition = logo1Transform.localPosition;
        logo1StartPosition = logo1TargetPosition + new Vector3(0, 200, 0);
        logo1Transform.localPosition = logo1StartPosition;

        logo2TargetPosition = logo2Transform.localPosition;
        logo2StartPosition = logo2TargetPosition + new Vector3(0, 200, 0);
        logo2Transform.localPosition = logo2StartPosition;

        // Attach listeners
        continueButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(ReturnToMenu);
        haxGameButton.onClick.AddListener(() => { GameManager.Instance.HaxResources(); AudioManager.Instance.PlayClickSound(); });
        resetDataButton.onClick.AddListener(() => { GameManager.Instance.ResetSaveData(); AudioManager.Instance.PlayClickSound(); });

        // Initially hide the pause panel
        PausePanel.SetActive(false);

        SetupButtonEffects();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check cooldown
            if (Time.time - lastPauseTime >= cooldownDuration)
            {
                if (!isPaused)
                {
                    ShowPausePanel();
                    Time.timeScale = 0;
                }
                else
                {
                    Resume();
                }
                lastPauseTime = Time.time; // Update the last pause time
            }
        }
    }


    private void SetupButtonEffects()
    {
        Color color;

        ColorUtility.TryParseHtmlString("#000000D9", out color);
        ColorUtility.TryParseHtmlString("#000000D9", out color);
        ColorUtility.TryParseHtmlString("#000000D9", out color);
        ColorUtility.TryParseHtmlString("#000000D9", out color);

        AddButtonEffects(continueButton, color);
        AddButtonEffects(mainMenuButton, color);
        AddButtonEffects(haxGameButton, color);
        AddButtonEffects(resetDataButton, color);
    }

    private void AddButtonEffects(Button button, Color hoverColor)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        bool isHovering = false;

        // Hover Enter
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) =>
        {
            isHovering = true;
            OnHoverEnter(button, hoverColor);
        });
        trigger.triggers.Add(pointerEnter);

        // Hover Exit
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) =>
        {
            isHovering = false;
            // Use a coroutine to delay the OnHoverExit call
            StartCoroutine(DelayedOnHoverExit(button));
        });
        trigger.triggers.Add(pointerExit);

    }

    private IEnumerator DelayedOnHoverExit(Button button)
    {
        yield return null; // Wait for the next frame

        if (!isHovering) // Only call OnHoverExit if the mouse is no longer hovering
        {
            OnHoverExit(button);
        }
    }

    private void ChangeButtonAlpha(Button button, float alpha)
    {
        Color color = button.image.color;
        color.a = alpha;
        button.image.color = color;
    }

    private void OnHoverEnter(Button button, Color color)
    {
        button.image.color = color;
        AudioManager.Instance.PlayHoverSound();
        LeanTween.scale(button.gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.15f).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);
    }

    private void OnHoverExit(Button button)
    {
        button.image.color = Color.black;
        ChangeButtonAlpha(button, 0.5f);

        LeanTween.scale(button.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeInQuad).setIgnoreTimeScale(true);
    }

    void ShowPausePanel()
    {
        ResetButtonStates();
        PausePanel.SetActive(true);
        isPaused = true;

        // Fade in the background overlay
        var overlayImage = BGOverlay.GetComponent<Image>();
        overlayImage.color = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 0);
        LeanTween.alpha(BGOverlay.GetComponent<RectTransform>(), 0.85f, fadeInDuration).setIgnoreTimeScale(true);

        // Move the logo down to its target position
        Logo.transform.localPosition = logoStartPosition;
        LeanTween.moveLocal(Logo, logoTargetPosition, moveDuration).setDelay(fadeInDuration).setIgnoreTimeScale(true)
            .setOnComplete(() => AnimateButtonsIn()); // Start button animation after logo animation is complete

        // Move the logos down after buttons are in place
        LeanTween.moveLocal(logo1Transform.gameObject, logo1TargetPosition, moveDuration).setDelay(fadeInDuration + buttonScaleDuration * 4).setIgnoreTimeScale(true).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveLocal(logo2Transform.gameObject, logo2TargetPosition, moveDuration).setDelay(fadeInDuration + buttonScaleDuration * 4).setIgnoreTimeScale(true).setEase(LeanTweenType.easeOutBack);
        LeanTween.scale(PersonalLogos, Vector3.one, 0.25f).setDelay(fadeInDuration + buttonScaleDuration * 4).setIgnoreTimeScale(true).setEase(LeanTweenType.easeOutBack);
    }

    private void ResetButtonStates()
    {
        continueButton.transform.localScale = buttonStartScale;
        mainMenuButton.transform.localScale = buttonStartScale;
        haxGameButton.transform.localScale = buttonStartScale;
        resetDataButton.transform.localScale = buttonStartScale;

        // Reset logos
        logo1Transform.localPosition = logo1StartPosition;
        logo2Transform.localPosition = logo2StartPosition;
        LeanTween.cancel(logo1Transform.gameObject);
        LeanTween.cancel(logo2Transform.gameObject);
        PersonalLogos.transform.localScale = Vector3.zero;

        ChangeButtonAlpha(continueButton, 0.5f);
        ChangeButtonAlpha(mainMenuButton, 0.5f);
        ChangeButtonAlpha(haxGameButton, 0.5f);
        ChangeButtonAlpha(resetDataButton, 0.5f);

        // Cancel any ongoing LeanTween animations on the buttons
        LeanTween.cancel(continueButton.gameObject);
        LeanTween.cancel(mainMenuButton.gameObject);
        LeanTween.cancel(haxGameButton.gameObject);
        LeanTween.cancel(resetDataButton.gameObject);
    }

    void AnimateButtonsIn()
    {
        LeanTween.scale(continueButton.gameObject, buttonIntermediateScale, buttonScaleDuration / 2)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    LeanTween.scale(continueButton.gameObject, buttonTargetScale, buttonScaleDuration / 2).setIgnoreTimeScale(true);
                });

        LeanTween.scale(mainMenuButton.gameObject, buttonIntermediateScale, buttonScaleDuration / 2)
                .setDelay(buttonScaleDuration / 2)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    LeanTween.scale(mainMenuButton.gameObject, buttonTargetScale, buttonScaleDuration / 2).setIgnoreTimeScale(true);
                });

        LeanTween.scale(haxGameButton.gameObject, buttonIntermediateScale, buttonScaleDuration / 2)
                .setDelay(buttonScaleDuration)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    LeanTween.scale(haxGameButton.gameObject, buttonTargetScale, buttonScaleDuration / 2).setIgnoreTimeScale(true);
                });

        LeanTween.scale(resetDataButton.gameObject, buttonIntermediateScale, buttonScaleDuration / 2)
                .setDelay(buttonScaleDuration * 1.5f)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    LeanTween.scale(resetDataButton.gameObject, buttonTargetScale, buttonScaleDuration / 2).setIgnoreTimeScale(true);
                    Time.timeScale = 0; // Pause the game only after all animations are complete
                });
    }

    public void Resume()
    {
        AudioManager.Instance.PlayClickSound();
        OnHoverExit(continueButton);
        isHovering = false;
        Time.timeScale = 1;
        // Reverse the animations but 30% faster
        LeanTween.alpha(BGOverlay.GetComponent<RectTransform>(), 0, fadeInDuration * animationSpeedMultiplier).setIgnoreTimeScale(true);
        LeanTween.moveLocal(Logo, logoStartPosition, moveDuration * animationSpeedMultiplier).setIgnoreTimeScale(true);

        // Move logos up when resuming
        LeanTween.moveLocal(logo1Transform.gameObject, logo1StartPosition, moveDuration * animationSpeedMultiplier).setIgnoreTimeScale(true).setEase(LeanTweenType.easeInBack);
        LeanTween.moveLocal(logo2Transform.gameObject, logo2StartPosition, moveDuration * animationSpeedMultiplier).setIgnoreTimeScale(true).setEase(LeanTweenType.easeInBack);
        LeanTween.scale(PersonalLogos, Vector3.zero, 0.25f * animationSpeedMultiplier).setIgnoreTimeScale(true).setEase(LeanTweenType.easeInBack);

        LeanTween.scale(continueButton.gameObject, buttonStartScale, buttonScaleDuration * animationSpeedMultiplier).setIgnoreTimeScale(true);
        LeanTween.scale(mainMenuButton.gameObject, buttonStartScale, buttonScaleDuration * animationSpeedMultiplier).setIgnoreTimeScale(true);
        LeanTween.scale(haxGameButton.gameObject, buttonStartScale, buttonScaleDuration * animationSpeedMultiplier).setIgnoreTimeScale(true);
        LeanTween.scale(resetDataButton.gameObject, buttonStartScale, buttonScaleDuration * animationSpeedMultiplier).setIgnoreTimeScale(true)
            .setOnComplete(() => 
            { 
                isPaused = false; 
                PausePanel.SetActive(false); 
            });
    }


    public void ReturnToMenu()
    {
        AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1;
        GameManager.Instance.SaveGameState();
        SceneManager.LoadScene(0);
    }
}
