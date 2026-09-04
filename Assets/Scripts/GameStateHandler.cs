using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Checkpoint 3B — Victory/Loss game-state controller.
///
/// Observes the existing ChestState.OnAnyChestOpened (any chest, party-wide heal)
/// and DefeatStateDetector.OnPartyDefeated (existing loss detection, unmodified)
/// static events. Does not touch Unit.cs, UnitManager.cs, BaseAction.cs, or
/// TurnSystem.cs beyond the single small HealToFull() passthrough already added
/// to Unit.cs/HealthSystem.cs.
///
/// Must be attached to a GameObject in GameScene.unity by Britt (scene-owned),
/// with the outcome panel/text/restart button wired via the Inspector - this
/// script never edits the scene itself.
/// </summary>
public class GameStateHandler : MonoBehaviour
{
    private const string VICTORY_TEXT = "YOU ESCAPED HOLLOWDEEP";
    private const string LOSS_TEXT = "THE PARTY HAS FALLEN";

    [SerializeField] private GameObject outcomePanel;
    [SerializeField] private TextMeshProUGUI outcomeText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitToTitleButton;

    private bool gameEnded;

    private void Awake()
    {
        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ChestState.OnAnyChestOpened += ChestState_OnAnyChestOpened;
        DefeatStateDetector.OnPartyDefeated += DefeatStateDetector_OnPartyDefeated;

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (exitToTitleButton != null)
        {
            exitToTitleButton.onClick.AddListener(RestartGame);
        }
    }

    private void OnDisable()
    {
        ChestState.OnAnyChestOpened -= ChestState_OnAnyChestOpened;
        DefeatStateDetector.OnPartyDefeated -= DefeatStateDetector_OnPartyDefeated;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }

        if (exitToTitleButton != null)
        {
            exitToTitleButton.onClick.RemoveListener(RestartGame);
        }
    }

    private void ChestState_OnAnyChestOpened(object sender, ChestState chestState)
    {
        // 1. chest already successfully opened (TryOpen()'s one-shot guard already
        //    guarantees this handler only runs on the transition to open).
        // 2. heal the surviving party.
        HealAllLivingFriendlyUnits();

        // 3. victory only after healing, per the locked sequencing.
        if (chestState.IsVictoryChest())
        {
            TriggerVictory();
        }
    }

    private void DefeatStateDetector_OnPartyDefeated(object sender, EventArgs e)
    {
        TriggerLoss();
    }

    private void HealAllLivingFriendlyUnits()
    {
        if (UnitManager.Instance == null)
        {
            // Defensive only, mirrors DefeatStateDetector's own guard - UnitManager.Instance
            // is set in its own Awake(), long before any chest can be opened in normal play.
            return;
        }

        // GetFriendlyUnitList() already excludes dead/destroyed units (Unit.OnAnyUnitDead
        // removes them), so this can never revive anyone - it only ever touches units that
        // are currently alive.
        foreach (Unit friendlyUnit in UnitManager.Instance.GetFriendlyUnitList())
        {
            friendlyUnit.HealToFull();
        }
    }

    private void TriggerVictory()
    {
        ShowOutcome(VICTORY_TEXT);
    }

    private void TriggerLoss()
    {
        ShowOutcome(LOSS_TEXT);
    }

    private void ShowOutcome(string message)
    {
        if (gameEnded)
        {
            return;
        }
        gameEnded = true;

        if (outcomeText != null)
        {
            outcomeText.text = message;
        }

        if (outcomePanel != null)
        {
            outcomePanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
