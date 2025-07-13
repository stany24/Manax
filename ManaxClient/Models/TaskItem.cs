using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models;

public class TaskItem : ObservableObject
{
    public string TaskName { get; set; } = string.Empty;
    public int Number { get; set; }
}