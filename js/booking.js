/**
 * CineSphere — Booking & Reservation Module
 * 
 * 👤 Відповідальний розробник: Ярік (@YaroslavYanushevych)
 * 🌿 Робоча гілка: feature/yarik-booking-logic
 * 
 * Модуль транзакційного створення броні з валідацією обов'язкових полів,
 * перевіркою ліміту до 6 квитків та захистом від подвійного бронювання (Double-Booking Prevention).
 */

import { db } from './db.js';
import { pricing } from './pricing.js';

export const MAX_SEATS_PER_ORDER = 6;

/**
 * Генерація унікального номера квитка у форматі CS-XXXXX
 */
function generateBookingCode() {
  const existingBookings = db.getBookings ? db.getBookings() : [];
  const existingIds = new Set(existingBookings.map(b => b.id || b.bookingCode));
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
  MAX_SEATS_PER_ORDER,

  /**
   * Отримання повної матриці сидінь залу для конкретного сеансу
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
   * Створення нового бронювання з валідацією
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
      throw new Error(`Дозволено бронювати максимум ${MAX_SEATS_PER_ORDER} місць в одному замовленні.`);
    }

    const customerName = (customer.fullName || customer.name || '').trim();
    const customerPhone = (customer.phone || '').trim();
    const customerEmail = (customer.email || '').trim();

    if (!customerName) {
      throw new Error("Обов'язково вкажіть ім'я та прізвище замовника.");
    }

    if (!customerPhone) {
      throw new Error("Обов'язково вкажіть контактний номер телефону.");
    }

    const phoneDigits = customerPhone.replace(/\D/g, '');
    if (phoneDigits.length < 9) {
      throw new Error('Некоректний формат номера телефону (мінімум 9 цифр).');
    }

    // Перевірка на дублікати місць у межах одного замовлення
    const uniqueSeatIds = new Set(seats.map(s => s.id || `R${s.row}-S${s.seat || s.number}`));
    if (uniqueSeatIds.size !== seats.length) {
      throw new Error('Одне й те саме місце не можна обрати двічі в одному замовленні.');
    }

    // Захист від подвійного бронювання (Double-Booking Prevention)
    const occupiedSeatIds = db.getOccupiedSeatIds ? db.getOccupiedSeatIds(showtimeId) : [];
    const conflictingSeats = seats.filter(s => {
      const seatNum = s.seat || s.number;
      const seatId = s.id || `R${s.row}-S${seatNum}`;
      return occupiedSeatIds.includes(seatId);
    });

    if (conflictingSeats.length > 0) {
      const conflictList = conflictingSeats.map(s => s.id || `Ряд ${s.row}, Місце ${s.seat || s.number}`).join(', ');
      throw new Error(`Місце ${conflictList} вже зайняте. Будь ласка, оберіть інші місця.`);
    }

    const showtime = db.getShowtimeById ? db.getShowtimeById(showtimeId) : null;
    const movie = showtime && db.getMovieById ? db.getMovieById(showtime.movieId) : null;
    const hall = showtime && db.getHallById ? db.getHallById(showtime.hallId) : null;

    const orderPricing = pricing.calculateOrderTotal ? pricing.calculateOrderTotal({
      seats,
      hall,
      showtime,
      promoCode,
      isStudent
    }) : { subtotal: 0, discountAmount: 0, total: 0 };

    const bookingCode = generateBookingCode();
    const bookingRecord = {
      id: bookingCode,
      bookingCode,
      showtimeId,
      movieId: showtime ? showtime.movieId : (movie ? movie.id : ''),
      movieTitle: movie ? movie.title : 'Фільм',
      hallName: hall ? hall.name : 'Кінозал',
      showtimeDate: showtime ? showtime.date : '',
      showtimeTime: showtime ? showtime.time : '',
      seats: seats.map(s => {
        const seatNum = s.seat || s.number;
        return {
          id: s.id || `R${s.row}-S${seatNum}`,
          row: s.row,
          seat: seatNum,
          tier: s.tier || 'standard',
          price: s.price || (pricing.calculateSeatPrice ? pricing.calculateSeatPrice(s.tier || 'standard', hall, showtime) : 150)
        };
      }),
      customer: {
        fullName: customerName,
        name: customerName,
        email: customerEmail,
        phone: customerPhone
      },
      pricing: {
        subtotal: orderPricing.subtotal,
        discountAmount: orderPricing.discountAmount,
        promoCodeApplied: orderPricing.isPromoValid ? (promoCode || '').trim().toUpperCase() : null,
        promoLabel: orderPricing.promoLabel,
        totalAmount: orderPricing.total,
        total: orderPricing.total
      },
      status: 'confirmed',
      createdAt: new Date().toISOString()
    };

    if (db.saveBooking) {
      const saveRes = db.saveBooking(bookingRecord);
      if (saveRes && saveRes.success === false) {
        throw new Error(saveRes.error || 'Помилка при збереженні бронювання в базі даних.');
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
   * Скасування існуючого бронювання
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
  module.exports = { booking, MAX_SEATS_PER_ORDER };
}
