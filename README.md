# MediCare – Clinic Management & Appointment System

## Authentication & Role-Based Access Control (RBAC)

### 1. Default Roles
The system automatically provisions three main roles upon application startup:
- `Admin`: Full clinic management access (represents Clinic Assistant in ERD).
- `Doctor`: Doctor account (subject to administrative approval gate).
- `Patient`: Patient self-service registration and booking.

---

### 2. Administrator Account Provisioning (Secure Configuration)
For security compliance, default administrative credentials are never committed in plaintext to source control.

To seed the initial administrator account (`admin@medicare.com`), configure the secret `Seed:AdminPassword` locally using one of the following methods:

#### Method A: .NET User Secrets (Recommended for Local Development)
The `MediCare.Web` project is configured with a `UserSecretsId`. If initializing from scratch:
```bash
cd Project/Project_Back/MediCare/MediCare.Web
dotnet user-secrets init
dotnet user-secrets set "Seed:AdminPassword" "<YourStrongPasswordHere>"
```

#### Method B: Environment Variable
You can set an environment variable before launching the application:
```bash
# Windows PowerShell
$env:Seed__AdminPassword = "<YourStrongPasswordHere>"

# Windows CMD
set Seed__AdminPassword=<YourStrongPasswordHere>
```

> **Important Security Policy:**  
> If `Seed:AdminPassword` is not configured or left empty, the application will **skip** creating the administrator account and log a warning explaining how to configure it. No hardcoded or default passwords are used.

---

### 3. Role-Based Access Control (RBAC) & Testing
- **Protected Endpoint for Testing:**  
  The endpoint `/Account/AdminOnly` is protected with `[Authorize(Roles = "Admin")]`.
- **Access Denied Redirection:**  
  When an authenticated user without the `Admin` role (e.g. a `Patient` or `Doctor`) attempts to access `/Account/AdminOnly`, ASP.NET Core redirects the user to `/Account/AccessDenied` (HTTP 403 Forbidden).

---

### 4. Database Migrations & Initialization
The database schema and seed data are applied automatically on application startup using Entity Framework Core Migrations (`MigrateAsync()`).

To manually apply migrations via the EF Core CLI:
```bash
dotnet ef database update --project Project/Project_Back/MediCare/MediCare.Data --startup-project Project/Project_Back/MediCare/MediCare.Web
```
