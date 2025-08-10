# SMS Authentication API with Native Identity + JWT Hybrid

Приклад реалізації SMS аутентифікації з використанням **нативних можливостей ASP.NET Core Identity**, **JWT в HttpOnly cookies** та Minimal API.

## 🎯 Ключові особливості

- ✅ **Нативний ASP.NET Core Identity** - використання готових рішень
- ✅ **JWT в HttpOnly cookies** - hybrid підхід для SPA
- ✅ **SMS-only passwordless логін** - вхід тільки через SMS код
- ✅ **Традиційний password логін** - класичний вхід з паролем
- ✅ **SMS підтвердження телефону** - через вбудовані методи
- ✅ **Двофакторна аутентифікація** - нативна підтримка 2FA
- ✅ **Recovery коди** - автоматична генерація
- ✅ **Account lockout** - захист від брутфорсу
- ✅ **SPA-friendly** - ідеально для React, Vue, Angular
- ✅ **Захист від XSS** - HttpOnly cookies
- ✅ **Minimal API endpoints** - сучасний підхід до API

## 🏗️ Архітектура

```
AspNet.MinimalApi.AuthSmsIdentity/
├── Data/
│   └── ApplicationDbContext.cs    # IdentityDbContext
├── Services/
│   └── SmsSender.cs              # ISmsSender реалізація
├── Models/
│   └── AuthModels.cs             # DTO для API
├── Endpoints/
│   └── AuthEndpoints.cs          # Minimal API endpoints
└── Program.cs                    # Конфігурація Identity
```

## 🔧 Налаштування

### Основні компоненти

1. **IdentityUser** - стандартна модель користувача
2. **UserManager<IdentityUser>** - управління користувачами
3. **SignInManager<IdentityUser>** - управління входом
4. **ISmsSender** - відправка SMS (demo реалізація)

### Конфігурація Identity

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Налаштування паролів
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;

    // Налаштування користувача
    options.User.AllowedUserNameCharacters = "0123456789+()-";

    // Налаштування блокування
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### Налаштування SMS провайдерів

Проект підтримує два режими відправки SMS:

1. **Demo режим** (для розробки):
   ```json
   // appsettings.Development.json
   {
     "SmsSettings": {
       "Provider": "Demo"
     }
   }
   ```
   SMS коди виводяться в консоль сервера.

2. **Twilio режим** (для production):
   ```json
   // appsettings.json
   {
     "SmsSettings": {
       "Provider": "Twilio"
     },
     "TwilioSettings": {
       "AccountSid": "your-twilio-account-sid",
       "AuthToken": "your-twilio-auth-token",
       "FromPhoneNumber": "+1234567890"
     }
   }
   ```
   Реальна відправка SMS через Twilio API.

## 🔄 JWT + Cookies Hybrid підхід

### Чому це найкраще для SPA?

**Проблеми localStorage:**
- ❌ Доступний для JavaScript (XSS атаки)
- ❌ Не відправляється автоматично
- ❌ Потрібно ручне управління

**Переваги JWT в HttpOnly cookies:**
- ✅ **Безпека** - HttpOnly захищає від XSS
- ✅ **Автоматичність** - відправляється з кожним запитом
- ✅ **SPA-friendly** - працює з `fetch({ credentials: 'include' })`
- ✅ **Stateless** - JWT містить всю інформацію
- ✅ **CORS** - підтримка cross-domain запитів

### Frontend приклади:

**React/Vue/Angular:**
```javascript
// Автоматично відправляє JWT cookie
const response = await fetch('/api/auth/me', {
  credentials: 'include'  // Важливо!
});

// Логін
await fetch('/api/auth/sms-login', {
  method: 'POST',
  credentials: 'include',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ phoneNumber: '+380501234567', code: '123456' })
});
```

**Axios налаштування:**
```javascript
axios.defaults.withCredentials = true;

// Або для конкретного запиту
axios.get('/api/auth/me', { withCredentials: true });
```

## 🚀 Запуск

1. **Клонування та встановлення:**
   ```bash
   cd AspNet.MinimalApi.AuthSmsIdentity
   dotnet restore
   dotnet run
   ```

2. **Відкрийте браузер:**
   - Swagger UI: `http://localhost:5091`

## 🧪 Тестування

### Автоматичні тести

**Demo режим** (SMS коди в консолі):
```bash
bash test-native-identity.sh
```

**JWT + Cookies Hybrid** (найкращий для SPA):
```bash
bash test-jwt-hybrid.sh
```

**Twilio режим** (реальні SMS):
```bash
bash test-twilio-identity.sh
```
⚠️ Для Twilio режиму потрібно налаштувати Twilio в `appsettings.json`

### Ручне тестування через Swagger

1. **Реєстрація:** `POST /api/auth/register`
2. **Підтвердження SMS:** `POST /api/auth/send-phone-confirmation`
3. **Підтвердження коду:** `POST /api/auth/confirm-phone`
4. **Увімкнення 2FA:** `POST /api/auth/enable-2fa-sms`
5. **Генерація recovery кодів:** `POST /api/auth/generate-recovery-codes`

## 📋 API Endpoints

### Основні операції
- `POST /api/auth/register` - Реєстрація користувача (JWT в cookie)
- `POST /api/auth/login` - Традиційний вхід (password + phone)
- `POST /api/auth/logout` - Вихід користувача (очищення JWT cookie)
- `GET /api/auth/me` - Інформація про користувача

### SMS-only логін (passwordless)
- `POST /api/auth/send-sms-login-code` - Відправка SMS коду для входу
- `POST /api/auth/sms-login` - Вхід тільки через SMS код

### SMS та 2FA
- `POST /api/auth/send-phone-confirmation` - Відправка SMS коду
- `POST /api/auth/confirm-phone` - Підтвердження телефону
- `POST /api/auth/enable-2fa-sms` - Увімкнення 2FA
- `POST /api/auth/disable-2fa` - Вимкнення 2FA
- `POST /api/auth/generate-recovery-codes` - Генерація recovery кодів

## 🔒 Безпека

### Вбудовані можливості Identity:
- **Хешування паролів** - автоматично через Identity
- **Account lockout** - захист від брутфорсу
- **Token validation** - для SMS кодів
- **Cookie security** - HttpOnly, Secure, SameSite
- **Recovery codes** - для відновлення доступу

### Налаштування cookies:
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
```

## 💡 Переваги нативного підходу

### ✅ Мінімум кастомного коду
- Використання готових методів Identity
- Автоматичне управління сесіями
- Вбудована валідація та безпека

### ✅ Готові рішення
- `UserManager.GenerateChangePhoneNumberTokenAsync()` - генерація SMS кодів
- `UserManager.ChangePhoneNumberAsync()` - підтвердження телефону
- `UserManager.SetTwoFactorEnabledAsync()` - управління 2FA
- `UserManager.GenerateNewTwoFactorRecoveryCodesAsync()` - recovery коди

### ✅ Автоматичні функції
- Валідація паролів
- Блокування акаунтів
- Управління токенами
- Логування подій

## 🔄 Порівняння з кастомним підходом

| Аспект | Native Identity | Кастомний підхід |
|--------|----------------|------------------|
| **Код** | Мінімум | Багато кастомного коду |
| **Безпека** | Вбудована | Потрібно реалізовувати |
| **Тестування** | Готове | Потрібно писати тести |
| **Підтримка** | Microsoft | Власна |
| **Гнучкість** | Обмежена | Повна |
| **Швидкість розробки** | Висока | Низька |

## 🛠️ Розширення

### Додавання реального SMS провайдера:
```csharp
public class TwilioSmsSender : ISmsSender
{
    public async Task SendSmsAsync(string number, string message)
    {
        // Інтеграція з Twilio
        await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(_fromNumber),
            to: new PhoneNumber(number)
        );
    }
}
```

### Додавання ролей:
```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddRoles<IdentityRole>();

// Використання в endpoints
.RequireAuthorization("AdminPolicy");
```

## 📊 Результат тестування

```json
{
  "success": true,
  "message": "User information retrieved",
  "data": {
    "id": "3514b646-b0a3-4086-bca7-d98f9f112038",
    "phoneNumber": "+380501234567",
    "phoneNumberConfirmed": true,
    "twoFactorEnabled": true
  }
}
```

**Recovery коди:**
```
5CKHB-Y89VM, 29PBF-5Q3YK, 5H7WB-QTD2Y, KM2PF-29TF4, NJHQ3-B7297
VN7FT-YX36M, K6R6M-HMR2M, BHV4B-2FX3W, FTB4K-KDQVP, V4889-J3CMK
```

## 🎯 Висновок

Цей проект демонструє, як максимально використовувати нативні можливості ASP.NET Core Identity для SMS аутентифікації. Підхід забезпечує:

- **Швидку розробку** - мінімум кастомного коду
- **Високу безпеку** - перевірені рішення Microsoft
- **Легку підтримку** - стандартні методи та практики
- **Масштабованість** - готова архітектура для розширення

Ідеально підходить для проектів, де потрібна надійна аутентифікація без складної кастомізації.
