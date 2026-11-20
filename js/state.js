/**
 * CineSphere — Reactive State Store & PubSub Event Bus
 * Single source of truth for runtime UI selection and view switching.
 */

class CineSphereStateStore {
  constructor() {
    this._state = {
      currentView: 'catalog',      // 'catalog' | 'hall' | 'bookings'
      selectedMovieId: null,
      selectedShowtimeId: null,
      selectedSeats: [],           // Max 6 seats: [{ id, row, seat, tier, price }]
      searchQuery: '',
      selectedGenre: 'all',
      customer: {
        fullName: '',
        email: '',
        phone: ''
      },
      promoCode: null,
      lastConfirmedBooking: null
    };

    this._listeners = new Map();
  }

  /**
   * Get immutable snapshot of current state
   */
  getState() {
    return {
      ...this._state,
      selectedSeats: [...this._state.selectedSeats],
      customer: { ...this._state.customer }
    };
  }

  /**
   * Merge partial state updates and trigger change events
   */
  setState(updates) {
    const prevState = this.getState();
    this._state = {
      ...this._state,
      ...updates,
      customer: updates.customer ? { ...this._state.customer, ...updates.customer } : this._state.customer
    };

    this.emit('stateChange', {
      prevState,
      currentState: this.getState(),
      changedKeys: Object.keys(updates)
    });
  }

  /**
   * Toggle seat selection (enforces max 6 seats limit)
   * Returns: { success: boolean, message?: string }
   */
  toggleSeat(seat) {
    const seats = [...this._state.selectedSeats];
    const index = seats.findIndex(s => s.id === seat.id);

    if (index > -1) {
      // Remove seat
      seats.splice(index, 1);
      this.setState({ selectedSeats: seats });
      this.emit('seatDeselected', seat);
      return { success: true, action: 'removed' };
    } else {
      // Check maximum limit
      if (seats.length >= 6) {
        return {
          success: false,
          error: 'Максимальна кількість обраних місць на одне замовлення — 6.'
        };
      }
      seats.push(seat);
      this.setState({ selectedSeats: seats });
      this.emit('seatSelected', seat);
      return { success: true, action: 'added' };
    }
  }

  /**
   * Clear all selected seats
   */
  clearSeats() {
    this.setState({ selectedSeats: [] });
    this.emit('seatsCleared');
  }

  /**
   * Subscribe to state bus events
   */
  subscribe(event, callback) {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, new Set());
    }
    this._listeners.get(event).add(callback);

    // Return unsubscribe function
    return () => {
      const handlers = this._listeners.get(event);
      if (handlers) {
        handlers.delete(callback);
      }
    };
  }

  /**
   * Emit an event to registered subscribers
   */
  emit(event, data) {
    const handlers = this._listeners.get(event);
    if (handlers) {
      handlers.forEach(fn => {
        try {
          fn(data);
        } catch (err) {
          console.error(`[CineSphereState] Error in handler for event "${event}":`, err);
        }
      });
    }
  }
}

export const state = new CineSphereStateStore();

if (typeof window !== 'undefined') {
  window.CineSphereState = state;
}

if (typeof module !== 'undefined' && module.exports) {
  module.exports = { state, CineSphereStateStore };
}
