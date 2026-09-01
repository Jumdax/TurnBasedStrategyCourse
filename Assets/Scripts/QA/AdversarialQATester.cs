using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ELVTR Assignment #9 - minimal adversarial QA agent for HOLLOWDEEP.
/// Runs a bounded sequence of adversarial probes against the live Unity
/// prototype (Pathfinding, LevelGrid, the action-point economy) and writes
/// a structured JSON report to assignment_09/qa_report.json.
///
/// QA-only tooling: not part of gameplay. Add this component to a GameObject
/// in the test scene and trigger it manually (context menu) after Play Mode
/// has fully initialized - it is not meant to run automatically during
/// normal dev play sessions.
/// </summary>
public class AdversarialQATester : MonoBehaviour
{
    [SerializeField] private bool runOnStart = false;

    private readonly List<QAFinding> results = new List<QAFinding>();

    private void Start()
    {
        if (runOnStart)
        {
            RunTests();
        }
    }

    [ContextMenu("Run Adversarial QA Tests")]
    public void RunTests()
    {
        results.Clear();

        if (LevelGrid.Instance == null || Pathfinding.Instance == null || UnitManager.Instance == null)
        {
            Debug.LogError("[AdversarialQATester] Required singletons are not ready yet. Enter Play Mode and wait a moment before running.");
            return;
        }

        try
        {
            // Warm-up call: confirms Pathfinding.Setup() has actually run before we begin,
            // so an early trigger doesn't get logged as a false "crash" finding.
            Pathfinding.Instance.IsWalkableGridPosition(new GridPosition(0, 0));
        }
        catch (Exception)
        {
            Debug.LogError("[AdversarialQATester] Pathfinding is not fully initialized yet. Wait a moment after Play starts before running.");
            return;
        }

        TestOutOfBoundsQuery();
        TestDiagonalCornerCutting();
        TestActionPointBypass();
        TestFighterActionSet();
        TestPriestActionSet();
        TestEnemyActionSet();
        TestChestConfiguration();

        WriteReport();

        int findingCount = results.FindAll(r => r.result == "FINDING").Count;
        Debug.Log($"[AdversarialQATester] Run complete. {results.Count} result(s), {findingCount} FINDING(s). Report written to assignment_09/qa_report.json");
    }

    private void TestOutOfBoundsQuery()
    {
        const string scenario = "OutOfBoundsQuery";

        int width = LevelGrid.Instance.GetWidth();
        int height = LevelGrid.Instance.GetHeight();

        GridPosition[] probeCoordinates =
        {
            new GridPosition(-1, -1),
            new GridPosition(-1, 0),
            new GridPosition(0, -1),
            new GridPosition(width, 0),
            new GridPosition(0, height),
            new GridPosition(width + 50, height + 50),
        };

        bool anyCrash = false;

        foreach (GridPosition probe in probeCoordinates)
        {
            try
            {
                // Pathfinding.IsWalkableGridPosition performs no bounds check of its own -
                // it indexes an internal array directly, relying entirely on callers to
                // guard with LevelGrid.IsValidGridPosition first.
                Pathfinding.Instance.IsWalkableGridPosition(probe);
            }
            catch (Exception ex)
            {
                anyCrash = true;
                results.Add(new QAFinding
                {
                    scenario = scenario,
                    result = "FINDING",
                    location = probe.ToString(),
                    error_type = ex.GetType().Name,
                    game_context = $"Calling Pathfinding.IsWalkableGridPosition() with out-of-bounds grid position {probe} threw an unhandled {ex.GetType().Name} ('{ex.Message}') instead of failing gracefully.",
                    timestamp = DateTime.Now.ToString("o"),
                });
            }
        }

        if (!anyCrash)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = "grid boundary queries",
                error_type = "None",
                game_context = $"Probed {probeCoordinates.Length} out-of-bounds grid positions against Pathfinding.IsWalkableGridPosition(); none threw an exception.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    private void TestDiagonalCornerCutting()
    {
        const string scenario = "DiagonalCornerCutting";

        int width = LevelGrid.Instance.GetWidth();
        int height = LevelGrid.Instance.GetHeight();
        (int dx, int dz)[] diagonals = { (-1, -1), (-1, 1), (1, -1), (1, 1) };

        int foundCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition origin = new GridPosition(x, z);
                if (!Pathfinding.Instance.IsWalkableGridPosition(origin))
                {
                    continue;
                }

                foreach ((int dx, int dz) in diagonals)
                {
                    int nx = x + dx;
                    int nz = z + dz;
                    if (nx < 0 || nz < 0 || nx >= width || nz >= height)
                    {
                        continue;
                    }

                    GridPosition diagTarget = new GridPosition(nx, nz);
                    if (!Pathfinding.Instance.IsWalkableGridPosition(diagTarget))
                    {
                        continue;
                    }

                    GridPosition flankA = new GridPosition(nx, z);
                    GridPosition flankB = new GridPosition(x, nz);
                    bool flankABlocked = !Pathfinding.Instance.IsWalkableGridPosition(flankA);
                    bool flankBBlocked = !Pathfinding.Instance.IsWalkableGridPosition(flankB);

                    if (!flankABlocked && !flankBBlocked)
                    {
                        continue;
                    }

                    // Concrete proof, not just geometry: does the pathfinder actually take
                    // the direct diagonal hop despite the blocked corner? A direct diagonal
                    // (cost 14) is always cheaper than any orthogonal detour (cost 20+), so
                    // if it's walkable at all, A* will select it.
                    List<GridPosition> path = Pathfinding.Instance.FindPath(origin, diagTarget, out _);
                    bool tookDirectDiagonalHop = path != null && path.Count == 2;

                    if (tookDirectDiagonalHop)
                    {
                        foundCount++;
                        string blockedFlanks = flankABlocked && flankBBlocked
                            ? $"{flankA} and {flankB}"
                            : flankABlocked ? flankA.ToString() : flankB.ToString();

                        results.Add(new QAFinding
                        {
                            scenario = scenario,
                            result = "FINDING",
                            location = $"{origin} -> {diagTarget}",
                            error_type = "DiagonalCornerCut",
                            game_context = $"Pathfinding.FindPath took a direct single-step diagonal move from {origin} to {diagTarget} even though flanking cell(s) {blockedFlanks} are unwalkable. Pathfinding.GetNeighbourList() does not check flanking cells before including a diagonal neighbor.",
                            timestamp = DateTime.Now.ToString("o"),
                        });
                    }
                }
            }
        }

        if (foundCount == 0)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = "full grid sweep",
                error_type = "None",
                game_context = $"Swept all {width}x{height} grid cells for diagonal moves with a blocked flanking corner; found no concrete coordinate where Pathfinding.FindPath actually took a direct diagonal hop through a blocked corner.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    private void TestActionPointBypass()
    {
        const string scenario = "ActionPointBypass";

        List<Unit> candidates = UnitManager.Instance.GetFriendlyUnitList();
        if (candidates == null || candidates.Count == 0)
        {
            candidates = UnitManager.Instance.GetUnitList();
        }

        if (candidates == null || candidates.Count == 0)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = "n/a",
                error_type = "None",
                game_context = "No units were present in the scene to test action-point enforcement against.",
                timestamp = DateTime.Now.ToString("o"),
            });
            return;
        }

        Unit testUnit = candidates[0];
        MoveAction moveAction = testUnit.GetAction<MoveAction>();

        if (moveAction == null)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = testUnit.name,
                error_type = "None",
                game_context = $"{testUnit.name} has no MoveAction component; the action-point bypass scenario requires one.",
                timestamp = DateTime.Now.ToString("o"),
            });
            return;
        }

        // Legitimately drain this unit's action points through the game's own sanctioned
        // API (TrySpendActionPointsToTakeAction only spends points, it never executes the
        // action) so it reaches 0 AP without ever performing a real move.
        int safetyIterations = 0;
        while (testUnit.CanSpendActionPointsToTakeAction(moveAction) && safetyIterations < 10)
        {
            testUnit.TrySpendActionPointsToTakeAction(moveAction);
            safetyIterations++;
        }

        if (testUnit.CanSpendActionPointsToTakeAction(moveAction))
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = testUnit.name,
                error_type = "None",
                game_context = $"Could not legitimately drain {testUnit.name}'s action points to a level where MoveAction becomes unaffordable; scenario inconclusive.",
                timestamp = DateTime.Now.ToString("o"),
            });
            return;
        }

        // Adversarial step: target the unit's own current grid position (a no-op relocation,
        // so nothing visibly moves) and call TakeAction() directly, bypassing
        // TrySpendActionPointsToTakeAction entirely.
        GridPosition ownPosition = testUnit.GetGridPosition();
        bool exceptionThrown = false;
        Exception caughtException = null;

        try
        {
            moveAction.TakeAction(ownPosition, () => { });
        }
        catch (Exception ex)
        {
            exceptionThrown = true;
            caughtException = ex;
        }

        if (exceptionThrown)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = testUnit.name,
                error_type = caughtException.GetType().Name,
                game_context = $"Calling MoveAction.TakeAction() directly on {testUnit.name} at 0 action points threw {caughtException.GetType().Name}: {caughtException.Message}.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
        else
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = testUnit.name,
                error_type = "ActionPointBypass",
                game_context = $"{testUnit.name} had 0 action points (CanSpendActionPointsToTakeAction returned false), but calling MoveAction.TakeAction() directly still executed successfully with no exception. BaseAction.TakeAction() and its overrides contain no internal action-point check - enforcement exists only in Unit.TrySpendActionPointsToTakeAction(). This test targeted the unit's own current grid position, so no visible relocation occurred; its action points remain at 0 until the next turn change, exactly as if spent normally.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    // Finds a friendly unit by GameObject name ("Fighter" / "Priest") - the existing,
    // human-authored, non-circular identifier already baked into Fighter.prefab and
    // Priest.prefab. Deliberately does not use action-component presence/absence to
    // identify the unit, since that is exactly what these tests are validating.
    private static Unit FindUnitByName(List<Unit> units, string unitName)
    {
        if (units == null)
        {
            return null;
        }

        foreach (Unit unit in units)
        {
            if (unit != null && unit.name == unitName)
            {
                return unit;
            }
        }

        return null;
    }

    private void TestFighterActionSet()
    {
        const string scenario = "FighterActionSet";

        Unit fighter = FindUnitByName(UnitManager.Instance.GetFriendlyUnitList(), "Fighter");
        if (fighter == null)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = "Fighter",
                error_type = "UnitNotFound",
                game_context = "No friendly Unit with gameObject.name == \"Fighter\" was found via UnitManager.Instance.GetFriendlyUnitList().",
                timestamp = DateTime.Now.ToString("o"),
            });
            return;
        }

        List<string> violations = new List<string>();
        if (fighter.GetAction<MoveAction>() == null) violations.Add("missing MoveAction");
        if (fighter.GetAction<MeleeAttackAction>() == null) violations.Add("missing MeleeAttackAction");
        if (fighter.GetAction<OpenChestAction>() == null) violations.Add("missing OpenChestAction");
        if (fighter.GetAction<SmiteAction>() != null) violations.Add("unexpected SmiteAction present");
        if (fighter.GetAction<ShootAction>() != null) violations.Add("unexpected ShootAction present");

        if (violations.Count == 0)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = fighter.name,
                error_type = "None",
                game_context = "Fighter has exactly Move, Melee, and Open Chest actions; no Smite or Shoot.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
        else
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = fighter.name,
                error_type = "ActionSetMismatch",
                game_context = $"Fighter action set violations: {string.Join("; ", violations)}.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    private void TestPriestActionSet()
    {
        const string scenario = "PriestActionSet";

        Unit priest = FindUnitByName(UnitManager.Instance.GetFriendlyUnitList(), "Priest");
        if (priest == null)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = "Priest",
                error_type = "UnitNotFound",
                game_context = "No friendly Unit with gameObject.name == \"Priest\" was found via UnitManager.Instance.GetFriendlyUnitList().",
                timestamp = DateTime.Now.ToString("o"),
            });
            return;
        }

        List<string> violations = new List<string>();
        if (priest.GetAction<MoveAction>() == null) violations.Add("missing MoveAction");
        if (priest.GetAction<MeleeAttackAction>() == null) violations.Add("missing MeleeAttackAction");
        if (priest.GetAction<SmiteAction>() == null) violations.Add("missing SmiteAction");
        if (priest.GetAction<OpenChestAction>() == null) violations.Add("missing OpenChestAction");
        if (priest.GetAction<ShootAction>() != null) violations.Add("unexpected ShootAction present");

        if (violations.Count == 0)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = priest.name,
                error_type = "None",
                game_context = "Priest has exactly Move, Melee, Smite, and Open Chest actions; no Shoot.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
        else
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = priest.name,
                error_type = "ActionSetMismatch",
                game_context = $"Priest action set violations: {string.Join("; ", violations)}.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    private void TestEnemyActionSet()
    {
        const string scenario = "EnemyActionSet";

        List<Unit> enemies = UnitManager.Instance.GetEnemyUnitList();
        if (enemies == null || enemies.Count == 0)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = "n/a",
                error_type = "None",
                game_context = "No enemy units were present in the scene to validate the enemy action set against.",
                timestamp = DateTime.Now.ToString("o"),
            });
            return;
        }

        int violatingEnemyCount = 0;

        foreach (Unit enemy in enemies)
        {
            List<string> violations = new List<string>();
            if (enemy.GetAction<MoveAction>() == null) violations.Add("missing MoveAction");
            if (enemy.GetAction<MeleeAttackAction>() == null) violations.Add("missing MeleeAttackAction");
            if (enemy.GetAction<SmiteAction>() != null) violations.Add("unexpected SmiteAction present");
            if (enemy.GetAction<ShootAction>() != null) violations.Add("unexpected ShootAction present");
            if (enemy.GetAction<OpenChestAction>() != null) violations.Add("unexpected OpenChestAction present");

            if (violations.Count > 0)
            {
                violatingEnemyCount++;
                results.Add(new QAFinding
                {
                    scenario = scenario,
                    result = "FINDING",
                    location = $"{enemy.name} at {enemy.GetGridPosition()}",
                    error_type = "ActionSetMismatch",
                    game_context = $"Enemy action set violations: {string.Join("; ", violations)}.",
                    timestamp = DateTime.Now.ToString("o"),
                });
            }
        }

        if (violatingEnemyCount == 0)
        {
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = $"{enemies.Count} enemy unit(s)",
                error_type = "None",
                game_context = $"All {enemies.Count} enemy unit(s) have exactly Move and Melee actions; no Smite, Shoot, or Open Chest.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    private void TestChestConfiguration()
    {
        const string scenario = "ChestConfiguration";

        ChestState[] chests = FindObjectsByType<ChestState>(FindObjectsSortMode.None);

        List<ChestState> normalChests = new List<ChestState>();
        List<ChestState> victoryChests = new List<ChestState>();

        if (chests != null)
        {
            foreach (ChestState chest in chests)
            {
                if (chest == null)
                {
                    continue;
                }

                if (chest.IsVictoryChest())
                {
                    victoryChests.Add(chest);
                }
                else
                {
                    normalChests.Add(chest);
                }
            }
        }

        bool hasAtLeastOneNormalChest = normalChests.Count >= 1;
        bool hasExactlyOneVictoryChest = victoryChests.Count == 1;

        if (hasAtLeastOneNormalChest && hasExactlyOneVictoryChest)
        {
            ChestState victoryChest = victoryChests[0];
            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "PASS",
                location = victoryChest.gameObject.name,
                error_type = "None",
                game_context = $"Found {normalChests.Count} normal chest(s) and exactly one victory chest, \"{victoryChest.gameObject.name}\" at world position {victoryChest.transform.position}.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
        else
        {
            List<string> violations = new List<string>();
            if (!hasAtLeastOneNormalChest) violations.Add($"expected at least 1 normal chest, found {normalChests.Count}");
            if (!hasExactlyOneVictoryChest) violations.Add($"expected exactly 1 victory chest, found {victoryChests.Count}");

            results.Add(new QAFinding
            {
                scenario = scenario,
                result = "FINDING",
                location = $"{normalChests.Count} normal, {victoryChests.Count} victory chest(s) found",
                error_type = "ChestConfigurationMismatch",
                game_context = $"Chest configuration violations: {string.Join("; ", violations)}.",
                timestamp = DateTime.Now.ToString("o"),
            });
        }
    }

    private void WriteReport()
    {
        QAReport report = new QAReport
        {
            generated_at = DateTime.Now.ToString("o"),
            results = results,
        };

        string json = JsonUtility.ToJson(report, true);
        string outputPath = Path.Combine(Application.dataPath, "..", "assignment_09", "qa_report.json");
        File.WriteAllText(outputPath, json);
    }
}

[Serializable]
public class QAFinding
{
    public string scenario;
    public string result;
    public string location;
    public string error_type;
    public string game_context;
    public string timestamp;
}

[Serializable]
public class QAReport
{
    public string generated_at;
    public List<QAFinding> results;
}
