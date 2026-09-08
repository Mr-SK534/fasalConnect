// frontend/src/services/routeService.js

import api from "./api";

export const optimizeRoute = (orderIds, depotLat, depotLng) =>
  api
    .post("/routes/optimize", { orderIds, depotLat, depotLng })
    .then((res) => res.data);

export const getRoute = (routeId) =>
  api.get(`/routes/${routeId}`).then((res) => res.data);

export const getAllRoutes = () =>
  api.get("/routes").then((res) => res.data);

export const runBatchWindow = (windowName = "morning") =>
  api.post(`/routes/batch-window?windowName=${windowName}`).then((res) => res.data);

export const getPendingOrders = () =>
  api.get("/routes/pending").then((res) => res.data);
