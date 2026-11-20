/**
 * CineSphere — Booking & Reservation Module
 * Handles seat matrix generation, double-booking prevention, and reservation transactions.
 */

import { db } from './db.js';
import { pricing } from './pricing.js';

/**
 * Generate a unique booking code, e.g. "CS-89241"
 */
function generateBookingCode() {
  const num = Math.floor(10000 + Math.random() * 90000);
  return `CS-${num}`;
}

/**
 * Determine seat tier based on hall tier definitions
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
   * Generates the seat grid for a specific showtime.
   * Supports both getSeatGrid(showtimeId) and getSeatGrid(movieId, showtimeStr)
   */
  getSeatGrid(showtimeIdOrMovieId, showtimeStr = null) {
    let showtime = null;

    if (showtimeStr) {
      // Look up by movieId and time
      const all = db.getShowtimes();
      showtime = all.find(s => s.movieId === showtimeIdOrMovieId && (s.time === showtimeStr || s.id === showtimeStr));
    }

    if (!showtime) {
      showtime = db.getShowtimeById(showtimeIdOrMovieId);
    }

    // Fallback: if not found by ID directly, find first showtime or default
    if (!showtime) {
      const all = db.getShowtimes();
      showtime = all[0];
    }

    if (!showtime) return [];

    const hall = db.getHallById(showtime.hallId) || db.getHalls()[0];
    if (!hall) return [];

    const occupiedSeatIds = db.getOccupiedSeatIds(showtime.id);
    const grid = [];

    const totalRows = hall.totalRows || 6;
    const seatsPerRow = hall.seatsPerRow || 8;

    for (let r = 1; r <= totalRows; r++) {
      for (let s = 1; s <= seatsPerRow; s++) {
        const seatId = `R${r}-S${s}`;
        const tier = resolveSeatTier(r, hall);
        const seatPrice = pricing.calculateSeatPrice(tier, hall, showtime);
        const isOccupied = occupiedSeatIds.includes(seatId);

        grid.push({
          id: seatId,
          row: r,
          seat: s,
          tier,
          basePrice: seatPrice,
          price: seatPrice,
          status: isOccupied ? 'occupied' : 'available'
        });
      }
    }

    return grid;
  },

  /**
   * Creates a confirmed reservation in LocalStorage.
   * Throws Error or returns failure if any seat is already booked (preventing double-booking).
   */
  createReservation(params = {}) {
    const { showtimeId, seats = [], customer = {}, promoCode = null } = params;

    // Validation
    if (!showtimeId) {
      throw new Error('Не вказано ідентифікатор сеансу (showtimeId).');
    }

    if (!Array.isArray(seats) || seats.length === 0) {
      throw new Error('Не обрано жодного місця для бронювання.');
    }

    if (seats.length > 6) {
      throw new Error('Неможливо забронювати більше 6 місць в одному замовленні.');
    }

    if (!customer.fullName || !customer.fullName.trim()) {
      throw new Error("Обов'язково вкажіть ім'я та прізвище замовника.");
    }

    if (!customer.phone || !customer.phone.trim()) {
      throw new Error("Обов'язково вкажіть контактний номер телефону.");
    }

    const showtime = db.getShowtimeById(showtimeId);
    if (!showtime) {
      throw new Error(`Сеанс "${showtimeId}" не знайдено.`);
    }

    const movie = db.getMovieById(showtime.movieId);
    const hall = db.getHallById(showtime.hallId);

    // Double-booking collision check
    const occupiedSeatIds = db.getOccupiedSeatIds(showtimeId);
    const conflictingSeats = seats.filter(s => occupiedSeatIds.includes(s.id || `R${s.row}-S${s.seat}`));

    if (conflictingSeats.length > 0) {
      const conflictList = conflictingSeats.map(s => s.id || `Ряд ${s.row}, Місце ${s.seat}`).join(', ');
      throw new Error(`Місце ${conflictList} вже заброньовано. Будь ласка, оберіть інші місця.`);
    }

    // Calculate final order financials
    const orderPricing = pricing.calculateOrderTotal({
      seats,
      hall,
      showtime,
      promoCode
    });

    const bookingRecord = {
      id: generateBookingCode(),
      showtimeId,
      movieId: showtime.movieId,
      movieTitle: movie ? movie.title : 'Фільм',
      hallName: hall ? hall.name : 'Кінозал',
      showtimeDate: showtime.date,
      showtimeTime: showtime.time,
      seats: seats.map(s => ({
        id: s.id || `R${s.row}-S${s.seat}`,
        row: s.row,
        seat: s.seat,
        tier: s.tier || 'standard',
        price: s.price || pricing.calculateSeatPrice(s.tier || 'standard', hall, showtime)
      })),
      customer: {
        fullName: customer.fullName.trim(),
        email: (customer.email || '').trim(),
        phone: customer.phone.trim()
      },
      pricing: {
        subtotal: orderPricing.subtotal,
        discountAmount: orderPricing.discountAmount,
        promoCodeApplied: orderPricing.isPromoValid ? promoCode : null,
        totalAmount: orderPricing.total
      },
      status: 'confirmed',
      createdAt: new Date().toISOString()
    };

    const saveResult = db.saveBooking(bookingRecord);
    if (!saveResult.success) {
      throw new Error(saveResult.error || 'Помилка при збереженні бронювання в базі даних.');
    }

    return {
      success: true,
      booking: bookingRecord
    };
  },

  /**
   * Alias for createReservation to satisfy createBooking API in some test cases
   */
  createBooking(showtimeId, seats, customer, promoCode) {
    if (typeof showtimeId === 'object' && showtimeId !== null) {
      return this.createReservation(showtimeId);
    }
    return this.createReservation({ showtimeId, seats, customer, promoCode });
  },

  /**
   * Cancel an existing reservation
   */
  cancelReservation(bookingId) {
    return db.cancelBooking(bookingId);
  },

  /**
   * Alias for cancelReservation
   */
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
