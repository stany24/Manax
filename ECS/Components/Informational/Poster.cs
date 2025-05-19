using ImageMagick;

namespace ECS.Components.Informational;

public class Poster:Component
{
    public MagickImage Value { get; set; }
}