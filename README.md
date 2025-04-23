# Changelog Manager for Unity

An **Editor‑only** window that makes it easy to keep a per‑scene changelog and automatically embed version data into your builds.

---

## Features

* `Tools ▸ Changelog Manager` window with a clean UI for editing:
  * **Build Version** (auto‑increments patch each save)
  * Scene list with version, description, and multiple changelog entries
* Stores data in `Assets/Resources/ChangelogInfo.json` – easily loaded at runtime
* Generates a Markdown summary next to the APK during **Android** builds (Quest compatible)
* Automatically updates `PlayerSettings.bundleVersion` and Android **BundleVersionCode**

### In‑game display

If you want to show the infos inside your app, the package includes **LoadSceneInfo.cs** (Runtime folder).

* Drag it onto a GameObject and wire up TextMeshProUGUI references.
* It will read `Resources/ChangelogInfo.json` at startup and populate the UI with the current scene's data.

---

## Installation

### Via Unity Package Manager (Git URL)

1. Open **Window ▸ Package Manager**.
2. Click the **+** button ▸ **Add package from Git URL…**
3. Enter: [https://github.com/inimart/ChangelogManager.git]

---

## Quick Start

1. Open the window: **Tools ▸ Changelog Manager**.
2. Set an initial **Build Version** (e.g. `1.0.000`).
3. Click **Add New Scene Info** – fill in name, version, description.
4. Within each scene foldout click **Add Changelog Entry** to document changes – scene version auto‑increments +0.1.
5. Press **Save to JSON** – the file is created/updated and `bundleVersion` is synced.
6. Build for Android – the processor writes `<YourApk>_SceneInfo.md` alongside the APK.

---

## Requirements & Notes

* Unity **2021.3 LTS** or newer.
* Window uses `JsonUtility`; custom fields must be serialisable.
* The Markdown generator only runs for **Android** builds (Quest).

---

## License

MIT – do what you want, attribution appreciated.
