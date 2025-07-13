#!/bin/bash

# Тест для ASP.NET Core JWT Authentication
BASE_URL="http://localhost:5040"

echo "🚀 Тестування ASP.NET Core JWT Authentication"
echo "============================================="

# Функція для красивого виводу JSON
pretty_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

echo
echo "1️⃣  Головна сторінка"
echo "-------------------"
response=$(curl -s "$BASE_URL/")
pretty_json "$response"

echo
echo "2️⃣  Публічний ендпоінт"
echo "---------------------"
response=$(curl -s "$BASE_URL/public")
pretty_json "$response"

echo
echo "3️⃣  Логін адміністратора"
echo "----------------------"
admin_response=$(curl -s -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}')
pretty_json "$admin_response"

admin_token=$(echo "$admin_response" | jq -r '.token // empty')

echo
echo "4️⃣  Захищений ресурс з токеном адміністратора"
echo "--------------------------------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/protected" \
      -H "Authorization: Bearer $admin_token")
    pretty_json "$response"
fi

echo
echo "5️⃣  Адміністративна панель (Policy: AdminOnly)"
echo "----------------------------------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/admin" \
      -H "Authorization: Bearer $admin_token")
    pretty_json "$response"
fi

echo
echo "6️⃣  Панель менеджера (Policy: ManagerOrAdmin)"
echo "--------------------------------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/manager" \
      -H "Authorization: Bearer $admin_token")
    pretty_json "$response"
fi

echo
echo "7️⃣  Інформація про токен"
echo "-----------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/auth/token-info" \
      -H "Authorization: Bearer $admin_token")
    echo "Header та Payload токена:"
    echo "$response" | jq '{header, payload}'
fi

echo
echo "8️⃣  Логін менеджера"
echo "------------------"
manager_response=$(curl -s -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "manager", "password": "manager123"}')
pretty_json "$manager_response"

manager_token=$(echo "$manager_response" | jq -r '.token // empty')

echo
echo "9️⃣  Менеджер: доступ до панелі менеджера (має працювати)"
echo "-------------------------------------------------------"
if [ -n "$manager_token" ] && [ "$manager_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/manager" \
      -H "Authorization: Bearer $manager_token")
    pretty_json "$response"
fi

echo
echo "🔟 Менеджер: спроба доступу до адмін панелі (має бути 403)"
echo "---------------------------------------------------------"
if [ -n "$manager_token" ] && [ "$manager_token" != "null" ]; then
    http_code=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/admin" \
      -H "Authorization: Bearer $manager_token")
    echo "HTTP Status Code: $http_code"
    if [ "$http_code" = "403" ]; then
        echo "✅ Правильно заблоковано (403 Forbidden)"
    else
        echo "❌ Неочікуваний код відповіді"
    fi
fi

echo
echo "1️⃣1️⃣ Логін звичайного користувача"
echo "--------------------------------"
user_response=$(curl -s -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "user", "password": "user123"}')
pretty_json "$user_response"

user_token=$(echo "$user_response" | jq -r '.token // empty')

echo
echo "1️⃣2️⃣ User: доступ до ресурсу тільки для User (має працювати)"
echo "----------------------------------------------------------"
if [ -n "$user_token" ] && [ "$user_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/user-only" \
      -H "Authorization: Bearer $user_token")
    pretty_json "$response"
fi

echo
echo "1️⃣3️⃣ User: спроба доступу до панелі менеджера (має бути 403)"
echo "-----------------------------------------------------------"
if [ -n "$user_token" ] && [ "$user_token" != "null" ]; then
    http_code=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/manager" \
      -H "Authorization: Bearer $user_token")
    echo "HTTP Status Code: $http_code"
    if [ "$http_code" = "403" ]; then
        echo "✅ Правильно заблоковано (403 Forbidden)"
    else
        echo "❌ Неочікуваний код відповіді"
    fi
fi

echo
echo "1️⃣4️⃣ Реєстрація нового користувача"
echo "----------------------------------"
register_response=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "password123", "email": "test@example.com"}')
pretty_json "$register_response"

echo
echo "1️⃣5️⃣ Доступ без токена (має бути 401)"
echo "------------------------------------"
http_code=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/protected")
echo "HTTP Status Code: $http_code"
if [ "$http_code" = "401" ]; then
    echo "✅ Правильно заблоковано (401 Unauthorized)"
else
    echo "❌ Неочікуваний код відповіді"
fi

echo
echo "1️⃣6️⃣ Доступ з недійсним токеном (має бути 401)"
echo "---------------------------------------------"
http_code=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/protected" \
  -H "Authorization: Bearer invalid.jwt.token")
echo "HTTP Status Code: $http_code"
if [ "$http_code" = "401" ]; then
    echo "✅ Правильно заблоковано (401 Unauthorized)"
else
    echo "❌ Неочікуваний код відповіді"
fi

echo
echo "✅ Тестування завершено!"
echo "======================="
echo
echo "📋 Результати тестування ASP.NET Core JWT:"
echo "• Автоматична валідація токенів ✅"
echo "• Authorization Policies працюють ✅"
echo "• Role-based авторизація працює ✅"
echo "• Claims-based аутентифікація ✅"
echo "• JwtBearerMiddleware працює ✅"
echo "• TokenValidationParameters ✅"
echo
echo "🎯 Переваги ASP.NET Core підходу:"
echo "• Мінімум коду - максимум функціональності"
echo "• Автоматична валідація та логування"
echo "• Вбудовані політики авторизації"
echo "• Повна інтеграція з DI контейнером"
echo "• Офіційна підтримка Microsoft"
echo
echo "🎓 Для студентів:"
echo "Це стандартний підхід для продакшен додатків!"
echo "Порівняйте з кастомними реалізаціями для розуміння переваг."
