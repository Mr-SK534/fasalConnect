# Frontend
npm create vite@latest frontend -- --template react
cd frontend
npm install react-router-dom axios leaflet tailwindcss postcss autoprefixer recharts react-i18next i18next
npx tailwindcss init -p

# Backend
mkdir backend && cd backend
python -m venv venv && source venv/bin/activate
pip install fastapi uvicorn sqlalchemy psycopg2-binary alembic python-jose[cryptography] passlib[bcrypt] prophet ortools python-multipart razorpay twilio --break-system-packages

# backend/.env
DATABASE_URL=postgresql://user:pass@localhost/farmer_marketplace
JWT_SECRET_KEY=your-secret-key
RAZORPAY_KEY_ID=rzp_test_xxxxx
RAZORPAY_KEY_SECRET=xxxxx
RAZORPAY_WEBHOOK_SECRET=xxxxx
TWILIO_ACCOUNT_SID=xxxxx
TWILIO_AUTH_TOKEN=xxxxx
TWILIO_WHATSAPP_NUMBER=+14155238886  # Twilio sandbox number

# frontend/.env
VITE_API_BASE_URL=http://localhost:8000
VITE_RAZORPAY_KEY_ID=rzp_test_xxxxx

