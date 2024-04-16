using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public GameObject LoadScreen;
    public GameObject This;
    public Slider LoadBar;
    public TextMeshProUGUI Txt;

    public void LoadScene(int sceneId)
    {
        DontDestroyOnLoad(This);
        DontDestroyOnLoad(LoadScreen);
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        LoadScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress);
            LoadBar.value = progressValue;
            yield return null;
        }
    }
}
