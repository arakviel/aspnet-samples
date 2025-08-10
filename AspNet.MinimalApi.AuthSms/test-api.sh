#!/bin/bash

# SMS Authentication API Test Script
# Цей скрипт демонструє повний цикл аутентифікації через SMS

BASE_URL="http://localhost:5186"
PHONE_NUMBER="+380501234567"

echo "🚀 Тестування SMS Authentication API"
echo "======================================"

# Функція для красивого виводу JSON
pretty_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

# 1. Перевірка статусу API
echo "📡 1. Перевірка статусу API..."
response=$(curl -s "$BASE_URL/")
echo "Відповідь:"
pretty_json "$response"
echo ""

# 2. Відправка SMS коду для реєстрації
echo "📱 2. Відправка SMS коду для реєстрації..."
response=$(curl -s -X POST "$BASE_URL/api/auth/send-registration-code" \
  -H "Content-Type: application/json" \
  -d "{\"phoneNumber\": \"$PHONE_NUMBER\", \"purpose\": \"Registration\"}")

echo "Відповідь:"
pretty_json "$response"

# Витягуємо код з відповіді (для демо)
if command -v jq &> /dev/null; then
    SMS_CODE=$(echo "$response" | jq -r '.data // empty')
else
    # Простий парсинг без jq
    SMS_CODE=$(echo "$response" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)
fi

if [ -z "$SMS_CODE" ] || [ "$SMS_CODE" = "null" ]; then
    echo "❌ Не вдалося отримати SMS код. Перевірте логи сервера."
    echo "💡 В development режимі код виводиться в консоль сервера."
    echo "💡 Використайте код з логів сервера для наступних кроків."
    SMS_CODE="123456"  # Fallback код для демо
fi

echo "📝 Отриманий SMS код: $SMS_CODE"
echo ""

# 3. Реєстрація користувача
echo "👤 3. Реєстрація користувача з SMS кодом..."
response=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"phoneNumber\": \"$PHONE_NUMBER\",
    \"code\": \"$SMS_CODE\",
    \"firstName\": \"Тест\",
    \"lastName\": \"Користувач\",
    \"password\": \"TestPassword123\"
  }")

echo "Відповідь:"
pretty_json "$response"

# Витягуємо JWT токен
if command -v jq &> /dev/null; then
    JWT_TOKEN=$(echo "$response" | jq -r '.accessToken // empty')
else
    JWT_TOKEN=$(echo "$response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
fi

if [ -n "$JWT_TOKEN" ] && [ "$JWT_TOKEN" != "null" ]; then
    echo "🔑 Отриманий JWT токен: ${JWT_TOKEN:0:50}..."
    echo ""
    
    # 4. Отримання інформації про користувача
    echo "ℹ️  4. Отримання інформації про поточного користувача..."
    response=$(curl -s -X GET "$BASE_URL/api/auth/me" \
      -H "Authorization: Bearer $JWT_TOKEN")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # 5. Вихід з системи
    echo "🚪 5. Вихід з системи..."
    response=$(curl -s -X POST "$BASE_URL/api/auth/logout" \
      -H "Authorization: Bearer $JWT_TOKEN")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
else
    echo "❌ Не вдалося отримати JWT токен. Реєстрація не вдалася."
    echo ""
fi

# 6. Тестування входу
echo "🔐 6. Тестування входу існуючого користувача..."

# Відправка SMS коду для входу
echo "📱 6.1. Відправка SMS коду для входу..."
response=$(curl -s -X POST "$BASE_URL/api/auth/send-login-code" \
  -H "Content-Type: application/json" \
  -d "{\"phoneNumber\": \"$PHONE_NUMBER\", \"purpose\": \"Login\"}")

echo "Відповідь:"
pretty_json "$response"

# Витягуємо новий код
if command -v jq &> /dev/null; then
    LOGIN_CODE=$(echo "$response" | jq -r '.data // empty')
else
    LOGIN_CODE=$(echo "$response" | grep -o '"data":"[^"]*"' | cut -d'"' -f4)
fi

if [ -z "$LOGIN_CODE" ] || [ "$LOGIN_CODE" = "null" ]; then
    LOGIN_CODE="123456"  # Fallback код
fi

echo "📝 Отриманий код для входу: $LOGIN_CODE"
echo ""

# Вхід користувача
echo "🔓 6.2. Вхід користувача з SMS кодом..."
response=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{
    \"phoneNumber\": \"$PHONE_NUMBER\",
    \"code\": \"$LOGIN_CODE\"
  }")

echo "Відповідь:"
pretty_json "$response"
echo ""

echo "✅ Тестування завершено!"
echo ""
echo "💡 Додаткові можливості:"
echo "   - Swagger UI: $BASE_URL"
echo "   - API документація доступна через браузер"
echo "   - Всі endpoints підтримують JSON формат"
echo ""
echo "🔧 Для production використання:"
echo "   - Налаштуйте реальний SMS провайдер"
echo "   - Використайте реальну базу даних"
echo "   - Додайте HTTPS сертифікати"
echo "   - Налаштуйте логування та моніторинг"
