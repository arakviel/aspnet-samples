using WebPush;
using AspNet.MinimalApi.PushNotifications.Models;
using System.Text.Json;
using System.Collections.Concurrent;
using WebPushSubscription = WebPush.PushSubscription;
using AppPushSubscription = AspNet.MinimalApi.PushNotifications.Models.PushSubscription;

namespace AspNet.MinimalApi.PushNotifications.Services;

public interface IPushNotificationService
{
    string GetVapidPublicKey();
    Task<string> GenerateVapidKeysAsync();
    Task SubscribeAsync(AppPushSubscription subscription);
    Task UnsubscribeAsync(string endpoint);
    Task SendNotificationAsync(NotificationPayload payload);
    Task SendNotificationToSubscriberAsync(string endpoint, NotificationPayload payload);
    IEnumerable<AppPushSubscription> GetAllSubscriptions();
    int GetSubscriptionCount();
}

public class PushNotificationService : IPushNotificationService
{
    private readonly WebPushClient _webPushClient;
    private readonly VapidDetails _vapidDetails;
    private readonly ConcurrentDictionary<string, AppPushSubscription> _subscriptions;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
    {
        _logger = logger;
        _subscriptions = new ConcurrentDictionary<string, AppPushSubscription>();
        
        // Генеруємо нові VAPID ключі для кожного запуску (в продакшені зберігайте їх постійно)
        var vapidKeys = VapidHelper.GenerateVapidKeys();
        var publicKey = configuration["Vapid:PublicKey"] ?? vapidKeys.PublicKey;
        var privateKey = configuration["Vapid:PrivateKey"] ?? vapidKeys.PrivateKey;
        var subject = configuration["Vapid:Subject"] ?? "mailto:admin@example.com";

        _logger.LogInformation("Generated VAPID keys - Public: {PublicKey}", publicKey);

        _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        _webPushClient = new WebPushClient();
    }

    public string GetVapidPublicKey()
    {
        return _vapidDetails.PublicKey;
    }

    public Task<string> GenerateVapidKeysAsync()
    {
        var vapidKeys = VapidHelper.GenerateVapidKeys();
        var result = JsonSerializer.Serialize(new
        {
            PublicKey = vapidKeys.PublicKey,
            PrivateKey = vapidKeys.PrivateKey
        });
        return Task.FromResult(result);
    }

    public Task SubscribeAsync(AppPushSubscription subscription)
    {
        if (string.IsNullOrEmpty(subscription.Endpoint))
        {
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(subscription));
        }

        _subscriptions.AddOrUpdate(subscription.Endpoint, subscription, (key, oldValue) => subscription);
        _logger.LogInformation("New subscription added: {Endpoint}", subscription.Endpoint);
        
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
        }

        _subscriptions.TryRemove(endpoint, out _);
        _logger.LogInformation("Subscription removed: {Endpoint}", endpoint);
        
        return Task.CompletedTask;
    }

    public async Task SendNotificationAsync(NotificationPayload payload)
    {
        var tasks = _subscriptions.Values.Select(subscription => 
            SendNotificationToSubscriberAsync(subscription.Endpoint, payload));
        
        await Task.WhenAll(tasks);
    }

    public async Task SendNotificationToSubscriberAsync(string endpoint, NotificationPayload payload)
    {
        if (!_subscriptions.TryGetValue(endpoint, out var subscription))
        {
            _logger.LogWarning("Subscription not found for endpoint: {Endpoint}", endpoint);
            return;
        }

        try
        {
            var pushSubscription = new WebPushSubscription(
                subscription.Endpoint,
                subscription.Keys.P256dh,
                subscription.Keys.Auth);

            var payloadJson = JsonSerializer.Serialize(payload);

            await _webPushClient.SendNotificationAsync(pushSubscription, payloadJson, _vapidDetails);
            _logger.LogInformation("Notification sent successfully to: {Endpoint}", endpoint);
        }
        catch (WebPushException ex)
        {
            _logger.LogError(ex, "Failed to send notification to: {Endpoint}. Status: {StatusCode}, Message: {Message}",
                endpoint, ex.StatusCode, ex.Message);

            // Видаляємо недійсні підписки
            if (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                ex.StatusCode == System.Net.HttpStatusCode.NotFound ||
                ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Removing invalid subscription: {Endpoint}", endpoint);
                await UnsubscribeAsync(endpoint);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending notification to: {Endpoint}", endpoint);
        }
    }

    public IEnumerable<AppPushSubscription> GetAllSubscriptions()
    {
        return _subscriptions.Values.ToList();
    }

    public int GetSubscriptionCount()
    {
        return _subscriptions.Count;
    }
}
