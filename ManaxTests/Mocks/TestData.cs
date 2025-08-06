using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using Moq;

namespace ManaxTests.Mocks;

public class TestData
{
    public Mock<ManaxContext> Context { get; set; }
    public List<Chapter> Chapters { get; set; }
    public List<Serie> Series { get; set; }
}