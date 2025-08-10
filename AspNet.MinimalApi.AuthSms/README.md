# SMS Authentication API

Приклад реалізації аутентифікації через SMS з використанням ASP.NET Core Minimal API, Identity та JWT токенів.

## Особливості

- ✅ SMS аутентифікація для реєстрації та входу
- ✅ ASP.NET Core Identity для управління користувачами
- ✅ JWT токени для авторизації
- ✅ Minimal API endpoints
- ✅ Swagger/OpenAPI документація
- ✅ Валідація та обмеження швидкості запитів
- ✅ Безпечне зберігання паролів
- ✅ Структурована архітектура з сервісами

## Архітектура

```
├── Models/
│   ├── ApplicationUser.cs      # Модель користувача (розширює IdentityUser)
│   └── SmsVerificationCode.cs  # Модель SMS кодів верифікації
├── DTOs/
│   └── AuthDtos.cs            # DTO для API запитів/відповідей
├── Data/
│   └── ApplicationDbContext.cs # Entity Framework DbContext
├── Services/
│   ├── ISmsService.cs         # Інтерфейс SMS сервісу
│   ├── SmsService.cs          # Demo SMS сервіс (фейк)
│   ├── TwilioSmsService.cs    # Реальний Twilio SMS сервіс
│   ├── SmsServiceFactory.cs   # Фабрика для вибору SMS провайдера
│   ├── IAuthService.cs        # Інтерфейс аутентифікації
│   └── AuthService.cs         # Логіка аутентифікації
├── Configuration/
│   └── TwilioSettings.cs      # Конфігурація для Twilio
├── Endpoints/
│   └── AuthEndpoints.cs       # Minimal API endpoints
└── Program.cs                 # Конфігурація додатку
```

## API Endpoints

### 1. Відправка коду для реєстрації
```http
POST /api/auth/send-registration-code
Content-Type: application/json

{
  "phoneNumber": "+380501234567",
  "purpose": "Registration"
}
```

### 2. Реєстрація користувача
```http
POST /api/auth/register
Content-Type: application/json

{
  "phoneNumber": "+380501234567",
  "code": "123456",
  "firstName": "Іван",
  "lastName": "Петренко",
  "password": "SecurePassword123"
}
```

### 3. Відправка коду для входу
```http
POST /api/auth/send-login-code
Content-Type: application/json

{
  "phoneNumber": "+380501234567",
  "purpose": "Login"
}
```

### 4. Вхід користувача
```http
POST /api/auth/login
Content-Type: application/json

{
  "phoneNumber": "+380501234567",
  "code": "123456"
}
```

### 5. Отримання інформації про користувача
```http
GET /api/auth/me
Authorization: Bearer <jwt-token>
```

### 6. Вихід
```http
POST /api/auth/logout
Authorization: Bearer <jwt-token>
```

## Запуск проекту

1. **Клонування та встановлення залежностей:**
```bash
cd AspNet.MinimalApi.AuthSms
dotnet restore
```

2. **Запуск додатку:**
```bash
dotnet run
```

3. **Відкрийте браузер:**
   - Swagger UI: `https://localhost:5001` або `http://localhost:5000`
   - API доступне за адресою: `https://localhost:5001/api/auth`

## Конфігурація

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "MyVeryLongSecretKeyForJWTTokenGeneration123456789",
    "Issuer": "AspNet.MinimalApi.AuthSms",
    "Audience": "AspNet.MinimalApi.AuthSms.Client",
    "ExpirationMinutes": 60
  },
  "SmsSettings": {
    "Provider": "Twilio",
    "ApplicationName": "AuthApp",
    "CodeExpirationMinutes": 5,
    "MaxAttemptsPerPeriod": 3,
    "RateLimitPeriodMinutes": 10
  },
  "TwilioSettings": {
    "AccountSid": "your-twilio-account-sid",
    "AuthToken": "your-twilio-auth-token",
    "FromPhoneNumber": "+1234567890",
    "EnableLogging": true,
    "TimeoutSeconds": 30
  }
}
```

### Налаштування SMS провайдера

Проект підтримує два режими відправки SMS:

1. **Demo режим** (для розробки):
   ```json
   {
     "SmsSettings": {
       "Provider": "Demo"
     }
   }
   ```
   SMS коди виводяться в консоль сервера.

2. **Twilio режим** (для production):
   ```json
   {
     "SmsSettings": {
       "Provider": "Twilio"
     }
   }
   ```
   Реальна відправка SMS через Twilio API.

📖 **Детальна інструкція з налаштування Twilio**: [TWILIO_SETUP.md](TWILIO_SETUP.md)

## Безпека

### Реалізовані заходи безпеки:
- ✅ Хешування паролів через Identity
- ✅ JWT токени з підписом
- ✅ Валідація номерів телефонів
- ✅ Обмеження кількості запитів SMS (3 за 10 хвилин)
- ✅ Термін дії SMS кодів (5 хвилин)
- ✅ Одноразове використання SMS кодів
- ✅ HTTPS для production

### Рекомендації для production:
- 🔄 Використовуйте реальну базу даних (PostgreSQL, SQL Server)
- 🔄 Інтегруйте реальний SMS провайдер (Twilio, AWS SNS)
- 🔄 Додайте Redis для кешування та rate limiting
- 🔄 Налаштуйте логування та моніторинг
- 🔄 Додайте refresh токени з базою даних
- 🔄 Використовуйте HTTPS сертифікати
- 🔄 Додайте CORS політики для production

## Тестування

### 1. Автоматичний тестовий скрипт

**Demo режим** (SMS коди в консолі):
```bash
bash test-api.sh
```

**Twilio режим** (реальні SMS):
```bash
bash test-twilio.sh
```
⚠️ Для Twilio режиму потрібно спочатку налаштувати Twilio (див. [TWILIO_SETUP.md](TWILIO_SETUP.md))

### 2. Postman Collection
Імпортуйте файл `SMS-Auth-API.postman_collection.json` в Postman для інтерактивного тестування.

### 3. Приклад тестування через curl:

1. **Відправка коду реєстрації:**
```bash
curl -X POST "http://localhost:5186/api/auth/send-registration-code" \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber": "+380501234567", "purpose": "Registration"}'
```

2. **Реєстрація (використовуйте код з відповіді):**
```bash
curl -X POST "http://localhost:5186/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+380501234567",
    "code": "819608",
    "firstName": "Тест",
    "lastName": "Користувач",
    "password": "TestPassword123"
  }'
```

### 4. Результат тестування
```json
{
  "success": true,
  "message": "Registration successful.",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "2dXmLzAmsVPP60E0UiH9Dsd2wLkENcFzs9Z+Vnff4gg=",
  "expiresAt": "2025-08-10T10:07:07.8731389Z",
  "user": {
    "id": "d2973de5-45ef-4ff4-a948-9db033d1d90a",
    "phoneNumber": "+380501234567",
    "firstName": "Тест",
    "lastName": "Користувач",
    "createdAt": "2025-08-10T09:07:07.708641Z",
    "lastLoginAt": "2025-08-10T09:07:07.8664904Z"
  }
}
```

## Розширення

Для розширення функціональності можна додати:
- Двофакторну аутентифікацію (2FA)
- Соціальні мережі (Google, Facebook)
- Email верифікацію
- Відновлення паролю через SMS
- Адмін панель
- Аудит логи
- Геолокацію для безпеки

## Ліцензія

MIT License
