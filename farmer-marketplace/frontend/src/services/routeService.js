import api from "./api";

export const optimizeRoute = (orderIds, deliveryHubLocation) =>
  api
    .post("/routes/optimize", { orderIds, deliveryHubLocation })
    .then((response) => response.data);

export const getRoute = (routeId) =>
  api.get(`/routes/${routeId}`).then((response) => response.data);

export const getBatchStatus = () =>
  api.get("/routes/batch-status").then((response) => response.data);

export const runBatch = () =>
  api.post("/routes/batch/run").then((response) => response.data);
