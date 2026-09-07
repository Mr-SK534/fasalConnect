// frontend/src/pages/admin/RouteDashboard.jsx

import React, { useState, useEffect, useMemo, useCallback } from "react";
import { toast } from "react-hot-toast";
import {
  getPendingOrders,
  optimizeRoute,
  getAllRoutes,
  getRoute,
  runBatchWindow,
} from "../../services/routeService";
import RouteMap from "../../components/map/RouteMap";

export default function RouteDashboard() {
  const [pendingOrders, setPendingOrders] = useState([]);
  const [selectedOrderIds, setSelectedOrderIds] = useState(new Set());
  const [loadingPending, setLoadingPending] = useState(true);
  const [optimizing, setOptimizing] = useState(false);

  // Route History state
  const [routesHistory, setRoutesHistory] = useState([]);
  const [selectedRouteId, setSelectedRouteId] = useState("");
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [runningBatchWindow, setRunningBatchWindow] = useState(false);

  // Depot Location state (Default: Mumbai 19.0760, 72.8777)
  const [depot, setDepot] = useState({ lat: 19.0760, lng: 72.8777 });

  // Optimization Result state
  const [routeResult, setRouteResult] = useState(null);

  // Fetch pending orders
  const fetchPending = useCallback(async () => {
    setLoadingPending(true);
    try {
      const data = await getPendingOrders();
      setPendingOrders(data || []);
      setSelectedOrderIds(new Set((data || []).map((o) => o.orderId || o.OrderId)));
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed to load pending orders.");
    } finally {
      setLoadingPending(false);
    }
  }, []);

  // Fetch routes history
  const fetchHistory = useCallback(async () => {
    setLoadingHistory(true);
    try {
      const data = await getAllRoutes();
      setRoutesHistory(data || []);
    } catch (err) {
      toast.error("Failed to load routes history.");
    } finally {
      setLoadingHistory(false);
    }
  }, []);

  useEffect(() => {
    fetchPending();
    fetchHistory();
  }, [fetchPending, fetchHistory]);

  // Load specific route details when selecting from history
  const handleSelectRouteHistory = async (routeId) => {
    if (!routeId) {
      setSelectedRouteId("");
      return;
    }
    setSelectedRouteId(routeId);
    try {
      const routeData = await getRoute(routeId);
      setRouteResult(routeData);
      toast.success(`Loaded saved route: ${routeId.slice(0, 8)}...`);
    } catch (err) {
      toast.error("Failed to load selected route details.");
    }
  };

  // Run manual batch window trigger
  const handleRunBatchWindow = async (windowName) => {
    setRunningBatchWindow(true);
    try {
      const res = await runBatchWindow(windowName);
      toast.success(res.message || `Batch window '${windowName}' executed!`);
      await fetchPending();
      await fetchHistory();
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed to run batch window.");
    } finally {
      setRunningBatchWindow(false);
    }
  };

  // Handle individual order checkbox toggle
  const toggleOrder = (orderId) => {
    setSelectedOrderIds((prev) => {
      const next = new Set(prev);
      if (next.has(orderId)) {
        next.delete(orderId);
      } else {
        next.add(orderId);
      }
      return next;
    });
  };

  // Select / Deselect All
  const toggleSelectAll = () => {
    if (selectedOrderIds.size === pendingOrders.length) {
      setSelectedOrderIds(new Set());
    } else {
      setSelectedOrderIds(new Set(pendingOrders.map((o) => o.orderId || o.OrderId)));
    }
  };

  // Live total quantity of selected orders (in kg)
  const selectedTotalKg = useMemo(() => {
    return pendingOrders
      .filter((o) => selectedOrderIds.has(o.orderId || o.OrderId))
      .reduce((sum, o) => sum + (o.quantity || o.Quantity || 0), 0);
  }, [pendingOrders, selectedOrderIds]);

  // Live vehicle count preview: Math.ceil(selectedTotalKg / 2000)
  const vehiclePreviewCount = useMemo(() => {
    if (selectedTotalKg <= 0) return 0;
    return Math.ceil(selectedTotalKg / 2000);
  }, [selectedTotalKg]);

  // Trigger Route Optimization
  const handleOptimize = async () => {
    if (selectedOrderIds.size === 0) {
      toast.error("Please select at least one order to optimize.");
      return;
    }

    if (!depot.lat || !depot.lng) {
      toast.error("Please provide valid depot latitude and longitude.");
      return;
    }

    setOptimizing(true);
    try {
      const orderIdList = Array.from(selectedOrderIds);
      const result = await optimizeRoute(orderIdList, parseFloat(depot.lat), parseFloat(depot.lng));
      setRouteResult(result);
      toast.success(`Route optimized! ${result.vehicleCount} vehicle(s) dispatched.`);
      
      // Refresh pending orders and history
      fetchPending();
      fetchHistory();
    } catch (err) {
      toast.error(err.response?.data?.message || err.response?.data || "Optimization failed.");
    } finally {
      setOptimizing(false);
    }
  };

  // Flatten all stops from routeResult.stopsByVehicle for the map
  const flattenedStops = useMemo(() => {
    if (!routeResult || !routeResult.stopsByVehicle) return [];
    return Object.values(routeResult.stopsByVehicle).flat();
  }, [routeResult]);

  const urgentStopsCount = useMemo(() => {
    return flattenedStops.filter((s) => s.isUrgent || s.IsUrgent).length;
  }, [flattenedStops]);

  const getPerishabilityBadge = (tier) => {
    if (!tier) return null;
    const t = String(tier).toLowerCase();
    if (t === "critical")
      return (
        <span className="bg-red-100 text-red-700 border border-red-200 text-[10px] font-extrabold px-1.5 py-0.5 rounded">
          🔴 CRITICAL
        </span>
      );
    if (t === "high")
      return (
        <span className="bg-orange-100 text-orange-700 border border-orange-200 text-[10px] font-extrabold px-1.5 py-0.5 rounded">
          🟠 HIGH
        </span>
      );
    if (t === "medium")
      return (
        <span className="bg-yellow-100 text-yellow-800 border border-yellow-200 text-[10px] font-extrabold px-1.5 py-0.5 rounded">
          🟡 MEDIUM
        </span>
      );
    return null;
  };

  const formatEta = (etaStr) => {
    if (!etaStr) return "";
    try {
      const d = new Date(etaStr);
      return d.toLocaleTimeString("en-IN", { hour: "2-digit", minute: "2-digit" });
    } catch {
      return "";
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 p-6 md:p-10 font-sans text-slate-800">
      <div className="max-w-7xl mx-auto space-y-8">
        
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-slate-200 pb-5">
          <div>
            <h1 className="text-2xl md:text-3xl font-extrabold text-slate-900 tracking-tight">
              Route Optimization & Automated Dispatch
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Automated 2-batch optimization at <strong>8:00 AM</strong> & <strong>2:00 PM (14:00)</strong> daily with Haversine VRP math & perishability urgency.
            </p>
          </div>
          
          <div className="flex items-center gap-2">
            <button
              onClick={fetchPending}
              disabled={loadingPending}
              className="px-4 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition shadow-sm"
            >
              Refresh Orders
            </button>
            <button
              onClick={fetchHistory}
              disabled={loadingHistory}
              className="px-4 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition shadow-sm"
            >
              Refresh History
            </button>
          </div>
        </div>

        {/* Automated Schedule & Batch Trigger Banner */}
        <div className="bg-emerald-900 text-white rounded-2xl p-5 shadow-sm flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-800 flex items-center justify-center text-xl font-bold text-emerald-300">
              ⏰
            </div>
            <div>
              <p className="text-xs text-emerald-300 font-bold uppercase tracking-wider">
                Automated Batch Schedule
              </p>
              <h3 className="text-sm font-extrabold text-white">
                Daily Batches: 8:00 AM &bull; 2:00 PM (14:00)
              </h3>
            </div>
          </div>

          <div className="flex items-center gap-3 w-full md:w-auto">
            <button
              onClick={() => handleRunBatchWindow("morning")}
              disabled={runningBatchWindow}
              className="flex-1 md:flex-none px-3.5 py-2 rounded-xl text-xs font-bold bg-emerald-600 hover:bg-emerald-500 active:bg-emerald-700 text-white transition shadow"
            >
              Trigger 8 AM Batch
            </button>
            <button
              onClick={() => handleRunBatchWindow("afternoon")}
              disabled={runningBatchWindow}
              className="flex-1 md:flex-none px-3.5 py-2 rounded-xl text-xs font-bold bg-blue-600 hover:bg-blue-500 active:bg-blue-700 text-white transition shadow"
            >
              Trigger 2 PM Batch
            </button>
          </div>
        </div>

        {/* Top Warning Banner for Urgent Stops */}
        {(routeResult?.hasUrgentStops || routeResult?.HasUrgentStops || urgentStopsCount > 0) && (
          <div className="bg-red-50 border border-red-300 text-red-900 p-4 rounded-2xl flex items-center gap-3 text-xs font-bold shadow-sm">
            <span className="text-xl">⚠️</span>
            <div>
              <p className="font-extrabold text-sm">Spoilage Risk Warning</p>
              <p className="font-medium text-red-700 mt-0.5">
                {routeResult?.warningMessage ||
                  routeResult?.WarningMessage ||
                  `Warning: ${urgentStopsCount} stops may receive spoiled produce at current ETA. Consider adding more vehicles or reducing batch size.`}
              </p>
            </div>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
          
          {/* Left Column: Route History + Config & Pending Orders (5 cols) */}
          <div className="lg:col-span-5 space-y-6">
            
            {/* Saved Route History Lookup Card */}
            <div className="bg-white rounded-2xl p-6 shadow-sm border border-slate-200 space-y-4">
              <div className="flex items-center justify-between">
                <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
                  <span className="w-3 h-3 rounded-full bg-purple-600"></span>
                  Saved Route History ({routesHistory.length})
                </h2>
              </div>

              <div>
                <label className="block text-xs font-medium text-slate-600 mb-1">
                  Select Optimized Route by ID
                </label>
                <select
                  value={selectedRouteId}
                  onChange={(e) => handleSelectRouteHistory(e.target.value)}
                  className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 bg-white font-mono"
                >
                  <option value="">-- Choose a Route ID to inspect --</option>
                  {routesHistory.map((r) => {
                    const id = r.id || r.Id;
                    const date = new Date(r.createdAt || r.CreatedAt).toLocaleDateString("en-IN");
                    const window = r.batchWindow || r.BatchWindow || "manual";
                    const vCount = r.vehicleCount || r.VehicleCount;
                    const dist = r.totalDistanceKm || r.TotalDistanceKm;

                    return (
                      <option key={id} value={id}>
                        {String(id).slice(0, 8)}... &bull; {window} &bull; {date} &bull; {vCount} V &bull; {dist}km
                      </option>
                    );
                  })}
                </select>
              </div>
            </div>

            {/* Depot Config Card */}
            <div className="bg-white rounded-2xl p-6 shadow-sm border border-slate-200 space-y-4">
              <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
                <span className="w-3 h-3 rounded-full bg-blue-600"></span>
                Central Depot / Hub Location
              </h2>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-slate-600 mb-1">
                    Depot Latitude
                  </label>
                  <input
                    type="number"
                    step="any"
                    value={depot.lat}
                    onChange={(e) => setDepot({ ...depot, lat: e.target.value })}
                    className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="19.0760"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-slate-600 mb-1">
                    Depot Longitude
                  </label>
                  <input
                    type="number"
                    step="any"
                    value={depot.lng}
                    onChange={(e) => setDepot({ ...depot, lng: e.target.value })}
                    className="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="72.8777"
                  />
                </div>
              </div>
            </div>

            {/* Pending Orders Selection Card */}
            <div className="bg-white rounded-2xl p-6 shadow-sm border border-slate-200 space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-base font-bold text-slate-900">
                    Pending Unrouted Orders ({pendingOrders.length})
                  </h2>
                  <p className="text-xs text-slate-500">
                    Select orders to include in batch
                  </p>
                </div>
                
                {pendingOrders.length > 0 && (
                  <button
                    onClick={toggleSelectAll}
                    className="text-xs font-bold text-blue-600 hover:text-blue-800"
                  >
                    {selectedOrderIds.size === pendingOrders.length ? "Deselect All" : "Select All"}
                  </button>
                )}
              </div>

              {loadingPending ? (
                <div className="py-8 text-center text-sm text-slate-400">Loading pending orders...</div>
              ) : pendingOrders.length === 0 ? (
                <div className="py-8 text-center text-sm text-slate-500 bg-slate-50 rounded-xl border border-dashed border-slate-200">
                  No unrouted confirmed orders found.
                </div>
              ) : (
                <div className="max-h-[350px] overflow-y-auto divide-y divide-slate-100 pr-1">
                  {pendingOrders.map((order) => {
                    const id = order.orderId || order.OrderId;
                    const isSelected = selectedOrderIds.has(id);

                    return (
                      <div
                        key={id}
                        onClick={() => toggleOrder(id)}
                        className={`p-3 rounded-xl flex items-start gap-3 cursor-pointer transition ${
                          isSelected ? "bg-blue-50/60 border border-blue-200" : "hover:bg-slate-50"
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => {}}
                          className="mt-1 h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
                        />
                        <div className="flex-1 text-xs space-y-1">
                          <div className="flex items-center justify-between font-bold text-slate-800">
                            <span>Buyer: {order.buyerName || order.BuyerName}</span>
                            <span className="bg-emerald-100 text-emerald-800 px-2 py-0.5 rounded-full font-extrabold">
                              {order.quantity || order.Quantity} kg
                            </span>
                          </div>
                          <p className="text-slate-600">
                            Farmer: <span className="font-semibold">{order.farmerName || order.FarmerName}</span> &bull; Crop: {order.cropName || order.CropName}
                          </p>
                          <p className="text-slate-400 text-[11px] truncate">
                            Address: {order.deliveryAddress || order.DeliveryAddress}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}

              {/* Live Vehicle Count Preview & Optimization Trigger */}
              <div className="pt-4 border-t border-slate-100 space-y-4">
                <div className="bg-slate-900 text-white rounded-xl p-4 flex items-center justify-between">
                  <div>
                    <span className="text-xs text-slate-400 uppercase font-semibold tracking-wider">
                      Selected Volume
                    </span>
                    <p className="text-sm font-extrabold text-slate-200">
                      Total qty: {selectedTotalKg.toLocaleString()} kg
                    </p>
                  </div>
                  <div className="text-right">
                    <span className="text-xs text-slate-400 uppercase font-semibold tracking-wider">
                      Vehicle Preview
                    </span>
                    <p className="text-sm font-extrabold text-amber-400">
                      {vehiclePreviewCount} vehicle(s) needed
                    </p>
                  </div>
                </div>

                <button
                  onClick={handleOptimize}
                  disabled={optimizing || selectedOrderIds.size === 0}
                  className={`w-full py-3.5 px-4 rounded-xl text-sm font-bold text-white shadow-lg transition flex items-center justify-center gap-2 ${
                    optimizing || selectedOrderIds.size === 0
                      ? "bg-slate-300 cursor-not-allowed shadow-none"
                      : "bg-blue-600 hover:bg-blue-700 active:bg-blue-800 shadow-blue-500/25"
                  }`}
                >
                  {optimizing ? (
                    <>
                      <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24" fill="none">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      <span>Solving VRP Math...</span>
                    </>
                  ) : (
                    <span>Optimize Route ({selectedOrderIds.size} Orders)</span>
                  )}
                </button>
              </div>

            </div>

          </div>

          {/* Right Column: Map & Route Output (7 cols) */}
          <div className="lg:col-span-7 space-y-6">
            
            {routeResult ? (
              <>
                {/* Summary Strip */}
                <div className="grid grid-cols-3 gap-4">
                  <div className="bg-white rounded-xl p-4 border border-slate-200 text-center shadow-sm">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
                      Total Distance
                    </span>
                    <p className="text-xl font-black text-slate-900 mt-1">
                      {routeResult.totalDistanceKm || routeResult.TotalDistanceKm} km
                    </p>
                  </div>
                  <div className="bg-white rounded-xl p-4 border border-slate-200 text-center shadow-sm">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
                      Vehicles Dispatched
                    </span>
                    <p className="text-xl font-black text-blue-600 mt-1">
                      {routeResult.vehicleCount || routeResult.VehicleCount}
                    </p>
                  </div>
                  <div className="bg-white rounded-xl p-4 border border-slate-200 text-center shadow-sm">
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
                      Route ID
                    </span>
                    <p className="text-xs font-black text-purple-600 font-mono mt-2 truncate">
                      {routeResult.id || routeResult.Id}
                    </p>
                  </div>
                </div>

                {/* Leaflet Map Card */}
                <div className="bg-white rounded-2xl p-4 border border-slate-200 shadow-sm space-y-3">
                  <div className="flex items-center justify-between px-2">
                    <h3 className="text-sm font-bold text-slate-800">
                      Optimized Multi-Vehicle Route Map
                    </h3>
                    <span className="text-xs text-slate-500 font-medium">
                      Depot: {depot.lat}, {depot.lng}
                    </span>
                  </div>
                  
                  <RouteMap depot={depot} stops={flattenedStops} />
                </div>

                {/* Vehicle Stop Sequence Breakdown */}
                <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-sm space-y-6">
                  <h3 className="text-base font-bold text-slate-900 border-b border-slate-100 pb-3">
                    Stop Sequence per Vehicle
                  </h3>

                  {Object.entries(routeResult.stopsByVehicle || routeResult.StopsByVehicle || {}).map(
                    ([vehicleNum, vehicleStops]) => (
                      <div key={vehicleNum} className="space-y-3">
                        <div className="flex items-center justify-between bg-slate-100 px-3 py-2 rounded-lg text-xs font-extrabold text-slate-800">
                          <span>Vehicle #{vehicleNum}</span>
                          <span>{vehicleStops.length} Stops</span>
                        </div>

                        <div className="space-y-2">
                          {vehicleStops.map((stop, idx) => {
                            const seq = stop.sequence || stop.Sequence || idx + 1;
                            const type = (stop.type || stop.Type || "").toLowerCase();
                            const label = stop.label || stop.Label || "";
                            const qty = stop.quantityAtStop || stop.QuantityAtStop || 0;
                            const isPickup = type === "pickup";

                            const isUrgent = stop.isUrgent || stop.IsUrgent || false;
                            const tier = stop.perishabilityTier || stop.PerishabilityTier;
                            const etaStr = stop.estimatedArrival || stop.EstimatedArrival;
                            const formattedEta = formatEta(etaStr);

                            return (
                              <div
                                key={idx}
                                className={`p-3 rounded-xl border text-xs transition space-y-1 ${
                                  isUrgent
                                    ? "bg-red-50/90 border-red-300"
                                    : "bg-white border-slate-100 hover:bg-slate-50"
                                }`}
                              >
                                <div className="flex items-center justify-between">
                                  <div className="flex items-center gap-2">
                                    <span
                                      className={`w-6 h-6 rounded-full flex items-center justify-center font-bold text-white text-[10px] ${
                                        isPickup ? "bg-orange-500" : "bg-emerald-600"
                                      }`}
                                    >
                                      #{seq}
                                    </span>
                                    <span
                                      className={`font-bold uppercase tracking-wider text-[10px] ${
                                        isPickup ? "text-orange-600" : "text-emerald-600"
                                      }`}
                                    >
                                      {isPickup ? "PICKUP" : "DELIVERY"}
                                    </span>
                                    {getPerishabilityBadge(tier)}
                                  </div>

                                  <div className="text-right font-medium text-slate-600">
                                    Load: <span className="font-bold text-slate-900">{qty} kg</span>
                                    {formattedEta && (
                                      <span className="ml-3 font-semibold text-slate-700">
                                        ETA: {formattedEta}
                                      </span>
                                    )}
                                  </div>
                                </div>

                                <div className="pl-8 text-slate-800 font-medium">
                                  {label}
                                </div>

                                {isUrgent && (
                                  <div className="pl-8 text-[11px] font-bold text-red-600">
                                    ⚠️ ETA exceeds freshness window
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    )
                  )}
                </div>

              </>
            ) : (
              <div className="bg-white rounded-2xl p-12 border border-slate-200 shadow-sm text-center space-y-4">
                <div className="w-16 h-16 bg-blue-50 text-blue-600 rounded-full flex items-center justify-center mx-auto text-2xl font-bold">
                  VRP
                </div>
                <h3 className="text-lg font-bold text-slate-900">
                  Ready to Optimize Routes
                </h3>
                <p className="text-xs text-slate-500 max-w-md mx-auto">
                  Select pending orders on the left, choose a saved Route ID from History, or click "Optimize Route" to dispatch the fleet.
                </p>
              </div>
            )}

          </div>

        </div>
      </div>
    </div>
  );
}
