# Contributing

Thanks for taking the time to contribute. This document covers what you need to know to get a change in.

## Before you start

- **Open an issue first** for anything beyond a small fix. A two-line description of the problem and the proposed direction prevents wasted effort if the change isn't a fit.
- **Bug reports** should include: ODC platform version, the action you called, the structure values you passed (redact secrets), the `TeamsResponse.ErrorMessage` you got back, and what you expected.
- **Feature requests** should explain the use case in Teams terms — what card or behavior should the user see — not just the API shape.

## Development setup

Requirements:
- .NET 10 SDK
- A Microsoft Teams channel with an Incoming Webhook URL you can post to (a personal sandbox tenant is ideal).

Build and run a local check:

```bash
dotnet restore
dotnet build
```

Produce the upload artifact:

```bash
dotnet publish -c Release -o publish_out
zip -j MicrosoftTeamsWebhookConnector.zip \
    publish_out/MicrosoftTeamsWebhookConnector.dll \
    publish_out/MicrosoftTeamsWebhookConnector.pdb
```

Upload the resulting zip to ODC Portal's External Logic page to validate end-to-end.

## Code conventions

- **Target stays at .NET 10** to match what ODC accepts.
- **Public surface lives in `IMSTeamsWebhookProvider.cs`.** Every public type carries `[OSStructure]` / `[OSStructureField]` / `[OSAction]` / `[OSParameter]` attributes with a `Description`. ODC reads those descriptions verbatim — keep them clear and complete.
- **Internal Adaptive Card DTOs stay `internal`** and live in the `OutSystems.MicrosoftTeamsWebhookConnector.Internal` namespace. They are serialized via `TeamsJsonContext` (source-generated). If you add a new internal type, register it in the `[JsonSerializable]` list on `TeamsJsonContext`.
- **Don't introduce reflection-based JSON.** Add new types to the source-gen context instead.
- **`HttpClient` is shared and static.** Don't create new instances per call.
- **Validate inputs at the public boundary** (e.g. webhook URL non-empty). Internal helpers can trust their inputs.
- **Nullable reference types are enabled.** Honor the annotations.

## Naming

- OS-facing structure fields use `PascalCase` and read naturally in OutSystems (`ActionUrl`, not `Action_URL`).
- Property descriptions should describe *what the user sees* in Teams, not the underlying Adaptive Card primitive.

## Testing changes

There is no automated test project yet. Until there is, every change must be validated by:

1. Building and publishing the zip.
2. Uploading to ODC Portal and confirming validation passes.
3. Calling the affected action(s) from a real ODC app against a Teams webhook and visually verifying the rendered card.

If you add an automated test project, prefer integration tests that exercise the JSON output of each action against a captured-payload baseline.

## Pull requests

- One logical change per PR.
- Update `README.md` if you add or rename a public action or structure.
- Update structure `Description` text if behavior changes.
- Don't commit `bin/`, `obj/`, `publish_out/`, or `.zip` files — `.gitignore` covers these.
- Keep the commit history clean (rebase if needed). Conventional commit prefixes (`feat:`, `fix:`, `docs:`, `refactor:`) are appreciated but not required.

## What's out of scope

- Anything that requires authentication beyond an Incoming Webhook URL (Graph API, bot framework registration). That belongs in a separate connector.
- Card layouts that aren't expressible in Adaptive Cards 1.2 — Teams' webhook renderer is pinned to that version.

## Reporting security issues

Don't open a public issue for security problems. Email the maintainer directly with details and a reproduction.
