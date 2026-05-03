$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
dotnet run --project (Join-Path $projectRoot "DragonOSBuilder.csproj") --configuration Release -- $args
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")