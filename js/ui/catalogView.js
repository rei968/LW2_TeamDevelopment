/**
 * CineSphere — Movie Catalog View
 * Renders movie showcase cards, search filtering, and genre tags.
 */

import { db } from '../db.js';
import { state } from '../state.js';

export const catalogView = {
  /**
   * Initialize catalog listeners and render initial catalog
   */
  init() {
    this.container = document.getElementById('movie-grid');
    this.searchInput = document.getElementById('search-movies');
    this.filterChips = document.querySelectorAll('.filter-chip');

    if (this.searchInput) {
      this.searchInput.addEventListener('input', (e) => {
        state.setState({ searchQuery: e.target.value.trim().toLowerCase() });
        this.render();
      });
    }

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
   * Render filtered movie cards
   */
  render() {
    if (!this.container) return;

    const movies = db.getMovies();
    const currentState = state.getState();
    const query = currentState.searchQuery;
    const genre = currentState.selectedGenre;

    const filtered = movies.filter(movie => {
      // Genre filter
      const matchesGenre = genre === 'all' || (Array.isArray(movie.genre) && movie.genre.includes(genre));
      // Search filter
      const matchesQuery = !query ||
        movie.title.toLowerCase().includes(query) ||
        (movie.originalTitle && movie.originalTitle.toLowerCase().includes(query)) ||
        (movie.director && movie.director.toLowerCase().includes(query));

      return matchesGenre && matchesQuery;
    });

    if (filtered.length === 0) {
      this.container.innerHTML = `
        <div class="catalog-empty" style="grid-column: 1 / -1;">
          <p style="font-size: 1.1rem; margin-bottom: 0.5rem;">Фільмів не знайдено 🔍</p>
          <p style="color: var(--color-text-muted);">Спробуйте змінити пошуковий запит або обрати інший жанр.</p>
        </div>
      `;
      return;
    }

    this.container.innerHTML = filtered.map(movie => {
      const showtimes = db.getShowtimes({ movieId: movie.id });
      const genreTags = (movie.genre || []).map(g => `<span class="genre-tag">${g}</span>`).join('');

      const showtimeButtons = showtimes.map(st => `
        <button class="showtime-pill" data-showtime-id="${st.id}" data-movie-id="${movie.id}">
          <span>${st.time}</span>
          <span class="showtime-format">${st.format}</span>
        </button>
      `).join('');

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
