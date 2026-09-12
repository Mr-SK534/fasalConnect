import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { useCart } from "../../hooks/useCart";
import { placeOrder } from "../../services/orderService";
import { createRazorpayOrder, confirmPayment, failPayment } from "../../services/paymentService";
import api from "../../services/api";

const formatCurrency = (value) =>
  new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(
    value,
  );

export default function Checkout() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { cartItems, cartTotal, clearCart } = useCart();
  const [isBulkOrder, setIsBulkOrder] = useState(false);
  const [deliveryType, setDeliveryType] = useState("Delivery");
  const [deliveryAddress, setDeliveryAddress] = useState(
    user?.deliveryAddress || "",
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");
  const requiresBulk = cartItems.some(
    (item) => item.requiresBulk || item.quantity > item.maxQuantity,
  );

  useEffect(() => {
    // Keep bulk mode synchronized with current cart availability.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    if (requiresBulk) setIsBulkOrder(true);
  }, [requiresBulk]);

  const submit = async (event) => {
    event.preventDefault();
    if (!cartItems.length) return;
    setError("");
    setIsSubmitting(true);

    let createdOrderId = null;

    try {
      console.log("[Checkout] validating aggregate stock");
      const aggregateEntries = await Promise.all(
        [...new Set(cartItems.map((item) => item.cropName).filter(Boolean))].map(
          async (cropName) => {
            try {
              const response = await api.get(
                `/products/aggregate/${encodeURIComponent(cropName)}`,
              );
              return [cropName.toLowerCase(), response.data];
            } catch {
              return [cropName.toLowerCase(), null];
            }
          },
        ),
      );
      const aggregates = Object.fromEntries(aggregateEntries);
      const unavailable = cartItems.find((item) => {
        const cropKey = (item.cropName || "").toLowerCase();
        const agg = aggregates[cropKey] || aggregates[item.cropName];
        const aggStock = Number(agg?.totalAvailableQuantity || 0);
        const availStock = aggStock > 0 ? aggStock : Number(item.maxQuantity || item.totalAvailableQuantity || item.quantity || 99999);
        return availStock < item.quantity;
      });
      if (unavailable) {
        const cropKey = (unavailable.cropName || "").toLowerCase();
        const agg = aggregates[cropKey] || aggregates[unavailable.cropName];
        const availStock = Number(agg?.totalAvailableQuantity || unavailable.maxQuantity || unavailable.quantity || 0);
        throw new Error(
          `Stock changed for ${unavailable.cropName}. Only ${availStock} kg is available.`,
        );
      }
      if (requiresBulk && !isBulkOrder) {
        throw new Error(
          "Some items require bulk sourcing. Please enable bulk order or reduce quantity.",
        );
      }
      console.log("[Checkout] placing internal order");
      const order = await placeOrder({
        items: cartItems.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
        isBulkOrder,
        deliveryType,
        ...(deliveryType === "Delivery" ? { deliveryAddress } : {}),
      });
      createdOrderId = order.id;
      console.log("[Checkout] internal order created", order.id);
      console.log("[Checkout] creating Razorpay order");
      const razorpayOrder = await createRazorpayOrder(
        order.id,
        order.totalAmount,
      );
      console.log(
        "[Checkout] Razorpay order created",
        razorpayOrder.razorpayOrderId,
      );

      if (!window.Razorpay) {
        throw new Error(
          "Razorpay checkout is unavailable. Please refresh and try again.",
        );
      }

      const options = {
        key: razorpayOrder.keyId || razorpayOrder.key || import.meta.env.VITE_RAZORPAY_KEY_ID,
        amount: Number(razorpayOrder.amount) * 100,
        currency: razorpayOrder.currency,
        order_id: razorpayOrder.razorpayOrderId,
        name: "FasalConnect",
        description: "Fresh produce order",
        handler: async (response) => {
          console.log("[Checkout] payment succeeded", response);
          try {
            await confirmPayment({
              orderId: order.id,
              razorpayPaymentId: response?.razorpay_payment_id,
              razorpayOrderId: response?.razorpay_order_id,
              razorpaySignature: response?.razorpay_signature,
            });
          } catch (err) {
            console.error("[Checkout] Failed to confirm payment on backend", err);
          }
          clearCart();
          window.dispatchEvent(new Event("products:refresh"));
          navigate("/buyer/orders");
        },
        prefill: {
          name: user?.name || "",
          contact: user?.phone || "",
        },
        theme: { color: "#16a34a" },
        modal: {
          ondismiss: async () => {
            console.log("[Checkout] payment modal dismissed");
            if (createdOrderId) {
              try {
                await failPayment({ orderId: createdOrderId, reason: "Modal dismissed" });
              } catch (e) {
                console.error("Error syncing order cancellation on dismiss", e);
              }
            }
            setIsSubmitting(false);
          },
        },
      };

      new window.Razorpay(options).open();
    } catch (requestError) {
      console.error("[Checkout] payment flow failed", requestError);
      if (createdOrderId) {
        try {
          await failPayment({ orderId: createdOrderId, reason: requestError.message });
        } catch (e) {
          console.error("Error syncing order cancellation on failure", e);
        }
      }
      const backendMessage =
        requestError.response?.data?.message ||
        requestError.message ||
        "Unknown error";
      setError(
        requestError.config?.url?.includes("/payments/")
          ? `Failed to initiate payment: ${backendMessage}`
          : requestError.config?.url?.includes("/orders")
            ? `Failed to place order: ${backendMessage}`
            : backendMessage,
      );
      setIsSubmitting(false);
    }
  };

  if (!cartItems.length) {
    return (
      <main className="mx-auto max-w-3xl px-4 py-16 text-center">
        <div className="rounded-2xl bg-green-50 p-10">
          <h1 className="text-3xl font-bold text-gray-900">
            Your cart is empty
          </h1>
          <p className="mt-2 text-gray-600">
            Add products before starting checkout.
          </p>
          <Link
            to="/buyer/browse"
            className="mt-6 inline-block rounded-lg bg-green-600 px-6 py-3 font-medium text-white hover:bg-green-700"
          >
            Browse Products
          </Link>
        </div>
      </main>
    );
  }

  return (
    <main className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-gray-900">Checkout</h1>
      {error && (
        <p className="mb-6 rounded-lg bg-red-50 p-4 text-sm font-medium text-red-700">
          {error}
        </p>
      )}
      <form onSubmit={submit} className="grid gap-8 lg:grid-cols-[1fr_380px]">
        <section className="space-y-6">
          <div className="rounded-xl border bg-white p-6 shadow-sm">
            <h2 className="mb-5 text-xl font-semibold">Order preferences</h2>
            <label className="flex cursor-pointer items-start gap-3 rounded-lg border p-3">
              <input
                type="checkbox"
                checked={isBulkOrder}
                onChange={(event) => setIsBulkOrder(event.target.checked)}
                className="mt-1 accent-green-600"
              />
              <span>
                <strong className="block">This is a bulk order</strong>
                <span className="text-sm text-gray-500">
                  We'll automatically source from multiple farmers if one
                  farmer's stock is insufficient
                </span>
                <span
                  className="ml-2 text-xs text-slate-400"
                  title="Bulk order automatically combines stock from multiple farmers to fulfill your quantity"
                >
                  ⓘ
                </span>
              </span>
            </label>
            {requiresBulk && !isBulkOrder && (
              <p className="mt-2 rounded bg-amber-50 px-2 py-1 text-xs font-medium text-amber-600">
                Some items require bulk sourcing. Please enable bulk order or
                reduce quantity.
              </p>
            )}
            <div className="mt-5 grid gap-3 sm:grid-cols-2">
              {["Delivery", "Pickup"].map((type) => (
                <label
                  key={type}
                  className="flex cursor-pointer items-center gap-3 rounded-lg border p-3"
                >
                  <input
                    type="radio"
                    name="deliveryType"
                    value={type}
                    checked={deliveryType === type}
                    onChange={(event) => setDeliveryType(event.target.value)}
                    className="accent-green-600"
                  />
                  {type}
                </label>
              ))}
            </div>
            {deliveryType === "Delivery" && (
              <label className="mt-5 block text-sm font-medium text-gray-700">
                Delivery address
                <textarea
                  required
                  value={deliveryAddress}
                  onChange={(event) => setDeliveryAddress(event.target.value)}
                  rows="4"
                  className="mt-1 w-full rounded-lg border px-3 py-2 outline-none focus:border-green-600"
                  placeholder="Enter the address for delivery"
                />
              </label>
            )}
          </div>
        </section>

        <aside className="h-fit rounded-xl border bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Order summary</h2>
          <div className="space-y-3 border-b pb-4">
            {cartItems.map((item) => (
              <div
                key={item.productId}
                className="flex justify-between gap-3 text-sm"
              >
                <span>
                  {item.cropName} × {item.quantity}
                  {item.requiresBulk && (
                    <small className="ml-2 text-purple-700">
                      - sourced from {item.farmerCount || 2} farmers
                    </small>
                  )}
                </span>
                <span>{formatCurrency(item.price * item.quantity)}</span>
              </div>
            ))}
          </div>
          <div className="mt-4 flex justify-between border-t pt-3 text-lg font-bold">
            <span>Total</span>
            <span className="text-green-700">{formatCurrency(cartTotal)}</span>
          </div>
          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-6 flex w-full items-center justify-center gap-2 rounded-lg bg-green-600 px-4 py-3 font-semibold text-white hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isSubmitting && (
              <span className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
            )}
            {isSubmitting ? "Starting payment..." : "Pay securely"}
          </button>
        </aside>
      </form>
    </main>
  );
}
