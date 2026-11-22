export const PROMO_CODES = {
  'STUDENT10': { discountPercent: 10, label: 'Студентська знижка (-10%)' },
  'CINE20': { discountPercent: 20, label: 'Промокод кіномана (-20%)' }
};

export const SEAT_SURCHARGES = {
  standard: 0,
  vip: 50,
  couches: 100
};

export function calculateTicketPrice(basePrice = 150, seatType = 'standard', isStudent = false) {
  if (typeof basePrice === 'object' && basePrice !== null) {
    const config = basePrice;
    basePrice = config.basePrice || 150;
    seatType = config.seatTier || config.seatType || 'standard';
    isStudent = Boolean(config.isStudent);
  }

  let price = Math.max(0, Number(basePrice) || 150);
  const normalizedType = String(seatType || 'standard').toLowerCase();

  if (normalizedType === 'vip') {
    price += SEAT_SURCHARGES.vip;
  } else if (normalizedType === 'couches' || normalizedType === 'couch') {
    price += SEAT_SURCHARGES.couches;
  }

  if (isStudent) {
    price *= 0.85;
  }

  return Math.round(price);
}

export function calculateSeatPrice(tier = 'standard', hall = null, showtime = null) {
  let basePrice = 150;

  if (hall && hall.seatTiers && hall.seatTiers[tier]) {
    basePrice = hall.seatTiers[tier].basePrice;
  }

  const modifier = (showtime && showtime.basePriceModifier) ? showtime.basePriceModifier : 1.0;
  const adjustedBase = Math.round(basePrice * modifier);

  return calculateTicketPrice(adjustedBase, tier, false);
}

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
  const upperCode = (promoCode || '').trim().toUpperCase();
  const hasVipSeats = seats.some(s => s.tier === 'vip');

  if (upperCode && PROMO_CODES[upperCode]) {
    discountRate = PROMO_CODES[upperCode].discountPercent / 100;
    isPromoValid = true;
    promoLabel = PROMO_CODES[upperCode].label;
  } else if (isStudent) {
    discountRate = 0.15;
    promoLabel = 'Студентська знижка (-15%)';
  } else if (seats.length >= 3) {
    discountRate = 0.15;
    promoLabel = 'Групова знижка від 3 квитків (-15%)';
  } else if (hasVipSeats) {
    discountRate = 0.10;
    promoLabel = 'VIP-знижка (-10%)';
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
  SEAT_SURCHARGES,
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
    PROMO_CODES,
    SEAT_SURCHARGES
  };
}
