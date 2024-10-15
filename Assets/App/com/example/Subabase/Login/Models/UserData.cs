using Postgrest.Attributes;
using Postgrest.Models;
using System;



[Table("UserData")]
public class UserData : BaseModel
{
    [PrimaryKey("id", false)]
    public int id { get; set; }

    [Column("name")]
    public string name { get; set; }

    [Column("password")]
    public string password { get; set; }

    [Column("created_at")]
    public DateTime createdAt { get; set; }

    [Column("nick_name")]
    public string nickName { get; set; }

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