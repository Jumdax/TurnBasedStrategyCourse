using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform smiteFireVfxPrefab;

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;  
            moveAction.OnStopMoving += MoveAction_OnStopMoving; 
        }
        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
        }
        if (TryGetComponent<MeleeAttackAction>(out MeleeAttackAction meleeAttackAction))
        {
            meleeAttackAction.OnMeleeAttack += MeleeAttackAction_OnMeleeAttack;
        }
        if (TryGetComponent<SmiteAction>(out SmiteAction smiteAction))
        {
            smiteAction.OnSmite += SmiteAction_OnSmite;
        }
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", true);
    }
    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", false);
    }
    private void MeleeAttackAction_OnMeleeAttack(object sender, EventArgs e)
    {
        animator.SetTrigger("Melee");
    }
    private void SmiteAction_OnSmite(object sender, SmiteAction.OnSmiteEventArgs e)
    {
        Vector3 targetPosition = e.targetUnit.GetWorldPosition();

        Transform smiteFireVfxInstance = Instantiate(smiteFireVfxPrefab, targetPosition, Quaternion.identity);

        Destroy(smiteFireVfxInstance.gameObject, 1.5f);
    }
    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("Shoot");

        Transform bulletProjectileTransform = 
            Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);

        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();

        targetUnitShootAtPosition.y = shootPointTransform.position.y;

        bulletProjectile.Setup(targetUnitShootAtPosition);
    }
}
