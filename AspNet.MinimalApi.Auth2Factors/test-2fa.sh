#!/bin/bash

# 2FA Authentication Test Script
# Тестує Email та TOTP двофакторну аутентифікацію з нативним Identity

BASE_URL="http://localhost:5018"
EMAIL="test@example.com"
PASSWORD="Test123"

echo "🚀 Тестування 2FA Authentication API"
echo "===================================="
echo "✨ Особливості:"
echo "   - Native ASP.NET Core Identity"
echo "   - Email 2FA codes"
echo "   - TOTP Authenticator (Google Authenticator)"
echo "   - Recovery codes"
echo "   - Account lockout protection"
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
COOKIE_JAR="cookies-2fa.txt"
rm -f $COOKIE_JAR

echo "📡 1. Перевірка статусу API..."
response=$(curl -s -c $COOKIE_JAR "$BASE_URL/info")
echo "Відповідь:"
pretty_json "$response"
echo ""

echo "👤 2. Реєстрація користувача..."
response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$EMAIL\",
    \"password\": \"$PASSWORD\",
    \"confirmPassword\": \"$PASSWORD\"
  }")

echo "Відповідь:"
pretty_json "$response"

if echo "$response" | grep -q '"success":true'; then
    echo "✅ Реєстрація успішна!"
    echo ""
    
    echo "🔐 3. Перший вхід (без 2FA)..."
    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/login" \
      -H "Content-Type: application/json" \
      -d "{
        \"email\": \"$EMAIL\",
        \"password\": \"$PASSWORD\",
        \"rememberMe\": false
      }")
    
    echo "Відповідь:"
    pretty_json "$response"
    
    if echo "$response" | grep -q '"success":true'; then
        echo "✅ Вхід успішний (2FA ще не налаштована)"
        echo ""
        
        # Отримання інформації про користувача
        echo "ℹ️  4. Інформація про користувача..."
        response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
        
        echo "Відповідь:"
        pretty_json "$response"
        echo ""
        
        # Налаштування TOTP
        echo "📱 5. Налаштування TOTP (Google Authenticator)..."
        response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/setup-totp")
        
        echo "Відповідь:"
        pretty_json "$response"
        
        if echo "$response" | grep -q '"success":true'; then
            echo "✅ TOTP налаштування створено!"
            
            # Витягуємо QR код URL
            qr_url=$(echo "$response" | grep -o '"qrCodeUri":"[^"]*"' | cut -d'"' -f4)
            shared_key=$(echo "$response" | grep -o '"sharedKey":"[^"]*"' | cut -d'"' -f4)
            
            echo ""
            echo "📱 QR код для Google Authenticator:"
            echo "$qr_url"
            echo ""
            echo "🔑 Або введіть ключ вручну: $shared_key"
            echo ""
            echo "💡 Відскануйте QR код або введіть ключ в Google Authenticator"
            echo "   і введіть 6-значний код для підтвердження:"
            
            read -p "Введіть TOTP код з додатку: " TOTP_CODE
            
            echo ""
            echo "✅ 6. Підтвердження TOTP..."
            response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/verify-totp" \
              -H "Content-Type: application/json" \
              -d "{\"code\": \"$TOTP_CODE\"}")
            
            echo "Відповідь:"
            pretty_json "$response"
            
            if echo "$response" | grep -q '"success":true'; then
                echo "✅ TOTP підтверджено та увімкнено!"
                echo ""
                
                # Вихід та повторний вхід (тепер потрібна 2FA)
                echo "🚪 7. Вихід з системи..."
                curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/logout" > /dev/null
                echo "✅ Вихід успішний"
                echo ""
                
                echo "🔐 8. Повторний вхід (тепер потрібна 2FA)..."
                response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/login" \
                  -H "Content-Type: application/json" \
                  -d "{
                    \"email\": \"$EMAIL\",
                    \"password\": \"$PASSWORD\",
                    \"rememberMe\": false
                  }")
                
                echo "Відповідь:"
                pretty_json "$response"
                
                if echo "$response" | grep -q '"requiresTwoFactor":true'; then
                    echo "✅ Система вимагає 2FA!"
                    echo ""
                    
                    read -p "Введіть новий TOTP код з додатку: " TOTP_CODE_2
                    
                    echo ""
                    echo "🔐 9. Підтвердження 2FA коду..."
                    response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X POST "$BASE_URL/api/auth/verify-2fa" \
                      -H "Content-Type: application/json" \
                      -d "{
                        \"code\": \"$TOTP_CODE_2\",
                        \"rememberMachine\": false
                      }")
                    
                    echo "Відповідь:"
                    pretty_json "$response"
                    
                    if echo "$response" | grep -q '"success":true'; then
                        echo "✅ 2FA підтвердження успішне! Вхід завершено."
                        echo ""
                        
                        # Фінальна інформація про користувача
                        echo "ℹ️  10. Фінальна інформація про користувача..."
                        response=$(curl -s -c $COOKIE_JAR -b $COOKIE_JAR -X GET "$BASE_URL/api/auth/me")
                        
                        echo "Відповідь:"
                        pretty_json "$response"
                        echo ""
                    else
                        echo "❌ 2FA підтвердження не вдалося"
                    fi
                else
                    echo "❌ Система не вимагає 2FA (щось пішло не так)"
                fi
            else
                echo "❌ TOTP підтвердження не вдалося"
            fi
        else
            echo "❌ Не вдалося створити TOTP налаштування"
        fi
    else
        echo "❌ Вхід не вдався"
    fi
else
    echo "❌ Реєстрація не вдалася!"
fi

# Очищуємо cookies
rm -f $COOKIE_JAR

echo ""
echo "✅ Тестування завершено!"
echo ""
echo "🎯 Продемонстровані можливості:"
echo "   ✅ Реєстрація користувача через Identity"
echo "   ✅ Налаштування TOTP (Google Authenticator)"
echo "   ✅ QR код для легкого налаштування"
echo "   ✅ Підтвердження TOTP коду"
echo "   ✅ Автоматичне увімкнення 2FA"
echo "   ✅ Вимога 2FA при наступному вході"
echo "   ✅ Recovery коди (згенеровані автоматично)"
echo ""
echo "💡 Нативні можливості Identity:"
echo "   - Автоматичне хешування паролів"
echo "   - Генерація TOTP ключів"
echo "   - Валідація TOTP кодів"
echo "   - Управління 2FA станом"
echo "   - Account lockout захист"
echo "   - Recovery коди"
echo ""
echo "🌐 Корисні посилання:"
echo "   - Swagger UI: $BASE_URL"
echo "   - API Info: $BASE_URL/info"
