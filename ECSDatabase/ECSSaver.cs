using ECS.Components;
using ECS.Entities;

namespace ECSDatabase;

public class ECSSaver
{
    public void Save(Entity entity)
    {
        foreach (Component component in entity.GetComponents())
        {
            
        }
    }
}