/**
 * CineSphere — Movie Catalog View
 * Renders movie showcase cards, search filtering, genre chips, and projection format badges.
 */

import { db } from '../db.js';
import { state } from '../state.js';

export const catalogView = {
  /**
   * Initialize catalog DOM elements and event listeners
   */
  init() {
    this.container = document.getElementById('movie-grid');
    this.searchInput = document.getElementById('search-movies');
    this.clearSearchBtn = document.getElementById('btn-clear-search');
    this.filterChips = document.querySelectorAll('.filter-chip');

    // Live search input handling
    if (this.searchInput) {
      this.searchInput.addEventListener('input', (e) => {
        const value = e.target.value;
        this.updateClearButtonVisibility(value);
        state.setState({ searchQuery: value.trim().toLowerCase() });
        this.render();
      });

      this.searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
          this.clearSearch();
        }
      });
    }

    // Search reset button handling
    if (this.clearSearchBtn) {
      this.clearSearchBtn.addEventListener('click', () => {
        this.clearSearch();
      });
    }

    // Category filter chips handling
    if (this.filterChips) {
      this.filterChips.forEach(chip => {
        chip.addEventListener('click', () => {
          this.filterChips.forEach(c => c.classList.remove('active'));
          chip.classList.add('active');
          state.setState({ selectedGenre: chip.dataset.genre || 'all' });
          this.render();
        });
      });
    }

    this.render();
  },

  /**
   * Update visibility of clear search button
   * @param {string} value
   */
  updateClearButtonVisibility(value) {
    if (!this.clearSearchBtn) return;
    if (value && value.length > 0) {
      this.clearSearchBtn.classList.remove('hidden');
    } else {
      this.clearSearchBtn.classList.add('hidden');
    }
  },

  /**
   * Clear search input and restore full catalog view
   */
  clearSearch() {
    if (this.searchInput) {
      this.searchInput.value = '';
      this.searchInput.focus();
    }
    this.updateClearButtonVisibility('');
    state.setState({ searchQuery: '' });
    this.render();
  },

  /**
   * Reset all search queries and genre filters
   */
  resetAllFilters() {
    if (this.searchInput) {
      this.searchInput.value = '';
    }
    this.updateClearButtonVisibility('');

    if (this.filterChips) {
      this.filterChips.forEach(chip => {
        if (chip.dataset.genre === 'all') {
          chip.classList.add('active');
        } else {
          chip.classList.remove('active');
        }
      });
    }

    state.setState({ searchQuery: '', selectedGenre: 'all' });
    this.render();
  },

  /**
   * Determine CSS badge class based on projection format
   * @param {string} format
   * @returns {string}
   */
  getFormatBadgeClass(format = '') {
    const fmt = String(format).toUpperCase();
    if (fmt.includes('IMAX')) return 'format-badge-imax';
    if (fmt.includes('3D')) return 'format-badge-3d';
    if (fmt.includes('ATMOS') || fmt.includes('DOLBY')) return 'format-badge-atmos';
    return 'format-badge-2d';
  },

  /**
   * Render filtered movie cards
   */
  render() {
    if (!this.container) return;

    const movies = db.getMovies();
    const currentState = state.getState();
    const query = currentState.searchQuery || '';
    const genre = currentState.selectedGenre || 'all';

    // Synchronize search input if updated externally
    if (this.searchInput && document.activeElement !== this.searchInput) {
      if (this.searchInput.value.trim().toLowerCase() !== query) {
        this.searchInput.value = query;
      }
      this.updateClearButtonVisibility(this.searchInput.value);
    }

    const filtered = movies.filter(movie => {
      // Genre filter
      const matchesGenre = genre === 'all' || 
        (Array.isArray(movie.genre) ? movie.genre.includes(genre) : (movie.genre || '').includes(genre)) ||
        (Array.isArray(movie.genres) && movie.genres.includes(genre));
      // Search filter
      const matchesQuery = !query ||
        movie.title.toLowerCase().includes(query) ||
        (movie.originalTitle && movie.originalTitle.toLowerCase().includes(query)) ||
        (movie.director && movie.director.toLowerCase().includes(query));

      return matchesGenre && matchesQuery;
    });

    // Empty state fallback
    if (filtered.length === 0) {
      this.container.innerHTML = `
        <div class="catalog-empty">
          <div class="catalog-empty-icon" aria-hidden="true">🔍</div>
          <h3 class="catalog-empty-title">Фільмів не знайдено</h3>
          <p class="catalog-empty-desc">
            ${query ? `За запитом «<strong>${query}</strong>»` : 'У вибраній категорії'} 
            не знайдено жодного сеансу. Спробуйте змінити критерії пошуку або обрати інший жанр.
          </p>
          <button type="button" id="btn-catalog-reset-filters" class="btn btn-outline btn-sm btn-reset-filters">
            🔄 Скинути всі фільтри
          </button>
        </div>
      `;

      const resetBtn = this.container.querySelector('#btn-catalog-reset-filters');
      if (resetBtn) {
        resetBtn.addEventListener('click', () => this.resetAllFilters());
      }
      return;
    }

    // Render movie cards grid
    this.container.innerHTML = filtered.map(movie => {
      const showtimes = db.getShowtimes({ movieId: movie.id });
      const genreList = Array.isArray(movie.genre) ? movie.genre : (Array.isArray(movie.genres) ? movie.genres : (movie.genre || '').split(',').map(s => s.trim()));
      const genreTags = genreList.map(g => `<span class="genre-tag">${g}</span>`).join('');

      const showtimeButtons = showtimes.map(st => {
        const badgeClass = this.getFormatBadgeClass(st.format);
        return `
          <button class="showtime-pill" data-showtime-id="${st.id}" data-movie-id="${movie.id}" title="Сеанс о ${st.time} (${st.format})">
            <span class="showtime-time">${st.time}</span>
            <span class="showtime-format ${badgeClass}">${st.format}</span>
          </button>
        `;
      }).join('');

      return `
        <article class="movie-card" data-movie-id="${movie.id}">
          <div class="movie-poster-wrapper">
            <img class="movie-poster-img" src="${movie.poster}" alt="${movie.title}" loading="lazy" />
            <div class="movie-poster-overlay">
              <div class="poster-top-badges">
                <span class="rating-badge">★ ${movie.rating.toFixed(1)}</span>
                <span class="age-badge">${movie.ageRating}</span>
              </div>
            </div>
          </div>
          <div class="movie-info">
            <h3 class="movie-title" title="${movie.title}">${movie.title}</h3>
            <div class="movie-meta">
              <span>⏱ ${movie.durationMinutes} хв</span>
              <span>•</span>
              <span>${movie.releaseYear}</span>
            </div>
            <div class="movie-genres">${genreTags}</div>
            <p class="movie-synopsis">${movie.description}</p>
            <div class="showtimes-group">
              <div class="showtimes-label">Сьогоднішні сеанси:</div>
              <div class="showtimes-list">
                ${showtimeButtons || '<span style="font-size: 12px; color: var(--color-text-muted);">Сеансів на сьогодні немає</span>'}
              </div>
            </div>
          </div>
        </article>
      `;
    }).join('');

    // Attach click listeners to showtime pills
    this.container.querySelectorAll('.showtime-pill').forEach(btn => {
      btn.addEventListener('click', (e) => {
        e.stopPropagation();
        const showtimeId = btn.dataset.showtimeId;
        const movieId = btn.dataset.movieId;
        state.clearSeats();
        state.setState({
          selectedMovieId: movieId,
          selectedShowtimeId: showtimeId,
          currentView: 'hall'
        });
      });
    });
  }
};
