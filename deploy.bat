@echo off
setlocal enabledelayedexpansion
title GitHub Intelligence - BUILDING PRODUCTION RELEASE (FIXED)

echo ========================================================
echo   GITHUB INTELLIGENCE SERVICE - ENTERPRISE RELEASE v2.1 🚀
echo ========================================================
echo.

:: 0. TEMIZLIK (Resource Locking Fix)
echo [0/4] Eski Surecler Temizleniyor...
taskkill /F /IM GitHubIntelligenceService.Api.exe >nul 2>&1
taskkill /F /IM node.exe >nul 2>&1
echo [OK] Hafiza temizlendi.
echo.

:: 1. FRONTEND BUILD
echo [1/4] Frontend (React) Derleniyor...
cd client
call npm run build
if errorlevel 1 (
    color 0C
    echo [HATA] Frontend derlenemedi!
    pause
    exit /b 1
)
cd ..
echo [OK] Frontend derlendi.
echo.

:: 2. BACKEND PUBLISH (Once kutuyu hazirla)
echo [2/4] Backend (API) Yayinlaniyor...
if exist "publish_output" rmdir /s /q "publish_output"

dotnet publish src/GitHubIntelligenceService.Api/GitHubIntelligenceService.Api.csproj -c Release -o ./publish_output /p:UseAppHost=true
if errorlevel 1 (
    color 0C
    echo [HATA] Backend derlenemedi!
    pause
    exit /b 1
)
echo [OK] Backend hazirlandi.
echo.

:: 3. ENTEGRASYON (Frontend -> Final Kutuya Kopyala)
echo [3/4] Dosyalar Birlestiriliyor (Integration)...

:: wwwroot klasorunu publish_output icinde olustur
mkdir "publish_output\wwwroot"

:: React dosyalarini ORADAN ORAYA kopyala
xcopy /E /Y /Q client\dist "publish_output\wwwroot" >nul
echo [OK] Frontend dosyalari eklendi.

:: 3.5 VERI TASIMA (Veritabanini da at)
if exist "src\GitHubIntelligenceService.Api\app.db" (
    copy /Y "src\GitHubIntelligenceService.Api\app.db" "publish_output\app.db" >nul
    echo [OK] Veritabani tasindi.
) else (
    echo [UYARI] Veritabani bulunamadi - Ilk calistirmada olusacak.
)
echo.

:: 4. CALISTIRMA
echo [4/4] Baslatiliyor...
echo --------------------------------------------------------
echo ADRES: http://localhost:5000
echo.
echo NOT: Bu pencereyi kapatma.
echo.

cd publish_output
GitHubIntelligenceService.Api.exe
pause
