// frontend/src/components/common/WhatsAppFloatingButton.jsx

import React, { useState } from "react";
import { FaWhatsapp } from "react-icons/fa";

export default function WhatsAppFloatingButton({
  phoneNumber = "911234567890",
  message = "Hello FasalConnect Support! I need assistance with marketplace orders and services.",
}) {
  const [showTooltip, setShowTooltip] = useState(false);

  const encodedMessage = encodeURIComponent(message);
  const whatsappUrl = `https://wa.me/${phoneNumber}?text=${encodedMessage}`;

  return (
    <div
      className="fixed bottom-6 right-6 z-50 flex items-center gap-3"
      onMouseEnter={() => setShowTooltip(true)}
      onMouseLeave={() => setShowTooltip(false)}
    >
      {/* Tooltip Popup */}
      <div
        className={`hidden sm:flex items-center gap-2 rounded-xl bg-slate-900 px-3.5 py-2 text-xs font-bold text-white shadow-xl transition-all duration-300 ${
          showTooltip
            ? "opacity-100 translate-x-0 pointer-events-auto"
            : "opacity-0 translate-x-2 pointer-events-none"
        }`}
      >
        <span className="h-2 w-2 rounded-full bg-emerald-400 animate-pulse"></span>
        <span>Chat on WhatsApp</span>
      </div>

      {/* Floating Action Button */}
      <a
        href={whatsappUrl}
        target="_blank"
        rel="noopener noreferrer"
        aria-label="Chat with FasalConnect Support on WhatsApp"
        className="group relative flex h-14 w-14 items-center justify-center rounded-full bg-[#25D366] text-white shadow-2xl shadow-green-500/50 border-2 border-white/40 hover:bg-[#20ba5a] hover:scale-110 active:scale-95 transition-all duration-300"
      >
        {/* Pulse Ring */}
        <span className="absolute -inset-1 rounded-full bg-[#25D366] opacity-40 animate-ping group-hover:opacity-0"></span>

        {/* Icon */}
        <FaWhatsapp size={32} className="relative z-10 drop-shadow-md" />

        {/* Notification Online Indicator */}
        <span className="absolute top-0 right-0 h-3.5 w-3.5 rounded-full bg-emerald-400 border-2 border-white"></span>
      </a>
    </div>
  );
}
