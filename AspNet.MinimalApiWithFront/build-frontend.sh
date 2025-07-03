#!/bin/bash

echo "🔨 Збірка TypeScript фронтенду..."

# Перехід до папки wwwroot
cd wwwroot

# Перевірка наявності node_modules
if [ ! -d "node_modules" ]; then
    echo "📦 Встановлення залежностей..."
    npm install
fi

# Збірка проєкту
echo "🏗️ Компіляція TypeScript та збірка з Vite..."
npm run build

echo "✅ Збірка завершена! Файли знаходяться в wwwroot/dist/"
echo "🚀 Тепер можна запускати ASP.NET проєкт"
