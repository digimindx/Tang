# Tang
file utility that turns Finder drag-and-drop into instant format conversion and file editing. Hold Shift for conversion wheels, Option-Shift for advanced tools, and Control for quick actions—all processed 100% offline for privacy.

# Requirements and Installation
- Avalonia: 
```bash
dotnet new install Avalonia.Templates

dotnet new sln -n TangApp

dotnet new avalonia.app -n TangerineClone.UI

dotnet new classlib -n TangApp.Core

dotnet sln TangApp.slnx add TangApp.UI/TangApp.UI.csproj

dotnet add TangApp.UI/TangApp.UI.csproj reference TangApp.Core/TangApp.Core.csproj

dotnet restore
dotnet build
```
