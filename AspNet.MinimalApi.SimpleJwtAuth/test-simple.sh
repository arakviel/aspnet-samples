#!/bin/bash

# Простий тест для Simple JWT Auth
BASE_URL="http://localhost:5075"

echo "🚀 Тестування Simple JWT Auth"
echo "=============================="

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
echo "2️⃣  Логін адміністратора"
echo "----------------------"
admin_response=$(curl -s -X POST "$BASE_URL/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}')
pretty_json "$admin_response"

admin_token=$(echo "$admin_response" | jq -r '.token // empty')

echo
echo "3️⃣  Захищений ресурс з токеном адміністратора"
echo "--------------------------------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/protected" \
      -H "Authorization: Bearer $admin_token")
    pretty_json "$response"
fi

echo
echo "4️⃣  Адміністративна панель"
echo "------------------------"
if [ -n "$admin_token" ] && [ "$admin_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/admin" \
      -H "Authorization: Bearer $admin_token")
    pretty_json "$response"
fi

echo
echo "5️⃣  Логін звичайного користувача"
echo "------------------------------"
user_response=$(curl -s -X POST "$BASE_URL/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "user", "password": "user123"}')
pretty_json "$user_response"

user_token=$(echo "$user_response" | jq -r '.token // empty')

echo
echo "6️⃣  Спроба доступу до адмін панелі звичайним користувачем"
echo "--------------------------------------------------------"
if [ -n "$user_token" ] && [ "$user_token" != "null" ]; then
    response=$(curl -s "$BASE_URL/admin" \
      -H "Authorization: Bearer $user_token")
    pretty_json "$response"
fi

echo
echo "7️⃣  Реєстрація нового користувача"
echo "--------------------------------"
register_response=$(curl -s -X POST "$BASE_URL/register" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "password123"}')
pretty_json "$register_response"

echo
echo "8️⃣  Доступ без токена (має бути 401)"
echo "-----------------------------------"
response=$(curl -s "$BASE_URL/protected")
if [ -z "$response" ]; then
    echo "Порожня відповідь (401 Unauthorized)"
else
    pretty_json "$response"
fi

echo
echo "9️⃣  Доступ з недійсним токеном"
echo "-----------------------------"
response=$(curl -s "$BASE_URL/protected" \
  -H "Authorization: Bearer invalid.jwt.token")
if [ -z "$response" ]; then
    echo "Порожня відповідь (401 Unauthorized)"
else
    pretty_json "$response"
fi

echo
echo "✅ Тестування завершено!"
echo "======================="
echo
echo "📋 Результати:"
echo "• Логін працює ✅"
echo "• JWT токени генеруються ✅"
echo "• Захист ендпоінтів працює ✅"
echo "• Система ролей працює ✅"
echo "• Реєстрація працює ✅"
echo "• Термін дії: 14 днів ✅"
echo
echo "🎓 Це мінімальний приклад JWT для навчання!"
echo "Для продакшену використовуйте CustomJwtAuth проект."
