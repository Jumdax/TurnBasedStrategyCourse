using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAction : BaseAction
{
    // Mirrors the existing ShootAction.OnShoot pattern - fired once when the
    // attack begins (not every frame) so UnitAnimator can trigger the shared
    // Melee Attack animation. No event-args payload is needed since, unlike
    // ShootAction, nothing downstream currently needs the target/attacker.
    public event EventHandler OnMeleeAttack;

    private enum State
    {
        Attacking,
        Cooloff,
    }

    private int maxMeleeDistance = 1;
    private int damageAmount = 30;
    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private bool canDealDamage;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Attacking:
                // targetUnit can be destroyed mid-state if the hit that just landed was lethal
                // (HealthSystem.Damage -> Die -> Unit.HealthSystem_OnDead -> Destroy runs
                // synchronously from within Attack() below), so guard against it on every
                // remaining frame of this state rather than assuming it survives the full
                // attackingStateTime window.
                if (targetUnit != null)
                {
                    Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                    float rotateSpeed = 10f;
                    transform.forward = Vector3.Lerp(transform.forward, aimDir, rotateSpeed * Time.deltaTime);
                }

                if (canDealDamage)
                {
                    Attack();
                    canDealDamage = false;
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
            case State.Attacking:
                state = State.Cooloff;
                float coolOffStateTime = 0.3f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    private void Attack()
    {
        targetUnit.Damage(damageAmount);
    }

    public override string GetActionName()
    {
        return "Melee";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        // 8-directional melee (including diagonals) is an intentional HOLLOWDEEP design choice.
        // Diagonal corner-cutting through walls is a known, unaddressed limitation (out of scope).
        for (int x = -maxMeleeDistance; x <= maxMeleeDistance; x++)
        {
            for (int z = -maxMeleeDistance; z <= maxMeleeDistance; z++)
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

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    //Grid Position is Empty, no Unit
                    continue;
                }

                Unit unitAtPosition = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                if (unitAtPosition.IsEnemy() == unit.IsEnemy())
                {
                    // Both Units on same 'Team'
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

        state = State.Attacking;
        float attackingStateTime = 0.2f;
        stateTimer = attackingStateTime;

        canDealDamage = true;

        OnMeleeAttack?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit unitAtPosition = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1 - unitAtPosition.GetHealthNormalized()) * 100f),
        };
    }
}
