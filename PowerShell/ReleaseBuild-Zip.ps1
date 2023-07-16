Write-Host "-------- Build as the Release Zip Edition --------" -ForegroundColor Magenta

Start-Sleep -Milliseconds 250

Write-Host "-------- Comfused Enabled --------" -ForegroundColor Green
.\PowerShell\Build\Release.ps1
Write-Host "-------- Build Completed --------" -ForegroundColor Green

Start-Sleep -Milliseconds 250

Write-Host "-------- Copy Files to AmongUs --------" -ForegroundColor Blue
.\PowerShell\Build\CopyFiles.ps1
Write-Host "-------- Copy Completed --------" -ForegroundColor Green

Start-Sleep -Milliseconds 250

Write-Host "-------- Compression Start --------" -ForegroundColor Blue
.\PowerShell\Build\ReleaseZip.ps1
Write-Host "-------- Compression Completed --------" -ForegroundColor Green
.\PowerShell\Build\Completed.ps1