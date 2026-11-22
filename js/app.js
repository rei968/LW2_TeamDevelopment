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
    db.init();

    this.catalogView.init();
    this.hallView.init(this);
    this.modalView.init(this);

    this.bindNavigation();

    state.subscribe('stateChange', ({ currentState, changedKeys }) => {
      if (changedKeys.includes('currentView')) {
        this.switchView(currentState.currentView);
      }
    });

    this.updateBookingsBadge();
  }

  bindNavigation() {
    document.querySelectorAll('.btn-nav-catalog, .brand').forEach(el => {
      el.addEventListener('click', (e) => {
        e.preventDefault();
        state.clearSeats();
        state.setState({ currentView: 'catalog' });
      });
    });

    const myBookingsBtn = document.getElementById('btn-open-bookings');
    if (myBookingsBtn) {
      myBookingsBtn.addEventListener('click', () => {
        this.modalView.openBookingsList();
      });
    }

    const hallsNavBtn = document.getElementById('btn-nav-halls');
    if (hallsNavBtn) {
      hallsNavBtn.addEventListener('click', (e) => {
        e.preventDefault();
        this.modalView.openHallsModal();
      });
    }

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

  updateBookingsBadge() {
    const badge = document.getElementById('bookings-count-badge');
    if (badge) {
      const count = db.getBookings().length;
      badge.textContent = count;
      badge.style.display = count > 0 ? 'inline-block' : 'none';
    }
  }

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

document.addEventListener('DOMContentLoaded', () => {
  const app = new CineSphereApp();
  window.CineSphere = app;
  app.init();
});
