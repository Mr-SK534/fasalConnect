import api from "./api";

export const getSummary = () =>
  api.get("/admin/summary").then((response) => response.data);
export const getAdminUsers = (params) =>
  api.get("/admin/users", { params }).then((response) => response.data);
export const getAdminUserById = (id) =>
  api.get(`/admin/users/${id}`).then((response) => response.data);
export const suspendUser = (id, suspended, reason) =>
  api
    .put(`/admin/users/${id}/suspend`, { suspended, reason })
    .then((response) => response.data);
export const getAdminOrders = (params) =>
  api.get("/admin/orders", { params }).then((response) => response.data);
export const overrideOrderStatus = (id, status, note) =>
  api
    .put(`/admin/orders/${id}/override-status`, { status, note })
    .then((response) => response.data);
export const triggerPaymentSplit = (orderId) =>
  api.post("/payments/split", { orderId }).then((response) => response.data);
export const getAuditLog = (params) =>
  api.get("/admin/audit-log", { params }).then((response) => response.data);
