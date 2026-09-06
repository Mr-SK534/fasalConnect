import { useCallback, useMemo, useState } from "react";
import { CartContext } from "./cartStore";

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
    const maxQuantity = Number(product.totalAvailableQuantity || 0);
    const requestedQuantity = Math.max(1, Number(quantity) || 1);

    updateCart((items) => {
      const existing = items.find((item) => item.productId === productId);
      const nextQuantity = Math.min(
        maxQuantity,
        (existing?.quantity || 0) + requestedQuantity,
      );

      if (existing) {
        return items.map((item) =>
          item.productId === productId
            ? {
                ...item,
                quantity: nextQuantity,
                maxQuantity,
                totalAvailableQuantity: Number(
                  product.totalAvailableQuantity || maxQuantity,
                ),
                farmerCount: Number(
                  product.farmerCount || item.farmerCount || 1,
                ),
                requiresBulk: nextQuantity > Number(product.quantity || 0),
              }
            : item,
        );
      }

      return [
        ...items,
        {
          productId,
          cropName: product.cropName,
          farmerName: product.farmerName,
          farmerLocation: product.farmerLocation,
          price: Number(product.price || 0),
          quantity: Math.min(maxQuantity, requestedQuantity),
          unit: product.unit,
          maxQuantity,
          totalAvailableQuantity: Number(
            product.totalAvailableQuantity || maxQuantity,
          ),
          farmerCount: Number(product.farmerCount || 1),
          requiresBulk: Boolean(product.requiresBulk),
          imageUrl: product.imageUrl,
        },
      ];
    });
  };

  const removeFromCart = (productId) => {
    updateCart((items) => items.filter((item) => item.productId !== productId));
  };

  const removeUnavailable = useCallback((availableProducts) => {
    const availableIds = new Set(
      availableProducts
        .filter(
          (product) =>
            product.isActive !== false && Number(product.quantity) > 0,
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
    setCartItems((items) => {
      const nextItems = items
        .map((item) => {
          const availability = availabilityByProductId[item.productId];
          if (!availability || availability.quantity <= 0) return null;
          return {
            ...item,
            maxQuantity: availability.totalAvailableQuantity,
            totalAvailableQuantity: availability.totalAvailableQuantity,
            farmerCount: availability.farmerCount,
            requiresBulk: item.quantity > availability.quantity,
            quantity: Math.min(
              item.quantity,
              availability.totalAvailableQuantity,
            ),
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
      items.map((item) =>
        item.productId === productId
          ? {
              ...item,
              quantity: Math.min(
                item.maxQuantity,
                Math.max(1, Number(quantity) || 1),
              ),
            }
          : item,
      ),
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
