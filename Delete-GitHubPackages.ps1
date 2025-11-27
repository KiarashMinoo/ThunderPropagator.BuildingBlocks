[CmdletBinding()]
param(
# Optional: will be inferred from git config / remote if omitted
    [string]$GitHubUserName,

# Optional: will be taken from env vars if omitted
    [string]$GitHubToken,

# Package name filter (supports wildcards: *, ?)
# Use '*' to clean ALL user packages
    [string]$PackageNameFilter = 'RapidStreamer.BuildingBlocks*'
)

function Get-GitHubUserName {
    param(
        [string]$ExplicitName
    )

    # Normalize input safely (handles arrays, chars, non-string values)
    function ConvertTo-PlainString {
        param($v)
        if ($null -eq $v) { return $null }
        if ($v -is [System.Array]) { return ($v -join '') }
        return [string]$v
    }

    $expName = ConvertTo-PlainString $ExplicitName
    if (-not [string]::IsNullOrWhiteSpace($expName)) {
        return $expName.Trim()
    }

    # Try git config github.user
    try {
        $name = git config --get github.user 2>$null
    } catch {
        $name = $null
    }

    $nameStr = ConvertTo-PlainString $name
    if (-not [string]::IsNullOrWhiteSpace($nameStr)) {
        return $nameStr.Trim()
    }

    # Try to parse from remote.origin.url (https or ssh)
    try {
        $remote = git config --get remote.origin.url 2>$null
    } catch {
        $remote = $null
    }

    $remoteStr = ConvertTo-PlainString $remote
    if (-not [string]::IsNullOrWhiteSpace($remoteStr)) {
        # Examples:
        #  https://github.com/username/repo.git
        #  git@github.com:username/repo.git
        if ($remoteStr -match 'github\.com[:/](?<user>[^/]+)/') {
            $user = ConvertTo-PlainString $Matches['user']
            if (-not [string]::IsNullOrWhiteSpace($user)) {
                return $user.Trim()
            }
        }
    }

    throw "GitHub user name could not be determined. Pass -GitHubUserName explicitly or configure git (github.user or remote.origin.url)."
}

function Get-GitHubToken {
    param(
        [string]$ExplicitToken
    )
    # Normalize helper (reuse above)
    function ConvertTo-PlainStringLocal { param($v) if ($null -eq $v) { return $null } if ($v -is [System.Array]) { return ($v -join '') } return [string]$v }

    $expTok = ConvertTo-PlainStringLocal $ExplicitToken
    if (-not [string]::IsNullOrWhiteSpace($expTok)) {
        return $expTok.Trim()
    }

    # Try common env vars and normalize them
    $candidates = @($env:GITHUB_TOKEN, $env:GH_TOKEN, $env:GH_PAT) | ForEach-Object { ConvertTo-PlainStringLocal $_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    if ($candidates.Count -gt 0) {
        return $candidates[0].Trim()
    }

    throw "GitHub token could not be determined. Set GITHUB_TOKEN / GH_TOKEN / GH_PAT environment variable or pass -GitHubToken explicitly."
}

# Resolve username and token (throws if both ways fail)
$GitHubUserName = Get-GitHubUserName -ExplicitName $GitHubUserName
$GitHubToken    = Get-GitHubToken    -ExplicitToken $GitHubToken

Write-Host "Using GitHub user: $GitHubUserName"
Write-Host "Using GitHub token from parameter/env."
Write-Host "Package name filter: '$PackageNameFilter'"
Write-Host ""

$baseUri = "https://api.github.com"
$headers = @{
    "Authorization"        = "Bearer $GitHubToken"
    "Accept"               = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

$iteration = 1
$maxIterations = 50   # safety guard against infinite loops

do {
    Write-Host "=== Iteration $iteration ==="

    $page       = 1
    $pageSize   = 100
    $allPackages = @()

    # Fetch all packages for the user (paged)
    do {
        $uri = "$baseUri/users/$GitHubUserName/packages?per_page=$pageSize&page=$page"
        $packages = Invoke-RestMethod -Uri $uri -Headers $headers -Method Get

        if ($packages -and $packages.Count -gt 0) {
            $allPackages += $packages
            $page++
        } else {
            break
        }
    } while ($true)

    if (-not $allPackages -or $allPackages.Count -eq 0) {
        Write-Host "No packages found for user '$GitHubUserName'."
        break
    }

    # Filter by name (supports wildcard)
    $packagesToDelete = $allPackages | Where-Object {
        $_.name -like $PackageNameFilter
    }

    if (-not $packagesToDelete -or $packagesToDelete.Count -eq 0) {
        Write-Host "No more packages match filter '$PackageNameFilter'. Cleanup complete."
        break
    }

    Write-Host "Packages matching '$PackageNameFilter' in this iteration:"
    $packagesToDelete | Select-Object id, name, package_type | Format-Table -AutoSize
    Write-Host ""

    foreach ($pkg in $packagesToDelete) {
        $packageName = $pkg.name
        $packageType = $pkg.package_type  # nuget, npm, container, etc.
        $deleteUri   = "$baseUri/users/$GitHubUserName/packages/$packageType/$packageName"

        # 🧪 DRY-RUN: uncomment to test and comment the real delete
        # Write-Host "Would DELETE: $deleteUri"
        # continue

        Write-Host "Deleting '$packageName' (type: $packageType)..."
        try {
            Invoke-RestMethod -Uri $deleteUri -Headers $headers -Method Delete
            Write-Host "Deleted '$packageName'."
        }
        catch {
            Write-Warning "Failed to delete '$packageName': $($_.Exception.Message)"
        }

        Write-Host ""
    }

    $iteration++
    if ($iteration -gt $maxIterations) {
        Write-Warning "Reached max iterations ($maxIterations). Stopping to avoid infinite loop."
        break
    }

} while ($true)

Write-Host "Done."
