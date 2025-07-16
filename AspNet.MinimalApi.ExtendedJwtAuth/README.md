# ASP.NET Core Detailed JWT Authentication - Minimal API

Цей проєкт демонструє **детальну реалізацію JWT аутентифікації** з використанням **UserManager**, **SignInManager** та *
*Identity** в **Minimal API** для .NET 9.

## 🎯 Мета проєкту

Показати студентам, як **вручну** реалізувати JWT аутентифікацію без використання `app.MapIdentityApi<User>()`, з повним
контролем над процесом аутентифікації та детальним розумінням кожного кроку.

## 🏗️ Архітектура проєкту

```
AspNet.MinimalApi.ExtendedJwtAuth/
├── Models/
│   ├── User.cs                    # Модель користувача з RefreshToken
│   └── AuthModels.cs              # Моделі для аутентифікації
├── Data/
│   └── ApplicationDbContext.cs    # Контекст бази даних
├── Services/
│   ├── JwtService.cs              # Сервіс для роботи з JWT
│   └── AuthService.cs             # Сервіс аутентифікації
├── Extensions/
│   └── ServiceExtensions.cs       # Розширення для налаштування
├── Program.cs                     # Головний файл
├── appsettings.json              # Конфігурація
└── README.md                     # Цей файл
```

## 🚀 Основні функції

### Ендпоінти аутентифікації:

- `POST /auth/register` - Реєстрація користувача
- `POST /auth/login` - Вхід в систему
- `POST /auth/refresh` - Оновлення токена
- `POST /auth/logout` - Вихід з системи

### Ендпоінти профілю:

- `GET /auth/profile` - Профіль користувача
- `PUT /auth/profile` - Оновлення профілю
- `PUT /auth/change-password` - Зміна пароля

### Ендпоінти користувачів:

- `GET /users` - Список користувачів

## 🛠️ Технології

- **ASP.NET Core 9.0** - Веб-фреймворк
- **ASP.NET Core Identity** - Система аутентифікації
- **JWT Bearer Authentication** - JWT токенна аутентифікація
- **Entity Framework Core** - ORM для роботи з базою даних
- **SQLite** - База даних (для простоти розробки)
- **Swagger/OpenAPI** - Документація API з JWT підтримкою

## 📦 Встановлення та запуск

### 1. Перейдіть в папку проєкту

```bash
cd AspNet.MinimalApi.ExtendedJwtAuth
```

### 2. Встановіть залежності

```bash
dotnet restore
```

### 3. Запустіть додаток

```bash
dotnet run
```

### 4. Відкрийте в браузері

- 🌐 **API**: https://localhost:7000
- 📖 **Swagger**: https://localhost:7000/swagger

## 🧪 Тестування API

### Крок 1: Зареєструйте користувача

```bash
curl -X POST https://localhost:7000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "firstName": "Тест",
    "lastName": "Користувач"
  }'
```

### Крок 2: Увійдіть в систему

```bash
curl -X POST https://localhost:7000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

### Крок 3: Використайте токен для доступу

```bash
curl -X GET https://localhost:7000/auth/profile \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Крок 4: Оновіть токен

```bash
curl -X POST https://localhost:7000/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## 🔐 Ключові відмінності від MapIdentityApi

### 1. **Повний контроль над процесом**

- Ручна реалізація реєстрації через `UserManager.CreateAsync()`
- Ручна перевірка пароля через `SignInManager.CheckPasswordSignInAsync()`
- Власна логіка генерації та валідації JWT токенів

### 2. **Детальна обробка помилок**

- Кастомні моделі відповідей `ApiResponse<T>`
- Детальні повідомлення про помилки
- Логування всіх операцій

### 3. **Управління Refresh токенами**

- Зберігання refresh токенів в базі даних
- Автоматичне очищення застарілих токенів
- Можливість відкликання токенів

### 4. **Розширювані сервіси**

- `JwtService` - для роботи з JWT
- `AuthService` - для аутентифікації
- Легко додавати нову функціональність

## 📚 Навчальні матеріали

### Ключові класи та методи:

#### UserManager<User>

```csharp
// Створення користувача
await userManager.CreateAsync(user, password);

// Пошук користувача
await userManager.FindByEmailAsync(email);

// Оновлення користувача
await userManager.UpdateAsync(user);

// Зміна пароля
await userManager.ChangePasswordAsync(user, oldPassword, newPassword);

// Отримання ролей
await userManager.GetRolesAsync(user);
```

#### SignInManager<User>

```csharp
// Перевірка пароля з блокуванням
await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
```

#### JwtSecurityTokenHandler

```csharp
// Створення токена
var token = new JwtSecurityToken(issuer, audience, claims, expires, credentials);
var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

// Валідація токена
var principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
```

## 🔧 Налаштування

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "MyVeryLongSecretKeyForJwtTokenGeneration123456789",
    "Issuer": "AspNet.MinimalApi.ExtendedJwtAuth",
    "Audience": "AspNet.MinimalApi.ExtendedJwtAuth.Client",
    "ExpirationMinutes": 60,
    "RefreshExpirationDays": 7
  }
}
```

### Identity Options

```csharp
services.AddIdentity<User, IdentityRole>(options =>
{
    // Налаштування паролів
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    
    // Налаштування блокування
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
```

## 🎓 Навчальні завдання

### Легкий рівень:

- [ ] Додайте валідацію моделей з DataAnnotations
- [ ] Створіть ендпоінт для отримання інформації про токен
- [ ] Додайте логування в файл

### Середній рівень:

- [ ] Реалізуйте ролі користувачів (Admin, User)
- [ ] Додайте ендпоінт тільки для адмінів
- [ ] Створіть middleware для обробки помилок

### Складний рівень:

- [ ] Додайте підтвердження email
- [ ] Реалізуйте двофакторну аутентифікацію
- [ ] Інтегруйте зовнішніх провайдерів (Google, Facebook)

## 💡 Корисні команди

```bash
# Створення міграції
dotnet ef migrations add InitialCreate

# Застосування міграцій
dotnet ef database update

# Видалення бази даних
dotnet ef database drop

# Перегляд SQL команд
dotnet ef migrations script
```

## 🔍 Порівняння з MapIdentityApi

| Функція             | MapIdentityApi | Детальна реалізація |
|---------------------|----------------|---------------------|
| Складність          | Проста         | Середня             |
| Контроль            | Обмежений      | Повний              |
| Налаштування        | Автоматичне    | Ручне               |
| Розширюваність      | Обмежена       | Висока              |
| Навчальна цінність  | Низька         | Висока              |
| Продакшн готовність | Висока         | Висока              |

## 🚨 Безпека

### Рекомендації для продакшену:

1. Використовуйте складні секретні ключі (мінімум 256 біт)
2. Зберігайте секретні ключі в Azure Key Vault або подібних сервісах
3. Використовуйте HTTPS для всіх запитів
4. Налаштуйте CORS правильно
5. Додайте rate limiting
6. Логуйте всі спроби аутентифікації

### Приклад безпечної конфігурації:

```csharp
// Використовуйте змінні середовища
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? throw new InvalidOperationException("JWT_SECRET_KEY не налаштований");
```

---

**Автор**: Розробник  
**Версія**: 1.0  
**Дата**: 2024
