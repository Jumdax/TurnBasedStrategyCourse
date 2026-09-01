using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Final Project - Fighter/Priest hero-select UI.
///
/// Two buttons that select an existing friendly hero via the game's existing
/// UnitActionSystem selection path (UnitActionSystem.TrySelectUnit) and, only if
/// that selection succeeds, focus the existing camera rig over that hero
/// (CameraController.FocusOnUnit). Does not introduce a second selection or
/// camera system, and never moves the camera on a rejected selection.
///
/// Also shows a persistent selected-hero highlight, reusing the same green
/// ActionButtonUI.prefab's "Selected" Image already uses elsewhere in the UI.
/// The highlight always reflects UnitActionSystem.Instance.GetSelectedUnit()
/// (via OnSelectedUnitChanged), not "last hero button clicked" - it stays
/// correct whether selection happened through these buttons or by clicking a
/// hero directly in the game world.
///
/// Must be attached to a GameObject in GameScene.unity by Britt (scene-owned),
/// with the two buttons and the CameraController wired via the Inspector - this
/// script never edits the scene itself.
/// </summary>
public class HeroSelectButtonUI : MonoBehaviour
{
    private const string FIGHTER_UNIT_NAME = "Fighter";
    private const string PRIEST_UNIT_NAME = "Priest";

    // Verified against Assets/Prefabs/ActionButtonUI.prefab's "Selected" child
    // Image (m_Color), the same green already used for the selected-action
    // highlight elsewhere in the UI.
    private static readonly Color SELECTED_HERO_COLOR = new Color(0.1764706f, 1f, 0.041651815f, 1f);

    [SerializeField] private Button fighterButton;
    [SerializeField] private Button priestButton;
    [SerializeField] private CameraController cameraController;

    private ColorBlock fighterOriginalColors;
    private ColorBlock priestOriginalColors;

    private void Awake()
    {
        // Cache each button's real, Inspector-configured colors before anything
        // ever overwrites them, so "unselected" is always a restore rather than
        // a hardcoded guess.
        if (fighterButton != null)
        {
            fighterOriginalColors = fighterButton.colors;
        }

        if (priestButton != null)
        {
            priestOriginalColors = priestButton.colors;
        }
    }

    private void OnEnable()
    {
        if (fighterButton != null)
        {
            fighterButton.onClick.AddListener(SelectFighter);
        }

        if (priestButton != null)
        {
            priestButton.onClick.AddListener(SelectPriest);
        }

        if (UnitActionSystem.Instance != null)
        {
            UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        }

        // Synchronize immediately so the correct hero button is already
        // highlighted when gameplay begins, regardless of whether a selection
        // already existed before this component's OnEnable ran.
        RefreshSelectedVisual();
    }

    private void OnDisable()
    {
        if (fighterButton != null)
        {
            fighterButton.onClick.RemoveListener(SelectFighter);
        }

        if (priestButton != null)
        {
            priestButton.onClick.RemoveListener(SelectPriest);
        }

        if (UnitActionSystem.Instance != null)
        {
            UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
        }
    }

    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        RefreshSelectedVisual();
    }

    private void RefreshSelectedVisual()
    {
        if (UnitActionSystem.Instance == null)
        {
            return;
        }

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        string selectedUnitName = selectedUnit != null ? selectedUnit.name : null;

        ApplySelectedVisual(fighterButton, fighterOriginalColors, selectedUnitName == FIGHTER_UNIT_NAME);
        ApplySelectedVisual(priestButton, priestOriginalColors, selectedUnitName == PRIEST_UNIT_NAME);
    }

    private static void ApplySelectedVisual(Button button, ColorBlock originalColors, bool isSelected)
    {
        if (button == null)
        {
            return;
        }

        if (!isSelected)
        {
            button.colors = originalColors;
            return;
        }

        // Start from the real cached colors so pressedColor, disabledColor,
        // colorMultiplier, and fadeDuration are preserved untouched.
        ColorBlock selectedColors = originalColors;
        selectedColors.normalColor = SELECTED_HERO_COLOR;
        selectedColors.highlightedColor = SELECTED_HERO_COLOR;
        selectedColors.selectedColor = SELECTED_HERO_COLOR;
        button.colors = selectedColors;
    }

    public void SelectFighter()
    {
        SelectHeroByName(FIGHTER_UNIT_NAME);
    }

    public void SelectPriest()
    {
        SelectHeroByName(PRIEST_UNIT_NAME);
    }

    private void SelectHeroByName(string heroName)
    {
        Unit hero = FindFriendlyUnitByName(heroName);
        if (hero == null)
        {
            Debug.LogWarning($"[HeroSelectButtonUI] No friendly unit named \"{heroName}\" was found via UnitManager.Instance.GetFriendlyUnitList().");
            return;
        }

        if (!UnitActionSystem.Instance.TrySelectUnit(hero))
        {
            // Selection was rejected (busy or not the player's turn) - leave the
            // camera exactly where it is.
            return;
        }

        if (cameraController != null)
        {
            cameraController.FocusOnUnit(hero);
        }
    }

    private static Unit FindFriendlyUnitByName(string unitName)
    {
        if (UnitManager.Instance == null)
        {
            return null;
        }

        foreach (Unit unit in UnitManager.Instance.GetFriendlyUnitList())
        {
            if (unit != null && unit.name == unitName)
            {
                return unit;
            }
        }

        return null;
    }
}
