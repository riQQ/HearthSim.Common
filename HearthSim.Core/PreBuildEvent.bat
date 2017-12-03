@echo off

set SolutionDir=%~dp1
set ProjectDir=%~dp2
set TargetDir=%~dp3

echo SolutionDir="%SolutionDir%"
echo ProjectDir="%ProjectDir%"
echo TargetDir="%TargetDir%""

echo.
echo.


if exist "%SolutionDir%HearthDb" (
  echo Updating "%SolutionDir%HearthDb" to origin/master
  git -C "%SolutionDir%HearthDb" fetch
  git -C "%SolutionDir%HearthDb" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HearthDb.git "%SolutionDir%HearthDb"
)

echo.

if exist "%SolutionDir%HearthMirror" (
  echo Updating "%SolutionDir%HearthMirror" to origin/master
  git -C "%SolutionDir%HearthMirror" fetch
  git -C "%SolutionDir%HearthMirror" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HearthMirror.git "%SolutionDir%HearthMirror"
)

echo.

if exist "%SolutionDir%HSReplay-Api" (
  echo Updating "%SolutionDir%HSReplay"-Api to origin/master
  git -C "%SolutionDir%HSReplay-Api" fetch
  git -C "%SolutionDir%HSReplay-Api" reset --hard origin/master
) else (
  git clone --depth 1 https://github.com/HearthSim/HSReplay-API-Client.git "%SolutionDir%HSReplay-API-Client"
)

echo Completed.
