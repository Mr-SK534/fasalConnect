// frontend/src/components/common/Sidebar.jsx
import { NavLink } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { ROLES } from "../../utils/roles";

const NAV_BY_ROLE = {
  [ROLES.FARMER]: [
    { label: "Dashboard", path: "/farmer/dashboard" },
    { label: "List Product", path: "/farmer/list-product" },
    { label: "My Orders", path: "/farmer/orders" },
  ],
  [ROLES.BUYER]: [
    { label: "Browse", path: "/buyer/browse" },
    { label: "Cart", path: "/buyer/cart" },
    { label: "My Orders", path: "/buyer/orders" },
  ],
  [ROLES.FPO_ADMIN]: [
    { label: "Dashboard", path: "/fpo-admin/dashboard" },
    { label: "Manage Farmers", path: "/fpo-admin/farmers" },
  ],
  [ROLES.PLATFORM_ADMIN]: [
    { label: "Overview", path: "/admin/dashboard" },
  ],
};

export default function Sidebar() {
  const { user } = useAuth();
  const navItems = NAV_BY_ROLE[user?.role] || [];

  return (
    <aside className="w-56 bg-green-950 text-white min-h-screen p-4 flex-shrink-0">
      <div className="text-xl font-bold mb-8 px-2">FasalConnect</div>
      <nav className="space-y-1">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              `block px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? "bg-green-700 text-white"
                  : "text-green-100/70 hover:bg-green-900 hover:text-white"
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}