using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8083");
var app = builder.Build();

// Логування запитів
app.Use(async (context, next) =>
{
    Console.WriteLine($"📨 Новий запит: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    await next();
    Console.WriteLine($"✅ Відповідь відправлено: {context.Response.StatusCode}");
});

Console.WriteLine("🚀 Запускаємо ASP.NET Core Minimal API сервер...");
Console.WriteLine("📍 Сервер буде доступний на: http://localhost:8083/");
Console.WriteLine("⏹️  Натисніть Ctrl+C для зупинки");
Console.WriteLine();

// Маршрути
app.MapGet("/", (HttpContext context) => Results.Content(GenerateHomePage(context.Request), "text/html; charset=utf-8"));
app.MapGet("/info", (HttpContext context) => Results.Content(GenerateInfoPage(context.Request), "text/html; charset=utf-8"));
app.MapGet("/headers", (HttpContext context) => Results.Content(GenerateHeadersPage(context.Request), "text/html; charset=utf-8"));
app.MapGet("/query", (HttpContext context) => Results.Content(GenerateQueryPage(context.Request), "text/html; charset=utf-8"));
app.MapGet("/form", () => Results.Content(GenerateFormPage(), "text/html; charset=utf-8"));
app.MapPost("/submit", async (HttpContext context, [FromForm] IFormCollection form) =>
    Results.Content(await HandleFormSubmit(context, form), "text/html; charset=utf-8"))
    .DisableAntiforgery();

app.Run();

static string GenerateHomePage(HttpRequest request) => $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>ASP.NET Core Minimal API</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; text-align: center; }}
        .nav {{ display: flex; gap: 15px; margin: 20px 0; flex-wrap: wrap; }}
        .nav a {{ padding: 10px 20px; background: #e74c3c; color: white; text-decoration: none; border-radius: 5px; }}
        .nav a:hover {{ background: #c0392b; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .current-request {{ background: #f8f9fa; padding: 15px; border-radius: 5px; border-left: 4px solid #e74c3c; }}
        .comparison {{ background: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 ASP.NET Core Minimal API Сервер</h1>
        
        <div class='comparison'>
            <h3>🎯 Порівняння з нативним HTTP сервером:</h3>
            <p><strong>Нативний сервер:</strong> ~590 рядків коду</p>
            <p><strong>ASP.NET Core Minimal API:</strong> ~150 рядків коду</p>
            <p><strong>Економія:</strong> 75% менше коду! 🎉</p>
        </div>

        <div class='info'>
            <h3>Ласкаво просимо!</h3>
            <p>Це демонстрація того ж HTTP сервера, але створеного з ASP.NET Core Minimal API.</p>
            <p>Порівняйте простоту та елегантність коду з нативною реалізацією!</p>
        </div>

        <div class='nav'>
            <a href='/'>🏠 Головна</a>
            <a href='/info'>ℹ️ Інфо про запит</a>
            <a href='/headers'>📋 Заголовки</a>
            <a href='/query?name=Іван&age=25&city=Київ'>🔍 Query String</a>
            <a href='/form'>📝 Форма</a>
        </div>

        <div class='current-request'>
            <h3>📨 Поточний запит:</h3>
            <p><strong>Метод:</strong> {request.Method}</p>
            <p><strong>Шлях:</strong> {request.Path}</p>
            <p><strong>Query String:</strong> {request.QueryString}</p>
            <p><strong>User-Agent:</strong> {request.Headers.UserAgent.FirstOrDefault() ?? "(невідомий)"}</p>
            <p><strong>Час:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        </div>
    </div>
</body>
</html>";

static string GenerateInfoPage(HttpRequest request) => $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>Інформація про запит</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; }}
        .back {{ display: inline-block; margin-bottom: 20px; padding: 8px 16px; background: #95a5a6; color: white; text-decoration: none; border-radius: 4px; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .info-table th, .info-table td {{ padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        .info-table th {{ background-color: #f8f9fa; font-weight: bold; }}
        .info-table tr:hover {{ background-color: #f5f5f5; }}
        .aspnet-badge {{ background: #e74c3c; color: white; padding: 5px 10px; border-radius: 3px; font-size: 0.8em; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        <span class='aspnet-badge'>ASP.NET Core</span>
        
        <h1>ℹ️ Детальна інформація про запит</h1>
        
        <table class='info-table'>
            <tr><th>Параметр</th><th>Значення</th></tr>
            <tr><td>HTTP Метод</td><td>{request.Method}</td></tr>
            <tr><td>Повний URL</td><td>{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}</td></tr>
            <tr><td>Схема (Protocol)</td><td>{request.Scheme}</td></tr>
            <tr><td>Хост</td><td>{request.Host}</td></tr>
            <tr><td>Шлях (Path)</td><td>{request.Path}</td></tr>
            <tr><td>Query String</td><td>{request.QueryString}</td></tr>
            <tr><td>User-Agent</td><td>{request.Headers.UserAgent.FirstOrDefault() ?? "(невідомий)"}</td></tr>
            <tr><td>Content Type</td><td>{request.ContentType ?? "(відсутній)"}</td></tr>
            <tr><td>Content Length</td><td>{request.ContentLength?.ToString() ?? "(невідомий)"}</td></tr>
            <tr><td>Має тіло запиту</td><td>{(request.ContentLength > 0 ? "Так" : "Ні")}</td></tr>
            <tr><td>HTTP Версія</td><td>{request.Protocol}</td></tr>
            <tr><td>Віддалений IP</td><td>{request.HttpContext.Connection.RemoteIpAddress}</td></tr>
            <tr><td>Час запиту</td><td>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</td></tr>
        </table>
    </div>
</body>
</html>";

static string GenerateHeadersPage(HttpRequest request)
{
    var headersHtml = new StringBuilder();
    foreach (var header in request.Headers)
    {
        var values = string.Join(", ", header.Value.ToArray());
        headersHtml.AppendLine($"<tr><td>{header.Key}</td><td>{System.Web.HttpUtility.HtmlEncode(values)}</td></tr>");
    }

    return $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>HTTP Заголовки</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; }}
        .back {{ display: inline-block; margin-bottom: 20px; padding: 8px 16px; background: #95a5a6; color: white; text-decoration: none; border-radius: 4px; }}
        .headers-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .headers-table th, .headers-table td {{ padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        .headers-table th {{ background-color: #f8f9fa; font-weight: bold; }}
        .headers-table tr:hover {{ background-color: #f5f5f5; }}
        .count {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .aspnet-badge {{ background: #e74c3c; color: white; padding: 5px 10px; border-radius: 3px; font-size: 0.8em; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        <span class='aspnet-badge'>ASP.NET Core</span>
        
        <h1>📋 HTTP Заголовки запиту</h1>
        
        <div class='count'>
            <strong>Загальна кількість заголовків:</strong> {request.Headers.Count}
        </div>
        
        <table class='headers-table'>
            <thead>
                <tr><th>Назва заголовка</th><th>Значення</th></tr>
            </thead>
            <tbody>
                {headersHtml}
            </tbody>
        </table>
    </div>
</body>
</html>";
}

static string GenerateQueryPage(HttpRequest request)
{
    var queryParamsHtml = new StringBuilder();
    foreach (var param in request.Query)
    {
        var values = string.Join(", ", param.Value.ToArray());
        queryParamsHtml.AppendLine($"<tr><td>{System.Web.HttpUtility.HtmlEncode(param.Key)}</td><td>{System.Web.HttpUtility.HtmlEncode(values)}</td></tr>");
    }

    return $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>Query String Параметри</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; }}
        .back {{ display: inline-block; margin-bottom: 20px; padding: 8px 16px; background: #95a5a6; color: white; text-decoration: none; border-radius: 4px; }}
        .query-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .query-table th, .query-table td {{ padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        .query-table th {{ background-color: #f8f9fa; font-weight: bold; }}
        .query-table tr:hover {{ background-color: #f5f5f5; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .examples {{ background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .examples a {{ display: block; margin: 5px 0; color: #e74c3c; }}
        .aspnet-badge {{ background: #e74c3c; color: white; padding: 5px 10px; border-radius: 3px; font-size: 0.8em; }}
        .advantage {{ background: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        <span class='aspnet-badge'>ASP.NET Core</span>

        <h1>🔍 Query String Параметри</h1>

        <div class='advantage'>
            <strong>💡 Переваги ASP.NET Core:</strong> Автоматичний розбір query параметрів через <code>request.Query</code>
        </div>

        <div class='info'>
            <strong>Повний Query String:</strong> {request.QueryString}
        </div>

        {(request.Query.Count == 0 ?
            "<div class='info'>❌ Query String параметри відсутні</div>" :
            $@"<table class='query-table'>
                <thead>
                    <tr><th>Параметр</th><th>Значення</th></tr>
                </thead>
                <tbody>
                    {queryParamsHtml}
                </tbody>
            </table>")}

        <div class='examples'>
            <h3>💡 Приклади для тестування:</h3>
            <a href='/query?name=Олександр&age=30'>?name=Олександр&age=30</a>
            <a href='/query?city=Львів&country=Україна&population=721301'>?city=Львів&country=Україна&population=721301</a>
            <a href='/query?search=програмування&category=технології&sort=date'>?search=програмування&category=технології&sort=date</a>
            <a href='/query?empty=&test=значення&спеціальні=символи%20%26%20кодування'>?empty=&test=значення&спеціальні=символи%20%26%20кодування</a>
        </div>
    </div>
</body>
</html>";
}

static string GenerateFormPage() => $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>Тестова форма</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; }}
        .back {{ display: inline-block; margin-bottom: 20px; padding: 8px 16px; background: #95a5a6; color: white; text-decoration: none; border-radius: 4px; }}
        .form-group {{ margin: 20px 0; }}
        label {{ display: block; margin-bottom: 5px; font-weight: bold; }}
        input, textarea, select {{ width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; font-size: 16px; }}
        button {{ background: #e74c3c; color: white; padding: 12px 24px; border: none; border-radius: 4px; font-size: 16px; cursor: pointer; }}
        button:hover {{ background: #c0392b; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .aspnet-badge {{ background: #e74c3c; color: white; padding: 5px 10px; border-radius: 3px; font-size: 0.8em; }}
        .advantage {{ background: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        <span class='aspnet-badge'>ASP.NET Core</span>

        <h1>📝 Тестова форма</h1>

        <div class='advantage'>
            <strong>💡 Переваги ASP.NET Core:</strong> Автоматичний model binding через <code>[FromForm] IFormCollection</code>
        </div>

        <div class='info'>
            <p>Ця форма демонструє обробку POST запитів в ASP.NET Core Minimal API.</p>
            <p>Зверніть увагу на простоту обробки даних порівняно з нативним сервером!</p>
        </div>

        <form action='/submit' method='POST'>
            <div class='form-group'>
                <label for='name'>Ім'я:</label>
                <input type='text' id='name' name='name' placeholder='Введіть ваше ім'я' required>
            </div>
            <div class='form-group'>
                <label for='email'>Email:</label>
                <input type='email' id='email' name='email' placeholder='example@email.com' required>
            </div>
            <div class='form-group'>
                <label for='age'>Вік:</label>
                <input type='number' id='age' name='age' min='1' max='120' placeholder='25'>
            </div>
            <div class='form-group'>
                <label for='city'>Місто:</label>
                <select id='city' name='city'>
                    <option value=''>Оберіть місто</option>
                    <option value='Київ'>Київ</option>
                    <option value='Львів'>Львів</option>
                    <option value='Одеса'>Одеса</option>
                    <option value='Харків'>Харків</option>
                    <option value='Дніпро'>Дніпро</option>
                </select>
            </div>
            <div class='form-group'>
                <label for='message'>Повідомлення:</label>
                <textarea id='message' name='message' rows='4' placeholder='Напишіть ваше повідомлення тут...'></textarea>
            </div>
            <div class='form-group'>
                <button type='submit'>📤 Відправити форму</button>
            </div>
        </form>
    </div>
</body>
</html>";

static async Task<string> HandleFormSubmit(HttpContext context, IFormCollection form)
{
    var formDataHtml = new StringBuilder();
    foreach (var field in form)
    {
        var values = string.Join(", ", field.Value.ToArray());
        formDataHtml.AppendLine($"<tr><td>{System.Web.HttpUtility.HtmlEncode(field.Key)}</td><td>{System.Web.HttpUtility.HtmlEncode(values)}</td></tr>");
    }

    return $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>Результат відправки форми</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; }}
        .back {{ display: inline-block; margin-bottom: 20px; padding: 8px 16px; background: #95a5a6; color: white; text-decoration: none; border-radius: 4px; }}
        .success {{ background: #d4edda; color: #155724; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #c3e6cb; }}
        .form-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .form-table th, .form-table td {{ padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        .form-table th {{ background-color: #f8f9fa; font-weight: bold; }}
        .form-table tr:hover {{ background-color: #f5f5f5; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .aspnet-badge {{ background: #e74c3c; color: white; padding: 5px 10px; border-radius: 3px; font-size: 0.8em; }}
        .advantage {{ background: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        <a href='/form' class='back'>📝 Назад до форми</a>
        <span class='aspnet-badge'>ASP.NET Core</span>

        <h1>✅ Форма успішно відправлена!</h1>

        <div class='advantage'>
            <strong>💡 Переваги ASP.NET Core:</strong> Дані автоматично розібрані в <code>IFormCollection</code> - жодного ручного парсингу!
        </div>

        <div class='success'>
            <strong>🎉 Дані успішно отримані сервером!</strong><br>
            Нижче ви можете побачити всі дані, які були передані через форму.
        </div>

        <div class='info'>
            <strong>Метод запиту:</strong> {context.Request.Method}<br>
            <strong>Content-Type:</strong> {context.Request.ContentType ?? "(не вказано)"}<br>
            <strong>Content-Length:</strong> {context.Request.ContentLength} байт<br>
            <strong>Час обробки:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}
        </div>

        {(formDataHtml.Length > 0 ?
            $@"<table class='form-table'>
                <thead>
                    <tr><th>Поле форми</th><th>Значення</th></tr>
                </thead>
                <tbody>
                    {formDataHtml}
                </tbody>
            </table>" :
            "<div class='info'>❌ Дані форми не знайдено</div>")}
    </div>
</body>
</html>";
}
