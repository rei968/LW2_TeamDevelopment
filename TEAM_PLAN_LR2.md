# План командної розробки проєкту CineSphere

Лабораторна робота №2: Командна розробка програмного забезпечення з використанням Git.  
Розподіл обов'язків реалізовано за технологічними напрямками (Frontend 1, Logic/JS, Frontend 2, QA).

---

## 1. Склад команди та закріплені ролі

| Студент | Роль | Зона відповідальності | Робоча гілка Git | Ключові файли |
| :--- | :--- | :--- | :--- | :--- |
| Же (`@rei968`) | Team Lead, Frontend 1 | Каркас index.html, навігація, вітрина фільмів, фільтрація, код-рев'ю | `feature/zhe-catalog-ui` | `index.html`, `css/components/header.css`, `css/components/catalog.css`, `js/ui/catalogView.js` |
| Ярік (`@YaroslavYanushevych`) | JS / Logic Lead | Бізнес-логіка: розрахунок цін, бронювання, LocalStorage, моделювання конфлікту | `feature/yarik-booking-logic` | `js/pricing.js`, `js/booking.js`, `js/db.js`, `js/state.js` |
| Льоша (`@oleksiystratonov`) | Frontend 2 & Hall UI | Схема залу, інтерактивні сидіння, екран, модальні вікна, дизайн-система | `feature/lyosha-hall-ui` | `css/components/hall.css`, `css/components/modal.css`, `css/variables.css`, `css/main.css` |
| Ілля (`@Iluha`) | QA Engineer | Автотести у tests.html, тест-план, чек-лісти, звіти тестування | `feature/illia-qa-tests` | `tests.html`, `scripts/run_tests.mjs`, `docs/TESTING.md`, `docs/QA_Report_Illia.docx` |

---

## 2. Індивідуальні завдання учасників

### 2.1. Студент 1: Же (Team Lead & Frontend 1)

Мета:
Розробка базової архітектури, верстка головної вітрини кінотеатру, реалізація пошуку та фільтрації фільмів, проведення код-рев'ю та координація фінального релізу.

Порядок виконання:
1. Створення робочої гілки:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/zhe-catalog-ui
   ```
2. Реалізація функціоналу:
   - Вдосконалення карток фільмів у `css/components/catalog.css` (плавні переходи, фокус-ефекти).
   - Відображення бейджів форматів показу (`IMAX`, `3D`, `Dolby Atmos`) для сеансів.
   - Забезпечення стабільної роботи фільтрів за жанрами (`Sci-Fi`, `Action`, `Drama`, `Animation`, `Biography`) та очищення пошуку.
3. Фіксація та публікація змін:
   ```bash
   git add css/components/catalog.css js/ui/catalogView.js index.html
   git commit -m "feat(catalog): вдосконалено вітрину фільмів, додано hover-ефекти та бейджі форматів показу"
   git push -u origin feature/zhe-catalog-ui
   ```
4. Оформлення Pull Request у гілку `develop` та проведення злиття після перевірки.

---

### 2.2. Студент 2: Ярік (JS / Logic Lead)

Мета:
Забезпечення бізнес-правил ціноутворення, валідація параметрів бронювання, збереження транзакцій у LocalStorage та моделювання конфлікту злиття.

Порядок виконання:
1. Створення робочої гілки:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/yarik-booking-logic
   ```
2. Реалізація функціоналу:
   - У `js/pricing.js`: коректний розрахунок націнок для категорій `vip` (+50 грн) та `couches` (+100 грн), дисконтні правила.
   - У `js/booking.js`: валідація ліміту до 6 місць на замовлення, генерація коду ваучера `CS-XXXXX`.
   - У `js/db.js`: звільнення місць (`status: available`) після скасування замовлення.
3. Фіксація та публікація змін:
   ```bash
   git add js/pricing.js js/booking.js js/db.js
   git commit -m "feat(logic): реалізовано динамічне ціноутворення та захист від подвійного бронювання"
   git push -u origin feature/yarik-booking-logic
   ```
4. Участь у моделюванні конфлікту в `js/pricing.js` відповідно до сценарію в `docs/GIT_WORKFLOW.md`.
5. Оформлення Pull Request у `develop` та злиття після апруву.

---

### 2.3. Студент 3: Льоша (Frontend 2 & Hall UI)

Мета:
Розробка інтерактивної схеми кінозалу, стилізація вигнутого екрана, налаштування колірних категорій сидінь та модальних вікон електронного квитка.

Порядок виконання:
1. Створення робочої гілки:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/lyosha-hall-ui
   ```
2. Реалізація функціоналу:
   - У `css/components/hall.css`: стилізація вигнутого екрана `.screen-curve`, диференціація категорій місць (Стандарт, VIP, Дивани), мікро-анімація вибору сидінь.
   - У `css/components/modal.css`: оформлення цифрового квитка-ваучера з лінією перфорації, імітацією штрих-коду та статусом оплати.
   - Адаптивність діалогових вікон для екранів мобільних пристроїв (до 380px).
3. Фіксація та публікація змін:
   ```bash
   git add css/components/hall.css css/components/modal.css css/variables.css index.html
   git commit -m "feat(ui): стилізовано вигнутий екран залу, категорії сидінь та цифровий квиток"
   git push -u origin feature/lyosha-hall-ui
   ```
4. Оформлення Pull Request у гілку `develop` та підтвердження у Тімліда.

---

### 2.4. Студент 4: Ілля (QA Engineer & Tech Writer)

Мета:
Забезпечення якості програмного продукту, розробка та виконання набору автотестів, документування результатів тестування.

Порядок виконання:
1. Створення робочої гілки:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/illia-qa-tests
   ```
2. Реалізація функціоналу:
   - Розробка та перевірка тестових сценаріїв у `tests.html` та `scripts/run_tests.mjs`:
     - Розрахунок цін з промокодом `CINE20` при виборі 3 місць.
     - Обов'язкова валідація номера телефону та контактного імені.
     - Захист від повторного бронювання (Double-Booking Prevention).
     - Звільнення місць після скасування бронювання.
   - Документування результатів у `docs/TESTING.md` та оформлення офіційного звіту `docs/QA_Report_Illia.docx`.
3. Фіксація та публікація змін:
   ```bash
   git add tests.html docs/TESTING.md docs/QA_Report_Illia.docx
   git commit -m "test(qa): додано нові автотести розрахунку промокодів та оформлено звіт тестування"
   git push -u origin feature/illia-qa-tests
   ```
4. Оформлення Pull Request у гілку `develop` та злиття після перевірки.

---

## 3. Чек-ліст готовності до здачі роботи

- Всі 4 гілки `feature/*` злито в `develop` через схвалені Pull Requests.
- У репозиторії зафіксовано демонстрацію змодельованого та розв'язаного Merge Conflict.
- Автоматизовані тести проходять на 100% (0 помилок).
- Виконано злиття `develop` у `main`, створено версійний тег `v1.0.0`.
- Офіційний звіт тестування `docs/QA_Report_Illia.docx` підготовлено для подання.
