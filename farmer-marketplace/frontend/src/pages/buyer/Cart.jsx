import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  FiArrowLeft,
  FiMinus,
  FiPlus,
  FiShoppingBag,
  FiTrash2,
} from "react-icons/fi";
import { useCart } from "../../hooks/useCart";
import api from "../../services/api";
import { toast } from "react-hot-toast";

const formatCurrency = (value) =>
  new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(
    value,
  );

export default function Cart() {
  const navigate = useNavigate();
  const {
    cartItems,
    cartTotal,
    updateQuantity,
    removeFromCart,
    updateAvailability,
  } = useCart();
  const [stockWarnings, setStockWarnings] = useState({});

  useEffect(() => {
    const refreshAvailability = async () => {
      if (!cartItems.length) return;
      const productsResponse = await api.get("/products");
      const products = productsResponse.data || [];
      const aggregateEntries = await Promise.all(
        [...new Set(cartItems.map((item) => item.cropName))].map(
          async (cropName) => {
            const aggregate = await api.get(
              `/products/aggregate/${encodeURIComponent(cropName)}`,
            );
            return [cropName, aggregate.data];
          },
        ),
      );
      const aggregates = Object.fromEntries(aggregateEntries);

      const byProductId = Object.fromEntries(
        cartItems.map((item) => {
          const product = products.find((p) => p.id === item.productId);
          const aggregate = aggregates[item.cropName];
          return [
            item.productId,
            {
              quantity: Number(product?.quantity || 0),
              totalAvailableQuantity: Number(
                aggregate?.totalAvailableQuantity || 0,
              ),
              farmerCount: Number(aggregate?.farmerCount || 0),
            },
          ];
        }),
      );

      setStockWarnings(
        Object.fromEntries(
          cartItems
            .filter(
              (item) =>
                Number(
                  byProductId[item.productId]?.totalAvailableQuantity || 0,
                ) < item.quantity,
            )
            .map((item) => [
              item.productId,
              byProductId[item.productId]?.totalAvailableQuantity || 0,
            ]),
        ),
      );

      updateAvailability(byProductId);
      if (Object.values(byProductId).some((item) => item.quantity < 1))
        toast("Some items in your cart are no longer available");
    };
    refreshAvailability().catch((error) =>
      toast.error(
        error.response?.data?.message ||
          "Could not refresh stock availability.",
      ),
    );
  }, [cartItems, updateAvailability]);

  return (
    <main className="min-h-screen bg-[#f8f9f4] px-4 py-8 text-slate-700">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-green-600">
              Marketplace
            </p>
            <h1 className="mt-2 text-3xl font-bold text-slate-800">
              Your Cart
            </h1>
          </div>
          <Link
            to="/buyer/browse"
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-green-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 shadow-sm hover:border-green-400"
          >
            <FiArrowLeft size={16} /> Continue Shopping
          </Link>
        </div>

        {!cartItems.length ? (
          <div className="rounded-3xl border border-dashed border-green-200 bg-white px-6 py-12 text-center shadow-sm">
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-50 text-green-700">
              <FiShoppingBag size={28} />
            </div>
            <h2 className="mt-6 text-2xl font-semibold text-slate-800">
              Your cart is empty
            </h2>
            <p className="mt-2 text-slate-500">
              Add fresh produce to continue.
            </p>
            <Link
              to="/buyer/browse"
              className="mt-6 inline-flex rounded-lg bg-green-600 px-5 py-3 text-sm font-semibold text-white hover:bg-green-700"
            >
              Browse Products
            </Link>
          </div>
        ) : (
          <div className="grid gap-6 lg:grid-cols-[1.7fr_0.9fr]">
            <div className="space-y-4">
              {cartItems.map((item) => (
                <div
                  key={item.productId}
                  className="rounded-2xl border border-green-100 bg-white p-4 shadow-sm sm:p-5"
                >
                  <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                    <div className="h-24 w-full overflow-hidden rounded-xl bg-green-50 sm:w-24">
                      {item.imageUrl ? (
                        <img
                          src={item.imageUrl}
                          alt={item.cropName}
                          className="h-full w-full object-cover"
                        />
                      ) : (
                        <div className="flex h-full items-center justify-center text-3xl">
                          🌾
                        </div>
                      )}
                    </div>
                    <div className="flex-1">
                      <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                        <div>
                          <h3 className="text-lg font-semibold text-slate-800">
                            {item.cropName}
                          </h3>
                          <p className="text-sm text-slate-500">
                            {item.farmerName || "Local farmer"}
                          </p>
                        </div>
                        {stockWarnings[item.productId] !== undefined && (
                          <span className="mb-1 block rounded bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-600">
                            Stock reduced - only {stockWarnings[item.productId]}{" "}
                            available now
                          </span>
                        )}
                        {item.requiresBulk && (
                          <span className="mb-1 block w-fit rounded-full bg-purple-50 px-2 py-0.5 text-xs font-semibold text-purple-700">
                            Multi-farmer
                          </span>
                        )}
                        <p className="text-xl font-bold text-green-700">
                          {formatCurrency(item.price * item.quantity)}
                        </p>
                      </div>
                      <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
                        <div className="flex items-center gap-3">
                          <button
                            type="button"
                            onClick={() =>
                              updateQuantity(item.productId, item.quantity - 1)
                            }
                            disabled={item.quantity <= 1}
                            className="flex h-9 w-9 items-center justify-center rounded-full border border-green-200 bg-green-50 text-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            <FiMinus size={16} />
                          </button>
                          <span className="min-w-[2rem] text-center font-semibold">
                            {item.quantity}
                          </span>
                          <button
                            type="button"
                            onClick={() =>
                              updateQuantity(item.productId, item.quantity + 1)
                            }
                            disabled={item.quantity >= item.maxQuantity}
                            className="flex h-9 w-9 items-center justify-center rounded-full border border-green-200 bg-green-50 text-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            <FiPlus size={16} />
                          </button>
                          <span className="text-xs text-slate-500">
                            of {item.maxQuantity} {item.unit}
                          </span>
                        </div>
                        <button
                          type="button"
                          onClick={() => removeFromCart(item.productId)}
                          className="inline-flex items-center gap-2 rounded-lg bg-red-50 px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-100"
                        >
                          <FiTrash2 size={14} /> Remove
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <aside className="h-fit rounded-2xl border border-green-100 bg-white p-5 shadow-sm">
              <h2 className="text-xl font-bold text-slate-800">
                Order Summary
              </h2>
              <div className="mt-5 space-y-4 text-sm text-slate-600">
                <div className="flex items-center justify-between">
                  <span>Items</span>
                  <span>{cartItems.length}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>Total</span>
                  <span className="text-lg font-bold text-green-700">
                    {formatCurrency(cartTotal)}
                  </span>
                </div>
              </div>
              <button
                type="button"
                onClick={() => navigate("/buyer/checkout")}
                className="mt-6 w-full rounded-lg bg-green-600 px-4 py-3 text-sm font-semibold text-white hover:bg-green-700"
              >
                Proceed to Checkout
              </button>
            </aside>
          </div>
        )}
      </div>
    </main>
  );
}
