using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Loads scene information from SceneInfo.json and displays it in UI text components
/// </summary>
public class LoadSceneInfo : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI BuildVersionText;
    [Tooltip("Text component to display the scene name")]
    public TextMeshProUGUI SceneNameText;
    
    [Tooltip("Text component to display the scene version")]
    public TextMeshProUGUI SceneVersionText;
    
    [Tooltip("Text component to display the scene description")]
    public TextMeshProUGUI SceneDescriptionText;
    
    [Tooltip("Text component to display the scene changelog")]
    public TextMeshProUGUI SceneChangelogText;
    
    [Header("Settings")]
    [Tooltip("Name of the JSON file in the Resources folder (without extension)")]
    public string JsonFileName = "ChangelogInfo";
    
    [Tooltip("Text to display if scene info is not found")]
    public string NotFoundText = "Information not available";
    
    // Current scene name
    private string CurrSceneName;
    
    // Scene info data loaded from JSON
    private SceneInfoData sceneInfoData;
    
    private void Start()
    {
        // Get current scene name
        CurrSceneName = SceneManager.GetActiveScene().name;
        
        // Load scene info
        LoadSceneInfoFromResources();
        
        // Display scene info
        DisplaySceneInfo();
    }
    
    /// <summary>
    /// Loads scene information from Resources folder
    /// </summary>
    private void LoadSceneInfoFromResources()
    {
        try
        {
            // Load the JSON file from the Resources folder
            TextAsset jsonTextAsset = Resources.Load<TextAsset>(JsonFileName);
            
            if (jsonTextAsset != null)
            {
                string jsonContent = jsonTextAsset.text;
                ProcessJsonContent(jsonContent);
                Debug.Log($"Successfully loaded scene info from Resources");
            }
            else
            {
                Debug.LogWarning($"Scene info file not found in Resources: {JsonFileName}");
                sceneInfoData = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading scene info: {e.Message}");
            sceneInfoData = null;
        }
    }

    private void ProcessJsonContent(string jsonContent)
    {
        try
        {
            sceneInfoData = JsonUtility.FromJson<SceneInfoData>(jsonContent);
            
            // Process the loaded data to restore newlines
            foreach (var scene in sceneInfoData.Scenes)
            {
                // Restore newlines in scene descriptions
                if (!string.IsNullOrEmpty(scene.SceneDescription))
                {
                    scene.SceneDescription = scene.SceneDescription.Replace("\\n", "\n");
                }
                
                // Also restore newlines in changelog descriptions
                if (scene.SceneChangelog != null)
                {
                    foreach (var changelog in scene.SceneChangelog)
                    {
                        if (!string.IsNullOrEmpty(changelog.ChangelogDescription))
                        {
                            changelog.ChangelogDescription = changelog.ChangelogDescription.Replace("\\n", "\n");
                        }
                    }
                }
            }
            
            Debug.Log($"Successfully processed scene info");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing scene info: {e.Message}");
            sceneInfoData = null;
        }
    }
    
    /// <summary>
    /// Displays the scene information in the UI text components
    /// </summary>
    private void DisplaySceneInfo()
    {
        // Find scene info for current scene
        SceneInfoEntry sceneInfo = FindSceneInfo();
        
        if (sceneInfo != null)
        {
            BuildVersionText.text = "AppVersion: " + Application.version + " BundleVersionCode: " + sceneInfoData.BundleVersionCode;
            // Display scene info
            if (SceneNameText != null)
                SceneNameText.text = sceneInfo.SceneName;
                
            if (SceneVersionText != null)
                SceneVersionText.text = sceneInfo.SceneVersion.ToString("F1");
                
            if (SceneDescriptionText != null)
                SceneDescriptionText.text = sceneInfo.SceneDescription;
                
            if (SceneChangelogText != null)
                SceneChangelogText.text = BuildChangelogText(sceneInfo);
                
            Debug.Log($"Displayed info for scene: {sceneInfo.SceneName}");
        }
        else
        {
            // Display not found message
            if (SceneNameText != null)
                SceneNameText.text = CurrSceneName;
                
            if (SceneVersionText != null)
                SceneVersionText.text = NotFoundText;
                
            if (SceneDescriptionText != null)
                SceneDescriptionText.text = NotFoundText;
                
            if (SceneChangelogText != null)
                SceneChangelogText.text = NotFoundText;
                
            Debug.LogWarning($"No info found for scene: {CurrSceneName}");
        }
    }
    
    /// <summary>
    /// Finds scene info for the current scene
    /// </summary>
    private SceneInfoEntry FindSceneInfo()
    {
        if (sceneInfoData == null || sceneInfoData.Scenes == null)
            return null;
            
        foreach (var sceneInfo in sceneInfoData.Scenes)
        {
            if (sceneInfo.SceneName == CurrSceneName)
            {
                return sceneInfo;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Builds the changelog text from all changelog entries
    /// </summary>
    private string BuildChangelogText(SceneInfoEntry sceneInfo)
    {
        if (sceneInfo.SceneChangelog == null || sceneInfo.SceneChangelog.Count == 0)
            return "No changelog entries.";
            
        // Get the last changelog entry
        var lastEntry = sceneInfo.SceneChangelog[sceneInfo.SceneChangelog.Count - 1];
        return lastEntry.ChangelogDescription;
    }
}

// These classes must match the ones in SceneInfoManagerWindow.cs
[Serializable]
public class SceneInfoData
{
    public List<SceneInfoEntry> Scenes = new List<SceneInfoEntry>();
    public string BuildVersion = "1.0.001";
    public int BundleVersionCode = 1;
}

[Serializable]
public class SceneInfoEntry
{
    public string SceneName;
    public float SceneVersion;
    public string SceneDescription;
    public List<ChangelogEntry> SceneChangelog = new List<ChangelogEntry>();
}

[Serializable]
public class ChangelogEntry
{
    public string ChangelogVersion;
    public string ChangelogDescription;
}