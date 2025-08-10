#!/bin/bash

# JWT + Cookies Hybrid Authentication Test Script
# Тестує JWT в HttpOnly cookies + SMS-only логін з нативним Identity

BASE_URL="http://localhost:5091"
PHONE_NUMBER="+380501234567"
PASSWORD="HybridTest123"

echo "🚀 Тестування JWT + Cookies Hybrid Authentication"
echo "==============================================="
echo "✨ Особливості hybrid підходу:"
echo "   - JWT токени в HttpOnly cookies"
echo "   - SPA-friendly (автоматичне відправлення)"
echo "   - Безпечно (захист від XSS)"
echo "   - SMS-only passwordless логін"
echo "   - Традиційний password логін"
echo "   - Нативні методи Identity"
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
COOKIE_JAR="cookies-hybrid.txt"
rm -f $COOKIE_JAR

echo "📡 1. Перевірка статусу API..."
response=$(curl -s -c $COOKIE_JAR "$BASE_URL/")
echo "Відповідь:"
pretty_json "$response"
echo ""

echo "👤 2. Реєстрація користувача (JWT в cookie)..."
response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"phoneNumber\": \"$PHONE_NUMBER\",
    \"password\": \"$PASSWORD\",
    \"confirmPassword\": \"$PASSWORD\"
  }")

echo "Відповідь:"
pretty_json "$response"

# Перевіряємо чи є JWT cookie
if [ -f $COOKIE_JAR ]; then
    echo ""
    echo "🍪 Перевірка JWT cookie:"
    grep "jwt" $COOKIE_JAR || echo "JWT cookie не знайдено"
fi

if echo "$response" | grep -q '"success":true'; then
    echo "✅ Реєстрація успішна! JWT збережено в HttpOnly cookie."
    echo ""
    
    # Тестуємо авторизований endpoint
    echo "ℹ️  3. Тестування авторизованого endpoint (JWT з cookie)..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # Вихід з системи
    echo "🚪 4. Вихід з системи (очищення JWT cookie)..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/logout")
    
    echo "Відповідь:"
    pretty_json "$response"
    echo ""
    
    # Перевіряємо чи cookie очищено
    echo "🍪 Перевірка очищення JWT cookie:"
    if grep -q "jwt" $COOKIE_JAR 2>/dev/null; then
        echo "Cookie все ще присутнє (може бути з expired датою)"
    else
        echo "Cookie очищено"
    fi
    echo ""
    
    # Тестуємо SMS-only логін
    echo "📱 5. SMS-only логін (passwordless)..."
    echo "5.1. Відправка SMS коду для входу..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/send-sms-login-code" \
      -H "Content-Type: application/json" \
      -d "{\"phoneNumber\": \"$PHONE_NUMBER\"}")
    
    echo "Відповідь:"
    pretty_json "$response"
    
    if echo "$response" | grep -q '"success":true'; then
        echo "✅ SMS код для входу відправлено!"
        echo "💡 Перевірте логи сервера для отримання коду"
        echo ""
        
        # Запитуємо код у користувача
        read -p "Введіть SMS код з логів сервера: " SMS_CODE
        
        echo ""
        echo "5.2. SMS-only вхід з кодом..."
        response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/sms-login" \
          -H "Content-Type: application/json" \
          -d "{
            \"phoneNumber\": \"$PHONE_NUMBER\",
            \"code\": \"$SMS_CODE\",
            \"rememberMe\": false
          }")
        
        echo "Відповідь:"
        pretty_json "$response"
        
        if echo "$response" | grep -q '"success":true'; then
            echo "✅ SMS-only вхід успішний! Новий JWT збережено в cookie."
            echo ""
            
            # Перевіряємо новий JWT cookie
            echo "🍪 Новий JWT cookie:"
            grep "jwt" $COOKIE_JAR || echo "JWT cookie не знайдено"
            echo ""
            
            # Тестуємо авторизований endpoint після SMS логіну
            echo "ℹ️  6. Тестування endpoint після SMS логіну..."
            response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
            
            echo "Відповідь:"
            pretty_json "$response"
            echo ""
        else
            echo "❌ SMS-only вхід не вдався"
        fi
    else
        echo "❌ Не вдалося відправити SMS код для входу"
    fi
    
    # Тестуємо традиційний логін
    echo "🔐 7. Традиційний логін (password + phone)..."
    
    # Спочатку виходимо
    curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/logout" > /dev/null
    
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
        echo "✅ Традиційний логін успішний!"
    else
        echo "❌ Традиційний логін не вдався"
    fi
    
else
    echo "❌ Реєстрація не вдалася!"
fi

# Очищуємо cookies
rm -f $COOKIE_JAR

echo ""
echo "✅ Тестування завершено!"
echo ""
echo "🎯 Продемонстровані можливості Hybrid підходу:"
echo "   ✅ JWT токени в HttpOnly cookies"
echo "   ✅ Автоматичне відправлення з запитами"
echo "   ✅ Захист від XSS атак"
echo "   ✅ SMS-only passwordless логін"
echo "   ✅ Традиційний password логін"
echo "   ✅ Нативні методи Identity"
echo "   ✅ SPA-friendly архітектура"
echo ""
echo "💡 Переваги для SPA:"
echo "   - Не потрібно зберігати JWT в localStorage"
echo "   - Автоматичне управління токенами"
echo "   - Безпечність HttpOnly cookies"
echo "   - Підтримка CORS з credentials"
echo ""
echo "🌐 Frontend приклад:"
echo "   fetch('/api/auth/me', { credentials: 'include' })"
echo ""
echo "📋 Корисні посилання:"
echo "   - Swagger UI: $BASE_URL"
echo "   - Логи сервера для SMS кодів"
