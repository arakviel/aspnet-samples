# 2FA Authentication API with Native Identity

Приклад реалізації **двофакторної аутентифікації через Email та TOTP** з використанням **максимально готових рішень ASP.NET Core Identity**.

## 🎯 Ключові особливості

- ✅ **Native ASP.NET Core Identity** - використання готових рішень
- ✅ **Email 2FA codes** - коди через email (демо в консолі)
- ✅ **TOTP Authenticator** - Google Authenticator, Authy, тощо
- ✅ **QR код генерація** - для легкого налаштування
- ✅ **Recovery коди** - автоматична генерація
- ✅ **Account lockout** - захист від брутфорсу
- ✅ **Мінімум кастомного коду** - максимум готових рішень

## 🏗️ Архітектура

```
AspNet.MinimalApi.Auth2Factors/
├── Data/
│   └── ApplicationDbContext.cs    # IdentityDbContext
├── Services/
│   └── EmailSender.cs             # IEmailSender (демо)
├── Models/
│   └── AuthModels.cs              # DTO для API
├── Endpoints/
│   └── AuthEndpoints.cs           # Minimal API endpoints
└── Program.cs                     # Конфігурація Identity + 2FA
```

## 🔧 Налаштування Identity для 2FA

### Основні компоненти

1. **IdentityUser** - стандартна модель користувача
2. **UserManager<IdentityUser>** - управління користувачами та 2FA
3. **SignInManager<IdentityUser>** - управління входом та 2FA
4. **IEmailSender** - відправка email кодів

### Конфігурація 2FA

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Налаштування паролів
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    
    // ===== НАЛАШТУВАННЯ 2FA =====
    // Identity автоматично налаштовує TOTP провайдери
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Додає TOTP, Email, Phone провайдери
```

## 🚀 Запуск

1. **Клонування та встановлення:**
   ```bash
   cd AspNet.MinimalApi.Auth2Factors
   dotnet restore
   dotnet run
   ```

2. **Відкрийте браузер:**
   - Swagger UI: `http://localhost:5018`
   - API Info: `http://localhost:5018/info`

## 🧪 Тестування

### Автоматичний тест
```bash
bash test-2fa.sh
```

### Ручне тестування через Swagger

1. **Реєстрація:** `POST /api/auth/register`
2. **Вхід:** `POST /api/auth/login` (спочатку без 2FA)
3. **Налаштування TOTP:** `POST /api/auth/setup-totp`
4. **Підтвердження TOTP:** `POST /api/auth/verify-totp`
5. **Повторний вхід:** тепер потрібна 2FA!
6. **Підтвердження 2FA:** `POST /api/auth/verify-2fa`

## 📋 API Endpoints

### Основні операції
- `POST /api/auth/register` - Реєстрація користувача
- `POST /api/auth/login` - Вхід (може вимагати 2FA)
- `POST /api/auth/verify-2fa` - Підтвердження 2FA коду
- `POST /api/auth/logout` - Вихід користувача
- `GET /api/auth/me` - Інформація про користувача

### TOTP управління
- `POST /api/auth/setup-totp` - Налаштування TOTP authenticator
- `POST /api/auth/verify-totp` - Підтвердження та увімкнення TOTP
- `POST /api/auth/disable-2fa` - Вимкнення 2FA

### Recovery коди
- `POST /api/auth/generate-recovery-codes` - Генерація recovery кодів
- `POST /api/auth/use-recovery-code` - Вхід через recovery код

## 🔒 Як працює 2FA з нативним Identity

### 1. TOTP (Time-based One-Time Password)

**Налаштування:**
```csharp
// Identity автоматично генерує унікальний ключ для TOTP
var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
if (string.IsNullOrEmpty(unformattedKey))
{
    await userManager.ResetAuthenticatorKeyAsync(user);
    unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
}
```

**Валідація:**
```csharp
// Identity автоматично валідує TOTP код
var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
    user, userManager.Options.Tokens.AuthenticatorTokenProvider, code);
```

### 2. Email 2FA

**Відправка коду:**
```csharp
// Identity автоматично генерує email код
var code = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
await emailSender.SendEmailAsync(user.Email, "2FA Code", $"Your code: {code}");
```

**Валідація:**
```csharp
// Identity автоматично валідує email код
var result = await signInManager.TwoFactorSignInAsync(
    TokenOptions.DefaultEmailProvider, code, rememberMe, rememberClient);
```

### 3. Recovery коди

**Генерація:**
```csharp
// Identity автоматично генерує recovery коди
var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
```

**Використання:**
```csharp
// Identity автоматично валідує recovery код
var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
```

## 💡 Переваги нативного підходу

### ✅ Мінімум кастомного коду
- `UserManager.GetAuthenticatorKeyAsync()` - генерація TOTP ключів
- `UserManager.VerifyTwoFactorTokenAsync()` - валідація TOTP
- `SignInManager.TwoFactorSignInAsync()` - 2FA вхід
- `UserManager.GenerateNewTwoFactorRecoveryCodesAsync()` - recovery коди

### ✅ Автоматичні функції
- Генерація QR кодів для TOTP
- Валідація часових вікон для TOTP
- Управління станом 2FA
- Блокування акаунтів
- Логування подій

### ✅ Безпека "з коробки"
- Криптографічно стійкі TOTP ключі
- Захист від replay атак
- Часові вікна для кодів
- Secure token generation

## 🛠️ Розширення

### Додавання реального Email провайдера:
```csharp
public class SmtpEmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Інтеграція з SMTP, SendGrid, Mailgun тощо
        using var client = new SmtpClient("smtp.gmail.com", 587);
        await client.SendMailAsync(email, subject, htmlMessage);
    }
}
```

### Додавання SMS 2FA:
```csharp
// Identity підтримує SMS 2FA через TokenOptions.DefaultPhoneProvider
var smsCode = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
await smsSender.SendSmsAsync(user.PhoneNumber, $"Code: {smsCode}");
```

## 🎯 Висновок

Цей проект демонструє, як максимально використовувати нативні можливості ASP.NET Core Identity для 2FA. Підхід забезпечує:

- **Мінімум коду** - готові методи для всіх операцій
- **Високу безпеку** - перевірені рішення Microsoft
- **Легку підтримку** - стандартні методи та практики
- **Швидку розробку** - готова інфраструктура

Ідеально підходить для проектів, де потрібна надійна 2FA без складної кастомізації.
