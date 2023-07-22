Start-Sleep -Milliseconds 250
$Directory = ".\AmongDogUs\bin\Debug\net7.0"
$AmongUs = $env:AmongUsDogRun # Environment Variables (GrandTheftAutoV)

# DekoKiyoTools.dll
Copy-Item  $Directory\AmongDogUs.dll  $AmongUs\BepInEx\Plugins -Force -ErrorAction SilentlyContinue
Write-Host "AmongDogUs.dll was moved!" -ForegroundColor DarkCyan