using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class ChangelogEntry
{
    public string ChangelogVersion;
    public string ChangelogDescription;
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
public class SceneInfoData
{
    public List<SceneInfoEntry> Scenes = new List<SceneInfoEntry>();
    public string BuildVersion = "1.0.001";
    public int BundleVersionCode = 1;
}

public class ChangelogManagerWindow : EditorWindow
{
    private SceneInfoData sceneInfoData;
    private Vector2 scrollPosition;
    private bool[] sceneInfoFoldouts;
    private bool[] changelogFoldouts;
    private string jsonFilePath = "Assets/Resources/ChangelogInfo.json";

    [MenuItem("Tools/Changelog Manager")]
    public static void ShowWindow()
    {
        GetWindow<ChangelogManagerWindow>("Changelog Manager");
    }

    private void OnEnable()
    {
        LoadSceneInfoData();
        UpdateFoldoutArrays();
    }

    private void LoadSceneInfoData()
    {
        // Create Resources folder if it doesn't exist
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
        }

        // Load existing data or create new
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            sceneInfoData = JsonUtility.FromJson<SceneInfoData>(json);
        }
        else
        {
            sceneInfoData = new SceneInfoData();
        }

        foreach (var scene in sceneInfoData.Scenes)
        {
            // Ensure newlines are preserved in scene descriptions
            if (!string.IsNullOrEmpty(scene.SceneDescription))
            {
                scene.SceneDescription = scene.SceneDescription.Replace("\\n", "\n");
            }
            
            // Also preserve newlines in changelog descriptions
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

    }

    private void UpdateFoldoutArrays()
    {
        if (sceneInfoData == null || sceneInfoData.Scenes == null)
            return;

        sceneInfoFoldouts = new bool[sceneInfoData.Scenes.Count];
        changelogFoldouts = new bool[sceneInfoData.Scenes.Count];

        for (int i = 0; i < sceneInfoFoldouts.Length; i++)
        {
            sceneInfoFoldouts[i] = true;
            changelogFoldouts[i] = true;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Changelog Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Build Version Field
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Build Version", EditorStyles.boldLabel);
        
        GUIContent buildVersionContent = new GUIContent(
            "Build Version", 
            "Format: MajorVersion.MinorVersion.PatchVersion\nPatch version will auto-increment by .001 on save."
        );
        
        EditorGUI.BeginChangeCheck();
        sceneInfoData.BuildVersion = EditorGUILayout.TextField(buildVersionContent, sceneInfoData.BuildVersion);
        if (EditorGUI.EndChangeCheck())
        {
            // Ensure format is valid
            if (!IsValidBuildVersion(sceneInfoData.BuildVersion))
            {
                Debug.LogWarning("Build version should be in format: MajorVersion.MinorVersion.PatchVersion");
                sceneInfoData.BuildVersion = "1.0.001"; // Reset to default if invalid
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        if (GUILayout.Button("Add New Scene Info", GUILayout.Height(30)))
        {
            AddNewSceneInfo();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (sceneInfoData != null && sceneInfoData.Scenes != null)
        {
            for (int i = 0; i < sceneInfoData.Scenes.Count; i++)
            {
                DrawSceneInfoEntry(i);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Save to JSON", GUILayout.Height(30)))
        {
            SaveToJson();
        }
    }

    private void DrawSceneInfoEntry(int index)
    {
        SceneInfoEntry sceneInfo = sceneInfoData.Scenes[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        sceneInfoFoldouts[index] = EditorGUILayout.Foldout(sceneInfoFoldouts[index], $"Scene: {sceneInfo.SceneName} (v{sceneInfo.SceneVersion:F1})", true);

        if (GUILayout.Button("Remove", GUILayout.Width(80)))
        {
            if (EditorUtility.DisplayDialog("Confirm Removal", 
                $"Are you sure you want to remove the scene info for '{sceneInfo.SceneName}'?", 
                "Yes", "No"))
            {
                RemoveSceneInfo(index);
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (sceneInfoFoldouts[index])
        {
            EditorGUI.indentLevel++;

            sceneInfo.SceneName = EditorGUILayout.TextField("Scene Name", sceneInfo.SceneName);
            
            // Make version editable with tooltip
            GUIContent versionContent = new GUIContent(
                "Scene Version", 
                "Enter version in format X.Y (only one decimal place will be kept). " +
                "When adding changelog entries, version will increment by 0.1 from this value."
            );
            
            EditorGUI.BeginChangeCheck();
            float newVersion = EditorGUILayout.FloatField(versionContent, sceneInfo.SceneVersion);
            if (EditorGUI.EndChangeCheck())
            {
                // Format to ensure only one decimal place
                sceneInfo.SceneVersion = Mathf.Floor(newVersion * 10) / 10;
                
                // No longer asking to update changelog versions - they will remain as they were
                SaveToJson();
            }
            
            // Scene Description
            EditorGUILayout.LabelField("Scene Description:", EditorStyles.boldLabel);
            sceneInfo.SceneDescription = EditorGUILayout.TextArea(sceneInfo.SceneDescription, GUILayout.Height(EditorGUIUtility.singleLineHeight * 8));
            EditorGUILayout.Space(5);

            EditorGUILayout.Space();

            // Changelog section
            changelogFoldouts[index] = EditorGUILayout.Foldout(changelogFoldouts[index], "Changelog", true);
            if (changelogFoldouts[index])
            {
                EditorGUI.indentLevel++;

                if (sceneInfo.SceneChangelog != null && sceneInfo.SceneChangelog.Count > 0)
                {
                    for (int j = 0; j < sceneInfo.SceneChangelog.Count; j++)
                    {
                        DrawChangelogEntry(sceneInfo, j);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No changelog entries yet.");
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Changelog Entry"))
                {
                    AddChangelogEntry(sceneInfo);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawChangelogEntry(SceneInfoEntry sceneInfo, int index)
    {
        ChangelogEntry changelog = sceneInfo.SceneChangelog[index];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Version {changelog.ChangelogVersion}", EditorStyles.boldLabel);

        if (GUILayout.Button("Remove", GUILayout.Width(80)))
        {
            if (EditorUtility.DisplayDialog("Confirm Removal", 
                $"Are you sure you want to remove this changelog entry?", 
                "Yes", "No"))
            {
                sceneInfo.SceneChangelog.RemoveAt(index);
                SaveToJson();
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        // Display version as read-only
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Version", changelog.ChangelogVersion);
        EditorGUI.EndDisabledGroup();

        changelog.ChangelogDescription = EditorGUILayout.TextArea(changelog.ChangelogDescription, GUILayout.Height(60));

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void AddNewSceneInfo()
    {
        SceneInfoEntry newSceneInfo = new SceneInfoEntry
        {
            SceneName = "New Scene",
            SceneVersion = 1.0f,
            SceneDescription = "Description here",
            SceneChangelog = new List<ChangelogEntry>()
        };

        sceneInfoData.Scenes.Add(newSceneInfo);
        UpdateFoldoutArrays();
        SaveToJson();
    }

    private void RemoveSceneInfo(int index)
    {
        sceneInfoData.Scenes.RemoveAt(index);
        UpdateFoldoutArrays();
        SaveToJson();
    }

    private void AddChangelogEntry(SceneInfoEntry sceneInfo)
    {
        // Increment version by 0.1
        float newVersion = sceneInfo.SceneVersion + 0.1f;
        
        // If we have a .9 version, increment to the next whole number
        if (Mathf.Approximately(newVersion * 10 % 10, 0))
        {
            newVersion = Mathf.Floor(newVersion) + 0.1f;
        }
        
        sceneInfo.SceneVersion = newVersion;

        ChangelogEntry newEntry = new ChangelogEntry
        {
            ChangelogVersion = newVersion.ToString("F1"),
            ChangelogDescription = "New changes in this version"
        };

        sceneInfo.SceneChangelog.Add(newEntry);
        SaveToJson();
    }

    private bool IsValidBuildVersion(string version)
    {
        // Check if the version is in the format MajorVersion.MinorVersion.PatchVersion
        string[] parts = version.Split('.');
        if (parts.Length != 3)
            return false;
            
        // Check if all parts are numeric
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out _))
                return false;
        }
        
        return true;
    }

    private string IncrementPatchVersion(string version)
    {
        string[] parts = version.Split('.');
        if (parts.Length != 3)
            return version;
            
        // Parse the patch version
        if (int.TryParse(parts[2], out int patchVersion))
        {
            // Increment patch version
            patchVersion++;
            
            // Format the patch version with leading zeros if needed
            string patchStr = patchVersion.ToString();
            if (parts[2].Length > patchStr.Length)
            {
                patchStr = patchStr.PadLeft(parts[2].Length, '0');
            }
            
            // Reconstruct the version string
            return $"{parts[0]}.{parts[1]}.{patchStr}";
        }
        
        return version;
    }

    private void UpdateProjectVersion(string buildVersion)
    {
        // Update the build number in ProjectSettings
        PlayerSettings.bundleVersion = buildVersion;
        Debug.Log($"Updated project version to {buildVersion}");
    }

    private void SaveToJson()
    {
        // Increment patch version
        sceneInfoData.BuildVersion = IncrementPatchVersion(sceneInfoData.BuildVersion);
        
        // Update project version
        UpdateProjectVersion(sceneInfoData.BuildVersion);
        
        // Create a copy of the data to modify for serialization
        var serializationData = JsonUtility.FromJson<SceneInfoData>(JsonUtility.ToJson(sceneInfoData));
        
        // Escape newlines in descriptions before serializing
        foreach (var scene in serializationData.Scenes)
        {
            // Ensure newlines are preserved in scene descriptions
            if (!string.IsNullOrEmpty(scene.SceneDescription))
            {
                scene.SceneDescription = scene.SceneDescription.Replace("\n", "\\n");
            }
            
            // Also preserve newlines in changelog descriptions
            if (scene.SceneChangelog != null)
            {
                foreach (var changelog in scene.SceneChangelog)
                {
                    if (!string.IsNullOrEmpty(changelog.ChangelogDescription))
                    {
                        changelog.ChangelogDescription = changelog.ChangelogDescription.Replace("\n", "\\n");
                    }
                }
            }
        }

        string json = JsonUtility.ToJson(serializationData, true);
        File.WriteAllText(jsonFilePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Changelog info saved to {jsonFilePath}");
    }
}

// Class for generating the Markdown file during the build
public class SceneInfoMarkdownGenerator : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Check that it is a build for Android (Oculus Quest)
        if (report.summary.platform == BuildTarget.Android)
        {
            // Load the SceneInfo data
            string jsonPath = "Assets/Resources/ChangelogInfo.json";
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                SceneInfoData sceneInfoData = JsonUtility.FromJson<SceneInfoData>(jsonContent);
                
                // Set the BundleVersionCode from PlayerSettings
                sceneInfoData.BundleVersionCode = PlayerSettings.Android.bundleVersionCode;
                
                // Save the updated SceneInfo data
                string updatedJson = JsonUtility.ToJson(sceneInfoData, true);
                File.WriteAllText(jsonPath, updatedJson);
            }
            
            GenerateMarkdownFile(report.summary.outputPath);
        }
    }

    private void GenerateMarkdownFile(string apkPath)
    {
        string jsonPath = "Assets/Resources/ChangelogInfo.json";
        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning("ChangelogInfo.json not found. Markdown file will not be generated.");
            return;
        }

        try
        {
            // Read the JSON file
            string jsonContent = File.ReadAllText(jsonPath);
            SceneInfoData sceneInfoData = JsonUtility.FromJson<SceneInfoData>(jsonContent);

            // Create the Markdown content
            string markdownContent = GenerateMarkdownContent(sceneInfoData);

            // Determine the output path (same folder as the APK)
            string directory = Path.GetDirectoryName(apkPath);
            string fileName = Path.GetFileNameWithoutExtension(apkPath) + "_SceneInfo.md";
            string outputPath = Path.Combine(directory, fileName);

            // Write the Markdown file
            File.WriteAllText(outputPath, markdownContent);
            Debug.Log($"Changelog Info Markdown file generated at: {outputPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating Markdown file: {e.Message}");
        }
    }

    private string GenerateMarkdownContent(SceneInfoData sceneInfoData)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Header with build version and bundle version code
        sb.AppendLine("# Scene Information");
        sb.AppendLine($"**Build Version:** {sceneInfoData.BuildVersion}");
        sb.AppendLine($"**BundleVersionCode:** {sceneInfoData.BundleVersionCode}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Scene information
        if (sceneInfoData.Scenes != null && sceneInfoData.Scenes.Count > 0)
        {
            foreach (var scene in sceneInfoData.Scenes)
            {
                // Scene header
                sb.AppendLine($"## {scene.SceneName}");
                sb.AppendLine($"**Version:** {scene.SceneVersion:F1}");
                sb.AppendLine();

                // Scene description
                sb.AppendLine("### Description");
                string description = scene.SceneDescription?.Replace("\\n", "\n") ?? "No description available.";
                sb.AppendLine(description);
                sb.AppendLine();

                // Changelog
                sb.AppendLine("### Changelog");
                if (scene.SceneChangelog != null && scene.SceneChangelog.Count > 0)
                {
                    sb.AppendLine("| Version | Description |");
                    sb.AppendLine("|---------|-------------|");
                    
                    foreach (var entry in scene.SceneChangelog)
                    {
                        string changelogDesc = entry.ChangelogDescription?.Replace("\\n", "\n").Replace("|", "\\|") ?? "No description.";
                        sb.AppendLine($"| {entry.ChangelogVersion} | {changelogDesc} |");
                    }
                }
                else
                {
                    sb.AppendLine("No changelog entries available.");
                }

                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("No scene information available.");
        }

        // Add generation date and time
        sb.AppendLine($"*Generated on: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}*");

        return sb.ToString();
    }
}