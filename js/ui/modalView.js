/**
 * CineSphere — Modal Views
 * Manages Checkout dialog, Digital Ticket Voucher, and My Bookings modal.
 */

import { db } from '../db.js';
import { state } from '../state.js';
import { booking } from '../booking.js';
import { pricing } from '../pricing.js';

export const modalView = {
  init(app) {
    this.app = app;
    this.checkoutModal = document.getElementById('checkout-modal');
    this.voucherModal = document.getElementById('voucher-modal');
    this.bookingsModal = document.getElementById('bookings-modal');
    this.hallsModal = document.getElementById('halls-modal');

    this.bindEvents();
  },

  bindEvents() {
    // Modal backdrop click to close
    document.querySelectorAll('.modal-backdrop').forEach(modal => {
      modal.addEventListener('click', (e) => {
        if (e.target === modal) {
          this.closeAll();
        }
      });
    });

    // Close buttons
    document.querySelectorAll('.modal-close-btn, .btn-modal-cancel').forEach(btn => {
      btn.addEventListener('click', () => this.closeAll());
    });

    // Checkout Form
    const checkoutForm = document.getElementById('checkout-form');
    if (checkoutForm) {
      checkoutForm.addEventListener('submit', (e) => {
        e.preventDefault();
        this.submitCheckout();
      });
    }

    // Apply Promo Button
    const applyPromoBtn = document.getElementById('btn-apply-promo');
    if (applyPromoBtn) {
      applyPromoBtn.addEventListener('click', () => {
        this.applyPromoCode();
      });
    }

    // Print Ticket Button
    const printBtn = document.getElementById('btn-print-voucher');
    if (printBtn) {
      printBtn.addEventListener('click', () => {
        window.print();
      });
    }
  },

  closeAll() {
    document.querySelectorAll('.modal-backdrop').forEach(m => m.classList.remove('active'));
  },

  /**
   * Open checkout modal and populate current order breakdown
   */
  openCheckout() {
    const { selectedShowtimeId, selectedSeats } = state.getState();
    if (!selectedShowtimeId || selectedSeats.length === 0) return;

    const showtime = db.getShowtimeById(selectedShowtimeId);
    const movie = db.getMovieById(showtime.movieId);
    const hall = db.getHallById(showtime.hallId);

    // Update details in modal
    const movieTitleEl = document.getElementById('checkout-movie-title');
    const hallMetaEl = document.getElementById('checkout-hall-meta');
    const seatsListEl = document.getElementById('checkout-seats-summary');
    const subtotalEl = document.getElementById('checkout-subtotal');
    const discountEl = document.getElementById('checkout-discount');
    const totalEl = document.getElementById('checkout-total');

    if (movieTitleEl && movie) movieTitleEl.textContent = movie.title;
    if (hallMetaEl && hall) {
      hallMetaEl.textContent = `${hall.name} • ${showtime.date} о ${showtime.time} (${showtime.format})`;
    }

    const subtotal = selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);

    if (seatsListEl) {
      seatsListEl.innerHTML = selectedSeats.map(s => {
        const rowLetter = String.fromCharCode(64 + s.row);
        return `<div>Ряд ${rowLetter}, Місце ${s.seat} (${s.tier}) — ${s.price} грн</div>`;
      }).join('');
    }

    if (subtotalEl) subtotalEl.textContent = `${subtotal} грн`;
    if (discountEl) discountEl.textContent = `0 грн`;
    if (totalEl) totalEl.textContent = `${subtotal} грн`;

    state.setState({ promoCode: null });
    const promoInput = document.getElementById('promo-code-input');
    if (promoInput) promoInput.value = '';

    if (this.checkoutModal) {
      this.checkoutModal.classList.add('active');
    }
  },

  /**
   * Verify promo code and update price in checkout modal
   */
  applyPromoCode() {
    const promoInput = document.getElementById('promo-code-input');
    const promoCode = promoInput ? promoInput.value.trim() : '';
    const { selectedSeats } = state.getState();
    const subtotal = selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);

    const promoResult = pricing.applyPromo(promoCode, subtotal);
    const discountEl = document.getElementById('checkout-discount');
    const totalEl = document.getElementById('checkout-total');

    if (promoResult.valid) {
      state.setState({ promoCode: promoResult.code });
      if (discountEl) discountEl.textContent = `-${promoResult.discountAmount} грн (${promoResult.label})`;
      if (totalEl) totalEl.textContent = `${promoResult.total} грн`;
      if (this.app) this.app.showToast(`Промокод успішно застосовано: ${promoResult.label}`, 'success');
    } else {
      state.setState({ promoCode: null });
      if (discountEl) discountEl.textContent = `0 грн`;
      if (totalEl) totalEl.textContent = `${subtotal} грн`;
      if (this.app) this.app.showToast('Невірний промокод. Спробуйте STUDENT10 або CINE20.', 'error');
    }
  },

  /**
   * Submit booking form
   */
  submitCheckout() {
    const fullName = document.getElementById('customer-name').value.trim();
    const email = document.getElementById('customer-email').value.trim();
    const phone = document.getElementById('customer-phone').value.trim();
    const { selectedShowtimeId, selectedSeats, promoCode } = state.getState();

    try {
      const result = booking.createReservation({
        showtimeId: selectedShowtimeId,
        seats: selectedSeats,
        customer: { fullName, email, phone },
        promoCode
      });

      if (result.success && result.booking) {
        state.clearSeats();
        this.closeAll();
        if (this.app) {
          this.app.showToast(`Бронювання ${result.booking.id} успішно підтверджено!`, 'success');
          this.app.updateBookingsBadge();
        }
        this.openVoucher(result.booking);
      }
    } catch (err) {
      if (this.app) {
        this.app.showToast(err.message, 'error');
      } else {
        alert(err.message);
      }
    }
  },

  /**
   * Open Digital Ticket Voucher modal
   */
  openVoucher(b) {
    if (!this.voucherModal || !b) return;

    document.getElementById('voucher-code').textContent = b.id;
    document.getElementById('voucher-movie-title').textContent = b.movieTitle;
    document.getElementById('voucher-hall-info').textContent = `${b.hallName} • ${b.showtimeDate} о ${b.showtimeTime}`;
    document.getElementById('voucher-customer-name').textContent = b.customer.fullName;
    document.getElementById('voucher-total-paid').textContent = `${b.pricing.totalAmount} грн`;

    const seatsListEl = document.getElementById('voucher-seats-list');
    if (seatsListEl) {
      seatsListEl.innerHTML = b.seats.map(s => {
        const rowLetter = String.fromCharCode(64 + s.row);
        return `<span class="badge badge-cyan">Ряд ${rowLetter}, Місце ${s.seat}</span>`;
      }).join(' ');
    }

    this.voucherModal.classList.add('active');
  },

  /**
   * Open "My Bookings" list modal
   */
  openBookingsList() {
    if (!this.bookingsModal) return;

    const listContainer = document.getElementById('my-bookings-list');
    const bookings = db.getBookings();

    if (listContainer) {
      if (bookings.length === 0) {
        listContainer.innerHTML = `
          <div style="text-align: center; padding: 2rem 0; color: var(--color-text-muted);">
            <p style="font-size: 1.1rem; margin-bottom: 0.5rem;">У вас ще немає активних бронювань 🎟</p>
            <p>Оберіть сеанс на головній сторінці, щоб забронювати квитки.</p>
          </div>
        `;
      } else {
        listContainer.innerHTML = bookings.map(b => {
          const seatsStr = b.seats.map(s => {
            const rowLetter = String.fromCharCode(64 + s.row);
            return `Р${rowLetter}-М${s.seat}`;
          }).join(', ');

          return `
            <div class="booking-item-card" data-booking-id="${b.id}">
              <div class="booking-item-details">
                <div style="display: flex; align-items: center; gap: 8px;">
                  <strong style="color: var(--color-cyan); font-family: monospace;">${b.id}</strong>
                  <span class="badge badge-emerald">Оплачено</span>
                </div>
                <h4 style="margin: 4px 0; font-size: 1rem;">${b.movieTitle}</h4>
                <div style="font-size: 12px; color: var(--color-text-secondary);">
                  ${b.hallName} • ${b.showtimeDate} о ${b.showtimeTime}
                </div>
                <div style="font-size: 12px; color: var(--color-text-muted); margin-top: 2px;">
                  Місця: <strong>${seatsStr}</strong> • Разом: <strong>${b.pricing.totalAmount} грн</strong>
                </div>
              </div>
              <div>
                <button class="btn btn-danger btn-sm btn-cancel-booking" data-booking-id="${b.id}">
                  Скасувати
                </button>
              </div>
            </div>
          `;
        }).join('');

        // Attach cancel button listeners
        listContainer.querySelectorAll('.btn-cancel-booking').forEach(btn => {
          btn.addEventListener('click', () => {
            const bookingId = btn.dataset.bookingId;
            if (confirm(`Ви дійсно бажаєте скасувати бронювання ${bookingId}? Місця будуть повернені у вільний продаж.`)) {
              db.cancelBooking(bookingId);
              if (this.app) {
                this.app.showToast(`Бронювання ${bookingId} скасовано. Місця звільнено.`, 'info');
                this.app.updateBookingsBadge();
                // If in hall view, refresh hall seat grid
                if (state.getState().currentView === 'hall' && this.app.hallView) {
                  this.app.hallView.render();
                }
              }
              this.openBookingsList();
            }
          });
        });
      }
    }

    this.bookingsModal.classList.add('active');
  },

  /**
   * Open Cinema Halls overview modal
   */
  openHallsModal() {
    if (!this.hallsModal) return;

    const listContainer = document.getElementById('halls-list-container');
    if (listContainer) {
      const halls = db.getHalls();
      const showtimes = db.getShowtimes();
      const movies = db.getMovies();

      listContainer.innerHTML = halls.map(hall => {
        const hallShowtimes = showtimes.filter(s => s.hallId === hall.id);
        const totalSeats = hall.totalRows * hall.seatsPerRow;
        const tierBadges = Object.keys(hall.seatTiers).map(t => {
          const label = t === 'couches' ? 'Дивани' : t.toUpperCase();
          const cls = t === 'vip' ? 'badge-amber' : (t === 'couches' ? 'badge-rose' : 'badge-cyan');
          return `<span class="badge ${cls}">${label} (${hall.seatTiers[t].basePrice} грн)</span>`;
        }).join(' ');

        const showtimesHtml = hallShowtimes.map(st => {
          const movie = movies.find(m => m.id === st.movieId);
          const movieTitle = movie ? movie.title : 'Фільм';
          return `
            <button class="btn btn-outline btn-sm btn-select-hall-showtime" data-showtime-id="${st.id}" style="text-align: left; justify-content: space-between; display: flex; width: 100%; margin-top: 6px;">
              <span><strong>${st.time}</strong> • ${movieTitle} (${st.format})</span>
              <span style="color: var(--color-cyan); font-weight: 700;">Обрати зал ➔</span>
            </button>
          `;
        }).join('');

        return `
          <div class="hall-info-card" style="background: var(--color-bg-base); border: 1px solid var(--color-border); border-radius: var(--radius-lg); padding: var(--spacing-4); margin-bottom: var(--spacing-4);">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
              <h4 style="font-size: 1.15rem; font-weight: 700; color: var(--color-text-primary); margin: 0;">🏛 ${hall.name}</h4>
              <span class="badge badge-cyan">${totalSeats} місць (${hall.totalRows} рядів по ${hall.seatsPerRow})</span>
            </div>
            <p style="font-size: 13px; color: var(--color-text-secondary); margin: 0 0 10px 0;">
              ${hall.id === 'hall-imax' ? 'Суперсучасний вигнутий екран IMAX Laser, проекція 4K та об’ємний звук Dolby Atmos.' : 'Затишний камерний зал із високою чіткістю зображення та багатоканальним звуком 7.1 Surround.'}
            </p>
            <div style="display: flex; gap: 6px; flex-wrap: wrap; margin-bottom: 12px;">
              ${tierBadges}
            </div>
            <div style="font-size: 12px; font-weight: 700; color: var(--color-text-muted); text-transform: uppercase; margin-bottom: 4px;">
              Найближчі сеанси в цьому залі:
            </div>
            <div class="hall-showtimes-list">
              ${showtimesHtml}
            </div>
          </div>
        `;
      }).join('');

      // Attach click listeners to select showtime and jump directly into hall scheme
      listContainer.querySelectorAll('.btn-select-hall-showtime').forEach(btn => {
        btn.addEventListener('click', () => {
          const showtimeId = btn.dataset.showtimeId;
          this.closeAll();
          state.clearSeats();
          state.setState({
            selectedShowtimeId: showtimeId,
            currentView: 'hall'
          });
        });
      });
    }

    this.hallsModal.classList.add('active');
  }
};
