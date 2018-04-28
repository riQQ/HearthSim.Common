if (Test-Path "hs-card-tiles") {
  Remove-Item -Recurse -Force "hs-card-tiles"
}

git clone --depth=1 "https://github.com/HearthSim/hs-card-tiles.git"

if (!(Test-Path "./Resources/Tiles")) {
	New-Item -Name "./Resources/Tiles" -ItemType directory
}

Move-Item -Path "./hs-card-tiles/Tiles/*" -Destination "./Resources/Tiles/" -Force

Remove-Item -Recurse -Force "./hs-card-tiles"