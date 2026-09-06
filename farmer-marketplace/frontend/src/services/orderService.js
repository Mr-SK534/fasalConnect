import api from "./api";

export const placeOrder = (payload) =>
  api.post("/orders", payload).then((response) => response.data);

export const getBuyerOrders = (buyerId) =>
  api.get(`/orders/buyer/${buyerId}`).then((response) => response.data);

export const getOrderById = (id) =>
  api.get(`/orders/${id}`).then((response) => response.data);

export const getFarmerOrders = (farmerId) =>
  api.get(`/orders/farmer/${farmerId}`).then((response) => response.data);

export const updateOrderStatus = (orderId, status) =>
  api
    .put(`/orders/${orderId}/status`, { status })
    .then((response) => response.data);
