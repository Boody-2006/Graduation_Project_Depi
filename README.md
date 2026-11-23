# MediCare – Clinic Management & Appointment System

## Authentication & Role-Based Access Control (RBAC)

### 1. Default Roles
The system automatically provisions three main roles upon startup:
- `Admin`: Full management access (represents Clinic Assistant in ERD).
- `Doctor`: Doctor account (subject to administrative approval gate).
- `Patient`: Patient self-service registration and booking.

---

### 2. Default Administrator Account
An initial administrator account is automatically seeded if it does not already exist:
- **Email:** `admin@medicare.com`
- **Password:** Configured via `Project/Project_Back/MediCare/MediCare.Web/appsettings.json` under `Seed:AdminPassword`. Default: `Admin@123456`.

#### How to change the Default Admin Password:
In `appsettings.json` (or `appsettings.Development.json`):
```json
{
  "Seed": {
    "AdminPassword": "YourStrongPasswordHere@2026"
  }
}
```
*Note: If the administrator account is already created in the database, changing the configuration will not override the existing hashed password. To reset or use a new password, either update the user in the database or initialize on a fresh database.*

---

### 3. Database Migration & Initialization
The database schema and seed data are automatically applied on application startup using Entity Framework Core Migrations (`MigrateAsync()`).

To manually update the database via CLI:
```bash
dotnet ef database update --project Project/Project_Back/MediCare/MediCare.Data --startup-project Project/Project_Back/MediCare/MediCare.Web
```
