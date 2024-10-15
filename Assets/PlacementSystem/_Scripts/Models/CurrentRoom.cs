using Postgrest.Attributes;
using Postgrest.Models;
using System;

[Table("CurrentRoom")]
public class CurrentRoom : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("roomid")]
    public int roomid { get; set; }

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
