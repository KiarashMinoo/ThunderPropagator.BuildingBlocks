#!/usr/bin/env bash

# Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

# Create all NuGet packages
dotnet pack ./BuildingBlocks/RapidStreamer.BuildingBlocks.Infrastructure/RapidStreamer.BuildingBlocks.Infrastructure.csproj --no-build -c Release -o ./artifacts