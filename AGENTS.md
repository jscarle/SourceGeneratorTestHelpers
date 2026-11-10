# Agent Instructions

## Release Intent, Versioning, and Workflow

Treat explicit release requests—such as “Mint a new release,” “Release this,” “Publish this as a new release,” “Create a new release,” or any clear equivalent—as authorization to carry out SourceGeneratorTestHelpers' full release workflow, including committing, pushing, tagging, and publishing as needed.

If the user has not clearly requested a release, or if the appropriate change category or package scope cannot be determined confidently, ask for clarification **before** committing, pushing, creating a tag, or publishing a release.

Only change package, assembly, and file versions when the user has clearly requested that a release be published. Do not bump versions during ordinary implementation, refactoring, bug-fixing, dependency updates, testing, commits, pushes, or pull-request preparation unless those actions are part of an explicitly requested release.

Version each affected package independently using these repository-specific rules:

- Minor, backward-compatible changes increment the affected package's patch component. Example: `10.1.2` → `10.1.3`.
- Moderate or breaking changes increment the affected package's middle component and reset its patch. Example: `10.1.2` → `10.2.0`.
- Increment an affected package's major component and reset the other components only when its newest supported .NET version changes. Example: `10.1.2` → `11.0.0`.
- Breaking changes alone do not justify a major-version increment in this repository. If multiple categories apply to a package, use the highest applicable category.

The packable projects are `SourceGeneratorTestHelpers`, `SourceGeneratorTestHelpers.XUnit`, `SourceGeneratorTestHelpers.NUnit`, and `SourceGeneratorTestHelpers.MSTest` under `src/`. Update only the affected projects, using three components for `Version` and matching four-component `AssemblyVersion` and `FileVersion` values. Use a lightweight `vX.Y.Z` tag for a repository-wide or multi-package release. `XUnit-vX.Y.Z` is the established prefix for an XUnit-only release; if another single-package release needs a scoped tag and no convention exists, ask before inventing a prefix. Some historical tags are annotated, but new release tags must be lightweight.

A clear release request authorizes this complete workflow:

1. Inspect the worktree, current branch, `origin`, `develop`, all package versions, recent commits, pull requests, tags, and releases before editing metadata.
2. Identify the packages included in the release, determine each next version, update only their project version properties, and verify that the intended tag and release do not already exist.
3. Validate the release-scoped changes using the repository's normal restore, Release build, test, and pack commands across all supported target frameworks.
4. Commit all release-scoped changes on the reusable `feature/work` branch using a concise past-tense sentence ending in a period, then push `feature/work`.
5. Open a ready pull request from `feature/work` to `develop` using recent title and body conventions, enable squash auto-merge, and monitor checks and review state.
6. Fix and push any issue that prevents the pull request from merging, then wait until it is merged.
7. Fast-forward local `develop` from `origin/develop` and reset local `feature/work` to that latest `develop` commit. The remote feature branch may be deleted automatically after merge.
8. Create the selected lightweight tag on the latest `develop` commit and push that tag to `origin`.
9. Publish a non-draft GitHub release named `<tag> - Description`, using concise release-note bullets consistent with the selected package scope.
10. Monitor the release-triggered `Publish` workflow until every newly versioned NuGet package is published successfully; unchanged package versions are expected to be skipped as duplicates.

Do not require separate confirmation for each Git or GitHub step after an unambiguous release command. Report the package versions, pull request, merge commit, tag, GitHub release URL, and NuGet publishing result when complete.
