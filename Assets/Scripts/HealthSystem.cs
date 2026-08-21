using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{

    public event EventHandler OnDead;
    public event EventHandler OnDamaged;
    [SerializeField] private int health = 100;
    private int healthMax;

    private void Awake()
    {
        healthMax = health;
    }
    public void Damage(int damageAmount)
    {
        health -= damageAmount;

        if (health < 0)
        {
            health = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);
        
        if (health == 0)
        {
            Die();
        }
        Debug.Log(health);
    }

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }

    /// <summary>Restores this unit to full health. No-op if already dead (health <= 0) -
    /// callers are expected to only heal currently-living units (e.g. via
    /// UnitManager.GetFriendlyUnitList(), which already excludes dead/destroyed units),
    /// but this guard keeps the method safe even if called directly.</summary>
    public void HealToFull()
    {
        if (health <= 0)
        {
            return;
        }

        health = healthMax;
        OnDamaged?.Invoke(this, EventArgs.Empty);
    }
}
