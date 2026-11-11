# 📋 Координаційна дошка завдань Же (Team Lead)

Цей файл є єдиним джерелом правди (Single Source of Truth) для розподілу завдань у команді та координації між студентами.

---

## 🧭 Поточний статус проєкту
* **Роль:** Же (Team Lead & Core Architecture)
* **Поточна гілка:** `develop`
* **Стек:** C# Windows Forms (.NET 10 / .NET 8) + JSON Local Storage (`shop_data.json`)
* **Поточний стан:** Базовий Solution `Shop.sln`, моделі `Shop.Shared`, форми `Shop.WinForms` та 18 юніт-тестів `Shop.Tests` повністю працездатні.

---

## 📌 Завдання команди за напрямками

### 1. 👨‍💻 Же (Team Lead — Ви)
* **Гілка:** `feature/zhe-core-storage`
* [x] **TASK-ZHE-01:** Ініціалізація спрощеного Solution (`Shop.Shared`, `Shop.WinForms`, `Shop.Tests`).
* [x] **TASK-ZHE-02:** Створення сервісу `ShopDataService.cs` із локальним збереженням у JSON (`shop_data.json`).
* [ ] **TASK-ZHE-03:** Розіграш та вирішення Merge Conflict у `OrderDto.cs` для звіту (`feature/zhe-order-status-conflict`).
* [ ] **TASK-ZHE-04:** Фінальне злиття `develop` -> `main`, встановлення тегу `v1.0.0`, оформлення звіту.

### 2. 🚀 Ярік (Admin UI)
* **Гілка:** `feature/yarik-admin-winforms`
* [x] **TASK-YAR-01:** Створення форми менеджера `AdminForm.cs` (каталог товарів, редагування цін та залишків).
* [x] **TASK-YAR-02:** Обробка замовлень клієнтів (перегляд списку, зміна статусів).

### 3. 🎨 Льоша (Customer UI)
* **Гілка:** `feature/lyosha-customer-store`
* [x] **TASK-LYO-01:** Створення вітрини покупця `CustomerForm.cs` (каталог товарів, панель фільтрів).
* [x] **TASK-LYO-02:** Інтерактивний кошик покупок та діалог оформлення замовлення (Checkout).
* [x] **TASK-LYO-03:** Маршрутизація в `LoginForm.cs`: клієнти автоматично потрапляють у вітрину, менеджери — в адмінку.

### 4. 🧪 Ілля (QA / Тестувальник)
* **Гілка:** `feature/illia-unit-tests`
* [x] **TASK-ILL-01:** Створення набору тестів розрахунку кошика (`OrderCalculationTests.cs`).
* [x] **TASK-ILL-02:** Створення тестів залишків на складі (`StockManagementTests.cs`, `ShopDataServiceTests.cs`).
* [x] **TASK-ILL-03:** Тести переходів статусів замовлень (`OrderStatusTransitionTests.cs`).
* [ ] **TASK-ILL-04:** Оформлення звіту тестування в `docs/TESTING.md`.
