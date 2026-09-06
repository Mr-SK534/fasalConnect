import api from "./api";

export const optimizeRoute = (orderIds, deliveryHubLocation) =>
  api
    .post("/routes/optimize", { orderIds, deliveryHubLocation })
    .then((response) => response.data);

export const getRoute = (routeId) =>
  api.get(`/routes/${routeId}`).then((response) => response.data);
