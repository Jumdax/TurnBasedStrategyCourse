using System;
using UnityEngine;

/// <summary>
/// Presentation-only visual hook for chest opening. Subscribes to the
/// existing ChestState.OnChestOpened event (Package A) and rotates the
/// chest's existing lid mesh open. Does not touch ChestState.cs itself or
/// any gameplay/action logic.
/// </summary>
[RequireComponent(typeof(ChestState))]
public class ChestLidVisual : MonoBehaviour
{
    [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(-100f, 0f, 0f);

    private ChestState chestState;
    private Transform lidTransform;

    private void Awake()
    {
        chestState = GetComponent<ChestState>();
        // Matches the child name already present on the existing
        // SM_Gen_Prop_Chest_01 mesh hierarchy - no new asset required.
        lidTransform = transform.Find("SM_Gen_Prop_Chest_01_Lid_01");
    }

    private void OnEnable()
    {
        chestState.OnChestOpened += ChestState_OnChestOpened;
    }

    private void OnDisable()
    {
        chestState.OnChestOpened -= ChestState_OnChestOpened;
    }

    private void ChestState_OnChestOpened(object sender, EventArgs e)
    {
        if (lidTransform != null)
        {
            lidTransform.localRotation = Quaternion.Euler(openLocalEulerAngles);
        }
    }
}
