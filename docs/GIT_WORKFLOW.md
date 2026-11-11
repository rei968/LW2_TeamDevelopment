# 🌿 Git Workflow: Посібник для команди

Цей посібник містить короткі готові команди для роботи з репозиторієм лабораторної роботи №2.

---

## 📌 Головне правило команди

> [!WARNING]
> **Ніколи не пишіть код і не робіть `git push` напряму в гілку `main` або `develop`!**  
> Всі нові фічі розробляються у власних гілках `feature/...` і потрапляють у `develop` ТІЛЬКИ через **Pull Request (PR)** після перевірки.

---

## 🛠️ Шпаргалка з щоденних команд Git

### 1. Початок роботи (Оновлення коду)
Перед тим як почати робити нове завдання, ЗАВЖДИ оновіть свою локальну гілку `develop`:
```bash
git checkout develop
git pull origin develop
```

### 2. Створення своєї робочої гілки
```bash
git checkout -b feature/ім_я-коротка_назва_задачі
```
*Приклади назв гілок:*
- `feature/zhe-core-storage`
- `feature/lyosha-customer-store`
- `feature/yarik-admin-panel`
- `feature/illia-unit-tests`

### 3. Перевірка змін та збереження (Коміт)
Подивитися, які файли ви змінили:
```bash
git status
```
Додати всі змінені файли до коміту:
```bash
git add .
```
Створити осмислений коміт за стандартом **Conventional Commits**:
```bash
git commit -m "feat: додав кнопку швидкого оформлення замовлення"
```
*Типи префіксів:*
- `feat:` — додано нову функцію або кнопку.
- `fix:` — виправлено баг чи помилку розрахунку.
- `test:` — додано або змінено тест.
- `docs:` — оновлено документацію або звіт.

### 4. Відправка гілки на GitHub
```bash
git push -u origin feature/ім_я-коротка_назва_задачі
```

### 5. Створення Pull Request (PR)
1. Відкрийте сторінку репозиторію на GitHub: [https://github.com/rei968/LW2_TeamDevelopment](https://github.com/rei968/LW2_TeamDevelopment).
2. Натисніть зелену кнопку **Compare & pull request**.
3. **ВАЖЛИВО:** Переконайтеся, що `base:` стоїть **`develop`** (а не `main`).
4. Напишіть коротко в описі: що було зроблено і як перевірити.
5. Натисніть **Create pull request**.
6. Скиньте посилання Тімліду (Же) в чат для перевірки та злиття (Merge).

---

## 💥 Сценарій вирішення Merge Conflict (для викладача)

Якщо двоє студентів змінили один і той самий рядок у файлі, Git не зможе злити їх автоматично і покаже `CONFLICT`.

### Як ми це розв'язуємо:
1. Ви робите:
   ```bash
   git merge develop
   ```
2. Git повідомляє:
   ```text
   CONFLICT (content): Merge conflict in src/Shop.Shared/Models/Order.cs
   Automatic merge failed; fix conflicts and then commit the result.
   ```
3. Відкриваєте файл `Order.cs` у Visual Studio або VS Code. Git позначає конфліктний фрагмент маркерами:
   ```csharp
   <<<<<<< HEAD (ваша поточна гілка)
   public string StatusDisplay => $"Поточний стан: {Status}";
   =======
   public string StatusDisplay => $"[ЗАМОВЛЕННЯ] Статус: {Status}";
   >>>>>>> develop (зміни з develop)
   ```
4. **📸 Робите скріншот цього місця для включення у звіт!**
5. Вручну видаляєте маркери `<<<<<<<`, `=======`, `>>>>>>>` і залишаєте єдиний узгоджений варіант коду:
   ```csharp
   public string StatusDisplay => $"[ЗАМОВЛЕННЯ] Стан: {Status}";
   ```
6. Зберігаєте файл і завершуєте злиття:
   ```bash
   git add src/Shop.Shared/Models/Order.cs
   git commit -m "fix(merge): вирішено конфлікт у Order.cs"
   git push
   ```
7. Конфлікт розв'язано! Робите скріншот консолі з успішним комітом.
