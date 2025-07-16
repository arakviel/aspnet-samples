# 📚 Покроковий гід для студентів: ASP.NET Core Identity

Цей гід допоможе вам зрозуміти, як працює аутентифікація в ASP.NET Core Identity.

## 🎯 Що ви дізнаєтесь

1. Як налаштувати ASP.NET Core Identity
2. Як створити модель користувача
3. Як налаштувати базу даних
4. Як створити API для аутентифікації
5. Як захистити ендпоінти
6. Як тестувати API

## 📋 Крок 1: Розуміння структури проєкту

### Основні компоненти:

**Models/User.cs** - Модель користувача
```csharp
public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    // IdentityUser вже містить: Id, UserName, Email, PasswordHash тощо
}
```

**Data/ApplicationDbContext.cs** - Контекст бази даних
```csharp
public class ApplicationDbContext : IdentityDbContext<User>
{
    // Автоматично створює таблиці для Identity
}
```

## 📋 Крок 2: Налаштування сервісів

### У файлі Program.cs:

1. **Додавання бази даних:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

2. **Налаштування аутентифікації:**
```csharp
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme)      // Для веб-додатків
    .AddBearerToken(IdentityConstants.BearerScheme);     // Для API
```

3. **Налаштування Identity:**
```csharp
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddApiEndpoints();
```

## 📋 Крок 3: Автоматичні ендпоінти

### MapIdentityApi<User>() створює:

- `POST /register` - Реєстрація
- `POST /login` - Вхід
- `POST /refresh` - Оновлення токена
- `POST /forgotPassword` - Відновлення пароля
- `POST /resetPassword` - Скидання пароля

### Приклад використання:

**Реєстрація:**
```json
POST /register
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Логін:**
```json
POST /login
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Відповідь:**
```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "refreshToken": "CfDJ8..."
}
```

## 📋 Крок 4: Кастомні ендпоінти

### Захищений ендпоінт:
```csharp
app.MapGet("/auth/profile", async (ClaimsPrincipal claims, ApplicationDbContext context) =>
{
    var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var user = await context.Users.FindAsync(userId);
    return Results.Ok(user);
})
.RequireAuthorization(); // Вимагає токен!
```

### Отримання даних користувача:
```csharp
var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var userName = claims.FindFirst(ClaimTypes.Name)?.Value;
var email = claims.FindFirst(ClaimTypes.Email)?.Value;
```

## 📋 Крок 5: Тестування

### 1. Запустіть додаток:
```bash
dotnet run
```

### 2. Відкрийте Swagger:
```
https://localhost:7000/swagger
```

### 3. Зареєструйте користувача:
- Знайдіть ендпоінт `/register`
- Введіть email та пароль
- Натисніть "Execute"

### 4. Увійдіть в систему:
- Знайдіть ендпоінт `/login`
- Введіть ті ж дані
- Скопіюйте `accessToken` з відповіді

### 5. Авторизуйтесь в Swagger:
- Натисніть кнопку "Authorize" вгорі
- Введіть: `Bearer ваш_токен`
- Натисніть "Authorize"

### 6. Тестуйте захищені ендпоінти:
- Спробуйте `/auth/profile`
- Спробуйте `/auth/users`

## 📋 Крок 6: Розуміння безпеки

### Хешування паролів:
```csharp
// Identity автоматично хешує паролі
// Ніколи не зберігайте паролі у відкритому вигляді!
```

### Токени:
```csharp
// Bearer токени мають обмежений термін дії
// Використовуйте refresh токени для оновлення
```

### Claims:
```csharp
// Claims - це інформація про користувача в токені
// Наприклад: ID, ім'я, email, ролі
```

## 🔍 Практичні завдання

### Завдання 1: Додайте нове поле
1. Додайте поле `DateOfBirth` до моделі `User`
2. Оновіть ендпоінт профілю
3. Створіть міграцію

### Завдання 2: Додайте валідацію
1. Додайте валідацію email
2. Додайте валідацію довжини імені
3. Додайте кастомні повідомлення про помилки

### Завдання 3: Додайте ролі
1. Створіть ролі "Admin" та "User"
2. Додайте ендпоінт тільки для адмінів
3. Протестуйте авторизацію за ролями

## ❓ Часті питання

**Q: Чому використовується IdentityUser?**
A: IdentityUser містить всі необхідні поля для аутентифікації: ID, email, хеш пароля тощо.

**Q: Що таке Claims?**
A: Claims - це пари ключ-значення з інформацією про користувача, що зберігаються в токені.

**Q: Як працює Bearer токен?**
A: Клієнт отримує токен при логіні та передає його в заголовку `Authorization: Bearer токен`.

**Q: Чому потрібен Refresh токен?**
A: Access токени мають короткий термін дії для безпеки. Refresh токени дозволяють отримати новий access токен.

## 🎓 Висновки

Ви навчились:
- ✅ Налаштовувати ASP.NET Core Identity
- ✅ Створювати API для аутентифікації
- ✅ Захищати ендпоінти
- ✅ Працювати з токенами
- ✅ Тестувати API

Тепер ви можете створювати безпечні веб-додатки з аутентифікацією!
