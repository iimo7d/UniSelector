# 🎓 UniSelector

> **A full-featured university admissions management platform** built with ASP.NET Core MVC, designed to match Jordanian students with private universities and BTEC programs — then manage the complete admissions lifecycle from application to enrollment.

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-SQL%20Server-orange?logo=microsoftsqlserver)](https://learn.microsoft.com/en-us/ef/core/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-green)](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
[![License](https://img.shields.io/badge/license-MIT-lightgrey)](LICENSE.txt)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [User Roles](#-user-roles)
- [System Architecture](#-system-architecture)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Database Setup](#database-setup)
  - [Configuration](#configuration)
  - [Running the App](#running-the-app)
- [Core Workflows](#-core-workflows)
- [Data Model](#-data-model)
- [Project Structure](#-project-structure)
- [Known Limitations](#-known-limitations)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🌟 Overview

**UniSelector** is a web platform purpose-built for the Jordanian private higher-education sector. It bridges the gap between students seeking university placement and universities managing admissions at scale.

The platform covers the **entire admissions journey**:

```
Student Registration → Profile Completion → Smart Recommendations
        ↓
Application Submission → University Review → Approval / Rejection
        ↓
Discount Grant → Commission Calculation → Monthly Settlement
        ↓
BTEC Program Lifecycle → Authority Approval → Compliance Reports
```

Additionally, it provides a **BTEC Authority** portal for reviewing vocational programs, publishing national standards, and exporting compliance reports.

---

## ✨ Key Features

### For Students
- 📝 **Smart Registration** — multi-step profile completion with path-specific logic (Academic / Vocational / BTEC)
- 🤖 **AI-Powered Recommendations** — programs scored by GPA, proximity, budget, declared major, language preference, and BTEC eligibility
- 📬 **Application Tracking** — full status history (Pending → Under Review → Approved / Rejected → Enrolled)
- 🎟️ **Discount Certificates** — downloadable PDF discount certificates issued upon approval
- 🔔 **Real-time Notifications** — instant alerts via SignalR on every status change

### For University Representatives
- 🏛️ **University & Program Management** — create/edit university profiles, programs, entry requirements
- 📋 **BTEC Program Management** — submit BTEC programs to the national authority for approval
- ✅ **Application Review** — approve/reject applications with automatic discount and commission creation
- 💰 **Commission Dashboard** — view earned commissions and monthly settlement summaries
- 🏷️ **Discount Redemption** — redeem student discount vouchers (with 90-day expiry enforcement)

### For Platform Administrators
- 👤 **User Management** — create, edit, reset passwords, and assign roles for all system users
- 🏫 **University Administration** — manage university records, assign/remove representatives
- 📊 **Application Oversight** — override any application status and force workflow transitions
- 💳 **Commission Administration** — trigger commission calculations, group into monthly settlements, close and export settlements
- 📢 **Broadcast Notifications** — send targeted notifications to individual users, roles, or entire universities

### For the BTEC Authority
- 🔍 **Program Review** — review university-submitted BTEC programs, approve or reject with feedback
- 📜 **Standards Management** — publish national BTEC standards notices to university representatives
- 📈 **Compliance Reporting** — export CSV reports: Program Compliance, Student Statistics, University Performance

---

## 👥 User Roles

| Role | Constant | Description |
|------|----------|-------------|
| **Student** | `Student` | Prospective students searching for and applying to universities |
| **University Representative** | `UniversityRep` | Staff from private universities managing their portal |
| **Platform Administrator** | `PlatformAdmin` | System-wide administrators with full access |
| **BTEC Authority** | `BtecAuthority` | National authority overseeing BTEC vocational programs |

> Role assignment is managed by Platform Admins through the User Management portal.

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        ASP.NET Core MVC                         │
│                                                                 │
│  ┌───────────┐  ┌───────────────┐  ┌────────────────────────┐  │
│  │  Student  │  │  University   │  │   Platform Admin /     │  │
│  │  Portal   │  │  Rep Portal   │  │   BTEC Authority       │  │
│  └───────────┘  └───────────────┘  └────────────────────────┘  │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Controller Layer                       │  │
│  │  AccountController │ ApplicationsController │ ...30+     │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌──────────────┐  ┌─────────────┐  ┌────────────────────────┐ │
│  │   Services   │  │  SignalR    │  │  Background Services   │ │
│  │  Email       │  │  Notification│  │  SmartRecommendation  │ │
│  │  Notif.      │  │  Hub        │  │  CpuMonitoring        │ │
│  │  FileUpload  │  └─────────────┘  └────────────────────────┘ │
│  │  Recommend.  │                                               │
│  └──────────────┘                                               │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │         EF Core + SQL Server (AppDbContext)               │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Middleware Pipeline
- **ResponseTimeMiddleware** — tracks per-request performance metrics
- **ASP.NET Core Identity** — authentication, role-based authorization, account lockout, email confirmation
- **Session** — 30-minute idle timeout

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 9.0 MVC |
| ORM | Entity Framework Core |
| Database | SQL Server / LocalDB |
| Auth | ASP.NET Core Identity |
| Real-time | ASP.NET Core SignalR |
| Email | SendGrid |
| File Storage | Local filesystem (`wwwroot/uploads/`) |
| UI Library | Bootstrap 5, SweetAlert2 |
| Localization | `en-US` and `ar-JO` |
| Charting | Chart.js |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server Express / LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Git](https://git-scm.com/)
- A SendGrid account (optional — required for email confirmation and password reset)

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/iimo7d/UniSelector.git
cd UniSelector

# 2. Restore NuGet packages
dotnet restore
```

### Database Setup

```bash
# Navigate to the web project
cd Uni_Selector

# Apply all EF Core migrations (creates the database and all tables)
dotnet ef database update
```

> The app uses database seeding on first run to create the default admin account and seed reference data. Check `Data/SeedData.cs` (or `Program.cs`) for seeded credentials.

### Configuration

Open `Uni_Selector/appsettings.json` and update the following sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(LocalDb)\\SQLEXPRESS;Database=Uni_Selector;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "SendGrid": {
    "ApiKey": "YOUR_SENDGRID_API_KEY",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "UniSelector"
  },
  "Email": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "Username": "apikey",
    "Password": "YOUR_SENDGRID_API_KEY"
  }
}
```

> **Note:** For local development, email delivery can be disabled or mocked — the app will still function without a valid SendGrid key, though password reset emails will not be sent.

### Running the App

```bash
# From the Uni_Selector directory
dotnet run

# Or with watch mode (hot reload)
dotnet watch run
```

The app will be available at:
- HTTP: `http://localhost:5128`
- HTTPS: `https://localhost:7251`

---

## 🔄 Core Workflows

### 1. Student Onboarding
```
Register account → Confirm email → Login → Complete profile
  ↳ Academic path:   Enter GPA, school track, grades
  ↳ Vocational path: Enter vocational branch + GPA
  ↳ BTEC path:       Upload BTEC certificate, enter level/score
```

### 2. Recommendation Engine
The platform scores every eligible university program against the student's profile using multiple weighted signals:

| Signal | Description |
|--------|-------------|
| GPA Match | Minimum GPA requirement vs. student GPA |
| Budget Fit | Estimated tuition vs. student budget |
| Geographic Proximity | University city vs. student city |
| Major Match | Desired major vs. program major |
| Language Preference | Program instruction language |
| BTEC Eligibility | Student BTEC level vs. program requirements |

### 3. Application Lifecycle

```
Student submits application (one active application at a time)
        ↓
Status: PENDING
        ↓
University Rep reviews → sets to UNDER REVIEW
        ↓
         ├── APPROVED ─→ DiscountGrant (5%) created
         │               Commission (2%) created
         │               Student notified
         │
         └── REJECTED → Rejection reason stored, student notified
                ↓
           (Admin can OVERRIDE any status at any time)
                ↓
           ENROLLED (final state)
```

### 4. Discount & Commission Flow

```
Approval Event
  ↓
DiscountGrant created (Status: Issued, 90-day expiry)
  ↓
Student downloads PDF certificate
  ↓
University Rep redeems discount (Status: Redeemed)
             — or —
90 days pass without redemption (Status: Expired)

Commission created alongside DiscountGrant
  ↓
Platform Admin runs Calculate (POST) → Commission amounts set
  ↓
Commissions grouped into MonthlySettlement
  ↓
Settlement closed → all commissions marked Settled
  ↓
Settlement exported as CSV
```

### 5. BTEC Program Lifecycle

```
University Rep creates BTEC program
        ↓
Status: Pending review by BTEC Authority
        ↓
         ├── APPROVED → Program becomes eligible for student recommendations
         │               Rep notified
         │
         └── REJECTED → Rejection reason stored, rep notified
                         ↓
                    Rep can edit and resubmit
                    (editing resets approval status)
```

---

## 🗃️ Data Model

### Core Entities

| Entity | Description |
|--------|-------------|
| `ApplicationUser` | Extended Identity user (base for all roles) |
| `Student` | Student profile: path, GPA, city, budget, BTEC data |
| `University` | University record: name, city, type, logo |
| `UniversityRepresentative` | Links `ApplicationUser` to a `University` |
| `ProgramEntity` | Master program catalog (major, level) |
| `UniversityProgram` | University-specific offering of a program (fees, language, etc.) |
| `EntryRequirement` | GPA/score requirements per `UniversityProgram` |
| `BtecProgram` | BTEC-specific program offered by a university |
| `BtecEntryRequirement` | Level/score requirements per `BtecProgram` |
| `StudentApplication` | One application per student (status-tracked) |
| `Recommendation` | Scored recommendation linking student ↔ program |
| `DiscountGrant` | 5% discount issued on approval (90-day expiry) |
| `Commission` | 2% commission earned by rep on approval |
| `MonthlySettlement` | Groups commissions for a billing period |
| `Notification` | Per-user notification with read/unread state |

### Key Enumerations

```csharp
enum ApplicationStatus  { Pending, UnderReview, Approved, Rejected, Enrolled, Cancelled }
enum DiscountStatus     { Issued, Redeemed, Expired }
enum PathType           { Academic, Vocational, BTEC }
enum BtecLevel          { Level2, Level3, Level4, Level5, Level6, Level7, Level8 }
```

---

## 📁 Project Structure

```
UniSelector/
├── Uni_Selector/                        # Main web project
│   ├── Controllers/                     # 30+ MVC controllers
│   │   ├── AccountController.cs         # Registration, login, password reset
│   │   ├── ApplicationsController.cs    # Student application flow
│   │   ├── ApplicationAdminController.cs# Admin application overrides
│   │   ├── ApplicationReviewController.cs# Rep review, approve, reject
│   │   ├── BTECProgramManagementController.cs
│   │   ├── BTECProgramReviewController.cs
│   │   ├── BTECReportsController.cs
│   │   ├── BTECStandardsController.cs
│   │   ├── CommissionAdminController.cs
│   │   ├── DiscountManagementController.cs
│   │   ├── NotificationAdminController.cs
│   │   ├── NotificationsController.cs   # API: mark read, delete
│   │   ├── ProgramManagementController.cs
│   │   ├── RecommendationsController.cs
│   │   ├── StudentController.cs
│   │   ├── UniversityAdminController.cs
│   │   ├── UserManagementController.cs
│   │   └── ...
│   ├── Models/                          # EF entities, ViewModels, enums
│   │   ├── Entities/                    # Database entity classes
│   │   ├── ViewModels/                  # View-specific DTOs
│   │   └── Enums/                       # ApplicationStatus, UserRoles, etc.
│   ├── Views/                           # Razor views
│   │   ├── Account/                     # Login, Register, ResetPassword
│   │   ├── Student/                     # Profile, dashboard
│   │   ├── Applications/               # Apply, track, details
│   │   ├── Recommendations/            # Index, details
│   │   ├── ApplicationReview/          # Rep review queue
│   │   ├── BTECAuthority/              # Authority dashboard
│   │   ├── BTECReports/               # Export, report views
│   │   ├── CommissionAdmin/            # Commission management
│   │   ├── UserManagement/             # Admin user CRUD
│   │   ├── UniversityAdmin/            # Admin university CRUD
│   │   └── Shared/                     # _Layout, _AdminLayout, _StudentLayout
│   ├── Data/
│   │   └── AppDbContext.cs             # EF Core DbContext
│   ├── Migrations/                     # EF Core migration history
│   ├── Services/
│   │   ├── EmailService.cs
│   │   ├── NotificationService.cs
│   │   ├── RecommendationService.cs
│   │   ├── FileUploadService.cs
│   │   └── BtecStandardsNotifier.cs
│   ├── Hubs/
│   │   └── NotificationHub.cs          # SignalR real-time hub
│   ├── Middleware/
│   │   └── ResponseTimeMiddleware.cs
│   ├── wwwroot/                         # Static assets + file uploads
│   │   └── uploads/                    # University logos, BTEC certificates
│   ├── appsettings.json
│   └── Program.cs
├── ConsoleApp1/                         # Utility / legacy console tool
├── Hash/                                # Password hashing utility
└── README.md
```

---

## ⚠️ Known Limitations

| Area | Limitation |
|------|-----------|
| **BTEC Standards** | Standards update is notice-only — no persistence to the database |
| **Recommendation Queue** | In-memory; recommendations queued in `SmartRecommendationService` are lost on app restart |
| **CPU / Response Stats** | Monitoring metrics are in-memory only, not persisted |
| **Email** | `ResetPassword` is a stub — token is generated but email delivery must be implemented via SendGrid API |
| **University Types** | Only `Private` universities are currently supported |
| **Test Coverage** | No automated test project exists |
| **File Validation** | Uploads are validated for type (jpg, png, pdf, gif) and size (≤ 5 MB) |
| **BTEC Standards** | No dedicated `BtecStandards` database table; standards are advisory-only |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feat/your-feature`
3. Commit your changes following conventional commits:
   ```
   feat: add new feature
   fix: resolve specific bug
   refactor: improve code structure
   ```
4. Push to your branch: `git push origin feat/your-feature`
5. Open a Pull Request against `master`

### Development Guidelines
- Follow the existing MVC pattern — controllers, services, view models
- All role-string references must use constants from `UserRoles` (never hard-coded strings)
- Validate all user-supplied IDs for existence before use
- Pagination parameters must be clamped: `page = Math.Max(1, page)`, `pageSize = Math.Clamp(pageSize, 1, 100)`
- State-mutating actions must use `[HttpPost]` with `[ValidateAntiForgeryToken]`

---

## 📄 License

This project is licensed under the [MIT License](LICENSE.txt).

---

<div align="center">

**UniSelector** — Empowering students to find their perfect university match.

[🌐 Live Repository](https://github.com/iimo7d/UniSelector) • [📝 Issues](https://github.com/iimo7d/UniSelector/issues) • [🔀 Pull Requests](https://github.com/iimo7d/UniSelector/pulls)

</div>
