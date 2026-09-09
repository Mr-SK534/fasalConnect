import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { FiShoppingCart } from "react-icons/fi";
import { toast } from "react-hot-toast";
import api from "../../services/api";
import { useCart } from "../../hooks/useCart";
import { toPricePerKg, toKgQuantity } from "../../utils/unitConverter";

const formatCurrency = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(value);

const formatDate = (value) =>
  value
    ? new Date(value).toLocaleDateString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
      })
    : "Not specified";

export default function BrowseProducts() {
  const navigate = useNavigate();
  const { addToCart, cartCount, cartItems, updateAvailability } = useCart();
  const [products, setProducts] = useState([]);
  const [quantities, setQuantities] = useState({});
  const [query, setQuery] = useState("");
  const [category, setCategory] = useState("All");
  const [sort, setSort] = useState("Recommended");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [aggregates, setAggregates] = useState({});

  const fetchProducts = useCallback(() => {
    return api
      .get("/products")
      .then(async (response) => {
        const nextProducts = Array.isArray(response.data)
          ? response.data.filter((product) => product.isActive !== false)
          : [];
        setProducts(nextProducts);
        const aggregateEntries = await Promise.all(
          nextProducts.map(async (product) => {
            const aggregateResponse = await api.get(
              `/products/aggregate/${encodeURIComponent(product.cropName)}`,
            );
            return [product.cropName.toLowerCase(), aggregateResponse.data];
          }),
        );
        const aggregateMap = Object.fromEntries(aggregateEntries);
        setAggregates(aggregateMap);
        const availabilityByProductId = Object.fromEntries(
          cartItems.map((item) => {
            const product = nextProducts.find(
              (candidate) => candidate.id === item.productId,
            );
            const cropKey = (item.cropName || product?.cropName || "").toLowerCase();
            const aggregate = cropKey ? aggregateMap[cropKey] : null;
            const productQty = Number(product?.quantityInKg || product?.quantity || 0);
            const aggQty = Number(aggregate?.totalAvailableQuantity || 0);
            const availableQty = productQty > 0 ? productQty : (aggQty > 0 ? aggQty : item.maxQuantity || 99999);
            const totalAvail = aggQty > 0 ? aggQty : availableQty;
            return [
              item.productId,
              {
                quantity: productQty > 0 ? productQty : availableQty,
                totalAvailableQuantity: totalAvail,
                farmerCount: Number(aggregate?.farmerCount || 1),
                farmers: aggregate?.farmers || [],
                averagePrice: Number(aggregate?.averagePrice || product?.price || 0),
                isAvailable: product ? product.isActive !== false : true,
              },
            ];
          }),
        );
        updateAvailability(availabilityByProductId);
      })
      .catch((requestError) =>
        setError(
          requestError.response?.data?.message || "Could not load products.",
        ),
      )
      .finally(() => setLoading(false));
  }, [updateAvailability]);

  useEffect(() => {
    fetchProducts();
    const handleFocus = () => fetchProducts();
    window.addEventListener("focus", handleFocus);
    window.addEventListener("products:refresh", handleFocus);
    return () => {
      window.removeEventListener("focus", handleFocus);
      window.removeEventListener("products:refresh", handleFocus);
    };
  }, [fetchProducts]);

  const categories = useMemo(
    () => ["All", ...new Set(products.map((product) => product.category))],
    [products],
  );

  const visibleProducts = useMemo(() => {
    const result = products.filter((product) => {
      const matchesCategory =
        category === "All" || product.category === category;
      const text =
        `${product.cropName} ${product.farmerName} ${product.farmerLocation || ""}`.toLowerCase();
      return matchesCategory && text.includes(query.toLowerCase());
    });

    if (sort === "Price: Low to High") result.sort((a, b) => a.price - b.price);
    if (sort === "Price: High to Low") result.sort((a, b) => b.price - a.price);
    return result;
  }, [category, products, query, sort]);

  const handleAdd = (product) => {
    const quantity = Number(quantities[product.id] || 1);
    const cropKey = (product.cropName || "").toLowerCase();
    const aggregate = aggregates[cropKey];
    const itemStock = Number(product.quantityInKg || product.quantity || 0);
    const totalAvail = Number(aggregate?.totalAvailableQuantity || itemStock || 99999);
    addToCart(
      {
        ...product,
        quantity: itemStock > 0 ? itemStock : 99999,
        quantityInKg: itemStock > 0 ? itemStock : 99999,
        totalAvailableQuantity: totalAvail,
        farmerCount: aggregate?.farmerCount || 1,
        farmers: aggregate?.farmers || [],
        averagePrice: Number(aggregate?.averagePrice || product.price || 0),
        requiresBulk: quantity > (itemStock > 0 ? itemStock : 99999),
      },
      quantity,
    );
    toast.success("Added to cart");
  };

  return (
    <main className="min-h-screen bg-slate-50 px-4 py-8 sm:px-8">
      <div className="mx-auto max-w-7xl">
        <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="mb-2 text-sm font-semibold uppercase tracking-wider text-green-600">
              Farmer marketplace
            </p>
            <h1 className="text-3xl font-bold text-slate-900">
              Browse fresh products
            </h1>
            <p className="mt-2 text-slate-500">
              Buy directly from trusted local farmers.
            </p>
          </div>
          <button
            type="button"
            onClick={() => navigate("/buyer/cart")}
            className="relative inline-flex items-center gap-2 rounded-lg bg-green-600 px-4 py-2.5 font-semibold text-white hover:bg-green-700"
          >
            <FiShoppingCart /> Cart
            {cartCount > 0 && (
              <span className="rounded-full bg-white px-2 py-0.5 text-xs font-bold text-green-700">
                {cartCount}
              </span>
            )}
          </button>
        </div>

        <section className="mb-8 rounded-xl bg-white p-4 shadow-sm">
          <div className="flex flex-col gap-3 md:flex-row">
            <input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Search products, farmers or locations..."
              className="flex-1 rounded-lg border border-slate-200 px-4 py-3 outline-none focus:border-green-500"
            />
            <select
              value={category}
              onChange={(event) => setCategory(event.target.value)}
              className="rounded-lg border border-slate-200 px-4 py-3 outline-none focus:border-green-500"
            >
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
            <select
              value={sort}
              onChange={(event) => setSort(event.target.value)}
              className="rounded-lg border border-slate-200 px-4 py-3 outline-none focus:border-green-500"
            >
              {["Recommended", "Price: Low to High", "Price: High to Low"].map(
                (item) => (
                  <option key={item}>{item}</option>
                ),
              )}
            </select>
          </div>
        </section>

        {loading && (
          <p className="py-16 text-center text-slate-500">
            Loading products...
          </p>
        )}
        {!loading && error && (
          <p className="rounded-lg bg-red-50 p-4 text-red-700">{error}</p>
        )}
        {!loading && !error && (
          <>
            <p className="mb-4 text-sm text-slate-500">
              {visibleProducts.length} products available
            </p>
            {visibleProducts.length ? (
              <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {visibleProducts.map((product) => (
                  <article
                    key={product.id}
                    className="overflow-hidden rounded-xl bg-white shadow-sm transition hover:-translate-y-1 hover:shadow-md"
                  >
                    <div className="h-40 overflow-hidden bg-green-50">
                      {product.imageUrl ? (
                        <img
                          src={product.imageUrl}
                          alt={product.cropName}
                          className="h-full w-full object-cover"
                        />
                      ) : (
                        <div className="flex h-full items-center justify-center text-6xl">
                          🌾
                        </div>
                      )}
                    </div>
                    <div className="p-5">
                      <h2 className="font-bold text-slate-900">
                        {product.cropName}
                      </h2>
                      <p className="mt-1 text-sm text-slate-500">
                        By {product.farmerName || "Local farmer"}
                      </p>
                      <p className="text-sm text-slate-500">
                        {product.farmerLocation ||
                          product.region ||
                          "Location not specified"}
                      </p>
                      <div className="mt-4 space-y-1 text-sm text-slate-600">
                        <p>
                          <strong className="text-green-700">
                            {formatCurrency(toPricePerKg(product.price, product.unit))}
                          </strong>{" "}
                          / kg
                        </p>
                        <p>
                          {toKgQuantity(product.quantity, product.unit)} kg available
                        </p>
                        <p className="text-xs text-slate-500">
                          Total available across all farmers:{" "}
                          {aggregates[product.cropName.toLowerCase()]
                            ?.totalAvailableQuantity || toKgQuantity(product.quantity, product.unit)}{" "}
                          kg
                        </p>
                        <p>Harvest: {formatDate(product.harvestDate)}</p>
                      </div>
                      <div className="mt-5 flex items-center gap-2">
                        <input
                          type="number"
                          min="1"
                          value={quantities[product.id] || 1}
                          onChange={(event) =>
                            setQuantities({
                              ...quantities,
                              [product.id]: event.target.value,
                            })
                          }
                          className="w-20 rounded-lg border border-slate-200 px-3 py-2 text-sm"
                          aria-label={`Quantity for ${product.cropName}`}
                        />
                        <div className="mt-1 text-xs">
                          {Number(quantities[product.id] || 1) <=
                          Number(product.quantity) ? (
                            <span className="rounded bg-green-50 px-2 py-0.5 font-medium text-green-600">
                              ✓ Available from 1 farmer
                            </span>
                          ) : Number(quantities[product.id] || 1) <=
                            Number(
                              aggregates[product.cropName.toLowerCase()]
                                ?.totalAvailableQuantity || 0,
                            ) ? (
                            <span className="rounded bg-blue-50 px-2 py-0.5 font-medium text-blue-600">
                              ℹ️ Will be sourced from{" "}
                              {aggregates[product.cropName.toLowerCase()]
                                ?.farmerCount || 0}{" "}
                              farmers (bulk order)
                            </span>
                          ) : (
                            <span className="rounded bg-red-50 px-2 py-0.5 font-medium text-red-600">
                              ⚠️ Only{" "}
                              {aggregates[product.cropName.toLowerCase()]
                                ?.totalAvailableQuantity || 0}{" "}
                              kg available across all farmers
                            </span>
                          )}
                        </div>
                        {Number(product.quantity) > 0 ? (
                          <button
                            type="button"
                            disabled={
                              Number(quantities[product.id] || 1) >
                              Number(
                                aggregates[product.cropName.toLowerCase()]
                                  ?.totalAvailableQuantity || product.quantity,
                              )
                            }
                            onClick={() => handleAdd(product)}
                            className="flex-1 rounded-lg bg-green-600 px-3 py-2 text-sm font-semibold text-white hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            Add to Cart
                          </button>
                        ) : (
                          <span className="rounded-full bg-red-100 px-2 py-1 text-xs font-semibold text-red-700">
                            Out of Stock
                          </span>
                        )}
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            ) : (
              <div className="rounded-xl bg-white py-16 text-center text-slate-500">
                No products found.
              </div>
            )}
          </>
        )}
      </div>
    </main>
  );
}
