# ASP.NET Core Identity Authentication - Minimal API

Цей проєкт демонструє реалізацію аутентифікації через **ASP.NET Core Identity** в **Minimal API** для .NET 9.

## 🎯 Мета проєкту

Показати студентам, як правильно інтегрувати ASP.NET Core Identity в Minimal API додаток з детальними поясненнями кожного кроку.

## 🏗️ Архітектура проєкту

```
AspNet.MinimalApi.IdentityAuth/
├── Models/
│   └── User.cs                    # Модель користувача (розширює IdentityUser)
├── Data/
│   └── ApplicationDbContext.cs    # Контекст бази даних для Identity
├── Extensions/
│   └── MigrationExtensions.cs     # Розширення для міграцій
├── Program.cs                     # Головний файл з налаштуваннями
├── appsettings.json              # Конфігурація додатку
├── IdentityAuth.http             # Файл для тестування API
└── README.md                     # Цей файл
```

## 🚀 Основні функції

### Автоматичні ендпоінти Identity:
- `POST /register` - Реєстрація користувача
- `POST /login` - Вхід в систему
- `POST /refresh` - Оновлення токена
- `GET /confirmEmail` - Підтвердження email
- `POST /resendConfirmationEmail` - Повторна відправка підтвердження
- `POST /forgotPassword` - Відновлення пароля
- `POST /resetPassword` - Скидання пароля
- `POST /manage/2fa` - Двофакторна аутентифікація
- `GET /manage/info` - Інформація про користувача

### Кастомні ендпоінти:
- `GET /auth/profile` - Профіль користувача
- `PUT /auth/profile` - Оновлення профілю
- `GET /auth/users` - Список користувачів

## 🛠️ Технології

- **ASP.NET Core 9.0** - Веб-фреймворк
- **ASP.NET Core Identity** - Система аутентифікації
- **Entity Framework Core** - ORM для роботи з базою даних
- **SQLite** - База даних (для простоти розробки)
- **Swagger/OpenAPI** - Документація API
- **Bearer Token Authentication** - Токенна аутентифікація для API
- **Cookie Authentication** - Аутентифікація через cookies для веб-додатків

## 📦 Встановлення та запуск

### 1. Клонування репозиторію
```bash
git clone <repository-url>
cd AspNet.MinimalApi.IdentityAuth
```

### 2. Встановлення залежностей
```bash
dotnet restore
```

### 3. Створення міграцій (опціонально)
```bash
dotnet ef migrations add InitialCreate
```

### 4. Запуск додатку
```bash
dotnet run
```

### 5. Відкриття в браузері
- API: `https://localhost:7000`
- Swagger: `https://localhost:7000/swagger`

## 🧪 Тестування

### Використання HTTP файлу
Відкрийте файл `IdentityAuth.http` в IDE та виконайте запити.

### Використання Swagger
1. Перейдіть на `https://localhost:7000/swagger`
2. Зареєструйте користувача через `/register`
3. Увійдіть через `/login` та скопіюйте токен
4. Натисніть "Authorize" в Swagger та вставте токен
5. Тестуйте захищені ендпоінти

### Приклад реєстрації
```json
POST /register
{
  "email": "student@example.com",
  "password": "Student123!"
}
```

### Приклад логіну
```json
POST /login
{
  "email": "student@example.com", 
  "password": "Student123!"
}
```

## 🔐 Безпека

### Налаштування паролів:
- Мінімальна довжина: 6 символів
- Вимагаються: великі літери, малі літери, цифри
- Не вимагаються: спеціальні символи

### Налаштування блокування:
- Максимум невдалих спроб: 5
- Час блокування: 15 хвилин

### Налаштування токенів:
- Термін дії Bearer токена: 1 година
- Термін дії Refresh токена: 7 днів
- Термін дії Cookie: 7 днів

## 📚 Навчальні матеріали

### Ключові концепції:
1. **IdentityUser** - Базова модель користувача
2. **IdentityDbContext** - Контекст бази даних для Identity
3. **AddIdentityCore** - Налаштування Identity сервісів
4. **AddAuthentication** - Налаштування аутентифікації
5. **MapIdentityApi** - Автоматичне створення API ендпоінтів
6. **RequireAuthorization** - Захист ендпоінтів

### Корисні посилання:
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Minimal APIs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

## 🐛 Відомі проблеми

1. **SQLite обмеження** - У продакшені краще використовувати PostgreSQL або SQL Server
2. **Email підтвердження** - Не налаштовано відправку email (потрібен SMTP)
3. **HTTPS сертифікати** - Можуть виникати попередження в браузері

## 🔄 Подальший розвиток

- [ ] Додати ролі користувачів
- [ ] Реалізувати відправку email
- [ ] Додати двофакторну аутентифікацію
- [ ] Інтегрувати зовнішніх провайдерів (Google, Facebook)
- [ ] Додати логування
- [ ] Покрити тестами

## 📝 Ліцензія

Цей проєкт призначений для навчальних цілей.
