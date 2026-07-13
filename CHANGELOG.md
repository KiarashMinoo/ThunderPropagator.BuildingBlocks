# Changelog

All notable changes to this project will be documented in this file.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.1-beta.106] — 2026-07-13

### 📦 Dependencies

| Package | Old | New |
|---------|-----|-----|
| Microsoft.NET.Test.Sdk | 18.6.0 | 18.7.0 |
| YamlDotNet | 18.0.0 | 18.1.0 |
| MessagePack | 3.1.7 | 3.1.8 |
| MessagePackAnalyzer | 3.1.7 | 3.1.8 |
| NSubstitute | 5.3.0 | 6.0.0 |

- Bump the microsoft-extensions group with 1 update `(c4a6d60)` — dependabot[bot]
- Bump YamlDotNet from 18.0.0 to 18.1.0 `(a471adb)` — dependabot[bot]
- Bump MessagePack and MessagePackAnalyzer `(41c9c61)` — dependabot[bot]
- Bump MessagePackAnalyzer from 3.1.7 to 3.1.8 `(f787f3f)` — dependabot[bot]
- Bump NSubstitute from 5.3.0 to 6.0.0 `(9a2be34)` — dependabot[bot]

### ⚙️ CI / Tooling

- Add NuGet OIDC publish jobs in CI `(1327268)` — Kiarash Minoo
- Tidy dependency groups and SAST schedule `(7e630da)` — Kiarash Minoo
- Centralize CI workflows and add concurrency controls `(95eb3a8)` — Kiarash Minoo
- Unify CI on shared reusable workflow `(5ed8827)` — Kiarash Minoo
- Gate CI jobs by event type `(9986e62)` — Kiarash Minoo
- Fix SAST inputs and pass GitHub token `(00171b4)` — Kiarash Minoo
- Allow manual workflow dispatch to choose beta or release `(029f50f)` — Kiarash Minoo
- Wire admin push token into CI workflow `(b968ee7)` — Kiarash Minoo

### 🏠 Chores

- bump version to 1.0.1-beta.105 [skip ci] `(56a559f)` — github-actions[bot]

## [Unreleased]

### ⚙️ CI / Tooling
- Integrate shared build props and clean up assets `(8fd7cc0)` — Kiarash Minoo
- Remove shared build submodule configuration `(4271974)` — Kiarash Minoo
- Remove legacy CI/CD scripts and workflows in favour of reusable templates `(c414d14)` — Kiarash Minoo
- Update projects to target .NET 10 `(4f57aa1)` — Kiarash Minoo
- Add analysers.props from SharedBuild `(452cc43)` — Kiarash Minoo
- Delegate ci.yml to reusable-ci.yml in .github `(2f5e441)` — Kiarash Minoo
- Add CodeQuality props to Directory.Build.props `(cd802e9)` — Kiarash Minoo
- Add shared .editorconfig `(09b9564)` — Kiarash Minoo
- Add CI workflow, .editorconfig and enhance Directory.Build.props `(e62d283)` — Kiarash Minoo
- Update .gitignore and add submodule for shared build `(b3f8144)` — Kiarash Minoo
- Update permissions and token reference in package cleanup workflow `(ef81bd7)` — Kiarash Minoo
- Correct script path in package cleanup workflow `(69affcc)` — Kiarash Minoo
- Update build scripts to use GH_TOKEN for improved security `(3d2d4e7)` — Kiarash Minoo
- Update GitHub token references to use GH_TOKEN for consistency `(1920466)` — Kiarash Minoo
- Add pack-all-platforms script `(8ed4b2c)` — Kiarash Minoo
- Enhance package detection and handling in pack-all-platforms script `(bf816e1)` — Kiarash Minoo
- Add support for .NET 10.0 in CI configurations and package management `(0080481)` — Kiarash Minoo
- Add support for building multiple platforms `(3aa1263)` — Kiarash Minoo
- Refine git CI jobs and scripts `(553a480)` — Kiarash Minoo
- Fix beta CI package versioning `(0c17bfa)` — Ahmad(Kia) Minoo
- Fix beta CI problems and git tagging `(77e12be)` — Ahmad(Kia) Minoo
- Update beta CI to increment build number when previous release exists `(d7c36ae)` — Ahmad(Kia) Minoo

### 🚀 Features
- Add GPU metrics collection and related infrastructure `(8e8fb0f)` — Kiarash Minoo
- Implement load generators and metrics sampler for resource monitoring tests `(9e35287)` — Kiarash Minoo
- Add PowerShell script for generating detailed release notes `(f8c755a)` — Kiarash Minoo
- Enhance package visibility handling in publish script with dynamic owner extraction `(0ebaad5)` — Kiarash Minoo
- Add FilterPattern parameter to publish script for selective package publishing `(3cb492e)` — Kiarash Minoo
- Add support for making GitHub Packages public in publish script `(17fbf9c)` — Kiarash Minoo
- Extend system monitoring to include hardware health and performance metrics `(c49affa)` — Kiarash Minoo
- Refactor metrics clients to use interfaces for better abstraction and dependency injection `(4178e04)` — Kiarash Minoo
- Add Toon serialization helpers and integrate with existing JSON serialization `(7682452)` — Kiarash Minoo
- Add architecture tests and enhance serialization tests `(2bd8749)` — Kiarash Minoo
- Add comprehensive documentation for application attributes and helper utilities `(42f42e0)` — Kiarash Minoo
- Add bind method to ServiceConfiguration `(646ff01)` — Ahmad(Kia) Minoo
- Add event handlers for disposable objects `(14e6555)` — Ahmad(Kia) Minoo
- Add PasswordGenerator `(df90b9d)` — Ahmad(Kia) Minoo
- Add ImmutableObject `(d2beada)` — Ahmad(Kia) Minoo
- Add CertificateModel `(c0351ea)` — Ahmad(Kia) Minoo
- Add arm64 support `(895205c)` — Ahmad(Kia) Minoo
- Add environment keys support `(e06158f)` — Kiarash Minoo
- Add NetJSON serialization support `(faffcca)` — Kiarash Minoo
- Handle DispatcherTimer task disposition `(1364ec0)` — Kiarash Minoo
- Add dispose functionalities on IAsyncDisposable `(7a5d1f9)` — Kiarash Minoo
- Enable compression and include native libraries for self-extraction `(8030b76)` — Kiarash Minoo

### 🐛 Bug Fixes
- Refine correlation ID logic and remove solution file `(b459d6a)` — Kiarash Minoo
- Update package versions for improved stability `(cff07cc)` — Kiarash Minoo
- Fix bug on FeederMessage hash code `(2d50b52)` — Kiarash Minoo
- Fix CorrelationIdProvider `(a0a5554)` — Kiarash Minoo
- Fix splice arrays `(220a64e)` — Kiarash Minoo
- Fix AnyCPU PlatformTarget `(b80f356)` — Kiarash Minoo
- Fix SystemResourceMonitor `(5f1bc24)` — Ahmad(Kia) Minoo
- Fix EquatableObject `(e1492e7)` — Ahmad(Kia) Minoo
- Fix GetAtomicValues `(8b853ef)` — Ahmad(Kia) Minoo
- Fix meters `(365164c)` — Ahmad(Kia) Minoo
- Fix Telemetry `(94e05ca)` — Ahmad(Kia) Minoo
- Fix readYaml on YAML type converters `(6f0136b)` — Ahmad(Kia) Minoo
- Correct variable interpolation in visibility change message for GitHub Packages `(cff168a)` — Kiarash Minoo
- Update package name filter to match ThunderPropagator.BuildingBlocks `(8f7f088)` — Kiarash Minoo
- Normalize release notes for MSBuild compatibility `(c7295c3)` — Kiarash Minoo
- Handle empty ReleaseNotes in CI packaging scripts `(68d77d9)` — Kiarash Minoo
- Remove unnecessary FrameworkReference from project files `(d334968)` — Kiarash Minoo
- Improve parameter validation in update-version script `(c2c4727)` — Kiarash Minoo
- Fix version not found `(7b8bf46)` — Ahmad(Kia) Minoo
- Update package versions and adjust publish script configuration `(2c509ae)` — Kiarash Minoo

### ♻️ Refactoring
- Refactor code structure for improved readability and maintainability `(99c79b7)` — Kiarash Minoo
- Update namespaces to align with ThunderPropagator structure in integration tests `(55fec48)` — Kiarash Minoo
- Refactor system resource monitoring metrics to implement IMetrics interface `(62f1dea)` — Kiarash Minoo
- Enhance JSON and YAML serialization methods for improved performance and clarity `(c602868)` — Kiarash Minoo
- Optimize string handling in EnvironmentHelper and ExceptionHelper `(bf89152)` — Kiarash Minoo
- Simplify CollectionHelper methods and improve argument handling `(4606ecb)` — Kiarash Minoo
- Enhance ActiveMQ health check and improve DateTimeHelper logic `(02743eb)` — Kiarash Minoo
- Simplify PackageId construction in project files `(2f1660a)` — Kiarash Minoo
- Replace StringBuilder with string concatenation for performance `(95bd5b6)` — Kiarash Minoo
- Optimise lock mechanism `(40644d2)` — Ahmad(Kia) Minoo

### 🧪 Tests
- Add integration tests and load generators for system resource monitoring `(a15d8a1)` — Kiarash Minoo
- Add integration tests and load generators for system resource metrics `(b979dc9)` — Kiarash Minoo
- Add unit tests for CPU, Disk, and GPU metrics clients `(4c68c3b)` — Kiarash Minoo
- Add benchmarks for collection helper methods `(7d95e13)` — Kiarash Minoo
- Add benchmarking for size calculations in unit tests `(3d72ae6)` — Kiarash Minoo

### 📝 Documentation
- Add documentation for BuildingBlocks.Application and BuildingBlocks.Infrastructure `(0e50fbf)` — Kiarash Minoo
- Add comprehensive AI agent instructions and project documentation `(3ac70c3)` — Kiarash Minoo
- Add comprehensive documentation for application attributes and helper utilities `(42f42e0)` — Kiarash Minoo
- Update readme files `(10a20e3)` — Ahmad(Kia) Minoo
- Update documentations `(0e73a07)` — Ahmad(Kia) Minoo
- Add documentations `(76a76cf)` — Ahmad(Kia) Minoo
- Add ReadMe.md `(747613c)` — Kiarash Minoo

### 📦 Dependencies
| Package | Old | New |
|---------|-----|-----|
| BenchmarkDotNet | 0.14.0 | 0.15.8 |
| Microsoft.Diagnostics.Tracing.TraceEvent | 3.1.28 | 3.1.29 |
| System.IdentityModel.Tokens.Jwt | 8.14.0 | 8.15.0 |

### 🏠 Chores
- Remove obsolete benchmark log file for CollectionHelperBenchmark `(3ed7282)` — Kiarash Minoo
- Update package references and add Directory.Packages.props for centralised version management `(271f614)` — Kiarash Minoo
- Update BenchmarkDotNet to 0.15.8 `(5fb86fb)` — Kiarash Minoo
- Remove unwanted prompt files `(381e2a4)` — Kiarash Minoo

