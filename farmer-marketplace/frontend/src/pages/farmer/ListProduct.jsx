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
};

export default function ListProduct() {
  const navigate = useNavigate();
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);
  const update = (event) =>
    setForm({ ...form, [event.target.name]: event.target.value });

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
    <div className="mx-auto max-w-4xl">
      <div className="mb-8">
        <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[#668357]">
          Farmer catalogue
        </p>
        <h2 className="mt-2 text-3xl font-bold text-[#193b2a]">
          List a product
        </h2>
        <p className="mt-2 text-base text-slate-600">
          Share your available produce with buyers.
        </p>
      </div>
      <form
        onSubmit={submit}
        className="space-y-6 rounded-2xl border border-[#eadfbe] bg-white p-6 shadow-sm sm:p-8"
      >
        {error && (
          <p className="rounded-lg bg-red-50 p-4 text-base text-red-700">
            {error}
          </p>
        )}
        <div className="grid gap-5 sm:grid-cols-2">
          <label className="text-base font-semibold text-slate-700">
            Crop name
            <input
              required
              name="cropName"
              value={form.cropName}
              onChange={update}
              className="mt-2 w-full rounded-lg border border-[#dcd6c2] px-4 py-3 font-normal outline-none focus:border-[#174d35]"
              placeholder="e.g. Fresh tomatoes"
            />
          </label>
          <label className="text-base font-semibold text-slate-700">
            Category
            <select
              name="category"
              value={form.category}
              onChange={update}
              className="mt-2 w-full rounded-lg border border-[#dcd6c2] bg-white px-4 py-3 font-normal outline-none focus:border-[#174d35]"
            >
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label className="text-base font-semibold text-slate-700">
            Price
            <input
              required
              min="0.01"
              step="0.01"
              type="number"
              name="price"
              value={form.price}
              onChange={update}
              className="mt-2 w-full rounded-lg border border-[#dcd6c2] px-4 py-3 font-normal outline-none focus:border-[#174d35]"
              placeholder="₹ per unit"
            />
          </label>
          <label className="text-base font-semibold text-slate-700">
            Quantity
            <input
              required
              min="0.01"
              step="0.01"
              type="number"
              name="quantity"
              value={form.quantity}
              onChange={update}
              className="mt-2 w-full rounded-lg border border-[#dcd6c2] px-4 py-3 font-normal outline-none focus:border-[#174d35]"
            />
          </label>
          <label className="text-base font-semibold text-slate-700">
            Unit
            <select
              name="unit"
              value={form.unit}
              onChange={update}
              className="mt-2 w-full rounded-lg border border-[#dcd6c2] bg-white px-4 py-3 font-normal outline-none focus:border-[#174d35]"
            >
              {units.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>
          <label className="text-base font-semibold text-slate-700">
            Harvest date
            <input
              required
              type="date"
              name="harvestDate"
              value={form.harvestDate}
              onChange={update}
              className="mt-2 w-full rounded-lg border border-[#dcd6c2] px-4 py-3 font-normal outline-none focus:border-[#174d35]"
            />
          </label>
        </div>
        <label className="block text-base font-semibold text-slate-700">
          Region
          <input
            name="region"
            value={form.region}
            onChange={update}
            className="mt-2 w-full rounded-lg border border-[#dcd6c2] px-4 py-3 font-normal outline-none focus:border-[#174d35]"
            placeholder="Village, district, or market region"
          />
        </label>
        <label className="block text-base font-semibold text-slate-700">
          Description
          <textarea
            name="description"
            value={form.description}
            onChange={update}
            rows="4"
            className="mt-2 w-full resize-none rounded-lg border border-[#dcd6c2] px-4 py-3 font-normal outline-none focus:border-[#174d35]"
            placeholder="Tell buyers about this harvest"
          />
        </label>
        <button
          disabled={saving}
          className="rounded-lg bg-[#174d35] px-6 py-3 text-base font-bold text-white hover:bg-[#226b48] disabled:opacity-60"
        >
          {saving ? "Saving product..." : "Publish product"}
        </button>
      </form>
    </div>
  );
}
