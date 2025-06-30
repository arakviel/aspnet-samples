using System.Net;
using System.Text;
using System.Web;

namespace AspNet.NativeHttpServerSimple;

/// <summary>
/// Простий HTTP сервер на нативних бібліотеках .NET
/// Демонструє роботу з роутингом, query string, заголовками
/// </summary>
class Program
{
    // URL на якому буде працювати наш сервер
    private static readonly string ServerUrl = "http://localhost:8082/";
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Запускаємо простий HTTP сервер...");
        Console.WriteLine($"📍 Сервер буде доступний на: {ServerUrl}");
        Console.WriteLine("⏹️  Натисніть Ctrl+C для зупинки");
        Console.WriteLine();

        // Створюємо HTTP слухач (listener)
        var listener = new HttpListener();
        
        // Додаємо префікс URL для прослуховування
        listener.Prefixes.Add(ServerUrl);
        
        try
        {
            // Запускаємо слухач
            listener.Start();
            Console.WriteLine("✅ HTTP сервер запущено успішно!");
            
            // Обробляємо Ctrl+C для коректного завершення
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                listener.Stop();
                Console.WriteLine("\n🛑 Сервер зупинено.");
                Environment.Exit(0);
            };

            // Основний цикл обробки запитів
            while (listener.IsListening)
            {
                try
                {
                    // Чекаємо на новий HTTP запит
                    var context = await listener.GetContextAsync();
                    
                    // Обробляємо запит в окремому завданні (Task)
                    // щоб не блокувати основний цикл
                    _ = Task.Run(() => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Ігноруємо помилки коли сервер зупиняється
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка в основному циклі: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Помилка запуску сервера: {ex.Message}");
        }
        finally
        {
            listener?.Close();
        }
    }

    /// <summary>
    /// Обробляє HTTP запит та формує відповідь
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    static async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            Console.WriteLine($"📨 Новий запит: {request.HttpMethod} {request.Url?.PathAndQuery}");

            // Отримуємо шлях запиту (без query string)
            string path = request.Url?.AbsolutePath ?? "/";
            
            // Визначаємо який контент повернути на основі шляху
            string htmlContent = path switch
            {
                "/" => GenerateHomePage(request),
                "/info" => GenerateInfoPage(request),
                "/headers" => GenerateHeadersPage(request),
                "/query" => GenerateQueryPage(request),
                "/form" => GenerateFormPage(request),
                "/submit" => await HandleFormSubmit(request),
                _ => GenerateNotFoundPage(request)
            };

            // Встановлюємо заголовки відповіді
            response.ContentType = "text/html; charset=utf-8";
            response.StatusCode = path == "/" || path == "/info" || path == "/headers" || 
                                 path == "/query" || path == "/form" || path == "/submit" ? 200 : 404;

            // Конвертуємо HTML в байти
            byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);
            response.ContentLength64 = buffer.Length;

            // Відправляємо відповідь
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            Console.WriteLine($"✅ Відповідь відправлено: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Помилка обробки запиту: {ex.Message}");
            
            // Намагаємося відправити помилку 500
            try
            {
                response.StatusCode = 500;
                byte[] errorBuffer = Encoding.UTF8.GetBytes("<h1>500 - Внутрішня помилка сервера</h1>");
                response.ContentLength64 = errorBuffer.Length;
                await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                response.OutputStream.Close();
            }
            catch
            {
                // Якщо не можемо відправити помилку, просто ігноруємо
            }
        }
    }

    /// <summary>
    /// Генерує головну сторінку з навігацією
    /// </summary>
    static string GenerateHomePage(HttpListenerRequest request)
    {
        return $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Простий HTTP Сервер</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #2c3e50; text-align: center; }}
        .nav {{ display: flex; gap: 15px; margin: 20px 0; flex-wrap: wrap; }}
        .nav a {{ padding: 10px 20px; background: #3498db; color: white; text-decoration: none; border-radius: 5px; }}
        .nav a:hover {{ background: #2980b9; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .current-request {{ background: #f8f9fa; padding: 15px; border-radius: 5px; border-left: 4px solid #28a745; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🌐 Простий HTTP Сервер</h1>
        
        <div class='info'>
            <h3>Ласкаво просимо!</h3>
            <p>Це демонстрація простого HTTP сервера на нативних бібліотеках .NET.</p>
            <p>Сервер вміє обробляти різні маршрути, показувати інформацію про запити та працювати з формами.</p>
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
            <p><strong>Метод:</strong> {request.HttpMethod}</p>
            <p><strong>Шлях:</strong> {request.Url?.AbsolutePath}</p>
            <p><strong>Query String:</strong> {request.Url?.Query ?? "(відсутній)"}</p>
            <p><strong>User-Agent:</strong> {request.UserAgent ?? "(невідомий)"}</p>
            <p><strong>Час:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Генерує сторінку з детальною інформацією про запит
    /// </summary>
    static string GenerateInfoPage(HttpListenerRequest request)
    {
        var url = request.Url;
        
        return $@"
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
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        
        <h1>ℹ️ Детальна інформація про запит</h1>
        
        <table class='info-table'>
            <tr><th>Параметр</th><th>Значення</th></tr>
            <tr><td>HTTP Метод</td><td>{request.HttpMethod}</td></tr>
            <tr><td>Повний URL</td><td>{url}</td></tr>
            <tr><td>Схема (Protocol)</td><td>{url?.Scheme}</td></tr>
            <tr><td>Хост</td><td>{url?.Host}</td></tr>
            <tr><td>Порт</td><td>{url?.Port}</td></tr>
            <tr><td>Шлях (Path)</td><td>{url?.AbsolutePath}</td></tr>
            <tr><td>Query String</td><td>{url?.Query ?? "(відсутній)"}</td></tr>
            <tr><td>Фрагмент</td><td>{url?.Fragment ?? "(відсутній)"}</td></tr>
            <tr><td>User-Agent</td><td>{request.UserAgent ?? "(невідомий)"}</td></tr>
            <tr><td>Content Type</td><td>{request.ContentType ?? "(відсутній)"}</td></tr>
            <tr><td>Content Length</td><td>{request.ContentLength64}</td></tr>
            <tr><td>Має тіло запиту</td><td>{request.HasEntityBody}</td></tr>
            <tr><td>HTTP Версія</td><td>{request.ProtocolVersion}</td></tr>
            <tr><td>Локальний endpoint</td><td>{request.LocalEndPoint}</td></tr>
            <tr><td>Віддалений endpoint</td><td>{request.RemoteEndPoint}</td></tr>
            <tr><td>Час запиту</td><td>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</td></tr>
        </table>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Генерує сторінку з усіма HTTP заголовками
    /// </summary>
    static string GenerateHeadersPage(HttpListenerRequest request)
    {
        var headersHtml = new StringBuilder();
        
        // Проходимо по всіх заголовках запиту
        foreach (string headerName in request.Headers.AllKeys)
        {
            string headerValue = request.Headers[headerName] ?? "";
            headersHtml.AppendLine($"<tr><td>{headerName}</td><td>{HttpUtility.HtmlEncode(headerValue)}</td></tr>");
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
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        
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

    /// <summary>
    /// Генерує сторінку з розбором Query String параметрів
    /// </summary>
    static string GenerateQueryPage(HttpListenerRequest request)
    {
        var queryString = request.Url?.Query;
        var queryParamsHtml = new StringBuilder();

        if (!string.IsNullOrEmpty(queryString))
        {
            // Видаляємо знак питання на початку
            queryString = queryString.TrimStart('?');

            // Розбираємо параметри
            var queryParams = HttpUtility.ParseQueryString(queryString);

            foreach (string key in queryParams.AllKeys)
            {
                if (key != null)
                {
                    string value = queryParams[key] ?? "";
                    queryParamsHtml.AppendLine($"<tr><td>{HttpUtility.HtmlEncode(key)}</td><td>{HttpUtility.HtmlEncode(value)}</td></tr>");
                }
            }
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
        .examples a {{ display: block; margin: 5px 0; color: #3498db; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>

        <h1>🔍 Query String Параметри</h1>

        <div class='info'>
            <strong>Повний Query String:</strong> {request.Url?.Query ?? "(відсутній)"}
        </div>

        {(string.IsNullOrEmpty(queryString) ?
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

    /// <summary>
    /// Генерує сторінку з формою для тестування POST запитів
    /// </summary>
    static string GenerateFormPage(HttpListenerRequest request)
    {
        return $@"
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
        button {{ background: #3498db; color: white; padding: 12px 24px; border: none; border-radius: 4px; font-size: 16px; cursor: pointer; }}
        button:hover {{ background: #2980b9; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>

        <h1>📝 Тестова форма</h1>

        <div class='info'>
            <p>Ця форма демонструє обробку POST запитів та отримання даних з форми.</p>
            <p>Після відправки ви побачите всі дані, які були передані на сервер.</p>
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
    }

    /// <summary>
    /// Обробляє відправку форми (POST запит)
    /// </summary>
    static async Task<string> HandleFormSubmit(HttpListenerRequest request)
    {
        var formDataHtml = new StringBuilder();

        if (request.HttpMethod == "POST" && request.HasEntityBody)
        {
            // Читаємо дані з тіла запиту
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
            string formData = await reader.ReadToEndAsync();

            // Розбираємо дані форми
            var formParams = HttpUtility.ParseQueryString(formData);

            foreach (string key in formParams.AllKeys)
            {
                if (key != null)
                {
                    string value = formParams[key] ?? "";
                    formDataHtml.AppendLine($"<tr><td>{HttpUtility.HtmlEncode(key)}</td><td>{HttpUtility.HtmlEncode(value)}</td></tr>");
                }
            }
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
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>
        <a href='/form' class='back'>📝 Назад до форми</a>

        <h1>✅ Форма успішно відправлена!</h1>

        <div class='success'>
            <strong>🎉 Дані успішно отримані сервером!</strong><br>
            Нижче ви можете побачити всі дані, які були передані через форму.
        </div>

        <div class='info'>
            <strong>Метод запиту:</strong> {request.HttpMethod}<br>
            <strong>Content-Type:</strong> {request.ContentType ?? "(не вказано)"}<br>
            <strong>Content-Length:</strong> {request.ContentLength64} байт<br>
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

    /// <summary>
    /// Генерує сторінку 404 для невідомих маршрутів
    /// </summary>
    static string GenerateNotFoundPage(HttpListenerRequest request)
    {
        return $@"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <title>404 - Сторінка не знайдена</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #e74c3c; text-align: center; }}
        .back {{ display: inline-block; margin-bottom: 20px; padding: 8px 16px; background: #95a5a6; color: white; text-decoration: none; border-radius: 4px; }}
        .error {{ background: #f8d7da; color: #721c24; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #f5c6cb; }}
        .info {{ background: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .routes {{ background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .routes a {{ display: block; margin: 5px 0; color: #3498db; }}
    </style>
</head>
<body>
    <div class='container'>
        <a href='/' class='back'>← Назад на головну</a>

        <h1>🚫 404 - Сторінка не знайдена</h1>

        <div class='error'>
            <strong>Помилка:</strong> Запитувана сторінка не існує на цьому сервері.
        </div>

        <div class='info'>
            <strong>Запитуваний шлях:</strong> {request.Url?.AbsolutePath}<br>
            <strong>Повний URL:</strong> {request.Url}<br>
            <strong>Метод:</strong> {request.HttpMethod}<br>
            <strong>Час запиту:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}
        </div>

        <div class='routes'>
            <h3>📍 Доступні маршрути:</h3>
            <a href='/'>🏠 Головна сторінка</a>
            <a href='/info'>ℹ️ Інформація про запит</a>
            <a href='/headers'>📋 HTTP заголовки</a>
            <a href='/query'>🔍 Query String параметри</a>
            <a href='/form'>📝 Тестова форма</a>
        </div>
    </div>
</body>
</html>";
    }
}
