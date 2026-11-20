/**
 * CineSphere — Booking & Reservation Module
 *
 * 👤 Відповідальний розробник: Ярік (@YaroslavYanushevych)
 * 🌿 Робоча гілка в Git: feature/yarik-booking-logic
 */

import { db } from './db.js';
import { pricing } from './pricing.js';

const MAX_SEATS_PER_ORDER = 6;

/**
 * Генерація унікального цифрового коду бронювання у форматі CS-XXXXX
 */
function generateBookingCode() {
  const existingIds = new Set((db.getBookings ? db.getBookings() : []).map(b => b.id));
  let code;
  do {
    const num = Math.floor(10000 + Math.random() * 90000);
    code = `CS-${num}`;
  } while (existingIds.has(code));
  return code;
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
   * Отримання списку всіх місць залу для конкретного сеансу з актуальними статусами зайнятості
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

    const seats = [];
    for (let r = 1; r <= totalRows; r++) {
      const tier = resolveSeatTier(r, hall);

      for (let s = 1; s <= seatsPerRow; s++) {
        const seatId = `R${r}-S${s}`;
        const isOccupied = occupiedSeatIds.includes(seatId);

        seats.push({
          id: seatId,
          row: r,
          seat: s,
          tier,
          status: isOccupied ? 'occupied' : 'available',
          price: pricing.calculateSeatPrice ? pricing.calculateSeatPrice(tier, hall, showtime) : 150
        });
      }
    }

    return seats;
  },

  /**
   * Створення нового бронювання з валідацією ліміту, даних покупця та захистом від подвійного бронювання
   */
  createReservation(params = {}) {
    const { showtimeId, seats = [], customer = {}, promoCode = null, isStudent = false } = params;

    if (!showtimeId) {
      throw new Error('Не вказано сеанс для бронювання.');
    }

    if (!Array.isArray(seats) || seats.length === 0) {
      throw new Error('Оберіть хоча б одне місце для бронювання.');
    }

    if (seats.length > MAX_SEATS_PER_ORDER) {
      throw new Error(`Максимальна кількість місць на одне замовлення — ${MAX_SEATS_PER_ORDER}.`);
    }

    const fullName = (customer.fullName || '').trim();
    const phone = (customer.phone || '').trim();
    const email = (customer.email || '').trim();

    if (!fullName) {
      throw new Error("Будь ласка, введіть ім'я та прізвище покупця.");
    }

    if (!phone) {
      throw new Error('Будь ласка, введіть контактний номер телефону.');
    }

    const phoneDigits = phone.replace(/\D/g, '');
    if (phoneDigits.length < 9) {
      throw new Error('Некоректний формат номера телефону (мінімум 9 цифр).');
    }

    // Перевірка на дублікати місць у межах одного замовлення
    const uniqueSeatIds = new Set(seats.map(s => s.id));
    if (uniqueSeatIds.size !== seats.length) {
      throw new Error('Одне й те саме місце не можна обрати двічі в одному замовленні.');
    }

    // Захист від подвійного бронювання (Double-Booking Prevention)
    const occupiedSeatIds = db.getOccupiedSeatIds ? db.getOccupiedSeatIds(showtimeId) : [];
    const conflictingSeat = seats.find(s => occupiedSeatIds.includes(s.id));
    if (conflictingSeat) {
      throw new Error(`Місце ${conflictingSeat.id} вже заброньовано іншим користувачем.`);
    }

    const showtime = db.getShowtimeById ? db.getShowtimeById(showtimeId) : null;
    const movie = showtime && db.getMovieById ? db.getMovieById(showtime.movieId) : null;
    const hall = showtime && db.getHallById ? db.getHallById(showtime.hallId) : null;

    const orderPricing = pricing.calculateOrderTotal({
      seats,
      hall,
      showtime,
      promoCode,
      isStudent
    });

    const bookingCode = generateBookingCode();
    const bookingRecord = {
      id: bookingCode,
      bookingCode,
      showtimeId,
      movieId: movie ? movie.id : null,
      movieTitle: movie ? movie.title : '',
      hallName: hall ? hall.name : '',
      showtimeDate: showtime ? showtime.date : '',
      showtimeTime: showtime ? showtime.time : '',
      seats: seats.map(s => ({ ...s })),
      customer: { fullName, email, phone },
      pricing: {
        subtotal: orderPricing.subtotal,
        discountAmount: orderPricing.discountAmount,
        promoCodeApplied: orderPricing.isPromoValid ? (promoCode || '').trim().toUpperCase() : null,
        totalAmount: orderPricing.total,
        total: orderPricing.total
      },
      status: 'confirmed',
      createdAt: new Date().toISOString()
    };

    if (db.saveBooking) {
      const saveResult = db.saveBooking(bookingRecord);
      if (saveResult && !saveResult.success) {
        throw new Error(saveResult.error || 'Не вдалося зберегти бронювання.');
      }
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
   * Скасування існуючого бронювання та повернення місць у статус available
   */
  cancelReservation(bookingId) {
    if (!bookingId) {
      return { success: false, error: 'Не вказано код бронювання.' };
    }
    if (db.cancelBooking) {
      return db.cancelBooking(bookingId);
    }
    return { success: true };
  },

  cancelBooking(bookingId) {
    return this.cancelReservation(bookingId);
  },

  /**
   * Повертає зведену інформацію про доступність місць на конкретний сеанс
   */
  getShowtimeAvailability(showtimeId) {
    const seats = this.getSeatGrid(showtimeId);
    const totalSeats = seats.length;
    const occupiedSeats = seats.filter(s => s.status === 'occupied').length;
    return {
      totalSeats,
      occupiedSeats,
      availableSeats: totalSeats - occupiedSeats
    };
  }
};

if (typeof window !== 'undefined') {
  window.CineSphereBooking = booking;
}

if (typeof module !== 'undefined' && module.exports) {
  module.exports = { booking };
}
