# 📋 Координаційна дошка завдань Же (Team Lead & Web API)

Цей файл є єдиним джерелом правди (Single Source of Truth) для взаємодії між Головним чатом (Тімлідом) та підлеглими чатами-розробниками.

---

## 🧭 Поточний статус проєкту
* **Роль користувача:** Же (Team Lead + Web API Lead)
* **Поточна гілка:** `feature/zhe-init-solution`
* **Поточний стан:** Базовий Solution `Shop.sln` та 4 проєкти створені, посилання налаштовані, білд успішний.
* **Поточний етап:** Етап 1 (Реалізація спільних DTO та Enums у `Shop.Shared`)

---

## 📌 Дошка завдань за напрямками

### [ВОРКСТРІМ 1] Базові моделі та DTO (`Shop.Shared`)
*Призначено для: Чат-Розробник 1 (Shared Models)*  
*Гілка: `feature/zhe-init-solution`*
- [ ] **TASK-SH-01:** Створити Enums (`OrderStatus`, `UserRole`) у просторі `Shop.Shared.Enums`.
- [ ] **TASK-SH-02:** Створити Auth DTO (`LoginRequest`, `RegisterRequest`, `AuthResponse`, `UserDto`) у `Shop.Shared.DTOs.Auth`.
- [ ] **TASK-SH-03:** Створити Product & Category DTO (`ProductDto`, `ProductFilterParameters`, `CategoryDto`, `PagedResult<T>`) у `Shop.Shared.DTOs.Catalog`.
- [ ] **TASK-SH-04:** Створити Order & Review DTO (`OrderDto`, `OrderItemDto`, `CreateOrderRequest`, `ReviewDto`, `CreateReviewRequest`) у `Shop.Shared.DTOs.Orders` та `Shop.Shared.DTOs.Reviews`.
*Статус виконання:* `READY_FOR_DEV` (Контракти зафіксовано нижче)  
*Звіт чату:* Очікує запуску промпту в Чат-Розробник 1.

---

### [ВОРКСТРІМ 2] Web API: Каталог і Авторизація (`Shop.Api`)
*Призначено для: Чат-Розробник 2 (Auth & Catalog API)*  
*Гілка: `feature/zhe-web-api-products-categories`*
- [ ] **TASK-API-01:** `AuthController` — реєстрація, логін, профіль поточного користувача.
- [ ] **TASK-API-02:** `CategoriesController` — отримання списку категорій, перегляд за ID.
- [ ] **TASK-API-03:** `ProductsController` — каталог з пагінацією, фільтрацією (ціна, бренд, категорія) та сортуванням.
*Статус виконання:* `BLOCKED` (Очікує готовності `Shop.Shared` DTO від Воркстріму 1)  
*Звіт чату:* Очікує.

---

### [ВОРКСТРІМ 3] Web API: Замовлення та Відгуки (`Shop.Api`)
*Призначено для: Чат-Розробник 3 (Orders & Reviews API)*  
*Гілка: `feature/zhe-web-api-orders-reviews`*
- [ ] **TASK-API-04:** `OrdersController` — створення замовлення, валідація кошика, історія клієнта, зміна статусу.
- [ ] **TASK-API-05:** `ReviewsController` — додавання відгуку з рейтингом (1-5), отримання відгуків для товару.
*Статус виконання:* `TODO` (Заплановано після Воркстріму 2)  
*Звіт чату:* Очікує.

---

### [ВОРКСТРІМ 4] Спеціальний сценарій: Merge Conflict у Git
*Призначено для: Головний чат (Тімлід) разом з гілкою Яріка*  
*Гілка: `feature/zhe-order-status`*
- [ ] **TASK-CONF-01:** Зміна `OrderDto.cs` (додавання статусної логіки) для розіграшу конфлікту з Яріком (`feature/yarik-order-discount`).
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
  - `TotalPages` (int => (int)Math.Ceiling(TotalCount / (double)PageSize))

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
* **`CreateReviewRequest`**: `ProductId` (int), `Rating` (int [Range(1, 5)]), `Comment` (string [Required])

---

## 📜 Журнал оновлень та комунікації (Changelog)

* **2026-10-23 13:50:** Ініціалізовано координаційну дошку завдань для ролі Же.
* **2026-10-23 13:56:** Створено гілку `feature/zhe-init-solution`. Згенеровано Solution `Shop.sln` і проєкти (`Shop.Shared`, `Shop.Api`, `Shop.Client`, `Shop.Tests`). Налаштовано зв'язки між проєктами, білд перевірено (`Build succeeded 0 Errors`). Зафіксовано архітектурні контракти для `Shop.Shared` DTO. Статус Воркстріму 1 переведено в `READY_FOR_DEV`.
