using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.MicrosoftTeamsWebhookConnector.Internal;

namespace OutSystems.MicrosoftTeamsWebhookConnector
{
    /// <summary>
    /// Implementation of the Microsoft Teams Webhook Connector.
    /// Optimized for high-performance messaging using JSON Source Generation.
    /// </summary>
    public sealed class MicrosoftTeamsWebhookConnector : IMicrosoftTeamsWebhookConnector
    {
        // High-performance HttpClient using SocketsHttpHandler for connection pooling and lifecycle management
        private static readonly HttpClient _httpClient = new(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 20
        });

        public TeamsResponse SendSimpleMessage(string webhookUrl, string message)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) throw new ArgumentException("Webhook URL is required.");
            
            var payload = new SimplePayload { Text = message };
            return PostToTeamsAsync(webhookUrl, payload, TeamsJsonContext.Default.SimplePayload).GetAwaiter().GetResult();
        }

        public TeamsResponse SendAlert(string webhookUrl, TeamsAlert alert)
        {
            var color = (alert.Status ?? string.Empty).ToLowerInvariant() switch
            {
                "error" => "Attention",
                "warning" => "Warning",
                "success" => "Good",
                _ => "Accent"
            };

            var card = new TeamsCard
            {
                Title = new TeamsTextConfig { Text = alert.Title },
                Subtitle = new TeamsTextConfig { Text = alert.Status?.ToUpperInvariant() ?? "INFO" },
                Text = alert.Message,
                ActionUrl = alert.ActionUrl,
                ActionLabel = "View Details"
            };

            return SendAdaptiveCardInternal(webhookUrl, card, color);
        }

        public TeamsResponse SendAdaptiveCard(string webhookUrl, TeamsCard card)
        {
            return SendAdaptiveCardInternal(webhookUrl, card, "Accent");
        }

        public TeamsResponse SendAnnouncement(string webhookUrl, TeamsAnnouncement announcement)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) throw new ArgumentException("Webhook URL is required.");

            var adaptiveCard = new AdaptiveCard();

            // Background Image handling
            if (!string.IsNullOrWhiteSpace(announcement.BackgroundImageUrl))
            {
                adaptiveCard.BackgroundImage = new BackgroundImage { Url = announcement.BackgroundImageUrl };
            }

            // Primary Content
            AddTextConfigToBody(adaptiveCard, announcement.Title, "ExtraLarge", "Bolder");
            AddTextConfigToBody(adaptiveCard, announcement.Subtitle, "Medium", "Default", true);

            if (!string.IsNullOrWhiteSpace(announcement.Text))
            {
                adaptiveCard.Body.Add(new TextBlock { Text = announcement.Text, Wrap = true, Spacing = "Large" });
            }

            // Primary Action
            if (!string.IsNullOrWhiteSpace(announcement.ActionUrl))
            {
                adaptiveCard.Actions.Add(new AdaptiveAction 
                { 
                    Title = string.IsNullOrWhiteSpace(announcement.ActionLabel) ? "View Details" : announcement.ActionLabel, 
                    Url = announcement.ActionUrl 
                });
            }

            var payload = new TeamsPayload();
            payload.Attachments.Add(new TeamsAttachment { Content = adaptiveCard });

            return PostToTeamsAsync(webhookUrl, payload, TeamsJsonContext.Default.TeamsPayload).GetAwaiter().GetResult();
        }

        public TeamsResponse SendListCard(string webhookUrl, TeamsListCard listCard)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) throw new ArgumentException("Webhook URL is required.");

            var adaptiveCard = new AdaptiveCard();

            // Card Header
            AddTextConfigToBody(adaptiveCard, listCard.Title, "Large", "Bolder", false, "Accent");

            // List Items
            if (listCard.Items != null)
            {
                foreach (var item in listCard.Items)
                {
                    var columnSet = new ColumnSet();
                    
                    // Leading Image/Thumbnail
                    if (!string.IsNullOrWhiteSpace(item.Image.Url))
                    {
                        columnSet.Columns.Add(new Column
                        {
                            Width = "auto",
                            Items = new List<AdaptiveElement>
                            {
                                new Image
                                {
                                    Url = item.Image.Url,
                                    Size = string.IsNullOrWhiteSpace(item.Image.Size) ? "Medium" : item.Image.Size
                                }
                            }
                        });
                    }

                    // Multi-line Text Content
                    var textItems = new List<AdaptiveElement>();
                    
                    if (!string.IsNullOrWhiteSpace(item.Title.Text))
                    {
                        textItems.Add(new TextBlock 
                        { 
                            Text = item.Title.Text, 
                            Weight = string.IsNullOrWhiteSpace(item.Title.Weight) ? "Bolder" : item.Title.Weight, 
                            Size = string.IsNullOrWhiteSpace(item.Title.Size) ? "Default" : item.Title.Size,
                            Color = item.Title.Color,
                            Wrap = true
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(item.Subtitle.Text))
                    {
                        textItems.Add(new TextBlock 
                        { 
                            Text = item.Subtitle.Text, 
                            Weight = string.IsNullOrWhiteSpace(item.Subtitle.Weight) ? "Default" : item.Subtitle.Weight,
                            Size = string.IsNullOrWhiteSpace(item.Subtitle.Size) ? "Small" : item.Subtitle.Size, 
                            Color = item.Subtitle.Color,
                            IsSubtle = true, 
                            Wrap = true, 
                            Spacing = "None" 
                        });
                    }

                    columnSet.Columns.Add(new Column { Width = "stretch", Items = textItems });

                    // Clickable item behavior
                    if (!string.IsNullOrWhiteSpace(item.ActionUrl))
                    {
                        columnSet.SelectAction = new AdaptiveAction { Url = item.ActionUrl };
                    }

                    adaptiveCard.Body.Add(columnSet);
                }
            }

            // Footer Action
            if (!string.IsNullOrWhiteSpace(listCard.ActionUrl))
            {
                adaptiveCard.Actions.Add(new AdaptiveAction 
                { 
                    Title = string.IsNullOrWhiteSpace(listCard.ActionLabel) ? "View More" : listCard.ActionLabel, 
                    Url = listCard.ActionUrl 
                });
            }

            var payload = new TeamsPayload();
            payload.Attachments.Add(new TeamsAttachment { Content = adaptiveCard });

            return PostToTeamsAsync(webhookUrl, payload, TeamsJsonContext.Default.TeamsPayload).GetAwaiter().GetResult();
        }

        private TeamsResponse SendAdaptiveCardInternal(string webhookUrl, TeamsCard card, string accentColor)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) throw new ArgumentException("Webhook URL is required.");

            var adaptiveCard = new AdaptiveCard();
            var msteamsMetadata = new MsTeamsMetadata();

            // Header Layout
            AddTextConfigToBody(adaptiveCard, card.Title, "Large", "Bolder", false, accentColor);
            AddTextConfigToBody(adaptiveCard, card.Subtitle, "Small", "Default", true);

            // Card Image
            if (!string.IsNullOrWhiteSpace(card.Image.Url))
            {
                adaptiveCard.Body.Add(new Image
                {
                    Url = card.Image.Url,
                    Size = string.IsNullOrWhiteSpace(card.Image.Size) ? "Large" : card.Image.Size
                });
            }

            // Body and Mentions logic
            var bodyText = new StringBuilder(card.Text ?? string.Empty);
            if (card.Mentions != null && card.Mentions.Count > 0)
            {
                if (bodyText.Length > 0) bodyText.Append("\n\n");
                
                foreach (var mention in card.Mentions)
                {
                    bodyText.Append($"<at>{mention.Name}</at> ");
                    msteamsMetadata.Entities.Add(new MentionEntity
                    {
                        Text = $"<at>{mention.Name}</at>",
                        Mentioned = new MentionedUser
                        {
                            Id = mention.Id,
                            Name = mention.Name
                        }
                    });
                }
            }

            if (bodyText.Length > 0)
            {
                adaptiveCard.Body.Add(new TextBlock { Text = bodyText.ToString() });
            }

            // FactSet Data Grid
            if (card.Facts != null && card.Facts.Count > 0)
            {
                var factSet = new FactSet();
                foreach (var fact in card.Facts)
                {
                    factSet.Facts.Add(new AdaptiveFact { Title = fact.Label, Value = fact.Value });
                }
                adaptiveCard.Body.Add(factSet);
            }

            // Action Button
            if (!string.IsNullOrWhiteSpace(card.ActionUrl))
            {
                adaptiveCard.Actions.Add(new AdaptiveAction 
                { 
                    Title = string.IsNullOrWhiteSpace(card.ActionLabel) ? "View Details" : card.ActionLabel, 
                    Url = card.ActionUrl 
                });
            }

            // Adaptive Card Metadata (Mentions)
            if (msteamsMetadata.Entities.Count > 0)
            {
                adaptiveCard.MSUtils = msteamsMetadata;
            }

            var payload = new TeamsPayload();
            payload.Attachments.Add(new TeamsAttachment { Content = adaptiveCard });

            return PostToTeamsAsync(webhookUrl, payload, TeamsJsonContext.Default.TeamsPayload).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Centralized text rendering with default handling.
        /// </summary>
        private static void AddTextConfigToBody(AdaptiveCard card, TeamsTextConfig config, string defaultSize, string defaultWeight, bool isSubtle = false, string defaultColor = "")
        {
            if (string.IsNullOrWhiteSpace(config.Text)) return;

            card.Body.Add(new TextBlock 
            { 
                Text = config.Text, 
                Size = string.IsNullOrWhiteSpace(config.Size) ? defaultSize : config.Size,
                Weight = string.IsNullOrWhiteSpace(config.Weight) ? defaultWeight : config.Weight,
                Color = string.IsNullOrWhiteSpace(config.Color) ? defaultColor : config.Color,
                IsSubtle = isSubtle,
                Spacing = isSubtle ? "None" : null
            });
        }

        /// <summary>
        /// Optimized POST method using JSON Source Generation for low allocations and peak performance.
        /// </summary>
        private async Task<TeamsResponse> PostToTeamsAsync<T>(string url, T payload, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(url, payload, typeInfo).ConfigureAwait(false);
                
                if (response.IsSuccessStatusCode)
                {
                    return new TeamsResponse { Success = true, ErrorMessage = string.Empty };
                }

                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new TeamsResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"{(int)response.StatusCode} {response.ReasonPhrase}: {errorContent}" 
                };
            }
            catch (HttpRequestException ex)
            {
                return new TeamsResponse { Success = false, ErrorMessage = $"Network Error: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new TeamsResponse { Success = false, ErrorMessage = $"Unexpected Error: {ex.Message}" };
            }
        }
    }
}

namespace OutSystems.MicrosoftTeamsWebhookConnector.Internal
{
    // High-performance DTOs for Microsoft Teams Adaptive Cards Payload
    internal class TeamsPayload
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "message";
        [JsonPropertyName("attachments")] public List<TeamsAttachment> Attachments { get; set; } = new();
    }

    internal class TeamsAttachment
    {
        [JsonPropertyName("contentType")] public string ContentType { get; set; } = "application/vnd.microsoft.card.adaptive";
        [JsonPropertyName("content")] public AdaptiveCard Content { get; set; } = new();
    }

    internal class AdaptiveCard
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "AdaptiveCard";
        [JsonPropertyName("version")] public string Version { get; set; } = "1.2";
        [JsonPropertyName("body")] public List<AdaptiveElement> Body { get; set; } = new();
        [JsonPropertyName("actions")] public List<AdaptiveAction> Actions { get; set; } = new();
        [JsonPropertyName("msteams")] public MsTeamsMetadata? MSUtils { get; set; }
        [JsonPropertyName("backgroundImage")] public BackgroundImage? BackgroundImage { get; set; }
    }

    internal class BackgroundImage
    {
        [JsonPropertyName("url")] public string? Url { get; set; }
        [JsonPropertyName("fillMode")] public string FillMode { get; set; } = "Cover";
    }

    internal class MsTeamsMetadata
    {
        [JsonPropertyName("entities")] public List<MentionEntity> Entities { get; set; } = new();
    }

    internal class MentionEntity
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "mention";
        [JsonPropertyName("text")] public string? Text { get; set; }
        [JsonPropertyName("mentioned")] public MentionedUser? Mentioned { get; set; }
    }

    internal class MentionedUser
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
    }

    // Support for Polymorphic JSON serialization
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextBlock), "TextBlock")]
    [JsonDerivedType(typeof(Image), "Image")]
    [JsonDerivedType(typeof(FactSet), "FactSet")]
    [JsonDerivedType(typeof(ColumnSet), "ColumnSet")]
    [JsonDerivedType(typeof(Column), "Column")]
    internal abstract class AdaptiveElement { }

    internal class TextBlock : AdaptiveElement
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
        [JsonPropertyName("weight")] public string? Weight { get; set; }
        [JsonPropertyName("size")] public string? Size { get; set; }
        [JsonPropertyName("color")] public string? Color { get; set; }
        [JsonPropertyName("isSubtle")] public bool? IsSubtle { get; set; }
        [JsonPropertyName("wrap")] public bool Wrap { get; set; } = true;
        [JsonPropertyName("spacing")] public string? Spacing { get; set; }
    }

    internal class Image : AdaptiveElement
    {
        [JsonPropertyName("url")] public string? Url { get; set; }
        [JsonPropertyName("size")] public string? Size { get; set; }
        [JsonPropertyName("style")] public string? Style { get; set; }
    }

    internal class FactSet : AdaptiveElement
    {
        [JsonPropertyName("facts")] public List<AdaptiveFact> Facts { get; set; } = new();
    }

    internal class ColumnSet : AdaptiveElement
    {
        [JsonPropertyName("columns")] public List<Column> Columns { get; set; } = new();
        [JsonPropertyName("selectAction")] public AdaptiveAction? SelectAction { get; set; }
    }

    internal class Column : AdaptiveElement
    {
        [JsonPropertyName("width")] public string? Width { get; set; }
        [JsonPropertyName("items")] public List<AdaptiveElement> Items { get; set; } = new();
    }

    internal class AdaptiveFact
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("value")] public string? Value { get; set; }
    }

    internal class AdaptiveAction
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "Action.OpenUrl";
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("url")] public string? Url { get; set; }
    }

    internal class SimplePayload
    {
        [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;
    }

    // JSON Source Generation Context for peak performance and AOT compatibility
    [JsonSourceGenerationOptions(
        WriteIndented = false, 
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
    [JsonSerializable(typeof(TeamsPayload))]
    [JsonSerializable(typeof(SimplePayload))]
    [JsonSerializable(typeof(AdaptiveCard))]
    [JsonSerializable(typeof(TextBlock))]
    [JsonSerializable(typeof(Image))]
    [JsonSerializable(typeof(ColumnSet))]
    [JsonSerializable(typeof(Column))]
    [JsonSerializable(typeof(FactSet))]
    [JsonSerializable(typeof(AdaptiveFact))]
    [JsonSerializable(typeof(AdaptiveAction))]
    [JsonSerializable(typeof(TeamsAttachment))]
    [JsonSerializable(typeof(MsTeamsMetadata))]
    [JsonSerializable(typeof(MentionEntity))]
    [JsonSerializable(typeof(MentionedUser))]
    internal partial class TeamsJsonContext : JsonSerializerContext { }
}
