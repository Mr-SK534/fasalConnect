import React, { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { FiArrowLeft, FiMinus, FiPlus, FiShoppingBag, FiTrash2 } from 'react-icons/fi';
import { toast } from 'react-hot-toast';
import api from '../../services/api';

const defaultImage =
  'https://images.unsplash.com/photo-1542838132-92c53300491e?auto=format&fit=crop&w=900&q=80';

const getCartItems = (payload) => {
  if (!payload) return [];

  if (Array.isArray(payload)) return payload;

  if (Array.isArray(payload.items)) return payload.items;
  if (Array.isArray(payload.cart)) return payload.cart;
  if (Array.isArray(payload.data?.items)) return payload.data.items;
  if (Array.isArray(payload.data?.cart)) return payload.data.cart;

  return [];
};

const Cart = () => {
  const navigate = useNavigate();
  const [cart, setCart] = useState([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  const fetchCart = async () => {
    try {
      setLoading(true);
      const response = await api.get('/cart');
      setCart(getCartItems(response.data));
    } catch (error) {
      console.error('Error fetching cart:', error);
      setCart([]);
      toast.error(error.response?.data?.message || 'Failed to load your cart');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCart();
  }, []);

  const subtotal = useMemo(
    () =>
      cart.reduce((sum, item) => {
        const product = item.product || {};
        const unitPrice = Number(item.price ?? product.price ?? 0);
        const quantity = Number(item.quantity ?? 1);
        return sum + unitPrice * quantity;
      }, 0),
    [cart]
  );

  const shipping = cart.length ? 50 : 0;
  const total = subtotal + shipping;

  const updateQuantity = async (itemId, nextQuantity) => {
    if (!itemId || nextQuantity < 1) return;

    try {
      setProcessing(true);
      const response = await api.put(`/cart/items/${itemId}`, { quantity: nextQuantity });
      const updatedQuantity = response.data?.quantity ?? nextQuantity;

      setCart((prev) =>
        prev.map((item) =>
          item._id === itemId ? { ...item, quantity: updatedQuantity } : item
        )
      );
    } catch (error) {
      console.error('Error updating quantity:', error);
      toast.error(error.response?.data?.message || 'Could not update quantity');
    } finally {
      setProcessing(false);
    }
  };

  const removeFromCart = async (itemId) => {
    try {
      setProcessing(true);
      await api.delete(`/cart/items/${itemId}`);
      setCart((prev) => prev.filter((item) => item._id !== itemId));
      toast.success('Item removed from cart');
    } catch (error) {
      console.error('Error removing item:', error);
      toast.error(error.response?.data?.message || 'Could not remove item');
    } finally {
      setProcessing(false);
    }
  };

  const handleCheckout = () => {
    if (!cart.length) {
      toast.error('Your cart is empty');
      return;
    }

    navigate('/buyer/checkout');
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#f8f9f4] px-4 py-8 text-slate-700">
        <div className="mx-auto max-w-6xl">
          <div className="animate-pulse rounded-3xl border border-[#edf0e7] bg-white p-6 shadow-sm">
            <div className="mb-4 h-8 w-36 rounded bg-slate-200" />
            <div className="space-y-4">
              <div className="h-28 rounded-xl bg-slate-200" />
              <div className="h-28 rounded-xl bg-slate-200" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#f8f9f4] px-4 py-8 text-slate-700">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-[#5b7c43]">
              Marketplace
            </p>
            <h1 className="mt-2 text-3xl font-bold text-slate-800">Your Cart</h1>
          </div>

          <Link
            to="/buyer/products"
            className="inline-flex items-center justify-center gap-2 rounded-full border border-[#dfe8d7] bg-white px-4 py-2.5 text-sm font-medium text-slate-700 shadow-sm transition hover:border-[#b8d3a5] hover:text-[#3e5d2a]"
          >
            <FiArrowLeft size={16} />
            Continue Shopping
          </Link>
        </div>

        {!cart.length ? (
          <div className="rounded-3xl border border-dashed border-[#dfe8d7] bg-white px-6 py-12 text-center shadow-sm">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-[#edf5e8] text-[#5b7c43]">
              <FiShoppingBag size={28} />
            </div>
            <h2 className="mt-6 text-2xl font-semibold text-slate-800">Your cart is empty</h2>
            <p className="mt-2 text-slate-500">Add fresh produce and essentials to continue.</p>
            <Link
              to="/buyer/products"
              className="mt-6 inline-flex items-center rounded-full bg-[#5b7c43] px-5 py-3 text-sm font-semibold text-white transition hover:bg-[#496a36]"
            >
              Browse Products
            </Link>
          </div>
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1.7fr_0.9fr]">
            <div className="space-y-4">
              {cart.map((item) => {
                const product = item.product || {};
                const productId = item._id || product._id || item.productId || item.id;
                const unitPrice = Number(item.price ?? product.price ?? 0);
                const quantity = Number(item.quantity ?? 1);
                const itemTotal = unitPrice * quantity;

                return (
                  <div
                    key={productId}
                    className="rounded-2xl border border-[#edf0e7] bg-white p-4 shadow-sm sm:p-5"
                  >
                    <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                      <div className="h-24 w-full overflow-hidden rounded-xl bg-[#f3f6ef] sm:w-24">
                        <img
                          src={product.image || defaultImage}
                          alt={product.name || 'Product image'}
                          className="h-full w-full object-cover"
                          onError={(e) => {
                            e.target.onerror = null;
                            e.target.src = defaultImage;
                          }}
                        />
                      </div>

                      <div className="flex-1">
                        <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                          <div>
                            <h3 className="text-lg font-semibold text-slate-800">
                              {product.name || 'Fresh product'}
                            </h3>
                            <p className="text-sm text-slate-500">
                              {product.farmer?.name || product.vendor?.name || 'Local farmer'}
                            </p>
                          </div>

                          <div className="text-right">
                            <p className="text-xl font-bold text-[#3e5d2a]">₹{itemTotal.toFixed(2)}</p>
                            <p className="text-xs text-slate-400">Qty: {quantity}</p>
                          </div>
                        </div>

                        <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                          <div className="flex items-center gap-3">
                            <button
                              type="button"
                              onClick={() => updateQuantity(productId, Math.max(1, quantity - 1))}
                              disabled={processing || quantity <= 1}
                              className="flex h-9 w-9 items-center justify-center rounded-full border border-[#dfe8d7] bg-[#f9fbf7] text-slate-700 transition hover:border-[#b8d3a5] hover:text-[#3e5d2a] disabled:cursor-not-allowed disabled:opacity-50"
                            >
                              <FiMinus size={16} />
                            </button>

                            <span className="min-w-[2rem] text-center text-base font-semibold text-slate-800">
                              {quantity}
                            </span>

                            <button
                              type="button"
                              onClick={() => updateQuantity(productId, quantity + 1)}
                              disabled={processing}
                              className="flex h-9 w-9 items-center justify-center rounded-full border border-[#dfe8d7] bg-[#f9fbf7] text-slate-700 transition hover:border-[#b8d3a5] hover:text-[#3e5d2a] disabled:cursor-not-allowed disabled:opacity-50"
                            >
                              <FiPlus size={16} />
                            </button>
                          </div>

                          <div className="flex items-center gap-3">
                            <p className="text-sm text-slate-500">₹{unitPrice.toFixed(2)} each</p>
                            <button
                              type="button"
                              onClick={() => removeFromCart(productId)}
                              className="inline-flex items-center gap-2 rounded-full bg-red-50 px-3 py-2 text-sm font-medium text-red-600 transition hover:bg-red-100"
                            >
                              <FiTrash2 size={14} />
                              Remove
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>

            <aside className="h-fit rounded-2xl border border-[#edf0e7] bg-white p-5 shadow-sm">
              <h2 className="text-xl font-bold text-slate-800">Order Summary</h2>

              <div className="mt-5 space-y-4 text-sm text-slate-600">
                <div className="flex items-center justify-between">
                  <span>Subtotal</span>
                  <span>₹{subtotal.toFixed(2)}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>Delivery</span>
                  <span>₹{shipping.toFixed(2)}</span>
                </div>
                <div className="my-3 h-px bg-[#edf0e7]" />
                <div className="flex items-center justify-between text-base font-bold text-slate-800">
                  <span>Total</span>
                  <span>₹{total.toFixed(2)}</span>
                </div>
              </div>

              <button
                type="button"
                onClick={handleCheckout}
                disabled={!cart.length || processing}
                className="mt-6 w-full rounded-full bg-[#5b7c43] px-4 py-3 text-sm font-semibold text-white transition hover:bg-[#496a36] disabled:cursor-not-allowed disabled:opacity-70"
              >
                Proceed to Checkout
              </button>
            </aside>
          </div>
        )}
      </div>
    </div>
  );
};

export default Cart;
