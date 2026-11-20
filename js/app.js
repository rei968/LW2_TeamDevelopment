/**
 * CineSphere — Main Application Controller
 * Coordinates Database, State, View transitions, and Toast notifications.
 */

import { db } from './db.js';
import { state } from './state.js';
import { catalogView } from './ui/catalogView.js';
import { hallView } from './ui/hallView.js';
import { modalView } from './ui/modalView.js';

class CineSphereApp {
  constructor() {
    this.catalogView = catalogView;
    this.hallView = hallView;
    this.modalView = modalView;
  }

  init() {
    console.log('[CineSphere] Initializing application...');

    // 1. Initialize LocalStorage Database with seed data
    db.init();

    // 2. Initialize UI Views
    this.catalogView.init();
    this.hallView.init(this);
    this.modalView.init(this);

    // 3. Bind Global Navigation Events
    this.bindNavigation();

    // 4. Subscribe to view state transitions
    state.subscribe('stateChange', ({ currentState, changedKeys }) => {
      if (changedKeys.includes('currentView')) {
        this.switchView(currentState.currentView);
      }
    });

    // 5. Update initial badge counters
    this.updateBookingsBadge();

    console.log('[CineSphere] Application ready.');
  }

  /**
   * Bind top header navigation buttons and quick actions
   */
  bindNavigation() {
    // Logo & Catalog Nav Links
    document.querySelectorAll('.btn-nav-catalog, .brand').forEach(el => {
      el.addEventListener('click', (e) => {
        e.preventDefault();
        state.clearSeats();
        state.setState({ currentView: 'catalog' });
      });
    });

    // My Bookings Nav Action
    const myBookingsBtn = document.getElementById('btn-open-bookings');
    if (myBookingsBtn) {
      myBookingsBtn.addEventListener('click', () => {
        this.modalView.openBookingsList();
      });
    }

    // Reset Database Diagnostic Button
    const resetDbBtn = document.getElementById('btn-reset-db');
    if (resetDbBtn) {
      resetDbBtn.addEventListener('click', () => {
        if (confirm('Скинути базу даних до початкового стану? Усі нові бронювання будуть очищені.')) {
          db.reset();
          state.clearSeats();
          state.setState({ currentView: 'catalog', searchQuery: '', selectedGenre: 'all' });
          this.catalogView.render();
          this.updateBookingsBadge();
          this.showToast('Базу даних успішно відновлено до початкового стану.', 'success');
        }
      });
    }
  }

  /**
   * View switcher between Catalog and Cinema Hall
   */
  switchView(viewName) {
    const catalogSection = document.getElementById('catalog-section');
    const hallSection = document.getElementById('hall-view');

    window.scrollTo({ top: 0, behavior: 'smooth' });

    if (viewName === 'hall') {
      if (catalogSection) catalogSection.classList.add('hidden');
      if (hallSection) {
        hallSection.classList.remove('hidden');
        this.hallView.render();
      }
    } else {
      if (hallSection) hallSection.classList.add('hidden');
      if (catalogSection) {
        catalogSection.classList.remove('hidden');
        this.catalogView.render();
      }
    }
  }

  /**
   * Update active bookings badge count on header
   */
  updateBookingsBadge() {
    const badge = document.getElementById('bookings-count-badge');
    if (badge) {
      const count = db.getBookings().length;
      badge.textContent = count;
      badge.style.display = count > 0 ? 'inline-block' : 'none';
    }
  }

  /**
   * Toast Notification Manager
   * @param {string} message - Message text
   * @param {'info'|'success'|'warning'|'error'} type - Toast type
   * @param {number} duration - Auto-dismiss timeout in ms
   */
  showToast(message, type = 'info', duration = 3500) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const icons = {
      info: 'ℹ',
      success: '✓',
      warning: '⚠',
      error: '✕'
    };

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
      <span class="toast-icon">${icons[type] || 'ℹ'}</span>
      <span class="toast-message">${message}</span>
    `;

    container.appendChild(toast);

    setTimeout(() => {
      toast.classList.add('toast-hiding');
      setTimeout(() => {
        if (toast.parentElement) toast.remove();
      }, 250);
    }, duration);
  }
}

// Bootstrap Application on DOM ready
document.addEventListener('DOMContentLoaded', () => {
  const app = new CineSphereApp();
  window.CineSphere = app;
  app.init();
});
