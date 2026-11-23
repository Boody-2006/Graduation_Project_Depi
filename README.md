# MediCare – Clinic Management & Appointment System

## Authentication & Role-Based Access Control (RBAC)

### 1. Default Roles
The system automatically provisions three main roles upon application startup:
- `Admin`: Full management access (represents Clinic Assistant in ERD).
- `Doctor`: Doctor account (subject to administrative approval gate).
- `Patient`: Patient self-service registration and booking.

---

### 2. Default Administrator Account Configuration
An initial administrator account is automatically seeded if it does not already exist:
- **Email:** `admin@medicare.com`
- **Password:** Configured via `Seed:AdminPassword`.

#### Setting the Admin Password Locally:
For security best practices, sensitive passwords should not be committed to source control. You can configure the seed password locally using either:

1. **.NET Secret Manager (User Secrets) - Recommended:**
   ```bash
   cd Project/Project_Back/MediCare/MediCare.Web
   dotnet user-secrets set "Seed:AdminPassword" "<YourStrongPasswordHere>"
   ```

2. **Environment Variable:**
   ```bash
   # Windows PowerShell
   $env:Seed__AdminPassword = "<YourStrongPasswordHere>"

   # Windows CMD
   set Seed__AdminPassword=<YourStrongPasswordHere>
   ```

3. **Development Local Fallback:**
   If `Seed:AdminPassword` is not set or left empty, the development environment defaults to a fallback development password (`Admin@123456`).

---

### 3. Database Migration & Initialization
The database schema and seed data are automatically applied on application startup using Entity Framework Core Migrations (`MigrateAsync()`).

To manually update the database via CLI:
```bash
dotnet ef database update --project Project/Project_Back/MediCare/MediCare.Data --startup-project Project/Project_Back/MediCare/MediCare.Web
```
