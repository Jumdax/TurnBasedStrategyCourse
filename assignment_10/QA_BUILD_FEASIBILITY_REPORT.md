# HOLLOWDEEP — QA / Release Engineer: Baseline Build Feasibility Report

**Package:** Sprint 1, Package 5 (QA / Release Engineer — Baseline Build Feasibility)
**Branch / commit at time of check:** `final-sprint-hollowdeep` @ `48ad33981da02d4a594a9c5d3f7fd3826bf06feb` (working tree clean at start)
**Date:** 2026-08-19
**Author:** QA / Release Engineer (agent)

## 1. Objective

Determine, via file-system inspection, whether the current clean baseline can produce a Unity development build, WebGL preferred — **before** any actual build was attempted, per charter §15 ("test WebGL early") and the work-packages doc's Package 5 procedure.

## 2. Unity Editor Installation Check

Unity Hub-managed editors found under `/Applications/Unity/Hub/Editor/`. The project-required version is installed:

- `/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app` — present.
- `ProjectSettings/ProjectVersion.txt` confirms the project targets exactly this editor/revision:
  ```
  m_EditorVersion: 6000.4.10f1
  m_EditorVersionWithRevision: 6000.4.10f1 (feeafc12a938)
  ```
- The editor's own `modules.json` manifest (`/Applications/Unity/Hub/Editor/6000.4.10f1/modules.json`) lists a WebGL module download URL containing the same revision hash (`feeafc12a938`), confirming the installed editor build and the available WebGL module package are for the matching revision (i.e. no version-mismatch risk if/when the module is installed later).

**Result: Editor installed and version-matched. No blocker here.**

## 3. WebGL Build Support Check (filesystem inspection, no build attempted)

Checked `/Applications/Unity/Hub/Editor/6000.4.10f1/PlaybackEngines/` directly:

```
PlaybackEngines/
└── AndroidPlayer/   (only entry present)
```

**No `WebGLSupport` subfolder exists.** For comparison, a properly installed WebGL module would place its files at `PlaybackEngines/WebGLSupport/` (confirmed via the module manifest's own `"destination": "{UNITY_PATH}/PlaybackEngines/WebGLSupport"` field for the `webgl` / "Web Build Support" module entry in `modules.json`).

Cross-checked `modules.json` for the `webgl` module entry itself — it is listed as an *available-to-install* module (id: `webgl`, name: "Web Build Support", category: "Platforms"), not as an already-applied one; its presence in the manifest only means Unity Hub knows how to install it, not that it is installed. The absence of the destination folder on disk is the authoritative signal that it is not installed.

**Result: WebGL Build Support is NOT installed for the 6000.4.10f1 editor used by this project.**

## 4. Decision Point

Per the work-packages doc's Package 5 procedure and the charter's §15/§20 stop condition:

> "If WebGL Build Support is unavailable, stop at 'report what's missing' — do not attempt to install Unity Editor modules automatically, and do not substitute a different build target without reporting that decision first."

Because WebGL support is confirmed absent on disk, **no build attempt was made** (WebGL or otherwise), **no module installation was attempted**, and **no alternate build target was substituted**. This report stops here, as instructed.

## 5. What Britt Needs to Do

1. Open Unity Hub.
2. Go to **Installs**.
3. Find the **6000.4.10f1** install.
4. Click the gear/settings icon → **Add Modules**.
5. Check **WebGL Build Support** (listed under "Platforms" in the module manifest as `webgl` / "Web Build Support").
6. Confirm/install. Per `modules.json`, this module's download is ~1.7 GB and its installed size is ~5.0 GB — allow time and disk space accordingly.
7. Once installed, QA / Release Engineer can re-run this feasibility check and, if confirmed present, proceed to an actual `-batchmode -buildTarget WebGL` development build attempt.

No other action is required from Britt for this specific package — the Editor version itself is already correct and matched.

## 6. Files Touched

**None outside this report.** This package was inspection-only:
- No source files, scenes, or prefabs were read/write-modified beyond this new report.
- No `ProjectSettings/` file was modified — no build target switch and no build attempt were performed, so Unity itself never had the opportunity to touch `ProjectSettings.asset`, `EditorBuildSettings.asset`, or any other `ProjectSettings/` file. `git status --short` immediately before writing this report showed only the untracked `assignment_10/` directory (from a different, parallel agent's package output — `PIPELINE_PROVENANCE.md` and `room_layout.json`, not touched by this package); no `ProjectSettings/` diff exists.
- No temporary Editor build-helper script was created — since WebGL support isn't installed, an actual `BuildPipeline.BuildPlayer` invocation was never attempted, so no such script was needed. (`Assets/Editor/` does not currently exist in the project at all; confirmed via directory listing.)

## 7. Acceptance Criteria Result

**(C) WebGL module/support is missing, and the required installation step is known.**

Established with direct filesystem evidence: `PlaybackEngines/` under the exact matched-revision `6000.4.10f1` editor install contains only `AndroidPlayer/`, no `WebGLSupport/`, cross-confirmed against the editor's own module manifest.

## 8. Deliberately Not Done

- Did not attempt a WebGL build (blocked by missing module — attempting would have failed immediately and told us nothing new).
- Did not attempt a build for any other target as a substitute (explicitly forbidden without reporting the decision first; not reported/approved, so not done).
- Did not install the WebGL module or invoke Unity Hub automation to do so.
- Did not create a temporary Editor build-helper script (unnecessary at this stage — see §6).
- Did not modify, stage, commit, or push anything.
- Did not touch `GameScene.unity`, any prefab, or any gameplay code.
