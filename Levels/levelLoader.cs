using UnityEngine;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    private Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>(); // Level ID → LevelData

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değiştiğinde kaybolmaz
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadAllLevels();
    }

    void LoadAllLevels()
    {

        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Levels");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonFile.text);
            levels[levelData.level_number] = levelData; // Level ID → LevelData olarak kaydet

            
        }
    }


    public LevelData GetLevel(int levelNumber)
    {
        if (levels.ContainsKey(levelNumber))
        {
            return levels[levelNumber];
        }

        Debug.LogError($"Level {levelNumber} bulunamadı!");
        return null;
    }
}
