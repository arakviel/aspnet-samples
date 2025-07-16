# 🚀 Швидкий старт - ASP.NET Core Identity Auth

## ⚡ Запуск за 5 хвилин

### 1. Перевірте, що у вас встановлено:
- ✅ .NET 9 SDK
- ✅ IDE (Visual Studio, VS Code, Rider)

### 2. Клонуйте та запустіть:
```bash
# Перейдіть в папку проєкту
cd AspNet.MinimalApi.IdentityAuth

# Відновіть пакети
dotnet restore

# Запустіть додаток
dotnet run
```

### 3. Відкрийте в браузері:
- 🌐 **API**: https://localhost:7000
- 📖 **Swagger**: https://localhost:7000/swagger

## 🧪 Швидке тестування

### Крок 1: Зареєструйте користувача
```bash
curl -X POST https://localhost:7000/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

### Крок 2: Увійдіть в систему
```bash
curl -X POST https://localhost:7000/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

### Крок 3: Скопіюйте токен з відповіді
```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",  // ← Скопіюйте цей токен
  "expiresIn": 3600,
  "refreshToken": "CfDJ8..."
}
```

### Крок 4: Отримайте профіль
```bash
curl -X GET https://localhost:7000/auth/profile \
  -H "Authorization: Bearer ВАШ_ТОКЕН_ТУТ"
```

## 📱 Тестування через Swagger

1. **Відкрийте Swagger**: https://localhost:7000/swagger
2. **Зареєструйтесь**: Знайдіть `POST /register` → Execute
3. **Увійдіть**: Знайдіть `POST /login` → Execute → Скопіюйте `accessToken`
4. **Авторизуйтесь**: Натисніть 🔒 "Authorize" → Введіть `Bearer ВАШ_ТОКЕН`
5. **Тестуйте**: Спробуйте `GET /auth/profile`

## 🎯 Основні ендпоінти

| Метод | Ендпоінт | Опис | Авторизація |
|-------|----------|------|-------------|
| `GET` | `/` | Інформація про API | ❌ |
| `POST` | `/register` | Реєстрація | ❌ |
| `POST` | `/login` | Вхід | ❌ |
| `POST` | `/refresh` | Оновлення токена | ❌ |
| `GET` | `/auth/profile` | Профіль користувача | ✅ |
| `PUT` | `/auth/profile` | Оновлення профілю | ✅ |
| `GET` | `/auth/users` | Список користувачів | ✅ |

## 🔧 Налаштування

### Зміна бази даних:
Відредагуйте `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=myapp.db"
  }
}
```

### Зміна налаштувань паролів:
У `Program.cs` знайдіть:
```csharp
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 8;        // Мінімальна довжина
    options.Password.RequireDigit = true;       // Вимагати цифри
    options.Password.RequireUppercase = false;  // Не вимагати великі літери
    // ...
});
```

### Зміна терміну дії токенів:
```csharp
.AddBearerToken(IdentityConstants.BearerScheme, options =>
{
    options.BearerTokenExpiration = TimeSpan.FromHours(2);  // 2 години
    options.RefreshTokenExpiration = TimeSpan.FromDays(14); // 14 днів
});
```

## 🐛 Вирішення проблем

### Проблема: "dotnet command not found"
**Рішення**: Встановіть .NET 9 SDK з https://dotnet.microsoft.com/download

### Проблема: "Port already in use"
**Рішення**: Змініть порт в `Properties/launchSettings.json`

### Проблема: "Database locked"
**Рішення**: Закрийте всі підключення до SQLite або видаліть `identity.db`

### Проблема: "Unauthorized" при доступі до API
**Рішення**: 
1. Перевірте, чи правильно скопійований токен
2. Переконайтесь, що токен не прострочений
3. Перевірте формат: `Authorization: Bearer ВАШ_ТОКЕН`

## 📚 Що далі?

1. **Прочитайте документацію**: `README.md`
2. **Вивчіть архітектуру**: `ARCHITECTURE.md`
3. **Пройдіть навчання**: `STUDENT_GUIDE.md`
4. **Подивіться приклади**: `EXAMPLES.md`
5. **Протестуйте API**: `IdentityAuth.http`

## 🎓 Навчальні завдання

### Легкий рівень:
- [ ] Додайте поле "Дата народження" до користувача
- [ ] Створіть ендпоінт для зміни пароля
- [ ] Додайте валідацію email

### Середній рівень:
- [ ] Реалізуйте ролі користувачів (Admin, User)
- [ ] Додайте ендпоінт тільки для адмінів
- [ ] Створіть middleware для логування запитів

### Складний рівень:
- [ ] Додайте двофакторну аутентифікацію
- [ ] Інтегруйте Google OAuth
- [ ] Реалізуйте відправку email для підтвердження

## 💡 Корисні команди

```bash
# Створення міграції
dotnet ef migrations add AddNewField

# Застосування міграцій
dotnet ef database update

# Видалення бази даних
dotnet ef database drop

# Перегляд логів
dotnet run --verbosity detailed

# Запуск в режимі розробки
dotnet run --environment Development

# Збірка для продакшену
dotnet publish -c Release
```

**Успіхів у навчанні! 🎉**
