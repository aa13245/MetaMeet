using Postgrest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class GridData 
{
    public Dictionary<Vector3Int, PlacementData> PlacementDataDictionary = new Dictionary<Vector3Int, PlacementData>();
    public List<Vector3Int> KeyList = new ();

    public void LoadCheckData(List<LoadData> dataes)
    {
        PlacementData data;
        int index = 0;
        int objectIndex = 0;
        for (int i = 0; i < dataes.Count; i++)
        {
            if (index != dataes[i].index)
            {
                //List<Vector3Int> positionToOccupy = dataes[i].
            }

        }
    }

    public List<LoadData> SaveCheckData()
    {
        List<LoadData> data = new List<LoadData>();

        int index = 0;
        int objectIndex = 0;
        foreach (var placementData in PlacementDataDictionary)
        {
            if (objectIndex != placementData.Value.PlacedObjectIndex)
            {
                objectIndex++;
            }
            for (int i = 0; i < placementData.Value.occupiedPositions.Count; i++)
            {
                data[i].roomId = 0;
                

                Debug.Log($"index = {index},placementData = {objectIndex},placementData = {placementData.Key}, id = {placementData.Value.ID}, PlacedObjectIndex = {placementData.Value.PlacedObjectIndex}, id = {placementData.Value.occupiedPositions[i]}");
            }
            index++;
        }   

        return data;
    }

    // 데이터 저장
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int iD, int placedObjectIndex, int driectionIndex, Vector2Int dynamicObjectSize) // 현재 생성 포지션, 오브젝트의 사이즈, 오브젝트의 아이디, 생성 리스트의 아이디 
    {
        // 우선 계산해서 사이즈 만큼에 백터 리스트를 구함
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize, driectionIndex, dynamicObjectSize);

        PlacementData data = new PlacementData(positionToOccupy, iD, placedObjectIndex);

        // 저장시 예외 처리
        foreach (var pos in positionToOccupy)
        {

            if (PlacementDataDictionary.ContainsKey(pos))
            {
                throw new Exception("중복 입니다.");
            }

            PlacementDataDictionary[pos] = data;
            KeyList.Add(pos);
        }
    }

    // 오브젝트에 전체 포함된 그리드 좌표 리스트르 구함
    public List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize, int driectionIndex, Vector2Int dynamicObjectSize)
    {
        List<Vector3Int> returnVal = new List<Vector3Int>();

        if (dynamicObjectSize.x != dynamicObjectSize.y && (driectionIndex == 1 || driectionIndex == 3))
        {
            gridPosition = new Vector3Int(gridPosition.x, 0, gridPosition.z - (dynamicObjectSize.x + (dynamicObjectSize.y - 2) - ((dynamicObjectSize.x - 1) * 2)));
        }

        objectSize = dynamicObjectSize;

        // 현재점을 기준으로 데이터에 넣음
        // 사이즈를 포함하여 전체의 그리드 리스트를 구함
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));

                //Debug.Log($"CalculatePositions returnVal ({x},{y}) = ({returnVal[x].x}, {returnVal[x].z})");
            }
        }



        return returnVal;
    }



    // 배치가 가능한지 판단
    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int size, int driectionIndex, Vector2Int dynamicObjectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, size, driectionIndex, dynamicObjectSize);

        // 배치시 가능 여부
        foreach (var pos in positionToOccupy)
        {
            if (PlacementDataDictionary.ContainsKey(pos))
            {
                Debug.Log($"중복된 포지션 입니다 {pos}");
                return false;
            }
        }
        return true;
    }

    public int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (PlacementDataDictionary.ContainsKey(gridPosition) == false)
            return -1;
        return PlacementDataDictionary[gridPosition].PlacedObjectIndex;
    }

    public void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach (var pos in PlacementDataDictionary[gridPosition].occupiedPositions)
        {
            PlacementDataDictionary.Remove(pos);
        }
    }
}

public class LoadData
{
    public int roomId;
    public int index;
    public int objectIndex;
    public Vector3Int keyPos;
    public Vector3Int occupiedPositions;
    public int ID;
    public int PlacedObjectIndex;
}

// 저장 데이터 객체
public class PlacementData
{
    public List<Vector3Int> occupiedPositions; // 포지션

    public int ID {  get; private set; }

    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }

}
