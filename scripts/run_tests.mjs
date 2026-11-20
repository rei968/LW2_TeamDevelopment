import { db } from '../js/db.js';
import { pricing, calculateTicketPrice, calculateSeatPrice, calculateOrderTotal, applyPromo } from '../js/pricing.js';
import { booking } from '../js/booking.js';

// Setup Mock LocalStorage
const storage = new Map();
globalThis.localStorage = {
  getItem: (k) => storage.get(k) || null,
  setItem: (k, v) => storage.set(k, String(v)),
  removeItem: (k) => storage.delete(k),
  clear: () => storage.clear()
};

// Minimal Test Runner matching tests.html
class TestRunner {
  constructor() {
    this.total = 0;
    this.passed = 0;
    this.failed = 0;
    this.suites = [];
  }

  describe(title, fn) {
    const suite = { title, tests: [] };
    this.suites.push(suite);
    this.currentSuite = suite;
    fn();
    this.currentSuite = null;
  }

  it(title, fn) {
    this.currentSuite.tests.push({ title, fn });
  }

  async run() {
    console.log("=== CineSphere Automated QA Test Suite Run ===");
    for (const suite of this.suites) {
      console.log(`\n📦 ${suite.title}`);
      for (const t of suite.tests) {
        this.total++;
        try {
          await t.fn();
          this.passed++;
          console.log(`  ✓ [PASS] ${t.title}`);
        } catch (err) {
          this.failed++;
          console.error(`  ✕ [FAIL] ${t.title}`);
          console.error(`     Error: ${err.message}`);
        }
      }
    }
    console.log(`\n========================================`);
    console.log(`Total: ${this.total} | Passed: ${this.passed} | Failed: ${this.failed}`);
    console.log(`Status: ${this.failed === 0 ? 'ALL TESTS PASSED ✅' : 'FAILURES DETECTED ❌'}`);
    return this.failed;
  }
}

function expect(actual) {
  return {
    toBe(expected) {
      if (actual !== expected) throw new Error(`Expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
    },
    toEqual(expected) {
      const a = JSON.stringify(actual);
      const b = JSON.stringify(expected);
      if (a !== b) throw new Error(`Expected ${b}, got ${a}`);
    },
    toBeGreaterThan(expected) {
      if (!(actual > expected)) throw new Error(`Expected > ${expected}, got ${actual}`);
    },
    toBeLessThanOrEqual(expected) {
      if (!(actual <= expected)) throw new Error(`Expected <= ${expected}, got ${actual}`);
    },
    toBeTruthy() {
      if (!actual) throw new Error(`Expected truthy, got ${actual}`);
    },
    toBeFalsy() {
      if (actual) throw new Error(`Expected falsy, got ${actual}`);
    },
    toThrow(expectedText) {
      if (typeof actual !== 'function') throw new Error('expect(fn).toThrow() requires a function');
      let threw = false;
      let thrownError = null;
      try {
        actual();
      } catch (e) {
        threw = true;
        thrownError = e;
      }
      if (!threw) throw new Error('Expected exception, but call succeeded');
      if (expectedText && !thrownError.message.includes(expectedText)) {
        throw new Error(`Exception occurred, but message "${thrownError.message}" does not include "${expectedText}"`);
      }
    }
  };
}

const runner = new TestRunner();

// Suite 1
runner.describe('1. Модуль ціноутворення (js/pricing.js)', () => {
  runner.it('Базова ціна для стандартного місця повертається без націнки', () => {
    const price = calculateTicketPrice(150, 'standard', false);
    expect(price).toBe(150);
  });

  runner.it('Націнка на VIP-місце становить +50 грн', () => {
    const standard = calculateTicketPrice(150, 'standard', false);
    const vip = calculateTicketPrice(150, 'vip', false);
    expect(vip).toBe(standard + 50);
  });

  runner.it('Націнка на категорію "Дивани" становить +100 грн', () => {
    const standard = calculateTicketPrice(150, 'standard', false);
    const couches = calculateTicketPrice(150, 'couches', false);
    expect(couches).toBe(standard + 100);
  });

  runner.it('Студентська знижка (15%) коректно зменшує вартість квитка', () => {
    const student = calculateTicketPrice(200, 'standard', true);
    expect(student).toBe(170);
  });

  runner.it('Промокод STUDENT10 надає 10% знижки на суму замовлення', () => {
    const res = applyPromo('STUDENT10', 400);
    expect(res.valid).toBe(true);
    expect(res.discountAmount).toBe(40);
    expect(res.total).toBe(360);
  });

  runner.it('Промокод CINE20 надає 20% знижки на суму замовлення', () => {
    const res = applyPromo('CINE20', 500);
    expect(res.valid).toBe(true);
    expect(res.discountAmount).toBe(100);
    expect(res.total).toBe(400);
  });

  runner.it('Недійсний промокод не застосовує знижку і зберігає початкову суму', () => {
    const res = applyPromo('UNKNOWN999', 300);
    expect(res.valid).toBe(false);
    expect(res.discountAmount).toBe(0);
    expect(res.total).toBe(300);
  });

  runner.it('calculateOrderTotal підсумовує ціни кількох обраних місць', () => {
    const seats = [
      { id: 'R1-S1', tier: 'standard', price: 150 },
      { id: 'R1-S2', tier: 'standard', price: 200 }
    ];
    const summary = calculateOrderTotal({ seats });
    expect(summary.subtotal).toBe(350);
    expect(summary.total).toBe(350);
  });
});

// Suite 2
runner.describe('2. Модуль бронювання (js/booking.js)', () => {
  runner.it('Ліміт вибору місць: заборонено бронювати більше 6 місць за одне замовлення', () => {
    const showtimes = db.getShowtimes();
    const showtimeId = showtimes[0].id;
    const seats = [
      { id: 'R1-S1', row: 1, number: 1, tier: 'standard' },
      { id: 'R1-S2', row: 1, number: 2, tier: 'standard' },
      { id: 'R1-S3', row: 1, number: 3, tier: 'standard' },
      { id: 'R1-S4', row: 1, number: 4, tier: 'standard' },
      { id: 'R1-S5', row: 1, number: 5, tier: 'standard' },
      { id: 'R1-S6', row: 1, number: 6, tier: 'standard' },
      { id: 'R1-S7', row: 1, number: 7, tier: 'standard' }
    ];

    expect(() => {
      booking.createReservation({
        showtimeId,
        seats,
        customer: { name: 'Тестовий Клієнт', phone: '+380501112233' }
      });
    }).toThrow('максимум 6 місць');
  });

  runner.it('Валідація обов’язкових полів клієнта (ім’я та телефон)', () => {
    const showtimeId = db.getShowtimes()[0].id;
    const seats = [{ id: 'R1-S1', row: 1, number: 1, tier: 'standard' }];

    expect(() => {
      booking.createReservation({
        showtimeId,
        seats,
        customer: { name: '', phone: '' }
      });
    }).toThrow("ім'я");
  });

  runner.it('Генерація коду бронювання у форматі CS-XXXXX', () => {
    const showtimeId = db.getShowtimes()[0].id;
    const uniqueSeatNum = Math.floor(Math.random() * 8) + 1;
    const seats = [{ id: `R5-S${uniqueSeatNum}`, row: 5, number: uniqueSeatNum, tier: 'standard', price: 150 }];
    
    const result = booking.createReservation({
      showtimeId,
      seats,
      customer: { name: 'Ілля QA', phone: '+380509998877' }
    });

    expect(result.success).toBe(true);
    expect(result.booking.bookingCode.startsWith('CS-')).toBe(true);
    expect(result.booking.bookingCode.length).toBe(8);
  });

  runner.it('Захист від подвійного бронювання (Double-Booking Prevention)', () => {
    const showtimeId = db.getShowtimes()[0].id;
    const seatId = 'R2-S3';
    const seats = [{ id: seatId, row: 2, number: 3, tier: 'standard', price: 150 }];

    booking.createReservation({
      showtimeId,
      seats,
      customer: { name: 'Перший Клієнт', phone: '+380501234567' }
    });

    expect(() => {
      booking.createReservation({
        showtimeId,
        seats,
        customer: { name: 'Другий Клієнт', phone: '+380507654321' }
      });
    }).toThrow('вже зайняте');
  });

  runner.it('Скасування бронювання вивільняє місця для повторного вибору', () => {
    const showtimeId = db.getShowtimes()[0].id;
    const seatId = 'R3-S4';
    const seats = [{ id: seatId, row: 3, number: 4, tier: 'standard', price: 150 }];

    const res = booking.createReservation({
      showtimeId,
      seats,
      customer: { name: 'Клієнт на скасування', phone: '+380500000000' }
    });

    const cancelRes = booking.cancelReservation(res.booking.id);
    expect(cancelRes.success).toBe(true);

    const rebookRes = booking.createReservation({
      showtimeId,
      seats,
      customer: { name: 'Новий Клієнт', phone: '+380501111111' }
    });
    expect(rebookRes.success).toBe(true);
  });
});

// Suite 3
runner.describe('3. Сховище даних LocalStorage (js/db.js)', () => {
  runner.it('Початковий сідер створює каталог фільмів у сховищі', () => {
    const movies = db.getMovies();
    expect(Array.isArray(movies)).toBe(true);
    expect(movies.length).toBeGreaterThan(0);
  });

  runner.it('Кожен фільм має обов’язкові атрибути (id, title, genre, duration)', () => {
    const movies = db.getMovies();
    const first = movies[0];
    expect(typeof first.id).toBe('string');
    expect(typeof first.title).toBe('string');
    expect(typeof first.genre).toBe('string');
    expect(first.duration).toBeGreaterThan(0);
  });

  runner.it('Пошук фільму за ідентифікатором знаходить правильний об’єкт', () => {
    const movies = db.getMovies();
    const target = movies[0];
    const found = db.getMovieById(target.id);
    expect(found.title).toBe(target.title);
  });

  runner.it('База даних надає схеми кінозалів із налаштованими категоріями місць', () => {
    const halls = db.getHalls();
    expect(halls.length).toBeGreaterThan(0);
    const hall = halls[0];
    expect(hall.rows).toBeGreaterThan(0);
    expect(hall.seatsPerRow).toBeGreaterThan(0);
    expect(Boolean(hall.seatTiers)).toBe(true);
  });

  runner.it('Історія бронювань оновлюється при створенні нового квитка', () => {
    const initialCount = db.getBookings().length;
    const showtimeId = db.getShowtimes()[0].id;
    const seatNum = Math.floor(Math.random() * 8) + 1;
    const seats = [{ id: `R7-S${seatNum}`, row: 7, number: seatNum, tier: 'standard', price: 150 }];

    booking.createReservation({
      showtimeId,
      seats,
      customer: { name: 'Історія QA', phone: '+380502223344' }
    });

    const newCount = db.getBookings().length;
    expect(newCount).toBe(initialCount + 1);
  });
});

// Suite 4
runner.describe('4. Регресійні тести цінових правил (Merge Conflict Target)', () => {
  runner.it('Розрахунок вартості для замовлення з VIP-місцем та промокодом працює коректно', () => {
    const seats = [
      { id: 'R1-VIP', tier: 'vip', price: 200 }
    ];
    const summary = calculateOrderTotal({
      seats,
      promoCode: 'STUDENT10'
    });
    expect(summary.subtotal).toBe(200);
    expect(summary.discountAmount).toBe(20);
    expect(summary.total).toBe(180);
  });

  runner.it('Групова знижка 15% застосовується автоматично для замовлень від 3 квитків (правило Же)', () => {
    const seats = [
      { id: 'R1-S1', tier: 'standard', price: 100 },
      { id: 'R1-S2', tier: 'standard', price: 100 },
      { id: 'R1-S3', tier: 'standard', price: 100 }
    ];
    const summary = calculateOrderTotal({ seats });
    expect(summary.subtotal).toBe(300);
    expect(summary.discountAmount).toBe(45);
    expect(summary.total).toBe(255);
  });

  runner.it('VIP-знижка 10% застосовується на замовлення з VIP-місцем без промокоду (правило Яріка)', () => {
    const seats = [
      { id: 'R1-VIP', tier: 'vip', price: 200 }
    ];
    const summary = calculateOrderTotal({ seats });
    expect(summary.subtotal).toBe(200);
    expect(summary.discountAmount).toBe(20);
    expect(summary.total).toBe(180);
  });
});

const failed = await runner.run();
process.exit(failed > 0 ? 1 : 0);
