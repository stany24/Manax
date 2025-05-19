using ECS.Components;

namespace ECS.Entities;

public class Entity
{
    private readonly List<Component> _components = [];
    
    public bool HasComponent(Type type)
    {
        return _components.Any(x => x.GetType() == type);
    }

    public void RemoveComponent(Type type)
    {
        _components.RemoveAll(x => x.GetType() == type);
    }

    public void AddComponent(Component component)
    {
        _components.Add(component);
    }
    
    public T? GetComponent<T>() where T : Component
    {
        return (T?)_components.FirstOrDefault(x => x.GetType() == typeof(T));
    }
}