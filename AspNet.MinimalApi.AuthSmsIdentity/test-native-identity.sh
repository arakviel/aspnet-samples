#!/bin/bash

# Native Identity SMS Authentication API Test Script
# Демонструє використання нативних можливостей ASP.NET Core Identity

BASE_URL="http://localhost:5091"
PHONE_NUMBER="+380501234567"
PASSWORD="TestPassword123"

echo "🚀 Тестування Native Identity SMS Authentication API"
echo "=================================================="
echo "✨ Використовуємо нативні можливості ASP.NET Core Identity:"
echo "   - Cookie-based authentication"
echo "   - Вбудовані методи UserManager та SignInManager"
echo "   - Нативна підтримка SMS підтвердження"
echo "   - Двофакторна аутентифікація"
echo "   - Recovery коди"
echo ""

# Функція для красивого виводу JSON
pretty_json() {
    if command -v jq &> /dev/null; then
        echo "$1" | jq .
    else
        echo "$1"
    fi
}

# Функція для збереження cookies
COOKIE_JAR="cookies.txt"
rm -f $COOKIE_JAR

# 1. Перевірка статусу API
echo "📡 1. Перевірка статусу API..."
response=$(curl -s -c $COOKIE_JAR "$BASE_URL/")
echo "Відповідь:"
pretty_json "$response"
echo ""

# 2. Реєстрація користувача
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
    
    # 3. Отримання інформації про користувача
    echo "ℹ️  3. Отримання інформації про поточного користувача..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # 4. Відправка SMS коду для підтвердження телефону
    echo "📱 4. Відправка SMS коду для підтвердження телефону..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/send-phone-confirmation")
    
    echo "Відповідь:"
    pretty_json "$response"
    
    if echo "$response" | grep -q '"success":true'; then
        echo "✅ SMS код відправлено!"
        echo "💡 Перевірте логи сервера для отримання коду"
        echo ""
        
        # Запитуємо код у користувача
        read -p "Введіть SMS код з логів сервера: " SMS_CODE
        
        # 5. Підтвердження номера телефону
        echo ""
        echo "✅ 5. Підтвердження номера телефону..."
        response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/confirm-phone" \
          -H "Content-Type: application/json" \
          -d "{
            \"phoneNumber\": \"$PHONE_NUMBER\",
            \"code\": \"$SMS_CODE\"
          }")
        
        echo "Відповідь:"
        pretty_json "$response"
        
        if echo "$response" | grep -q '"success":true'; then
            echo "✅ Номер телефону підтверджено!"
            echo ""
            
            # 6. Увімкнення двофакторної аутентифікації
            echo "🔐 6. Увімкнення двофакторної аутентифікації через SMS..."
            response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/enable-2fa-sms")
            
            echo "Відповідь:"
            pretty_json "$response"
            
            if echo "$response" | grep -q '"success":true'; then
                echo "✅ Двофакторна аутентифікація увімкнена!"
                echo ""
                
                # 7. Генерація recovery кодів
                echo "🔑 7. Генерація recovery кодів..."
                response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/generate-recovery-codes")
                
                echo "Відповідь:"
                pretty_json "$response"
                echo ""
                
                # 8. Перевірка оновленої інформації про користувача
                echo "ℹ️  8. Перевірка оновленої інформації про користувача..."
                response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
                
                echo "Відповідь:"
                pretty_json "$response"
                echo ""
                
                # 9. Вимкнення двофакторної аутентифікації
                echo "🔓 9. Вимкнення двофакторної аутентифікації..."
                response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/disable-2fa")
                
                echo "Відповідь:"
                pretty_json "$response"
                echo ""
            else
                echo "❌ Не вдалося увімкнути двофакторну аутентифікацію"
            fi
        else
            echo "❌ Не вдалося підтвердити номер телефону"
        fi
    else
        echo "❌ Не вдалося відправити SMS код"
    fi
    
    # 10. Вихід з системи
    echo "🚪 10. Вихід з системи..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/logout")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # 11. Тестування входу
    echo "🔐 11. Тестування входу існуючого користувача..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/login" \
      -H "Content-Type: application/json" \
      -d "{
        \"phoneNumber\": \"$PHONE_NUMBER\",
        \"password\": \"$PASSWORD\",
        \"rememberMe\": false
      }")
    
    echo "Відповідь:"
    pretty_json "$response"
    
    if echo "$response" | grep -q '"success":true'; then
        echo "✅ Вхід успішний!"
    else
        echo "❌ Помилка при вході"
    fi
    
else
    echo "❌ Реєстрація не вдалася!"
    echo "💡 Можливо користувач з таким номером вже існує"
fi

# Очищуємо cookies
rm -f $COOKIE_JAR

echo ""
echo "✅ Тестування завершено!"
echo ""
echo "🎯 Продемонстровані можливості Native Identity:"
echo "   ✅ Cookie-based аутентифікація"
echo "   ✅ Автоматичний вхід після реєстрації"
echo "   ✅ SMS підтвердження номера телефону"
echo "   ✅ Двофакторна аутентифікація"
echo "   ✅ Генерація recovery кодів"
echo "   ✅ Управління сесіями"
echo ""
echo "💡 Переваги нативного підходу:"
echo "   - Мінімум кастомного коду"
echo "   - Вбудована безпека"
echo "   - Готові методи для всіх операцій"
echo "   - Автоматичне управління сесіями"
echo "   - Підтримка lockout та recovery"
echo ""
echo "🌐 Корисні посилання:"
echo "   - Swagger UI: $BASE_URL"
echo "   - Логи сервера для SMS кодів"
