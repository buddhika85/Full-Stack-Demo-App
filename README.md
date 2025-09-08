# 🧰 Full Stack Demo App

[![Build Status](https://dev.azure.com/gsoft85512/FullStackInterviewDemo/_apis/build/status/buddhika85.Full-Stack-Demo-App?branchName=main)](https://dev.azure.com/gsoft85512/FullStackInterviewDemo/_build/latest?definitionId=1&branchName=main)

**Duration**: July 2026 – Present  
**Tech Stack**: Angular 20 · ASP.NET Core Web API (.NET 9) · JWT Authentication · Serilog · Entity Framework Core · Swagger/OpenAPI · xUnit · Moq · Azure SQL · Azure App Services · Azure Static Web Apps · Azure DevOps CI/CD

---

## 📦 Project Overview

This demo showcases full-stack development skills and architectural patterns gained from industry experience. It features a modern Angular frontend integrated with a robust .NET backend, designed to highlight reusable business logic, clean API design, and scalable data access.

---

## 🎯 Purpose

This project demonstrates:

- End-to-end solution delivery using modern Microsoft technologies  
- Clean separation of concerns across frontend, backend, and data layers  
- Real-world patterns for scalable, maintainable enterprise applications  
- Interview-ready code showcasing technical depth and presentation polish

---

## 🧱 Architecture Overview

### 🔧 Backend

- ASP.NET Core Web API (.NET 9)
- Service layer + Repository + Unit of Work pattern
- Azure SQL Server with Entity Framework Core
- Serilog logging and custom middleware
- JWT token validation, blacklisting, and authorization
- Output caching and cache eviction
- 100+ unit tests using `xUnit`, `Moq`, and `FluentAssertions`

![Backend Architecture](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/public/img/backend-architecture.png?raw=true)

#### 📚 Backend Tech Stack Summary

| Component           | Library             | Version         |
|---------------------|---------------------|-----------------|
| .NET Framework      | .NET                | net9.0          |
| Logging             | Serilog             | 9.0.0           |
| API UI Testing      | Swagger             | 9.0.1           |
| OpenAPI Docs        | OpenApi             | 9.0.7           |
| DB Access           | EF Core SQL Server  | 9.0.7           |
| DB Migrations       | EF Core Design      | 9.0.7           |
| Unit Testing        | xUnit               | 2.9.2           |
| Mocking             | Moq                 | 4.2             |
| Assertions          | FluentAssertions    | 8.5             |
| Password Hashing    | BCrypt.Net-Next     | 4.0.3           |

#### 🔍 Key Backend Features

- [CustomExceptionMiddleware](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Backend/Emp.Api/Middleware/CustomExceptionMiddleware.cs)
- [ConsoleLoggerFilter](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Backend/Emp.Api/Filters/ConsoleLoggerFilter.cs)
- [FirstLetterUpperCaseAttribute](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Backend/Emp.Core/ValidationAttributes/FirstLetterUpperCaseAttribute.cs)
- 100+ unit tests using `xUnit`, `Moq`, and `FluentAssertions`
- ...

![Unit Testing Preview](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/public/img/unit-testing.png?raw=true)

---

### 🎨 Frontend

- Angular 20 with Tailwind CSS
- Modular components, route-based navigation
- Deployed via Azure Static Web Apps
- Reactive forms, lifecycle hooks, snack bar service, loading spinner
- ...

![Frontend Architecture](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/public/img/frontend-architecture.png?raw=true)

#### 🔍 Key Frontend Features

- [JWT Interceptor](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/src/app/interceptors/jwt-interceptor.ts)
- [User Role Pipe](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/src/app/pipes/user-role-enum-to-user-role-pipe.ts)
- [Auth Guard](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/src/app/guards/auth-guard.ts)
- ...

---

### 🔄 CI/CD Workflow Summary

- CI/CD pipeline using Azure DevOps  
- Automated build, test, and deployment  
- API deployed to Azure App Service via ZIP Deploy  
- Angular app deployed to Azure Static Web App  


![Dev Ops](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/public/img/Azure-devOps.png?raw=true)

---

### ⚙️ Azure Hosting

- Azure SQL Database, Azure App Service, Azure Static Web App was utilized for hosting.
- Secrets managed securely via Environment variables and Azure App Settings
- Serilog logs accessible via Kudu console
![SeriLog ](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/public/img/Azure-Kudu-Logs.png?raw=true)


###  Other Azure Services used

- Azure Static Web App
- Azure API App
- Azure Function
- Azure Service Bus
- Azure Container Instance

And Docker Hub (Hosted [Image](https://hub.docker.com/r/gsoft85512/docker-number-minimal-api))



---

## 🖼️ Screenshots / Preview

![Application ](https://github.com/buddhika85/Full-Stack-Demo-App/blob/main/Emp.Angular/public/img/deplopyed-app.png?raw=true)

---



---

## 🚀 Getting Started

To run locally:

1. Clone the repo  
2. Restore NuGet packages and npm dependencies  
3. Update `appsettings.json` with your local DB connection string  
4. Run the backend API (`Emp.Api`)  
5. Serve the Angular app with `ng serve`  

> Requires .NET 9 SDK and Node.js 20+
## 🛠️ Known Issues / Roadmap

- EF CLI version mismatch warning (resolved via local tool manifest)  
- Swagger endpoint validation pending automated health check  
- Angular environment switching for preview vs production slots

---

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you’d like to change.

---

## 🌐 Live Demo

[Launch Frontend](https://delightful-desert-009c44900.2.azurestaticapps.net/)

---

## 📄 License

This project is open-source and available under the MIT License.
