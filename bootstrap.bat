@echo off

nuget restore
git clone https://github.com/HearthSim/HearthDb HearthDb
git clone https://github.com/HearthSim/HearthMirror HearthMirror
git clone https://github.com/HearthSim/HSReplay-API-Client.git HSReplay-API-Client
call update_card_tiles.bat