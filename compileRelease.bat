@echo off
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" FocLauncher.sln /t:Build /p:Configuration=Release /p:Platform="Any CPU" /m
set BUILD_STATUS=%ERRORLEVEL% 
if %BUILD_STATUS%==0 goto buildSuccess 
if not %BUILD_STATUS%==0 goto fail 
 
:buildSuccess 
echo build was successful
pause 
goto createMetadata
 
 
:createMetadata
MetadataCreator.exe -o gen
set CREATOR_STATUS=%ERRORLEVEL% 
if %BUILD_STATUS%==0 goto createSuccess
if not %CREATOR_STATUS%==0 goto fail

:createSuccess
pause 
exit /b 0


:fail 
echo operation failed...
pause 
exit /b 1 