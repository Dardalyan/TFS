namespace TFS.entities;

public abstract class Entity
{
    protected string ID{ get; set; }

    public Entity(string id)
    {
        ID = id;
    }

    public string GetID()
    {
        return ID;
    }

}