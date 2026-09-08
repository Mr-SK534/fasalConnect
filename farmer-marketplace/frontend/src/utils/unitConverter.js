// frontend/src/utils/unitConverter.js

/**
 * Returns the conversion factor to convert quantity from given unit to KG.
 * 1 Ton = 1000 KG
 * 1 Quintal = 100 KG
 * 1 KG = 1 KG
 * Fallback = 1.0 (for missing, invalid or non-weight units)
 */
export const getKgFactor = (unit) => {
  if (!unit) return 1;
  const normalized = String(unit).trim().toLowerCase();
  if (normalized === "ton" || normalized === "tons" || normalized === "t") return 1000;
  if (normalized === "quintal" || normalized === "quintals" || normalized === "q") return 100;
  if (normalized === "kg" || normalized === "kgs" || normalized === "kilogram" || normalized === "kilograms") return 1;
  return 1;
};

/**
 * Converts quantity in source unit to quantity in KG.
 * @param {number} quantity 
 * @param {string} unit 
 * @returns {number}
 */
export const toKgQuantity = (quantity, unit) => {
  const q = Number(quantity || 0);
  return Number((q * getKgFactor(unit)).toFixed(2));
};

/**
 * Returns asking price per KG.
 * Price is specified per KG by farmers across all product listing units.
 * @param {number} price 
 * @param {string} unit 
 * @returns {number}
 */
export const toPricePerKg = (price, unit) => {
  return Number(price || 0);
};

/**
 * Formats a price value to display as price per KG.
 * @param {number} price 
 * @param {string} unit 
 * @returns {string}
 */
export const formatPricePerKg = (price, unit) => {
  const priceKg = toPricePerKg(price, unit);
  return new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(priceKg) + " / kg";
};
