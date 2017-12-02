@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%""

echo.
echo.

echo Running resource generator...
cd "%SolutionDir%"
call "generate_resources.bat"

if not exist "%TargetDir%Images\Tiles" (
	echo Creating missing directory "%TargetDir%Images\Tiles"
	mkdir "%TargetDir%Images\Tiles"
	echo.
)

if not exist "%TargetDir%Images\Themes" (
	echo Creating missing directory "%TargetDir%Images\Themes"
	mkdir "%TargetDir%Images\Themes"
	echo.
)

if exist "%SolutionDir%Resources\Generated\Tiles" if exist "%TargetDir%Images\Tiles" (
	echo Copying Generated tiles from "%SolutionDir%Resources\Generated\Tiles" to "%TargetDir%Images\Tiles"
	xcopy /E /Y /Q "%SolutionDir%Resources\Generated\Tiles" "%TargetDir%Images\Tiles"
	echo.
)

if exist "%ProjectDir%Resources\Themes" if exist "%TargetDir%Images\Themes" (
	echo Copying Themes from "%ProjectDir%Resources\Themes" to "%TargetDir%Images\Themes"
	xcopy /E /Y /Q "%ProjectDir%Resources\Themes" "%TargetDir%Images\Themes"
	echo.
)

echo Completed.
