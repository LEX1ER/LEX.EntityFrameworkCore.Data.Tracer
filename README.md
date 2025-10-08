# LX.EntityFrameworkCore.Data.Tracer

[![NuGet](https://img.shields.io/nuget/v/LEX1ER.EntityFrameworkCore.Data.Tracer.svg)](https://www.nuget.org/packages/LEX1ER.EntityFrameworkCore.Data.Tracer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

An extension for **Entity Framework Core** that allows you to **trace data changes** (Create, Update, Delete) automatically for debugging, auditing, or logging purposes.

---

## 🚀 Features

- Tracks entity actions in `DbContext` (Create / Update / Delete)  
- Records before-and-after state changes  
- Supports custom user tracking via `ICurrentUser`  
- Lightweight and dependency-free (except EF Core + Newtonsoft.Json)  
- Easy integration with existing EF Core projects  

---

## 🧩 Installation

Install via NuGet:

```bash
dotnet add package LEX1ER.EntityFrameworkCore.Data.Tracer
