**Layer**	      					**Tech**
*Design*						Figma, Canva


*Frontend*					React (Vite) + React Router, Tailwind CSS v4, shadcn/ui, Leaflet, Recharts, react-i18next


*Backend*						ASP.NET Core Web API (.NET 10) — single flat project (Controllers → Services → Data, no Clean Architecture layering)


*Database*					PostgreSQL (via Entity Framework Core + Npgsql)


*Auth*						JWT (Microsoft.AspNetCore.Authentication.JwtBearer)


*Password Hashing*				BCrypt.Net-Next


*Demand Forecasting*				ML.NET (SSA time-series)


*Route Optimization*				Google.OrTools


*Payments*					Razorpay (.NET SDK, test mode) + Razorpay Route (split payments)


*Notifications*					Twilio WhatsApp API (sandbox)


*API Docs*					Swashbuckle (Swagger — built into ASP.NET Core template)


*Deployment*					Vercel/Netlify (frontend) + Railway/Render (backend + DB)

