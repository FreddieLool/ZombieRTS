using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;


    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Settings Panel")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Button settingsBackButton;

    [Header("Credits Panel")]
    [SerializeField] private Button creditsBackButton;



    private void Start()
    {
        // Saved values
        InitializeSliders();

        // Setup buttons and effects
        SetupButtonListeners();
        SetupButtonEffects();

        // Other panels
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);

        settingsBackButton.onClick.AddListener(() =>
        {
            TogglePanel(settingsPanel, false);
            AudioManager.Instance.PlaySoundEffect(AudioManager.SoundEffect.HoverAway);
        });
        creditsBackButton.onClick.AddListener(() =>
        {
            TogglePanel(creditsPanel, false);
            AudioManager.Instance.PlaySoundEffect(AudioManager.SoundEffect.HoverAway);
        });
    }

    private void InitializeSliders()
    {
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSFXVolume();

        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }

    private void SetupButtonListeners()
    {
        playButton.onClick.AddListener(() => {
            StartGame();
            AudioManager.Instance.StopAllUIAudio();
        });
        settingsButton.onClick.AddListener(() => {
            OpenSettings();
            AudioManager.Instance.StopAllUIAudio();
        });
        creditsButton.onClick.AddListener(() => {
            OpenCredits();
            AudioManager.Instance.StopAllUIAudio();
        });
    }

    private void SetupButtonEffects()
    {
        // Define colors
        Color playButtonColor;
        Color settingsButtonColor;
        Color creditsButtonColor;
        Color backButtonColor;

        // to hex
        ColorUtility.TryParseHtmlString("#B90606D9", out playButtonColor);
        ColorUtility.TryParseHtmlString("#B87000D9", out settingsButtonColor);
        ColorUtility.TryParseHtmlString("#003CB8D9", out creditsButtonColor);
        ColorUtility.TryParseHtmlString("#90081180", out backButtonColor);

        // add button fx
        AddButtonEffects(playButton, playButtonColor);
        AddButtonEffects(settingsButton, settingsButtonColor);
        AddButtonEffects(creditsButton, creditsButtonColor);
        AddButtonEffects(settingsBackButton, backButtonColor);
        AddButtonEffects(creditsBackButton, backButtonColor);
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
        AudioManager.Instance.PlayClickSound();
        Camera.main.GetComponent<CameraControl>().StartCameraMovement();
    }

    public void Quit()
    {
        Application.Quit();
        AudioManager.Instance.PlayClickSound();
    }

    private void AddButtonEffects(Button button, Color hoverColor)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        // Hover effect
        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((data) => { OnHoverEnter(button, hoverColor); });
        trigger.triggers.Add(pointerEnter);

        // Exit hover effect
        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => { OnHoverExit(button); });
        trigger.triggers.Add(pointerExit);

        // Click effect
        // All buttons except Back buttons that use a diff sfx
        if(trigger.name != "Back Button")
        {
            var pointerClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            pointerClick.callback.AddListener((data) => { AudioManager.Instance.PlayClickSound(); });
            trigger.triggers.Add(pointerClick);
        }
    }

    private void ChangeButtonAlpha(Button button, float alpha)
    {
        Color color = button.image.color;
        color.a = alpha;
        button.image.color = color;
    }

    public void OpenSettings()
    {
        TogglePanel(settingsPanel, true);
        AudioManager.Instance.PlayClickSound();

    }

    public void OpenCredits()
    {
        TogglePanel(creditsPanel, true);
        AudioManager.Instance.PlayClickSound();
    }

    private void TogglePanel(GameObject panel, bool isActive)
    {
        AudioManager.Instance.SetUIInteractable(false);
        // Duration settings
        float closeDuration = 0.3f; // Faster close
        float openDuration = 0.5f; // Slower open

        // Cancel any ongoing animations (main and target panel)
        LeanTween.cancel(mainPanel);
        LeanTween.cancel(panel);

        if (isActive)
        {
            // First, shrink the main panel
            LeanTween.scale(mainPanel, Vector3.zero, closeDuration)
                .setEaseInBack()
                .setOnComplete(() =>
                {
                    mainPanel.SetActive(false);

                    // Then, open the subpanel after the main panel is hidden
                    panel.SetActive(true);
                    panel.transform.localScale = Vector3.zero; // Start scaled down
                    LeanTween.scale(panel, Vector3.one, openDuration)
                        .setEaseOutBack() // Animate to full size
                        .setOnComplete(() => AudioManager.Instance.SetUIInteractable(true));
                });
        }
        else
        {
            // shrink the subpanel
            LeanTween.scale(panel, Vector3.zero, closeDuration)
                .setEaseInBack()
                .setOnComplete(() =>
                {
                    panel.SetActive(false);

                    // Then, expand..
                    mainPanel.SetActive(true);
                    mainPanel.transform.localScale = Vector3.zero; // Start scaled down
                    LeanTween.scale(mainPanel, Vector3.one, openDuration)
                        .setEaseOutBack() // Animate to full size
                        .setOnComplete(() => AudioManager.Instance.SetUIInteractable(true)); 
                });
        }

        // Reset colors
        //ResetButtonColors();
    }

    private void ResetButtonColors()
    {
        playButton.image.color = Color.black;
        settingsButton.image.color = Color.black;
        creditsButton.image.color = Color.black;
        ChangeButtonAlpha(creditsBackButton, 0.5f);
        ChangeButtonAlpha(settingsBackButton, 0.5f);
    }

    private void OnHoverEnter(Button button, Color color)
    {
        if (button == settingsBackButton)
        {
            Color _settingsBackBtnColor;
            ColorUtility.TryParseHtmlString("#900811DB", out _settingsBackBtnColor);
            button.image.color = _settingsBackBtnColor;
        }
        else
        {
            button.image.color = color;
        }
        AudioManager.Instance.PlayHoverSound();
        LeanTween.scale(button.gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.15f).setEase(LeanTweenType.easeOutQuad);
    }

    private void OnHoverExit(Button button)
    {
        // default button colors 
        if (button == creditsBackButton)
        {
            Color _creditsBackBtnColor;
            ColorUtility.TryParseHtmlString("#20202080", out _creditsBackBtnColor);
            button.image.color = _creditsBackBtnColor;
        }
        if(button == settingsBackButton)
        {
            Color _settingsBackBtnColor;
            ColorUtility.TryParseHtmlString("#90081180", out _settingsBackBtnColor);
            button.image.color = _settingsBackBtnColor;
        }
        else
        {
            button.image.color = Color.black;
            ChangeButtonAlpha(button, 0.5f);
        }

        LeanTween.scale(button.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeInQuad);
        if (button.gameObject.CompareTag("Back Button")) return; // Ignore back button hover sfx. 
        AudioManager.Instance.PlayHoverAwaySound();
    }
}


