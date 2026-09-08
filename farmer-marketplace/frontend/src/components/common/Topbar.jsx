// frontend/src/components/common/Topbar.jsx
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

export default function Topbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <header className="flex items-center justify-between px-6 py-4 border-b bg-white">
      <div className="text-sm text-gray-500">{user?.role ? `${user.role} Dashboard` : ""}</div>
      <div className="flex items-center gap-4">
        <span className="text-sm font-medium text-gray-700">{user?.name}</span>
        <button onClick={handleLogout} className="text-sm text-red-600 font-medium hover:underline">
          Logout
        </button>
      </div>
    </header>
  );
}