Invoke-Expression "./update_card_tiles.ps1"
msbuild /t:ResourceGenerator /p:Configuration=Debug
Invoke-Expression "./ResourceGenerator/bin/x86/Debug/ResourceGenerator.exe Resources Tiles"