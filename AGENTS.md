# AGENTS.md

Windows tray application that prevents sleep mode when Spotify is playing.

## Build & Test Commands

```bash
# Build entire solution
msbuild SpotifySleepModeStopper.sln /p:Configuration=Release

# Build single project
msbuild SpotifySleepModeStopper/SpotifySleepModeStopper.csproj

# Run tests (requires Visual Studio or vstest.console.exe)
vstest.console.exe UnitTestProject/bin/Debug/UnitTestProject.dll
```

## Project Structure

| Project | Type | Purpose |
|---------|------|---------|
| `SpotifySleepModeStopper/` | Class Library (.NET 4.8) | Core logic: audio analysis, power management, settings |
| `SpotifySleepModeStopperGui/` | WPF App (.NET 4.8) | System tray GUI, depends on ↑ |
| `UnitTestProject/` | MSTest | Unit tests, depends on Core lib, uses Rhino.Mocks |

## Architecture Notes

- **Entry point**: `SpotifySleepModeStopperGui/App.xaml.cs` launches WPF GUI
- **Core facade**: `SpotifySaveModeStopperFacade.cs` orchestrates detection + sleep prevention
- **Audio detection**: Uses CSCore library to check if Spotify process is outputting sound
- **Power management**: `PowerRequestContextHandler.cs` calls Windows `SetThreadExecutionState` API
- **Namespace mismatch**: Project folder uses "SpotifySleepModeStopper" but code namespace is `SpotifyTools`

## Dependencies & Package Restore

- Uses legacy `packages.config` (not PackageReference)
- NuGet packages:
  - `CSCore` (audio analysis) - Core lib only
  - `RhinoMocks` (mocking) - Test project only
- **Restore before build**: `nuget restore SpotifySleepModeStopper.sln`

## Testing Quirks

- MSTest framework (not xUnit/NUnit)
- Tests mock `IPreventSleepScreen`, `ISoundAnalyser`, etc.
- No integration tests - all mocked

## Operational Notes

- **Windows-only**: Uses `kernel32.dll` P/Invoke for power management
- Requires Spotify desktop app running (detects "Spotify" process name)
- Settings persisted to JSON file via `SettingsManager`
- Supports auto-start with Windows via registry

## Release Build

- GUI project outputs `SpotifySleepModeStopper.exe`
- Copy `CSCore.dll` alongside executable
- Uses icon resources: `music.ico`, `music_playing.ico`, `music_big.ico`
