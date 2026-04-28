# Product Updates Provider (ODC)

A high-performance .NET 8 external logic component for OutSystems Developer Cloud (ODC) designed to extract product updates from web portals.

## Features

- **Stream-Based Parsing**: Uses `HtmlAgilityPack` with stream-based loading to maintain a low memory footprint.
- **Robust URL Normalization**: Automatically handles absolute, relative, and protocol-relative URLs, forcing HTTPS for secure delivery.
- **Efficient Metadata Extraction**: Captures structured data including titles, multi-paragraph content, and images.

## Project Structure

- `IProductUpdateService.cs`: Defines the `ProductUpdate` structure and the `IOSProductUpdatesProvider` interface.
- `ProductUpdateService.cs`: Implementation of the high-speed scraper.
- `OutSystemsProductUpdates.csproj`: .NET 8 project configuration.

## Build and Package

To prepare for ODC upload:
```bash
dotnet publish -c Release -f net8.0 -o ./publish
cd publish
zip -r OSProductUpdatesProvider.zip .
# Add source files for ODC validation
cd ..
zip -u OSProductUpdatesProvider.zip IProductUpdateService.cs ProductUpdateService.cs OutSystemsProductUpdates.csproj README.md Resources/*
```

## Usage

Pass a target `DateTime` and the `URL` of the updates portal. The service will return a collection of `ProductUpdate` structures matching that date.

### Output Structure
- **Title**: String
- **Content**: Multi-line description.
- **ImageUrl**: Normalized absolute HTTPS URL.
- **Url**: Canonical link to the full update.
- **PublishDate**: The date the update was released.
