# 📖 Персональний покроковий гід для кожного студента (ЛР2)
> **Проєкт:** Платформа інтернет-магазину (C# Windows Forms + JSON Storage)  
> **Мета лабораторної:** Продемонструвати злагоджену колективну розробку за допомогою Git, Pull Requests та вирішення Merge Conflict.

---

## 🧭 Швидка навігація для студентів:
- [👨‍💻 Інструкція для Же (Team Lead)](#1-інструкція-для-же-team-lead)
- [🎨 Інструкція для Льоші (Вітрина покупця)](#2-інструкція-для-льоші-вітрина-покупця)
- [🚀 Інструкція для Яріка (Панель адміністратора)](#3-інструкція-для-яріка-панель-адміністратора)
- [🧪 Інструкція для Іллі (Тестувальник / QA)](#4-інструкція-для-іллі-тестувальник--qa)
- [💥 Як ми розігруємо Merge Conflict для викладача](#5-як-ми-розігруємо-merge-conflict-для-викладача)

---

## 1. 👨‍💻 Інструкція для Же (Team Lead)

### 🎯 Твоя місія:
Ти — лідер команди. Ти організував Solution, налаштував моделі в `Shop.Shared`, керуєш злиттям коду в `develop`, допомагаєш хлопцям і робиш фінальний реліз `v1.0.0`.

### 📂 Твій робочий файл:
- `src/Shop.Shared/Services/ShopService.cs`

### 💻 Твоє завдання з коду:
Додати в `ShopService.cs` реальне збереження товарів і замовлень у JSON-файл за допомогою стандартного `System.Text.Json`:
1. У методі `SaveToFile(string path)` написати:
   ```csharp
   var data = new { Products, Orders };
   var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
   File.WriteAllText(path, json);
   ```
2. У методі `LoadFromFile(string path)` перевірити `if (File.Exists(path))` і завантажити дані.

### 🌿 Твої команди в терміналі:
```bash
# 1. Створити свою гілку від develop:
git checkout develop
git pull origin develop
git checkout -b feature/zhe-core-storage

# 2. Написати код, перевірити білд:
dotnet build

# 3. Закомітити та відправити на GitHub:
git add .
git commit -m "feat: реалізовано збереження та завантаження каталогу в JSON файл"
git push -u origin feature/zhe-core-storage

# 4. Відкрити PR на GitHub у гілку develop та прийняти його після тестування.
```

### 🗣️ Що говорити викладачу на захисті:
> *«Я як Тімлід створив каркас проєкту, налаштував зв'язки між проєктами в Solution, розробив базові моделі та сервіс збереження даних у JSON. Проводив код-рев'ю всіх Pull Request'ів команди, перевіряв білди та разом із Яріком продемонстрував вирішення конфлікту злиття в Git.»*

---

## 2. 🎨 Інструкція для Льоші (Вітрина покупця)

### 🎯 Твоя місія:
Ти відповідаєш за те, що бачить звичайний покупець: каталог товарів, пошук, додавання в кошик та оформлення покупки.

### 📂 Твій робочий файл:
- `src/Shop.WinForms/Forms/CustomerForm.cs`

### 💻 Твоє завдання з коду:
У файлі вже є таблиця товарів і таблиця кошика. Тобі потрібно його покращити:
1. **Пошук товарів:** додати зверху `TextBox txtSearch` та кнопку `btnSearch`. При кліку фільтрувати товари:
   ```csharp
   var query = txtSearch.Text.ToLower();
   _gridProducts.DataSource = ShopService.Instance.Products
       .Where(p => p.Name.ToLower().Contains(query))
       .ToList();
   ```
2. **Видалення з кошика:** додати кнопку `btnRemove`, яка при кліку видаляє виділений товар з `_cart` і викликає `UpdateCart()`.
3. **Очищення кошика:** кнопка `btnClear`, яка робить `_cart.Clear(); UpdateCart();`.

### 🌿 Твої команди в терміналі:
```bash
# 1. Оновити develop і створити свою гілку:
git checkout develop
git pull origin develop
git checkout -b feature/lyosha-customer-store

# 2. Написати код у CustomerForm.cs, запустити через F5 і перевірити

# 3. Закомітити та запушити:
git add .
git commit -m "feat: додано пошук товарів та видалення позицій з кошика"
git push -u origin feature/lyosha-customer-store

# 4. Відкрити на GitHub Pull Request у гілку develop і скинути посилання Тімліду!
```

### 🗣️ Що говорити викладачу на захисті:
> *«Я розробив клієнтську частину інтернет-магазину в CustomerForm. Реалізував інтерактивний кошик із динамічним перерахунком суми, швидкий пошук товарів за назвою та діалог оформлення замовлення. Свою задачу виконав у гілці feature/lyosha-customer-store і злив у develop через Pull Request.»*

---

## 3. 🚀 Інструкція для Яріка (Панель адміністратора)

### 🎯 Твоя місія:
Ти відповідаєш за інтерфейс менеджера магазину: контроль залишків на складі та перегляд замовлень, які надійшли від покупців.

### 📂 Твій робочий файл:
- `src/Shop.WinForms/Forms/AdminForm.cs`

### 💻 Твоє завдання з коду:
У файлі вже є таблиця складу і таблиця замовлень. Тобі потрібно додати кнопки керування:
1. **Зміна залишків:** кнопки «+5 шт» та «-1 шт» для виділеного у таблиці товару:
   ```csharp
   if (_gridStock.CurrentRow?.DataBoundItem is Product p)
   {
       p.StockQuantity += 5;
       RefreshData();
   }
   ```
2. **Зміна статусу замовлення:** додати кнопку «Змінити статус на Processing» (В обробці):
   ```csharp
   // Знаходимо вибране замовлення в таблиці та оновлюємо статус:
   order.Status = OrderStatus.Processing;
   RefreshData();
   ```

### 🌿 Твої команди в терміналі:
```bash
# 1. Оновити develop і створити свою гілку:
git checkout develop
git pull origin develop
git checkout -b feature/yarik-admin-panel

# 2. Написати код у AdminForm.cs, запустити через F5 і перевірити

# 3. Закомітити та запушити:
git add .
git commit -m "feat: реалізовано зміну залишків товару на складі та оновлення статусів"
git push -u origin feature/yarik-admin-panel

# 4. Відкрити Pull Request у гілку develop на GitHub!
```

### 🗣️ Що говорити викладачу на захисті:
> *«Я створив панель адміністратора в AdminForm: облік товарів на складі, оперативне коригування залишків та перегляд замовлень клієнтів із можливістю оновлення їх статусів. Також брав участь у демонстрації розв'язання merge-конфлікту в Git.»*

---

## 4. 🧪 Інструкція для Іллі (Тестувальник / QA)

### 🎯 Твоя місія:
Ти забезпечуєш надійність проєкту: пишеш автоматизовані тести на xUnit та ведеш журнал тестування для звіту.

### 📂 Твої робочі файли:
- `tests/Shop.Tests/ShopTests.cs` (код тестів)
- `docs/TESTING.md` (документація тест-кейсів та знайдених багів)

### 💻 Твоє завдання з коду:
Додати в `ShopTests.cs` 2-3 нових тести бізнес-правил:
```csharp
[Fact]
public void NewOrder_ShouldHaveStatusNew()
{
    var service = new ShopService();
    var order = service.CreateOrder("Олег", "+380991234567", new List<OrderItem>());
    
    order.Status.Should().Be(OrderStatus.New);
}

[Fact]
public void CalculateTotal_WithMultipleItems_ShouldSumCorrectly()
{
    var items = new List<OrderItem>
    {
        new() { ProductId = 1, ProductName = "Товар 1", UnitPrice = 500m, Quantity = 2 }, // 1000
        new() { ProductId = 2, ProductName = "Товар 2", UnitPrice = 300m, Quantity = 3 }  // 900
    };
    
    decimal total = items.Sum(x => x.TotalPrice);
    total.Should().Be(1900m);
}
```
Також відкрити `docs/TESTING.md` і переконатися, що таблиця тест-кейсів заповнена.

### 🌿 Твої команди в терміналі:
```bash
# 1. Оновити develop і створити свою гілку:
git checkout develop
git pull origin develop
git checkout -b feature/illia-unit-tests

# 2. Написати тести, запустити перевірку:
dotnet test

# 3. Закомітити та запушити:
git add .
git commit -m "test: додано xUnit тести перевірки статусів замовлень та сум кошика"
git push -u origin feature/illia-unit-tests

# 4. Відкрити Pull Request у develop!
```

### 🗣️ Що говорити викладачу на захисті:
> *«Я відповідав за якість коду та тестування: написав набір автоматизованих юніт-тестів на xUnit із використанням FluentAssertions для перевірки логіки кошика та замовлень (усі тести зелені). Склав матрицю тест-кейсів у TESTING.md та описав виявлені баг-репорти.»*

---

## 5. 💥 Як ми розігруємо Merge Conflict для викладача

> [!TIP]
> Це обов'язковий пункт захисту лабораторної! Викладач хоче бачити, що студенти розуміють, що таке маркер `<<<<<<< HEAD` і як вирішувати конфлікт злиття.

### 🎭 Сценарій між Же та Яріком:
Обидва одночасно змінюють рядок 19 у файлі `src/Shop.Shared/Models/Order.cs`:
```csharp
public string StatusDisplay => $"Статус: {Status}";
```

1. **Же** у своїй гілці змінює його на:
   ```csharp
   public string StatusDisplay => $"[ЗАМОВЛЕННЯ] Статус: {Status}";
   ```
   Комітить і зливає свій PR у `develop`.
2. **Ярік** у своїй гілці той самий рядок змінює на:
   ```csharp
   public string StatusDisplay => $"Поточний стан: {Status}";
   ```
   Комітить у себе.
3. Тепер **Ярік** робить злиття актуального `develop` у свою гілку:
   ```bash
   git merge develop
   ```
4. **Git видає червоне повідомлення:**  
   `CONFLICT (content): Merge conflict in src/Shop.Shared/Models/Order.cs`
5. **📸 РОБИМО СКРІНШОТ ДЛЯ ЗВІТУ!**  
   Відкриваємо `Order.cs` у VS Code / Visual Studio і бачимо:
   ```csharp
   <<<<<<< HEAD
   public string StatusDisplay => $"Поточний стан: {Status}";
   =======
   public string StatusDisplay => $"[ЗАМОВЛЕННЯ] Статус: {Status}";
   >>>>>>> develop
   ```
6. **Вирішення конфлікту:** видаляємо маркери і залишаємо гарний об'єднаний варіант:
   ```csharp
   public string StatusDisplay => $"[ЗАМОВЛЕННЯ] Стан: {Status}";
   ```
7. Зберігаємо файл і завершуємо злиття:
   ```bash
   git add src/Shop.Shared/Models/Order.cs
   git commit -m "fix(merge): вирішено конфлікт у Order.cs між гілками Же та Яріка"
   git push
   ```
8. **📸 РОБИМО СКРІНШОТ УСПІШНОГО КОМІТУ З РОЗВ'ЯЗАНИМ КОНФЛІКТОМ!**
