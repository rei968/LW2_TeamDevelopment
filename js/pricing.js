/**
 * CineSphere — Ticket Pricing Module
 * 
 * 👤 Відповідальний розробник: Ярік (@YaroslavYanushevych)
 * 🌿 Робоча гілка в Git: feature/yarik-booking-logic
 * 
 * 📌 Завдання для Яріка:
 * 1. calculateTicketPrice(basePrice, seatType, isStudent) — розрахунок вартості квитка з націнками
 *    (VIP +50 грн, Дивани +100 грн) та студентською знижкою 15%.
 * 2. calculateSeatPrice(tier, hall, showtime) — визначення вартості конкретного крісла.
 * 3. calculateOrderTotal(params) — розрахунок повної суми замовлення, знижок та промокодів.
 * 4. applyPromo(promoCode, subtotal) — перевірка та застосування промокодів (STUDENT10, CINE20).
 * 5. Змоделювати Merge Conflict у функції calculateOrderTotal згідно з docs/GIT_WORKFLOW.md!
 */

// Зареєстровані промокоди кінотеатру
export const PROMO_CODES = {
  'STUDENT10': { discountPercent: 10, label: 'Студентська знижка (-10%)' },
  'CINE20': { discountPercent: 20, label: 'Промокод кіномана (-20%)' }
};

/**
 * Розрахунок вартості одного квитка
 * TODO (Ярік): Реалізувати націнки за категорію сидіння та знижку для студентів
 */
export function calculateTicketPrice(basePrice = 150, seatType = 'standard', isStudent = false) {
  // Підтримка виклику як об'єктом, так і позиційними аргументами
  if (typeof basePrice === 'object' && basePrice !== null) {
    const config = basePrice;
    basePrice = config.basePrice || 150;
    seatType = config.seatTier || config.seatType || 'standard';
    isStudent = Boolean(config.isStudent);
  }

  let price = Number(basePrice) || 150;

  // TODO (Ярік): Додати націнки для VIP (+50) та Couches (+100)
  // TODO (Ярік): Додати застосування студентської знижки (15%)

  return Math.round(price);
}

/**
 * Розрахунок ціни окремого місця з урахуванням конфігурації залу та сеансу
 * TODO (Ярік): Отримати базову ціну категорії з hall.seatTiers
 */
export function calculateSeatPrice(tier = 'standard', hall = null, showtime = null) {
  let basePrice = 150;

  if (hall && hall.seatTiers && hall.seatTiers[tier]) {
    basePrice = hall.seatTiers[tier].basePrice;
  }

  const modifier = (showtime && showtime.basePriceModifier) ? showtime.basePriceModifier : 1.0;
  const adjustedBase = Math.round(basePrice * modifier);

  return calculateTicketPrice(adjustedBase, tier, false);
}

/**
 * Підрахунок загальної суми замовлення
 * TODO (Ярік): Реалізувати підсумовування цін обраних місць, застосування знижок та промокодів
 * ⚠️ УВАГА: У цій функції Ярік та Же моделюють Merge Conflict згідно з docs/GIT_WORKFLOW.md!
 */
export function calculateOrderTotal(params = {}) {
  const { seats = [], hall = null, showtime = null, promoCode = null, isStudent = false } = params;

  let subtotal = 0;
  const seatItems = seats.map(s => {
    const price = s.price || calculateSeatPrice(s.tier, hall, showtime);
    subtotal += price;
    return {
      seatId: s.id,
      tier: s.tier,
      basePrice: price,
      finalPrice: price
    };
  });

  let discountRate = 0;
  let isPromoValid = false;

  // TODO (Ярік): Додати перевірку валідності промокоду та нарахування знижки

  const discountAmount = Math.round(subtotal * discountRate);
  const total = Math.max(0, subtotal - discountAmount);

  return {
    seatItems,
    subtotal,
    discountAmount,
    discountRate,
    total,
    isPromoValid,
    promoLabel: null
  };
}

/**
 * Хелпер перевірки промокоду
 * TODO (Ярік): Перевірка існування коду в PROMO_CODES та розрахунок знижки
 */
export function applyPromo(promoCode, subtotal = 0) {
  const upperCode = (promoCode || '').trim().toUpperCase();

  // TODO (Ярік): Реалізувати валідацію промокоду

  return {
    valid: false,
    code: promoCode,
    discountAmount: 0,
    total: subtotal
  };
}

export const pricing = {
  PROMO_CODES,
  calculateTicketPrice,
  calculateSeatPrice,
  calculateOrderTotal,
  applyPromo
};

if (typeof window !== 'undefined') {
  window.CineSpherePricing = pricing;
}

if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    pricing,
    calculateTicketPrice,
    calculateSeatPrice,
    calculateOrderTotal,
    applyPromo,
    PROMO_CODES
  };
}
