using TFS.entities;

namespace TFS.facility;

public interface ICreator <out T> where T:Entity
{
    public T CreateEntity(object information);

}