using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;

    [Header("Sound Manager")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioSource audioSource;

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
        if (audioSource == null)
        {
            Debug.LogError("Missing AudioSource component on the GameObject");
            return;
        }

        // Setup and play background music
        if (backgroundMusic == null)
        {
            Debug.LogError("Background music clip is not assigned!");
            return;
        }

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log("Background music should be playing now");

        // Setup buttons and effects
        SetupButtonListeners();
        SetupButtonEffects();

        // Other panels
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        settingsBackButton.onClick.AddListener(() => TogglePanel(settingsPanel, false));
        creditsBackButton.onClick.AddListener(() => TogglePanel(creditsPanel, false));
    }

    private void SetupButtonListeners()
    {
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
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
        ColorUtility.TryParseHtmlString("#E5A500D9", out backButtonColor);

        // add button fx
        AddButtonEffects(playButton, playButtonColor);
        AddButtonEffects(settingsButton, settingsButtonColor);
        AddButtonEffects(creditsButton, creditsButtonColor);
        AddButtonEffects(settingsBackButton, backButtonColor);
        AddButtonEffects(creditsBackButton, backButtonColor);
    }


    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1); // Load the game scene
        PlayClickSound();
    }

    public void Quit()
    {
        Application.Quit();
        PlayClickSound();
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
        var pointerClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        pointerClick.callback.AddListener((data) => { PlayClickSound(); });
        trigger.triggers.Add(pointerClick);
    }


    private void OnHoverEnter(Button button, Color color)
    {
        button.image.color = color; 
        PlayHoverSound();
    }

    private void OnHoverExit(Button button)
    {
        button.image.color = Color.black; // Reset to default color on hover exit
    }


    private void ChangeButtonAlpha(Button button, float alpha)
    {
        Color color = button.image.color;
        color.a = alpha;
        button.image.color = color;
    }

    private void PlayHoverSound()
    {
        if (hoverSound == null)
        {
            Debug.LogError("Hover sound clip is not assigned!");
            return;
        }
        audioSource.PlayOneShot(hoverSound);
    }

    private void PlayClickSound()
    {
        if (clickSound == null)
        {
            Debug.LogError("Click sound clip is not assigned!");
            return;
        }
        audioSource.PlayOneShot(clickSound);
    }

    public void OpenSettings()
    {
        TogglePanel(settingsPanel, true);
        PlayClickSound();

    }

    public void OpenCredits()
    {
        TogglePanel(creditsPanel, true);
        PlayClickSound();
    }

    private void ResetButtonColors()
    {
        playButton.image.color = Color.black; 
        settingsButton.image.color = Color.black;
        creditsButton.image.color = Color.black;
        creditsBackButton.image.color = Color.black;
        settingsBackButton.image.color = Color.black;
    }

    private void TogglePanel(GameObject panel, bool isActive)
    {
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
                        .setEaseOutBack(); // Animate to full size
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
                        .setEaseOutBack(); // Animate to full size
                });
        }

        // Reset colors or other UI elements
        ResetButtonColors();
    }

    private void SetMusicVolume(float volume)
    {
        audioSource.volume = volume;
    }

    private void SetSFXVolume(float volume)
    {
        // logica
    }

}


