@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo.
echo  Dang chay server tai http://localhost:8080/
echo  Mo link do bang Chrome hoac Edge, roi bam nut cai dat tren thanh dia chi.
echo  Dong cua so nay de tat server.
echo.
start "" http://localhost:8080/
python -m http.server 8080
if errorlevel 1 (
  echo.
  echo  Khong tim thay Python. Cai tai python.org va nho tich "Add Python to PATH".
  pause
)
