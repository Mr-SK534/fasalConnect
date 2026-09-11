import { useState } from "react";
import { toast } from "react-hot-toast";
import {
  FiPhone,
  FiMail,
  FiMapPin,
  FiClock,
  FiSend,
  FiMessageSquare,
  FiUserCheck,
  FiHelpCircle,
} from "react-icons/fi";
import { useAuth } from "../../hooks/useAuth";

export default function ContactUs() {
  const { user } = useAuth();
  const [form, setForm] = useState({
    subject: "",
    category: "General Inquiry",
    message: "",
  });
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.subject.trim() || !form.message.trim()) {
      toast.error("Please fill in both subject and message.");
      return;
    }
    setSubmitting(true);
    setTimeout(() => {
      setSubmitting(false);
      toast.success("Your message has been sent to Platform Administration!");
      setForm({ subject: "", category: "General Inquiry", message: "" });
    }, 600);
  };

  const adminContacts = [
    {
      name: "Platform Chief Administrator",
      role: "System Governance & Escrow Disputes",
      phone: "+91 8582992746",
      email: "admin@fasalconnect.com",
    },
    {
      name: "Farmer & FPO Helpline Lead",
      role: "Onboarding & Technical Support",
      phone: "+91 98123 4567",
      email: "support@fasalconnect.gov.in",
    },
    {
      name: "Logistics & Route Coordinator",
      role: "Pickup Scheduling & VRP Assistance",
      phone: "+91 97654 32109",
      email: "logistics@fasalconnect.com",
    },
  ];

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      {/* Header Banner */}
      <section className="rounded-2xl bg-gradient-to-r from-[#2e7d32] via-[#246b28] to-[#1b5e20] p-6 text-white shadow-md sm:p-8">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-white/10 text-2xl backdrop-blur-xs">
            📞
          </div>
          <div>
            <p className="text-xs font-extrabold uppercase tracking-wider text-[#f5d77f]">
              FasalConnect Help & Support
            </p>
            <h2 className="mt-0.5 text-2xl font-black sm:text-3xl tracking-tight">
              Contact Platform Administration
            </h2>
          </div>
        </div>
        <p className="mt-3 max-w-2xl text-sm leading-relaxed text-emerald-100 font-normal">
          Need help with produce listings, payment escrow, logistics routes, or account verification? Our admin team is available to assist you.
        </p>
      </section>

      {/* Main Grid: Contact Cards & Admin Details */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Left Column: Admin Contact Details */}
        <div className="space-y-4 lg:col-span-1">
          <div className="rounded-2xl border border-[#eadaaf] bg-white p-5 shadow-xs sm:p-6">
            <h3 className="flex items-center gap-2 text-base font-extrabold text-[#163820]">
              <FiUserCheck className="text-[#2e7d32]" size={20} />
              <span>Admin Key Contacts</span>
            </h3>
            <p className="mt-1 text-xs text-slate-500">
              Direct contacts for administrative assistance.
            </p>

            <div className="mt-4 divide-y divide-[#eee5cc]">
              {adminContacts.map((contact, idx) => (
                <div key={idx} className="py-3.5 first:pt-0 last:pb-0">
                  <p className="text-sm font-bold text-slate-900">
                    {contact.name}
                  </p>
                  <p className="text-xs font-semibold text-[#2e7d32]">
                    {contact.role}
                  </p>

                  <div className="mt-2 space-y-1.5 text-xs text-slate-600">
                    <a
                      href={`tel:${contact.phone.replace(/\s+/g, "")}`}
                      className="flex items-center gap-2 font-medium transition hover:text-[#2e7d32]"
                    >
                      <FiPhone className="text-slate-400" />
                      <span>{contact.phone}</span>
                    </a>
                    <a
                      href={`mailto:${contact.email}`}
                      className="flex items-center gap-2 font-medium transition hover:text-[#2e7d32]"
                    >
                      <FiMail className="text-slate-400" />
                      <span>{contact.email}</span>
                    </a>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Support Hours & Location Info */}
          <div className="rounded-2xl border border-[#eadaaf] bg-[#fdfbf3] p-5 text-xs shadow-xs space-y-3">
            <div className="flex items-start gap-3">
              <FiClock className="mt-0.5 shrink-0 text-[#2e7d32]" size={16} />
              <div>
                <p className="font-bold text-slate-900">Operating Hours</p>
                <p className="mt-0.5 text-slate-600">
                  Monday – Saturday: 8:00 AM – 8:00 PM IST
                </p>
                <p className="text-slate-500">Sunday: Closed (Emergency Escrow On-Call)</p>
              </div>
            </div>

            <div className="border-t border-[#eee5cc] pt-3 flex items-start gap-3">
              <FiMapPin className="mt-0.5 shrink-0 text-[#2e7d32]" size={16} />
              <div>
                <p className="font-bold text-slate-900">Headquarters Address</p>
                <p className="mt-0.5 text-slate-600">
                   FasalConnect Agritech Hub, New Delhi, India
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Right Column: Send Direct Inquiry Form */}
        <div className="lg:col-span-2">
          <div className="rounded-2xl border border-[#eadaaf] bg-white p-6 shadow-xs sm:p-8">
            <div className="flex items-center justify-between border-b border-[#eee5cc] pb-4">
              <div>
                <h3 className="flex items-center gap-2 text-lg font-bold text-[#163820]">
                  <FiMessageSquare className="text-[#2e7d32]" size={20} />
                  <span>Send Direct Inquiry to Admin</span>
                </h3>
                <p className="mt-0.5 text-xs text-slate-500">
                  Submit a message and our team will get back to you within 24 hours.
                </p>
              </div>
              <span className="hidden sm:inline-flex items-center gap-1 rounded-full bg-[#f4e8c5] px-3 py-1 text-[11px] font-bold text-[#284a2f]">
                <FiHelpCircle size={14} /> Priority Support
              </span>
            </div>

            <form onSubmit={handleSubmit} className="mt-6 space-y-4 text-xs font-semibold text-slate-800">
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  Your Name
                  <input
                    type="text"
                    disabled
                    value={user?.name || "Logged-in User"}
                    className="mt-1.5 w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-2.5 font-medium text-slate-600 outline-none cursor-not-allowed"
                  />
                </label>

                <label className="block">
                  Your Account Role
                  <input
                    type="text"
                    disabled
                    value={user?.role || "Member"}
                    className="mt-1.5 w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-2.5 font-medium text-slate-600 outline-none cursor-not-allowed"
                  />
                </label>
              </div>

              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  Inquiry Category
                  <select
                    value={form.category}
                    onChange={(e) => setForm({ ...form, category: e.target.value })}
                    className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] bg-white px-3.5 py-2.5 font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
                  >
                    <option>General Inquiry</option>
                    <option>Produce Listing Issue</option>
                    <option>Payment & Escrow Inquiry</option>
                    <option>Logistics & Route Delivery</option>
                    <option>FPO Network Management</option>
                    <option>Technical Bug Report</option>
                  </select>
                </label>

                <label className="block">
                  Subject
                  <input
                    required
                    type="text"
                    placeholder="e.g. Issue with payment verification for Order #104"
                    value={form.subject}
                    onChange={(e) => setForm({ ...form, subject: e.target.value })}
                    className="mt-1.5 w-full rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
                  />
                </label>
              </div>

              <label className="block">
                Detailed Message
                <textarea
                  required
                  rows="5"
                  placeholder="Describe your query or issue in detail..."
                  value={form.message}
                  onChange={(e) => setForm({ ...form, message: e.target.value })}
                  className="mt-1.5 w-full resize-none rounded-xl border border-[#d2c5a2] px-3.5 py-2.5 font-medium text-slate-900 outline-none focus:border-[#2e7d32] focus:ring-2 focus:ring-[#2e7d32]/20"
                />
              </label>

              <div className="flex justify-end pt-2">
                <button
                  type="submit"
                  disabled={submitting}
                  className="flex items-center gap-2 rounded-xl bg-[#2e7d32] px-6 py-3 font-bold text-white shadow-xs transition hover:bg-[#246b28] hover:shadow-sm disabled:opacity-60"
                >
                  <FiSend size={16} />
                  <span>{submitting ? "Sending..." : "Submit Message to Admin"}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
