import api from "./api";

export const createRazorpayOrder = (orderId, amount) =>
  api
    .post("/payments/create-order", { orderId, amount })
    .then((response) => response.data);
