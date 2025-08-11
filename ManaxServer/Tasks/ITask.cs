namespace ManaxServer.Tasks;

public interface ITask
{
    public void Execute();
    public string GetName();
    public TaskPriority GetPriority();
}