# 📋 Координаційна дошка завдань Же (Team Lead & Web API)

Цей файл є єдиним джерелом правди (Single Source of Truth) для розподілу завдань, фіксації архітектурних контрактів та передачі ТЗ робочим чатам або учасникам команди.

---

## 🧭 Поточний статус проєкту
* **Роль:** Же (Team Lead + Web API Lead)
* **Поточна гілка:** `develop`
* **Стан репозиторію:** 
  - База даних EF Core SQLite, сутності, ASP.NET Core Identity + JWT та DbSeeder — **реалізовано Яріком** (`DONE`).
  - Адмін-панель WinForms (`AdminForm.cs`) та сервіс `ApiClient.cs` — **реалізовано Яріком** (`DONE`).
  - Контролери Web API відкочено для чистої, самостійної реалізації Тімлідом (Же).
* **Поточний етап:** Етап 2 (Реалізація Web API контролерів Тімлідом Же у фіча-гілках).

---

## 📌 Перелік завдань Же (Team Lead)

### 🔹 [ВОРКСТРІМ 1] Web API: Каталог, Категорії та Авторизація
* **Гілка:** `feature/zhe-web-api-products-categories`
* **Задачі:**
  - [ ] **TASK-ZHE-01:** `AuthController` — ендпоінти реєстрації (`POST /api/auth/register`), логіну (`POST /api/auth/login`) та отримання поточного користувача (`GET /api/auth/me`).
  - [ ] **TASK-ZHE-02:** `CategoriesController` — отримання списку категорій (`GET /api/categories`), отримання за ID (`GET /api/categories/{id}`), додавання/редагування/видалення для ролі `Admin`.
  - [ ] **TASK-ZHE-03:** `ProductsController` — каталог товарів (`GET /api/products`) із фільтрацією за діапазоном цін (`minPrice`, `maxPrice`), брендом (`brand`), категорією (`categoryId`), текстовим пошуком (`search`), сортуванням (`sortBy`), пагінацією; отримання за ID; CRUD операції для ролі `Admin`.
* **Статус:** `READY_FOR_DEV`

---

### 🔹 [ВОРКСТРІМ 2] Web API: Замовлення, Відгуки та Тестові запити
* **Гілка:** `feature/zhe-web-api-orders-reviews`
* **Задачі:**
  - [ ] **TASK-ZHE-04:** `OrdersController` — створення замовлення покупцем (`POST /api/orders`) з перевіркою та списанням залишків зі складу, перегляд власних замовлень покупцем (`GET /api/orders/my`), перегляд усіх замовлень адміном (`GET /api/orders`), зміна статусу замовлення (`PUT /api/orders/{id}/status`).
  - [ ] **TASK-ZHE-05:** `ReviewsController` — перегляд відгуків до товару (`GET /api/reviews/product/{productId}`), додавання відгуку авторизованим покупцем (`POST /api/reviews`).
  - [ ] **TASK-ZHE-06:** Файл тестування `api-tests.http` — повна колекція запитів для перевірки всіх контролерів через VS / Rider / VS Code.
* **Статус:** `TODO` (Після завершення Воркстріму 1)

---

### 🔹 [ВОРКСТРІМ 3] Спеціальний сценарій: Merge Conflict у Git (для викладача)
* **Гілка:** `feature/zhe-order-status-conflict`
* **Задачі:**
  - [ ] **TASK-ZHE-07:** Внесення конфліктної зміни в `src/Shop.Shared/DTOs/OrderDto.cs` у гілці Же паралельно з гілкою Яріка.
  - [ ] **TASK-ZHE-08:** Виклик `git merge`, фіксація та скріншоти конфлікту `<<<<<<< HEAD` для звіту, розв'язання конфлікту, коміт `fix(merge): ...`.
* **Статус:** `TODO`

---

### 🔹 [ВОРКСТРІМ 4] Фіналізація та Реліз
* **Гілка:** `main`
* **Задачі:**
  - [ ] **TASK-ZHE-09:** Перевірка інтеграції між Web API та WinForms (маршрутизація входу для покупця `customer@shop.com`).
  - [ ] **TASK-ZHE-10:** Фінальний Pull Request `develop` -> `main`, релізний тег `v1.0.0`, зведення матеріалів до звіту.
* **Статус:** `TODO`

---

## 📐 Архітектурні контракти контролерів Web API

### 1. `AuthController` (`/api/auth`)
* `POST /api/auth/register` — приймає `RegisterRequest` (Email, Password, FullName). Створює користувача через `UserManager`, призначає роль `Customer`. Повертає `AuthResponse` або 400 Bad Request.
* `POST /api/auth/login` — приймає `LoginRequest` (Email, Password). Перевіряє через `UserManager`, генерує токен через `JwtTokenService`. Повертає `AuthResponse` або 401 Unauthorized.
* `GET /api/auth/me` `[Authorize]` — повертає `UserDto` поточного авторизованого користувача на основі Claims (`ClaimTypes.NameIdentifier`, `ClaimTypes.Email`).

### 2. `CategoriesController` (`/api/categories`)
* `GET /api/categories` — повертає `IEnumerable<CategoryDto>`. Доступний для всіх (анонімний).
* `GET /api/categories/{id}` — повертає `CategoryDto` або 404 Not Found.
* `POST /api/categories` `[Authorize(Roles = "Admin")]` — приймає `CategoryDto`, створює категорію, повертає 201 Created.
* `PUT /api/categories/{id}` `[Authorize(Roles = "Admin")]` — оновлює категорію.
* `DELETE /api/categories/{id}` `[Authorize(Roles = "Admin")]` — видаляє категорію (якщо немає прив'язаних товарів).

### 3. `ProductsController` (`/api/products`)
* `GET /api/products` — фільтрація:
  - Параметри запиту: `int? categoryId`, `string? brand`, `decimal? minPrice`, `decimal? maxPrice`, `string? search`, `string? sortBy`, `int page = 1`, `int pageSize = 12`.
  - Повертає `PagedResult<ProductDto>` або список `IEnumerable<ProductDto>`.
* `GET /api/products/{id}` — повертає `ProductDto` (включаючи середній рейтинг відгуків).
* `POST /api/products` `[Authorize(Roles = "Admin")]` — додає товар у базу даних.
* `PUT /api/products/{id}` `[Authorize(Roles = "Admin")]` — оновлює товар.
* `DELETE /api/products/{id}` `[Authorize(Roles = "Admin")]` — видаляє товар.
* `PATCH /api/products/{id}/stock` `[Authorize(Roles = "Admin")]` — швидке оновлення кількості залишків на складі.

### 4. `OrdersController` (`/api/orders`)
* `POST /api/orders` `[Authorize]` — створення замовлення:
  - Приймає `CreateOrderRequest` (список `ProductId` + `Quantity`, адресу та телефон).
  - Перевіряє наявність на складі: якщо `StockQuantity < Quantity` -> повертає 400 Bad Request із зазначенням товару.
  - Списує залишки з таблиці `Products`.
  - Створює `Order` зі статусом `New`, зберігає `OrderItems`.
  - Повертає створений `OrderDto`.
* `GET /api/orders/my` `[Authorize]` — замовлення поточного користувача.
* `GET /api/orders` `[Authorize(Roles = "Admin")]` — усі замовлення для менеджера.
* `PUT /api/orders/{id}/status` `[Authorize(Roles = "Admin")]` — зміна статусу (`New` -> `Processing` -> `Completed` / `Cancelled`). Якщо `Cancelled` — залишки повертаються на склад.

### 5. `ReviewsController` (`/api/reviews`)
* `GET /api/reviews/product/{productId}` — список відгуків до товару.
* `POST /api/reviews` `[Authorize]` — додавання відгуку (Rating 1..5, Comment).

---

## 📜 Журнал оновлень та комунікації (Changelog)
* **2026-11-02 13:00:** Наведено порядок у репозиторії. Видалено попередньо скомпільовані контролери з `develop`, щоб Тімлід (Же) реалізував їх власноруч у фіча-гілках. Оновлено координаційну дошку завдань.
