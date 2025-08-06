using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using Moq;

namespace ManaxTests.Mocks;

public class TestData
{
    public Mock<ManaxContext> Context { get; init; } = new();
    public List<Chapter> Chapters { get; init; } = [];
    public List<Serie> Series { get; init; } = [];
}