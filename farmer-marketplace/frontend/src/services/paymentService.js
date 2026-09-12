import api from "./api";

export const createRazorpayOrder = (orderId, amount) =>
  api
    .post("/payments/create-order", { orderId, amount })
    .then((response) => response.data);

export const confirmPayment = (data) =>
  api.post("/payments/confirm", data).then((response) => response.data);

export const failPayment = (data) =>
  api.post("/payments/fail", data).then((response) => response.data);
