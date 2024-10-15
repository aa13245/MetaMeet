using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionMaster : MonoBehaviour
{
    public static DirectionMaster instance;
    public ObjectsDatabaseSO database;

    int CurrentDirectionIndex = 1; // 처음 down

    // up down left right
    public static Vector3[] directionRot = new Vector3[4]
    {
        new Vector3 (0, 180, 0),
        new Vector3(0, 0, 0),
        new Vector3(0, 90, 0),
        new Vector3(0, 270, 0),
    };

    List<Vector3Int> directionPosition = new();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        directionPosition.Clear(); // 초기화
        directionPosition.Add(new Vector3Int(0, 0, 0));
        directionPosition.Add(new Vector3Int(0, 0, 0));
        directionPosition.Add(new Vector3Int(0, 0, 0));
        directionPosition.Add(new Vector3Int(0, 0, 0));
    }

    public void SetDirectionIndex(int directionIndex)
    {
        CurrentDirectionIndex = directionIndex;
        GetDirectionIndex();
    }

    public void SettingDirectionIndexById(int id)
    {
        directionPosition.Clear();
        directionPosition.Add(new Vector3Int((int)database.objectsData[id].Size.x, 0, database.objectsData[id].Size.y));
        directionPosition.Add(new Vector3Int(0, 0, 0));
        directionPosition.Add(new Vector3Int(0, 0, database.objectsData[id].Size.x));
        directionPosition.Add(new Vector3Int(database.objectsData[id].Size.y, 0, 0));
        GetDirectionPositions();
    }

    public Vector3 GetDirectionRotation(int directionIndex)
    {
        return directionRot[directionIndex];
    }

    public int GetDirectionIndex()
    {
        //Debug.Log("GetDirectionIndex() = " + CurrentDirectionIndex);
        return CurrentDirectionIndex;
    }

    public Vector3Int GetDirectionPosition(int directionIndex)
    {

        //Debug.Log("GetDirectionPosition() = " + directionPosition[directionIndex]);
        return directionPosition[directionIndex];
    }
    
    public List<Vector3Int> GetDirectionPositions()
    {
        foreach (var direction in directionPosition)
        {
            //Debug.Log("GetDirectionPositions() = " + direction);
        }

        return directionPosition;
    }
}
