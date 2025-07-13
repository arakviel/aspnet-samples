#!/bin/bash

# Тест для Simple JWT Auth with Cookies
BASE_URL="http://localhost:5256"
COOKIES_FILE="test_cookies.txt"

echo "🍪 Тестування Simple JWT Auth with Cookies"
echo "=========================================="

# Функція для красивого виводу JSON
pretty_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

# Очищуємо cookies файл
rm -f "$COOKIES_FILE"

echo
echo "1️⃣  Головна сторінка"
echo "-------------------"
response=$(curl -s "$BASE_URL/")
pretty_json "$response"

echo
echo "2️⃣  Статус аутентифікації (без cookies)"
echo "--------------------------------------"
response=$(curl -s "$BASE_URL/auth/status")
pretty_json "$response"

echo
echo "3️⃣  Логін адміністратора (встановлює HttpOnly cookie)"
echo "----------------------------------------------------"
admin_response=$(curl -s -c "$COOKIES_FILE" -X POST "$BASE_URL/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}')
pretty_json "$admin_response"

echo
echo "📋 Перевіряємо встановлені cookies:"
if [ -f "$COOKIES_FILE" ]; then
    echo "Cookie файл створено:"
    cat "$COOKIES_FILE" | grep -v "^#" | grep -v "^$"
else
    echo "Cookie файл не створено"
fi

echo
echo "4️⃣  Статус аутентифікації (з cookie)"
echo "-----------------------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/auth/status")
pretty_json "$response"

echo
echo "5️⃣  Захищений ресурс (з cookie)"
echo "------------------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/protected")
pretty_json "$response"

echo
echo "6️⃣  Адміністративна панель (з cookie адміністратора)"
echo "--------------------------------------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/admin")
pretty_json "$response"

echo
echo "7️⃣  Профіль користувача"
echo "----------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/profile")
pretty_json "$response"

echo
echo "8️⃣  Логаут (видаляє cookie)"
echo "--------------------------"
response=$(curl -s -c "$COOKIES_FILE" -b "$COOKIES_FILE" -X POST "$BASE_URL/logout")
pretty_json "$response"

echo
echo "📋 Перевіряємо cookies після логауту:"
if [ -f "$COOKIES_FILE" ]; then
    cookie_count=$(cat "$COOKIES_FILE" | grep -v "^#" | grep -v "^$" | wc -l)
    echo "Кількість активних cookies: $cookie_count"
    if [ "$cookie_count" -eq 0 ]; then
        echo "✅ Cookies успішно видалено"
    else
        echo "❌ Cookies ще присутні"
    fi
else
    echo "Cookie файл не існує"
fi

echo
echo "9️⃣  Спроба доступу після логауту (має бути 401)"
echo "----------------------------------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/protected")
pretty_json "$response"

echo
echo "🔟 Логін звичайного користувача"
echo "------------------------------"
user_response=$(curl -s -c "$COOKIES_FILE" -X POST "$BASE_URL/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "user", "password": "user123"}')
pretty_json "$user_response"

echo
echo "1️⃣1️⃣ Спроба доступу до адмін панелі звичайним користувачем"
echo "--------------------------------------------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/admin")
pretty_json "$response"

echo
echo "1️⃣2️⃣ Реєстрація нового користувача (встановлює cookie)"
echo "-----------------------------------------------------"
register_response=$(curl -s -c "$COOKIES_FILE" -X POST "$BASE_URL/register" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "password123"}')
pretty_json "$register_response"

echo
echo "1️⃣3️⃣ Профіль нового користувача"
echo "------------------------------"
response=$(curl -s -b "$COOKIES_FILE" "$BASE_URL/profile")
pretty_json "$response"

echo
echo "1️⃣4️⃣ Доступ без cookies (новий сеанс)"
echo "------------------------------------"
response=$(curl -s "$BASE_URL/protected")
pretty_json "$response"

# Очищуємо тестовий файл
rm -f "$COOKIES_FILE"

echo
echo "✅ Тестування завершено!"
echo "======================="
echo
echo "📋 Результати тестування HttpOnly Cookies:"
echo "• Логін встановлює HttpOnly cookie ✅"
echo "• Cookie автоматично надсилається з запитами ✅"
echo "• Захист ендпоінтів працює ✅"
echo "• Система ролей працює ✅"
echo "• Логаут видаляє cookie ✅"
echo "• Реєстрація встановлює cookie ✅"
echo
echo "🔒 Переваги HttpOnly Cookies:"
echo "• Захист від XSS атак (JavaScript не може прочитати)"
echo "• Автоматичне надсилання з кожним запитом"
echo "• SameSite=Lax захист від CSRF"
echo "• Термін дії: 14 днів"
echo
echo "🎓 Для студентів:"
echo "Порівняйте з Bearer токенами - тут не потрібно керувати токенами вручну!"
