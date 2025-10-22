# Release Notes Generator

Automated CHANGELOG.md generator for LicenseManager with C/C++ API-aware analysis.

## Features

- **Incremental updates**: Tracks processed commits in `.github/release-notes.state.json`
- **Tag-based sections**: Generates sections for each git tag matching the filter pattern
- **Conventional Commits**: Groups commits by type (feat, fix, perf, refactor, docs, chore, build, ci, test, style, revert, deps, other)
- **C/C++ API detection**: Analyzes diff hunks to detect function signatures, class declarations, and build system changes
- **GitHub integration**: Generates compare links for each release
- **Path filtering**: Include/exclude specific files or patterns
- **Multiple content modes**: diff-hunks, added-lines, removed-lines, api-changes

## Quick Start

```powershell
# Full rebuild (from all tags)
python .github/release-notes.py --resetAll

# Incremental update (only new commits since last run)
python .github/release-notes.py

# Refresh (ignore state, recompute all)
python .github/release-notes.py --refresh
```

## Common Options

| Option | Default | Description |
|--------|---------|-------------|
| `--output` | `CHANGELOG.md` | Output file path |
| `--resetAll` | `false` | Rebuild from scratch (overwrite file) |
| `--refresh` | `false` | Ignore state and recompute from tags |
| `--maxHighlights` | `6` | Max commits shown per type section |
| `--includeFiles` | `true` | Show changed files per commit |
| `--includeSnippets` | `false` | Include code diff snippets |
| `--contentsMode` | `api-changes` | Content analysis mode |
| `--pathInclude` | `**/*.{h,hpp,c,cpp},...` | Glob patterns to include |
| `--pathExclude` | `` | Glob patterns to exclude |

## Content Modes

- **`api-changes`**: Detect C/C++ API/ABI changes (function signatures, classes)
- **`diff-hunks`**: Show diff context for each commit
- **`added-lines`**: Show only added lines
- **`removed-lines`**: Show only removed lines

## State Management

The script persists the last processed commit SHA in `.github/release-notes.state.json`:

```json
{
  "lastProcessedSha": "abc123...",
  "branch": "develop",
  "updatedAt": "2025-10-22T06:54:17.804096Z",
  "outputFile": "CHANGELOG.md"
}
```

### Incremental Workflow

1. First run (no state): Processes all tags → writes full CHANGELOG
2. Subsequent runs: Only processes commits since `lastProcessedSha` → updates Unreleased section
3. After release (new tag): Next run will move Unreleased commits into the new tag section

### Force Full Rebuild

```powershell
# Delete state and regenerate from scratch
python .github/release-notes.py --resetAll

# Or delete state manually
Remove-Item .github/release-notes.state.json
python .github/release-notes.py
```

## Advanced Examples

### Show detailed snippets

```powershell
python .github/release-notes.py --includeSnippets true --snippetLimit 500 --contentsMode diff-hunks
```

### Filter by path (only core sources)

```powershell
python .github/release-notes.py --pathInclude "include/**/*.h,src/**/*.cpp" --pathExclude "**/test/**"
```

### More commits per section

```powershell
python .github/release-notes.py --maxHighlights 20
```

## Integration with CI/CD

Add to `.github/workflows/release.yml`:

```yaml
- name: Generate changelog
  run: |
    python .github/release-notes.py --refresh
    git add CHANGELOG.md .github/release-notes.state.json
    git diff --cached --quiet || git commit -m "docs: update CHANGELOG [skip ci]"
```

## Implementation Details

Implements `.github/prompts/release-notes.prompt.md`:

- Tag filtering with regex patterns
- Conventional Commit parsing
- Diff content analysis with configurable limits
- C/C++ language-aware heuristics:
  - Function signature detection in headers
  - Class/struct declaration detection
  - Build system change detection (CMake, vcpkg, Conan)
- Managed block updates (`<!-- BEGIN/END AUTO-RELEASE-NOTES -->`)
- GitHub compare URL generation

## Troubleshooting

### "Already up-to-date" but I want to regenerate

```powershell
python .github/release-notes.py --refresh
```

### Commits on different branch not appearing

The state file tracks the branch. If you switch branches, the script will do a full rebuild. To force:

```powershell
python .github/release-notes.py --resetAll
```

### Want to exclude version bump commits

```powershell
python .github/release-notes.py --skipTypes chore
```

## Dependencies

- Python 3.7+
- git (on PATH)
- Standard library only (no pip dependencies)
