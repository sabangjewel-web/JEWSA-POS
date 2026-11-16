# JEWSA-POS — Jewel Sabang, Section 3-B
Database: `sabang`  |  C# WinForms, .NET Framework 4.8, SQL Server

## Setup
1. Open `Database/01_CreateDatabase.sql` in SSMS and run the whole script (creates database `sabang`).
2. Open `JEWSA-POS.sln` in Visual Studio 2019/2022. Set `JewSaPOS.WinForms` as startup project.
3. Edit `JewSaPOS.WinForms/App.config` -> `Data Source=` = your SQL Server instance.
4. Press F5.

## Demo accounts
admin / Admin@123 · manager / Manager@123 · cashier / Cashier@123
(Cashier can view inventory but cannot add/edit/delete/adjust.)
