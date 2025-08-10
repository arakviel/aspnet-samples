#!/bin/bash

# Twilio SMS Authentication API Test Script with Native Identity
# Тестує реальну відправку SMS через Twilio з нативним Identity

BASE_URL="http://localhost:5091"
PHONE_NUMBER="+380501234567"  # Замініть на ваш реальний номер телефону
PASSWORD="TwilioTest123"

echo "🚀 Тестування Native Identity SMS Authentication API з Twilio"
echo "=========================================================="
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

# Функція для збереження cookies
COOKIE_JAR="cookies-twilio.txt"
rm -f $COOKIE_JAR

echo "📡 1. Перевірка статусу API..."
response=$(curl -s -c $COOKIE_JAR "$BASE_URL/")
echo "Відповідь:"
pretty_json "$response"
echo ""

echo "👤 2. Реєстрація користувача з нативним Identity..."
response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"phoneNumber\": \"$PHONE_NUMBER\",
    \"password\": \"$PASSWORD\",
    \"confirmPassword\": \"$PASSWORD\"
  }")

echo "Відповідь:"
pretty_json "$response"

# Перевіряємо успішність реєстрації
if echo "$response" | grep -q '"success":true'; then
    echo "✅ Реєстрація успішна! Користувач автоматично увійшов в систему."
    echo ""
    
    # Отримання інформації про користувача
    echo "ℹ️  3. Отримання інформації про поточного користувача..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # Відправка реального SMS коду через Twilio
    echo "📱 4. Відправка реального SMS коду через Twilio..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/send-phone-confirmation")
    
    echo "Відповідь:"
    pretty_json "$response"
    
    if echo "$response" | grep -q '"success":true'; then
        echo "✅ SMS код відправлено через Twilio!"
        wait_for_sms_code
        
        # Підтвердження номера телефону
        echo ""
        echo "✅ 5. Підтвердження номера телефону з реальним SMS кодом..."
        response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/confirm-phone" \
          -H "Content-Type: application/json" \
          -d "{
            \"phoneNumber\": \"$PHONE_NUMBER\",
            \"code\": \"$SMS_CODE\"
          }")
        
        echo "Відповідь:"
        pretty_json "$response"
        
        if echo "$response" | grep -q '"success":true'; then
            echo "✅ Номер телефону підтверджено з реальним SMS!"
            echo ""
            
            # Увімкнення двофакторної аутентифікації
            echo "🔐 6. Увімкнення двофакторної аутентифікації..."
            response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/enable-2fa-sms")
            
            echo "Відповідь:"
            pretty_json "$response"
            
            if echo "$response" | grep -q '"success":true'; then
                echo "✅ Двофакторна аутентифікація увімкнена!"
                echo ""
                
                # Генерація recovery кодів
                echo "🔑 7. Генерація recovery кодів..."
                response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/generate-recovery-codes")
                
                echo "Відповідь:"
                pretty_json "$response"
                echo ""
                
                # Перевірка фінального стану користувача
                echo "ℹ️  8. Перевірка фінального стану користувача..."
                response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
                
                echo "Відповідь:"
                pretty_json "$response"
                echo ""
            else
                echo "❌ Не вдалося увімкнути двофакторну аутентифікацію"
            fi
        else
            echo "❌ Не вдалося підтвердити номер телефону"
            echo "💡 Можливі причини:"
            echo "   - Неправильний SMS код"
            echo "   - Код застарів"
            echo "   - Проблеми з Twilio"
        fi
    else
        echo "❌ Не вдалося відправити SMS код через Twilio"
        echo "💡 Перевірте:"
        echo "   - Налаштування Twilio в appsettings.json"
        echo "   - Чи встановлений Provider: 'Twilio'"
        echo "   - Чи правильний номер телефону"
        echo "   - Чи достатньо коштів на Twilio акаунті"
        echo "   - Логи сервера для деталей помилки"
    fi
    
    # Вихід з системи
    echo "🚪 9. Вихід з системи..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/logout")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
else
    echo "❌ Реєстрація не вдалася!"
    echo "💡 Можливо користувач з таким номером вже існує"
fi

# Очищуємо cookies
rm -f $COOKIE_JAR

echo ""
echo "✅ Тестування завершено!"
echo ""
echo "🎯 Продемонстровані можливості:"
echo "   ✅ Native Identity з реальними SMS через Twilio"
echo "   ✅ Cookie-based аутентифікація"
echo "   ✅ Реальне SMS підтвердження телефону"
echo "   ✅ Двофакторна аутентифікація"
echo "   ✅ Recovery коди"
echo ""
echo "💰 Вартість тестування:"
echo "   - 1 SMS через Twilio (~$0.0075 - $0.05)"
echo "   - Перевірте витрати в Twilio Console"
echo ""
echo "🌐 Корисні посилання:"
echo "   - Twilio Console: https://console.twilio.com"
echo "   - Swagger UI: $BASE_URL"
echo "   - Логи сервера для деталей"
