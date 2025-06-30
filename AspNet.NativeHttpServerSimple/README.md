# 🌐 Простий HTTP Сервер на нативних бібліотеках .NET

Це демонстрація найпростішого HTTP сервера, створеного на чистому .NET без використання ASP.NET Core або інших веб-фреймворків. Весь код знаходиться в одному файлі `Program.cs`.

## 🎯 Мета проєкту

Показати як працює HTTP сервер "під капотом" та продемонструвати базові концепції веб-розробки:
- HTTP запити та відповіді
- Роутинг (маршрутизація)
- Query String параметри
- HTTP заголовки
- POST запити та форми
- Обробка помилок (404)

## ✨ Функціональність

### 🏠 Головна сторінка (`/`)
- Привітання та опис сервера
- Навігаційне меню
- Інформація про поточний запит

### ℹ️ Інформація про запит (`/info`)
- Детальна інформація про HTTP запит
- URL компоненти (схема, хост, порт, шлях)
- Метадані запиту (User-Agent, Content-Type, тощо)

### 📋 HTTP заголовки (`/headers`)
- Показує всі HTTP заголовки запиту
- Кількість заголовків
- Таблиця з назвами та значеннями

### 🔍 Query String (`/query`)
- Розбір параметрів з URL
- Демонстрація кодування українських символів
- Приклади для тестування

### 📝 Форма (`/form`)
- HTML форма з різними типами полів
- Демонстрація POST запитів

### ✅ Обробка форми (`/submit`)
- Обробка POST даних
- Показ отриманих даних з форми
- Інформація про запит

### 🚫 404 сторінка
- Красива сторінка помилки
- Список доступних маршрутів

## 🚀 Запуск

1. Переконайтеся, що у вас встановлено .NET 9.0 SDK
2. Перейдіть до директорії проєкту:
   ```bash
   cd AspNet.NativeHttpServerSimple
   ```
3. Запустіть проєкт:
   ```bash
   dotnet run
   ```
4. Відкрийте браузер і перейдіть до `http://localhost:8082`

## 🧪 Тестування

### Через браузер:
- `http://localhost:8082/` - головна сторінка
- `http://localhost:8082/info` - інформація про запит
- `http://localhost:8082/headers` - HTTP заголовки
- `http://localhost:8082/query?name=Іван&age=25` - query параметри
- `http://localhost:8082/form` - тестова форма

### Через curl:
```bash
# Головна сторінка
curl http://localhost:8082/

# Інформація про запит
curl http://localhost:8082/info

# Query параметри
curl "http://localhost:8082/query?name=Тест&city=Київ"

# POST запит
curl -X POST http://localhost:8082/submit \
  -d "name=Іван&email=ivan@test.com&message=Привіт" \
  -H "Content-Type: application/x-www-form-urlencoded"

# 404 помилка
curl http://localhost:8082/nonexistent
```

## 🔧 Як це працює

### HTTP Listener
```csharp
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8082/");
listener.Start();
```

### Роутинг
```csharp
string path = request.Url?.AbsolutePath ?? "/";
string htmlContent = path switch
{
    "/" => GenerateHomePage(request),
    "/info" => GenerateInfoPage(request),
    "/headers" => GenerateHeadersPage(request),
    // ...
    _ => GenerateNotFoundPage(request)
};
```

### Query String
```csharp
var queryParams = HttpUtility.ParseQueryString(queryString);
foreach (string key in queryParams.AllKeys)
{
    string value = queryParams[key] ?? "";
    // Обробка параметра
}
```

### POST дані
```csharp
using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
string formData = await reader.ReadToEndAsync();
var formParams = HttpUtility.ParseQueryString(formData);
```

## 📚 Що демонструє цей код

### ✅ Основи HTTP:
- Створення HTTP сервера
- Обробка GET та POST запитів
- Встановлення заголовків відповіді
- Відправка HTML контенту

### ✅ Роутинг:
- Простий роутинг на основі шляху
- Обробка різних маршрутів
- 404 для невідомих маршрутів

### ✅ Обробка даних:
- Розбір Query String параметрів
- Читання POST даних з форм
- Кодування/декодування URL

### ✅ HTML генерація:
- Динамічне створення HTML
- CSS стилі
- Форми та таблиці

### ✅ Обробка помилок:
- Try-catch блоки
- Логування в консоль
- Коректне завершення

## 🎓 Освітня цінність

Цей проєкт показує:
1. **Скільки коду** потрібно для базового веб-сервера
2. **Як працюють** HTTP запити та відповіді
3. **Що робить** ASP.NET Core "під капотом"
4. **Чому фреймворки** роблять розробку простішою

## 🔄 Порівняння з ASP.NET Core

Аналогічна функціональність в ASP.NET Core Minimal API:

```csharp
var app = WebApplication.Create();

app.MapGet("/", () => "Hello World!");
app.MapGet("/info", (HttpContext ctx) => ctx.Request.Headers);
app.MapPost("/submit", (IFormCollection form) => form);

app.Run();
```

**Різниця в кількості коду: ~590 рядків vs ~10 рядків!** 🤯

## 🏁 Висновок

Цей простий сервер демонструє основи веб-розробки та показує цінність сучасних фреймворків. Хоча можна створити HTTP сервер на чистому .NET, ASP.NET Core значно спрощує та прискорює розробку.
