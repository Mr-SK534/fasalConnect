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
    <div className="mx-auto max-w-7xl space-y-8">
      <section className="rounded-2xl bg-[#174d35] p-7 text-white shadow-lg sm:p-9">
        <p className="text-base font-semibold text-[#f3c969]">
          Good to see you, {user?.name || "Farmer"}
        </p>
        <h2 className="mt-2 text-3xl font-bold">Your farm marketplace</h2>
        <p className="mt-3 max-w-2xl text-base leading-7 text-white/75">
          Keep your produce visible, your inventory current, and your buyers
          close.
        </p>
      </section>
      <section className="grid gap-4 sm:grid-cols-3">
        {[
          ["Listed products", products.length, "Your catalogue"],
          ["Active listings", activeProducts, "Visible to buyers"],
          ["Total inventory", inventory, "Units listed"],
        ].map(([label, value, hint]) => (
          <article
            key={label}
            className="rounded-2xl border border-[#eadfbe] bg-white p-6 shadow-sm"
          >
            <p className="text-base font-semibold text-slate-500">{label}</p>
            <p className="mt-3 text-3xl font-bold text-[#174d35]">
              {loading ? "..." : value}
            </p>
            <p className="mt-2 text-sm text-slate-500">{hint}</p>
          </article>
        ))}
      </section>
      <section className="rounded-2xl border border-[#eadfbe] bg-white p-6 shadow-sm sm:p-7">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-xl font-bold text-[#193b2a]">
              Recent listings
            </h3>
            <p className="mt-1 text-base text-slate-500">
              Products connected to your account.
            </p>
          </div>
          <NavLink
            to="/farmer/list-product"
            className="rounded-lg bg-[#f3c969] px-4 py-2.5 text-base font-bold text-[#174d35] hover:bg-[#e8ba4c]"
          >
            Add product
          </NavLink>
        </div>
        {products.length ? (
          <div className="mt-6 divide-y divide-[#f0e7cd]">
            {products.slice(0, 5).map((product) => (
              <div
                key={product.id}
                className="flex flex-wrap items-center justify-between gap-3 py-4"
              >
                <div>
                  <p className="text-lg font-semibold text-slate-800">
                    {product.cropName}
                  </p>
                  <p className="text-sm text-slate-500">
                    {product.category} · {product.quantity} {product.unit}
                  </p>
                </div>
                <p className="text-lg font-bold text-[#174d35]">
                  ₹{Number(product.price || 0).toLocaleString("en-IN")}
                </p>
              </div>
            ))}
          </div>
        ) : (
          <p className="mt-6 rounded-xl bg-[#fffaf0] p-5 text-base text-slate-600">
            No products listed yet. Add your first crop to start selling.
          </p>
        )}
      </section>
    </div>
  );
}
