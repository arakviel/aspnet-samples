# ASP.NET Core JWT Authentication - Офіційний підхід

Приклад JWT аутентифікації з використанням **вбудованих можливостей ASP.NET Core**.

## 🎯 Особливості

- ✅ **Microsoft.AspNetCore.Authentication.JwtBearer** - офіційний пакет
- ✅ **TokenValidationParameters** - автоматична валідація
- ✅ **Authorization Policies** - гнучка система авторизації
- ✅ **Claims-based Authorization** - сучасний підхід
- ✅ **Role-based Authorization** - традиційні ролі
- ✅ **JwtBearerEvents** - логування та debugging
- ✅ **Dependency Injection** - повна інтеграція з DI

## 🏗️ Архітектура

### Налаштування JWT аутентифікації:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });
```

### Authorization Policies:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("ManagerOrAdmin", policy => 
        policy.RequireRole("Manager", "Admin"));
});
```

### Захищені ендпоінти:
```csharp
// Базова аутентифікація
app.MapGet("/protected", [Authorize] (ClaimsPrincipal user) => { ... });

// Політика авторизації
app.MapGet("/admin", [Authorize(Policy = "AdminOnly")] (ClaimsPrincipal user) => { ... });

// Role-based авторизація
app.MapGet("/user-only", [Authorize(Roles = "User")] (ClaimsPrincipal user) => { ... });
```

## 🚀 Запуск

```bash
cd AspNet.MinimalApi.JwtAuth
dotnet run
```

Додаток буде доступний на `http://localhost:5000`

## 📋 API Ендпоінти

### Публічні:
| Ендпоінт | Метод | Опис |
|----------|-------|------|
| `/` | GET | Головна сторінка |
| `/public` | GET | Публічний ресурс |
| `/auth/login` | POST | Вхід в систему |
| `/auth/register` | POST | Реєстрація |
| `/auth/status` | GET | Статус аутентифікації |

### Захищені:
| Ендпоінт | Авторизація | Опис |
|----------|-------------|------|
| `/auth/profile` | `[Authorize]` | Профіль користувача |
| `/auth/token-info` | `[Authorize]` | Інформація про токен |
| `/protected` | `[Authorize]` | Захищений ресурс |
| `/admin` | `[Authorize(Policy = "AdminOnly")]` | Тільки адміністратори |
| `/manager` | `[Authorize(Policy = "ManagerOrAdmin")]` | Менеджери та адміністратори |
| `/user-only` | `[Authorize(Roles = "User")]` | Тільки звичайні користувачі |

## 👥 Тестові користувачі

- **admin** / **admin123** (роль: Admin)
- **manager** / **manager123** (роль: Manager)  
- **user** / **user123** (роль: User)

## 🔍 Ключові відмінності від кастомної реалізації

| Аспект | Кастомна реалізація | ASP.NET Core |
|--------|---------------------|--------------|
| **Валідація токенів** | Ручна реалізація | Автоматична |
| **Middleware** | Кастомний middleware | Вбудований JwtBearerMiddleware |
| **Авторизація** | Ручна перевірка ролей | Policies + Attributes |
| **Логування** | Ручне логування | Вбудовані JwtBearerEvents |
| **Конфігурація** | Кастомні опції | TokenValidationParameters |
| **DI інтеграція** | Ручна реєстрація | Повна інтеграція |

## 🧪 Тестування

Використайте файл `test-aspnet.http`:

```bash
# Логін
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'

# Захищений ресурс
curl http://localhost:5000/protected \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 🎓 Для студентів

### Переваги вбудованого підходу:

1. **Автоматична валідація**: Не потрібно писати код валідації
2. **Стандартизація**: Використовує загальноприйняті практики
3. **Безпека**: Перевірені та протестовані алгоритми
4. **Підтримка**: Офіційна підтримка Microsoft
5. **Інтеграція**: Повна інтеграція з ASP.NET Core

### Коли використовувати:

- **Продакшен додатки** - надійність та безпека
- **Корпоративні системи** - стандартизація
- **Команди розробників** - зрозумілість
- **Довгострокові проекти** - підтримка

### Коли НЕ використовувати:

- **Навчальні цілі** - не видно внутрішньої роботи
- **Специфічні вимоги** - обмежена кастомізація
- **Мікросервіси** - може бути надмірним

## 🔧 Конфігурація

### appsettings.json:
```json
{
  "JwtSettings": {
    "SecretKey": "MyAspNetCoreJwtSecretKeyForDemonstrationPurposes123456789",
    "Issuer": "AspNet.MinimalApi.JwtAuth",
    "Audience": "AspNet.MinimalApi.JwtAuth.Users",
    "ExpirationMinutes": 60
  }
}
```

### TokenValidationParameters:
```csharp
new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,    // Перевіряти підпис
    ValidateIssuer = true,              // Перевіряти видавця
    ValidateAudience = true,            // Перевіряти аудиторію
    ValidateLifetime = true,            // Перевіряти термін дії
    ClockSkew = TimeSpan.FromMinutes(5) // Допустиме відхилення часу
}
```

## 🔒 Безпека

### Автоматичні перевірки:
- ✅ Підпис токена (HMAC SHA-256)
- ✅ Термін дії (exp claim)
- ✅ Видавець (iss claim)
- ✅ Аудиторія (aud claim)
- ✅ Час початку дії (nbf claim)

### Логування:
```csharp
options.Events = new JwtBearerEvents
{
    OnAuthenticationFailed = context => { /* Логування помилок */ },
    OnTokenValidated = context => { /* Логування успішної валідації */ }
};
```

## 📊 Порівняння підходів

### Простий кастомний (SimpleJwtAuth):
- 👍 Легко зрозуміти
- 👎 Багато ручної роботи

### Кастомний з повним функціоналом (CustomJwtAuth):
- 👍 Повний контроль
- 👎 Складність підтримки

### ASP.NET Core (цей проект):
- 👍 Продакшен готовність
- 👍 Офіційна підтримка
- 👎 Менше навчальної цінності

## 💡 Висновок

**ASP.NET Core JWT Authentication** - це **стандартний підхід** для продакшен додатків. Він забезпечує:

- 🔒 **Безпеку** - перевірені алгоритми
- 🚀 **Продуктивність** - оптимізована реалізація  
- 🛠️ **Зручність** - мінімум коду
- 📈 **Масштабованість** - готове для enterprise

Використовуйте цей підхід для **реальних проектів**! 🎯
