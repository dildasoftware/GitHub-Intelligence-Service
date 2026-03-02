@echo off
echo ========================================================
echo   GITHUB INTELLIGENCE SERVICE - BASLATILIYOR 🚀
echo ========================================================
echo.

:: 1. Backend (API) Başlatılıyor
echo [1/3] Backend (API) ayaga kaldiriliyor...
start "GitHub Intelligence - BACKEND API" cmd /k "dotnet run --project src/GitHubIntelligenceService.Api/GitHubIntelligenceService.Api.csproj --urls=http://localhost:5000"

:: Backend'in açılması için 5 saniye bekle
timeout /t 5 /nobreak >nul

:: 2. Frontend (React) Başlatılıyor
echo [2/3] Frontend (React Client) ayaga kaldiriliyor...
cd client
start "GitHub Intelligence - FRONTEND" cmd /k "npm run dev"

:: Frontend'in açılması için 3 saniye bekle
timeout /t 3 /nobreak >nul

:: 3. Tarayıcıyı Aç
echo [3/3] Tarayici aciliyor...
start http://localhost:5173

echo.
echo ========================================================
echo   BASARIYLA CALISTI! 🎉
echo   Frontend: http://localhost:5173
echo   Backend:  http://localhost:5000/swagger
echo.
echo   NOT: Projeyi kapatmak icin acilan siyah pencereleri kapatabilirsin.
echo ========================================================
pause
