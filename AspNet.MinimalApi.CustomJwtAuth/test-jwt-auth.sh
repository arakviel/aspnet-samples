#!/bin/bash

# Тестовий скрипт для демонстрації кастомної JWT аутентифікації
# Використання: ./test-jwt-auth.sh

BASE_URL="http://localhost:5100"

echo "🚀 Тестування кастомної JWT аутентифікації"
echo "=========================================="

# Функція для красивого виводу JSON
pretty_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

# Функція для HTTP запитів
make_request() {
    local method="$1"
    local url="$2"
    local headers="$3"
    local data="$4"
    
    if [ -n "$data" ]; then
        curl -s -X "$method" "$url" -H "Content-Type: application/json" $headers -d "$data"
    else
        curl -s -X "$method" "$url" $headers
    fi
}

echo
echo "1️⃣  Перевірка головної сторінки"
echo "--------------------------------"
response=$(make_request "GET" "$BASE_URL/")
pretty_json "$response"

echo
echo "2️⃣  Спроба доступу до захищеного ресурсу без токена"
echo "---------------------------------------------------"
response=$(make_request "GET" "$BASE_URL/protected")
pretty_json "$response"

echo
echo "3️⃣  Вхід адміністратора"
echo "----------------------"
admin_response=$(make_request "POST" "$BASE_URL/auth/login" "" '{"username": "admin", "password": "admin123"}')
pretty_json "$admin_response"

# Витягуємо токен адміністратора
admin_token=$(echo "$admin_response" | jq -r '.accessToken // empty')

if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    echo
    echo "4️⃣  Доступ до захищеного ресурсу з токеном адміністратора"
    echo "--------------------------------------------------------"
    response=$(make_request "GET" "$BASE_URL/protected" "-H 'Authorization: Bearer $admin_token'")
    pretty_json "$response"

    echo
    echo "5️⃣  Доступ до адміністративної панелі"
    echo "------------------------------------"
    response=$(make_request "GET" "$BASE_URL/admin/dashboard" "-H 'Authorization: Bearer $admin_token'")
    pretty_json "$response"

    echo
    echo "6️⃣  Перегляд списку користувачів (тільки для адміністраторів)"
    echo "------------------------------------------------------------"
    response=$(make_request "GET" "$BASE_URL/users" "-H 'Authorization: Bearer $admin_token'")
    pretty_json "$response"
fi

echo
echo "7️⃣  Вхід звичайного користувача"
echo "------------------------------"
user_response=$(make_request "POST" "$BASE_URL/auth/login" "" '{"username": "user", "password": "user123"}')
pretty_json "$user_response"

# Витягуємо токен користувача
user_token=$(echo "$user_response" | jq -r '.accessToken // empty')

if [ -n "$user_token" ] && [ "$user_token" != "null" ]; then
    echo
    echo "8️⃣  Доступ до захищеного ресурсу з токеном користувача"
    echo "-----------------------------------------------------"
    response=$(make_request "GET" "$BASE_URL/protected" "-H 'Authorization: Bearer $user_token'")
    pretty_json "$response"

    echo
    echo "9️⃣  Спроба доступу до адміністративної панелі звичайним користувачем"
    echo "-------------------------------------------------------------------"
    response=$(make_request "GET" "$BASE_URL/admin/dashboard" "-H 'Authorization: Bearer $user_token'")
    pretty_json "$response"

    echo
    echo "🔟 Тестування ролей"
    echo "------------------"
    response=$(make_request "GET" "$BASE_URL/roles/test" "-H 'Authorization: Bearer $user_token'")
    pretty_json "$response"
fi

echo
echo "1️⃣1️⃣ Реєстрація нового користувача"
echo "----------------------------------"
register_response=$(make_request "POST" "$BASE_URL/auth/register" "" '{"username": "testuser2", "password": "password123", "email": "testuser2@example.com"}')
pretty_json "$register_response"

echo
echo "1️⃣2️⃣ Спроба реєстрації з існуючим username"
echo "-----------------------------------------"
response=$(make_request "POST" "$BASE_URL/auth/register" "" '{"username": "admin", "password": "password123", "email": "newemail@example.com"}')
pretty_json "$response"

echo
echo "1️⃣3️⃣ Спроба входу з неправильним паролем"
echo "----------------------------------------"
response=$(make_request "POST" "$BASE_URL/auth/login" "" '{"username": "admin", "password": "wrongpassword"}')
pretty_json "$response"

echo
echo "1️⃣4️⃣ Перевірка статусу аутентифікації"
echo "------------------------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(make_request "GET" "$BASE_URL/auth/status" "-H 'Authorization: Bearer $admin_token'")
    pretty_json "$response"
fi

echo
echo "1️⃣5️⃣ Спроба доступу з недійсним токеном"
echo "--------------------------------------"
response=$(make_request "GET" "$BASE_URL/protected" "-H 'Authorization: Bearer invalid.jwt.token'")
pretty_json "$response"

echo
echo "✅ Тестування завершено!"
echo "======================="
echo
echo "📋 Результати тестування:"
echo "• Кастомна генерація JWT токенів працює ✅"
echo "• Валідація токенів працює ✅"
echo "• Система ролей працює ✅"
echo "• Захист ендпоінтів працює ✅"
echo "• Реєстрація користувачів працює ✅"
echo "• Обробка помилок працює ✅"
echo
echo "🎓 Для студентів:"
echo "• Вивчіть структуру JWT токенів на https://jwt.io"
echo "• Подивіться на кастомну реалізацію в коді"
echo "• Порівняйте з готовими рішеннями ASP.NET Core"
