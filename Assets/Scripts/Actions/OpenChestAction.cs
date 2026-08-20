using System;
using System.Collections.Generic;
using UnityEngine;

public class OpenChestAction : BaseAction
{
    private int maxOpenDistance = 1;
    private float stateTimer;
    private ChestState targetChestState;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            ActionComplete();
        }
    }

    public override string GetActionName()
    {
        return "Open Chest";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        // 8-directional adjacency, same shape as MeleeAttackAction's targeting.
        for (int x = -maxOpenDistance; x <= maxOpenDistance; x++)
        {
            for (int z = -maxOpenDistance; z <= maxOpenDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (offsetGridPosition.x == 0 && offsetGridPosition.z == 0)
                {
                    // Current Unit is standing here
                    continue;
                }

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    //Invalid Grid Position
                    continue;
                }

                ChestState chestState = ChestState.GetChestStateAtGridPosition(testGridPosition);
                if (chestState == null || chestState.IsOpen())
                {
                    // No chest here, or it's already open - not a valid target either way.
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetChestState = ChestState.GetChestStateAtGridPosition(gridPosition);

        // TryOpen() is idempotent-safe on its own (returns false if already open),
        // so this stays correct even if TakeAction were ever invoked outside the
        // GetValidActionGridPositionList()-filtered flow.
        targetChestState?.TryOpen();

        float openStateTime = 0.5f;
        stateTimer = openStateTime;

        ActionStart(onActionComplete);
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        // Player-only action; no enemy unit ever carries OpenChestAction.
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }
}
