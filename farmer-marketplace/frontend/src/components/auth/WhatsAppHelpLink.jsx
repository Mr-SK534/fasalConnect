// frontend/src/components/auth/WhatsAppHelpLink.jsx
//
// The "Need help? Chat on WhatsApp" link at the bottom of the auth
// screens. Pass a real wa.me link once your support number is set up.

export default function WhatsAppHelpLink({ phoneNumber = "911234567890" }) {
  return (
    <a
      href={`https://wa.me/${phoneNumber}`}
      target="_blank"
      rel="noopener noreferrer"
      className="flex items-center justify-center gap-2 text-sm text-white/70 hover:text-white transition-colors"
    >
      <svg
        className="h-4 w-4"
        fill="currentColor"
        viewBox="0 0 24 24"
        aria-hidden="true"
      >
        <path d="M12.04 2C6.58 2 2.13 6.45 2.13 11.91c0 1.75.46 3.45 1.32 4.95L2 22l5.29-1.39a9.9 9.9 0 0 0 4.75 1.21h.01c5.46 0 9.91-4.45 9.91-9.91 0-2.65-1.03-5.14-2.9-7.01A9.82 9.82 0 0 0 12.04 2z" />
      </svg>
      Need help? Chat on WhatsApp
    </a>
  );
}
