# E-Commerce Order Management System (Full Stack Assessment)

This repository contains a **production-ready full-stack e-commerce order management assignment**, showcasing a modern, scalable architecture with separate backend and frontend layers, real-time notifications, robust state management, and clean code practices.

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Frontend**: Angular 19 + Angular Material
- **Authentication**: JWT + ASP.NET Identity
- **Architecture**: Clean Architecture + CQRS (MediatR)

---

## 📂 Repository Structure

```text
E-Commerce/
│
├── Back__end/
│   ├── ECommerce.Api/             # API Controllers, Middleware, SignalR Hubs
│   ├── ECommerce.Application/     # CQRS (Commands/Queries), DTOs, Interfaces
│   ├── ECommerce.Domain/          # Entities, Value Objects, Enums
│   └── ECommerce.Infrastructure/  # EF Core DbContext, Repositories, Identity
│
├── Front__end/
│   └── src/
│        ├── app/
│        │   ├── core/             # Interceptors, Guards, Services, Models
│        │   ├── features/         # Standalone feature modules (Products, Orders, Cart, etc.)
│        │   ├── shared/           # Reusable UI components (Navbar, Dialogs)
│        │   ├── app.routes.ts
│        │   └── app.component.ts
│        ├── environments/         # Environment configurations
│        └── main.ts
│
└── README.md
```

---

## 🛠️ Backend – ASP.NET Core Web API

### Tech Stack

- ASP.NET Core (.NET 8)
- Entity Framework Core (SQL Server)
- ASP.NET Identity & JWT Authentication
- MediatR (CQRS Pattern)
- AutoMapper & FluentValidation
- SignalR (Real-time WebSockets)
- Serilog (Structured Logging)

### Backend Architecture Highlights

- **Clean Architecture**: Strong separation of concerns. The Domain has no dependencies, Application depends only on Domain, and Infrastructure handles external concerns.
- **CQRS with MediatR**: Commands (state changes) and Queries (data retrieval) are strictly separated, making the system highly scalable and maintainable.
- **Product-Lock & Stock Validation**: Concurrency handling and strict stock validation during checkout. You cannot order a product that does not exist in stock, even if it was added to the cart seconds ago.
- **Historical Pricing & Discounts**: Order items snapshot the exact price and discount at the time of purchase. Changing a product's price or discount later will not affect historical orders.
- **Real-time Notifications**: SignalR pushes live updates to connected clients for order status changes and stock deductions.

### Key Endpoints

Refer to the **API Endpoints** section below for a complete list of available API routes.

---

## 💻 Frontend – Angular 19

### Tech Stack

- Angular 19
- Angular Material & SCSS (Dark/Light themes)
- RxJS & Angular Signals (Modern State Management)
- JWT Interceptor & Route Guards
- SignalR Client

### UI Features & Best Practices

- **Angular Signals**: Utilized modern Angular Signals (e.g., in `CartService`) for reactive, glitch-free state management and computed properties.
- **Environment Variables**: All configuration values, API URLs, and keys are strictly managed via environment files (`environment.ts`).
- **Real-time Order Tracking**: The order summary page features a live progress bar that updates instantly via WebSockets when the backend advances the order status.
- **Live Notifications**: The notification center updates in real-time. Unread badges and toast messages appear instantly without page reloads.
- **Responsive Design**: Fully responsive layout using modern CSS Grid/Flexbox and SCSS variables.
- **Clean Code**: Unnecessary files and boilerplate have been meticulously removed to ensure maximum readability for code reviewers.

---

## 🚀 Order Management Flow

1. **Browsing**: Users browse products. Admins can manage products, categories, and set **per-product discounts**.
2. **Cart & Validation**: Users add items to the cart. The cart dynamically calculates subtotal and per-product discounts.
3. **Checkout**: The system performs a strict pre-check (`validate-cart`) to ensure no overselling occurs.
4. **Order Placement**: A single transactional unit of work deducts stock, records historical prices/discounts, and creates the order.
5. **Real-time Tracking**: The user is redirected to the order summary, where a progress bar tracks the order lifecycle (`Pending` → `Processing` → `Shipped` → `Completed`).
6. **Order Cancellation**: If an order is cancelled, the system automatically restores the exact stock quantities back to the inventory and broadcasts the updated stock to all users.
7. **Live Updates**: SignalR broadcasts stock changes to all users and order status updates to the specific customer.
8. **Server-Side Pagination & Filtering**: The admin dashboard and order lists utilize highly optimized server-side pagination, sorting, and multi-parameter filtering (date ranges, status, customer search) via Entity Framework.

## 🚀 API Endpoints

### Auth
- `POST /api/Auth/login`: Authenticate user and return JWT token.
- `POST /api/Auth/register`: Register a new user account.
- `POST /api/Auth/logout`: Revoke the current token.

### Products
- `GET /api/Products`: Retrieve a paginated, filterable, and sortable list of products.
- `GET /api/Products/{id}`: Retrieve details of a specific product.
- `POST /api/Products`: Create a new product (Admin only).
- `PUT /api/Products/{id}`: Update an existing product (Admin only).
- `DELETE /api/Products/{id}`: Soft-delete a product (Admin only).

### Categories
- `GET /api/Categories`: Retrieve all categories.
- `POST /api/Categories`: Create a new category (Admin only).
- `PUT /api/Categories/{id}`: Update an existing category (Admin only).
- `DELETE /api/Categories/{id}`: Soft-delete a category (Admin only).

### Orders
- `GET /api/Orders`: Retrieve a paginated list of orders for the currently authenticated user.
- `GET /api/Orders/all`: Retrieve a paginated list of all orders across the system (Admin only).
- `GET /api/Orders/{id}`: Retrieve details of a specific order.
- `POST /api/Orders`: Create a new order (Checkout).
- `POST /api/Orders/{id}/cancel`: Cancel an existing order and restore stock (Admin only).
- `POST /api/Orders/validate-cart`: Validate cart items against current stock and prices before checkout.

### Notifications
- `GET /api/Notifications`: Retrieve all notifications for the authenticated user.
- `GET /api/Notifications/unread-count`: Get the number of unread notifications.
- `POST /api/Notifications/{id}/read`: Mark a specific notification as read.

### Payments
- `POST /api/Payments/simulate`: Simulate a payment gateway transaction.

---

## ⚙️ Setup & Execution

### Backend Setup

```bash
cd Back__end/ECommerce.Api
dotnet restore
dotnet ef database update --project ../ECommerce.Infrastructure
dotnet run
```
*The API will run on `http://localhost:5017`. EF Core migrations will automatically apply on startup.*

### Frontend Setup

```bash
cd Front__end
npm install
npm start
```
*The application will be available at `http://localhost:4200`.*

---

## 🏆 Best Practices Applied

- Clean Architecture & CQRS
- Modern Angular Signals for State Management
- Strict Environment Variable Configuration
- Real-time WebSockets Integration
- Historical Data Integrity (Price/Discount Snapshots)
- Centralized Error Handling & Validation
- Secure Authentication Flow

---

## 👨‍💻 Author

**Mohamed Sayed**  
Senior Full Stack Developer Assessment (.NET + Angular)  
*Built with scalability, security, and clean architecture in mind.*