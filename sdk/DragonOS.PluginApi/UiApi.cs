namespace DragonOS.PluginApi;

using System.Threading;
using System.Threading.Tasks;

public enum UiImageFitMode
{
    Natural,
    Contain,
    CoverCrop,
    Stretch,
    Tile,
    Center,
    FillWidth,
    FillHeight
}

public readonly record struct UiColor(byte R, byte G, byte B);

public sealed record UiOutline(UiColor Color, float Transparency = 0f, int ThicknessPixels = 1);

public abstract record UiElement
{
    public required string Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int CornerRadius { get; init; }
    public float BackgroundTransparency { get; init; }
    public UiOutline? Outline { get; init; }
}

public abstract record UiInteractiveElement : UiElement
{
    public string? ClickEvent { get; init; }
}

public sealed record UiButton : UiInteractiveElement
{
    public required string Text { get; init; }
    public float TextTransparency { get; init; }
}

public sealed record UiImageButton : UiInteractiveElement
{
    public string? Image { get; init; }
    public required string ImagePath { get; init; }
    public UiImageFitMode FitMode { get; init; } = UiImageFitMode.Natural;
}

public sealed record UiImageLabel : UiElement
{
    public string? Image { get; init; }
    public required string ImagePath { get; init; }
    public UiImageFitMode FitMode { get; init; } = UiImageFitMode.Natural;
}

public sealed record UiVideoLabel : UiElement
{
    public string? Video { get; init; }
    public required string VideoPath { get; init; }
    public string? PosterImagePath { get; init; }
    public UiImageFitMode FitMode { get; init; } = UiImageFitMode.Contain;
    public bool AutoPlay { get; init; } = true;
    public bool Loop { get; init; }
    public bool Muted { get; init; } = true;
    public double StartSeconds { get; init; }
}

public sealed record VideoFrame : UiElement
{
    public required string Video { get; init; }
    public string? PosterImagePath { get; init; }
    public UiImageFitMode FitMode { get; init; } = UiImageFitMode.Contain;
    public bool AutoPlay { get; init; } = true;
    public bool Loop { get; init; }
    public bool Muted { get; init; } = true;
    public double StartSeconds { get; init; }
}

public sealed record UiLabel : UiElement
{
    public required string Text { get; init; }
    public float TextTransparency { get; init; }
}

public sealed record UiScene
{
    public float BackgroundTransparency { get; init; }
    public List<UiElement> Elements { get; } = [];

    public void AddElement(UiElement element) => Elements.Add(element);

    public bool RemoveElement(string id)
    {
        var element = Elements.FirstOrDefault(existing => existing.Id.Equals(id, StringComparison.Ordinal));
        if (element is null)
        {
            return false;
        }

        Elements.Remove(element);
        return true;
    }

    public bool UpdateElement(UiElement element)
    {
        for (var i = 0; i < Elements.Count; i++)
        {
            if (!Elements[i].Id.Equals(element.Id, StringComparison.Ordinal))
            {
                continue;
            }

            Elements[i] = element;
            return true;
        }

        return false;
    }
}

public sealed record UiEventContext
{
    public required string ElementId { get; init; }
    public required string EventName { get; init; }
    public int PointerX { get; init; }
    public int PointerY { get; init; }
    public bool PrimaryButtonDown { get; init; }
}

public sealed record DragonModuleDefinition
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public bool AutoStart { get; init; } = true;
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string?> Variables { get; init; } = new Dictionary<string, string?>();
}

public sealed record DragonTaskHandle(string Id);

public interface IDragonVariableContext
{
    bool SetVariable(string name, string? value);
    bool TryGetVariable(string name, out string? value);
    bool RemoveVariable(string name);
}

public interface IDragonRuntimeUiContext
{
    bool AddElement(UiElement element);
    bool UpdateElement(UiElement element);
    bool RemoveElement(string id);
}

public interface IDragonRuntimeModuleContext : IDragonRuntimeUiContext, IDragonVariableContext
{
    bool LoadModule(string moduleId);
    bool UnloadModule(string moduleId);
    bool IsModuleLoaded(string moduleId);
    ValueTask DelayAsync(TimeSpan duration, CancellationToken cancellationToken = default);
    DragonTaskHandle RunAsync(Func<CancellationToken, Task> work, string? name = null, CancellationToken cancellationToken = default);
}

public interface IDragonModuleMetadataProvider
{
    DragonModuleDefinition DescribeModule();
}

public interface IDragonUiModule
{
    UiScene BuildScene();
    void OnEvent(UiEventContext context, IDragonRuntimeUiContext runtimeUi);
    void OnTick(double deltaSeconds, IDragonRuntimeUiContext runtimeUi);
}

public interface IDragonAsyncUiModule
{
    ValueTask OnEventAsync(UiEventContext context, IDragonRuntimeModuleContext runtime, CancellationToken cancellationToken = default);
    ValueTask OnTickAsync(double deltaSeconds, IDragonRuntimeModuleContext runtime, CancellationToken cancellationToken = default);
}
