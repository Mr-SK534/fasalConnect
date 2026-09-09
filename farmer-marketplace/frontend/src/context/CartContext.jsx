import { useCallback, useMemo, useState } from "react";
import { CartContext } from "./cartStore";
import { toPricePerKg, toKgQuantity } from "../utils/unitConverter";

const CART_STORAGE_KEY = "fasalconnect_cart";

const readStoredCart = () => {
  try {
    const stored = JSON.parse(localStorage.getItem(CART_STORAGE_KEY) || "[]");
    return Array.isArray(stored) ? stored : [];
  } catch {
    return [];
  }
};

const getProductId = (product) => product.productId || product.id;

export function computeEffectivePrice(item, qty) {
  const requested = Math.max(1, Number(qty) || 1);
  const primaryStock = Number(item.primaryStock || item.singleFarmerStock || 0);
  const primaryPrice = Number(item.primaryPrice || item.unitPrice || item.price || 0);

  if (!primaryStock || requested <= primaryStock) {
    return primaryPrice;
  }

  let remaining = requested;
  let totalCost = 0;

  // 1. Primary farmer portion
  const fromPrimary = Math.min(remaining, primaryStock);
  totalCost += fromPrimary * primaryPrice;
  remaining -= fromPrimary;

  // 2. Secondary suppliers if available
  const suppliers = Array.isArray(item.farmers)
    ? item.farmers.filter((f) => f.farmerId !== item.farmerId)
    : [];

  for (const supplier of suppliers) {
    if (remaining <= 0) break;
    const suppStock = Number(supplier.availableQuantity || supplier.quantity || 0);
    const suppPrice = Number(supplier.price || primaryPrice);
    const take = Math.min(remaining, suppStock > 0 ? suppStock : remaining);
    totalCost += take * suppPrice;
    remaining -= take;
  }

  // 3. Fallback for any remaining bulk portion using average price or primary price
  if (remaining > 0) {
    const avgPrice = Number(item.averagePrice || primaryPrice);
    totalCost += remaining * avgPrice;
  }

  return totalCost / requested;
}

export function CartProvider({ children }) {
  const [cartItems, setCartItems] = useState(readStoredCart);

  const updateCart = (updater) => {
    setCartItems((currentItems) => {
      const nextItems = updater(currentItems);
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(nextItems));
      return nextItems;
    });
  };

  const addToCart = (product, quantity) => {
    const productId = getProductId(product);
    const itemPriceKg = toPricePerKg(product.price, product.unit);
    const primaryPrice = Number(itemPriceKg || product.price || 0);
    const primaryStock = Number(product.quantityInKg || product.quantity || 0);
    const totalAvail = Number(
      product.totalAvailableQuantity || primaryStock || 99999
    );
    const maxQuantity = totalAvail > 0 ? totalAvail : 99999;
    const requestedQuantity = Math.max(1, Number(quantity) || 1);

    updateCart((items) => {
      const existing = items.find((item) => item.productId === productId);
      const currentQty = existing?.quantity || 0;
      const nextQuantity = Math.min(
        maxQuantity,
        currentQty + requestedQuantity
      );

      if (existing) {
        return items.map((item) => {
          if (item.productId !== productId) return item;
          const updatedQty = nextQuantity > 0 ? nextQuantity : requestedQuantity;
          const mergedItem = {
            ...item,
            quantity: updatedQty,
            maxQuantity,
            totalAvailableQuantity: maxQuantity,
            primaryStock: primaryStock > 0 ? primaryStock : item.primaryStock || 0,
            primaryPrice: primaryPrice > 0 ? primaryPrice : item.primaryPrice || item.price,
            farmerCount: Number(product.farmerCount || item.farmerCount || 1),
            farmers: product.farmers || item.farmers || [],
            averagePrice: Number(product.averagePrice || item.averagePrice || primaryPrice),
            requiresBulk: updatedQty > (primaryStock > 0 ? primaryStock : item.primaryStock || maxQuantity),
          };
          return {
            ...mergedItem,
            price: computeEffectivePrice(mergedItem, updatedQty),
          };
        });
      }

      const newItem = {
        productId,
        cropName: product.cropName,
        farmerId: product.farmerId,
        farmerName: product.farmerName,
        farmerLocation: product.farmerLocation,
        primaryPrice,
        primaryStock,
        price: primaryPrice,
        quantity: Math.min(maxQuantity, requestedQuantity),
        unit: "kg",
        maxQuantity,
        totalAvailableQuantity: maxQuantity,
        farmerCount: Number(product.farmerCount || 1),
        farmers: product.farmers || [],
        averagePrice: Number(product.averagePrice || primaryPrice),
        requiresBulk: Boolean(product.requiresBulk || (requestedQuantity > primaryStock && primaryStock > 0)),
        imageUrl: product.imageUrl,
      };

      newItem.price = computeEffectivePrice(newItem, newItem.quantity);
      return [...items, newItem];
    });
  };

  const removeFromCart = (productId) => {
    updateCart((items) => items.filter((item) => item.productId !== productId));
  };

  const removeUnavailable = useCallback((availableProducts) => {
    if (!Array.isArray(availableProducts) || !availableProducts.length) return;
    const availableIds = new Set(
      availableProducts
        .filter(
          (product) =>
            product.isActive !== false && Number(product.quantity || product.quantityInKg || 1) > 0,
        )
        .map((product) => product.id),
    );
    setCartItems((items) => {
      const nextItems = items.filter((item) =>
        availableIds.has(item.productId),
      );
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(nextItems));
      return nextItems;
    });
  }, []);

  const updateAvailability = useCallback((availabilityByProductId) => {
    if (!availabilityByProductId || !Object.keys(availabilityByProductId).length) return;
    setCartItems((items) => {
      const nextItems = items
        .map((item) => {
          const availability = availabilityByProductId[item.productId];
          if (!availability) return item;

          const availQty = Number(
            availability.totalAvailableQuantity || availability.quantity || item.maxQuantity || 99999
          );

          if (availability.isAvailable === false || (availability.quantity <= 0 && availability.totalAvailableQuantity <= 0)) {
            return null;
          }

          const safeMax = availQty > 0 ? availQty : item.maxQuantity || 99999;
          const updatedItem = {
            ...item,
            maxQuantity: safeMax,
            totalAvailableQuantity: safeMax,
            farmerCount: availability.farmerCount || item.farmerCount || 1,
            farmers: availability.farmers || item.farmers || [],
            averagePrice: Number(availability.averagePrice || item.averagePrice || item.primaryPrice || item.price),
            primaryStock: Number(availability.quantity || item.primaryStock || 0),
            requiresBulk: item.quantity > Number(availability.quantity || item.primaryStock || safeMax),
            quantity: Math.min(
              item.quantity,
              safeMax
            ),
          };
          return {
            ...updatedItem,
            price: computeEffectivePrice(updatedItem, updatedItem.quantity),
          };
        })
        .filter(Boolean);
      if (JSON.stringify(nextItems) === JSON.stringify(items)) return items;
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(nextItems));
      return nextItems;
    });
  }, []);

  const updateQuantity = (productId, quantity) => {
    updateCart((items) =>
      items.map((item) => {
        if (item.productId !== productId) return item;
        const maxQty = item.totalAvailableQuantity || item.maxQuantity || 99999;
        const newQty = Math.min(maxQty, Math.max(1, Number(quantity) || 1));
        const updatedItem = {
          ...item,
          quantity: newQty,
          requiresBulk: newQty > Number(item.primaryStock || item.singleFarmerStock || maxQty),
        };
        return {
          ...updatedItem,
          price: computeEffectivePrice(updatedItem, newQty),
        };
      }),
    );
  };

  const clearCart = () => {
    localStorage.removeItem(CART_STORAGE_KEY);
    setCartItems([]);
  };

  const cartTotal = useMemo(
    () =>
      cartItems.reduce((total, item) => total + item.price * item.quantity, 0),
    [cartItems],
  );
  const cartCount = useMemo(
    () => cartItems.reduce((count, item) => count + item.quantity, 0),
    [cartItems],
  );

  return (
    <CartContext.Provider
      value={{
        cartItems,
        addToCart,
        removeFromCart,
        removeUnavailable,
        updateAvailability,
        updateQuantity,
        clearCart,
        cartTotal,
        cartCount,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}
