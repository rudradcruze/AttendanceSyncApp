# Smart Tools App

A web-based application for managing attendance, employee data, and multi-tool access control with role-based authentication.

---

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Initial Setup](#initial-setup)
  - [Database Configuration](#database-configuration)
  - [Google OAuth Configuration](#google-oauth-configuration)
- [Default Login Credentials](#default-login-credentials)
- [Adding Admin Users](#adding-admin-users)
- [User Registration and Login](#user-registration-and-login)
- [Attendance Tool Setup](#attendance-tool-setup)
  - [Admin Setup Process](#admin-setup-process)
  - [User Request Process](#user-request-process)
- [Server IP Configuration](#server-ip-configuration)
- [Tool Management](#tool-management)
- [Security Notes](#security-notes)

---

## Overview

The Attendance Sync App provides a centralized platform for:
- Managing employee attendance across multiple companies
- Role-based access control (Admin and User roles)
- Multi-database support with dynamic configuration
- Google OAuth integration
- Tool assignment and access management

---

## Prerequisites

- SQL Server database
- IIS or compatible web server
- .NET Framework (ASP.NET MVC)
- PowerShell (for generating password hashes)
- Google OAuth credentials (optional, for Google login)

---

## Initial Setup

### Database Configuration

1. Open `web.config` in the project root directory

2. Locate the `<connectionStrings>` section:

```xml
<connectionStrings>
    <add name="AttandanceSyncConnection"
         connectionString="Data Source=YOUR_SERVER;Initial Catalog=Smart_Tools;User Id=YOUR_USER;Password=YOUR_PASSWORD"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

3. Update the connection string with your database credentials:
   - **Data Source**: Your SQL Server IP or instance name (e.g., `192.168.17.30`)
   - **Initial Catalog**: Database name (default: `Smart_Tools`)
   - **User Id**: SQL Server username
   - **Password**: SQL Server password

### Google OAuth Configuration

1. Create OAuth credentials in [Google Developer Console](https://console.developers.google.com/)

2. Open `web.config` and locate the Google OAuth settings:

```xml
<!-- Google OAuth Settings -->
<add key="GoogleClientId" value="" />
<add key="GoogleClientSecret" value="" />
<add key="GoogleRedirectUri" value="https://localhost:44340/Auth/GoogleCallback" />
```

3. Configure the values:
   - **GoogleClientId**: Your Google OAuth Client ID
   - **GoogleClientSecret**: Your Google OAuth Client Secret
   - **GoogleRedirectUri**: Update with your domain
     - For local development: `https://localhost:44340/Auth/GoogleCallback`
     - For production: `https://yourdomain.com/Auth/GoogleCallback`

4. **Important**: Add the same redirect URI in Google Developer Console under "Authorized redirect URIs"

---

## Default Login Credentials

### Admin Account
- **Email**: `admin@admin.com`
- **Password**: `admin@123`

### Test User Account
- **Email**: `test@test.com`
- **Password**: `test@123`

**Note**: Change these credentials immediately after first login in production environments.

---

## Adding Admin Users

Admin users can only be added through direct database operations.

### Step 1: Generate Password Hash

1. Open **PowerShell** (Press `Win + X` and select "Windows PowerShell")

2. Run this command (replace `admin@123` with your desired password):

```powershell
[Convert]::ToBase64String([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes("admin@123")))
```

3. Copy the generated hash (e.g., `JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=`)

### Step 2: Insert Admin User via SQL

Run this SQL query in your database:

```sql
INSERT INTO Users (Name, Email, Password, Role, IsActive, CreatedAt)
VALUES ('Administrator', 'admin@example.com', 'YOUR_PASSWORD_HASH', 'ADMIN', 1, GETDATE());
```

Replace:
- `admin@example.com` with the desired admin email
- `YOUR_PASSWORD_HASH` with the hash generated in Step 1

---

## User Registration and Login

### Registration (`/Auth/Register`)

Users can self-register with the USER role:

1. Navigate to `/Auth/Register`
2. Fill in the registration form:
   - **Full Name**
   - **Email**
   - **Password** (minimum 8 characters)
   - **Confirm Password**
3. Submit to create a USER account

### Login (`/Auth/Login`)

1. Navigate to `/Auth/Login`
2. Enter credentials:
   - **Email**
   - **Password**
3. The system automatically routes to the appropriate dashboard based on role (Admin or User)

---

## Attendance Tool Setup

Setting up the Attendance Tool requires coordination between Admin and User.

### Admin Setup Process

#### 1. Add Company

1. Login as **Admin**
2. Navigate to **AdminCompanies/Index** (Menu: Companies > Manage Companies)
3. Click **Add Company**
4. Fill in company details and save

#### 2. Add Employees

1. Navigate to **AdminEmployees/Index** (Menu: Employees > Manage Employees)
2. Click **Add Employee**
3. Fill in employee details and save
4. Repeat for all employees

#### 3. Configure Database

1. Navigate to **AdminDatabaseConfigurations/Index** (Menu: Databases > Database Config)
2. Click **Add Database Configuration**
3. Configure database settings for the company:
   - Select the company
   - Enter database connection details
4. Save the configuration

### User Request Process

#### 4. User Requests Company Access

1. Login as **User**
2. Navigate to **CompanyRequest/Index** (Dashboard: Click "Request Company")
3. Fill in the request form:
   - **Select Employee**: Choose your employee record
   - **Select Company**: Choose the company
   - **Select Tool**: Choose "Attendance Tool"
4. Submit the request

#### 5. Admin Approves and Assigns Database

1. Login as **Admin**
2. Navigate to **AdminCompanyRequests/Index** (Menu: Requests > Company Requests)
3. Review pending requests
4. Click **Accept** on the desired request
5. Click **Assign DB** button (appears after acceptance)
6. Confirm the database assignment by clicking **Yes**

#### 6. User Access

Once approved and assigned:
- The user will see the Attendance Tool in their dashboard
- The tool is now ready to use

---

## Server IP Configuration

For additional tools (e.g., Salary Issue tool), server IP configuration is required.

### Add Server IP

1. Login as **Admin**
2. Navigate to **AdminServerIp/Index** (Menu: Salary Issue > Server IPs)
3. Click **Add Server IP**
4. Fill in the form:
   - **IP Address**: Server IP address
   - **Database User**: SQL Server username
   - **Database Password**: SQL Server password
5. Click **Add**

The system will:
- Automatically scan the server
- Discover all databases
- Grant access to all databases by default

### Managing Database Access

1. Navigate to **AdminDatabaseAccess/Index** (Menu: Database Access)
2. Select the IP address
3. View all discovered databases
4. Toggle access permissions as needed (enable/disable access)

---

## Tool Management

Admins can create custom tools and assign them to users.

### Create a Tool

1. Login as **Admin**
2. Navigate to **AdminTools/Index** (Menu: Tools > Manage Tools)
3. Click **Add Tool**
4. Fill in tool details:
   - Tool name
   - Description
   - Other settings
5. Save the tool

### Assign Tool to User

1. Navigate to **AdminUserTools/Index** (Menu: Tools > Assign Tools)
2. Click **Assign Tool to User**
3. Fill in the assignment form:
   - **Select User**: Choose the user
   - **Select Tool**: Choose the tool to assign
4. Submit the assignment

The user will now have access to the assigned tool in their dashboard.

---

## Security Notes

### Password Requirements
- Minimum 8 characters
- Passwords are hashed using SHA-256

### Best Practices
1. **Change default credentials** immediately in production
2. **Use strong passwords** for admin accounts
3. **Restrict database access** based on least privilege principle
4. **Enable HTTPS** in production environments
5. **Regularly review** user permissions and tool assignments
6. **Secure web.config** file with appropriate file permissions
7. **Rotate Google OAuth credentials** periodically
8. **Monitor admin activities** and database access logs

### Production Checklist
- [ ] Change all default passwords
- [ ] Update Google OAuth redirect URIs
- [ ] Configure production database connection
- [ ] Enable HTTPS/SSL
- [ ] Set appropriate file permissions on web.config
- [ ] Remove or disable test accounts
- [ ] Configure backup strategy
- [ ] Set up monitoring and logging

---

## Troubleshooting

### Common Issues

**Issue**: Cannot login with default credentials
- **Solution**: Ensure database connection is properly configured in web.config

**Issue**: Google OAuth not working
- **Solution**: Verify redirect URI matches in both web.config and Google Developer Console

**Issue**: User cannot see assigned tool
- **Solution**: Check that the tool is properly assigned in AdminUserTools/Index and the user has accepted company request

**Issue**: Database assignment fails
- **Solution**: Verify database configuration exists for the company in AdminDatabaseConfigurations/Index

---

---

**Last Updated**: 29 January 2026 By FRANCIS RUDRA D CRUZE
