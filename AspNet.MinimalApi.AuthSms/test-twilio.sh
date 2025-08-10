#!/bin/bash

# Twilio SMS Authentication API Test Script
# Цей скрипт тестує API з реальною відправкою SMS через Twilio

BASE_URL="http://localhost:5186"
PHONE_NUMBER="+380501234567"  # Замініть на ваш реальний номер телефону

echo "🚀 Тестування SMS Authentication API з Twilio"
echo "=============================================="
echo "⚠️  УВАГА: Цей скрипт відправляє реальні SMS через Twilio!"
echo "⚠️  Переконайтеся, що:"
echo "   - Twilio налаштований в appsettings.json"
echo "   - Provider встановлений в 'Twilio'"
echo "   - Номер телефону $PHONE_NUMBER може отримувати SMS"
echo ""

read -p "Продовжити? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Тестування скасовано."
    exit 1
fi

# Функція для красивого виводу JSON
pretty_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

# Функція для очікування вводу користувача
wait_for_sms_code() {
    echo ""
    echo "📱 SMS код відправлено на номер $PHONE_NUMBER"
    echo "⏳ Очікуйте отримання SMS (може зайняти до 30 секунд)"
    echo ""
    read -p "Введіть отриманий SMS код: " SMS_CODE
    echo "Введений код: $SMS_CODE"
}

echo "📡 1. Перевірка статусу API..."
response=$(curl -s "$BASE_URL/")
echo "Відповідь:"
pretty_json "$response"
echo ""

echo "📱 2. Відправка реального SMS коду для реєстрації..."
echo "   Номер: $PHONE_NUMBER"
response=$(curl -s -X POST "$BASE_URL/api/auth/send-registration-code" \
  -H "Content-Type: application/json" \
  -d "{\"phoneNumber\": \"$PHONE_NUMBER\", \"purpose\": \"Registration\"}")

echo "Відповідь від сервера:"
pretty_json "$response"

# Перевіряємо чи запит був успішним
if echo "$response" | grep -q '"success":true'; then
    echo "✅ SMS код успішно відправлено!"
    wait_for_sms_code
else
    echo "❌ Помилка при відправці SMS коду!"
    echo "💡 Перевірте:"
    echo "   - Налаштування Twilio в appsettings.json"
    echo "   - Чи встановлений Provider: 'Twilio'"
    echo "   - Чи правильний номер телефону"
    echo "   - Чи достатньо коштів на Twilio акаунті"
    exit 1
fi

echo ""
echo "👤 3. Реєстрація користувача з реальним SMS кодом..."
response=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"phoneNumber\": \"$PHONE_NUMBER\",
    \"code\": \"$SMS_CODE\",
    \"firstName\": \"Twilio\",
    \"lastName\": \"Тестер\",
    \"password\": \"TwilioTest123\"
  }")

echo "Відповідь:"
pretty_json "$response"

# Витягуємо JWT токен
if command -v jq &> /dev/null; then
    JWT_TOKEN=$(echo "$response" | jq -r '.accessToken // empty')
    SUCCESS=$(echo "$response" | jq -r '.success // false')
else
    JWT_TOKEN=$(echo "$response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    SUCCESS=$(echo "$response" | grep -o '"success":[^,}]*' | cut -d':' -f2)
fi

if [ "$SUCCESS" = "true" ] && [ -n "$JWT_TOKEN" ] && [ "$JWT_TOKEN" != "null" ]; then
    echo "✅ Реєстрація успішна!"
    echo "🔑 JWT токен отримано: ${JWT_TOKEN:0:50}..."
    echo ""
    
    # Тестуємо авторизований endpoint
    echo "ℹ️  4. Отримання інформації про користувача..."
    response=$(curl -s -X GET "$BASE_URL/api/auth/me" \
      -H "Authorization: Bearer $JWT_TOKEN")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # Вихід з системи
    echo "🚪 5. Вихід з системи..."
    response=$(curl -s -X POST "$BASE_URL/api/auth/logout" \
      -H "Authorization: Bearer $JWT_TOKEN")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # Тестуємо вхід
    echo "🔐 6. Тестування входу з новим SMS кодом..."
    echo "📱 Відправка SMS коду для входу..."
    response=$(curl -s -X POST "$BASE_URL/api/auth/send-login-code" \
      -H "Content-Type: application/json" \
      -d "{\"phoneNumber\": \"$PHONE_NUMBER\", \"purpose\": \"Login\"}")
    
    echo "Відповідь:"
    pretty_json "$response"
    
    if echo "$response" | grep -q '"success":true'; then
        wait_for_sms_code
        
        echo ""
        echo "🔓 Вхід користувача з SMS кодом..."
        response=$(curl -s -X POST "$BASE_URL/api/auth/login" \
          -H "Content-Type: application/json" \
          -d "{
            \"phoneNumber\": \"$PHONE_NUMBER\",
            \"code\": \"$SMS_CODE\"
          }")
        
        echo "Відповідь:"
        pretty_json "$response"
        
        if echo "$response" | grep -q '"success":true'; then
            echo "✅ Вхід успішний!"
        else
            echo "❌ Помилка при вході"
        fi
    else
        echo "❌ Помилка при відправці SMS коду для входу"
    fi
    
else
    echo "❌ Реєстрація не вдалася!"
    echo "💡 Можливі причини:"
    echo "   - Неправильний SMS код"
    echo "   - Код застарів (більше 5 хвилин)"
    echo "   - Користувач з таким номером вже існує"
fi

echo ""
echo "✅ Тестування завершено!"
echo ""
echo "📊 Статистика Twilio:"
echo "   - Перевірте витрати в Twilio Console"
echo "   - Переглядайте логи повідомлень в Monitor → Messaging"
echo ""
echo "💡 Корисні посилання:"
echo "   - Twilio Console: https://console.twilio.com"
echo "   - Swagger UI: $BASE_URL"
echo "   - Документація: TWILIO_SETUP.md"
