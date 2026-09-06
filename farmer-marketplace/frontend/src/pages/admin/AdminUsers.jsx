import { useCallback, useEffect, useState } from "react";
import { toast } from "react-hot-toast";
import {
  getAdminUserById,
  getAdminUsers,
  createAdminUser,
  suspendUser,
} from "../../services/adminService";

const roles = {
  Farmer: "bg-green-100 text-green-700",
  Buyer: "bg-blue-100 text-blue-700",
  FpoAdmin: "bg-purple-100 text-purple-700",
  PlatformAdmin: "bg-red-100 text-red-700",
};
const unwrap = (data) =>
  Array.isArray(data) ? data : data?.items || data?.users || [];
const getMeta = (data, page, pageSize, count) => ({
  page: data?.page || page,
  pageSize: data?.pageSize || pageSize,
  totalCount: data?.totalCount ?? count,
  totalPages:
    data?.totalPages ||
    Math.max(1, Math.ceil((data?.totalCount ?? count) / pageSize)),
});
const location = (user) =>
  [user.village, user.district, user.state].filter(Boolean).join(", ") ||
  user.location ||
  "-";
const mask = (value) => (value ? `****${String(value).slice(-4)}` : "-");

export default function AdminUsers() {
  const [users, setUsers] = useState([]);
  const [meta, setMeta] = useState({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 1,
  });
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [detail, setDetail] = useState(null);
  const [suspendTarget, setSuspendTarget] = useState(null);
  const [reason, setReason] = useState("");
  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState("");
  const [createForm, setCreateForm] = useState({
    name: "",
    phone: "",
    email: "",
    password: "",
    role: "Farmer",
    preferredLanguage: "en",
    village: "",
    district: "",
    state: "",
    pincode: "",
    primaryCrops: "",
    bankAccountNumber: "",
    bankIfsc: "",
    accountHolderName: "",
    upiId: "",
    businessName: "",
    gstNumber: "",
    deliveryAddress: "",
  });
  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await getAdminUsers({
        role: role || undefined,
        search: search || undefined,
        page,
        pageSize: 20,
      });
      setUsers(unwrap(data));
      setMeta(getMeta(data, page, 20, unwrap(data).length));
    } catch (requestError) {
      setError(requestError.response?.data?.message || "Could not load users.");
    } finally {
      setLoading(false);
    }
  }, [page, role, search]);
  useEffect(() => {
    // Synchronize the table with the authenticated admin API.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);
  const view = async (user) => {
    try {
      setDetail(await getAdminUserById(user.id));
    } catch {
      setDetail(user);
    }
  };
  const reactivate = async (user) => {
    try {
      await suspendUser(user.id, false, "Reactivated by admin");
      toast.success("User reactivated");
      await load();
    } catch (requestError) {
      toast.error(
        requestError.response?.data?.message || "Could not reactivate user.",
      );
    }
  };
  const suspend = async (event) => {
    event.preventDefault();
    try {
      await suspendUser(suspendTarget.id, true, reason);
      toast.success("User suspended");
      setSuspendTarget(null);
      setReason("");
      await load();
    } catch (requestError) {
      toast.error(
        requestError.response?.data?.message || "Could not suspend user.",
      );
    }
  };
  const createUser = async (event) => {
    event.preventDefault();
    setCreating(true);
    setCreateError("");
    try {
      await createAdminUser({
        ...createForm,
        email: createForm.email || undefined,
        village: createForm.village || undefined,
        district: createForm.district || undefined,
        state: createForm.state || undefined,
        pincode: createForm.pincode || undefined,
        primaryCrops: createForm.primaryCrops || undefined,
        businessName: createForm.businessName || undefined,
        deliveryAddress: createForm.deliveryAddress || undefined,
      });
      setShowCreate(false);
      setCreateForm({
        ...createForm,
        name: "",
        phone: "",
        email: "",
        password: "",
      });
      toast.success("User account created");
      await load();
    } catch (requestError) {
      setCreateError(
        requestError.response?.data?.message || "Could not create user.",
      );
    } finally {
      setCreating(false);
    }
  };
  return (
    <div className="mx-auto max-w-7xl space-y-5">
      <header>
        <p className="text-xs font-bold uppercase tracking-wider text-green-700">
          Platform control
        </p>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h2 className="text-3xl font-black text-[#163820]">All Users</h2>
          <button
            type="button"
            onClick={() => {
              setCreateError("");
              setShowCreate(true);
            }}
            className="rounded-lg bg-green-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-green-700"
          >
            + Add User
          </button>
        </div>
      </header>
      <div className="flex flex-col gap-3 sm:flex-row">
        <input
          value={search}
          onChange={(event) => {
            setPage(1);
            setSearch(event.target.value);
          }}
          placeholder="Search name, phone, or email"
          className="flex-1 rounded-lg border border-gray-200 bg-white px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400"
        />
        <select
          value={role}
          onChange={(event) => {
            setPage(1);
            setRole(event.target.value);
          }}
          className="rounded-lg border border-gray-200 bg-white px-4 py-3"
        >
          <option value="">All roles</option>
          {Object.keys(roles).map((item) => (
            <option key={item}>{item}</option>
          ))}
        </select>
      </div>
      {error && (
        <p className="rounded-lg bg-red-50 p-4 text-red-700">{error}</p>
      )}
      <section className="overflow-x-auto rounded-xl border border-gray-100 bg-white shadow-sm">
        <table className="w-full border-collapse text-left text-sm">
          <thead className="bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              {[
                "Name",
                "Phone",
                "Email",
                "Role",
                "Location",
                "Profile",
                "Joined",
                "Status",
                "Actions",
              ].map((heading) => (
                <th key={heading} className="p-3">
                  {heading}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan="9" className="p-8 text-center text-slate-500">
                  Loading users...
                </td>
              </tr>
            ) : (
              users.map((user) => (
                <tr
                  key={user.id}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="p-3 font-semibold">{user.name}</td>
                  <td className="p-3">{user.phone || "—"}</td>
                  <td className="p-3">{user.email || "—"}</td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-semibold ${roles[user.role] || "bg-gray-100 text-gray-700"}`}
                    >
                      {user.role}
                    </span>
                  </td>
                  <td className="p-3">{location(user)}</td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-semibold ${user.isProfileComplete ? "bg-green-100 text-green-700" : "bg-amber-100 text-amber-700"}`}
                    >
                      {user.isProfileComplete ? "Complete" : "Incomplete"}
                    </span>
                  </td>
                  <td className="p-3">
                    {new Date(user.createdAt).toLocaleDateString("en-IN")}
                  </td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-semibold ${user.suspended ? "bg-red-100 text-red-700" : "bg-green-100 text-green-700"}`}
                    >
                      {user.suspended ? "Suspended" : "Active"}
                    </span>
                  </td>
                  <td className="flex gap-2 p-3">
                    <button
                      type="button"
                      onClick={() => view(user)}
                      className="rounded-lg bg-gray-100 px-3 py-1 text-sm font-medium"
                    >
                      View
                    </button>
                    {user.suspended ? (
                      <button
                        type="button"
                        onClick={() => reactivate(user)}
                        className="rounded-lg bg-green-100 px-3 py-1 text-sm font-medium text-green-700"
                      >
                        Reactivate
                      </button>
                    ) : (
                      <button
                        type="button"
                        onClick={() => setSuspendTarget(user)}
                        className="rounded-lg bg-red-100 px-3 py-1 text-sm font-medium text-red-700"
                      >
                        Suspend
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </section>
      <div className="flex items-center justify-between text-sm text-slate-600">
        <span>
          Showing {users.length ? (meta.page - 1) * meta.pageSize + 1 : 0}-
          {Math.min(meta.page * meta.pageSize, meta.totalCount)} of{" "}
          {meta.totalCount} users
        </span>
        <div className="flex gap-2">
          <button
            disabled={page <= 1}
            onClick={() => setPage(page - 1)}
            className="rounded-lg border px-3 py-1 disabled:opacity-40"
          >
            Previous
          </button>
          <span className="px-2 py-1">
            Page {meta.page} of {meta.totalPages}
          </span>
          <button
            disabled={page >= meta.totalPages}
            onClick={() => setPage(page + 1)}
            className="rounded-lg border px-3 py-1 disabled:opacity-40"
          >
            Next
          </button>
        </div>
      </div>
      {detail && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl">
            <div className="flex justify-between">
              <h3 className="text-xl font-bold">User details</h3>
              <button onClick={() => setDetail(null)}>×</button>
            </div>
            <div className="mt-4 grid gap-2 text-sm">
              {[
                ["Name", detail.name],
                ["Phone", detail.phone],
                ["Email", detail.email || "—"],
                ["Role", detail.role],
                ["Village", detail.village],
                ["District", detail.district],
                ["State", detail.state],
                ["Pincode", detail.pincode],
                ["Language", detail.preferredLanguage],
                ["Crops", detail.primaryCrops],
                ["Business", detail.businessName],
                ["Delivery", detail.deliveryAddress],
                ["Bank", mask(detail.bankAccountNumber)],
                ["IFSC", detail.bankIfsc],
                ["Account holder", detail.accountHolderName],
                [
                  "Profile",
                  detail.isProfileComplete ? "Complete" : "Incomplete",
                ],
              ].map(([label, value]) => (
                <p key={label}>
                  <strong>{label}:</strong> {value || "—"}
                </p>
              ))}
            </div>
            {!detail.suspended && (
              <button
                onClick={() => {
                  setSuspendTarget(detail);
                  setDetail(null);
                }}
                className="mt-5 rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white"
              >
                Suspend Account
              </button>
            )}
          </div>
        </div>
      )}
      {suspendTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <form
            onSubmit={suspend}
            className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-2xl"
          >
            <h3 className="text-xl font-bold">Suspend account</h3>
            <p className="mt-2 text-sm text-red-600">
              This will block the user from logging in.
            </p>
            <textarea
              required
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              rows="4"
              placeholder="Reason"
              className="mt-4 w-full rounded-lg border border-gray-200 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400"
            />
            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setSuspendTarget(null)}
                className="rounded-lg border px-4 py-2"
              >
                Cancel
              </button>
              <button className="rounded-lg bg-red-600 px-4 py-2 font-medium text-white">
                Confirm Suspend
              </button>
            </div>
          </form>
        </div>
      )}
      {showCreate && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <form
            onSubmit={createUser}
            className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl"
          >
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-bold">Create User Account</h3>
              <button type="button" onClick={() => setShowCreate(false)}>
                ×
              </button>
            </div>
            {createError && (
              <p className="mt-3 rounded-lg bg-red-50 p-3 text-sm text-red-700">
                {createError}
              </p>
            )}
            <div className="mt-4 grid gap-3 sm:grid-cols-2">
              {[
                ["name", "Full name", true],
                ["phone", "Phone", true],
                ["email", "Email", false],
                ["password", "Password", true],
                ["village", "Village", false],
                ["district", "District", false],
                ["state", "State", false],
                ["pincode", "Pincode", false],
                ["primaryCrops", "Primary crops", false],
                ["bankAccountNumber", "Bank account number", false],
                ["bankIfsc", "IFSC", false],
                ["accountHolderName", "Account holder", false],
                ["upiId", "UPI ID", false],
                ["businessName", "Business name", false],
                ["gstNumber", "GST number", false],
              ].map(([name, label, required]) => (
                <label
                  key={name}
                  className="text-sm font-medium text-slate-700"
                >
                  {label}
                  <input
                    required={required}
                    minLength={name === "password" ? 6 : undefined}
                    type={
                      name === "password"
                        ? "password"
                        : name === "email"
                          ? "email"
                          : "text"
                    }
                    value={createForm[name]}
                    onChange={(event) =>
                      setCreateForm({
                        ...createForm,
                        [name]: event.target.value,
                      })
                    }
                    className="mt-1 w-full rounded-lg border border-gray-200 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400"
                  />
                </label>
              ))}
              <label className="text-sm font-medium text-slate-700">
                Role
                <select
                  value={createForm.role}
                  onChange={(event) =>
                    setCreateForm({ ...createForm, role: event.target.value })
                  }
                  className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-4 py-3"
                >
                  <option>Farmer</option>
                  <option>Buyer</option>
                  <option>FpoAdmin</option>
                </select>
              </label>
              <label className="text-sm font-medium text-slate-700 sm:col-span-2">
                Delivery address
                <textarea
                  value={createForm.deliveryAddress}
                  onChange={(event) =>
                    setCreateForm({
                      ...createForm,
                      deliveryAddress: event.target.value,
                    })
                  }
                  rows="2"
                  className="mt-1 w-full rounded-lg border border-gray-200 px-4 py-3 focus:outline-none focus:ring-2 focus:ring-green-400"
                />
              </label>
            </div>
            <div className="mt-5 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setShowCreate(false)}
                className="rounded-lg border px-4 py-2"
              >
                Cancel
              </button>
              <button
                disabled={creating}
                className="rounded-lg bg-green-600 px-4 py-2 font-medium text-white hover:bg-green-700 disabled:opacity-60"
              >
                {creating ? "Creating..." : "Create User"}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
