import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../../services/api";

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
const initialForm = {
  cropName: "",
  price: "",
  quantity: "",
  unit: "Kg",
  category: "Vegetables",
  harvestDate: "",
  description: "",
  region: "",
  imageUrl: "",
};

export default function ListProduct() {
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const [imagePreview, setImagePreview] = useState("");

  const update = (event) =>
    setForm({ ...form, [event.target.name]: event.target.value });

  const selectImage = (event) => {
    const file = event.target.files?.[0];
    if (file) setImagePreview(URL.createObjectURL(file));
  };

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    setSaving(true);
    try {
      await api.post("/products", {
        ...form,
        price: Number(form.price),
        quantity: Number(form.quantity),
        harvestDate: new Date(form.harvestDate).toISOString(),
      });
      navigate("/farmer/dashboard");
    } catch (err) {
      setError(err.response?.data?.message || "Could not list this product.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="mx-auto max-w-5xl text-xl">
      <div className="mb-8">
        <p className="text-base font-extrabold uppercase tracking-widest text-[#406836]">
          Farmer Catalogue
        </p>
        <h2 className="mt-1 text-4xl font-black text-[#163820]">
          List a Product
        </h2>
        <p className="mt-2 text-xl font-semibold text-slate-600">
          Share your available produce with buyers.
        </p>
      </div>

      <form
        onSubmit={submit}
        className="space-y-8 rounded-3xl border border-[#eadaaf] bg-white p-8 shadow-md sm:p-12"
      >
        {error && (
          <p className="rounded-2xl bg-red-50 p-5 text-xl font-bold text-red-700 ring-1 ring-red-200">
            {error}
          </p>
        )}

        <div className="grid gap-6 sm:grid-cols-2">
          <label className="text-lg font-extrabold text-slate-800">
            Crop Name
            <input
              required
              name="cropName"
              value={form.cropName}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
              placeholder="e.g. Fresh tomatoes"
            />
          </label>

          <label className="text-lg font-extrabold text-slate-800">
            Category
            <select
              name="category"
              value={form.category}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] bg-white px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            >
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>

          <label className="text-lg font-extrabold text-slate-800">
            Price
            <input
              required
              min="0.01"
              step="0.01"
              type="number"
              name="price"
              value={form.price}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
              placeholder="₹ per unit"
            />
          </label>

          <label className="text-lg font-extrabold text-slate-800">
            Quantity
            <input
              required
              min="0.01"
              step="0.01"
              type="number"
              name="quantity"
              value={form.quantity}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            />
          </label>

          <label className="text-lg font-extrabold text-slate-800">
            Unit
            <select
              name="unit"
              value={form.unit}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] bg-white px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            >
              {units.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>

          <label className="text-lg font-extrabold text-slate-800">
            Harvest Date
            <input
              required
              type="date"
              name="harvestDate"
              value={form.harvestDate}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            />
          </label>
        </div>

        <label className="block text-lg font-extrabold text-slate-800">
          Region
          <input
            name="region"
            value={form.region}
            onChange={update}
            className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            placeholder="Village, district, or market region"
          />
        </label>

        <label className="block text-lg font-extrabold text-slate-800">
          Description
          <textarea
            name="description"
            value={form.description}
            onChange={update}
            rows="4"
            className="mt-2.5 w-full resize-none rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            placeholder="Tell buyers about this harvest"
          />
        </label>

        <div className="grid gap-6 sm:grid-cols-[1fr_220px]">
          <label className="block text-lg font-extrabold text-slate-800">
            Crop Picture URL
            <input
              type="url"
              name="imageUrl"
              value={form.imageUrl}
              onChange={update}
              className="mt-2.5 w-full rounded-2xl border border-[#d2c5a2] px-5 py-4 text-xl font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
              placeholder="https://example.com/tomatoes.jpg"
            />
            <span className="mt-2 block text-base font-semibold text-slate-500">
              Use an image URL to save the picture with this listing.
            </span>
          </label>

          <label className="flex min-h-44 cursor-pointer flex-col items-center justify-center overflow-hidden rounded-2xl border-2 border-dashed border-[#d2c5a2] bg-[#fdfbf3] text-center text-lg font-bold text-[#406836] transition hover:border-[#2e7d32]">
            {imagePreview ? (
              <img
                src={imagePreview}
                alt="Selected crop preview"
                className="h-44 w-full object-cover"
              />
            ) : (
              <>
                <span className="text-4xl">🌾</span>
                <span className="mt-2 px-4">Preview crop photo</span>
              </>
            )}
            <input
              type="file"
              accept="image/*"
              onChange={selectImage}
              className="sr-only"
            />
          </label>
        </div>

        <button
          disabled={saving}
          className="rounded-2xl bg-[#2e7d32] px-8 py-4 text-xl font-black text-white shadow-md transition hover:bg-[#246b28] hover:shadow-lg disabled:opacity-60"
        >
          {saving ? "Saving Product..." : "Publish Product"}
        </button>
      </form>
    </div>
  );
}