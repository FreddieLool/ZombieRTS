using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int health;
    public int experience;
}

[System.Serializable]
public class GameSettings
{
    public float musicVolume;
    public float sfxVolume;
}

public static class SaveSystem
{
    private static string playerDataPath = Path.Combine(Application.persistentDataPath, "playerData.json");
    private static string settingsDataPath = Path.Combine(Application.persistentDataPath, "settingsData.json");
    private static string gameDataPath = Path.Combine(Application.persistentDataPath, "gameData.json");

    public static void SavePlayerData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(playerDataPath, json);
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(playerDataPath))
        {
            string json = File.ReadAllText(playerDataPath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData(); // Return new data if nothing is saved
    }

    public static void SaveSettings(GameSettings settings)
    {
        PlayerPrefs.SetFloat("MusicVolume", settings.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", settings.sfxVolume);
        PlayerPrefs.Save();
    }

    public static GameSettings LoadSettings()
    {
        GameSettings settings = new GameSettings
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.65f), // Default value if not set
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f),
        };
        return settings;
    }

    // game data

    public static void SaveGame(List<Building> buildings, List<Unit> units)
    {
        GameSaveData data = new GameSaveData();
        foreach (var building in buildings)
        {
            data.buildings.Add(new BuildingSaveData
            {
                buildingName = building.data.buildingName,
                position = building.transform.position,
                health = building.data.health
            });
        }
        foreach (var unit in units)
        {
            data.units.Add(new UnitSaveData
            {
                unitName = unit.unitName,
                position = unit.transform.position,
                health = unit.health
            });
        }

        // Save resources
        foreach (var resource in ResourceManager.Instance.GetAllResources())
        {
            data.resources.Add(new ResourceSaveData
            {
                resourceName = resource.Key,
                amount = resource.Value
            });
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(gameDataPath, json);
    }

    public static GameSaveData LoadGame()
    {
        if (File.Exists(gameDataPath))
        {
            string json = File.ReadAllText(gameDataPath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        return new GameSaveData(); // Return empty data if nothing is saved
    }

    public static void ResetSaveData()
    {
        Debug.Log("Deleting all save files...");
        if (File.Exists(playerDataPath))
        {
            File.Delete(playerDataPath);
            Debug.Log("Deleted Player Data.");
        }
        if (File.Exists(settingsDataPath))
        {
            File.Delete(settingsDataPath);
            Debug.Log("Deleted Settings Data.");
        }
        if (File.Exists(gameDataPath))
        {
            File.Delete(gameDataPath);
            Debug.Log("Deleted Game Data.");
        }

        // Optionally reset PlayerPrefs if used
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class BuildingSaveData
{
    public string buildingName;
    public UnityEngine.Vector3 position;
    public int health;
}

[System.Serializable]
public class UnitSaveData
{
    public string unitName;
    public UnityEngine.Vector3 position;
    public int health;
}

[System.Serializable]
public class ResourceSaveData
{
    public string resourceName;
    public int amount;
}

[System.Serializable]
public class GameSaveData
{
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
    public List<UnitSaveData> units = new List<UnitSaveData>();
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();
}
