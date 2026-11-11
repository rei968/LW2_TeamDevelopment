# 🛒 E-Commerce Desktop Platform
> **Лабораторна робота №2:** Колективна розробка додатків із використанням Git  
> **Стек:** C# (.NET 10 / .NET 8), Windows Forms, System.Text.Json (Local Storage), xUnit  
> **Репозиторій:** [https://github.com/rei968/LW2_TeamDevelopment](https://github.com/rei968/LW2_TeamDevelopment)

---

## 📌 1. Про проєкт

Цей проєкт — навчальний інтернет-магазин (Desktop-додаток на базі **Windows Forms**).  
Головна мета лабораторної роботи — **продемонструвати злагоджену командну роботу в Git** (галуження `feature/*`, Pull Requests, рев'ю коду, вирішення merge-конфліктів та тестування).

Для максимальної надійності та швидкості запуску архітектуру спрощено:
- ❌ **Ніяких складних веб-серверів, портів чи мережевих збоїв.**
- ❌ **Ніяких сторонніх СУБД, паролів до баз даних чи міграцій.**
- ✅ **Запуск в один клік:** відкрив у Visual Studio $\rightarrow$ натиснув **F5** $\rightarrow$ все відразу працює!
- ✅ **Збереження даних:** каталог, залишки на складі та замовлення зберігаються локально в файл `shop_data.json` через швидку серіалізацію `System.Text.Json`.

---

## 👥 2. Розподіл команди та зон відповідальності

У проєкті беруть участь 4 студенти. Кожен відповідає за свій окремий файл і працює у своїй власній гілці в Git:

| Студент | Роль | Файл для роботи | Гілка в Git | Що робить |
| :--- | :--- | :--- | :--- | :--- |
| 👨‍💻 **Же** (`@rei968`) | **Team Lead & Core** | `src/Shop.Shared/Services/ShopService.cs` | `feature/zhe-core-storage` | Архітектура, збереження/завантаження з JSON, організація Merge Conflict, фінальний реліз |
| 🚀 **Ярік** | **Admin UI Lead** | `src/Shop.WinForms/Forms/AdminForm.cs` | `feature/yarik-admin-panel` | Панель менеджера: склад товарів, зміна залишків, список замовлень клієнтів |
| 🎨 **Льоша** | **Customer UI Lead** | `src/Shop.WinForms/Forms/CustomerForm.cs` | `feature/lyosha-customer-store` | Вітрина покупця: перегляд товарів, фільтри за ціною/пошуком, інтерактивний кошик, покупка |
| 🧪 **Ілля** | **QA / Тестувальник** | `tests/Shop.Tests/ShopTests.cs` | `feature/illia-unit-tests` | Написання xUnit-тестів для перевірки кошика та залишків, звіт у `docs/TESTING.md` |

---

## 📂 3. Структура Solution (`Shop.sln`)

```text
LW2_TeamDevelopment/
├── Shop.sln                      # Головний файл Solution для Visual Studio
├── src/
│   ├── Shop.Shared/              # Спільні класи та сервіс даних
│   │   ├── Enums/OrderStatus.cs  # Статуси замовлення (New, Processing, Completed, Cancelled)
│   │   ├── Models/Product.cs     # Модель товару (Id, Name, Price, Category, StockQuantity)
│   │   ├── Models/Order.cs       # Модель замовлення (Id, Customer, TotalAmount, Status)
│   │   ├── Models/OrderItem.cs   # Елемент кошика/замовлення
│   │   └── Services/ShopService.cs # Сервіс з тестовими товарами та методами
│   └── Shop.WinForms/            # Графічний інтерфейс
│       ├── Forms/MainForm.cs     # Головне меню запуску (Вибір: Вітрина / Адмінка)
│       ├── Forms/CustomerForm.cs # Форма магазину для покупця (Зона Льоші)
│       ├── Forms/AdminForm.cs    # Форма керування складом для менеджера (Зона Яріка)
│       └── Program.cs            # Точка входу в програму
├── tests/
│   └── Shop.Tests/               # Автоматизовані тести xUnit (Зона Іллі)
│       └── ShopTests.cs          # Тести бізнес-правил
└── docs/
    ├── TEAM_PLAN_LR2.md          # 📖 ДЕТАЛЬНІ ПОКРОКОВІ ІНСТРУКЦІЇ ДЛЯ КОЖНОГО СТУДЕНТА
    ├── GIT_WORKFLOW.md           # 🌿 Шпаргалка з команд Git та сценарій Merge Conflict
    ├── TESTING.md                # 🧪 Тест-план та журнал багів для тестувальника
    └── ZHE_COORDINATION_BOARD.md # 📋 Координаційна дошка задач Тімліда
```

---

## 🚀 4. Швидкий старт для кожного розробника

### Крок 1. Завантажити проєкт собі на комп'ютер:
```bash
git clone https://github.com/rei968/LW2_TeamDevelopment.git
cd LW2_TeamDevelopment
git checkout develop
git pull origin develop
```

### Крок 2. Відкрити проєкт:
1. Запустити файл `Shop.sln` у **Visual Studio 2022** (або Rider / VS Code).
2. Переконатися, що праворуч у Solution Explorer жирним шрифтом виділено проєкт `Shop.WinForms` (якщо ні — правий клік по `Shop.WinForms` $\rightarrow$ **Set as Startup Project**).
3. Натиснути **F5** (або зелену стрілочку «Start») — відкриється головне вікно з вибором вітрини та адмінки.

### Крок 3. Запустити тести:
У терміналі:
```bash
dotnet test
```
Усі тести мають бути зеленими!

---

## 🌿 5. Як правильно здавати роботу в Git (Пам'ятка команді)

> [!IMPORTANT]
> **НІКОЛИ НЕ ПУШТЕ ПРЯМО В ГІЛКУ `main` ЧИ `develop`!**  
> Кожен студент працює у своїй ізольованій гілці, після чого створює Pull Request на GitHub.

### Стандартний робочий цикл студента:

1. **Створити власну гілку від актуального `develop`:**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/ваше_ім_я-назва_задачі
   ```
2. **Пишете код у своєму файлі**, запускаєте програму через F5 і перевіряєте, що все працює.
3. **Робите коміт із чітким описом українською мовою:**
   ```bash
   git add .
   git commit -m "feat: додав фільтрацію товарів за ціною"
   ```
4. **Відправляєте свою гілку на GitHub:**
   ```bash
   git push -u origin feature/ваше_ім_я-назва_задачі
   ```
5. **Створюєте Pull Request (PR):**
   - Відкриваєте [https://github.com/rei968/LW2_TeamDevelopment](https://github.com/rei968/LW2_TeamDevelopment).
   - Натискаєте кнопку **Compare & pull request**.
   - Переконуєтеся, що цільова гілка (base) — **`develop`**.
   - Тімлід (Же) дивиться ваш код, тисне **Approve** і робить **Merge**.

---

## 📖 Докладна документація для кожного:
- 👉 **Покрокова інструкція особисто для вас:** [TEAM_PLAN_LR2.md](TEAM_PLAN_LR2.md)
- 👉 **Шпаргалка з Git та вирішення конфлікту:** [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md)
- 👉 **Звіт про тестування для Іллі:** [docs/TESTING.md](docs/TESTING.md)
