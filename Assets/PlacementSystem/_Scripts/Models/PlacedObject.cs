using Postgrest.Attributes;
using Postgrest.Models;
using System;

[Table("PlacedObject")]
public class PlacedObject : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("room_id")]
    public int roomid { get; set; }

    [Column("ob_id")]
    public int obId { get; set; }

    [Column("x")]
    public float x { get; set; }

    [Column("y")]
    public float y { get; set; }


    [Column("z")]
    public float z { get; set; }

    [Column("rot_y")]
    public float rotY { get; set; }

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
