using DragonOS.PluginApi;

namespace DragonSetupUi;

public sealed class SetupModule : IDragonUiModule, IDragonModuleMetadataProvider
{
    public DragonModuleDefinition DescribeModule() => new()
    {
        Id = "dragon.setup",
        DisplayName = "Dragon Setup UI",
        AutoStart = true,
        Dependencies = Array.Empty<string>(),
        Variables = new Dictionary<string, string?>
        {
            ["setup.mode"] = "interactive"
        }
    };

    public UiScene BuildScene()
    {
        var scene = new UiScene
        {
            BackgroundTransparency = 0f
        };

        scene.Elements.Add(new UiImageLabel
        {
            Id = "mascot",
            X = 96,
            Y = 184,
            Width = 496,
            Height = 647,
            ImagePath = "Icon_Happy.png",
            FitMode = UiImageFitMode.Contain,
            BackgroundTransparency = 0f
        });

        scene.Elements.Add(new UiButton
        {
            Id = "install",
            X = 1228,
            Y = 643,
            Width = 488,
            Height = 88,
            CornerRadius = 25,
            BackgroundTransparency = 0f,
            Outline = new UiOutline(new UiColor(0, 0, 0), 1f, 0),
            Text = "Install",
            ClickEvent = "install.clicked"
        });

        scene.Elements.Add(new UiButton
        {
            Id = "try",
            X = 1228,
            Y = 776,
            Width = 488,
            Height = 88,
            CornerRadius = 25,
            Text = "Try",
            ClickEvent = "try.clicked"
        });

        scene.Elements.Add(new UiImageButton
        {
            Id = "logo-button",
            X = 152,
            Y = 152,
            Width = 96,
            Height = 96,
            ImagePath = "Icon_Headshot.png",
            FitMode = UiImageFitMode.Contain,
            BackgroundTransparency = 1f,
            Outline = new UiOutline(new UiColor(0, 0, 0), 0.85f, 2),
            ClickEvent = "logo.clicked"
        });

        scene.Elements.Add(new UiVideoLabel
        {
            Id = "loading-preview",
            X = 980,
            Y = 220,
            Width = 340,
            Height = 190,
            CornerRadius = 18,
            VideoPath = "LoadingScreen.mp4",
            PosterImagePath = "Icon_Yoink.png",
            FitMode = UiImageFitMode.CoverCrop,
            BackgroundTransparency = 0f,
            Outline = new UiOutline(new UiColor(0, 0, 0), 0.70f, 2),
            AutoPlay = true,
            Loop = true,
            Muted = true
        });

        return scene;
    }

    public void OnEvent(UiEventContext context, IDragonRuntimeUiContext runtimeUi)
    {
        if (context.EventName == "logo.clicked")
        {
            runtimeUi.RemoveElement("logo-button");
            return;
        }

        if (context.EventName == "try.clicked")
        {
            runtimeUi.UpdateElement(new UiLabel
            {
                Id = "runtime-message",
                X = 980,
                Y = 430,
                Width = 420,
                Height = 48,
                Text = "Runtime update requested from Try.",
                BackgroundTransparency = 1f
            });
        }
    }

    public void OnTick(double deltaSeconds, IDragonRuntimeUiContext runtimeUi)
    {
        _ = deltaSeconds;
        _ = runtimeUi;
    }
}
