namespace RoundedWhiteCubePlugin;

public static class PluginEntry
{
    public static UiSceneDefinition Create() =>
        new(
            Boxes: new[]
            {
                new UiBoxDefinition(
                    X: 220,
                    Y: 120,
                    Width: 2000,
                    Height: 2000,
                    CornerRadius: 22,
                    Red: 200,
                    Green: 200,
                    Blue: 200,
                    Draggable: false),

                new UiBoxDefinition(
                    X: 220,
                    Y: 120,
                    Width: 400,
                    Height: 200,
                    CornerRadius: 22,
                    Red: 255,
                    Green: 255,
                    Blue: 255,
                    Draggable: false),

            });    
}

public sealed record UiSceneDefinition(IReadOnlyList<UiBoxDefinition> Boxes);

public sealed record UiBoxDefinition(
    int X,
    int Y,
    int Width,
    int Height,
    int CornerRadius,
    byte Red,
    byte Green,
    byte Blue,
    bool Draggable);