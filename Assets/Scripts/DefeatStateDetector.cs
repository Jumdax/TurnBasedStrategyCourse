using System;
using UnityEngine;

/// <summary>
/// Sprint 1, Package 1 (Gameplay: Defeat-State Detection).
///
/// Additive, read-only observer built entirely on the existing architecture:
///   - Unit.OnAnyUnitDead (static event on Unit.cs)
///   - UnitManager.Instance.GetEnemyUnitList() / GetFriendlyUnitList()
///
/// It detects the two defeat-state conditions the vertical slice needs:
///   - All enemy units are dead      -> OnEnemiesDefeated / EnemiesDefeated
///   - All friendly/player units are dead -> OnPartyDefeated / PartyDefeated
///
/// This script does NOT modify Unit.cs, UnitManager.cs, HealthSystem.cs, or any
/// other existing gameplay file, scene, or prefab. It self-bootstraps at
/// runtime (see Bootstrap() below) specifically so it can be verified in Play
/// Mode without requiring any edit to GameScene.unity or any prefab, both of
/// which are outside this package's file ownership.
///
/// Explicitly out of scope for this script (per Sprint 1 Package 1): any
/// chest/objective/win-trigger logic, any UI, and any placeholder
/// "TriggerObjectiveReached()"-style API. Win-condition wiring (e.g. reaching
/// a chest) and on-screen Win/Loss presentation are later packages that will
/// consume the events/properties exposed here.
/// </summary>
public class DefeatStateDetector : MonoBehaviour
{
    public static DefeatStateDetector Instance { get; private set; }

    /// <summary>Fired exactly once, the moment the enemy unit list first reaches zero.</summary>
    public static event EventHandler OnEnemiesDefeated;

    /// <summary>Fired exactly once, the moment the friendly/player unit list first reaches zero.</summary>
    public static event EventHandler OnPartyDefeated;

    public bool EnemiesDefeated { get; private set; }
    public bool PartyDefeated { get; private set; }

    /// <summary>
    /// Creates a DefeatStateDetector automatically once the first scene has
    /// loaded, so this package's detection logic is active in Play Mode
    /// without any scene or prefab edit (both outside this package's file
    /// ownership). Safe to call more than once; it is a no-op if an instance
    /// already exists (e.g. one was added to the scene manually later).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null || FindFirstObjectByType<DefeatStateDetector>() != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(DefeatStateDetector));
        go.AddComponent<DefeatStateDetector>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one DefeatStateDetector! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void OnDisable()
    {
        Unit.OnAnyUnitDead -= Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        if (EnemiesDefeated && PartyDefeated)
        {
            // Both terminal states already latched; nothing further to detect.
            return;
        }

        if (UnitManager.Instance == null)
        {
            // Defensive only: UnitManager.Instance is set in its own Awake(),
            // long before any Unit can take damage and die, so this should be
            // unreachable in normal play.
            return;
        }

        Unit deadUnit = sender as Unit;

        int enemyCount = UnitManager.Instance.GetEnemyUnitList().Count;
        int friendlyCount = UnitManager.Instance.GetFriendlyUnitList().Count;

        // Defensive against MonoBehaviour subscription/execution order:
        // Unit.cs invokes OnAnyUnitDead (from HealthSystem_OnDead) AFTER
        // calling Destroy(gameObject), and UnitManager subscribes its own
        // OnAnyUnitDead handler (which removes the dead unit from its lists)
        // independently, in its own Start(). Unity does not guarantee that
        // UnitManager's handler runs before this one for the same event
        // invocation. If this handler happens to run first, the dead unit
        // may still be present in the list just read above — detect and
        // correct for that explicitly rather than depending on subscription
        // order between two independent scripts.
        if (deadUnit != null)
        {
            if (deadUnit.IsEnemy() && UnitManager.Instance.GetEnemyUnitList().Contains(deadUnit))
            {
                enemyCount--;
            }
            else if (!deadUnit.IsEnemy() && UnitManager.Instance.GetFriendlyUnitList().Contains(deadUnit))
            {
                friendlyCount--;
            }
        }

        if (!EnemiesDefeated && enemyCount <= 0)
        {
            EnemiesDefeated = true;
            Debug.Log("DefeatStateDetector: all enemy units are dead (OnEnemiesDefeated).");
            OnEnemiesDefeated?.Invoke(this, EventArgs.Empty);
        }

        if (!PartyDefeated && friendlyCount <= 0)
        {
            PartyDefeated = true;
            Debug.Log("DefeatStateDetector: all friendly units are dead (OnPartyDefeated).");
            OnPartyDefeated?.Invoke(this, EventArgs.Empty);
        }
    }
}
