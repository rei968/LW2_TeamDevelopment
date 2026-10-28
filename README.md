# 🛒 E-Commerce Platform — Інтернет-магазин
> **Лабораторна робота №2:** Колективна розробка додатків із використанням Git  
> **Команда:** Же (TL + Web API Lead), Ярік (Backend Core + Admin WinForms), Льоша (Client WinForms), Ілля (QA / Tester)

---

## 📌 Про проєкт

Платформа для онлайн-торгівлі на базі **ASP.NET Core Web API** та десктопного клієнта **Windows Forms (WinForms)** із двома інтерфейсами:
1. **Клієнтська частина (Вітрина покупця в WinForms):**
   - Каталог товарів із динамічною фільтрацією (ціновий діапазон, категорії, бренди) через `DataGridView`.
   - Інтерактивний кошик покупок з авто-підрахунком підсумкової вартості.
   - Оформлення замовлення (діалог Checkout).
   - Перегляд відгуків та рейтингу товарів.
2. **Адмін-панель (Інтерфейс менеджерів у WinForms):**
   - Управління каталогом (CRUD товарів і категорій через таблиці).
   - Управління залишками на складі (Stock Management).
   - Обробка та зміна статусів замовлень у реальному часі через Web API.

---

## 🛠️ Стек технологій

- **Backend:** ASP.NET Core Web API (.NET 10 / .NET 8) — REST API, маршрутизація, валідація, бізнес-логіка
- **Frontend / Desktop UI:** Windows Forms (.NET Desktop App) — візуальний Drag-and-Drop дизайнер, `DataGridView`, швидкий запуск без накладних витрат WebAssembly/CSS
- **Спільна бібліотека:** Shared Class Library (`Shop.Shared`) — пряме перевикористання C# DTO, Enums та моделей валідації між сервером і WinForms-клієнтом
- **База даних:** Entity Framework Core + SQLite (`shop.db`) — автономна локальна БД
- **Безпека:** ASP.NET Core Identity + JWT Token Bearer (ролі `Admin`, `Customer`), збереження токена в пам'яті WinForms-клієнта для заголовків `Authorization: Bearer`
- **Тестування:** xUnit + FluentAssertions + Moq (`Shop.Tests`), HTTP API tests (`api-tests.http`)

### Структура Solution (`Shop.sln`):
```text
Shop.sln
├── src/
│   ├── Shop.Api/          # ASP.NET Core Web API, EF Core DbContext, Контролери
│   ├── Shop.WinForms/     # Windows Forms додаток (Вітрина клієнта + Адмін-панель)
│   └── Shop.Shared/       # Спільні DTO, Enums, моделі запитів/відповідей
├── tests/
│   └── Shop.Tests/        # xUnit тести бізнес-логіки та розрахунків
└── docs/
    ├── TESTING.md         # Тест-план, чеклісти перевірок та баг-репорти (Ілля)
    └── GIT_WORKFLOW.md    # Інструкція з гілок, комітів та вирішення конфліктів
```

---

## 👥 Розподіл обов'язків у команді

| Студент | Роль | Основні задачі | Робочі гілки |
| :--- | :--- | :--- | :--- |
| **Же** (`@rei968`) | **Team Lead + Web API Lead** | Архітектура, спільні DTO у `Shop.Shared`, всі контролери Web API (`Auth`, `Products` з фільтрами, `Categories`, `Orders`, `Reviews`), Code Review | `feature/zhe-*` |
| **Ярік** | **Backend Core & Admin WinForms** | База даних (EF Core SQLite), Identity + JWT, `DbSeeder`, Адмін-форма WinForms (`AdminForm.cs`: таблиця товарів, залишків та замовлень) | `feature/yarik-*` |
| **Льоша** | **Client WinForms UI** | Клієнтська вітрина WinForms (`CustomerForm.cs`): каталог у `DataGridView`, панель фільтрів (ціна, категорія, бренд), кошик, Checkout | `feature/lyosha-*` |
| **Ілля** | **QA / Tester** | Тестовий проєкт `Shop.Tests` (xUnit: тести кошика, залишків, статусів), файл `api-tests.http`, документація дефектів | `feature/illia-*` |

Детальний план спринтів та завдань: **[TEAM_PLAN_LR2.md](./TEAM_PLAN_LR2.md)**

---

## 🌿 Git Workflow та правила роботи команди

Проєкт використовує модель **GitHub Flow** із робочою гілкою `develop`:

- `main` — стабільна релізна гілка (захищена, заборонений прямий `push`).
- `develop` — основна робоча гілка для інтеграції фіч.
- `feature/<ім'я>-<назва-задачі>` — індивідуальні гілки розробників.

### Швидкий старт для учасників команди:

1. **Клонувати репозиторій:**
   ```bash
   git clone https://github.com/rei968/LW2_TeamDevelopment.git
   cd LW2_TeamDevelopment
   ```
2. **Перейти на гілку `develop`:**
   ```bash
   git checkout develop
   git pull origin develop
   ```
3. **Створити власну гілку для задачі:**
   ```bash
   git checkout -b feature/ім_я-назва_задачі
   ```
4. **Формат коммітів (Conventional Commits):**
   - `feat: додав компонент фільтрації товарів за ціною`
   - `fix: виправлено розрахунок суми кошика`
   - `test: додано юніт-тест для перевірки залишків товару`
   - `docs: описано баг-репорт #2`
5. **Створення Pull Request (PR):**
   - Створюється PR із вашої гілки у `develop`.
   - Обов'язкове рев'ю від Тімліда (`@rei968`) або колеги.
   - Після схвалення (Approve) виконується злиття (Merge).

Детальні інструкції та сценарій вирішення merge-конфліктів: **[docs/GIT_WORKFLOW.md](./docs/GIT_WORKFLOW.md)**

---

## 🧪 Тестування та баг-трекінг

Всі тест-кейси, результати тестування та виявлені дефекти фіксуються в:
👉 **[docs/TESTING.md](./docs/TESTING.md)**
