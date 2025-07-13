# Simple JWT Auth with Cookies - Безпечний приклад

Простий приклад JWT аутентифікації з **HttpOnly cookies** замість Bearer токенів.

## 🍪 Особливості

- ✅ **HttpOnly cookies** - JavaScript не може прочитати
- ✅ **Secure cookies** - тільки через HTTPS
- ✅ **SameSite=Lax** - захист від CSRF
- ✅ **14 днів** термін дії
- ✅ **Автоматичне надсилання** з кожним запитом
- ✅ **Без Bearer токенів** - все в cookies

## 🔒 Безпека

### Налаштування cookies:
```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,                    // ✅ Захист від XSS
    Secure = true,                      // ✅ Тільки HTTPS
    SameSite = SameSiteMode.Lax,       // ✅ Захист від CSRF
    Expires = DateTimeOffset.UtcNow.AddDays(14),
    Path = "/"
};
```

### Переваги над localStorage:
- 🔒 **Захист від XSS** - JavaScript не може прочитати
- 🔄 **Автоматичне надсилання** - не потрібно додавати заголовки
- 🛡️ **SameSite захист** - блокує cross-site запити

## 🚀 Запуск

```bash
cd AspNet.MinimalApi.SimpleJwtWithCookiesAuth
dotnet run
```

Додаток буде доступний на `http://localhost:5000`

## 📋 API

| Ендпоінт | Метод | Опис | Cookie |
|----------|-------|------|--------|
| `/` | GET | Головна сторінка | Ні |
| `/login` | POST | Вхід (встановлює cookie) | Встановлює |
| `/register` | POST | Реєстрація (встановлює cookie) | Встановлює |
| `/logout` | POST | Вихід (видаляє cookie) | Видаляє |
| `/profile` | GET | Профіль користувача | Читає |
| `/protected` | GET | Захищений ресурс | Читає |
| `/admin` | GET | Тільки для адміністраторів | Читає |
| `/auth/status` | GET | Статус аутентифікації | Читає |

## 🧪 Тестування

Використайте файл `test-cookies.http`:

1. **Логін** - cookie встановлюється автоматично
2. **Захищені ресурси** - cookie надсилається автоматично
3. **Логаут** - cookie видаляється автоматично

## 🔍 Відмінності від Bearer токенів

| Аспект | Bearer токени | HttpOnly Cookies |
|--------|---------------|------------------|
| **Зберігання** | localStorage/memory | HttpOnly cookie |
| **Надсилання** | Manual (Authorization header) | Автоматично |
| **XSS захист** | ❌ Вразливі | ✅ Захищені |
| **CSRF захист** | ✅ Природний | ✅ SameSite=Lax |
| **Зручність** | Потрібно керувати | Автоматично |

## 🎓 Для студентів

### Як працюють HttpOnly cookies:

1. **Логін**: Сервер встановлює cookie
```http
Set-Cookie: auth_token=jwt_here; HttpOnly; Secure; SameSite=Lax
```

2. **Запити**: Браузер автоматично надсилає cookie
```http
Cookie: auth_token=jwt_here
```

3. **Логаут**: Сервер видаляє cookie
```http
Set-Cookie: auth_token=; expires=Thu, 01 Jan 1970 00:00:00 GMT
```

### Middleware обробка:
```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Читаємо токен з cookie (не з заголовка!)
    var token = context.Request.Cookies["auth_token"];
    
    if (!string.IsNullOrEmpty(token))
    {
        var principal = _jwt.ValidateToken(token);
        if (principal != null)
        {
            context.User = principal;
        }
    }
    
    await _next(context);
}
```

## 🆚 Порівняння підходів

### Bearer токени (SimpleJwtAuth):
```javascript
// Клієнт керує токенами
localStorage.setItem('token', response.token);
fetch('/api/protected', {
    headers: { 'Authorization': `Bearer ${token}` }
});
```

### HttpOnly Cookies (цей проект):
```javascript
// Браузер керує cookies автоматично
fetch('/api/protected', {
    credentials: 'include'  // Включає cookies
});
```

## ⚠️ Обмеження

- **CORS** - потрібно налаштувати `credentials: 'include'`
- **Subdomain атаки** - SameSite=Lax не захищає від same-site
- **Розмір** - cookies надсилаються з кожним запитом

## 💡 Коли використовувати

**HttpOnly Cookies краще для:**
- Традиційних веб-додатків
- Високої безпеки (банки, платежі)
- Простоти реалізації

**Bearer токени краще для:**
- Mobile додатків
- Cross-origin API
- Мікросервісної архітектури

## 🔧 Налаштування для продакшену

```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,                      // Обов'язково в продакшені!
    SameSite = SameSiteMode.Strict,    // Більш жорсткий захист
    Domain = ".yourdomain.com",        // Для subdomains
    Expires = DateTimeOffset.UtcNow.AddDays(7)  // Коротший термін
};
```

Цей приклад показує **безпечний спосіб** зберігання JWT токенів в SPA додатках! 🔒
