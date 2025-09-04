namespace ManaxServer.Tasks;

public interface IBackGroundTask
{
    public void Execute();
    public string GetName();
    public TaskPriority GetPriority();
}