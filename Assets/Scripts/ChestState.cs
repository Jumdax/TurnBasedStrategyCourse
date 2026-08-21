using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestState : MonoBehaviour
{
    private static readonly List<ChestState> allChestStates = new List<ChestState>();

    public event EventHandler OnChestOpened;

    /// <summary>Fires for any chest, alongside the per-instance OnChestOpened, so a single
    /// game-state listener can observe every chest without being wired to each one individually
    /// in the Inspector. Mirrors the existing Unit.OnAnyUnitDead static-event pattern.</summary>
    public static event EventHandler<ChestState> OnAnyChestOpened;

    [SerializeField] private bool isOpen = false;

    [Tooltip("If true, successfully opening this chest triggers the Victory state (after party healing).")]
    [SerializeField] private bool isVictoryChest = false;

    private void Awake()
    {
        allChestStates.Add(this);
    }

    private void OnDestroy()
    {
        allChestStates.Remove(this);
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public bool IsVictoryChest()
    {
        return isVictoryChest;
    }

    public bool TryOpen()
    {
        if (isOpen)
        {
            return false;
        }

        isOpen = true;
        OnChestOpened?.Invoke(this, EventArgs.Empty);
        OnAnyChestOpened?.Invoke(this, this);
        return true;
    }

    public GridPosition GetGridPosition()
    {
        return LevelGrid.Instance.GetGridPosition(transform.position);
    }

    public static ChestState GetChestStateAtGridPosition(GridPosition gridPosition)
    {
        foreach (ChestState chestState in allChestStates)
        {
            if (chestState.GetGridPosition() == gridPosition)
            {
                return chestState;
            }
        }
        return null;
    }
}
