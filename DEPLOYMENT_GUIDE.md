# 🚀 Deployment Guide: GitHub Intelligence Service

Congratulations regarding reaching the deployment phase! This document outlines two primary methods to deploy your application to a live production environment.

---

## ✅ Method 1: The "Easy & Fast" Way (Railway / Render)
Best for: Quick prototypes, Docker-based deployments, and free/cheap tiers.

### Prerequisites
1.  Push your code to **GitHub**.
2.  Create an account on **[Railway.app](https://railway.app)** or **[Render.com](https://render.com)**.

### Steps (Example: Railway)
1.  **Dashboard:** Click "New Project" -> "Deploy from GitHub repo".
2.  **Select Repo:** Choose your `GitHubIntelligenceService` repository.
3.  **Docker Support:** Railway will detect the `Dockerfile` in `src/GitHubIntelligenceService.Api`.
    *   *Note:* Since we have a multi-container setup (Backend + Frontend), it's best to use `docker-compose`.
4.  **Environment Variables:** Add your secrets (if any, like API Keys) in the "Variables" tab.
5.  **Database:** Add a PostgreSQL or SQL Server plugin from the Railway dashboard and update the connection string.
6.  **Deploy:** Click Deploy! Your API will be live at `https://your-app.up.railway.app`.

---

## ☁️ Method 2: The "Professional" Way (Azure App Service)
Best for: .NET Developers, Enterprise projects, Resume building.

### Prerequisites
1.  **Azure Account:** (Free tier available).
2.  **Azure CLI** (or use VS Code Azure Extension).

### Steps
1.  **Create Resource Group:**
    ```bash
    az group create --name GitHubIntelligenceGroup --location westeurope
    ```
2.  **Create App Service Plan (Free Tier):**
    ```bash
    az appservice plan create --name GitHubPlan --resource-group GitHubIntelligenceGroup --sku F1 --is-linux
    ```
3.  **Create Web App:**
    ```bash
    az webapp create --resource-group GitHubIntelligenceGroup --plan GitHubPlan --name MyGitHubIntelligence --runtime "DOTNET|9.0"
    ```
4.  **Deploy Code:**
    ```bash
    az webapp up --sku F1 --name MyGitHubIntelligence --os-type Linux
    ```
    *Or configure Deployment Center in Azure Portal to sync with GitHub.*

5.  **Database (Azure SQL):** Create an Azure SQL Database (Basic Tier ~$5/mo) and update the connection string in App Service Configuration.

---

## 🐳 Method 3: The "Hacker" Way (VPS + Docker)
Best for: Total control, learning Linux, cheapest option ($5/mo).

1.  **Rent a VPS:** (DigitalOcean, Hetzner, AWS EC2).
2.  **SSH into Server:** `ssh root@your-ip`.
3.  **Install Docker:**
    ```bash
    curl -fsSL https://get.docker.com | sh
    ```
4.  **Clone Repo:**
    ```bash
    git clone https://github.com/your-username/GitHubIntelligenceService.git
    cd GitHubIntelligenceService
    ```
5.  **Run:**
    ```bash
    docker compose up -d --build
    ```
6.  **Profit:** Your app is running on `http://your-ip:3000`.

---

### 💡 Recommendation
Start with **Method 1 (Railway/Render)** if you want to see it live in 5 minutes.
Use **Method 2 (Azure)** if you want to impress recruiters for a .NET Developer role.
