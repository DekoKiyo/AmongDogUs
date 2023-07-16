Start-Sleep -Milliseconds 250
Remove-Item .\AmongDogUs\bin -Force -Recurse
dotnet build AmongDogUs/AmongDogUs.csproj