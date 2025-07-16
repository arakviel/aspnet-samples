# ASP.NET Core Identity Authentication with Email Confirmation - Minimal API

Цей проєкт демонструє реалізацію аутентифікації через **ASP.NET Core Identity** в **Minimal API** з **обов'язковим підтвердженням електронної пошти** для .NET 9.

## 🎯 Мета проєкту

Показати студентам, як правильно інтегрувати ASP.NET Core Identity в Minimal API додаток з підтвердженням email та детальними поясненнями кожного кроку.

## 🏗️ Архітектура проєкту

```
AspNet.MinimalApi.IdentityWithEmailConfirmationAuth/
├── Models/
│   └── User.cs                    # Модель користувача (розширює IdentityUser)
├── Data/
│   └── ApplicationDbContext.cs    # Контекст бази даних для Identity
├── Extensions/
│   └── MigrationExtensions.cs     # Розширення для міграцій
├── Services/
│   ├── IEmailService.cs          # Інтерфейс email сервісу
│   └── EmailService.cs           # Реалізація email сервісу
├── Program.cs                     # Головний файл з налаштуваннями
├── appsettings.json              # Конфігурація додатку
├── appsettings.Development.json  # Конфігурація для розробки
├── EmailConfirmationAuth.http    # Файл для тестування API
└── README.md                     # Цей файл
```

## 🚀 Основні функції

### Автоматичні ендпоінти Identity:
- `POST /register` - Реєстрація користувача (з відправкою email підтвердження)
- `POST /login` - Вхід в систему (тільки після підтвердження email)
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
- `GET /auth/email-status` - Статус підтвердження email
- `POST /auth/resend-email-confirmation` - Кастомна повторна відправка підтвердження

## 🛠️ Технології

- **ASP.NET Core 9.0** - Веб-фреймворк
- **ASP.NET Core Identity** - Система аутентифікації
- **Entity Framework Core** - ORM для роботи з базою даних
- **SQLite** - База даних (для простоти розробки)
- **MailKit** - Бібліотека для відправки email
- **MimeKit** - Бібліотека для створення email повідомлень
- **Swagger/OpenAPI** - Документація API
- **Bearer Token Authentication** - Токенна аутентифікація для API

## 📦 Встановлення та запуск

### 1. Клонування репозиторію
```bash
git clone <repository-url>
cd AspNet.MinimalApi.IdentityWithEmailConfirmationAuth
```

### 2. Встановлення залежностей
```bash
dotnet restore
```

### 3. Налаштування email (опціонально для розробки)
Відредагуйте `appsettings.Development.json`:
```json
{
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": "1025",
    "EnableSsl": "false"
  }
}
```

Для тестування можна використати [MailHog](https://github.com/mailhog/MailHog):
```bash
# Встановлення MailHog (локальний SMTP сервер для тестування)
go install github.com/mailhog/MailHog@latest
# Запуск MailHog
MailHog
# Веб-інтерфейс: http://localhost:8025
```

### 4. Запуск додатку
```bash
dotnet run
```

### 5. Відкрийте в браузері:
- 🌐 **API**: https://localhost:7084
- 📖 **Swagger**: https://localhost:7084/swagger

## 🔐 Ключові відмінності від базового проєкту

### 1. **Обов'язкове підтвердження email**
```csharp
options.SignIn.RequireConfirmedEmail = true; // УВІМКНЕНО!
```

### 2. **Email сервіс**
- Інтерфейс `IEmailService` для абстракції
- Реалізація `EmailService` з використанням MailKit
- Підтримка HTML та текстових повідомлень

### 3. **Розширена модель користувача**
```csharp
public class User : IdentityUser
{
    public DateTime? EmailConfirmedAt { get; set; } // Додаткове поле
    // ... інші поля
}
```

### 4. **Кастомні ендпоінти**
- Статус підтвердження email
- Повторна відправка підтвердження
- Розширена інформація про користувача

## 🧪 Тестування

### Використання HTTP файлу:
1. Відкрийте `EmailConfirmationAuth.http` в IDE
2. Виконайте запити послідовно
3. Замініть токени та ID на реальні значення

### Послідовність тестування:
1. **Реєстрація** → Email відправлено
2. **Спроба входу** → Помилка (email не підтверджено)
3. **Підтвердження email** → Перехід за посиланням
4. **Вхід** → Успішно після підтвердження
5. **Доступ до захищених ресурсів** → З Bearer токеном

## 📧 Налаштування email

### Для Gmail:
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "EnableSsl": "true"
  }
}
```

### Для розробки (MailHog):
```json
{
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": "1025",
    "EnableSsl": "false"
  }
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

## 📚 Навчальні матеріали

### Ключові концепції:
1. **RequireConfirmedEmail** - Обов'язкове підтвердження email
2. **IEmailService** - Сервіс для відправки email
3. **EmailConfirmationTokenProvider** - Провайдер токенів підтвердження
4. **MailKit/MimeKit** - Бібліотеки для роботи з email
5. **SMTP Configuration** - Налаштування SMTP сервера

### Потік підтвердження email:
1. Користувач реєструється
2. Генерується токен підтвердження
3. Відправляється email з посиланням
4. Користувач переходить за посиланням
5. Email підтверджується в базі даних
6. Користувач може увійти в систему

## 🎓 Завдання для практики

### Легкий рівень:
- [ ] Додайте поле "Дата народження" до користувача
- [ ] Створіть ендпоінт для зміни email
- [ ] Додайте валідацію міцності пароля

### Середній рівень:
- [ ] Реалізуйте ролі користувачів (Admin, User)
- [ ] Додайте ендпоінт тільки для адмінів
- [ ] Створіть middleware для логування email відправок

### Складний рівень:
- [ ] Додайте двофакторну аутентифікацію
- [ ] Інтегруйте Google OAuth
- [ ] Реалізуйте підтвердження зміни email

## 🚨 Поширені помилки

1. **Не налаштований SMTP** - Перевірте налаштування email
2. **Email не підтверджено** - Перевірте, чи перейшли за посиланням
3. **Токен застарів** - Згенеруйте новий токен підтвердження
4. **Неправильний URL** - Перевірте URL в email посиланні

## 💡 Корисні команди

```bash
# Створення міграції
dotnet ef migrations add AddEmailConfirmation

# Застосування міграцій
dotnet ef database update

# Видалення бази даних
dotnet ef database drop

# Перегляд логів
dotnet run --verbosity detailed
```
