# 🚀 ASP.NET Core Minimal API - Простий HTTP Сервер

Це демонстрація того ж HTTP сервера, що і в `NativeHttpServerSimple`, але створеного з використанням ASP.NET Core Minimal API. Проєкт показує, наскільки простішим стає код при використанні сучасного веб-фреймворку.

## 🎯 Порівняння з нативним сервером

| Характеристика | Нативний HTTP Сервер | ASP.NET Core Minimal API |
|---|---|---|
| **Кількість рядків коду** | ~590 рядків | ~150 рядків |
| **Економія коду** | - | **75% менше коду!** 🎉 |
| **Складність** | Висока | Низька |
| **Підтримка** | Важка | Легка |

## ✨ Переваги ASP.NET Core Minimal API

### 🔧 **Автоматизація**
- ✅ **Роутинг**: `app.MapGet("/path", handler)` замість ручного switch
- ✅ **Query String**: `request.Query` замість ручного парсингу
- ✅ **Model Binding**: `[FromForm] IFormCollection` замість ручного читання
- ✅ **HTTP заголовки**: `request.Headers` з типізованим доступом
- ✅ **Логування**: Вбудоване структуроване логування

### 🛡️ **Безпека та надійність**
- ✅ **Anti-forgery захист**: Автоматичний захист від CSRF
- ✅ **Валідація**: Вбудована валідація моделей
- ✅ **Обробка помилок**: Централізована обробка винятків
- ✅ **HTTPS**: Легке налаштування SSL/TLS

### 🚀 **Продуктивність**
- ✅ **Kestrel**: Високопродуктивний веб-сервер
- ✅ **Middleware Pipeline**: Ефективна обробка запитів
- ✅ **Dependency Injection**: Вбудований DI контейнер
- ✅ **Configuration**: Гнучка система конфігурації

## 🌐 Функціональність

Сервер реалізує ту ж функціональність, що і нативний:

### 🏠 Головна сторінка (`/`)
- Порівняння з нативним сервером
- Навігаційне меню
- Інформація про поточний запит

### ℹ️ Інформація про запит (`/info`)
- Детальна інформація про HTTP запит
- URL компоненти
- Метадані запиту

### 📋 HTTP заголовки (`/headers`)
- Всі HTTP заголовки запиту
- Автоматичний розбір через `request.Headers`

### 🔍 Query String (`/query`)
- Автоматичний розбір через `request.Query`
- Приклади з українськими символами

### 📝 Форма (`/form` та `/submit`)
- HTML форма з різними полями
- Автоматичний model binding через `[FromForm]`
- Захист від CSRF (вимкнений для простоти)

## 🚀 Запуск

1. Переконайтеся, що у вас встановлено .NET 9.0 SDK
2. Перейдіть до директорії проєкту:
   ```bash
   cd AspNet.MinimalApiSimple
   ```
3. Запустіть проєкт:
   ```bash
   dotnet run
   ```
4. Відкрийте браузер і перейдіть до `http://localhost:8083`

## 🧪 Тестування

### Через браузер:
- `http://localhost:8083/` - головна сторінка
- `http://localhost:8083/info` - інформація про запит
- `http://localhost:8083/headers` - HTTP заголовки
- `http://localhost:8083/query?name=Іван&age=25` - query параметри
- `http://localhost:8083/form` - тестова форма

### Через curl:
```bash
# Головна сторінка
curl http://localhost:8083/

# Query параметри
curl "http://localhost:8083/query?name=Тест&city=Київ"

# POST запит
curl -X POST http://localhost:8083/submit \
  -d "name=Іван&email=ivan@test.com&message=Привіт" \
  -H "Content-Type: application/x-www-form-urlencoded"
```

## 💻 Ключові відмінності в коді

### Роутинг
**Нативний сервер:**
```csharp
string htmlContent = path switch
{
    "/" => GenerateHomePage(request),
    "/info" => GenerateInfoPage(request),
    // ...
    _ => GenerateNotFoundPage(request)
};
```

**ASP.NET Core Minimal API:**
```csharp
app.MapGet("/", (HttpContext context) => 
    Results.Content(GenerateHomePage(context.Request), "text/html; charset=utf-8"));
app.MapGet("/info", (HttpContext context) => 
    Results.Content(GenerateInfoPage(context.Request), "text/html; charset=utf-8"));
```

### Query String
**Нативний сервер:**
```csharp
var queryParams = HttpUtility.ParseQueryString(queryString);
foreach (string key in queryParams.AllKeys)
{
    string value = queryParams[key] ?? "";
    // Ручна обробка
}
```

**ASP.NET Core Minimal API:**
```csharp
foreach (var param in request.Query)
{
    var values = string.Join(", ", param.Value.ToArray());
    // Автоматичний розбір
}
```

### POST дані
**Нативний сервер:**
```csharp
using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
string formData = await reader.ReadToEndAsync();
var formParams = HttpUtility.ParseQueryString(formData);
```

**ASP.NET Core Minimal API:**
```csharp
app.MapPost("/submit", async (HttpContext context, [FromForm] IFormCollection form) => 
    Results.Content(await HandleFormSubmit(context, form), "text/html; charset=utf-8"))
    .DisableAntiforgery();
```

## 🎓 Висновки

### ✅ Переваги ASP.NET Core Minimal API:
1. **Менше коду**: 75% економії коду
2. **Простіша підтримка**: Менше місць для помилок
3. **Краща продуктивність**: Оптимізований Kestrel сервер
4. **Безпека**: Вбудовані механізми захисту
5. **Екосистема**: Величезна кількість готових рішень
6. **Тестування**: Легке unit та integration тестування
7. **Документація**: Автоматична генерація OpenAPI/Swagger

### 🤔 Коли використовувати нативний підхід:
- Навчальні цілі (розуміння основ)
- Мікросервіси з мінімальними залежностями
- Embedded системи з обмеженими ресурсами
- Специфічні вимоги до продуктивності

### 🎯 Рекомендації:
Для реальних проєктів **завжди використовуйте ASP.NET Core** або інші сучасні фреймворки. Нативний підхід корисний лише для розуміння того, як працюють веб-сервери "під капотом".

## 🔄 Наступні кроки

Цей проєкт можна розширити:
1. Додати Swagger/OpenAPI документацію
2. Підключити Entity Framework для роботи з базою даних
3. Додати автентифікацію та авторизацію
4. Реалізувати API versioning
5. Додати кешування та логування
6. Написати unit та integration тести
