namespace AspNet.MinimalApi.PushNotifications.Models;

public class PushSubscription
{
    public string Endpoint { get; set; } = string.Empty;
    public PushKeys Keys { get; set; } = new();
}

public class PushKeys
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class NotificationPayload
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? Tag { get; set; }
    public bool RequireInteraction { get; set; } = false;
    public bool Silent { get; set; } = false;
    public int[]? Vibrate { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public NotificationAction[]? Actions { get; set; }
}

public class NotificationAction
{
    public string Action { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
}
