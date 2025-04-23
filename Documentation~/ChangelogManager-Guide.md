# Changelog Manager – User Guide

## Window Overview

Open via **Tools ▸ Changelog Manager**

| UI Element              | Purpose                                                                               |
| ----------------------- | ------------------------------------------------------------------------------------- |
| **Build Version**       | Global build string (Major.Minor.Patch). Patch auto‑increments on each save.          |
| **Add New Scene Info**  | Creates a new scene entry.                                                            |
| **Scene Foldout**       | Edit scene name, version (one decimal), description.                                  |
| **Changelog Foldout**   | List of changelog entries for that scene.                                             |
| **Add Changelog Entry** | Adds an entry and bumps scene version by +0.1.                                        |
| **Save to JSON**        | Writes `Assets/Resources/ChangelogInfo.json`, updates `PlayerSettings.bundleVersion`. |

---

## Workflow

1. Fill **Build Version**; keep patch at 000 – the tool handles increments.
2. Add scenes and describe them.
3. For each change press **Add Changelog Entry** and edit the text.
4. When you **Build**, a Markdown report is generated next to the APK with the same information.

---

## Tips

* JSON lives in **Resources** so you can load it at runtime if needed:

```csharp
var data = Resources.Load<TextAsset>("ChangelogInfo");
var info = JsonUtility.FromJson<SceneInfoData>(data.text);
```

---

## Runtime Display (optional)

Need an in‑game popup with the current build and scene info?  Add the **LoadSceneInfo** component:

1. Create a Canvas with TextMeshProUGUI elements for build, name, version, description, and changelog.
2. Add **LoadSceneInfo** to any GameObject and assign those TMP fields.
3. On `Start()` the script:
   * Loads `Resources/ChangelogInfo.json`.
   * Finds the entry matching the active scene name.
   * Fills the text fields (or a *Not Found* fallback).
4. The first line always shows `AppVersion` (from `Application.version`) and the **BundleVersionCode** captured at build time.

You can ship the JSON file with your build or strip it for release; the script handles missing data gracefully.

---

Happy change‑tracking!
