/**
 * CineSphere — Cinema Hall View
 * Renders the curved screen, interactive seat grid, legend, and live selection summary.
 */

import { db } from '../db.js';
import { state } from '../state.js';
import { booking } from '../booking.js';
import { pricing } from '../pricing.js';

export const hallView = {
  init(app) {
    this.app = app;
    this.container = document.getElementById('hall-view');
    this.backBtn = document.getElementById('btn-back-to-catalog');
    this.checkoutBtn = document.getElementById('btn-checkout');

    if (this.backBtn) {
      this.backBtn.addEventListener('click', () => {
        state.clearSeats();
        state.setState({ currentView: 'catalog' });
      });
    }

    if (this.checkoutBtn) {
      this.checkoutBtn.addEventListener('click', () => {
        const { selectedSeats } = state.getState();
        if (selectedSeats.length === 0) {
          if (this.app) this.app.showToast('Будь ласка, оберіть хоча б одне місце на схемі залу.', 'warning');
          return;
        }
        if (this.app && this.app.modalView) {
          this.app.modalView.openCheckout();
        }
      });
    }

    // Subscribe to seat selection changes to update summary dynamically
    state.subscribe('seatSelected', () => this.updateSummary());
    state.subscribe('seatDeselected', () => this.updateSummary());
    state.subscribe('seatsCleared', () => this.updateSummary());
  },

  /**
   * Render complete hall screen for the active showtime
   */
  render() {
    if (!this.container) return;

    const { selectedShowtimeId } = state.getState();
    if (!selectedShowtimeId) return;

    const showtime = db.getShowtimeById(selectedShowtimeId);
    if (!showtime) return;

    const movie = db.getMovieById(showtime.movieId);
    const hall = db.getHallById(showtime.hallId);

    // Update Hall Header Meta
    const titleEl = document.getElementById('hall-movie-title');
    const metaEl = document.getElementById('hall-session-meta');

    if (titleEl && movie) {
      titleEl.textContent = movie.title;
    }
    if (metaEl && hall) {
      metaEl.innerHTML = `
        <strong>${hall.name}</strong>
        <span>•</span>
        <span>${showtime.format}</span>
        <span>•</span>
        <span>${showtime.date}</span>
        <span>•</span>
        <span>Початок о <strong>${showtime.time}</strong></span>
      `;
    }

    // Generate and render seating grid
    const gridContainer = document.getElementById('seat-grid');
    if (!gridContainer) return;

    const seats = booking.getSeatGrid(selectedShowtimeId);
    const { selectedSeats } = state.getState();
    const selectedIds = selectedSeats.map(s => s.id);

    // Group seats by row
    const rowMap = new Map();
    seats.forEach(s => {
      if (!rowMap.has(s.row)) rowMap.set(s.row, []);
      rowMap.get(s.row).push(s);
    });

    let gridHtml = '';
    rowMap.forEach((rowSeats, rowNumber) => {
      const rowLetter = String.fromCharCode(64 + rowNumber); // 1->A, 2->B, etc.
      const seatsHtml = rowSeats.map(s => {
        const isSelected = selectedIds.includes(s.id);
        const isOccupied = s.status === 'occupied';

        const classes = [
          'seat-btn',
          `tier-${s.tier}`,
          isSelected ? 'selected' : '',
          isOccupied ? 'occupied' : ''
        ].filter(Boolean).join(' ');

        return `
          <button 
            type="button"
            class="${classes}"
            data-seat-id="${s.id}"
            data-row="${s.row}"
            data-seat="${s.seat}"
            data-tier="${s.tier}"
            data-price="${s.price}"
            title="Ряд ${rowLetter} (${s.row}), Місце ${s.seat} • ${s.price} грн (${s.tier.toUpperCase()})"
            ${isOccupied ? 'disabled' : ''}
          >
            ${s.seat}
          </button>
        `;
      }).join('');

      gridHtml += `
        <div class="seat-row" data-row="${rowNumber}">
          <span class="row-label">${rowLetter}</span>
          <div class="seats-container">
            ${seatsHtml}
          </div>
          <span class="row-label">${rowLetter}</span>
        </div>
      `;
    });

    gridContainer.innerHTML = gridHtml;

    // Attach click events on seat buttons
    gridContainer.querySelectorAll('.seat-btn:not(.occupied)').forEach(btn => {
      btn.addEventListener('click', () => {
        const seat = {
          id: btn.dataset.seatId,
          row: Number(btn.dataset.row),
          seat: Number(btn.dataset.seat),
          tier: btn.dataset.tier,
          price: Number(btn.dataset.price)
        };

        const result = state.toggleSeat(seat);
        if (result.success) {
          btn.classList.toggle('selected');
        } else if (result.error && this.app) {
          this.app.showToast(result.error, 'warning');
        }
      });
    });

    this.updateSummary();
  },

  /**
   * Update live selection summary panel
   */
  updateSummary() {
    const summaryContainer = document.getElementById('selected-seats-list');
    const totalEl = document.getElementById('total-amount-display');
    const countEl = document.getElementById('selected-count-badge');
    const { selectedSeats } = state.getState();

    if (countEl) {
      countEl.textContent = `${selectedSeats.length}/6`;
    }

    if (summaryContainer) {
      if (selectedSeats.length === 0) {
        summaryContainer.innerHTML = `<span style="font-size: 13px; color: var(--color-text-muted);">Місця ще не обрані (клікніть на сидіння на схемі)</span>`;
      } else {
        summaryContainer.innerHTML = selectedSeats.map(s => {
          const rowLetter = String.fromCharCode(64 + s.row);
          return `
            <span class="selected-seat-badge">
              <span>Ряд ${rowLetter}, М${s.seat} (${s.price} грн)</span>
              <button class="remove-seat-btn" data-seat-id="${s.id}" title="Прибрати місце">✕</button>
            </span>
          `;
        }).join('');

        // Attach removal listeners
        summaryContainer.querySelectorAll('.remove-seat-btn').forEach(btn => {
          btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const seatId = btn.dataset.seatId;
            const seat = selectedSeats.find(s => s.id === seatId);
            if (seat) {
              state.toggleSeat(seat);
              // Also remove selected class from seat button in grid
              const gridBtn = document.querySelector(`.seat-btn[data-seat-id="${seatId}"]`);
              if (gridBtn) gridBtn.classList.remove('selected');
            }
          });
        });
      }
    }

    // Calculate total price
    const subtotal = selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);
    if (totalEl) {
      totalEl.innerHTML = `<span>${subtotal}</span> грн`;
    }

    if (this.checkoutBtn) {
      this.checkoutBtn.disabled = selectedSeats.length === 0;
    }
  }
};
