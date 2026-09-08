// frontend/src/pages/admin/AdminConfigDashboard.jsx

import React, { useState, useEffect } from "react";
import api from "../../services/api";
import { toast } from "react-hot-toast";

export default function AdminConfigDashboard() {
  const [configs, setConfigs] = useState({});
  const [userRole, setUserRole] = useState("manager");
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("config"); // "config" | "audit" | "simulate"

  // Edit Modal State
  const [editingItem, setEditingItem] = useState(null);
  const [newValue, setNewValue] = useState("");
  const [reason, setReason] = useState("");
  const [saving, setSaving] = useState(false);

  // Add New Crop Config Modal State
  const [showAddCropModal, setShowAddCropModal] = useState(false);
  const [newCropName, setNewCropName] = useState("");
  const [newCommissionPct, setNewCommissionPct] = useState("0.08");
  const [addCropReason, setAddCropReason] = useState("");
  const [addingCrop, setAddingCrop] = useState(false);

  // Audit History State
  const [auditHistory, setAuditHistory] = useState([]);
  const [loadingAudit, setLoadingAudit] = useState(false);

  // Simulator State
  const [simFarmerPrice, setSimFarmerPrice] = useState(25);
  const [simCommissionPct, setSimCommissionPct] = useState(0.08);
  const [simResult, setSimResult] = useState(null);
  const [simulating, setSimulating] = useState(false);

  useEffect(() => {
    fetchConfig();
  }, []);

  const fetchConfig = async () => {
    setLoading(true);
    try {
      const res = await api.get("/admin/config");
      setConfigs(res.data.config || {});
      setUserRole(res.data.userRole || "manager");
    } catch (err) {
      console.error("Failed to load platform config:", err);
      toast.error(err.response?.data?.message || "Failed to load platform configuration");
    } finally {
      setLoading(false);
    }
  };

  const fetchAuditHistory = async () => {
    setLoadingAudit(true);
    try {
      const res = await api.get("/admin/config/audit-history?limit=50");
      setAuditHistory(res.data || []);
    } catch (err) {
      console.error("Failed to load audit history:", err);
      toast.error("Failed to load audit history");
    } finally {
      setLoadingAudit(false);
    }
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
    if (tab === "audit" && auditHistory.length === 0) {
      fetchAuditHistory();
    }
    if (tab === "simulate" && !simResult) {
      runSimulation(simFarmerPrice, simCommissionPct);
    }
  };

  const openEditModal = (item) => {
    setEditingItem(item);
    setNewValue(item.value);
    setReason("");
  };

  const handleSaveUpdate = async (e) => {
    e.preventDefault();
    if (!editingItem) return;
    if (!reason.trim()) {
      toast.error("Please provide a reason/description for this change");
      return;
    }

    setSaving(true);
    try {
      const res = await api.post("/admin/config/update", {
        key: editingItem.key,
        newValue: newValue,
        new_value: newValue,
        description: reason,
      });

      toast.success(`✓ Updated ${res.data.key} to '${res.data.newValue || res.data.new_value}'`);
      setEditingItem(null);
      fetchConfig();
    } catch (err) {
      console.error("Update failed:", err);
      toast.error(err.response?.data?.message || "Failed to update configuration");
    } finally {
      setSaving(false);
    }
  };

  const handleAddCropSubmit = async (e) => {
    e.preventDefault();
    if (!newCropName.trim()) {
      toast.error("Please enter a crop name");
      return;
    }

    setAddingCrop(true);
    try {
      const res = await api.post("/admin/config/add-crop", {
        cropName: newCropName.trim(),
        commissionPct: Number(newCommissionPct),
        description: addCropReason.trim() || `Override commission % for ${newCropName.trim()}`,
      });

      toast.success(`✓ Added crop config for '${newCropName}' (${Number(newCommissionPct) * 100}% commission)`);
      setShowAddCropModal(false);
      setNewCropName("");
      setNewCommissionPct("0.08");
      setAddCropReason("");
      fetchConfig();
    } catch (err) {
      console.error("Add crop config failed:", err);
      toast.error(err.response?.data?.message || "Failed to add crop configuration");
    } finally {
      setAddingCrop(false);
    }
  };

  const runSimulation = async (fPrice, commPct) => {
    setSimulating(true);
    const farmerPrice = Number(fPrice) || 0;
    let pct = Number(commPct) || 0;
    if (pct > 1) pct /= 100;

    // Instant calculated fallback preview
    const commissionAmt = Math.round(farmerPrice * pct * 100) / 100;
    const logisticsTotal = 2.50; // ₹2.00 partner + ₹0.50 margin
    const computedBuyerPrice = Math.round((farmerPrice + commissionAmt + logisticsTotal) * 100) / 100;
    const computedPlatformRevenue = Math.round((commissionAmt + 0.50) * 100) / 100;

    const fallbackResult = {
      farmerPrice,
      newCommissionPct: pct,
      buyerPrice: computedBuyerPrice,
      platformRevenuePerKg: computedPlatformRevenue,
      message: `Simulated buyer price with ${(pct * 100).toFixed(1)}% commission`,
    };

    setSimResult(fallbackResult);

    try {
      const res = await api.post("/admin/config/simulate", {
        farmerPrice,
        newCommissionPct: pct,
      });
      if (res.data) {
        setSimResult(res.data);
      }
    } catch (err) {
      console.error("API simulation call error (using computed result):", err);
    } finally {
      setSimulating(false);
    }
  };

  const handleSimulate = (e) => {
    e.preventDefault();
    runSimulation(simFarmerPrice, simCommissionPct);
  };

  const getRoleBadgeStyle = (role) => {
    const r = String(role).toLowerCase();
    if (r === "superadmin" || r === "platformadmin") {
      return "bg-rose-100 text-rose-800 border-rose-200";
    }
    if (r === "admin" || r === "fpoadmin") {
      return "bg-blue-100 text-blue-800 border-blue-200";
    }
    return "bg-emerald-100 text-emerald-800 border-emerald-200";
  };

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg font-medium text-slate-600">Loading Platform Configuration...</div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6 p-4">
      {/* Header */}
      <header className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <div>
          <span className="text-xs font-bold uppercase tracking-wider text-green-700">RBAC Configuration Management</span>
          <h1 className="mt-1 text-3xl font-black text-slate-900">Platform Settings & Controls</h1>
          <p className="mt-1 text-sm text-slate-500">
            Dynamically adjust pricing, commission rates, logistics charges, and operational thresholds.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-sm font-semibold text-slate-600">Active Role:</span>
          <span className={`rounded-full border px-4 py-1.5 text-xs font-bold uppercase tracking-wide shadow-xs ${getRoleBadgeStyle(userRole)}`}>
            {userRole}
          </span>
        </div>
      </header>

      {/* Navigation Tabs */}
      <div className="flex border-b border-slate-200">
        <button
          onClick={() => handleTabChange("config")}
          className={`px-6 py-3 text-sm font-bold transition-colors ${
            activeTab === "config" ? "border-b-2 border-green-600 text-green-700" : "text-slate-500 hover:text-slate-800"
          }`}
        >
          ⚙️ Category Settings
        </button>
        <button
          onClick={() => handleTabChange("audit")}
          className={`px-6 py-3 text-sm font-bold transition-colors ${
            activeTab === "audit" ? "border-b-2 border-green-600 text-green-700" : "text-slate-500 hover:text-slate-800"
          }`}
        >
          📜 Audit Trail History
        </button>
        <button
          onClick={() => handleTabChange("simulate")}
          className={`px-6 py-3 text-sm font-bold transition-colors ${
            activeTab === "simulate" ? "border-b-2 border-green-600 text-green-700" : "text-slate-500 hover:text-slate-800"
          }`}
        >
          🧮 Buyer Price Simulator
        </button>
      </div>

      {/* TAB 1: Configuration Categories */}
      {activeTab === "config" && (
        <div className="space-y-8">
          {Object.entries(configs).map(([category, items]) => (
            <section key={category} className="rounded-xl border border-slate-200 bg-white shadow-sm overflow-hidden">
              <div className="bg-slate-50 px-6 py-4 border-b border-slate-200 flex items-center justify-between">
                <h2 className="font-bold text-slate-800 uppercase text-sm tracking-wider">
                  {category.replace("_", " ")} ({items.length} settings)
                </h2>
                {category === "crop_pricing" && (
                  <button
                    onClick={() => setShowAddCropModal(true)}
                    className="rounded-lg bg-green-600 px-4 py-1.5 text-xs font-bold text-white shadow-xs hover:bg-green-700 transition-colors flex items-center gap-1.5"
                  >
                    <span>➕</span> Add New Crop Config
                  </button>
                )}
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm border-collapse">
                  <thead className="bg-slate-100/70 text-xs uppercase text-slate-600 font-bold border-b border-slate-200">
                    <tr>
                      <th className="p-4">Key</th>
                      <th className="p-4">Current Value</th>
                      <th className="p-4">Type</th>
                      <th className="p-4">Requires Role</th>
                      <th className="p-4">Description</th>
                      <th className="p-4 text-right">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {items.map((item) => (
                      <tr key={item.key} className={item.canEdit ? "hover:bg-amber-50/40" : "bg-slate-50/50 opacity-80"}>
                        <td className="p-4 font-mono font-bold text-slate-900">{item.key}</td>
                        <td className="p-4">
                          <span className="inline-block rounded-md bg-slate-100 px-2.5 py-1 font-mono font-semibold text-slate-800 border border-slate-200">
                            {item.value} {item.valueType === "Percent" ? `(${(Number(item.value) * 100).toFixed(1)}%)` : ""}
                          </span>
                        </td>
                        <td className="p-4 text-xs font-semibold text-slate-500">{item.valueType}</td>
                        <td className="p-4">
                          <span className={`inline-block px-2 py-0.5 rounded text-xs font-bold uppercase ${
                            item.requiresRole === "superadmin" ? "bg-rose-100 text-rose-700" : "bg-blue-100 text-blue-700"
                          }`}>
                            {item.requiresRole}
                          </span>
                        </td>
                        <td className="p-4 text-xs text-slate-600 max-w-xs">{item.description}</td>
                        <td className="p-4 text-right">
                          {item.canEdit ? (
                            <button
                              onClick={() => openEditModal(item)}
                              className="rounded-lg bg-green-600 px-3.5 py-1.5 text-xs font-bold text-white shadow-xs hover:bg-green-700 transition-colors"
                            >
                              Edit
                            </button>
                          ) : (
                            <span className="inline-flex items-center gap-1 text-xs text-slate-400 font-medium">
                              🔒 Read-only
                            </span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          ))}
        </div>
      )}

      {/* TAB 2: Audit History */}
      {activeTab === "audit" && (
        <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-xl font-bold text-slate-900 mb-4">Configuration Change Audit Trail</h2>
          {loadingAudit ? (
            <p className="text-sm text-slate-500">Loading audit trail...</p>
          ) : auditHistory.length === 0 ? (
            <p className="text-sm text-slate-500">No configuration changes logged yet.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm border-collapse">
                <thead className="bg-slate-50 text-xs uppercase text-slate-600 font-bold border-b border-slate-200">
                  <tr>
                    <th className="p-3">Timestamp</th>
                    <th className="p-3">User</th>
                    <th className="p-3">Description & Reason</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {auditHistory.map((log) => (
                    <tr key={log.id} className="hover:bg-slate-50">
                      <td className="p-3 font-mono text-xs text-slate-600">
                        {new Date(log.timestamp).toLocaleString("en-IN")}
                      </td>
                      <td className="p-3 font-bold text-slate-800">{log.createdBy || log.fromAccount}</td>
                      <td className="p-3 text-slate-700">{log.notes}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}

      {/* TAB 3: Price Simulator */}
      {activeTab === "simulate" && (
        <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm max-w-2xl space-y-6">
          <div>
            <h2 className="text-xl font-bold text-slate-900">Buyer Price Simulator</h2>
            <p className="text-sm text-slate-500 mt-1">
              Preview the resulting buyer price (Farmer Price + Commission + Logistics) before committing changes to configuration.
            </p>
          </div>

          <form onSubmit={handleSimulate} className="space-y-4">
            <div>
              <label className="block text-xs font-bold uppercase text-slate-600 mb-1">Farmer Asking Price (₹/kg)</label>
              <input
                type="number"
                step="0.1"
                value={simFarmerPrice}
                onChange={(e) => {
                  const val = e.target.value;
                  setSimFarmerPrice(val);
                  runSimulation(val, simCommissionPct);
                }}
                className="w-full rounded-lg border border-slate-300 p-2.5 text-sm font-semibold text-slate-900 focus:border-green-600 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-bold uppercase text-slate-600 mb-1">Hypothetical Commission % (e.g. 0.08 = 8%)</label>
              <input
                type="number"
                step="0.01"
                value={simCommissionPct}
                onChange={(e) => {
                  const val = e.target.value;
                  setSimCommissionPct(val);
                  runSimulation(simFarmerPrice, val);
                }}
                className="w-full rounded-lg border border-slate-300 p-2.5 text-sm font-semibold text-slate-900 focus:border-green-600 focus:outline-none"
              />
            </div>

            <button
              type="submit"
              disabled={simulating}
              className="rounded-lg bg-green-600 px-6 py-2.5 text-sm font-bold text-white hover:bg-green-700 transition-colors"
            >
              {simulating ? "Simulating..." : "Calculate Simulated Price"}
            </button>
          </form>

          {simResult && (
            <div className="rounded-lg border border-green-200 bg-green-50 p-5 space-y-2 text-sm text-slate-800">
              <p className="font-bold text-green-900 text-lg">Simulated Results</p>
              <p>Farmer Asking Price: <strong>₹{simResult.farmerPrice}/kg</strong></p>
              <p>Commission ({simResult.newCommissionPct * 100}%): <strong>₹{(simResult.farmerPrice * simResult.newCommissionPct).toFixed(2)}/kg</strong></p>
              <p>Logistics Charge (Partner + Margin): <strong>₹2.50/kg</strong></p>
              <hr className="border-green-200 my-2" />
              <p className="text-xl font-black text-green-900">
                Buyer Price: ₹{simResult.buyerPrice}/kg
              </p>
              <p className="text-xs text-green-700">Platform Revenue per kg: ₹{simResult.platformRevenuePerKg}</p>
            </div>
          )}
        </section>
      )}

      {/* Edit Modal */}
      {editingItem && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-xs p-4">
          <div className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl space-y-4">
            <h3 className="text-lg font-bold text-slate-900">Update Configuration</h3>
            <p className="text-xs font-mono font-bold text-green-700">{editingItem.key}</p>
            <p className="text-xs text-slate-500">{editingItem.description}</p>

            <form onSubmit={handleSaveUpdate} className="space-y-4">
              <div>
                <label className="block text-xs font-bold uppercase text-slate-600 mb-1">New Value</label>
                <input
                  type="text"
                  value={newValue}
                  onChange={(e) => setNewValue(e.target.value)}
                  required
                  className="w-full rounded-lg border border-slate-300 p-2.5 text-sm font-bold text-slate-900 focus:border-green-600 focus:outline-none"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase text-slate-600 mb-1">Reason / Description for Change *</label>
                <textarea
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  required
                  placeholder="e.g. Partner rate adjustment Q3"
                  rows={3}
                  className="w-full rounded-lg border border-slate-300 p-2.5 text-sm text-slate-900 focus:border-green-600 focus:outline-none"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setEditingItem(null)}
                  className="rounded-lg bg-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-300"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="rounded-lg bg-green-600 px-5 py-2 text-sm font-bold text-white hover:bg-green-700"
                >
                  {saving ? "Saving..." : "Confirm & Save"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add New Crop Config Modal */}
      {showAddCropModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-xs p-4">
          <div className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl space-y-4">
            <h3 className="text-lg font-bold text-slate-900">Add New Crop Pricing Override</h3>
            <p className="text-xs text-slate-500">
              Create a custom platform commission rule for any new crop (e.g. Red Onion, Wheat, Garlic, Mango).
            </p>

            <form onSubmit={handleAddCropSubmit} className="space-y-4">
              <div>
                <label className="block text-xs font-bold uppercase text-slate-600 mb-1">Crop Name *</label>
                <input
                  type="text"
                  value={newCropName}
                  onChange={(e) => setNewCropName(e.target.value)}
                  placeholder="e.g. Red Onion, Garlic, Wheat"
                  required
                  className="w-full rounded-lg border border-slate-300 p-2.5 text-sm font-bold text-slate-900 focus:border-green-600 focus:outline-none"
                />
              </div>

              <div>
                <label className="block text-xs font-bold uppercase text-slate-600 mb-1">Commission Rate (e.g. 0.08 = 8%) *</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  max="1"
                  value={newCommissionPct}
                  onChange={(e) => setNewCommissionPct(e.target.value)}
                  required
                  className="w-full rounded-lg border border-slate-300 p-2.5 text-sm font-bold text-slate-900 focus:border-green-600 focus:outline-none"
                />
                <p className="mt-1 text-xs text-slate-500">
                  Current value: <strong>{(Number(newCommissionPct) * 100).toFixed(1)}%</strong>
                </p>
              </div>

              <div>
                <label className="block text-xs font-bold uppercase text-slate-600 mb-1">Description / Reason</label>
                <textarea
                  value={addCropReason}
                  onChange={(e) => setAddCropReason(e.target.value)}
                  placeholder="e.g. Dynamic pricing override for Red Onion harvest"
                  rows={2}
                  className="w-full rounded-lg border border-slate-300 p-2.5 text-sm text-slate-900 focus:border-green-600 focus:outline-none"
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowAddCropModal(false)}
                  className="rounded-lg bg-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-300"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={addingCrop}
                  className="rounded-lg bg-green-600 px-5 py-2 text-sm font-bold text-white hover:bg-green-700"
                >
                  {addingCrop ? "Adding Crop..." : "Create Crop Config"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
