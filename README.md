# MediCare – Clinic Management & Appointment System

## Project Overview

MediCare is a web-based clinic management and appointment booking system designed to connect patients with doctors and make healthcare services easier to access.

The system allows patients to explore available doctors, search by medical specialty and location, and manage their appointments. It also aims to help doctors and clinic staff manage their work through a centralized platform.

## Main Features

- Browse approved doctors.
- Search doctors by name and city.
- Filter doctors by medical specialty.
- Filter doctors based on availability.
- Display doctor profiles, consultation fees, and ratings.
- User registration and authentication.
- Appointment booking and management.

*Note: Features are implemented progressively as part of the graduation project.*

## Technologies Used

- **Backend:** ASP.NET Core MVC, C#
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Frontend:** HTML, CSS, JavaScript, Razor Views
- **Architecture:** Layered Architecture (Web, Services, Data)

## Project Structure

```text
Graduation_Project_Depi/
│
├── Project/
│   └── Project_Back/
│       └── MediCare/
│           ├── MediCare.Web/
│           ├── MediCare.Services/
│           └── MediCare.Data/
│
├── frontend-prototype/
├── MediCare_ERD.pdf
├── .gitignore
└── README.md
```

## Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/Boody-2006/Graduation_Project_Depi.git
   ```

2. Open the solution file:

   `Project/Project_Back/MediCare/MediCare.slnx`

3. Configure the database connection string in `appsettings.json`.

4. Make sure SQL Server or LocalDB is installed and available.

5. Build and run `MediCare.Web` using Visual Studio.

## Team Project

This project is developed as part of the Digital Egypt Pioneers Initiative (DEPI) graduation project.