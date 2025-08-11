# 📝 ASP.NET Core Minimal API Blog з Frontend

Повнофункціональний блог-додаток, побудований з використанням ASP.NET Core Minimal API, Entity Framework Core, PostgreSQL та сучасного JavaScript frontend.

## 🚀 Особливості

### Backend (ASP.NET Core Minimal API)
- **Архітектура**: Vertical Slice Architecture для кращої організації коду
- **Авторизація**: JWT токени з ASP.NET Core Identity
- **База даних**: PostgreSQL з Entity Framework Core
- **Міграції**: Автоматичне застосування міграцій при запуску
- **API**: RESTful ендпоінти з OpenAPI/Swagger документацією
- **Безпека**: Роль-базована авторизація (User, Admin)

### Frontend (Vanilla JavaScript)
- **Модульна архітектура**: ES6 модулі з чіткою структурою
- **Адаптивний дизайн**: Mobile-first підхід з CSS Grid/Flexbox
- **SPA навігація**: Односторінковий додаток без перезавантаження
- **Адмін панель**: Повноцінна панель адміністрування
- **Модальні вікна**: Інтерактивні форми для CRUD операцій

### Функціональність
- ✅ Реєстрація та авторизація користувачів
- ✅ Створення, редагування та видалення постів
- ✅ Система лайків для постів
- ✅ Коментарі до постів
- ✅ Адмін панель для керування користувачами та контентом
- ✅ Роль-базована авторизація
- ✅ Адаптивний дизайн для всіх пристроїв

## 🛠 Технології

### Backend
- **ASP.NET Core 9.0** - Minimal API
- **Entity Framework Core** - ORM для роботи з базою даних
- **PostgreSQL** - Реляційна база даних
- **ASP.NET Core Identity** - Система авторизації
- **JWT Bearer** - Токени авторизації
- **Swagger/OpenAPI** - Документація API

### Frontend
- **Vanilla JavaScript (ES6+)** - Без фреймворків
- **CSS3** - Сучасні стилі з CSS Grid/Flexbox
- **HTML5** - Семантична розмітка
- **Fetch API** - HTTP запити

### DevOps
- **Docker & Docker Compose** - Контейнеризація
- **PostgreSQL** - База даних в контейнері
- **Nginx** - Веб-сервер (опціонально)

## 📁 Структура проекту

```
AspNet.MinimalApi.BlogWithFront/
├── Common/                     # Загальні інтерфейси та утиліти
│   └── IEndpoint.cs           # Інтерфейс для ендпоінтів
├── Data/                      # Контекст бази даних
│   └── AppDbContext.cs        # EF Core контекст
├── Domain/                    # Доменні моделі
│   ├── ApplicationUser.cs     # Модель користувача
│   ├── Post.cs               # Модель посту
│   ├── Comment.cs            # Модель коментаря
│   └── PostLike.cs           # Модель лайку
├── Migrations/               # EF Core міграції
├── Slices/                   # Vertical Slices (функціональність)
│   ├── Auth/                 # Авторизація
│   ├── Posts/                # Пости
│   ├── Comments/             # Коментарі
│   ├── Likes/                # Лайки
│   └── Admin/                # Адмін функції
├── wwwroot/                  # Статичні файли frontend
│   ├── js/                   # JavaScript модулі
│   │   ├── api.js           # API клієнт
│   │   ├── utils.js         # Утилітарні функції
│   │   └── main.js          # Головний файл додатку
│   ├── styles.css           # Стилі
│   └── index.html           # Головна сторінка
├── Program.cs               # Точка входу додатку
├── docker-compose.yml       # Docker Compose конфігурація
├── Dockerfile              # Docker образ
└── README.md               # Документація
```

## 🚀 Швидкий старт

### Вимоги
- **Docker** та **Docker Compose**
- **Git**

### Запуск
1. **Клонуйте репозиторій**:
   ```bash
   git clone <repository-url>
   cd AspNet.MinimalApi.BlogWithFront
   ```

2. **Запустіть додаток**:
   ```bash
   docker compose up --build
   ```

3. **Відкрийте браузер**:
   - Frontend: http://localhost:8081
   - API документація: http://localhost:8081/swagger

### Перший запуск
1. Зареєструйте нового користувача через frontend
2. Для отримання адмін прав, підключіться до бази даних та оновіть роль користувача:
   ```sql
   -- Підключення до PostgreSQL
   docker exec -it aspnetminimalapiblogwithfront-db-1 psql -U bloguser -d blogdb
   
   -- Додавання ролі Admin
   INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName") 
   VALUES (gen_random_uuid()::text, 'Admin', 'ADMIN');
   
   -- Призначення ролі користувачу (замініть user_id на ID вашого користувача)
   INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") 
   SELECT 'your-user-id', "Id" FROM "AspNetRoles" WHERE "Name" = 'Admin';
   ```

## 📚 API Документація

### Авторизація
- `POST /auth/register` - Реєстрація користувача
- `POST /auth/login` - Вхід користувача

### Пости
- `GET /posts` - Отримання всіх постів
- `GET /posts/{id}` - Отримання посту за ID
- `POST /posts` - Створення нового посту (потрібна авторизація)
- `PUT /posts/{id}` - Оновлення посту (автор або адмін)
- `DELETE /posts/{id}` - Видалення посту (автор або адмін)

### Коментарі
- `POST /posts/{postId}/comments` - Додавання коментаря
- `PUT /comments/{id}` - Оновлення коментаря (автор або адмін)
- `DELETE /comments/{id}` - Видалення коментаря (автор або адмін)

### Лайки
- `POST /posts/{postId}/likes/toggle` - Перемикання лайку

### Адмін
- `GET /admin/users` - Список користувачів (тільки адмін)
- `GET /admin/posts` - Список постів з деталями (тільки адмін)
- `POST /admin/users/assign-role` - Призначення ролі (тільки адмін)
- `POST /admin/users/remove-role` - Видалення ролі (тільки адмін)

## 🎨 Frontend Архітектура

### Модулі
- **api.js** - Централізований API клієнт з автоматичним керуванням токенами
- **utils.js** - Утилітарні функції (форматування дат, JWT декодування, тощо)
- **main.js** - Головний клас додатку з керуванням станом та навігацією

### Особливості
- **Модульна архітектура** - Чіткий поділ відповідальності
- **Централізоване керування станом** - Один клас BlogApp керує всім станом
- **Автоматичне керування токенами** - JWT токени зберігаються та оновлюються автоматично
- **Адаптивний дизайн** - Mobile-first підхід з CSS Grid/Flexbox
- **Доступність** - Семантична HTML розмітка та ARIA атрибути

## 🔧 Розробка

### Локальна розробка без Docker
1. **Встановіть PostgreSQL** та створіть базу даних
2. **Оновіть connection string** в `Program.cs`
3. **Запустіть міграції**:
   ```bash
   dotnet ef database update
   ```
4. **Запустіть додаток**:
   ```bash
   dotnet run
   ```

### Додавання нових ендпоінтів
1. Створіть новий файл в папці `Slices/`
2. Реалізуйте інтерфейс `IEndpoint`
3. Ендпоінт автоматично зареєструється через рефлексію

### Додавання нових міграцій
```bash
dotnet ef migrations add MigrationName
```

## 🐳 Docker

### Конфігурація
- **API**: Порт 8081
- **PostgreSQL**: Порт 5432 (внутрішній)
- **Volumes**: Дані PostgreSQL зберігаються в Docker volume

### Команди
```bash
# Запуск
docker compose up -d

# Перебудова
docker compose up --build

# Зупинка
docker compose down

# Логи
docker compose logs -f api
```

## 🔒 Безпека

### Авторизація
- JWT токени з коротким терміном дії
- Роль-базована авторизація (User, Admin)
- Захищені ендпоінти вимагають валідного токена

### Валідація
- Валідація всіх вхідних даних
- Захист від SQL ін'єкцій через EF Core
- CORS налаштування для безпечних запитів

## 📱 Адаптивність

Додаток повністю адаптивний та працює на:
- 📱 Мобільних пристроях (320px+)
- 📱 Планшетах (768px+)
- 💻 Десктопах (1024px+)
- 🖥 Великих екранах (1200px+)

## 🤝 Внесок

1. Fork проекту
2. Створіть feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit зміни (`git commit -m 'Add some AmazingFeature'`)
4. Push в branch (`git push origin feature/AmazingFeature`)
5. Відкрийте Pull Request

## 📄 Ліцензія

Цей проект ліцензовано під MIT License - дивіться файл [LICENSE](LICENSE) для деталей.

## 👨‍💻 Автор

Створено як демонстрація сучасних веб-технологій та кращих практик розробки.

---

**Насолоджуйтесь використанням! 🚀**
