using System;
using System.Collections.Generic;
using UnityEngine;

public class SmiteAction : BaseAction
{
    public static event EventHandler<OnSmiteEventArgs> OnAnySmite;
    public event EventHandler<OnSmiteEventArgs> OnSmite;

    public class OnSmiteEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit smitingUnit;
    }

    private enum State
    {
        Aiming,
        Smiting,
        Cooloff,
    }

    [SerializeField] private LayerMask obstaclesLayerMask;
    private State state;
    private int maxSmiteDistance = 7;
    private int damageAmount = 40;
    private float stateTimer;
    private Unit targetUnit;
    private bool canApplySmite;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aiming:
                // Guard against a target destroyed mid-state (same lesson as the
                // MeleeAttackAction MissingReferenceException fix) even though
                // Smite's damage isn't applied until the later Smiting state.
                if (targetUnit != null)
                {
                    Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                    float rotateSpeed = 10f;
                    transform.forward = Vector3.Lerp(transform.forward, aimDir, rotateSpeed * Time.deltaTime);
                }
                break;
            case State.Smiting:
                if (canApplySmite)
                {
                    Smite();
                    canApplySmite = false;
                }
                break;
            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Smiting;
                float smitingStateTime = 0.1f;
                stateTimer = smitingStateTime;
                break;
            case State.Smiting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    private void Smite()
    {
        OnAnySmite?.Invoke(this, new OnSmiteEventArgs
        {
            targetUnit = targetUnit,
            smitingUnit = unit
        });

        OnSmite?.Invoke(this, new OnSmiteEventArgs
        {
            targetUnit = targetUnit,
            smitingUnit = unit
        });

        targetUnit.Damage(damageAmount);
    }

    public override string GetActionName()
    {
        return "Smite";
    }

    public override int GetActionPointCost()
    {
        // Locked Sprint 2 baseline value.
        return 1;
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxSmiteDistance; x <= maxSmiteDistance; x++)
        {
            for (int z = -maxSmiteDistance; z <= maxSmiteDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    //Invalid Grid Position
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxSmiteDistance)
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    //Grid Position is Empty, no Unit
                    continue;
                }

                Unit targetUnitAtPosition = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                if (targetUnitAtPosition.IsEnemy() == unit.IsEnemy())
                {
                    // Both Units on same 'Team'
                    continue;
                }

                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 smiteDir = (targetUnitAtPosition.GetWorldPosition() - unitWorldPosition).normalized;

                float unitShoulderHeight = 1.7f;

                if (Physics.Raycast(
                    unitWorldPosition + Vector3.up * unitShoulderHeight,
                    smiteDir,
                    Vector3.Distance(unitWorldPosition, targetUnitAtPosition.GetWorldPosition()),
                    obstaclesLayerMask))
                {
                    // Blocked by an obstacle - same line-of-sight rule ShootAction already uses.
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;

        canApplySmite = true;
        ActionStart(onActionComplete);
    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public int GetMaxSmiteDistance()
    {
        return maxSmiteDistance;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnitAtPosition = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1 - targetUnitAtPosition.GetHealthNormalized()) * 100f),
        };
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }
}
