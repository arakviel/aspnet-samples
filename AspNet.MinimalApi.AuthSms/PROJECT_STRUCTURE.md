# Структура проекту SMS Authentication API

## Огляд файлів

```
AspNet.MinimalApi.AuthSms/
├── 📁 Configuration/
│   └── TwilioSettings.cs              # Конфігураційні класи для Twilio та SMS
├── 📁 Data/
│   └── ApplicationDbContext.cs        # EF Core контекст з Identity
├── 📁 DTOs/
│   └── AuthDtos.cs                   # DTO для API запитів та відповідей
├── 📁 Endpoints/
│   └── AuthEndpoints.cs              # Minimal API endpoints
├── 📁 Models/
│   ├── ApplicationUser.cs            # Розширена модель користувача
│   └── SmsVerificationCode.cs        # Модель SMS кодів верифікації
├── 📁 Services/
│   ├── IAuthService.cs               # Інтерфейс сервісу аутентифікації
│   ├── AuthService.cs                # Реалізація логіки аутентифікації
│   ├── ISmsService.cs                # Інтерфейс SMS сервісу
│   ├── SmsService.cs                 # Demo SMS сервіс (фейк)
│   ├── TwilioSmsService.cs           # Реальний Twilio SMS сервіс
│   └── SmsServiceFactory.cs          # Фабрика для вибору SMS провайдера
├── 📄 Program.cs                     # Конфігурація додатку та DI
├── 📄 appsettings.json               # Основна конфігурація
├── 📄 appsettings.Development.json   # Конфігурація для розробки
├── 📄 AspNet.MinimalApi.AuthSms.csproj # Файл проекту з пакетами
├── 📄 README.md                      # Основна документація
├── 📄 ARCHITECTURE.md                # Архітектурна документація
├── 📄 TWILIO_SETUP.md               # Інструкції з налаштування Twilio
├── 📄 PROJECT_STRUCTURE.md          # Цей файл
├── 📄 test-api.sh                   # Тестовий скрипт (Demo режим)
├── 📄 test-twilio.sh                # Тестовий скрипт (Twilio режим)
└── 📄 SMS-Auth-API.postman_collection.json # Postman колекція
```

## Опис ключових файлів

### 🔧 Конфігурація

**Program.cs**
- Налаштування DI контейнера
- Конфігурація Identity, JWT, EF Core
- Вибір SMS провайдера на основі конфігурації
- Налаштування Swagger та CORS

**appsettings.json**
- JWT налаштування
- Twilio конфігурація для production
- SMS налаштування

**appsettings.Development.json**
- Перевизначає провайдера на "Demo" для розробки
- Налаштування логування

### 📊 Моделі та DTO

**Models/ApplicationUser.cs**
- Розширює IdentityUser
- Додає FirstName, LastName, CreatedAt, LastLoginAt
- Підтримує підтвердження телефону

**Models/SmsVerificationCode.cs**
- Зберігає SMS коди з терміном дії
- Підтримує різні цілі (реєстрація, вхід)
- Одноразове використання

**DTOs/AuthDtos.cs**
- SendSmsRequest, VerifySmsRequest
- RegisterRequest, LoginRequest
- AuthResponse, UserInfo, ApiResponse<T>

### 🗄️ Дані

**Data/ApplicationDbContext.cs**
- Наслідує IdentityDbContext<ApplicationUser>
- Налаштовує індекси для оптимізації
- Конфігурує зв'язки між таблицями

### 🔧 Сервіси

**Services/IAuthService.cs + AuthService.cs**
- Основна логіка аутентифікації
- Генерація та валідація SMS кодів
- Створення JWT токенів
- Управління користувачами

**Services/ISmsService.cs**
- Загальний інтерфейс для SMS провайдерів
- SendSmsAsync(), SendVerificationCodeAsync()

**Services/SmsService.cs**
- Demo реалізація (логує в консоль)
- Використовується для розробки та тестування

**Services/TwilioSmsService.cs**
- Реальна інтеграція з Twilio API
- Обробка помилок та логування
- Налаштовується через TwilioSettings

**Services/SmsServiceFactory.cs**
- Фабрика для вибору SMS провайдера
- Альтернативний підхід до DI реєстрації

### 🌐 API

**Endpoints/AuthEndpoints.cs**
- Minimal API endpoints
- Валідація вхідних даних
- Обробка помилок
- Swagger документація

### 📋 Тестування

**test-api.sh**
- Автоматичне тестування в Demo режимі
- Повний цикл: реєстрація → вхід → отримання інформації
- Використовує curl та jq

**test-twilio.sh**
- Тестування з реальними SMS через Twilio
- Інтерактивний ввід SMS кодів
- Перевірки та попередження

**SMS-Auth-API.postman_collection.json**
- Готова колекція для Postman
- Автоматичне збереження токенів
- Всі endpoints з прикладами

### 📚 Документація

**README.md**
- Основна документація проекту
- Швидкий старт та приклади
- API endpoints та тестування

**ARCHITECTURE.md**
- Детальний опис архітектури
- Діаграми потоків аутентифікації
- Рекомендації для production

**TWILIO_SETUP.md**
- Покрокова інструкція налаштування Twilio
- Безпека та найкращі практики
- Обробка помилок та моніторинг

## Залежності (NuGet пакети)

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.8" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.8" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.3" />
<PackageReference Include="Twilio" Version="7.12.0" />
```

## Особливості архітектури

### 🔄 Dependency Injection
- Сервіси реєструються через інтерфейси
- Вибір SMS провайдера на основі конфігурації
- Легко тестувати та замінювати компоненти

### 🛡️ Безпека
- JWT токени з підписом
- Валідація всіх вхідних даних
- Rate limiting для SMS
- Хешування паролів через Identity

### 📈 Масштабованість
- Чітке розділення відповідальностей
- Легко додавати нові SMS провайдери
- Підтримка різних баз даних через EF Core

### 🧪 Тестованість
- Інтерфейси для всіх сервісів
- Demo режим для розробки
- Автоматичні тестові скрипти

## Наступні кроки для розвитку

1. **Додати unit тести**
   - xUnit проект
   - Мокування сервісів
   - Тестування контролерів

2. **Додати integration тести**
   - TestServer для API тестів
   - In-memory база даних
   - Тестування повних сценаріїв

3. **Покращити безпеку**
   - Rate limiting middleware
   - CAPTCHA для запобігання зловживанням
   - IP whitelist/blacklist

4. **Додати моніторинг**
   - Application Insights
   - Structured logging з Serilog
   - Health checks

5. **Розширити функціональність**
   - Refresh токени
   - Двофакторна аутентифікація
   - Соціальні мережі (OAuth)
   - Email верифікація
