using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxApp.Models;

public class TaskItem : ObservableObject
{
    public string TaskName { get; set; } = string.Empty;
    public int Number { get; set; }
}