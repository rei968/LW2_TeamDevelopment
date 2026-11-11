# План виконання ЛР2: Колективна розробка додатків із використанням Git
## Проєкт: Платформа для онлайн-торгівлі (C# Windows Forms + JSON Storage)

---

## 1. Стек технологій та Архітектура

| Рівень | Технології | Опис |
| :--- | :--- | :--- |
| **UI / Desktop** | Windows Forms (.NET Desktop App) | Нативний десктопний додаток із двома формами (Вітрина покупця + Панель менеджера) |
| **Спільний код та сховище** | `Shop.Shared` (Class Library) | DTO, Enums, та сервіс збереження даних `ShopDataService.cs` (`shop_data.json`) |
| **Тестування** | xUnit + FluentAssertions (`Shop.Tests`) | 18 автоматизованих юніт-тестів перевірки розрахунку кошика, списання залишків та фільтрів |

---

## 2. Розподіл ролей та обов'язків

### 👨‍💻 Же (Team Lead & Core Architecture)
* **Гілки в Git:** `feature/zhe-init-solution`, `feature/zhe-core-storage`, `feature/zhe-order-status-conflict`
* **Задачі:**
  1. Ініціалізація Solution, проєктів, `.gitignore` та структури репозиторію.
  2. Проєктування моделей DTO (`ProductDto`, `OrderDto`, `CategoryDto`, `UserDto`) та перерахувань (`OrderStatus`, `UserRole`).
  3. Реалізація сервісу локального сховища даних `ShopDataService.cs` (автоматичне збереження та завантаження з `shop_data.json`, методи фільтрації, створення замовлення та списання залишків).
  4. Code Review усіх Pull Request'ів команди.
  5. Організація та вирішення сценарію **Merge Conflict** у Git разом із Яріком.
  6. Фінальний реліз версії `v1.0.0` у гілку `main`.

---

### 🚀 Ярік (Admin UI Lead)
* **Гілка в Git:** `feature/yarik-admin-winforms`
* **Задачі:**
  1. Розробка адмін-панелі менеджера `AdminForm.cs`:
     - Перегляд каталогу товарів у `DataGridView`, додавання нових товарів (`ProductEditForm.cs`), редагування та видалення.
     - Керування категоріями (`CategoryEditForm.cs`).
     - Швидке коригування залишків товарів на складі.
     - Таблиця обробки замовлень клієнтів (зміна статусів `New` -> `Processing` -> `Completed` / `Cancelled`).

---

### 🎨 Льоша (Customer UI Lead)
* **Гілка в Git:** `feature/lyosha-customer-store`
* **Задачі:**
  1. Розробка клієнтської вітрини `CustomerForm.cs`:
     - Каталог товарів у `DataGridView` із зазначенням цін, наявності та категорій.
     - Панель фільтрації: швидкий пошук за назвою, фільтр за категорією (`ComboBox`) та обмеження за максимальною ціною.
     - Інтерактивний кошик: додавання обраного товару, зміна кількості, видалення позицій, автоматичний перерахунок підсумку.
     - Діалог швидкого оформлення замовлення (ПІБ, телефон, адреса) із валідацією полів.
     - Перегляд історії замовлень поточного клієнта.

---

### 🧪 Ілля (QA Engineer / Тестувальник)
* **Гілка в Git:** `feature/illia-unit-tests`
* **Задачі:**
  1. Написання та підтримка автоматизованих xUnit-тестів у проєкті `Shop.Tests`:
     - Тести розрахунку суми кошика (`OrderCalculationTests.cs`).
     - Тести списання та відновлення залишків при замовленні (`StockManagementTests.cs`, `ShopDataServiceTests.cs`).
     - Тести переходів статусів замовлень (`OrderStatusTransitionTests.cs`).
     - Тести фільтрації товарів за ціною.
  2. Заповнення звіту тестування `docs/TESTING.md`: матриця тест-кейсів та журнал виявлених дефектів (баг-репорти).

---

## 3. Спеціальний сценарій: Вирішення Merge Conflict (для викладача)

1. Же та Ярік працюють у паралельних гілках від `develop`:
   - Же: `feature/zhe-order-status-conflict`
   - Ярік: `feature/yarik-order-discount`
2. Обидва редагують файл `src/Shop.Shared/DTOs/OrderDto.cs` в одному й тому ж місці (властивість форматування рядка статусу):
   - Же: `public string StatusDisplay => $"[Статус замовлення]: {Status}";`
   - Ярік: `public string StatusDisplay => $"Знижка {Discount}% ({Status})";`
3. Же зливає свою гілку в `develop`.
4. Ярік підтягує `develop` у свою гілку (`git merge develop`) і отримує:
   ```text
   CONFLICT (content): Merge conflict in src/Shop.Shared/DTOs/OrderDto.cs
   ```
5. Розробники відкривають файл, роблять скріншот маркерів `<<<<<<< HEAD`, поєднують зміни:
   `public string StatusDisplay => $"Статус: {Status} | Знижка: {Discount}%";`
6. Завершують злиття: `git commit -m "fix(merge): вирішено конфлікт у OrderDto"` і додають скріншоти до звіту.
