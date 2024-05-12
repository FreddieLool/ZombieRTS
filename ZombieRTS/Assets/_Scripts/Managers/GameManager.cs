using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private Transform unitParent, buildingParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }



    void Start()
    {
        GameSettings settings = SaveSystem.LoadSettings();

        // Apply loaded settings
        AudioManager.Instance.SetMusicVolume(settings.musicVolume);
        AudioManager.Instance.SetSFXVolume(settings.sfxVolume);


        int gameSceneIndex = 1;
        if (SceneManager.GetActiveScene().buildIndex == gameSceneIndex)
        {
            InitializeGameScene();
        }
    }

    void Update()
    {
/*        if (Input.GetKeyDown(KeyCode.O))
        {
            ResetSaveData();
        }*/

        if (Input.GetKeyDown(KeyCode.P))
        {
            HaxResources();
        }
    }


    public void HaxResources(Action onResourcesAdded = null)
    {
        ResourceManager.Instance.AddResource("Bone", 512, onResourcesAdded);
        ResourceManager.Instance.AddResource("Flesh", 222, onResourcesAdded);
        ResourceManager.Instance.AddResource("Biohazard", 123, onResourcesAdded);

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            UIManager.Instance.UpdateResourceUI();
        }
    }



    public void ResetSaveData()
    {
        Debug.Log("Resetting save data...");
        SaveSystem.ResetSaveData();
        ResourceManager.Instance.ResetResources();
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void InitializeGameScene()
    {
        // Setup code for the game scene, such as finding parents and loading state
        unitParent = GameObject.Find("Units/Player Units")?.transform;
        buildingParent = GameObject.Find("--- BUILDINGS PARENT")?.transform;

        if (unitParent == null || buildingParent == null)
        {
            Debug.LogError("Parent objects not found in the scene!");
            return;
        }

        LoadGameState(); // Load or initialize the game state as needed
    }

    //
    //
    //
    // backup
    //
    //

    void OnApplicationQuit()
    {
        SaveGameState();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearGameObjects();
        InitializeGameScene();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClearGameObjects();
    }


    public void ClearGameObjects()
    {
        // Properly destroy all dynamically created objects in the scene to reset state
        if (unitParent != null)
        {
            foreach (Transform child in unitParent)
            {
                Destroy(child.gameObject);
            }
        }

        if (buildingParent != null)
        {
            foreach (Transform child in buildingParent)
            {
                Destroy(child.gameObject);
            }
        }
    }


    private IEnumerator SetupGameScene()
    {
        // Wait for a frame to ensure all scene objects are loaded
        yield return null;

        InitializeGameScene();
        ClearGameObjects(); // Clear previous game objects
        LoadGameState(); // Load game state after initialization
    }


    public void SaveGameState()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            List<Building> buildings = new List<Building>(FindObjectsOfType<Building>());
            List<Unit> units = new List<Unit>(FindObjectsOfType<Unit>());

            if (units.Count > 1) 
            {
                units.RemoveAt(0);
            }
            SaveSystem.SaveGame(buildings, units);
        }
    }


    private void LoadGameState()
    {
        try
        {
            Debug.Log("Loading savegame");
            GameSaveData data = SaveSystem.LoadGame();

            // Load buildings
            foreach (var buildingData in data.buildings)
            {
                BuildingData bd = Resources.Load<BuildingData>("Buildings/" + buildingData.buildingName);
                if (bd != null)
                {
                    GameObject buildingObject = Instantiate(bd.prefab, buildingData.position, Quaternion.identity);
                    buildingObject.transform.SetParent(buildingParent.transform); // Set the parent
                    Building buildingComponent = buildingObject.GetComponent<Building>();
                    if (buildingComponent != null)
                    {
                        buildingComponent.data.health = buildingData.health;
                        buildingComponent.ResumeBuildingActivity();
                    }
                }
            }

            // Load units, skipping the first one
            for (int i = 1; i < data.units.Count; i++)
            {
                var unitData = data.units[i];
                UnitData ud = Resources.Load<UnitData>("Units/" + unitData.unitName);
                if (ud != null)
                {
                    GameObject unitObject = Instantiate(ud.unitPrefab, unitData.position, Quaternion.identity);
                    unitObject.transform.SetParent(unitParent.transform); // Set the parent
                    Unit unitComponent = unitObject.GetComponent<Unit>();
                    if (unitComponent != null)
                    {
                        unitComponent.health = unitData.health;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load game state: " + ex.ToString());
        }
    }

}
