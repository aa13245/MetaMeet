using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem2 : MonoBehaviour
{
    [SerializeField] GameObject mouseIndicator, cellIndicator;
    [SerializeField] InputManager inputManager;
    [SerializeField] private Grid grid;

    private void Update()
    {

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.WorldToCell(mousePosition);
    }
}
