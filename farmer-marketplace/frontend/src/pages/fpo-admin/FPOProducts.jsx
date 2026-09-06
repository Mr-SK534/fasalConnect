import { useCallback, useEffect, useState } from "react";
import { toast } from "react-hot-toast";
import api from "../../services/api";
import { useAuth } from "../../hooks/useAuth";

const categories = [
  "Vegetables",
  "Fruits",
  "Grains",
  "Pulses",
  "Spices",
  "Dairy",
  "Other",
];
const units = ["Kg", "Quintal", "Ton", "Dozen", "Litre", "Piece"];
const emptyForm = {
  cropName: "",
  price: "",
  quantity: "",
  unit: "Kg",
  category: "Vegetables",
  harvestDate: "",
  description: "",
  region: "",
};
const inputClass =
  "mt-1 w-full rounded-lg border border-gray-200 bg-white px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400";
const getError = (error, fallback) =>
  error.response?.data?.message || error.response?.data?.title || fallback;

export default function FPOProducts() {
  const { user } = useAuth();
  const [products, setProducts] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [imageFile, setImageFile] = useState(null);
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const loadProducts = useCallback(async () => {
    if (!user?.id) return;
    setLoading(true);
    try {
      const response = await api.get(
        `/products/farmer/${user.id}?includeInactive=true`,
      );
      setProducts(Array.isArray(response.data) ? response.data : []);
    } catch (requestError) {
      setError(getError(requestError, "Could not load products."));
    } finally {
      setLoading(false);
    }
  }, [user]);
  useEffect(() => {
    // Synchronize this view with the authenticated product API.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadProducts();
  }, [loadProducts]);
  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      if (!imageFile) throw new Error("Select a product image.");
      const data = new FormData();
      data.append("file", imageFile);
      const upload = await api.post("/upload/product-image", data);
      const payload = {
        ...form,
        price: Number(form.price),
        quantity: Number(form.quantity),
        harvestDate: new Date(form.harvestDate).toISOString(),
        imageBase64: upload.data.imageBase64,
        imageContentType: upload.data.contentType,
      };
      if (editingId) await api.put(`/products/${editingId}`, payload);
      else await api.post("/products", payload);
      toast.success(editingId ? "Product updated" : "Product created");
      setForm(emptyForm);
      setImageFile(null);
      setEditingId(null);
      await loadProducts();
    } catch (requestError) {
      setError(getError(requestError, "Could not save product."));
    } finally {
      setSaving(false);
    }
  };
  const edit = (product) => {
    setEditingId(product.id);
    setForm({
      cropName: product.cropName,
      price: product.price,
      quantity: product.quantity,
      unit: product.unit,
      category: product.category,
      harvestDate: product.harvestDate?.slice(0, 10) || "",
      description: product.description || "",
      region: product.region || "",
    });
    setImageFile(null);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };
  const remove = async (product) => {
    if (!window.confirm(`Remove ${product.cropName}?`)) return;
    try {
      await api.delete(`/products/${product.id}`);
      toast.success("Product removed");
      await loadProducts();
    } catch (requestError) {
      toast.error(getError(requestError, "Could not remove product."));
    }
  };
  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          FPO Catalogue
        </p>
        <h2 className="text-2xl font-black text-[#163820]">My Products</h2>
        <p className="mt-1 text-sm text-slate-500">
          List and manage produce sold under your FPO account.
        </p>
      </div>
      <form onSubmit={submit} className="rounded-xl bg-white p-6 shadow-sm">
        <h3 className="text-lg font-bold">
          {editingId ? "Edit product" : "Add product"}
        </h3>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          {[
            ["cropName", "Crop name"],
            ["price", "Price"],
            ["quantity", "Quantity"],
            ["harvestDate", "Harvest date"],
            ["region", "Region"],
          ].map(([name, label]) => (
            <label key={name}>
              {label}
              <input
                required
                name={name}
                type={
                  name === "price" || name === "quantity"
                    ? "number"
                    : name === "harvestDate"
                      ? "date"
                      : "text"
                }
                min={
                  name === "price" || name === "quantity" ? "0.01" : undefined
                }
                value={form[name]}
                onChange={(event) =>
                  setForm({ ...form, [name]: event.target.value })
                }
                className={inputClass}
              />
            </label>
          ))}
          <label>
            Unit
            <select
              value={form.unit}
              onChange={(event) =>
                setForm({ ...form, unit: event.target.value })
              }
              className={inputClass}
            >
              {units.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label>
            Category
            <select
              value={form.category}
              onChange={(event) =>
                setForm({ ...form, category: event.target.value })
              }
              className={inputClass}
            >
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label className="sm:col-span-2">
            Description
            <textarea
              name="description"
              rows="3"
              value={form.description}
              onChange={(event) =>
                setForm({ ...form, description: event.target.value })
              }
              className={inputClass}
            />
          </label>
          <label className="sm:col-span-2">
            Product image
            <input
              required
              type="file"
              accept=".jpg,.jpeg,.png,.webp"
              onChange={(event) =>
                setImageFile(event.target.files?.[0] || null)
              }
              className={inputClass}
            />
          </label>
        </div>
        {error && (
          <p className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
            {error}
          </p>
        )}
        <div className="mt-5 flex gap-3">
          <button
            disabled={saving}
            className="rounded-lg bg-green-600 px-5 py-3 font-bold text-white hover:bg-green-700 disabled:opacity-60"
          >
            {saving
              ? "Saving..."
              : editingId
                ? "Update Product"
                : "Add Product"}
          </button>
          {editingId && (
            <button
              type="button"
              onClick={() => {
                setEditingId(null);
                setForm(emptyForm);
                setImageFile(null);
              }}
              className="rounded-lg border border-gray-200 px-5 py-3 font-semibold text-slate-600"
            >
              Cancel
            </button>
          )}
        </div>
      </form>
      {loading ? (
        <p className="p-8 text-center text-slate-500">Loading products...</p>
      ) : (
        <section className="overflow-hidden rounded-xl bg-white shadow-sm">
          <div className="grid grid-cols-[1fr_120px_180px] border-b bg-green-50 px-5 py-3 text-xs font-bold uppercase text-green-800">
            <span>Product</span>
            <span>Stock</span>
            <span>Actions</span>
          </div>
          {products.map((product) => (
            <div
              key={product.id}
              className="grid grid-cols-[1fr_120px_180px] items-center border-b px-5 py-4 last:border-0"
            >
              <div>
                <p className="font-bold text-slate-900">{product.cropName}</p>
                <p className="text-xs text-slate-500">
                  {product.category} · ₹{product.price}/{product.unit}
                </p>
              </div>
              <div>
                <p className="text-sm">
                  {product.quantity} {product.unit}
                </p>
                {product.isActive === false && (
                  <span className="rounded-full bg-red-100 px-2 py-1 text-xs font-semibold text-red-700">
                    Inactive
                  </span>
                )}
              </div>
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => edit(product)}
                  className="rounded-lg bg-green-50 px-3 py-2 text-xs font-bold text-green-700"
                >
                  Edit
                </button>
                <button
                  type="button"
                  onClick={() => remove(product)}
                  className="rounded-lg bg-red-50 px-3 py-2 text-xs font-bold text-red-700"
                >
                  Remove
                </button>
              </div>
            </div>
          ))}
          {!products.length && (
            <p className="p-8 text-center text-slate-500">
              No products listed yet.
            </p>
          )}
        </section>
      )}
    </div>
  );
}
