@echo off

call update_card_tiles.bat

if not exist "/Resources/Tiles" (
	mkdir "Resources/Tiles"
)
msbuild /t:ResourceGenerator /p:Configuration=Debug
.\ResourceGenerator\bin\x86\Debug\ResourceGenerator.exe .\Resources Tiles