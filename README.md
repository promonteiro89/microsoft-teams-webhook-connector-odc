[![Platform](https://img.shields.io/badge/Platform-OutSystems_ODC-red.svg)](https://www.outsystems.com/odc/)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# Microsoft Teams Webhook Connector for OutSystems Developer Cloud

A .NET 8 external logic library for **OutSystems Developer Cloud (ODC)** that sends rich, structured messages to Microsoft Teams via Incoming Webhooks. It exposes five purpose-built actions covering everything from quick text pings to high-impact announcement banners — all powered by Adaptive Cards under the hood.

No raw JSON. No boilerplate. Just typed structures you can drop into your ODC logic.

## Why this connector

Microsoft Teams accepts Adaptive Card payloads, but assembling them by hand in OutSystems means string concatenation, escaping, and a lot of trial and error. This library wraps the most common card patterns into typed OS structures so you can:

- Send styled alerts (Success / Warning / Error / Info) without writing a card schema.
- Post announcements with banner backgrounds and call-to-action buttons.
- Render summary lists of items, each with its own thumbnail, title, subtitle, and link.
- @mention users by Entra ID — properly registered with Teams' `msteams.entities` metadata, not just text.
- Get back a clean `TeamsResponse` with `Success` and `ErrorMessage` fields for observability.

## Actions

| Action | Use it for |
| --- | --- |
| `SendSimpleMessage` | Quick plain-text or Markdown ping. |
| `SendAlert` | Color-coded status notification (DevOps, monitoring, incident updates). |
| `SendAdaptiveCard` | Standard rich card: title, subtitle, image, body, facts grid, mentions, action button. |
| `SendListCard` | Multi-item summary (daily digests, release notes, batched updates). |
| `SendAnnouncement` | High-impact card with a banner background image. |

## Structures

| Structure | Purpose |
| --- | --- |
| `TeamsCard` | Input for `SendAdaptiveCard`. Title, subtitle, body text, optional image, action button, facts, mentions. |
| `TeamsListCard` | Input for `SendListCard`. Header plus a list of `TeamsListItem`. |
| `TeamsListItem` | One row in a list card: title, subtitle, optional image, optional click-through URL. |
| `TeamsAnnouncement` | Input for `SendAnnouncement`. Title, subtitle, banner URL, body, action. |
| `TeamsAlert` | Input for `SendAlert`. Title, message, status (Success/Warning/Error/Info), action URL. |
| `TeamsTextConfig` | Reusable text styling: text, size, weight, color. |
| `TeamsImageConfig` | Reusable image config: URL and size. |
| `TeamsFact` | Key-value pair for the facts grid. |
| `TeamsMention` | A user to @mention by display name and Entra ID (or email). |
| `TeamsResponse` | Output of every action: `Success` (bool) and `ErrorMessage` (string). |

## Performance notes

- **Zero-reflection serialization** via `System.Text.Json` source generation (`JsonSerializerContext`) — AOT-friendly, low allocation.
- **Pooled `HttpClient`** built on `SocketsHttpHandler` with a 2-minute connection lifetime and 20 connections per server.
- Actions are synchronous on the OutSystems side but use `async` HTTP under the hood.

## Build

Requirements:
- .NET 8 SDK
- The `OutSystems.ExternalLibraries.SDK` NuGet package (already referenced in the `.csproj`).

```bash
dotnet publish -c Release -o publish_out
zip -j MicrosoftTeamsWebhookConnector.zip \
    publish_out/MicrosoftTeamsWebhookConnector.dll \
    publish_out/MicrosoftTeamsWebhookConnector.pdb
```

The resulting `MicrosoftTeamsWebhookConnector.zip` is what you upload to the ODC Portal as an External Logic dependency.

> The OutSystems SDK assembly and `deps.json` are intentionally excluded — ODC supplies them at runtime.

## Use in ODC

1. Upload `MicrosoftTeamsWebhookConnector.zip` in the **External Logic** section of ODC Portal.
2. Wait for validation, then publish the dependency.
3. In ODC Studio, consume the connector and call the actions, passing the relevant structures.
4. Pass the **incoming webhook URL** from your Teams channel as the first argument of every action.

To get a webhook URL, in Microsoft Teams: channel → **⋯** → *Workflows* → *Post to a channel when a webhook request is received*. Copy the generated URL.

## Project layout

```
.
├── IMSTeamsWebhookProvider.cs    # Public interface + OS structures
├── MSTeamsWebhookProvider.cs     # Implementation + internal Adaptive Card DTOs
├── MSTeamsWebhookProvider.csproj # .NET 8 project file
├── Resources/                    # Action and connector icons (embedded)
├── README.md
└── CONTRIBUTING.md
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE) © Paulo Ricardo Oliveira Monteiro
