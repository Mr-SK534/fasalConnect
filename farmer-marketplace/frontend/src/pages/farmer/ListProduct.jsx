import { useEffect, useState } from "react";
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
  const [imagePreview, setImagePreview] = useState("");
  const [imageFile, setImageFile] = useState(null);

  useEffect(() => {
    return () => {
      if (imagePreview) URL.revokeObjectURL(imagePreview);
    };
  }, [imagePreview]);

  const update = (event) =>
    setForm({ ...form, [event.target.name]: event.target.value });

  const selectImage = (event) => {
    const file = event.target.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      setError("Choose an image smaller than 5 MB.");
      return;
    }
    setError("");
    setImageFile(file);
    setImagePreview(URL.createObjectURL(file));
  };

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    if (!imageFile) {
      setError("Add a crop picture before publishing the product.");
      return;
    }
    setSaving(true);
    try {
      const imageData = new FormData();
      imageData.append("file", imageFile);
      const uploadResponse = await api.post("/upload/product-image", imageData);

      await api.post("/products", {
        ...form,
        price: Number(form.price),
        quantity: Number(form.quantity),
        harvestDate: new Date(form.harvestDate).toISOString(),
        imageBase64: uploadResponse.data.imageBase64,
        imageContentType: uploadResponse.data.contentType,
      });
      navigate("/farmer/dashboard");
    } catch (err) {
      const responseData = err.response?.data;
      const validationMessage = responseData?.errors
        ? Object.values(responseData.errors).flat().join(" ")
        : null;
      setError(
        validationMessage ||
          responseData?.message ||
          responseData?.title ||
          "Could not list this product.",
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="mx-auto max-w-4xl space-y-5 text-sm">
      <div>
        <p className="text-xs font-bold uppercase tracking-wider text-[#406836]">
          Farmer Catalogue
        </p>
        <h2 className="mt-0.5 text-2xl font-black text-[#163820]">
          List a Product
        </h2>
        <p className="mt-1 text-xs text-slate-500">
          Share your available produce with buyers.
        </p>
      </div>

      <form
        onSubmit={submit}
        className="space-y-5 rounded-2xl border border-[#eadaaf] bg-white p-6 shadow-xs sm:p-8"
      >
        {error && (
          <p className="rounded-xl bg-red-50 p-3.5 text-xs font-semibold text-red-700 ring-1 ring-red-200">
            {error}
          </p>
        )}

        <div className="grid gap-4 sm:grid-cols-2">
          <label className="text-xs font-bold text-slate-800">
            Crop Name
            <input
              required
              name="cropName"
              value={form.cropName}
              onChange={update}
              className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
              placeholder="e.g. Fresh tomatoes"
            />
          </label>

          <label className="text-xs font-bold text-slate-800">
            Category
            <select
              name="category"
              value={form.category}
              onChange={update}
              className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] bg-white px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            >
              {categories.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>

          <label className="text-xs font-bold text-slate-800">
            Price
            <input
              required
              min="0.01"
              step="0.01"
              type="number"
              name="price"
              value={form.price}
              onChange={update}
              className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
              placeholder="₹ per unit"
            />
          </label>

          <label className="text-xs font-bold text-slate-800">
            Quantity
            <input
              required
              min="0.01"
              step="0.01"
              type="number"
              name="quantity"
              value={form.quantity}
              onChange={update}
              className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            />
          </label>

          <label className="text-xs font-bold text-slate-800">
            Unit
            <select
              name="unit"
              value={form.unit}
              onChange={update}
              className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] bg-white px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            >
              {units.map((item) => (
                <option key={item}>{item}</option>
              ))}
            </select>
          </label>

          <label className="text-xs font-bold text-slate-800">
            Harvest Date
            <input
              required
              type="date"
              name="harvestDate"
              value={form.harvestDate}
              onChange={update}
              className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            />
          </label>
        </div>

        <label className="block text-xs font-bold text-slate-800">
          Region
          <input
            name="region"
            value={form.region}
            onChange={update}
            className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            placeholder="Address, district, or market region"
          />
        </label>

        <label className="block text-xs font-bold text-slate-800">
          Description
          <textarea
            name="description"
            value={form.description}
            onChange={update}
            rows="3"
            className="mt-1.5 w-full resize-none rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 text-sm font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
            placeholder="Tell buyers about this harvest"
          />
        </label>

        <div>
          <p className="text-xs font-bold text-slate-800">Crop Picture</p>
          <div className="mt-1.5 grid gap-4 sm:grid-cols-[1fr_160px]">
            <div className="rounded-xl border border-[#d2c5a2] bg-[#fdfbf3] p-3 text-xs text-slate-600">
              Upload a JPG, PNG, or WEBP image up to 5 MB. A picture is required
              for every listing.
              {imageFile && (
                <p className="mt-2 font-semibold text-[#406836]">
                  Selected: {imageFile.name}
                </p>
              )}
            </div>
            <label className="flex h-28 cursor-pointer flex-col items-center justify-center overflow-hidden rounded-xl border-2 border-dashed border-[#d2c5a2] bg-[#fdfbf3] text-center text-xs font-semibold text-[#406836] transition hover:border-[#2e7d32]">
              {imagePreview ? (
                <img
                  src={imagePreview}
                  alt="Selected crop preview"
                  className="h-28 w-full object-cover"
                />
              ) : (
                <>
                  <span className="text-2xl">🌾</span>
                  <span className="mt-1 px-2">Preview photo</span>
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
        </div>

        <button
          disabled={saving}
          className="rounded-xl bg-[#2e7d32] px-6 py-2.5 text-xs font-bold text-white shadow-xs transition hover:bg-[#246b28] hover:shadow-sm disabled:opacity-60"
        >
          {saving ? "Saving Product..." : "Publish Product"}
        </button>
      </form>
    </div>
  );
}
