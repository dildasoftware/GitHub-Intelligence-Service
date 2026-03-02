@echo off
echo ===================================================
echo   GITHUB INTELLIGENCE SERVICE - YUKLEME ARACI
echo ===================================================

:: 1. Doğru klasöre git
cd /d "C:\Users\hp\ApıConsumerApp"

echo [1/4] Klasor kontrol ediliyor...
if not exist .git (
    echo HATA: Burada git deposu yok! Once 'git init' yapilmali.
    echo Otomatik baslatiliyor...
    git init
    git add .
    git commit -m "Initial Commit"
)

:: 2. Eski baglantiyi temizle (Hata vermemesi için)
echo [2/4] Eski baglantilar temizleniyor...
git remote remove origin 2>nul

:: 3. Yeni adresi ekle
echo [3/4] GitHub adresi ekleniyor...
git remote add origin https://github.com/dildasoftware/GitHub-Intelligence-Service.git

:: 4. Yukle
echo [4/4] GitHub'a yukleniyor (PUSH)...
echo Lutfen acilan pencerede veya asagida Sifreni gir...
echo ---------------------------------------------------
git branch -M main
git push -u origin main

echo.
echo ===================================================
if %errorlevel% neq 0 (
    echo [HATA] Yukleme basarisiz oldu!
    echo Lutfen internet baglantini ve sifreni kontrol et.
) else (
    echo [BASARILI] Proje GitHub'a yuklendi! Tebrikler!
)
echo ===================================================
pause
