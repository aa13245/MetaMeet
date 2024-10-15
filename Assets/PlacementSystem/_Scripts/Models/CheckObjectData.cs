using Postgrest.Attributes;
using Postgrest.Models;
using System;
using UnityEngine;

/*
public class LoadData
{
    public int roomid;
    public int index;
    public int objectIndex;
    public Vector3Int keypos;
    public Vector3Int occupiedPositions;
    public int ID;
    public int PlacedObjectIndex;
}
*/

[Table("PlacedObject")]
public class CheckObjectData : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("roomid")]
    public int roomid { get; set; }

    [Column("index")]
    public int index { get; set; }

    [Column("objectindex")]
    public int objectindex { get; set; }

    [Column("keypos")]
    public string keypos { get; set; }


    [Column("occupiedposition")]
    public string occupiedposition { get; set; }

    [Column("idid")]
    public int idid { get; set; }

    [Column("placedobjectindex")]
    public int placedobjectindex { get; set; }

    [Column("created_at")]
    public DateTime createdAt { get; set; }

    public override bool Equals(object obj)
    {
        return obj is UserData productInstance &&
                id == productInstance.id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(id);
    }
}