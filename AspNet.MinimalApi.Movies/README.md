# ASP.NET Core Minimal API Movies

Цей проект демонструє створення REST API для фільмів з використанням ASP.NET Core Minimal API, Entity Framework Core, SQLite, Repository Pattern та сервісів.

## Особливості

- ✅ **Minimal API** - сучасний підхід до створення API в ASP.NET Core
- ✅ **Entity Framework Core** з SQLite для зберігання даних
- ✅ **Repository Pattern** для абстракції доступу до даних
- ✅ **Service Layer** для бізнес-логіки
- ✅ **Dependency Injection** для управління залежностями
- ✅ **CORS** підтримка для фронтенду
- ✅ **Seed Data** з 15 популярними фільмами для демонстрації пагінації
- ✅ **Web Components** фронтенд
- ✅ **Serilog** структуроване логування
- ✅ **Глобальна обробка помилок** з RFC Problem Details
- ✅ **Кастомні винятки** для різних типів помилок

## Структура проекту

```
AspNet.MinimalApi.Movies/
├── Data/                    # Контекст бази даних
│   └── ApplicationDbContext.cs
├── Dtos/                   # Data Transfer Objects для API відповідей
│   └── MovieSearchResponseDto.cs
├── Exceptions/             # Кастомні винятки
│   ├── MovieNotFoundException.cs
│   ├── MovieAlreadyExistsException.cs
│   └── ValidationException.cs
├── Middleware/             # Middleware компоненти
│   └── GlobalExceptionMiddleware.cs
├── Models/                  # Моделі даних (сутності)
│   └── Movie.cs
├── Repositories/            # Репозиторії для доступу до даних
│   ├── IMovieRepository.cs  # SearchAsync, GetByIdAsync, CreateAsync, etc.
│   └── MovieRepository.cs
├── Services/               # Бізнес-логіка
│   ├── IMovieService.cs     # SearchAsync, GetDetailsAsync, CreateAsync, etc.
│   └── MovieService.cs
├── wwwroot/               # Статичні файли та фронтенд
│   ├── src/
│   │   ├── components/    # Web Components
│   │   ├── services/      # API клієнт
│   │   └── style.css
│   └── index.html
├── Program.cs             # Налаштування та API endpoints
├── Movies.http            # HTTP файл для тестування API
├── appsettings.json       # Конфігурація для Production
├── appsettings.Development.json  # Конфігурація для Development
├── appsettings.Production.json   # Додаткова конфігурація для Production
├── logs/                  # Файли логів Serilog
│   ├── movies-dev-YYYYMMDD.log   # Development логи
│   └── movies-prod-YYYYMMDD.log  # Production логи
└── movies.db             # SQLite база даних
```

## API Endpoints

### Пошук фільмів
```
GET /api/movies/search?s={title}&page={page}
```

### Отримання деталей фільму
```
GET /api/movies/{imdbId}
```

### Створення фільму
```
POST /api/movies
Content-Type: application/json
```

### Оновлення фільму
```
PUT /api/movies/{id}
Content-Type: application/json
```

### Видалення фільму
```
DELETE /api/movies/{id}
```

## Запуск проекту

1. Встановіть залежності:
```bash
dotnet restore
```

2. Запустіть проект:
```bash
dotnet run
```

3. Відкрийте браузер: http://localhost:5049

## Seed Data

Проект автоматично створює базу даних з 15 популярними фільмами:

1. **The Shawshank Redemption** (1994) - 9.3 ⭐
2. **The Godfather** (1972) - 9.2 ⭐
3. **The Dark Knight** (2008) - 9.0 ⭐
4. **Pulp Fiction** (1994) - 8.9 ⭐
5. **Forrest Gump** (1994) - 8.8 ⭐
6. **Inception** (2010) - 8.8 ⭐
7. **Fight Club** (1999) - 8.8 ⭐
8. **The Matrix** (1999) - 8.7 ⭐
9. **Goodfellas** (1990) - 8.7 ⭐
10. **The Lord of the Rings: The Return of the King** (2003) - 9.0 ⭐
11. **Star Wars: Episode IV - A New Hope** (1977) - 8.6 ⭐
12. **Interstellar** (2014) - 8.6 ⭐
13. **Parasite** (2019) - 8.5 ⭐
14. **The Avengers** (2012) - 8.0 ⭐
15. **Titanic** (1997) - 7.9 ⭐

**Пагінація:** 10 фільмів на сторінку (сторінка 1: фільми 1-10, сторінка 2: фільми 11-15)

## Тестування API

Використовуйте файл `Movies.http` для тестування всіх endpoints:

### Успішні запити:
- Пошук фільмів за назвою
- Отримання деталей фільму
- Створення нового фільму
- Оновлення існуючого фільму
- Видалення фільму

### Тестування помилок:
- 400 Bad Request (валідаційні помилки)
- 404 Not Found (неіснуючі ресурси)
- 409 Conflict (дублікати)
- 405 Method Not Allowed (неправильні HTTP методи)

**Примітка:** Всі помилки повертаються у стандартному форматі RFC Problem Details.

### Як використовувати Movies.http:

1. **VS Code**: Встановіть розширення "REST Client"
2. **JetBrains IDEs**: Вбудована підтримка HTTP файлів
3. **Інші редактори**: Скопіюйте запити та використовуйте curl/Postman

### Приклади з Movies.http:

```http
### Успішний пошук
GET http://localhost:5049/api/movies/search?s=Dark

### Помилка валідації
GET http://localhost:5049/api/movies/search?s=Dark&page=0

### Створення фільму
POST http://localhost:5049/api/movies
Content-Type: application/json

{
  "title": "New Movie",
  "year": "2023",
  "imdbId": "tt1234567",
  "imdbRating": 8.5
}
```

## Налаштування середовищ

Проект автоматично визначає середовище та використовує відповідні файли:

- **Development**: використовує файли з `wwwroot/` (src папка)
- **Production**: використовує зібрані файли з `wwwroot/dist/` папки

Для Production збірки фронтенду:
```bash
cd wwwroot
npm run build
```

## Тестування API

### Пошук фільмів:
```bash
curl "http://localhost:5049/api/movies/search?s=Dark"
```

### Отримання деталей:
```bash
curl "http://localhost:5049/api/movies/tt0468569"
```

### Додавання фільму:
```bash
curl -X POST "http://localhost:5049/api/movies" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "New Movie",
    "year": "2023",
    "imdbId": "tt1234567",
    "type": "movie",
    "poster": "https://example.com/poster.jpg",
    "plot": "Movie description",
    "genre": "Action",
    "director": "Director Name",
    "actors": "Actor Names"
  }'
```

## Архітектурні рішення

### Розділення відповідальностей:
- **Models** - сутності бази даних (Movie)
- **DTOs** - об'єкти для API відповідей (MovieSearchResponseDto, MovieSearchItemDto)
- **Repositories** - доступ до даних з чистими назвами методів
- **Services** - бізнес-логіка та маппінг між моделями та DTO
- **Minimal API** - endpoints без контролерів

## Логування та обробка помилок

### Serilog конфігурація (з appsettings.json):

**Development (appsettings.Development.json):**
- **Рівень логування**: Debug для додатку, Information для Microsoft
- **Console sink**: кольорові логи з детальним форматом
- **File sink**: `logs/movies-dev-YYYYMMDD.log` (7 днів, 50MB файли)
- **Додаткові деталі**: Properties, детальні SQL запити

**Production (appsettings.json + appsettings.Production.json):**
- **Рівень логування**: Warning для додатку, Error для Microsoft
- **Console sink**: мінімальний формат без кольорів
- **File sink**: `logs/movies-prod-YYYYMMDD.log` (90 днів, 100MB файли)
- **Оптимізація**: тільки критичні помилки та попередження

**Загальні особливості:**
- **Structured logging** - структуровані дані з контекстом
- **Enrichers**: Environment, ProcessId, ThreadId
- **Ротація файлів** по дням з автоматичним видаленням старих
- **Різні формати** для консолі та файлів

### Глобальна обробка помилок (еволюція підходів):

**1. Кастомний Middleware (GlobalExceptionMiddleware):**
- Повний контроль над обробкою помилок
- Власна логіка маппінгу винятків
- Демонстрація принципів роботи middleware

**2. Вбудований UseExceptionHandler (поточна реалізація):**
- Використання стандартного ASP.NET Core підходу
- Менше boilerplate коду
- Інтеграція з IExceptionHandlerFeature

**Спільні особливості:**
- **RFC Problem Details** - стандартний формат помилок
- **Кастомні винятки**: MovieNotFoundException, ValidationException, MovieAlreadyExistsException
- **Автоматичний маппінг** винятків на HTTP статус коди
- **Детальна інформація** про помилки в логах

### Приклади відповідей на помилки:

**404 Not Found:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Фільм не знайдено",
  "status": 404,
  "detail": "Фільм з IMDB ID 'tt9999999' не знайдено",
  "instance": "/api/movies/tt9999999",
  "imdbId": "tt9999999"
}
```

**400 Validation Error:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Помилка валідації",
  "status": 400,
  "detail": "Помилка валідації фільму",
  "instance": "/api/movies",
  "errors": {
    "title": ["Назва фільму є обов'язковою"],
    "imdbRating": ["Рейтинг IMDB повинен бути від 0 до 10"]
  }
}
```

### Порівняння підходів до обробки помилок:

| Аспект | Кастомний Middleware | UseExceptionHandler |
|--------|---------------------|-------------------|
| **Складність** | Більше коду | Менше коду |
| **Контроль** | Повний контроль | Стандартний підхід |
| **Тестування** | Потребує окремих тестів | Вбудоване тестування |
| **Підтримка** | Власна підтримка | Microsoft підтримка |
| **Гнучкість** | Максимальна | Достатня для більшості випадків |
| **Продуктивність** | Залежить від реалізації | Оптимізовано Microsoft |

**Рекомендація:** Використовуйте `UseExceptionHandler` для більшості проектів. Кастомний middleware тільки для специфічних вимог.

### Приклади конфігурації логування:

**appsettings.Development.json:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "AspNet.MinimalApi.Movies": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/movies-dev-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

**appsettings.Production.json:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Error",
        "AspNet.MinimalApi.Movies": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/movies-prod-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90
        }
      }
    ]
  }
}
```

## Технології

- **ASP.NET Core 9.0** - веб-фреймворк
- **Entity Framework Core** - ORM
- **SQLite** - база даних
- **Serilog** - структуроване логування
- **Web Components** - фронтенд
- **Vanilla JavaScript** - без додаткових фреймворків

## Переваги цього підходу

1. **Простота** - Minimal API зменшує кількість boilerplate коду
2. **Продуктивність** - швидкий запуск та виконання
3. **Тестованість** - чітке розділення відповідальностей
4. **Масштабованість** - легко додавати нові функції
5. **Сучасність** - використання найновіших можливостей .NET
6. **Гнучкість середовищ** - автоматичне перемикання між dev/prod файлами
7. **Clean URLs** - підтримка чистих URL без /index.html
8. **Чисті назви методів** - без дублювання назви сутності (SearchAsync замість SearchMoviesAsync)
9. **DTO Pattern** - окремі класи для API відповідей, відділені від моделей даних
10. **Структуроване логування** - Serilog з різними рівнями та форматами
11. **Глобальна обробка помилок** - централізована обробка з RFC Problem Details
12. **Кастомні винятки** - спеціалізовані винятки для різних бізнес-сценаріїв
