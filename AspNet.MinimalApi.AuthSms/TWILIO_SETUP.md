# Налаштування Twilio для SMS аутентифікації

Цей документ описує, як налаштувати реальну відправку SMS через Twilio.

## 1. Створення Twilio акаунту

1. Перейдіть на [twilio.com](https://www.twilio.com)
2. Зареєструйтеся або увійдіть в існуючий акаунт
3. Перейдіть в Console Dashboard

## 2. Отримання облікових даних

### Account SID та Auth Token
1. В Console Dashboard знайдіть секцію "Account Info"
2. Скопіюйте:
   - **Account SID** (починається з "AC...")
   - **Auth Token** (натисніть "Show" щоб побачити)

### Номер телефону для відправки
1. Перейдіть в розділ "Phone Numbers" → "Manage" → "Active numbers"
2. Якщо у вас немає номера:
   - Натисніть "Buy a number"
   - Виберіть країну та тип номера
   - Переконайтеся, що номер підтримує SMS
   - Придбайте номер

## 3. Налаштування проекту

### Оновлення appsettings.json

```json
{
  "SmsSettings": {
    "Provider": "Twilio"
  },
  "TwilioSettings": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your-auth-token-here",
    "FromPhoneNumber": "+1234567890",
    "EnableLogging": true,
    "TimeoutSeconds": 30
  }
}
```

### Використання змінних середовища (рекомендовано для production)

Замість зберігання облікових даних в appsettings.json, використовуйте змінні середовища:

```bash
# Linux/macOS
export TWILIO_ACCOUNT_SID="ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
export TWILIO_AUTH_TOKEN="your-auth-token-here"
export TWILIO_FROM_PHONE_NUMBER="+1234567890"

# Windows
set TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
set TWILIO_AUTH_TOKEN=your-auth-token-here
set TWILIO_FROM_PHONE_NUMBER=+1234567890
```

Потім оновіть appsettings.json:

```json
{
  "TwilioSettings": {
    "AccountSid": "",
    "AuthToken": "",
    "FromPhoneNumber": ""
  }
}
```

І додайте в Program.cs:

```csharp
// Читання з змінних середовища
builder.Configuration.AddEnvironmentVariables();

// Або явно встановити значення
builder.Services.Configure<TwilioSettings>(options =>
{
    options.AccountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID") ?? 
                        builder.Configuration["TwilioSettings:AccountSid"];
    options.AuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? 
                       builder.Configuration["TwilioSettings:AuthToken"];
    options.FromPhoneNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_PHONE_NUMBER") ?? 
                             builder.Configuration["TwilioSettings:FromPhoneNumber"];
});
```

## 4. Тестування

### Перемикання між Demo та Twilio

В `appsettings.Development.json`:
```json
{
  "SmsSettings": {
    "Provider": "Demo"
  }
}
```

В `appsettings.json` (production):
```json
{
  "SmsSettings": {
    "Provider": "Twilio"
  }
}
```

### Тестування через API

1. Запустіть додаток:
   ```bash
   dotnet run
   ```

2. Відправте запит на реєстрацію:
   ```bash
   curl -X POST "http://localhost:5186/api/auth/send-registration-code" \
     -H "Content-Type: application/json" \
     -d '{"phoneNumber": "+380501234567", "purpose": "Registration"}'
   ```

3. Перевірте, що SMS прийшов на вказаний номер

## 5. Обробка помилок

### Типові помилки та їх вирішення

1. **"The number +... is unverified"**
   - В trial акаунті можна відправляти SMS тільки на верифіковані номери
   - Додайте номер в "Verified Caller IDs" або оновіть акаунт

2. **"Invalid 'From' phone number"**
   - Переконайтеся, що номер в форматі E.164 (+1234567890)
   - Номер повинен бути придбаний в Twilio

3. **"Authentication Error"**
   - Перевірте правильність Account SID та Auth Token
   - Переконайтеся, що токен не застарів

## 6. Моніторинг та логування

### Перегляд логів відправки

Twilio надає детальні логи в Console:
1. Перейдіть в "Monitor" → "Logs" → "Messaging"
2. Переглядайте статус кожного повідомлення

### Налаштування webhook'ів (опціонально)

Для отримання статусів доставки:

1. В Twilio Console перейдіть в "Messaging" → "Settings"
2. Додайте webhook URL: `https://yourdomain.com/api/webhooks/twilio/status`
3. Реалізуйте endpoint для обробки статусів

## 7. Вартість

- **Trial акаунт**: $15.50 кредиту безкоштовно
- **SMS в США/Канаді**: ~$0.0075 за повідомлення
- **SMS міжнародні**: від $0.05 за повідомлення
- **Номер телефону**: $1/місяць

## 8. Безпека

### Рекомендації:
- Ніколи не зберігайте Auth Token в коді
- Використовуйте HTTPS для всіх запитів
- Регулярно ротуйте Auth Token
- Налаштуйте IP whitelist в Twilio Console
- Моніторьте використання для виявлення зловживань

### Приклад безпечної конфігурації:

```csharp
public class TwilioSmsService : ISmsService
{
    public TwilioSmsService(IOptions<TwilioSettings> options, ILogger<TwilioSmsService> logger)
    {
        var settings = options.Value;
        
        // Валідація конфігурації
        if (string.IsNullOrEmpty(settings.AccountSid) || 
            string.IsNullOrEmpty(settings.AuthToken) ||
            string.IsNullOrEmpty(settings.FromPhoneNumber))
        {
            throw new InvalidOperationException("Twilio configuration is incomplete");
        }
        
        // Ініціалізація з таймаутом
        TwilioClient.Init(settings.AccountSid, settings.AuthToken);
        
        logger.LogInformation("TwilioSmsService initialized successfully");
    }
}
```

## 9. Альтернативи Twilio

Якщо Twilio не підходить, можна використати:
- **AWS SNS** - інтеграція з AWS екосистемою
- **Azure Communication Services** - для Azure проектів  
- **Vonage (Nexmo)** - альтернативний провайдер
- **MessageBird** - європейський провайдер

Архітектура проекту дозволяє легко додати нові провайдери через реалізацію `ISmsService`.
