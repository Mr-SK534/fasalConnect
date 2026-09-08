import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { toast } from "react-hot-toast";
import { useAuth } from "../../hooks/useAuth";
import api from "../../services/api";

export default function FarmerDashboard() {
  const { user } = useAuth();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState(null);

  useEffect(() => {
    const loadProducts = async () => {
      try {
        const response = await api.get(`/products/farmer/${user.id}`);
        setProducts(Array.isArray(response.data) ? response.data : []);
      } catch {
        setProducts([]);
      } finally {
        setLoading(false);
      }
    };
    if (user?.id) loadProducts();
  }, [user?.id]);

  const removeProduct = async (product) => {
    if (!window.confirm(`Remove ${product.cropName} from your listings?`))
      return;

    setDeletingId(product.id);
    try {
      await api.delete(`/products/${product.id}`);
      setProducts((currentProducts) =>
        currentProducts.filter((item) => item.id !== product.id),
      );
      toast.success("Product removed");
    } catch (error) {
      toast.error(
        error.response?.data?.message || "Could not remove this product.",
      );
    } finally {
      setDeletingId(null);
    }
  };

  const activeProducts = products.filter(
    (product) => product.isActive !== false,
  ).length;
  const inventory = products.reduce(
    (total, product) => total + Number(product.quantity || 0),
    0,
  );

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      {/* Header Banner */}
      <section className="rounded-2xl bg-gradient-to-r from-[#2e7d32] via-[#246b28] to-[#1b5e20] p-6 text-white shadow-md sm:p-8">
        <p className="text-xs font-extrabold uppercase tracking-wider text-[#f5d77f]">
          Good to see you, {user?.name || "Farmer"} 👋
        </p>
        <h2 className="mt-1 text-2xl font-black sm:text-3xl tracking-tight">
          Your Farm Marketplace
        </h2>
        <p className="mt-2 max-w-2xl text-sm leading-relaxed text-emerald-100 font-normal">
          Keep your produce visible, your inventory current, and your buyers
          close.
        </p>
      </section>

      {/* Overview Metric Cards */}
      <section className="grid gap-4 sm:grid-cols-3">
        {[
          ["Listed products", products.length, "Your catalogue"],
          ["Active listings", activeProducts, "Visible to buyers"],
          ["Total inventory", inventory, "Units listed"],
        ].map(([label, value, hint]) => (
          <article
            key={label}
            className="rounded-2xl border border-[#eadaaf] bg-white/90 p-5 shadow-xs backdrop-blur transition hover:shadow-md"
          >
            <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">
              {label}
            </p>
            <p className="mt-2 text-3xl font-black text-[#1b5e20]">
              {loading ? "..." : value}
            </p>
            <p className="mt-1 text-xs font-semibold text-slate-500">{hint}</p>
          </article>
        ))}
      </section>

      {/* Recent Listings List */}
      <section className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-xs sm:p-6">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-lg font-bold text-[#163820]">
              Recent Listings
            </h3>
            <p className="mt-0.5 text-xs text-slate-500">
              Products connected to your account.
            </p>
          </div>
          <NavLink
            to="/farmer/list-product"
            className="rounded-xl bg-[#f5d77f] px-4 py-2 text-xs font-bold text-[#1b5e20] shadow-2xs transition hover:bg-[#eac459] hover:shadow-xs"
          >
            + Add Product
          </NavLink>
        </div>

        {products.length ? (
          <div className="mt-5 divide-y divide-[#eee5cc]">
            {products.slice(0, 5).map((product) => (
              <div
                key={product.id}
                className="flex flex-wrap items-center justify-between gap-4 py-3.5"
              >
                <div className="flex items-center gap-3.5">
                  {product.imageUrl ? (
                    <img
                      src={product.imageUrl}
                      alt=""
                      className="h-11 w-11 rounded-xl object-cover ring-1 ring-[#eadaaf]"
                    />
                  ) : (
                    <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-[#f4e8c5] text-lg ring-1 ring-[#eadaaf]">
                      🌾
                    </div>
                  )}
                  <div>
                    <p className="text-sm font-bold text-slate-900">
                      {product.cropName}
                    </p>
                    <p className="text-xs font-medium text-slate-500">
                      {product.category} · {product.quantity} {product.unit}
                    </p>
                    {product.description && (
                      <p className="mt-0.5 max-w-xl text-xs text-slate-600 line-clamp-1">
                        {product.description}
                      </p>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <p className="text-base font-extrabold text-[#1b5e20]">
                    ₹{Number(product.price || 0).toLocaleString("en-IN")}
                  </p>
                  <button
                    type="button"
                    disabled={deletingId === product.id}
                    onClick={() => removeProduct(product)}
                    className="rounded-lg bg-red-50 px-3 py-2 text-xs font-bold text-red-700 transition hover:bg-red-100 disabled:opacity-60"
                  >
                    {deletingId === product.id ? "Removing..." : "Remove"}
                  </button>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="mt-5 rounded-xl border border-dashed border-[#eadaaf] bg-[#fdfbf3] p-6 text-center text-xs font-medium text-slate-500">
            No products listed yet. Add your first crop to start selling.
          </div>
        )}
      </section>
    </div>
  );
}
