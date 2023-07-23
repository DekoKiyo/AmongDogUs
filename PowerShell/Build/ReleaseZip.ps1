Start-Sleep -Milliseconds 250

$Release = ".\Release"
$DLLPath = ".\AmongDogUs\bin\Debug\net6.0"
$AmongUs = $env:AmongUsDogRun # Environment Variables (GrandTheftAutoV)

New-Item $Release -ItemType Directory -ErrorAction SilentlyContinue
New-Item $Release\AmongDogUs -ItemType Directory -ErrorAction SilentlyContinue
New-Item $Release\AmongDogUs\dotnet -ItemType Directory -ErrorAction SilentlyContinue
New-Item $Release\AmongDogUs\BepInEx -ItemType Directory -ErrorAction SilentlyContinue
New-Item $Release\AmongDogUs\BepInEx\Plugins -ItemType Directory -ErrorAction SilentlyContinue

Copy-Item $AmongUs\BepInEx\Core $Release\AmongDogUs\BepInEx\ -Force -Recurse
Write-Host "Core folder were copied!" -ForegroundColor DarkCyan
Copy-Item $AmongUs\BepInEx\Interop $Release\AmongDogUs\BepInEx\ -Force -Recurse
Write-Host "Interop folder were copied!" -ForegroundColor DarkCyan
Copy-Item $AmongUs\dotnet $Release\AmongDogUs\ -Force -Recurse
Write-Host "dotnet folder were copied!" -ForegroundColor DarkCyan
Copy-Item $DLLPath\AmongDogUs.dll $Release\AmongDogUs\BepInEx\Plugins -Force -Recurse
Write-Host "AmongDogUs.dll were copied!" -ForegroundColor DarkCyan
Copy-Item $AmongUs\.doorstop_version $Release\AmongDogUs -Force -Recurse
Write-Host ".doorstop_version were copied!" -ForegroundColor DarkCyan
Copy-Item $AmongUs\doorstop_config.ini $Release\AmongDogUs -Force -Recurse
Write-Host "doorstop_config.ini were copied!" -ForegroundColor DarkCyan
Copy-Item $AmongUs\steam_appid.txt $Release\AmongDogUs -Force -Recurse
Write-Host "steam_appid.txt were copied!" -ForegroundColor DarkCyan
Copy-Item $AmongUs\winhttp.dll $Release\AmongDogUs -Force -Recurse
Write-Host "winhttp.dll were copied!" -ForegroundColor DarkCyan

Start-Sleep -Milliseconds 250

# Create zip archive
Remove-Item $Release\AmongDogUs.zip -ErrorAction SilentlyContinue
7z.exe a $Release\AmongDogUs.zip .\$Release\AmongDogUs\
Write-Host "The AmongDogUs.zip file was successfully archived!" -ForegroundColor Green