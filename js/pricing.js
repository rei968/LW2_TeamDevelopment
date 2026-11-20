/**
 * CineSphere — Ticket Pricing Module
 * Baseline pricing calculation engine ready for enhancement and merge conflict simulation.
 */

// Registered Promo Codes
const PROMO_CODES = {
  'STUDENT10': { discountPercent: 10, label: 'Студентська знижка (-10%)' },
  'CINE20': { discountPercent: 20, label: 'Промокод кіномана (-20%)' }
};

/**
 * Baseline Ticket Price Calculator
 * Supports both positional args: (basePrice, seatType, isStudent)
 * and object config: ({ basePrice, seatTier, isStudent })
 */
export function calculateTicketPrice(basePrice, seatType = 'standard', isStudent = false) {
  // Support object parameter style
  if (typeof basePrice === 'object' && basePrice !== null) {
    const config = basePrice;
    basePrice = config.basePrice || 0;
    seatType = config.seatTier || config.seatType || 'standard';
    isStudent = Boolean(config.isStudent);
  }

  let price = Number(basePrice) || 0;

  // Baseline tier surcharge
  if (seatType === 'vip') {
    price += 50;
  } else if (seatType === 'couches') {
    price += 100;
  }

  // Baseline student discount
  if (isStudent) {
    price *= 0.85;
  }

  return Math.round(price);
}

/**
 * Calculate individual seat price based on hall tier definition and showtime modifier
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
 * Calculate total order summary for a set of selected seats
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
  const upperCode = (promoCode || '').trim().toUpperCase();

  if (upperCode && PROMO_CODES[upperCode]) {
    discountRate = PROMO_CODES[upperCode].discountPercent / 100;
    isPromoValid = true;
  } else if (isStudent) {
    discountRate = 0.15; // 15% student discount baseline
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
    promoLabel: isPromoValid ? PROMO_CODES[upperCode].label : null
  };
}

/**
 * Apply promo helper
 */
export function applyPromo(promoCode, subtotal = 0) {
  const upperCode = (promoCode || '').trim().toUpperCase();
  if (PROMO_CODES[upperCode]) {
    const discountRate = PROMO_CODES[upperCode].discountPercent / 100;
    const discountAmount = Math.round(subtotal * discountRate);
    return {
      valid: true,
      code: upperCode,
      label: PROMO_CODES[upperCode].label,
      discountAmount,
      total: Math.max(0, subtotal - discountAmount)
    };
  }
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
