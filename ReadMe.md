# RapidStreamer BuildingBlocks

RapidStreamer BuildingBlocks is a comprehensive library of production-ready components for .NET application development. It provides robust, reusable building blocks for scalable, high-performance, and cloud-native solutions.

---

## 📚 Documentation

**Main Documentation Hub:**
- [Full Documentation Catalog](docs/README.md)

Explore all Application and Infrastructure components, API references, integration patterns, and performance benchmarks in the [docs/README.md](docs/README.md) landing page.

---

## 🚀 NuGet Installation (GitHub Packages)

RapidStreamer packages are hosted at GitHub Packages:
`https://nuget.pkg.github.com/KiarashMinoo/index.json`

**Add the GitHub Packages feed to your NuGet config:**

```xml
<!-- nuget.config -->
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/KiarashMinoo/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="github">
      <package pattern="RapidStreamer.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

**Add the source via CLI:**
```bash
dotnet nuget add source --name github --source https://nuget.pkg.github.com/KiarashMinoo/index.json --username KiarashMinoo --password $GITHUB_TOKEN --store-password-in-clear-text
```

---

## 📦 RapidStreamer Packages

| Package | Description | Docs Usage |
|---------|-------------|------------|
| RapidStreamer.BuildingBlocks.Application | Core application building blocks | [docs/BuildingBlocks.Application/README.md](docs/BuildingBlocks.Application/README.md) |
| RapidStreamer.BuildingBlocks.Infrastructure | Infrastructure and monitoring | [docs/BuildingBlocks.Infrastructure/README.md](docs/BuildingBlocks.Infrastructure/README.md) |

---

## 🛠️ Build & Restore

```bash
dotnet restore
dotnet build -c Release
```

---

## 📊 Coverage Audit

See [docs/README.md](docs/README.md#coverage-audit) for a full audit of documentation coverage, folder READMEs, and API details.

---

For more details, see the [Documentation Catalog](docs/README.md).