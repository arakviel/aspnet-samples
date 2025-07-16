# 💡 Приклади використання ASP.NET Core Identity

## 🔐 Приклади аутентифікації

### 1. Реєстрація користувача

**Запит:**
```http
POST /register
Content-Type: application/json

{
  "email": "student@university.com",
  "password": "Student123!"
}
```

**Відповідь (успіх):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.3.1",
  "title": "OK",
  "status": 200
}
```

**Відповідь (помилка):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "DuplicateEmail": [
      "Email 'student@university.com' is already taken."
    ]
  }
}
```

### 2. Логін користувача

**Запит:**
```http
POST /login
Content-Type: application/json

{
  "email": "student@university.com",
  "password": "Student123!"
}
```

**Відповідь (успіх):**
```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "CfDJ8OZNzKbKjqwrJ8..."
}
```

### 3. Використання токена

**Запит:**
```http
GET /auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Відповідь:**
```json
{
  "id": "12345678-1234-1234-1234-123456789012",
  "userName": "student@university.com",
  "email": "student@university.com",
  "firstName": null,
  "lastName": null,
  "fullName": "",
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLoginAt": null,
  "emailConfirmed": false,
  "phoneNumber": null,
  "phoneNumberConfirmed": false
}
```

## 🛠️ Приклади роботи з профілем

### 1. Оновлення профілю

**Запит:**
```http
PUT /auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "firstName": "Олександр",
  "lastName": "Петренко",
  "phoneNumber": "+380123456789"
}
```

**Відповідь:**
```json
{
  "message": "Профіль успішно оновлено"
}
```

### 2. Отримання списку користувачів

**Запит:**
```http
GET /auth/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Відповідь:**
```json
[
  {
    "id": "12345678-1234-1234-1234-123456789012",
    "userName": "student@university.com",
    "email": "student@university.com",
    "firstName": "Олександр",
    "lastName": "Петренко",
    "fullName": "Олександр Петренко",
    "createdAt": "2024-01-15T10:30:00Z",
    "lastLoginAt": "2024-01-15T11:00:00Z",
    "emailConfirmed": false
  },
  {
    "id": "87654321-4321-4321-4321-210987654321",
    "userName": "teacher@university.com",
    "email": "teacher@university.com",
    "firstName": "Марія",
    "lastName": "Іваненко",
    "fullName": "Марія Іваненко",
    "createdAt": "2024-01-14T09:15:00Z",
    "lastLoginAt": "2024-01-15T08:30:00Z",
    "emailConfirmed": true
  }
]
```

## 🔄 Приклади оновлення токенів

### 1. Оновлення access токена

**Запит:**
```http
POST /refresh
Content-Type: application/json

{
  "refreshToken": "CfDJ8OZNzKbKjqwrJ8..."
}
```

**Відповідь:**
```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "refreshToken": "CfDJ8NewRefreshToken..."
}
```

## 🚫 Приклади помилок

### 1. Неавторизований доступ

**Запит:**
```http
GET /auth/profile
```

**Відповідь:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 2. Невірний пароль

**Запит:**
```http
POST /login
Content-Type: application/json

{
  "email": "student@university.com",
  "password": "WrongPassword"
}
```

**Відповідь:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Failed"
}
```

### 3. Слабкий пароль

**Запит:**
```http
POST /register
Content-Type: application/json

{
  "email": "newuser@university.com",
  "password": "123"
}
```

**Відповідь:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PasswordTooShort": [
      "Passwords must be at least 6 characters."
    ],
    "PasswordRequiresUpper": [
      "Passwords must have at least one uppercase ('A'-'Z')."
    ],
    "PasswordRequiresLower": [
      "Passwords must have at least one lowercase ('a'-'z')."
    ]
  }
}
```

## 📱 Приклади для різних клієнтів

### JavaScript (Fetch API)

```javascript
// Реєстрація
async function register(email, password) {
  const response = await fetch('/register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email, password })
  });
  
  return response.ok;
}

// Логін
async function login(email, password) {
  const response = await fetch('/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email, password })
  });
  
  if (response.ok) {
    const data = await response.json();
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  }
  
  throw new Error('Login failed');
}

// Отримання профілю
async function getProfile() {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch('/auth/profile', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  if (response.ok) {
    return response.json();
  }
  
  throw new Error('Failed to get profile');
}
```

### C# (HttpClient)

```csharp
public class AuthService
{
    private readonly HttpClient _httpClient;
    
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<bool> RegisterAsync(string email, string password)
    {
        var request = new { email, password };
        var response = await _httpClient.PostAsJsonAsync("/register", request);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<TokenResponse> LoginAsync(string email, string password)
    {
        var request = new { email, password };
        var response = await _httpClient.PostAsJsonAsync("/login", request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }
        
        throw new Exception("Login failed");
    }
    
    public async Task<UserProfile> GetProfileAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
        var response = await _httpClient.GetAsync("/auth/profile");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserProfile>();
        }
        
        throw new Exception("Failed to get profile");
    }
}
```

## 🧪 Тестування з curl

```bash
# Реєстрація
curl -X POST https://localhost:7000/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# Логін
curl -X POST https://localhost:7000/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}' \
  | jq '.accessToken' | tr -d '"' > token.txt

# Отримання профілю
curl -X GET https://localhost:7000/auth/profile \
  -H "Authorization: Bearer $(cat token.txt)"

# Оновлення профілю
curl -X PUT https://localhost:7000/auth/profile \
  -H "Authorization: Bearer $(cat token.txt)" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Тест","lastName":"Користувач","phoneNumber":"+380123456789"}'
```

Ці приклади показують, як використовувати API в реальних сценаріях!
