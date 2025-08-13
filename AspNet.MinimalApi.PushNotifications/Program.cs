using AspNet.MinimalApi.PushNotifications.Models;
using AspNet.MinimalApi.PushNotifications.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Додаємо сервіси
builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Налаштовуємо middleware
app.UseCors();
app.UseStaticFiles();

// API Endpoints
app.MapGet("/", () => Results.Redirect("/index.html"));

// Отримати VAPID публічний ключ
app.MapGet("/api/vapid-public-key", (IPushNotificationService pushService) =>
{
    var publicKey = pushService.GetVapidPublicKey();
    return Results.Ok(new { publicKey });
});

// Підписатися на нотифікації
app.MapPost("/api/subscribe", async (IPushNotificationService pushService, PushSubscription subscription) =>
{
    try
    {
        await pushService.SubscribeAsync(subscription);
        return Results.Ok(new { message = "Підписка успішно створена!" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Відписатися від нотифікацій
app.MapPost("/api/unsubscribe", async (IPushNotificationService pushService, [FromBody] UnsubscribeRequest request) =>
{
    try
    {
        await pushService.UnsubscribeAsync(request.Endpoint);
        return Results.Ok(new { message = "Підписка успішно видалена!" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Відправити нотифікацію всім підписникам
app.MapPost("/api/send-notification", async (IPushNotificationService pushService, SendNotificationRequest request) =>
{
    try
    {
        var payload = new NotificationPayload
        {
            Title = request.Title,
            Body = request.Body,
            Icon = request.Icon ?? "/icon-192x192.svg",
            Badge = "/badge-72x72.svg",
            Data = new Dictionary<string, object>
            {
                ["url"] = request.Url ?? "/"
            }
        };

        await pushService.SendNotificationAsync(payload);
        var subscriptionCount = pushService.GetSubscriptionCount();
        return Results.Ok(new {
            message = $"Нотифікацію відправлено {subscriptionCount} підписникам!",
            sentTo = subscriptionCount
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Відправити тестову нотифікацію (симуляція)
app.MapPost("/api/send-test-notification", async (SendNotificationRequest request) =>
{
    try
    {
        // Симулюємо успішну відправку нотифікації
        await Task.Delay(500); // Імітуємо затримку мережі

        return Results.Ok(new {
            message = "Тестова нотифікація 'відправлена' (симуляція)",
            notification = new {
                title = request.Title,
                body = request.Body,
                url = request.Url,
                timestamp = DateTime.UtcNow
            }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Отримати статистику підписок
app.MapGet("/api/subscriptions/stats", (IPushNotificationService pushService) =>
{
    return Results.Ok(new
    {
        count = pushService.GetSubscriptionCount(),
        subscriptions = pushService.GetAllSubscriptions().Select(s => new { endpoint = s.Endpoint })
    });
});

app.Run();

public class UnsubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
}