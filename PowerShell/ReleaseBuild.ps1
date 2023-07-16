Write-Host "-------- Build as the Release Edition --------" -ForegroundColor Magenta

Start-Sleep -Milliseconds 250

Write-Host "-------- Comfused Disabled --------" -ForegroundColor Red
.\PowerShell\Build\Release.ps1
Write-Host "-------- Build Completed --------" -ForegroundColor Green

Start-Sleep -Milliseconds 250

Write-Host "-------- Copy Files to AmongUs --------" -ForegroundColor Blue
.\PowerShell\Build\CopyFiles.ps1
Write-Host "-------- Copy Completed --------" -ForegroundColor Blue
.\PowerShell\Build\Completed.ps1