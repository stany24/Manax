namespace ManaxApi.Task;

public interface ITask
{
    public void Execute();
    public string GetName();
}