@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%""

echo.
echo.

cd "%SolutionDir%"

if not exist "%TargetDir%Images\Themes" (
	echo Creating missing directory "%TargetDir%Images\Themes"
	mkdir "%TargetDir%Images\Themes"
	echo.
)

if exist "%ProjectDir%Resources\Themes" if exist "%TargetDir%Images\Themes" (
	echo Copying Themes from "%ProjectDir%Resources\Themes" to "%TargetDir%Images\Themes"
	xcopy /E /Y /Q "%ProjectDir%Resources\Themes" "%TargetDir%Images\Themes"
	echo.
)

echo Completed.
