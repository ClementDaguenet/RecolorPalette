@echo off
:: Get ADMIN Privs
mkdir "%windir%\BatchGotAdmin"
if '%errorlevel%' == '0' (
  rmdir "%windir%\BatchGotAdmin" & goto gotAdmin
) else ( goto UACPrompt )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute %0, "", "", "runas", 1 >> "%temp%\getadmin.vbs"
    "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    if exist "%temp%\getadmin.vbs" ( del "%temp%\getadmin.vbs" )
    pushd "%CD%"
    CD /D "%~dp0"

if not exist "RecolorTakePalette.dll" (
  echo RecolorTakePalette.dll introuvable. Compilez d'abord les plugins avec CodeLab.
  goto fail
)
if not exist "RecolorApplyPalette.dll" (
  echo RecolorApplyPalette.dll introuvable. Compilez d'abord les plugins avec CodeLab.
  goto fail
)

:: Read registry to find Paint.NET install directory
reg query HKLM\SOFTWARE\Paint.NET /v TARGETDIR 2>nul || (echo Sorry, I can't find Paint.NET! & goto store)
set PDN_DIR=
for /f "tokens=2,*" %%a in ('reg query HKLM\SOFTWARE\Paint.NET /v TARGETDIR ^| findstr TARGETDIR') do (
  set PDN_DIR=%%b
)
if not defined PDN_DIR (echo Sorry, I can't find Paint.NET! & goto store)

@echo off
cls
echo.
echo Installing Recolor Palette plugins to %PDN_DIR%\Effects\
echo.
copy /y "RecolorTakePalette.dll" "%PDN_DIR%\Effects\"
copy /y "RecolorApplyPalette.dll" "%PDN_DIR%\Effects\"
if '%errorlevel%' == '0' ( goto success ) else ( goto fail )

:store
reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders" /v Personal 2>nul || (echo Sorry, I can't find Paint.NET! & goto fail)
set PDN_DIR=
for /f "tokens=2,*" %%a in ('reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders" /v Personal ^| findstr Personal') do (
  set PDN_DIR=%%b
)
if not defined PDN_DIR (echo Sorry, I can't find Paint.NET! & goto fail)
@echo off
cls
setlocal enabledelayedexpansion
set PDN_DIR=!PDN_DIR:%%USERPROFILE%%=%USERPROFILE%!
setlocal disabledelayedexpansion
echo.
echo Standard Paint.NET not found. Installing to Documents folder ^(Store version^).
echo.
echo Installing to %PDN_DIR%\paint.net App Files\Effects\
echo.
mkdir "%PDN_DIR%\paint.net App Files\Effects\" 2>nul
copy /y "RecolorTakePalette.dll" "%PDN_DIR%\paint.net App Files\Effects\"
copy /y "RecolorApplyPalette.dll" "%PDN_DIR%\paint.net App Files\Effects\"
if '%errorlevel%' == '0' ( goto success ) else ( goto fail )

:success
echo.
echo    _____ _    _  _____ _____ ______  _____ _____  _
echo   / ____) !  ! !/  ___)  ___)  ____)/ ____) ____)! !
echo  ( (___ ! !  ! !  /  /  /   ! (__  ( (___( (___  ! !
echo   \___ \! !  ! ! (  (  (    !  __)  \___ \\___ \ ! !
echo   ____) ) !__! !  \__\  \___! (____ ____) )___) )!_!
echo  (_____/ \____/ \_____)_____)______)_____/_____/ (_)
echo.
echo RecolorTakePalette + RecolorApplyPalette installed.
goto done

:fail
echo.
echo  _____       _____ _      _
echo !  ___)/\   (_   _) !    ! !
echo ! (__ /  \    ! ! ! !    ! !
echo !  __) /\ \   ! ! ! !    ! !
echo ! ! / ____ \ _! !_! !___ !_!
echo !_!/_/    \_\_____)_____)(_)
echo.
echo Close Paint.NET and try installing again.

:done
echo.
pause
