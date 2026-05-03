# Dragon OS DLL Guide

Dragon OS currently has a packaging system and a plugin API.

What works:
- `.dll` files are copied into the Dragon runtime image.
- they are listed in `DRAGON\MANIFEST.TXT`
- you can build against a real API assembly at `sdk\DragonOS.PluginApi\`
- the API now includes runtime-style add/update/remove hooks and a tick callback!

What does not work yet:
- Dragon OS does not execute arbitrary PE DLL code inside the OS yet
- button events, live runtime mutation, and video playback are defined in the API now, but the Dragon OS runtime dispatcher/player is still future work.

## Where to put DLLs

Drop compiled DLL files in any of these places before building:

- `Dragon OS\`
- `Dragon OS\dlls\`
- `Dragon OS\modules\`
- `Dragon OS\plugins\`

They will be copied into the runtime image under `DRAGON\DLL\...`.

## API DLL

Reference the API project here:

- `sdk\DragonOS.PluginApi\DragonOS.PluginApi.csproj`

Or the built DLL once compiled:

- `dlls\DragonOS.PluginApi.dll`

Main API surface:

- `UiElement`
- `UiButton`
- `UiImageButton`
- `UiImageLabel`
- `UiVideoLabel`
- `UiLabel`
- `UiOutline`
- `UiImageFitMode`
- `UiScene`
- `UiEventContext`
- `IDragonRuntimeUiContext`
- `IDragonUiModule`

## UI Features

- `BackgroundTransparency`
  - `0` = fully visible
  - `1` = fully invisible
- `Outline`
  - `Color`
  - `Transparency`
  - `ThicknessPixels`
- `UiImageButton`
  - image-backed button
  - supports click events
- `UiImageLabel`
  - image-backed non-clickable element
- `UiVideoLabel`
  - video-backed non-clickable element
  - supports `VideoPath`
  - optional `PosterImagePath`
  - `AutoPlay`
  - `Loop`
  - `Muted`
  - `StartSeconds`
- `UiImageFitMode`
  - `Natural`
  - `Contain`
  - `CoverCrop`
  - `Stretch`
  - `Tile`
  - `Center`
  - `FillWidth`
  - `FillHeight`

## Button Events

Buttons and image buttons now expose:

- `ClickEvent`

Modules implement:

```csharp
void OnEvent(UiEventContext context, IDragonRuntimeUiContext runtimeUi);
void OnTick(double deltaSeconds, IDragonRuntimeUiContext runtimeUi);
```

Runtime UI mutation methods:

```csharp
bool AddElement(UiElement element);
bool UpdateElement(UiElement element);
bool RemoveElement(string id);
```

## Deleting Elements

Before returning the scene:

```csharp
scene.RemoveElement("old-element");
```

At runtime:

```csharp
runtimeUi.RemoveElement("logo-button");
```

## MP4-like Elements

Use `UiVideoLabel`:

```csharp
scene.AddElement(new UiVideoLabel
{
    Id = "intro-video",
    X = 100,
    Y = 100,
    Width = 640,
    Height = 360,
    VideoPath = "LoadingScreen.mp4",
    PosterImagePath = "frame0.png",
    FitMode = UiImageFitMode.Contain,
    AutoPlay = true,
    Loop = true,
    Muted = true
});
```

## Example Module

The setup example now uses the API here:

- `samples\DragonSetupUiPlugin\`

It includes:

- text buttons
- an image label
- an image button
- a video label example
- click event ids
- outline/background transparency examples
- runtime remove/update examples

## Build Flow

1. Build the API DLL if you want a binary reference:

```powershell
dotnet build .\sdk\DragonOS.PluginApi\DragonOS.PluginApi.csproj --configuration Release -o .\dlls
```

2. Build your module DLL and place it in one of the supported DLL folders.

3. Rebuild Dragon OS:

```powershell
dotnet run --project .\DragonOSBuilder.csproj --configuration Release
```

4. Boot:

- `build\os-uefi.iso`

5. Confirm packaging in:

- `build\DRAGON_MANIFEST.TXT`
