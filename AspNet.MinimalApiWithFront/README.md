# CRUD Демонстрація - Управління Товарами

Демонстрація повнофункціонального CRUD додатку з використанням ASP.NET Minimal API на бекенді та ванільного TypeScript з Vite на фронтенді.

## 🚀 Особливості

- **Бекенд**: ASP.NET Minimal API з .NET 9
- **База даних**: SQLite з Entity Framework Core
- **Фронтенд**: Ванільний TypeScript з Vite
- **Архітектура**: Класична багатошарова архітектура
- **CRUD операції**: Повний набір операцій Create, Read, Update, Delete
- **Валідація**: Серверна та клієнтська валідація
- **Документація**: Українською мовою з детальними коментарями

## 📁 Структура проєкту

```
AspNet.MinimalApiWithFront/
├── Data/                    # Контекст бази даних
│   └── ApplicationDbContext.cs
├── Models/                  # Моделі даних
│   └── Product.cs
├── Repositories/            # Репозиторії для доступу до даних
│   ├── IProductRepository.cs
│   └── ProductRepository.cs
├── Services/               # Бізнес-логіка
│   ├── IProductService.cs
│   └── ProductService.cs
├── wwwroot/               # Статичні файли та фронтенд
│   ├── src/
│   │   ├── api.ts         # API клієнт
│   │   ├── types.ts       # TypeScript типи
│   │   └── main.ts        # Головна логіка фронтенду
│   ├── index.html         # Головна сторінка
│   ├── package.json       # Конфігурація npm
│   ├── tsconfig.json      # Конфігурація TypeScript
│   └── vite.config.ts     # Конфігурація Vite
├── Program.cs             # Точка входу з Minimal API
└── appsettings.json       # Конфігурація додатку
```

## 🛠️ Технології

### Бекенд
- **ASP.NET Core 9** - веб-фреймворк
- **Entity Framework Core** - ORM для роботи з базою даних
- **SQLite** - легка база даних
- **Minimal API** - спрощений підхід до створення API

### Фронтенд
- **TypeScript** - типізована версія JavaScript
- **Vite** - швидкий інструмент збірки
- **Vanilla JS/TS** - без додаткових фреймворків
- **CSS Grid/Flexbox** - сучасна верстка

## 🚀 Запуск проєкту

### Передумови
- .NET 9 SDK
- Node.js та npm (для збірки TypeScript фронтенду)

### Швидкий запуск
```bash
# 1. Збірка TypeScript фронтенду
./build-frontend.sh

# 2. Запуск ASP.NET проєкту
dotnet run --project AspNet.MinimalApiWithFront
```

Сервер буде доступний за адресою: `http://localhost:5211`

### Детальні інструкції

#### Збірка фронтенду
```bash
# Перейти до папки wwwroot
cd wwwroot

# Встановити залежності (якщо ще не встановлені)
npm install

# Зібрати TypeScript для продакшену
npm run build

# Або запустити в режимі розробки з hot reload
npm run dev
```

#### Запуск бекенду
```bash
# Перейти до папки проєкту
cd AspNet.MinimalApiWithFront

# Запустити проєкт
dotnet run
```

## 📋 API Endpoints

### Товари (Products)

| Метод | URL | Опис |
|-------|-----|------|
| GET | `/api/products` | Отримати всі товари |
| GET | `/api/products/{id}` | Отримати товар за ID |
| POST | `/api/products` | Створити новий товар |
| PUT | `/api/products/{id}` | Оновити товар |
| DELETE | `/api/products/{id}` | Видалити товар |

### Приклади запитів

**Отримати всі товари:**
```bash
curl -X GET http://localhost:5211/api/products
```

**Створити новий товар:**
```bash
curl -X POST http://localhost:5211/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Новий товар",
    "description": "Опис товару",
    "price": 1000.00,
    "stock": 10
  }'
```

## 🎯 Функціональність

### Веб-інтерфейс
- ✅ Перегляд списку товарів у вигляді карток
- ✅ Додавання нових товарів через форму
- ✅ Редагування існуючих товарів
- ✅ Видалення товарів з підтвердженням
- ✅ Валідація форм
- ✅ Повідомлення про успіх/помилки
- ✅ Адаптивний дизайн

### Бекенд
- ✅ CRUD операції для товарів
- ✅ Валідація даних
- ✅ Обробка помилок
- ✅ Автоматичне створення бази даних
- ✅ Початкові дані (seed data)
- ✅ CORS підтримка

## 🏗️ Архітектурні принципи

1. **Розділення відповідальності** - кожен клас має свою чітку роль
2. **Dependency Injection** - використання DI контейнера ASP.NET Core
3. **Repository Pattern** - абстракція доступу до даних
4. **Service Layer** - бізнес-логіка винесена в окремий шар
5. **Clean Code** - читабельний код з українськими коментарями

## 📝 Модель даних

### Product (Товар)
```csharp
public class Product
{
    public int Id { get; set; }              // Унікальний ідентифікатор
    public string Name { get; set; }         // Назва товару (обов'язково)
    public string? Description { get; set; } // Опис товару (опціонально)
    public decimal Price { get; set; }       // Ціна товару (обов'язково)
    public int Stock { get; set; }           // Кількість на складі
    public DateTime CreatedAt { get; set; }  // Дата створення
    public DateTime UpdatedAt { get; set; }  // Дата останнього оновлення
}
```

## 🔧 Налаштування

### База даних
База даних SQLite створюється автоматично при першому запуску в файлі `products.db`.

### CORS
CORS налаштований для дозволу всіх origins, methods та headers для розробки.

## 🎨 Дизайн

Інтерфейс використовує сучасний Material Design підхід з:
- Картковим відображенням товарів
- Адаптивною сіткою
- Приємною колірною схемою
- Анімаціями при наведенні
- Зручними формами

## 📚 Навчальні цілі

Цей проєкт демонструє:
- Створення Minimal API в ASP.NET Core
- Роботу з Entity Framework Core
- Організацію коду за принципами Clean Architecture
- Створення TypeScript фронтенду без фреймворків
- Інтеграцію фронтенду та бекенду
- Валідацію даних на різних рівнях
- Обробку помилок та користувацький досвід

## 🤝 Внесок

Проєкт створений для навчальних цілей. Ви можете використовувати його як основу для власних проєктів або для вивчення технологій.
