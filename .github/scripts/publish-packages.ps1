<#
.SYNOPSIS
  Publish NuGet packages and symbols to a feed.

.DESCRIPTION
  PowerShell script that handles:
   - Downloading artifacts from GitHub Actions
   - Publishing .nupkg files to NuGet feed
   - Publishing .snupkg symbol files
   - Making GitHub Packages public (if targeting GitHub)
   - Deleting all workflow artifacts
   - Cleanup

  Used by GitHub Actions publish jobs.

.PARAMETER NuGetSource
  NuGet feed source URL

.PARAMETER NuGetApiKey
  NuGet feed API key

.PARAMETER PackagesPath
  Path where packages are downloaded (default: ./dist/packages)

.PARAMETER SymbolsPath
  Path where symbols are downloaded (default: ./dist/symbols)

.PARAMETER SkipSymbols
  Skip publishing symbol packages

.PARAMETER SkipCleanup
  Skip cleanup of downloaded artifacts

.PARAMETER MakePublic
  Make GitHub Packages public after publishing (requires GitHubApiToken)

.PARAMETER GitHubApiToken
  GitHub API token for setting package visibility

.PARAMETER GitHubOwner
  GitHub repository owner (default: yanis_1984)

.EXAMPLE
  pwsh .github/scripts/publish-packages.ps1 -NuGetSource $env:NUGET_SOURCE -NuGetApiKey $env:NUGET_API_KEY

.EXAMPLE
  pwsh .github/scripts/publish-packages.ps1 -NuGetSource $env:NUGET_SOURCE -NuGetApiKey $env:NUGET_API_KEY -MakePublic -GitHubApiToken $env:GITHUB_TOKEN
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$NuGetSource,
    
    [Parameter(Mandatory = $true)]
    [string]$NuGetApiKey,
    
    [string]$PackagesPath = './dist/packages',
    [string]$SymbolsPath = './dist/symbols',
    [switch]$SkipSymbols,
    [switch]$SkipCleanup,
    [switch]$ReplaceIfExists,
    [switch]$MakePublic,
    [string]$GitHubApiToken = '',
    [string]$GitHubOwner = 'yanis_1984'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "=== NuGet Package Publishing ===" -ForegroundColor Cyan
Write-Host "Feed: $NuGetSource"
Write-Host "Packages Path: $PackagesPath"

# Validate required parameters
if (-not $NuGetSource -or -not $NuGetApiKey) {
    Write-Error "NuGetSource and NuGetApiKey are required."
    exit 1
}

# Push .nupkg files
Write-Host "`n--- Publishing Packages (.nupkg) ---" -ForegroundColor Yellow
$packages = Get-ChildItem -Path $PackagesPath -Filter '*.nupkg' -ErrorAction SilentlyContinue

if (-not $packages) {
    Write-Warning "No .nupkg files found in $PackagesPath"
}
else {
    Write-Host "Found $($packages.Count) package(s) to publish"
    $successCount = 0
    $skipCount = 0
    $failCount = 0
    
    foreach ($pkg in $packages) {
        Write-Host "`nPushing: $($pkg.Name)" -ForegroundColor Cyan
        # If requested, attempt to delete existing version on the feed before pushing
        if ($ReplaceIfExists) {
            try {
                $baseName = [System.IO.Path]::GetFileNameWithoutExtension($pkg.Name)
                $versionPattern = '\d+\.\d+\.\d+(?:[.-][A-Za-z0-9\.\-]+)*$'
                if ($baseName -match "(?<id>.+)\.(?<version>$versionPattern)") {
                    $pkgId = $Matches['id']
                    $pkgVersion = $Matches['version']
                    Write-Host "  Replace requested: attempting to delete existing $pkgId $pkgVersion from feed..." -ForegroundColor Yellow
                    dotnet nuget delete $pkgId $pkgVersion --source $NuGetSource --api-key $NuGetApiKey --non-interactive --yes 2>&1 | Out-Null
                    if ($LASTEXITCODE -eq 0) { Write-Host "  Deleted existing $pkgId $pkgVersion (if present)" -ForegroundColor Yellow }
                }
                else {
                    Write-Host "  Could not parse package id/version from '$($pkg.Name)'; skipping pre-delete." -ForegroundColor DarkYellow
                }
            }
            catch {
                Write-Warning "  Pre-delete attempt failed: $($_.Exception.Message)"
            }
        }

        dotnet nuget push $pkg.FullName `
            --source $NuGetSource `
            --api-key $NuGetApiKey `
            --skip-duplicate `
            2>&1 | Tee-Object -Variable output
        
        if ($LASTEXITCODE -eq 0) {
            if ($output -match 'already exists') {
                Write-Host "  ✓ Skipped (already exists)" -ForegroundColor Yellow
                $skipCount++
            }
            else {
                Write-Host "  ✓ Published successfully" -ForegroundColor Green
                $successCount++
            }
        }
        else {
            Write-Warning "  ✗ Failed to publish $($pkg.Name)"
            $failCount++
        }
    }
    
    Write-Host "`nPackage Summary:" -ForegroundColor Yellow
    Write-Host "  Published: $successCount" -ForegroundColor Green
    Write-Host "  Skipped: $skipCount" -ForegroundColor Yellow
    if ($failCount -gt 0) {
        Write-Host "  Failed: $failCount" -ForegroundColor Red
    }
}

# Push .snupkg files (symbols)
if (-not $SkipSymbols) {
    Write-Host "`n--- Publishing Symbols (.snupkg) ---" -ForegroundColor Yellow
    $symbols = Get-ChildItem -Path $SymbolsPath -Filter '*.snupkg' -ErrorAction SilentlyContinue
    
    if (-not $symbols) {
        Write-Host "No .snupkg files found in $SymbolsPath" -ForegroundColor Gray
    }
    else {
        Write-Host "Found $($symbols.Count) symbol package(s) to publish"
        
        foreach ($sym in $symbols) {
            Write-Host "`nPushing: $($sym.Name)" -ForegroundColor Cyan
            
            dotnet nuget push $sym.FullName `
                --source $NuGetSource `
                --api-key $NuGetApiKey `
                --skip-duplicate `
                2>&1 | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  ✓ Published successfully" -ForegroundColor Green
            }
            else {
                Write-Warning "  ✗ Failed to publish $($sym.Name) (non-fatal)"
            }
        }
    }
}

# Make GitHub Packages public
if ($MakePublic -and $NuGetSource -match 'nuget.pkg.github.com') {
    Write-Host "`n--- Setting GitHub Packages Visibility to Public ---" -ForegroundColor Yellow
    
    if (-not $GitHubApiToken) {
        Write-Warning "MakePublic specified but GitHubApiToken not provided. Skipping visibility update."
    }
    else {
        $packages = Get-ChildItem -Path $PackagesPath -Filter '*.nupkg' -ErrorAction SilentlyContinue
        
        if ($packages) {
            $headers = @{
                'Authorization' = "Bearer $GitHubApiToken"
                'Accept' = 'application/vnd.github+json'
                'X-GitHub-Api-Version' = '2022-11-28'
            }
            
            # Package names to make public
            $packageNames = @('ThunderPropagator.BuildingBlocks', 'ThunderPropagator.BuildingBlocks.Modules')
            
            foreach ($pkgName in $packageNames) {
                Write-Host "`nSetting visibility for package: $pkgName" -ForegroundColor Cyan
                
                try {
                    $uri = "https://api.github.com/user/packages/nuget/$pkgName"
                    
                    # First, check if package exists
                    try {
                        $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers -ErrorAction Stop
                        Write-Host "  Package found (current visibility: $($response.visibility))" -ForegroundColor Gray
                    }
                    catch {
                        Write-Warning "  Package not found or not accessible: $pkgName"
                        continue
                    }
                    
                    # Update visibility to public
                    $body = @{ visibility = 'public' } | ConvertTo-Json
                    $response = Invoke-RestMethod -Uri $uri -Method Patch -Headers $headers -Body $body -ContentType 'application/json' -ErrorAction Stop
                    
                    if ($response.visibility -eq 'public') {
                        Write-Host "  ✓ Set to PUBLIC successfully" -ForegroundColor Green
                    }
                    else {
                        Write-Warning "  Visibility update returned: $($response.visibility)"
                    }
                }
                catch {
                    $statusCode = $_.Exception.Response.StatusCode.value__
                    if ($statusCode -eq 404) {
                        Write-Host "  Package not found (may not exist yet or wrong name)" -ForegroundColor DarkYellow
                    }
                    elseif ($statusCode -eq 403) {
                        Write-Warning "  Permission denied. Token may lack 'write:packages' scope"
                    }
                    else {
                        Write-Warning "  Failed to update visibility: $($_.Exception.Message)"
                    }
                }
            }
        }
        else {
            Write-Host "No packages found to update visibility" -ForegroundColor Gray
        }
    }
}

# Cleanup
if (-not $SkipCleanup) {
    Write-Host "`n--- Cleanup ---" -ForegroundColor Yellow
    
    if (Test-Path './dist') {
        Remove-Item -Path './dist' -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removed ./dist" -ForegroundColor Gray
    }
    
    if (Test-Path './artifacts') {
        Remove-Item -Path './artifacts' -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removed ./artifacts" -ForegroundColor Gray
    }
}

Write-Host "`n=== Publishing Complete ===" -ForegroundColor Green
exit 0
