using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

    [SerializeField] private Unit unit;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {

            
            // --- ScreenShake Test ---
            // ScreenShake.Instance.Shake(5f);

            
            // --- Pathfinding Test Start    
            // GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
            // GridPosition startGridPosition = new GridPosition(0, 0);

            // List<GridPosition> gridPositionList = Pathfinding.Instance.FindPath(startGridPosition, mouseGridPosition, out int pathLength);

            // for (int i = 0; i < gridPositionList.Count - 1; i++)
            // {
            //     Debug.DrawLine(
            //         LevelGrid.Instance.GetWorldPosition(gridPositionList[i]),
            //         LevelGrid.Instance.GetWorldPosition(gridPositionList[i + 1]),
            //         Color.white,
            //         10f
            //     );
            // }
            // --- Pathfinding Test End
        }
    }

}