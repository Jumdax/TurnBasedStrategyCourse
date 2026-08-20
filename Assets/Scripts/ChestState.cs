using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestState : MonoBehaviour
{
    private static readonly List<ChestState> allChestStates = new List<ChestState>();

    public event EventHandler OnChestOpened;

    [SerializeField] private bool isOpen = false;

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

    public bool TryOpen()
    {
        if (isOpen)
        {
            return false;
        }

        isOpen = true;
        OnChestOpened?.Invoke(this, EventArgs.Empty);
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
