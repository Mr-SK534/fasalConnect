import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import api from "../../services/api";

export default function FarmerDashboard() {
  const { user } = useAuth();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);

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

  const activeProducts = products.filter(
    (product) => product.isActive !== false,
  ).length;
  const inventory = products.reduce(
    (total, product) => total + Number(product.quantity || 0),
    0,
  );

  return (
    <div className="mx-auto max-w-7xl space-y-10 text-xl font-medium">
      {/* Header Banner */}
      <section className="rounded-3xl bg-gradient-to-r from-[#2e7d32] via-[#246b28] to-[#1b5e20] p-8 text-white shadow-xl sm:p-12">
        <p className="text-xl font-extrabold uppercase tracking-widest text-[#f5d77f]">
          Good to see you, {user?.name || "Farmer"} 👋
        </p>
        <h2 className="mt-3 text-4xl font-black sm:text-5xl">
          Your Farm Marketplace
        </h2>
        <p className="mt-4 max-w-3xl text-xl leading-relaxed text-emerald-100">
          Keep your produce visible, your inventory current, and your buyers
          close.
        </p>
      </section>

      {/* Overview Metric Cards */}
      <section className="grid gap-6 sm:grid-cols-3">
        {[
          ["Listed products", products.length, "Your catalogue"],
          ["Active listings", activeProducts, "Visible to buyers"],
          ["Total inventory", inventory, "Units listed"],
        ].map(([label, value, hint]) => (
          <article
            key={label}
            className="rounded-3xl border border-[#eadaaf] bg-white/90 p-8 shadow-md backdrop-blur transition hover:shadow-lg"
          >
            <p className="text-lg font-extrabold uppercase tracking-wider text-slate-500">
              {label}
            </p>
            <p className="mt-4 text-5xl font-black text-[#1b5e20]">
              {loading ? "..." : value}
            </p>
            <p className="mt-3 text-lg font-bold text-slate-600">{hint}</p>
          </article>
        ))}
      </section>

      {/* Recent Listings List */}
      <section className="rounded-3xl border border-[#eadaaf] bg-white p-8 shadow-md sm:p-10">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h3 className="text-3xl font-black text-[#163820]">
              Recent Listings
            </h3>
            <p className="mt-1 text-xl font-semibold text-slate-600">
              Products connected to your account.
            </p>
          </div>
          <NavLink
            to="/farmer/list-product"
            className="rounded-2xl bg-[#f5d77f] px-6 py-3.5 text-xl font-extrabold text-[#1b5e20] shadow-sm transition hover:bg-[#eac459] hover:shadow"
          >
            + Add Product
          </NavLink>
        </div>

        {products.length ? (
          <div className="mt-8 divide-y divide-[#eee5cc]">
            {products.slice(0, 5).map((product) => (
              <div
                key={product.id}
                className="flex flex-wrap items-center justify-between gap-4 py-5"
              >
                <div className="flex items-center gap-5">
                  {product.imageUrl ? (
                    <img
                      src={product.imageUrl}
                      alt=""
                      className="h-16 w-16 rounded-2xl object-cover ring-2 ring-[#eadaaf]"
                    />
                  ) : (
                    <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-[#f4e8c5] text-3xl ring-2 ring-[#eadaaf]">
                      🌾
                    </div>
                  )}
                  <div>
                    <p className="text-2xl font-extrabold text-slate-900">
                      {product.cropName}
                    </p>
                    <p className="mt-1 text-lg font-bold text-slate-500">
                      {product.category} · {product.quantity} {product.unit}
                    </p>
                    {product.description && (
                      <p className="mt-2 max-w-2xl text-base font-medium leading-6 text-slate-600">
                        {product.description}
                      </p>
                    )}
                  </div>
                </div>
                <p className="text-2xl font-black text-[#1b5e20]">
                  ₹{Number(product.price || 0).toLocaleString("en-IN")}
                </p>
              </div>
            ))}
          </div>
        ) : (
          <div className="mt-8 rounded-2xl border border-dashed border-[#eadaaf] bg-[#fdfbf3] p-8 text-center text-xl font-bold text-slate-600">
            No products listed yet. Add your first crop to start selling.
          </div>
        )}
      </section>
    </div>
  );
}
