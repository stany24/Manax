// ReSharper disable PropertyCanBeMadeInitOnly.Global

using ManaxApi.Models.User;

namespace ManaxApi.Models.Issue;

public class Issue
{
    public long Id { get; set; }
    public List<Chapter.Chapter> Chapters { get; set; } = [];
    public UserRole Role { get; set; }
}