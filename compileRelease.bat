@echo off
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" FocLauncher.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
set BUILD_STATUS=%ERRORLEVEL% 
if %BUILD_STATUS%==0 goto buildSuccess 
if not %BUILD_STATUS%==0 goto fail 
 
:buildSuccess 
echo build was successful
pause 
exit /b 0 


:fail 
echo operation failed...
pause 
exit /b 1 