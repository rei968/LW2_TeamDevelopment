/**
 * CineSphere — Booking & Reservation Module
 * 
 * 👤 Відповідальний розробник: Ярік (@YaroslavYanushevych)
 * 🌿 Робоча гілка в Git: feature/yarik-booking-logic
 * 
 * 📌 Завдання для Яріка:
 * 1. generateBookingCode() — генерація унікального номера квитка у форматі CS-XXXXX (де X - цифри).
 * 2. getSeatGrid(showtimeId) — генерація сітки місць залу з категоріями та статусами зайнятості.
 * 3. createReservation(params) — валідація замовлення:
 *    - перевірка ліміту (максимум 6 місць на замовлення);
 *    - обов'язковість імені та контактного номеру телефону покупця;
 *    - захист від подвійного бронювання (Double-Booking Prevention);
 *    - збереження транзакції бронювання у LocalStorage через db.saveBooking.
 * 4. cancelReservation(bookingId) — скасування квитка та повернення місць у вільний статус.
 */

import { db } from './db.js';
import { pricing } from './pricing.js';

/**
 * Генерація унікального цифрового коду бронювання (наприклад, "CS-89241")
 * TODO (Ярік): Реалізувати генерацію випадкового 5-значного числа з префіксом CS-
 */
function generateBookingCode() {
  const num = Math.floor(10000 + Math.random() * 90000);
  return `CS-${num}`;
}

/**
 * Визначення категорії місця на основі конфігурації залу
 */
function resolveSeatTier(rowNumber, hall) {
  if (!hall || !hall.seatTiers) return 'standard';

  if (hall.seatTiers.couches && Array.isArray(hall.seatTiers.couches.rows) && hall.seatTiers.couches.rows.includes(rowNumber)) {
    return 'couches';
  }
  if (hall.seatTiers.vip && Array.isArray(hall.seatTiers.vip.rows) && hall.seatTiers.vip.rows.includes(rowNumber)) {
    return 'vip';
  }
  return 'standard';
}

export const booking = {
  /**
   * Отримання повної матриці сидінь залу для конкретного сеансу
   * TODO (Ярік): Згенерувати ряди та місця з актуальними статусами зайнятості
   */
  getSeatGrid(showtimeIdOrMovieId, showtimeStr = null) {
    let showtime = null;

    if (showtimeStr) {
      const all = db.getShowtimes();
      showtime = all.find(s => s.movieId === showtimeIdOrMovieId && (s.time === showtimeStr || s.id === showtimeStr));
    }

    if (!showtime) {
      showtime = db.getShowtimeById(showtimeIdOrMovieId);
    }

    if (!showtime) {
      const all = db.getShowtimes();
      showtime = all[0];
    }

    if (!showtime) return [];

    const hall = db.getHallById(showtime.hallId) || db.getHalls()[0];
    if (!hall) return [];

    const totalRows = hall.rows || hall.totalRows || 6;
    const seatsPerRow = hall.seatsPerRow || 8;
    const occupiedSeatIds = db.getOccupiedSeatIds ? db.getOccupiedSeatIds(showtime.id) : [];

    const grid = [];
    for (let r = 1; r <= totalRows; r++) {
      const rowSeats = [];
      const tier = resolveSeatTier(r, hall);

      for (let s = 1; s <= seatsPerRow; s++) {
        const seatId = `R${r}-S${s}`;
        const isOccupied = occupiedSeatIds.includes(seatId);

        rowSeats.push({
          id: seatId,
          row: r,
          seat: s,
          tier,
          status: isOccupied ? 'occupied' : 'available',
          price: pricing.calculateSeatPrice ? pricing.calculateSeatPrice(tier, hall, showtime) : 150
        });
      }
      grid.push(rowSeats);
    }

    return grid;
  },

  /**
   * Створення нового бронювання
   * TODO (Ярік): Реалізувати перевірку ліміту (до 6 місць), валідацію замовника,
   * захист від double-booking та збереження через db.saveBooking.
   */
  createReservation(params = {}) {
    const { showtimeId, seats = [], customer = {}, promoCode = null } = params;

    // TODO (Ярік): Додати перевірку на порожній масив місць
    // TODO (Ярік): Додати ліміт seats.length <= 6
    // TODO (Ярік): Додати валідацію customer.fullName та customer.phone
    // TODO (Ярік): Додати перевірку db.getOccupiedSeatIds на зайнятість місць
    // TODO (Ярік): Сформувати об'єкт bookingRecord та зберегти через db.saveBooking(bookingRecord)

    const bookingCode = generateBookingCode();
    const bookingRecord = {
      id: bookingCode,
      bookingCode,
      showtimeId,
      seats,
      customer,
      pricing: { total: 0 }
    };

    if (db.saveBooking) {
      db.saveBooking(bookingRecord);
    }

    return {
      success: true,
      booking: bookingRecord
    };
  },

  createBooking(showtimeId, seats, customer, promoCode) {
    if (typeof showtimeId === 'object' && showtimeId !== null) {
      return this.createReservation(showtimeId);
    }
    return this.createReservation({ showtimeId, seats, customer, promoCode });
  },

  /**
   * Скасування існуючого бронювання
   * TODO (Ярік): Вивільнення місць у базі даних
   */
  cancelReservation(bookingId) {
    if (db.cancelBooking) {
      return db.cancelBooking(bookingId);
    }
    return { success: true };
  },

  cancelBooking(bookingId) {
    return this.cancelReservation(bookingId);
  }
};

if (typeof window !== 'undefined') {
  window.CineSphereBooking = booking;
}

if (typeof module !== 'undefined' && module.exports) {
  module.exports = { booking };
}
