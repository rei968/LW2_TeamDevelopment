# 📋 Координаційна дошка завдань Же (Team Lead & Web API)

Цей файл є єдиним джерелом правди (Single Source of Truth) для взаємодії між Головним чатом (Тімлідом) та підлеглими чатами-розробниками.

---

## 🧭 Поточний статус проєкту
* **Роль користувача:** Же (Team Lead + Web API Lead)
* **Поточна гілка:** `feature/zhe-init-solution`
* **Поточний стан:** Базовий Solution `Shop.sln`, 4 проєкти та спільні DTO створені, збірка успішна, перші юніт-тести проходять.
* **Поточний етап:** Етап 1 (Завершення ініціалізації Solution та злиття в `develop`)

---

## 📌 Дошка завдань за напрямками

### [ВОРКСТРІМ 1] Базові моделі та DTO (`Shop.Shared`)
*Призначено для: Тімлід (Же)*  
*Гілка: `feature/zhe-init-solution`*
- [x] **TASK-SH-01:** Створити Enums (`OrderStatus`, `UserRole`) у просторі `Shop.Shared.Enums`.
- [x] **TASK-SH-02:** Створити Auth DTO (`LoginRequest`, `RegisterRequest`, `AuthResponse`, `UserDto`) у `Shop.Shared.DTOs.Auth`.
- [x] **TASK-SH-03:** Створити Product & Category DTO (`ProductDto`, `ProductFilterParameters`, `CategoryDto`, `PagedResult<T>`) у `Shop.Shared.DTOs.Catalog`.
- [x] **TASK-SH-04:** Створити Order & Review DTO (`OrderDto`, `OrderItemDto`, `CreateOrderRequest`, `ReviewDto`, `CreateReviewRequest`) у `Shop.Shared.DTOs.Orders` та `Shop.Shared.DTOs.Reviews`.
*Статус виконання:* `DONE`  
*Звіт чату:* Моделі та DTO успішно створено, протестовано компіляцію.

---

### [ВОРКСТРІМ 2] Web API: Каталог і Авторизація (`Shop.Api`)
*Призначено для: Чат-Розробник / Же (Auth & Catalog API)*  
*Гілка: `feature/zhe-web-api-products-categories`*
- [ ] **TASK-API-01:** `AuthController` — реєстрація, логін, профіль поточного користувача.
- [ ] **TASK-API-02:** `CategoriesController` — отримання списку категорій, перегляд за ID.
- [ ] **TASK-API-03:** `ProductsController` — каталог з пагінацією, фільтрацією (ціна, бренд, категорія) та сортуванням.
*Статус виконання:* `READY_FOR_DEV` (Очікує злиття `feature/zhe-init-solution` у `develop`)  
*Звіт чату:* Очікує.

---

### [ВОРКСТРІМ 3] Windows Forms Клієнт (`Shop.WinForms`)
*Призначено для: Льоша (Client UI) та Ярік (Admin UI)*  
*Гілки: `feature/lyosha-winforms-catalog`, `feature/yarik-admin-winforms`*
- [ ] **TASK-WF-01:** Льоша: головна форма вітрини покупця (`CustomerForm.cs`), `DataGridView` каталогу, панель фільтрів за ціною та категоріями.
- [ ] **TASK-WF-02:** Льоша: інтерфейс кошика, зміна кількості товару, діалог оформлення `CheckoutForm.cs`.
- [ ] **TASK-WF-03:** Ярік: адмін-форма (`AdminForm.cs`), керування товарами, складом та обробка замовлень.
*Статус виконання:* `TODO` (Очікує створення API та сервісів)

---

### [ВОРКСТРІМ 4] Спеціальний сценарій: Merge Conflict у Git
*Призначено для: Головний чат (Тімлід) разом з гілкою Яріка*  
*Гілка: `feature/zhe-order-status`*
- [ ] **TASK-CONF-01:** Зміна `OrderDto.cs` (властивість `StatusDisplay`) для розіграшу конфлікту з Яріком (`feature/yarik-order-discount`).
- [ ] **TASK-CONF-02:** Демонстрація маркерів `<<<<<<<`, розв'язання конфлікту, фіксація у звіті.
*Статус виконання:* `TODO`

---

## 📐 Архітектурні рішення та контракти (Веде Тімлід)

### 1. Перерахування (Enums) — `Shop.Shared.Enums`
* **`UserRole`**:
  ```csharp
  public enum UserRole { Customer, Admin }
  ```
* **`OrderStatus`**:
  ```csharp
  public enum OrderStatus
  {
      New = 0,
      Processing = 1,
      Completed = 2,
      Cancelled = 3
  }
  ```

### 2. DTO Автентифікації — `Shop.Shared.DTOs.Auth`
* **`LoginRequest`**: `Email` (string, [Required, EmailAddress]), `Password` (string, [Required])
* **`RegisterRequest`**: `Email` (string, [Required, EmailAddress]), `Password` (string, [Required, MinLength(6)]), `FullName` (string, [Required])
* **`AuthResponse`**: `Token` (string), `Email` (string), `FullName` (string), `Roles` (List<string>), `ExpiresAt` (DateTime)
* **`UserDto`**: `Id` (string), `Email` (string), `FullName` (string), `Roles` (List<string>)

### 3. DTO Каталогу — `Shop.Shared.DTOs.Catalog`
* **`CategoryDto`**: `Id` (int), `Name` (string), `Description` (string?)
* **`ProductDto`**:
  - `Id` (int), `Name` (string), `Description` (string), `Price` (decimal), `StockQuantity` (int)
  - `Brand` (string), `CategoryId` (int), `CategoryName` (string?), `ImageUrl` (string?)
* **`ProductFilterParameters`**:
  - `SearchTerm` (string?), `CategoryId` (int?), `Brand` (string?)
  - `MinPrice` (decimal?), `MaxPrice` (decimal?)
  - `SortBy` (string? default "name"), `PageNumber` (int = 1), `PageSize` (int = 12)
* **`PagedResult<T>`**:
  - `Items` (List<T>), `TotalCount` (int), `PageNumber` (int), `PageSize` (int)
  - `TotalPages` (int => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0)

### 4. DTO Замовлень та Відгуків — `Shop.Shared.DTOs.Orders` та `Shop.Shared.DTOs.Reviews`
* **`OrderItemDto`**:
  - `ProductId` (int), `ProductName` (string), `UnitPrice` (decimal), `Quantity` (int)
  - `TotalPrice` (decimal => UnitPrice * Quantity)
* **`CreateOrderItemRequest`**: `ProductId` (int), `Quantity` (int)
* **`CreateOrderRequest`**: `ShippingAddress` (string), `PhoneNumber` (string), `Items` (List<CreateOrderItemRequest>)
* **`OrderDto`**:
  - `Id` (int), `UserId` (string), `CustomerEmail` (string), `CustomerName` (string), `CreatedAt` (DateTime)
  - `Status` (OrderStatus), `ShippingAddress` (string), `PhoneNumber` (string), `TotalAmount` (decimal)
  - `Items` (List<OrderItemDto>), `StatusDisplay` (string => $"Статус: {Status}")
* **`ReviewDto`**: `Id` (int), `ProductId` (int), `UserId` (string), `CustomerName` (string), `Rating` (int 1-5), `Comment` (string), `CreatedAt` (DateTime)
* **`CreateReviewRequest`**: `ProductId` (int), `Rating` (int), `Comment` (string)

---

## 📜 Журнал оновлень та комунікації (Changelog)

* **2026-11-02 12:40:** Створено гілку `feature/zhe-init-solution`. Створено Solution `Shop.sln` і проєкти (`Shop.Shared`, `Shop.Api`, `Shop.WinForms`, `Shop.Tests`). Налаштовано зв'язки між проєктами, білд перевірено (`Build succeeded: 0 Warning(s), 0 Error(s)`). Реалізовано повний набір спільних DTO та Enums. Додано перший пройдений xUnit тест `OrderItemTests`.
