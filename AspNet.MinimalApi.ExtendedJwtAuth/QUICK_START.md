# 🚀 Швидкий старт - Detailed JWT Authentication

## ⚡ Запуск за 5 хвилин

### 1. Перевірте, що у вас встановлено:

- ✅ .NET 9 SDK
- ✅ IDE (Visual Studio, VS Code, Rider)

### 2. Клонуйте та запустіть:

```bash
# Перейдіть в папку проєкту
cd AspNet.MinimalApi.ExtendedJwtAuth

# Відновіть пакети
dotnet restore

# Запустіть додаток
dotnet run
```

### 3. Відкрийте в браузері:

- 🌐 **API**: https://localhost:7001
- 📖 **Swagger**: https://localhost:7001/swagger

## 🧪 Швидке тестування

### Крок 1: Зареєструйте користувача

```bash
curl -X POST https://localhost:7001/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"test@example.com",
    "password":"Test123!",
    "confirmPassword":"Test123!",
    "firstName":"Тест",
    "lastName":"Користувач"
  }'
```

**Очікувана відповідь:**

```json
{
  "success": true,
  "message": "Користувач успішно зареєстрований",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64_encoded_refresh_token",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "id": "user_id",
      "email": "test@example.com",
      "firstName": "Тест",
      "lastName": "Користувач"
    }
  }
}
```

### Крок 2: Увійдіть в систему

```bash
curl -X POST https://localhost:7001/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email":"test@example.com",
    "password":"Test123!"
  }'
```

### Крок 3: Використайте токен

```bash
# Замініть YOUR_TOKEN на отриманий accessToken
curl -X GET https://localhost:7001/auth/profile \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Крок 4: Оновіть токен

```bash
# Замініть YOUR_REFRESH_TOKEN на отриманий refreshToken
curl -X POST https://localhost:7001/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"YOUR_REFRESH_TOKEN"}'
```

## 📋 Доступні ендпоінти

### 🔓 Публічні (без аутентифікації):

- `GET /` - Інформація про API
- `POST /auth/register` - Реєстрація
- `POST /auth/login` - Вхід в систему
- `POST /auth/refresh` - Оновлення токена
- `POST /auth/logout` - Вихід з системи

### 🔒 Захищені (потребують токена):

- `GET /auth/profile` - Профіль користувача
- `PUT /auth/profile` - Оновлення профілю
- `PUT /auth/change-password` - Зміна пароля
- `GET /users` - Список користувачів

## 🔑 Як працювати з токенами

### 1. Отримання токенів:

- При реєстрації або вході отримуєте `accessToken` та `refreshToken`
- `accessToken` - для доступу до API (діє 60 хв)
- `refreshToken` - для оновлення токена (діє 7 днів)

### 2. Використання токенів:

```bash
# Додайте заголовок Authorization до кожного запиту
Authorization: Bearer YOUR_ACCESS_TOKEN
```

### 3. Оновлення токенів:

```bash
# Коли accessToken закінчується, використайте refreshToken
POST /auth/refresh
{
  "refreshToken": "YOUR_REFRESH_TOKEN"
}
```

## 🛠️ Налаштування для розробки

### Зміна портів:

Відредагуйте `Properties/launchSettings.json`:

```json
{
  "applicationUrl": "https://localhost:7001;http://localhost:5001"
}
```

### Зміна налаштувань JWT:

Відредагуйте `appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "ExpirationMinutes": 120,
    "RefreshExpirationDays": 30
  }
}
```

### Зміна бази даних:

Відредагуйте `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=my_custom_db.db"
  }
}
```

## 🐛 Вирішення проблем

### Проблема: "JWT SecretKey не налаштований"

**Рішення:** Перевірте `appsettings.json` - має бути секретний ключ мінімум 32 символи.

### Проблема: "Database не створюється"

**Рішення:**

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Проблема: "Токен недійсний"

**Рішення:** Перевірте:

- Чи правильно скопійований токен
- Чи не закінчився термін дії
- Чи правильний формат заголовка `Authorization: Bearer TOKEN`

### Проблема: "CORS помилки"

**Рішення:** Додайте CORS налаштування в `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors();
```

## 📚 Наступні кроки

1. **Прочитайте документацію**: `README.md`
2. **Вивчіть код**: Почніть з `Program.cs`
3. **Протестуйте API**: Використайте `ExtendedJwtAuth.http`
4. **Експериментуйте**: Додайте нові ендпоінти
5. **Вивчіть сервіси**: `Services/AuthService.cs` та `Services/JwtService.cs`

## 🎓 Навчальні завдання

### Легкий рівень (15-30 хв):

- [ ] Додайте поле "Дата народження" до користувача
- [ ] Створіть ендпоінт для отримання кількості користувачів
- [ ] Додайте валідацію номера телефону

### Середній рівень (1-2 години):

- [ ] Реалізуйте ролі користувачів
- [ ] Додайте ендпоінт тільки для адмінів
- [ ] Створіть middleware для логування запитів

### Складний рівень (3-5 годин):

- [ ] Додайте підтвердження email
- [ ] Реалізуйте двофакторну аутентифікацію
- [ ] Інтегруйте Google OAuth

## 💡 Корисні команди

```bash
# Перегляд логів
dotnet run --verbosity detailed

# Запуск з іншим профілем
dotnet run --environment Production

# Створення міграції
dotnet ef migrations add AddNewField

# Перегляд SQL команд
dotnet ef migrations script

# Видалення бази даних
dotnet ef database drop --force
```

---

**Готово!** Тепер у вас є повнофункціональний JWT API з детальною реалізацією! 🎉
