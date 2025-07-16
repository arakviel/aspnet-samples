# 🏗️ Архітектура ASP.NET Core Identity в Minimal API

## 📊 Діаграма компонентів

```
┌─────────────────────────────────────────────────────────────┐
│                    HTTP Requests                            │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 Middleware Pipeline                         │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────────┐ │
│  │   HTTPS     │ │ Authentication │ │    Authorization      │ │
│  │ Redirection │ │   Middleware   │ │     Middleware        │ │
│  └─────────────┘ └─────────────┘ └─────────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 Minimal API Endpoints                       │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │            Identity API Endpoints                      │ │
│  │  /register, /login, /refresh, /forgotPassword...       │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              Custom Endpoints                          │ │
│  │  /auth/profile, /auth/users, /auth/profile (PUT)       │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                ASP.NET Core Identity                        │
│                                                             │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐ │
│  │   UserManager   │ │  SignInManager  │ │  RoleManager    │ │
│  │                 │ │                 │ │                 │ │
│  │ - CreateAsync   │ │ - SignInAsync   │ │ - CreateAsync   │ │
│  │ - FindByEmail   │ │ - SignOutAsync  │ │ - FindByName    │ │
│  │ - UpdateAsync   │ │ - CheckPassword │ │ - DeleteAsync   │ │
│  └─────────────────┘ └─────────────────┘ └─────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Entity Framework Core                          │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │            ApplicationDbContext                         │ │
│  │                                                         │ │
│  │  - Users (DbSet<User>)                                  │ │
│  │  - Roles (DbSet<IdentityRole>)                          │ │
│  │  - UserRoles, UserClaims, UserLogins...                 │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   SQLite Database                           │
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                   Tables                                │ │
│  │                                                         │ │
│  │  - AspNetUsers                                          │ │
│  │  - AspNetRoles                                          │ │
│  │  - AspNetUserRoles                                      │ │
│  │  - AspNetUserClaims                                     │ │
│  │  - AspNetUserLogins                                     │ │
│  │  - AspNetUserTokens                                     │ │
│  │  - AspNetRoleClaims                                     │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 🔄 Потік аутентифікації

### 1. Реєстрація користувача
```
Client → POST /register → Identity API → UserManager.CreateAsync() → EF Core → Database
```

### 2. Логін користувача
```
Client → POST /login → Identity API → SignInManager.CheckPasswordSignInAsync() → Generate JWT → Return Token
```

### 3. Доступ до захищеного ресурсу
```
Client → GET /auth/profile + Bearer Token → Authentication Middleware → Validate Token → Extract Claims → Endpoint
```

## 🧩 Ключові компоненти

### 1. **User Model**
```csharp
public class User : IdentityUser
{
    // Розширює стандартну модель IdentityUser
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. **ApplicationDbContext**
```csharp
public class ApplicationDbContext : IdentityDbContext<User>
{
    // Автоматично створює всі необхідні таблиці для Identity
}
```

### 3. **Authentication Configuration**
```csharp
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme)      // Для веб-додатків
    .AddBearerToken(IdentityConstants.BearerScheme);     // Для API
```

### 4. **Identity Configuration**
```csharp
builder.Services.AddIdentityCore<User>(options => {
    // Налаштування паролів, блокування, користувачів
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddApiEndpoints();
```

## 🔐 Схема безпеки

### Типи аутентифікації:

1. **Cookie Authentication** - для веб-додатків
   - Зберігає інформацію в HTTP cookies
   - Автоматично відправляється з кожним запитом
   - Підходить для традиційних веб-додатків

2. **Bearer Token Authentication** - для API
   - Використовує JWT токени
   - Передається в заголовку `Authorization: Bearer <token>`
   - Підходить для SPA, мобільних додатків, API

### Структура JWT токена:
```
Header.Payload.Signature

Header: { "alg": "HS256", "typ": "JWT" }
Payload: { "sub": "user-id", "email": "user@example.com", "exp": 1234567890 }
Signature: HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)
```

## 📋 Таблиці бази даних

### AspNetUsers
- Id (string) - Унікальний ідентифікатор
- UserName (string) - Ім'я користувача
- Email (string) - Електронна пошта
- PasswordHash (string) - Хеш пароля
- SecurityStamp (string) - Штамп безпеки
- FirstName (string) - Додаткове поле
- LastName (string) - Додаткове поле
- CreatedAt (datetime) - Додаткове поле

### AspNetRoles
- Id (string) - Унікальний ідентифікатор ролі
- Name (string) - Назва ролі
- NormalizedName (string) - Нормалізована назва

### AspNetUserRoles
- UserId (string) - Зовнішній ключ до Users
- RoleId (string) - Зовнішній ключ до Roles

### AspNetUserClaims
- Id (int) - Унікальний ідентифікатор
- UserId (string) - Зовнішній ключ до Users
- ClaimType (string) - Тип claim
- ClaimValue (string) - Значення claim

## 🚀 Переваги цієї архітектури

1. **Безпека** - Використання перевірених рішень ASP.NET Core Identity
2. **Гнучкість** - Можливість розширення моделі користувача
3. **Масштабованість** - Підтримка різних типів аутентифікації
4. **Простота** - Мінімальний код для максимальної функціональності
5. **Стандартність** - Використання стандартних підходів Microsoft

## 🔧 Налаштування для продакшену

1. **База даних** - Замінити SQLite на PostgreSQL/SQL Server
2. **Секрети** - Використовувати Azure Key Vault або подібні сервіси
3. **HTTPS** - Обов'язкове використання HTTPS
4. **Логування** - Додати структуроване логування
5. **Моніторинг** - Додати Application Insights або подібні інструменти
