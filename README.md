# Vaultr
A MAUI application to manage multiple KeyVaults at the same time, making comparing, copying and moving secrets between KeyVaults easy.

![Vaultr](readme.png)

## Installation

### Windows
Download the latest build from the [releases](https://github.com/ThomasBleijendaal/Vaultr/releases) page. To install using the MSIX use [this documentation from Microsoft](https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-cli?view=net-maui-8.0#installing-the-app).

### macOS
**Requirements:**
- macOS 15.0 (Sequoia) or later
- .NET 10 SDK
- Xcode 16.2 or later with Command Line Tools
- Azure CLI (for authentication)

**Installing Azure CLI:**
```bash
brew install azure-cli
```

**Building from source:**
```bash
# Install .NET 10 MAUI workload
sudo dotnet workload install maui

# Clone the repository
git clone https://github.com/ThomasBleijendaal/Vaultr.git
cd Vaultr/Vaultr/Vaultr.Client

# Build and run
dotnet build -f net10.0-maccatalyst -c Release
open bin/Release/net10.0-maccatalyst/maccatalyst-arm64/Vaultr.Client.app

# Or for development
dotnet run -f net10.0-maccatalyst
```

## Development
Download the latest Visual Studio (Windows) or Visual Studio Code (Mac) with MAUI workload and compile and deploy Vaultr yourself. 
