# ASP.NET Minimal API - Custom JWT Authentication

Повноцінна кастомна реалізація JWT аутентифікації без використання готових бібліотек для демонстрації студентам принципів роботи JWT токенів.

## 🎯 Навчальні цілі

Цей проект демонструє:

- 🔐 **Кастомну генерацію JWT токенів** - повна реалізація без готових бібліотек
- 🔑 **Підпис та валідацію токенів** - HMAC SHA-256 алгоритм
- 🎫 **Access та Refresh токени** - повний цикл управління токенами
- 🛡️ **Middleware аутентифікації** - кастомний middleware для перевірки токенів
- 👥 **Систему ролей** - розмежування доступу за ролями
- 🗄️ **Інтеграцію з базою даних** - Entity Framework Core + SQLite
- 🔒 **Безпечне зберігання паролів** - BCrypt хешування
- 📝 **Логування** - Serilog для детального логування

## 🏗️ Архітектура проекту

```
AspNet.MinimalApi.CustomJwtAuth/
├── Models/                          # Моделі даних
│   ├── User.cs                     # Модель користувача
│   └── AuthModels.cs               # DTO для аутентифікації
├── Authentication/                  # Кастомна JWT реалізація
│   ├── JwtTokenGenerator.cs        # Генерація та валідація токенів
│   ├── JwtAuthenticationOptions.cs # Опції конфігурації
│   └── JwtClaims.cs               # Константи для claims
├── Middleware/                     # Middleware
│   └── JwtAuthenticationMiddleware.cs # JWT аутентифікація
├── Services/                       # Бізнес-логіка
│   ├── IUserService.cs            # Інтерфейс сервісу користувачів
│   ├── UserService.cs             # Реалізація сервісу користувачів
│   ├── IJwtService.cs             # Інтерфейс JWT сервісу
│   └── JwtService.cs              # Реалізація JWT сервісу
├── Data/                          # База даних
│   └── AuthDbContext.cs           # EF Core контекст
├── Extensions/                    # Розширення
│   └── JwtAuthenticationExtensions.cs # DI та middleware розширення
├── Program.cs                     # Головний файл
├── JwtAuth.http                   # HTTP запити для тестування
└── README.md                      # Документація
```

## 🔧 Технології

- **ASP.NET Core 9.0** - Minimal API
- **Entity Framework Core** - ORM для роботи з базою даних
- **SQLite** - Легка база даних для демонстрації
- **BCrypt.Net** - Хешування паролів
- **Serilog** - Структуроване логування
- **System.Text.Json** - Серіалізація JSON

## 🚀 Запуск проекту

### 1. Клонування та встановлення залежностей

```bash
cd AspNet.MinimalApi.CustomJwtAuth
dotnet restore
```

### 2. Запуск додатку

```bash
dotnet run
```

Додаток буде доступний за адресою: `https://localhost:7000`

### 3. Тестування

Використайте файл `JwtAuth.http` для тестування всіх ендпоінтів.

## 📋 API Ендпоінти

### Публічні ендпоінти

| Метод | Шлях | Опис |
|-------|------|------|
| GET | `/` | Головна сторінка з інформацією про API |
| GET | `/public` | Публічний ендпоінт без аутентифікації |
| POST | `/auth/register` | Реєстрація нового користувача |
| POST | `/auth/login` | Вхід в систему |
| POST | `/auth/refresh` | Оновлення токена доступу |

### Захищені ендпоінти

| Метод | Шлях | Роль | Опис |
|-------|------|------|------|
| GET | `/auth/status` | Any | Статус аутентифікації |
| GET | `/auth/profile` | Any | Профіль користувача |
| GET | `/protected` | Any | Захищений ресурс |
| GET | `/roles/test` | Any | Тестування ролей |
| GET | `/users` | Admin | Список всіх користувачів |
| GET | `/admin/dashboard` | Admin | Адміністративна панель |

## 👤 Тестові користувачі

| Username | Password | Role | Email |
|----------|----------|------|-------|
| admin | admin123 | Admin | admin@example.com |
| user | user123 | User | user@example.com |
| testuser | test123 | User | test@example.com |

## 🔐 Як працює JWT аутентифікація

### 1. Структура JWT токена

JWT токен складається з трьох частин, розділених крапками:

```
header.payload.signature
```

**Header (заголовок):**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (корисне навантаження):**
```json
{
  "sub": "1",
  "username": "admin",
  "email": "admin@example.com",
  "role": "Admin",
  "token_type": "access",
  "iss": "AspNet.MinimalApi.CustomJwtAuth",
  "aud": "AspNet.MinimalApi.CustomJwtAuth.Users",
  "exp": 1640995200,
  "iat": 1640991600,
  "jti": "unique-token-id"
}
```

**Signature (підпис):**
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

### 2. Процес аутентифікації

1. **Реєстрація/Вхід** → Сервер генерує Access та Refresh токени
2. **Запит до API** → Клієнт надсилає Access токен в заголовку `Authorization: Bearer <token>`
3. **Валідація** → Middleware перевіряє підпис, термін дії та claims
4. **Авторизація** → Перевірка ролей та дозволів
5. **Оновлення** → Використання Refresh токена для отримання нового Access токена

### 3. Безпека

- **Підпис токена** - HMAC SHA-256 з секретним ключем
- **Термін дії** - Access токени мають короткий термін дії (15 хв в dev, 60 хв в prod)
- **Refresh токени** - Довший термін дії (7 днів) для оновлення Access токенів
- **Хешування паролів** - BCrypt з солью
- **Валідація claims** - Перевірка issuer, audience, expiration

## 🔍 Ключові компоненти

### JwtTokenGenerator

Основний клас для роботи з JWT токенами:

- `GenerateAccessToken()` - Генерація токена доступу
- `GenerateRefreshToken()` - Генерація токена оновлення  
- `ValidateToken()` - Валідація токена та отримання claims
- `CreateSignature()` - Створення HMAC SHA-256 підпису
- `VerifySignature()` - Перевірка підпису токена

### JwtAuthenticationMiddleware

Middleware для автоматичної аутентифікації:

- Витягує токен з заголовка `Authorization`
- Валідує токен та встановлює `HttpContext.User`
- Додає інформацію про користувача в `HttpContext.Items`
- Логує спроби аутентифікації

### UserService

Сервіс для управління користувачами:

- Реєстрація та аутентифікація користувачів
- Хешування та перевірка паролів
- Управління ролями та дозволами
- Інтеграція з базою даних

## 📊 Логування

Проект використовує Serilog для структурованого логування:

- **Debug** - Детальна інформація про токени та аутентифікацію
- **Information** - Успішні операції (вхід, реєстрація)
- **Warning** - Невдалі спроби аутентифікації
- **Error** - Помилки валідації токенів та бази даних

## 🧪 Тестування

### Автоматичне тестування

```bash
# Запуск всіх тестів
dotnet test

# Запуск з покриттям коду
dotnet test --collect:"XPlat Code Coverage"
```

### Ручне тестування

Використайте файл `JwtAuth.http` з різними сценаріями:

1. **Позитивні сценарії** - Успішна аутентифікація та авторизація
2. **Негативні сценарії** - Невірні токени, відсутні дозволи
3. **Граничні випадки** - Прострочені токени, невірні ролі

## ⚠️ Важливі зауваження

### Для навчальних цілей:

- Секретний ключ зберігається в конфігурації (в продакшені використовуйте Azure Key Vault)
- Спрощена валідація email
- Базова система ролей

### Для продакшену потрібно:

- **Зовнішнє зберігання секретів** (Azure Key Vault, AWS Secrets Manager)
- **HTTPS для всіх запитів**
- **Rate limiting** для запобігання brute force атакам
- **Додаткова валідація** вхідних даних
- **Аудит та моніторинг** безпеки
- **Відкликання токенів** (token blacklist)

## 🔄 Порівняння з готовими рішеннями

| Аспект | Наша реалізація | ASP.NET Core Identity |
|--------|-----------------|----------------------|
| Складність | Середня | Висока |
| Контроль | Повний | Обмежений |
| Навчальна цінність | Висока | Низька |
| Продакшен готовність | Потребує доопрацювання | Готове |
| Розуміння JWT | Глибоке | Поверхневе |

## 📚 Додаткові ресурси

- [RFC 7519 - JSON Web Token (JWT)](https://tools.ietf.org/html/rfc7519)
- [JWT.io - JWT Debugger](https://jwt.io/)
- [OWASP JWT Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)
- [ASP.NET Core Security Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/)

## 🤝 Внесок

Цей проект створений для навчальних цілей. Пропозиції та покращення вітаються!
