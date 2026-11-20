/**
 * CineSphere — LocalStorage Database Engine & Mock Seeder
 * Persistence layer for movies, halls, showtimes, and bookings.
 */

// LocalStorage Namespaced Keys
const STORAGE_KEYS = {
  MOVIES: 'cinesphere_movies_v1',
  HALLS: 'cinesphere_halls_v1',
  SHOWTIMES: 'cinesphere_showtimes_v1',
  BOOKINGS: 'cinesphere_bookings_v1',
  INITIALIZED: 'cinesphere_initialized_v1'
};

// High-resolution offline SVG Data URIs for movie posters
const POSTERS = {
  DUNE: `data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="300" height="420" viewBox="0 0 300 420"><defs><linearGradient id="g1" x1="0%" y1="0%" x2="100%" y2="100%"><stop offset="0%" stop-color="%2378350f"/><stop offset="50%" stop-color="%23b45309"/><stop offset="100%" stop-color="%230f172a"/></linearGradient><radialGradient id="sun" cx="50%" cy="30%" r="40%"><stop offset="0%" stop-color="%23f59e0b"/><stop offset="100%" stop-color="%2378350f" stop-opacity="0"/></radialGradient></defs><rect width="300" height="420" fill="url(%23g1)"/><circle cx="150" cy="140" r="90" fill="url(%23sun)"/><path d="M-20 320 Q 80 240, 180 290 T 320 260 L 320 420 L -20 420 Z" fill="%23451a03" opacity="0.9"/><path d="M-10 350 Q 120 280, 240 330 T 320 300 L 320 420 L -10 420 Z" fill="%231e1b4b" opacity="0.8"/><text x="150" y="80" fill="%23fef3c7" font-family="sans-serif" font-size="28" font-weight="900" text-anchor="middle" letter-spacing="6">DUNE</text><text x="150" y="110" fill="%23fde68a" font-family="sans-serif" font-size="14" font-weight="600" text-anchor="middle" letter-spacing="4">PART TWO</text><text x="150" y="390" fill="%23fcd34d" font-family="sans-serif" font-size="12" font-weight="700" text-anchor="middle" letter-spacing="2">A DENIS VILLENEUVE FILM</text></svg>`,

  OPPENHEIMER: `data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="300" height="420" viewBox="0 0 300 420"><defs><linearGradient id="bg" x1="0%" y1="0%" x2="0%" y2="100%"><stop offset="0%" stop-color="%230f172a"/><stop offset="40%" stop-color="%231e1b4b"/><stop offset="80%" stop-color="%237c2d12"/><stop offset="100%" stop-color="%23ea580c"/></linearGradient><radialGradient id="blast" cx="50%" cy="65%" r="50%"><stop offset="0%" stop-color="%23fef08a"/><stop offset="30%" stop-color="%23f97316"/><stop offset="100%" stop-color="%237c2d12" stop-opacity="0"/></radialGradient></defs><rect width="300" height="420" fill="url(%23bg)"/><circle cx="150" cy="270" r="110" fill="url(%23blast)"/><circle cx="150" cy="270" r="15" fill="%23fff" opacity="0.9"/><ellipse cx="150" cy="270" rx="90" ry="25" fill="none" stroke="%23fed7aa" stroke-width="2" transform="rotate(-25 150 270)" opacity="0.7"/><ellipse cx="150" cy="270" rx="90" ry="25" fill="none" stroke="%23fed7aa" stroke-width="2" transform="rotate(35 150 270)" opacity="0.7"/><text x="150" y="70" fill="%23fed7aa" font-family="sans-serif" font-size="20" font-weight="900" text-anchor="middle" letter-spacing="4">OPPENHEIMER</text><text x="150" y="95" fill="%23fdba74" font-family="sans-serif" font-size="11" font-weight="600" text-anchor="middle" letter-spacing="3">CHRISTOPHER NOLAN</text></svg>`,

  SPIDERMAN: `data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="300" height="420" viewBox="0 0 300 420"><defs><linearGradient id="sp-bg" x1="0%" y1="0%" x2="100%" y2="100%"><stop offset="0%" stop-color="%231e1b4b"/><stop offset="50%" stop-color="%23831843"/><stop offset="100%" stop-color="%230284c7"/></linearGradient><linearGradient id="neon" x1="0%" y1="0%" x2="100%" y2="0%"><stop offset="0%" stop-color="%23ec4899"/><stop offset="100%" stop-color="%2338bdf8"/></linearGradient></defs><rect width="300" height="420" fill="url(%23sp-bg)"/><polygon points="150,110 185,155 165,190 150,220 135,190 115,155" fill="%230f172a" stroke="url(%23neon)" stroke-width="4"/><path d="M 150 110 L 90 90 M 150 110 L 210 90 M 150 220 L 70 250 M 150 220 L 230 250" stroke="%2338bdf8" stroke-width="3"/><text x="150" y="60" fill="%2338bdf8" font-family="sans-serif" font-size="18" font-weight="900" text-anchor="middle" letter-spacing="2">SPIDER-MAN</text><text x="150" y="80" fill="%23f472b6" font-family="sans-serif" font-size="12" font-weight="700" text-anchor="middle" letter-spacing="3">ACROSS THE SPIDER-VERSE</text></svg>`,

  INTERSTELLAR: `data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="300" height="420" viewBox="0 0 300 420"><defs><radialGradient id="wormhole" cx="50%" cy="45%" r="45%"><stop offset="0%" stop-color="%23000"/><stop offset="35%" stop-color="%230369a1"/><stop offset="65%" stop-color="%230284c7"/><stop offset="85%" stop-color="%2338bdf8"/><stop offset="100%" stop-color="%23090d16" stop-opacity="0"/></radialGradient></defs><rect width="300" height="420" fill="%23090d16"/><circle cx="150" cy="180" r="110" fill="url(%23wormhole)"/><ellipse cx="150" cy="180" rx="125" ry="30" fill="none" stroke="%23bae6fd" stroke-width="3" transform="rotate(-15 150 180)" opacity="0.85"/><text x="150" y="65" fill="%23f0f9ff" font-family="sans-serif" font-size="20" font-weight="900" text-anchor="middle" letter-spacing="6">INTERSTELLAR</text><text x="150" y="380" fill="%2394a3b8" font-family="sans-serif" font-size="11" font-weight="600" text-anchor="middle" letter-spacing="3">MANKIND WAS BORN ON EARTH</text></svg>`
};

// Seed Fixtures
const SEED_MOVIES = [
  {
    id: 'mov-dune2',
    title: 'Дюна: Частина друга',
    originalTitle: 'Dune: Part Two',
    genre: ['Sci-Fi', 'Adventure', 'Action'],
    ageRating: '16+',
    durationMinutes: 166,
    duration: 166,
    rating: 8.6,
    basePrice: 160,
    poster: POSTERS.DUNE,
    posterUrl: POSTERS.DUNE,
    backdropUrl: POSTERS.DUNE,
    description: 'Пол Атрід об’єднується з Чані та фременами, вирушаючи на стежку помсти змовникам, які знищили його родину.',
    director: 'Дені Вільньов',
    cast: ['Тімоті Шаламе', 'Зендея', 'Ребекка Фергюсон', 'Хав’єр Бардем'],
    releaseYear: 2024,
    showtimes: ['st-101', 'st-102']
  },
  {
    id: 'mov-oppenheimer',
    title: 'Оппенгеймер',
    originalTitle: 'Oppenheimer',
    genre: ['Biography', 'Drama', 'History'],
    ageRating: '16+',
    durationMinutes: 180,
    duration: 180,
    rating: 8.9,
    basePrice: 150,
    poster: POSTERS.OPPENHEIMER,
    posterUrl: POSTERS.OPPENHEIMER,
    backdropUrl: POSTERS.OPPENHEIMER,
    description: 'Історія життя американського фізика Роберта Оппенгеймера, який очолив "Мангеттенський проєкт" створення першої ядерної зброї.',
    director: 'Крістофер Нолан',
    cast: ['Кілліан Мерфі', 'Емілі Блант', 'Метт Деймон', 'Роберт Дауні-мол.'],
    releaseYear: 2023,
    showtimes: ['st-103', 'st-104']
  },
  {
    id: 'mov-spiderverse',
    title: 'Людина-павук: Крізь всесвіт',
    originalTitle: 'Spider-Man: Across the Spider-Verse',
    genre: ['Animation', 'Action', 'Sci-Fi'],
    ageRating: '12+',
    durationMinutes: 140,
    duration: 140,
    rating: 8.7,
    basePrice: 140,
    poster: POSTERS.SPIDERMAN,
    posterUrl: POSTERS.SPIDERMAN,
    backdropUrl: POSTERS.SPIDERMAN,
    description: 'Майлз Моралес катапультується через Мультивсесвіт, де зустрічає команду Людей-павуків, покликаних захищати саме його існування.',
    director: 'Жоакін Душ Сантуш',
    cast: ['Шамейк Мур', 'Гейлі Стайнфільд', 'Оскар Айзек'],
    releaseYear: 2023,
    showtimes: ['st-105']
  },
  {
    id: 'mov-interstellar',
    title: 'Інтерстеллар',
    originalTitle: 'Interstellar',
    genre: ['Sci-Fi', 'Drama', 'Adventure'],
    ageRating: '12+',
    durationMinutes: 169,
    duration: 169,
    rating: 8.7,
    basePrice: 150,
    poster: POSTERS.INTERSTELLAR,
    posterUrl: POSTERS.INTERSTELLAR,
    backdropUrl: POSTERS.INTERSTELLAR,
    description: 'Команда дослідників вирушає крізь червоточину в космосі в пошуках нової придатної для життя планети для людства.',
    director: 'Крістофер Нолан',
    cast: ['Меттью МакКонахі', 'Енн Гетевей', 'Джессіка Честейн', 'Майкл Кейн'],
    releaseYear: 2014,
    showtimes: ['st-106']
  }
];

const SEED_HALLS = [
  {
    id: 'hall-imax',
    name: 'IMAX Laser Hall',
    totalRows: 8,
    seatsPerRow: 10,
    seatTiers: {
      standard: { rows: [1, 2, 3, 4], basePrice: 160 },
      vip: { rows: [5, 6, 7], basePrice: 260 },
      couches: { rows: [8], basePrice: 380 }
    }
  },
  {
    id: 'hall-standard-1',
    name: 'Chamber Hall (Зал 1)',
    totalRows: 6,
    seatsPerRow: 8,
    seatTiers: {
      standard: { rows: [1, 2, 3, 4], basePrice: 130 },
      vip: { rows: [5, 6], basePrice: 200 }
    }
  }
];

const SEED_SHOWTIMES = [
  {
    id: 'st-101',
    movieId: 'mov-dune2',
    hallId: 'hall-imax',
    date: '2026-11-20',
    time: '18:30',
    format: 'IMAX 2D',
    basePriceModifier: 1.0
  },
  {
    id: 'st-102',
    movieId: 'mov-dune2',
    hallId: 'hall-imax',
    date: '2026-11-20',
    time: '21:45',
    format: 'IMAX 2D',
    basePriceModifier: 1.15
  },
  {
    id: 'st-103',
    movieId: 'mov-oppenheimer',
    hallId: 'hall-imax',
    date: '2026-11-20',
    time: '14:00',
    format: 'IMAX 2D',
    basePriceModifier: 0.9
  },
  {
    id: 'st-104',
    movieId: 'mov-oppenheimer',
    hallId: 'hall-standard-1',
    date: '2026-11-20',
    time: '19:00',
    format: 'Standard 2D',
    basePriceModifier: 1.0
  },
  {
    id: 'st-105',
    movieId: 'mov-spiderverse',
    hallId: 'hall-standard-1',
    date: '2026-11-20',
    time: '16:30',
    format: 'Standard 2D',
    basePriceModifier: 1.0
  },
  {
    id: 'st-106',
    movieId: 'mov-interstellar',
    hallId: 'hall-imax',
    date: '2026-11-20',
    time: '20:30',
    format: 'IMAX 2D',
    basePriceModifier: 1.1
  }
];

// Initial mock bookings to verify occupied seats immediately
const SEED_BOOKINGS = [
  {
    id: 'CS-89241',
    showtimeId: 'st-101',
    movieId: 'mov-dune2',
    movieTitle: 'Дюна: Частина друга',
    hallName: 'IMAX Laser Hall',
    showtimeDate: '2026-11-20',
    showtimeTime: '18:30',
    seats: [
      { id: 'R4-S5', row: 4, seat: 5, tier: 'standard', price: 160 },
      { id: 'R4-S6', row: 4, seat: 6, tier: 'standard', price: 160 }
    ],
    customer: {
      fullName: 'Олексій Стратонов',
      email: 'ossstratonov@gmail.com',
      phone: '+380501234567'
    },
    pricing: {
      subtotal: 320,
      discountAmount: 0,
      promoCodeApplied: null,
      totalAmount: 320
    },
    status: 'confirmed',
    createdAt: '2026-11-20T10:15:00Z'
  },
  {
    id: 'CS-44120',
    showtimeId: 'st-101',
    movieId: 'mov-dune2',
    movieTitle: 'Дюна: Частина друга',
    hallName: 'IMAX Laser Hall',
    showtimeDate: '2026-11-20',
    showtimeTime: '18:30',
    seats: [
      { id: 'R6-S4', row: 6, seat: 4, tier: 'vip', price: 260 },
      { id: 'R6-S5', row: 6, seat: 5, tier: 'vip', price: 260 }
    ],
    customer: {
      fullName: 'Ярослав Коваленко',
      email: 'yarik@cinesphere.ua',
      phone: '+380671112233'
    },
    pricing: {
      subtotal: 520,
      discountAmount: 0,
      promoCodeApplied: null,
      totalAmount: 520
    },
    status: 'confirmed',
    createdAt: '2026-11-20T11:00:00Z'
  }
];

// Fallback in-memory storage for non-browser / test runner environments
const memoryStorage = {};

function getStorage() {
  if (typeof window !== 'undefined' && window.localStorage) {
    return window.localStorage;
  }
  return {
    getItem: (key) => (key in memoryStorage ? memoryStorage[key] : null),
    setItem: (key, val) => { memoryStorage[key] = String(val); },
    removeItem: (key) => { delete memoryStorage[key]; },
    clear: () => { Object.keys(memoryStorage).forEach(k => delete memoryStorage[k]); }
  };
}

/**
 * Robust JSON reader with error handling
 */
function readStorage(key, defaultValue = []) {
  try {
    const raw = getStorage().getItem(key);
    if (!raw) return defaultValue;
    return JSON.parse(raw);
  } catch (err) {
    console.warn(`[CineSphereDB] Warning: Corrupted JSON for key "${key}". Returning default.`, err);
    return defaultValue;
  }
}

/**
 * Robust JSON writer
 */
function writeStorage(key, value) {
  try {
    getStorage().setItem(key, JSON.stringify(value));
    return true;
  } catch (err) {
    console.error(`[CineSphereDB] Error writing to storage key "${key}":`, err);
    return false;
  }
}

/**
 * CineSphere Database API
 */
export const db = {
  KEYS: STORAGE_KEYS,

  /**
   * Initialize LocalStorage with seed data if not yet seeded
   */
  init() {
    const isInit = getStorage().getItem(STORAGE_KEYS.INITIALIZED);
    const existingMovies = readStorage(STORAGE_KEYS.MOVIES, null);

    if (!isInit || !existingMovies || existingMovies.length === 0) {
      writeStorage(STORAGE_KEYS.MOVIES, SEED_MOVIES);
      writeStorage(STORAGE_KEYS.HALLS, SEED_HALLS);
      writeStorage(STORAGE_KEYS.SHOWTIMES, SEED_SHOWTIMES);
      writeStorage(STORAGE_KEYS.BOOKINGS, SEED_BOOKINGS);
      getStorage().setItem(STORAGE_KEYS.INITIALIZED, 'true');
      console.log('[CineSphereDB] Successfully initialized database with seed fixtures.');
    }
  },

  /**
   * Reset database back to default seed fixtures
   */
  reset() {
    writeStorage(STORAGE_KEYS.MOVIES, SEED_MOVIES);
    writeStorage(STORAGE_KEYS.HALLS, SEED_HALLS);
    writeStorage(STORAGE_KEYS.SHOWTIMES, SEED_SHOWTIMES);
    writeStorage(STORAGE_KEYS.BOOKINGS, SEED_BOOKINGS);
    getStorage().setItem(STORAGE_KEYS.INITIALIZED, 'true');
    console.log('[CineSphereDB] Database reset to default seed fixtures.');
  },

  /**
   * Generic load helper (graceful handling of corrupted json)
   */
  load(key) {
    return readStorage(key, null);
  },

  // Movies
  getMovies() {
    this.init();
    return readStorage(STORAGE_KEYS.MOVIES, []);
  },

  getMovieById(id) {
    const movies = this.getMovies();
    return movies.find(m => m.id === id) || null;
  },

  // Halls
  getHalls() {
    this.init();
    return readStorage(STORAGE_KEYS.HALLS, []);
  },

  getHallById(id) {
    const halls = this.getHalls();
    return halls.find(h => h.id === id) || null;
  },

  // Showtimes
  getShowtimes(filter = {}) {
    this.init();
    let list = readStorage(STORAGE_KEYS.SHOWTIMES, []);
    if (filter.movieId) {
      list = list.filter(s => s.movieId === filter.movieId);
    }
    if (filter.date) {
      list = list.filter(s => s.date === filter.date);
    }
    return list;
  },

  getShowtimeById(id) {
    const showtimes = this.getShowtimes();
    return showtimes.find(s => s.id === id) || null;
  },

  // Bookings
  getBookings() {
    this.init();
    return readStorage(STORAGE_KEYS.BOOKINGS, []);
  },

  getBookingById(id) {
    const bookings = this.getBookings();
    return bookings.find(b => b.id === id) || null;
  },

  /**
   * Returns an array of occupied seat IDs for a given showtime
   * (e.g. ['R4-S5', 'R4-S6'])
   */
  getOccupiedSeatIds(showtimeId) {
    const bookings = this.getBookings().filter(
      b => b.showtimeId === showtimeId && b.status === 'confirmed'
    );
    const occupied = new Set();
    bookings.forEach(b => {
      if (Array.isArray(b.seats)) {
        b.seats.forEach(s => occupied.add(s.id));
      }
    });
    return Array.from(occupied);
  },

  /**
   * Saves a new booking with double-booking collision check
   */
  saveBooking(booking) {
    if (!booking || !booking.showtimeId || !Array.isArray(booking.seats) || booking.seats.length === 0) {
      return { success: false, error: 'Некоректні дані бронювання або не обрано жодного місця.' };
    }

    const occupied = this.getOccupiedSeatIds(booking.showtimeId);
    const hasCollision = booking.seats.some(s => occupied.includes(s.id));

    if (hasCollision) {
      return {
        success: false,
        error: 'Одне або декілька обраних місць вже заброньовано іншим користувачем.'
      };
    }

    const bookings = this.getBookings();
    bookings.push(booking);
    writeStorage(STORAGE_KEYS.BOOKINGS, bookings);

    return { success: true, bookingId: booking.id };
  },

  /**
   * Cancels a booking by ID (marking as cancelled or removing it, freeing up seats)
   */
  cancelBooking(bookingId) {
    const bookings = this.getBookings();
    const index = bookings.findIndex(b => b.id === bookingId);
    if (index === -1) {
      return { success: false, error: 'Бронювання не знайдено.' };
    }

    // Remove from array so seats become available immediately
    bookings.splice(index, 1);
    writeStorage(STORAGE_KEYS.BOOKINGS, bookings);
    return { success: true };
  },

  /**
   * Alias for cancelBooking to satisfy TC-10 (deleteBooking)
   */
  deleteBooking(bookingId) {
    return this.cancelBooking(bookingId);
  }
};

// Global browser window attachment for non-module scripts and test runner
if (typeof window !== 'undefined') {
  window.CineSphereDB = db;
}

// CommonJS export for Node.js test runners
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { db, STORAGE_KEYS, SEED_MOVIES, SEED_HALLS, SEED_SHOWTIMES, SEED_BOOKINGS };
}
