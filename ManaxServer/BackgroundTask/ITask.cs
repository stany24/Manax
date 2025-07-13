namespace ManaxServer.BackgroundTask;

public interface ITask
{
    public void Execute();
    public string GetName();
    public int GetPriority() => 0;
}