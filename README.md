# 🌐 ASP.NET HTTP Servers Comparison

Цей репозиторій демонструє різні підходи до створення HTTP серверів на .NET, від нативних бібліотек до сучасних фреймворків.

## 📁 Проєкти

### 🏗️ [AspNet.NativeHttpServer](./AspNet.NativeHttpServer/)
**Повнофункціональний веб-сервер на чистому .NET**
- ✅ CRUD API для продуктів (JSON + HTML)
- ✅ Професійна архітектура (Repository, Service, Controller)
- ✅ 31 тест (19 unit + 12 integration)
- ✅ ~1000 рядків коду з документацією
- 🎯 **Демонструє**: Складність створення веб-додатку без фреймворків

### 🎯 [AspNet.NativeHttpServerSimple](./AspNet.NativeHttpServerSimple/)
**Простий HTTP сервер в одному файлі**
- ✅ Роутинг, Query String, HTTP заголовки
- ✅ HTML форми та POST обробка
- ✅ ~590 рядків коду
- 🎯 **Демонструє**: Базові принципи роботи HTTP серверів

### 🚀 [AspNet.MinimalApiSimple](./AspNet.MinimalApiSimple/)
**Той же функціонал на ASP.NET Core Minimal API**
- ✅ Ідентична функціональність до NativeHttpServerSimple
- ✅ ~150 рядків коду (**75% менше!**)
- ✅ Автоматичний роутинг, model binding, логування
- 🎯 **Демонструє**: Переваги сучасних веб-фреймворків

## 📊 Порівняння

| Проєкт | Рядків коду | Складність | Функціональність | Підтримка |
|--------|-------------|------------|------------------|-----------|
| **NativeHttpServer** | ~1000 | Висока | Повна (CRUD API) | Важка |
| **NativeHttpServerSimple** | ~590 | Середня | Базова (HTTP демо) | Середня |
| **MinimalApiSimple** | ~150 | Низька | Базова (HTTP демо) | Легка |

## 🎓 Освітня цінність

### 💡 Що демонструють проєкти:

1. **Складність без фреймворків**: Скільки коду потрібно написати вручну
2. **Цінність фреймворків**: Як ASP.NET Core спрощує розробку
3. **Еволюція підходів**: Від нативних бібліотек до Minimal API
4. **Найкращі практики**: Архітектура, тестування, документація

### 🔍 Ключові уроки:

- **Нативний підхід** корисний для розуміння основ
- **Фреймворки** економлять 75%+ коду та часу
- **Тестування** критично важливе для якості
- **Архітектура** має значення навіть для простих проєктів

## 🚀 Швидкий старт

```bash
# Клонування репозиторію
git clone <repository-url>
cd AspNet

# Запуск повнофункціонального сервера
cd AspNet.NativeHttpServer
dotnet run
# Відкрийте http://localhost:8081

# Запуск простого нативного сервера
cd ../AspNet.NativeHttpServerSimple
dotnet run
# Відкрийте http://localhost:8082

# Запуск ASP.NET Core Minimal API
cd ../AspNet.MinimalApiSimple
dotnet run
# Відкрийте http://localhost:8083
```

## 🧪 Тестування

```bash
# Запуск всіх тестів
dotnet test

# Тільки unit тести
dotnet test --filter "ProductServiceTests"

# Тільки integration тести
dotnet test --filter "HttpIntegrationTests"
```

## 🎯 Рекомендації

### 📚 Для навчання:
1. Почніть з **NativeHttpServerSimple** - зрозумійте основи
2. Вивчіть **NativeHttpServer** - побачте професійну архітектуру
3. Порівняйте з **MinimalApiSimple** - оцініть переваги фреймворків

### 🏭 Для реальних проєктів:
- **Завжди використовуйте ASP.NET Core** або інші сучасні фреймворки
- Нативний підхід тільки для embedded систем або специфічних вимог
- Інвестуйте в тестування та документацію

## 🛠️ Технології

- **.NET 9.0** - Остання версія платформи
- **HttpListener** - Нативний HTTP сервер
- **ASP.NET Core** - Сучасний веб-фреймворк
- **xUnit** - Фреймворк для тестування
- **HttpClient** - Для integration тестів

## 📖 Детальна документація

Кожен проєкт містить власний README з детальним описом:
- [NativeHttpServer README](./AspNet.NativeHttpServer/README.md)
- [NativeHttpServerSimple README](./AspNet.NativeHttpServerSimple/README.md)
- [MinimalApiSimple README](./AspNet.MinimalApiSimple/README.md)

---

**🎉 Результат**: Ідеальна демонстрація еволюції веб-розробки від нативних бібліотек до сучасних фреймворків!
