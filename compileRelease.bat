@echo off

:constants
set MS_BUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
set BUILD_PLATFORM="Any CPU"

set METADATA_OUTPUT="gen"
set CURRENT_METADATA_LOCATION="https://republicatwar.com/downloads/FocLauncher/LauncherUpdateData.xml"
set COPY_FILES_LOCATION="C:\Users\Anakin\source\repos\AnakinSklavenwalker\FoC-Mod-Launcher-Builds"

set METADATA_CREATOR_BIN=".\tools\MetadataCreator\bin"
set METADATA_CREATOR_FILE="MetadataCreator.exe"


:configureBuild
echo "Select [B] to build or [R] to rebuild the visual studio solution"
CHOICE /C BR
IF %ERRORLEVEL% EQU 1 set BUILD_TYPE="Build"
IF %ERRORLEVEL% EQU 2 set BUILD_TYPE="Rebuild"

echo "Select [D] or [R] to build as [D]ebug or [R]elease"
CHOICE /C DR
IF %ERRORLEVEL% EQU 1 set BUILD_CONFIG="Debug"
IF %ERRORLEVEL% EQU 2 set BUILD_CONFIG="Release"


:build
echo %BUILD_TYPE% with configuration %BUILD_CONFIG% on platfrom %BUILD_PLATFORM%

%MS_BUILD_PATH% FocLauncher.sln /t:%BUILD_TYPE% /p:Configuration=%BUILD_CONFIG% /p:Platform=%BUILD_PLATFORM% /m
set BUILD_STATUS=%ERRORLEVEL% 

if %BUILD_STATUS%==0 goto buildSuccess 
if not %BUILD_STATUS%==0 goto fail 
 
:buildSuccess 
echo build was successful
pause 
goto copyMetadataCreator


:copyMetadataCreator

echo removing old metadata creator
if exist %METADATA_CREATOR_FILE% (
	del %METADATA_CREATOR_FILE%
)

echo copying metadata creator to current directory
if exist "%METADATA_CREATOR_BIN%\%BUILD_CONFIG%\%METADATA_CREATOR_FILE%" (
	copy "%METADATA_CREATOR_BIN%\%BUILD_CONFIG%\%METADATA_CREATOR_FILE%" %METADATA_CREATOR_FILE%
) else (
	goto fail
)


:configureMetadata

echo "Select the appliction type the metadata shall be generated for:"
echo 1 - Stable
echo 2 - Beta
echo 3 - Test
CHOICE /C 123
IF %ERRORLEVEL% EQU 1 set APPLICATION_TYPE="Stable"
IF %ERRORLEVEL% EQU 2 set APPLICATION_TYPE="Beta"
IF %ERRORLEVEL% EQU 3 set APPLICATION_TYPE="Test"

echo "Select the interation mode"
echo 0 - no integration
echo 1 - product integration
echo 2 - full dependency integration
echo 3 - version based dependency integration
CHOICE /C 0123
IF %ERRORLEVEL% EQU 1 set INTEGRATION_MODE=0
IF %ERRORLEVEL% EQU 2 set INTEGRATION_MODE=1
IF %ERRORLEVEL% EQU 3 set INTEGRATION_MODE=2
IF %ERRORLEVEL% EQU 4 set INTEGRATION_MODE=3


:createMetadata
MetadataCreator.exe -o %METADATA_OUTPUT% -b %BUILD_CONFIG% -f %CURRENT_METADATA_LOCATION% -t %APPLICATION_TYPE% -m %INTEGRATION_MODE% -l %COPY_FILES_LOCATION%
set CREATOR_STATUS=%ERRORLEVEL% 
if %BUILD_STATUS%==0 goto createSuccess
if not %CREATOR_STATUS%==0 goto fail

:createSuccess
echo creating metadata was successful
pause 
goto commitChangesChoice


:commitChangesChoice

echo "Shall the changes to %COPY_FILES_LOCATION% be commited"
CHOICE /C YN
IF %ERRORLEVEL% EQU 1 goto commitChanges
exit /b 0

:commitChanges

cd %COPY_FILES_LOCATION%
git add .
git commit -m "Updated version for %APPLICATION_TYPE%"
if not %BUILD_STATUS%==0 goto fail 

echo commit created.
echo pushing needs to be done manually.
goto success


:success
echo operation finished
pause
exit /b 0 


:fail 
echo operation failed...
pause 
exit /b 1 