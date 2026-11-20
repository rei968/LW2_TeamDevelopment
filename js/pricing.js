/**
 * CineSphere — Ticket Pricing Module
 *
 * 👤 Відповідальний розробник: Ярік (@YaroslavYanushevych)
 * 🌿 Робоча гілка в Git: feature/yarik-booking-logic
 */

// Зареєстровані промокоди кінотеатру
export const PROMO_CODES = {
  'STUDENT10': { discountPercent: 10, label: 'Студентська знижка (-10%)' },
  'CINE20': { discountPercent: 20, label: 'Промокод кіномана (-20%)' }
};

// Націнки за категорію сидіння (грн)
const SEAT_SURCHARGES = {
  standard: 0,
  vip: 50,
  couches: 100
};

/**
 * Розрахунок вартості одного квитка з урахуванням категорії сидіння та студентської знижки
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
  const normalizedType = String(seatType || 'standard').toLowerCase();

  if (normalizedType === 'vip') {
    price += SEAT_SURCHARGES.vip;
  } else if (normalizedType === 'couches' || normalizedType === 'couch') {
    price += SEAT_SURCHARGES.couches;
  }

  if (isStudent) {
    price *= 0.85; // Студентська знижка 15%
  }

  return Math.round(price);
}

/**
 * Розрахунок ціни окремого місця з урахуванням конфігурації залу та сеансу
 */
export function calculateSeatPrice(tier = 'standard', hall = null, showtime = null) {
  const modifier = (showtime && showtime.basePriceModifier) ? showtime.basePriceModifier : 1.0;

  if (hall && hall.seatTiers && hall.seatTiers[tier]) {
    return Math.round(hall.seatTiers[tier].basePrice * modifier);
  }

  const adjustedBase = Math.round(150 * modifier);
  return calculateTicketPrice(adjustedBase, tier, false);
}

/**
 * Підрахунок загальної суми замовлення зі знижками та промокодами
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
  let promoLabel = null;

  // Перевірка промокоду
  const upperCode = (promoCode || '').trim().toUpperCase();
  if (upperCode && PROMO_CODES[upperCode]) {
    const promo = PROMO_CODES[upperCode];
    discountRate = promo.discountPercent / 100;
    isPromoValid = true;
    promoLabel = promo.label;
  } else if (isStudent) {
    discountRate = 0.15;
    promoLabel = 'Студентська знижка (-15%)';
  } else if (seats.length >= 3) {
    discountRate = 0.15; // Групова знижка 15% для компаній від 3 осіб
    promoLabel = 'Групова знижка від 3 квитків (-15%)';
  }

  const discountAmount = Math.round(subtotal * discountRate);
  const total = Math.max(0, subtotal - discountAmount);

  return {
    seatItems,
    subtotal,
    discountAmount,
    discountRate,
    total,
    isPromoValid,
    promoLabel
  };
}

/**
 * Хелпер перевірки та застосування промокоду
 */
export function applyPromo(promoCode, subtotal = 0) {
  const upperCode = (promoCode || '').trim().toUpperCase();
  const promo = PROMO_CODES[upperCode];

  if (!promo) {
    return {
      valid: false,
      code: promoCode,
      label: null,
      discountPercent: 0,
      discountAmount: 0,
      total: subtotal
    };
  }

  const discountAmount = Math.round(subtotal * (promo.discountPercent / 100));
  const total = Math.max(0, subtotal - discountAmount);

  return {
    valid: true,
    code: upperCode,
    label: promo.label,
    discountPercent: promo.discountPercent,
    discountAmount,
    total
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
