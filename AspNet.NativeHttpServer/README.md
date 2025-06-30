# Native HTTP Server Demo

Це демонстраційний проєкт простого HTTP сервера, побудованого на чистій .NET бібліотеці `HttpListener` без використання ASP.NET Core або інших веб-фреймворків.

## Особливості

- ✅ Нативний HTTP сервер з використанням `HttpListener`
- ✅ RESTful API ендпоінти
- ✅ HTML представлення та JSON відповіді
- ✅ Витягування параметрів з маршрутів
- ✅ In-memory зберігання даних
- ✅ CRUD операції для продуктів
- ✅ Простий роутинг з підтримкою параметрів
- ✅ Обробка помилок та валідація

## Архітектура

Проєкт організований за принципами чистої архітектури:

```
AspNet.NativeHttpServer/
├── Models/                 # Моделі даних
│   └── Product.cs
├── Repositories/           # Шар доступу до даних
│   ├── IProductRepository.cs
│   └── InMemoryProductRepository.cs
├── Services/              # Бізнес-логіка
│   └── ProductService.cs
├── Controllers/           # HTTP контролери
│   └── ProductController.cs
├── Http/                  # HTTP інфраструктура
│   ├── HttpContext.cs
│   ├── HttpServer.cs
│   └── Router.cs
├── Views/                 # HTML шаблони
│   └── ProductViews.cs
└── Program.cs            # Точка входу
```

## API Ендпоінти

### HTML Представлення
- `GET /` - Головна сторінка
- `GET /products` - Список всіх продуктів (HTML)
- `GET /products/{id}` - Деталі продукту (HTML)

### JSON API
- `GET /api/products` - Отримати всі продукти (JSON)
- `GET /api/products/{id}` - Отримати продукт за ID (JSON)
- `POST /api/products` - Створити новий продукт (JSON)
- `PUT /api/products/{id}` - Оновити продукт (JSON)
- `DELETE /api/products/{id}` - Видалити продукт (JSON)

## Запуск

1. Переконайтеся, що у вас встановлено .NET 9.0 SDK
2. Перейдіть до директорії проєкту:
   ```bash
   cd AspNet.NativeHttpServer
   ```
3. Запустіть проєкт:
   ```bash
   dotnet run
   ```
4. Відкрийте браузер і перейдіть до `http://localhost:8081`

## Тестування

Проєкт містить два типи тестів:

### Unit тести
Тестують бізнес-логіку сервісів:
```bash
dotnet test --filter "ProductServiceTests"
```

### HTTP інтеграційні тести
Тестують HTTP ендпоінти через HttpClient:
```bash
dotnet test --filter "HttpIntegrationTests"
```

### Запуск всіх тестів
```bash
dotnet test AspNet.NativeHttpServer.Tests/
```

## Приклади використання API

### Отримати всі продукти
```bash
curl http://localhost:8080/api/products
```

### Отримати продукт за ID
```bash
curl http://localhost:8080/api/products/1
```

### Створити новий продукт
```bash
curl -X POST http://localhost:8080/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Product",
    "description": "Product description",
    "price": 99.99,
    "stock": 10
  }'
```

### Оновити продукт
```bash
curl -X PUT http://localhost:8080/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Product",
    "description": "Updated description",
    "price": 149.99,
    "stock": 5
  }'
```

### Видалити продукт
```bash
curl -X DELETE http://localhost:8080/api/products/1
```

## Ключові компоненти

### HttpServer
Основний HTTP сервер, що використовує `HttpListener` для обробки запитів.

### Router
Простий роутер з підтримкою параметрів у маршрутах (наприклад, `/products/{id}`).

### HttpContext
Обгортка навколо `HttpListenerContext` для зручної роботи з запитами та відповідями.

### ProductService
Сервіс з бізнес-логікою для роботи з продуктами, включаючи валідацію.

### InMemoryProductRepository
In-memory реалізація репозиторію з початковими тестовими даними.

## Порівняння з ASP.NET Core

Цей проєкт демонструє, скільки коду потрібно написати для створення простого веб-сервера без використання ASP.NET Core. Він буде використовуватися як "до" для порівняння з ASP.NET Core Minimal API ("після").

## Обмеження

- Відсутність middleware pipeline
- Ручна обробка серіалізації/десеріалізації JSON
- Відсутність dependency injection контейнера
- Ручна обробка помилок
- Відсутність автоматичної валідації моделей
- Відсутність підтримки HTTPS (можна додати)
- Відсутність логування (можна додати)
