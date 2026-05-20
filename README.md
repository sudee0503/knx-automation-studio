# 🎛️ KNX Automation Studio

A professional desktop automation software developed to manage the testing processes of KNX-based smart home and automation devices, dynamically construct device test scenarios, and automate unique Hexadecimal serial number assignments. 

The project is developed entirely using **C# / WPF** technologies and the **MVVM architecture**, adhering to corporate software engineering standards.

---

## 🚀 Key Features

* **Dynamic Testing & Scenario Architecture (`DeviceAssets`):** A flexible infrastructure capable of defining dynamic test steps (instruction texts or visual buttons) for any KNX device (sensors, relays, thermostats) rather than limiting the system to static devices with predefined buttons.
* **Smart Serial Number Assignment:** Automatically generates unique hexadecimal serial numbers upon successful device control operations and seamlessly associates them with the EITT integration.
* **Advanced Filtering & Live Log Stream:** Captures and details all user and device operations on the dashboard (logins, logouts, successful/failed assignments, device controls) in real-time.
* **Excel Integration (Export & Archiving):** * *Reporting:* Export dynamically filtered log records to an Excel file with a single click.
  * *Database Maintenance:* A built-in maintenance module that archives logs older than 1 year into an Excel format and permanently cleans them from the database to optimize system performance.
* **User-Friendly & Modern UI:** Designed with the `Material Design` toolkit, prioritizing user experience (UX) with a modern dark-mode aesthetic and a customized burgundy/dark color palette.

---

## 🛠️ Technologies & Stack

* **Framework:** .NET 9.0 / WPF (Windows Presentation Foundation)
* **Architecture Pattern:** MVVM (Model-View-ViewModel) & CommunityToolkit.Mvvm
* **Database & ORM:** MS SQL Server & Dapper (High-Performance Micro-ORM)
* **UI Library:** Material Design in XAML Toolkit
* **File Management:** ClosedXML (For Excel Export/Archive operations)
* **External Integration:** EITT (Device Control) Protocol

---

## 📁 Project Folder Structure

```text
├── Models/          # Database entities and business logic classes (Log, Device, User, etc.)
├── ViewModels/      # Business layer connecting the UI with data and managing commands
├── Views/           # WPF XAML interface designs, dialogs, and user panels
├── Services/        # DatabaseService and external device connection management
└── Converters/      # XAML data conversion helper classes
