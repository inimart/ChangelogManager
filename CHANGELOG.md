# Changelog

## [1.0.0] – 2024‑06‑28

### Added

* `ChangelogManagerWindow` editor UI for maintaining per‑scene changelogs and build version.
* JSON serialization to `Assets/Resources/ChangelogInfo.json`.
* Automatic patch increment and `bundleVersion` sync.
* Pre‑build processor that embeds `bundleVersionCode` and writes a Markdown summary for Android builds.
