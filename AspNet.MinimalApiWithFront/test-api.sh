#!/bin/bash

# Скрипт для тестування CRUD API
BASE_URL="http://localhost:5211/api/products"

echo "🧪 Тестування CRUD API для товарів"
echo "=================================="

# Тест 1: Отримання всіх товарів
echo "📋 1. Отримання всіх товарів:"
curl -s -X GET $BASE_URL | jq '.[0:2]' || echo "Помилка при отриманні товарів"
echo ""

# Тест 2: Створення нового товару
echo "➕ 2. Створення нового товару:"
NEW_PRODUCT='{"name":"Тестовий товар","description":"Товар для тестування API","price":999.99,"stock":5}'
CREATED_PRODUCT=$(curl -s -X POST $BASE_URL -H "Content-Type: application/json" -d "$NEW_PRODUCT")
PRODUCT_ID=$(echo $CREATED_PRODUCT | jq -r '.id')
echo "Створено товар з ID: $PRODUCT_ID"
echo $CREATED_PRODUCT | jq .
echo ""

# Тест 3: Отримання товару за ID
echo "🔍 3. Отримання товару за ID ($PRODUCT_ID):"
curl -s -X GET $BASE_URL/$PRODUCT_ID | jq . || echo "Товар не знайдено"
echo ""

# Тест 4: Оновлення товару
echo "✏️ 4. Оновлення товару:"
UPDATED_PRODUCT='{"name":"Оновлений тестовий товар","description":"Оновлений опис товару","price":1299.99,"stock":8}'
curl -s -X PUT $BASE_URL/$PRODUCT_ID -H "Content-Type: application/json" -d "$UPDATED_PRODUCT" | jq . || echo "Помилка при оновленні"
echo ""

# Тест 5: Видалення товару
echo "🗑️ 5. Видалення товару:"
DELETE_RESPONSE=$(curl -s -w "%{http_code}" -X DELETE $BASE_URL/$PRODUCT_ID)
if [ "$DELETE_RESPONSE" = "204" ]; then
    echo "Товар успішно видалено (HTTP 204)"
else
    echo "Помилка при видаленні: $DELETE_RESPONSE"
fi
echo ""

# Тест 6: Перевірка, що товар видалено
echo "❌ 6. Перевірка видалення (має повернути 404):"
RESPONSE_CODE=$(curl -s -w "%{http_code}" -o /dev/null $BASE_URL/$PRODUCT_ID)
if [ "$RESPONSE_CODE" = "404" ]; then
    echo "✅ Товар успішно видалено (HTTP 404)"
else
    echo "❌ Неочікуваний код відповіді: $RESPONSE_CODE"
fi
echo ""

# Тест 7: Валідація - спроба створити товар з некоректними даними
echo "⚠️ 7. Тест валідації (некоректні дані):"
INVALID_PRODUCT='{"name":"","price":-100,"stock":-5}'
VALIDATION_RESPONSE=$(curl -s -X POST $BASE_URL -H "Content-Type: application/json" -d "$INVALID_PRODUCT")
echo "Відповідь на некоректні дані:"
echo $VALIDATION_RESPONSE | jq . || echo $VALIDATION_RESPONSE
echo ""

echo "🎉 Тестування завершено!"
echo "Для перегляду всіх товарів: curl -X GET $BASE_URL | jq ."
