
# RapidStreamer.BuildingBlocks

**RapidStreamer.BuildingBlocks** is a NuGet package designed to provide foundational components and utilities for building applications on .NET 8. This package is maintained by the **RapidStreamer** team, offering reusable building blocks to streamline development.

## Overview

RapidStreamer is a cutting-edge software solution designed for redefining real-time data streaming. Our mission is to provide effortless, blazingly fast, and cloud-native streaming capabilities for maximum impact.

## Table of Contents

- [Installation](#installation)
- [Configuring GitHub as a NuGet Package Source](#configuring-github-as-a-nuget-package-source)
- [Getting Started](#getting-started)
- [License](#license)

## Installation

The **RapidStreamer.BuildingBlocks** package is hosted on GitHub Packages. To install it, add the GitHub package source configuration, then use the **NuGet Package Manager** in Visual Studio or the **dotnet CLI**.

### NuGet CLI

To install directly via the .NET CLI:
```bash
dotnet add package RapidStreamer.BuildingBlocks --version [Latest Version]
```

### Package Manager Console in Visual Studio

```powershell
Install-Package RapidStreamer.BuildingBlocks -Version [Latest Version]
```

## Configuring GitHub as a NuGet Package Source

The **RapidStreamer.BuildingBlocks** package is available from GitHub Packages. To enable this GitHub source in your project, add the GitHub Packages URL to your NuGet configuration.

1. **Edit NuGet.config**:
   Add the following GitHub package source to your `NuGet.config` file:

   ```xml
   <configuration>
     <packageSources>
       <add key="GitHub-KAB-TEAM" value="https://nuget.pkg.github.com/KAB-TEAM/index.json" />
     </packageSources>
   </configuration>
   ```

2. **Authentication**:
    - GitHub Packages requires authentication to access private repositories. Use your GitHub personal access token (PAT) as your password and GitHub username as the username.
    - Set up a GitHub PAT in your NuGet configuration to access the GitHub source:

   ```bash
   dotnet nuget add source https://nuget.pkg.github.com/KAB-TEAM/index.json -n GitHub-KAB-TEAM -u USERNAME -p TOKEN --store-password-in-clear-text
   ```
   Replace `USERNAME` with your GitHub username and `TOKEN` with a GitHub PAT with `read:packages` and `repo` scope.

3. **Verify the Configuration**:
   After adding the source, confirm that the GitHub source is listed with:
   ```bash
   dotnet nuget list source
   ```

## Getting Started

Once configured, you can use the components provided by **RapidStreamer.BuildingBlocks** by importing the package into your .NET 8 project. Refer to the documentation for each component to explore the available functionality and example usage.

## License

This project is licensed under the MIT License.

---

© 2024 RapidStreamer. All rights reserved.

