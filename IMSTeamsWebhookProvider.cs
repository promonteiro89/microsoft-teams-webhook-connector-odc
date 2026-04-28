using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.MicrosoftTeamsWebhookConnector
{
    [OSStructure(Description = "A key-value pair used to display data in a grid layout (FactSet).")]
    public struct TeamsFact
    {
        [OSStructureField(Description = "The label or key (e.g., 'Status').")]
        public string Label { get; set; }
        
        [OSStructureField(Description = "The value associated with the label (e.g., 'Active').")]
        public string Value { get; set; }
    }

    [OSStructure(Description = "Defines a user to be @mentioned in a message.")]
    public struct TeamsMention
    {
        [OSStructureField(Description = "The display name of the user.")]
        public string Name { get; set; }

        [OSStructureField(Description = "The user's unique identifier (Entra ID Object ID or Email).")]
        public string Id { get; set; }
    }

    [OSStructure(Description = "Standard response containing the result of the webhook operation.")]
    public struct TeamsResponse
    {
        [OSStructureField(Description = "True if the message was accepted by the server.")]
        public bool Success { get; set; }

        [OSStructureField(Description = "Detailed error message or status code in case of failure.")]
        public string ErrorMessage { get; set; }
    }

    [OSStructure(Description = "Configuration for styling and content of text elements.")]
    public struct TeamsTextConfig
    {
        [OSStructureField(Description = "The text content to display.")]
        public string Text { get; set; }

        [OSStructureField(Description = "Optional: Text size (Small, Default, Medium, Large, ExtraLarge).")]
        public string Size { get; set; }

        [OSStructureField(Description = "Optional: Font weight (Lighter, Default, Bolder).")]
        public string Weight { get; set; }

        [OSStructureField(Description = "Optional: Text color (Default, Dark, Light, Accent, Good, Warning, Attention).")]
        public string Color { get; set; }
    }

    [OSStructure(Description = "Configuration for an image element in a Teams card.")]
    public struct TeamsImageConfig
    {
        [OSStructureField(Description = "The full URL of the image.")]
        public string Url { get; set; }

        [OSStructureField(Description = "Optional: Size of the image (Small, Medium, Large, Stretch).")]
        public string Size { get; set; }

        [OSStructureField(Description = "Optional: Style of the image (Default, Person).")]
        public string Style { get; set; }
    }

    [OSStructure(Description = "Parameters for sending a styled alert notification.")]
    public struct TeamsAlert
    {
        [OSStructureField(Description = "The heading of the alert.")]
        public string Title { get; set; }

        [OSStructureField(Description = "The main message body.")]
        public string Message { get; set; }

        [OSStructureField(Description = "The alert level (e.g., Success, Warning, Error, Info).")]
        public string Status { get; set; }

        [OSStructureField(Description = "Optional: Target URL for the action button.")]
        public string ActionUrl { get; set; }
    }

    [OSStructure(Description = "Parameters for a high-impact announcement with a background banner.")]
    public struct TeamsAnnouncement
    {
        [OSStructureField(Description = "Configuration for the primary header.")]
        public TeamsTextConfig Title { get; set; }

        [OSStructureField(Description = "Configuration for the secondary sub-header.")]
        public TeamsTextConfig Subtitle { get; set; }

        [OSStructureField(Description = "Full URL for the large banner image background.")]
        public string BackgroundImageUrl { get; set; }

        [OSStructureField(Description = "The primary descriptive text.")]
        public string Text { get; set; }

        [OSStructureField(Description = "Optional: URL for the primary action button.")]
        public string ActionUrl { get; set; }

        [OSStructureField(Description = "Optional: Label for the action button.")]
        public string ActionLabel { get; set; }
    }

    [OSStructure(Description = "An individual entry within a List Card.")]
    public struct TeamsListItem
    {
        [OSStructureField(Description = "The item title configuration.")]
        public TeamsTextConfig Title { get; set; }

        [OSStructureField(Description = "The item subtitle or description configuration.")]
        public TeamsTextConfig Subtitle { get; set; }

        [OSStructureField(Description = "Optional: Image configuration for the item.")]
        public TeamsImageConfig Image { get; set; }

        [OSStructureField(Description = "Optional: URL to navigate to when the item is clicked.")]
        public string ActionUrl { get; set; }
    }

    [OSStructure(Description = "Parameters for a summary card displaying multiple items.")]
    public struct TeamsListCard
    {
        [OSStructureField(Description = "The main card title configuration.")]
        public TeamsTextConfig Title { get; set; }

        [OSStructureField(Description = "The collection of items to display.")]
        public List<TeamsListItem> Items { get; set; }

        [OSStructureField(Description = "Optional: Label for the footer action button.")]
        public string ActionLabel { get; set; }

        [OSStructureField(Description = "Optional: URL for the footer action button.")]
        public string ActionUrl { get; set; }
    }

    [OSStructure(Description = "Parameters for a versatile Adaptive Card message.")]
    public struct TeamsCard
    {
        [OSStructureField(Description = "Configuration for the main title.")]
        public TeamsTextConfig Title { get; set; }

        [OSStructureField(Description = "Configuration for the subtle subtitle shown below the title.")]
        public TeamsTextConfig Subtitle { get; set; }

        [OSStructureField(Description = "The main body content.")]
        public string Text { get; set; }

        [OSStructureField(Description = "Optional: Configuration for the card's image.")]
        public TeamsImageConfig Image { get; set; }

        [OSStructureField(Description = "Optional: URL for the primary button.")]
        public string ActionUrl { get; set; }

        [OSStructureField(Description = "Optional: Label for the primary button.")]
        public string ActionLabel { get; set; }
        
        [OSStructureField(Description = "Optional: Key-value facts to display in a grid.")]
        public List<TeamsFact> Facts { get; set; }

        [OSStructureField(Description = "Optional: Users to @mention in the message.")]
        public List<TeamsMention> Mentions { get; set; }
    }

    [OSInterface(Description = "High-performance connector for sending rich content to Microsoft Teams via Webhooks.", Name = "MicrosoftTeamsWebhookConnector", IconResourceName = "OutSystems.MicrosoftTeamsWebhookConnector.Resources.microsoft_teams_webhook_connector.png")]
    public interface IMicrosoftTeamsWebhookConnector
    {
        [OSAction(Description = "Sends a versatile Adaptive Card supporting images, facts, and mentions.", IconResourceName = "OutSystems.MicrosoftTeamsWebhookConnector.Resources.microsoft_teams_action.png")]
        TeamsResponse SendAdaptiveCard(
            [OSParameter(Description = "The incoming webhook URL provided by Teams.")] string webhookUrl, 
            [OSParameter(Description = "The structured card content.")] TeamsCard card);

        [OSAction(Description = "Sends a summary list card, optimized for displaying multiple related updates.", IconResourceName = "OutSystems.MicrosoftTeamsWebhookConnector.Resources.microsoft_teams_action.png")]
        TeamsResponse SendListCard(
            [OSParameter(Description = "The incoming webhook URL provided by Teams.")] string webhookUrl, 
            [OSParameter(Description = "The structured list content.")] TeamsListCard listCard);

        [OSAction(Description = "Sends a high-impact announcement card with a large background banner.", IconResourceName = "OutSystems.MicrosoftTeamsWebhookConnector.Resources.microsoft_teams_action.png")]
        TeamsResponse SendAnnouncement(
            [OSParameter(Description = "The incoming webhook URL provided by Teams.")] string webhookUrl, 
            [OSParameter(Description = "The announcement content.")] TeamsAnnouncement announcement);

        [OSAction(Description = "Sends a styled alert notification (Success, Warning, Error, Info).", IconResourceName = "OutSystems.MicrosoftTeamsWebhookConnector.Resources.microsoft_teams_action.png")]
        TeamsResponse SendAlert(
            [OSParameter(Description = "The incoming webhook URL provided by Teams.")] string webhookUrl, 
            [OSParameter(Description = "The alert details.")] TeamsAlert alert);
        
        [OSAction(Description = "Sends a lightweight plain text or markdown message.", IconResourceName = "OutSystems.MicrosoftTeamsWebhookConnector.Resources.microsoft_teams_action.png")]
        TeamsResponse SendSimpleMessage(
            [OSParameter(Description = "The incoming webhook URL provided by Teams.")] string webhookUrl, 
            [OSParameter(Description = "The message text.")] string message);
    }
}
