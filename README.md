# Payroll SaaS — Local Development Setup

## Prerequisites
| Tool | Version |
|------|---------|
| .NET SDK | 8.0 |
| Node.js | 18+ |
| MySQL | 8.0 |

---

## 1. Database Setup

Create the MySQL database:
```sql
CREATE DATABASE payroll_saas;
```

---

## 2. Backend Setup

```bash
cd PayrollAPI
```

Edit **`appsettings.Development.json`** — set your MySQL password:
```json
"DefaultConnection": "Server=localhost;Port=3306;Database=payroll_saas;User=root;Password=YOUR_PASSWORD;"
```

Also set a real JWT secret in `appsettings.json` (min 32 chars):
```json
"SecretKey": "MyStrongKeyAtLeast32CharactersLong!"
```

Install packages and run migrations:
```bash
dotnet restore
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
```

Start the API:
```bash
dotnet run
# API runs on http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

---

## 3. Frontend Setup

```bash
cd payroll-client
npm install
npm run dev
# Frontend runs on http://localhost:5173
```

The Vite dev server proxies all `/api/*` requests to `http://localhost:5000` automatically.

---

## 4. First Login

1. Open `http://localhost:5173/register`
2. Register your company + admin account
3. Login at `http://localhost:5173/login`

---

## Project Structure

```
Payroll Software/
├── PayrollAPI/               ← ASP.NET Core 8 backend
│   ├── Controllers/          ← HTTP endpoints
│   ├── Services/             ← Business logic
│   ├── Repositories/         ← DB access layer
│   ├── Models/               ← EF Core entities
│   ├── DTOs/                 ← Request/Response objects
│   ├── Interfaces/           ← Contracts
│   ├── Data/                 ← DbContext + Migrations
│   ├── Middleware/           ← Global exception handler
│   └── Helpers/              ← JWT claims, password hash
│
├── payroll-client/           ← React + Vite frontend
│   └── src/
│       ├── app/              ← Redux store + RTK Query base
│       ├── features/         ← API slices per module
│       ├── pages/            ← Route-level pages
│       ├── components/       ← Reusable UI components
│       ├── layouts/          ← Sidebar + Auth layouts
│       ├── routes/           ← React Router config
│       ├── hooks/            ← useAuth, usePagination
│       └── utils/            ← formatters, helpers
│
├── nginx.conf                ← Production Nginx config
├── payroll-api.service       ← Systemd unit file for Linux VPS
└── README.md
```

---

## API Endpoints Summary

| Module | Method | Route |
|--------|--------|-------|
| Auth | POST | `/api/auth/register` |
| Auth | POST | `/api/auth/login` |
| Employees | GET/POST | `/api/employees` |
| Employees | PUT/DELETE | `/api/employees/{id}` |
| Departments | GET/POST | `/api/employees/departments` |
| Attendance | GET/POST | `/api/attendance` |
| Attendance | POST | `/api/attendance/bulk` |
| Attendance | GET | `/api/attendance/monthly-summary` |
| Payroll | POST | `/api/payroll/process` |
| Payroll | GET | `/api/payroll/runs` |
| Payslips | GET | `/api/payroll/payslips/{id}` |
| Payslips | GET | `/api/payroll/payslips/{id}/pdf` |
| Dashboard | GET | `/api/dashboard` |
| Settings | GET/PUT | `/api/settings/company` |
| Settings | GET/PUT | `/api/settings/salary-rules` |

---

## Linux VPS Deployment

```bash
# 1. Publish backend
cd PayrollAPI
dotnet publish -c Release -o /var/www/payroll-api

# 2. Build frontend
cd ../payroll-client
npm run build
cp -r dist/* /var/www/payroll-client/

# 3. Install systemd service
sudo cp ../payroll-api.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable payroll-api
sudo systemctl start payroll-api

# 4. Configure Nginx
sudo cp ../nginx.conf /etc/nginx/sites-available/payroll
sudo ln -s /etc/nginx/sites-available/payroll /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

---

## Roles & Permissions

| Feature | Admin | HR | Employee |
|---------|-------|----|----------|
| View employees | ✓ | ✓ | ✓ |
| Add/Edit employees | ✓ | ✓ | ✗ |
| Delete employees | ✓ | ✗ | ✗ |
| Mark attendance | ✓ | ✓ | ✗ |
| Process payroll | ✓ | ✓ | ✗ |
| Mark payroll as paid | ✓ | ✗ | ✗ |
| Update settings | ✓ | ✗ | ✗ |
