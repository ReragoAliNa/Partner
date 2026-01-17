# 🐾 Desktop Pet Installer Script
# This script "installs" the pet by moving it to AppData and creating shortcuts.

$AppName = "DesktopPet"
$InstallDir = "$env:LOCALAPPDATA\$AppName"
$ExeName = "DesktopPet.exe"
$SourcePath = "$PSScriptRoot\ReleasePackage"

Write-Host "--- Installing Desktop Pet ---" -ForegroundColor Cyan

# 1. Create Install Directory
if (!(Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir | Out-Null
    Write-Host "[✓] Created directory: $InstallDir"
}

# 2. Copy Files
Write-Host "[...] Copying files..."
if (Test-Path "$SourcePath\$ExeName") {
    Copy-Item -Path "$SourcePath\*" -Destination $InstallDir -Recurse -Force
    Write-Host "[✓] Files copied to $InstallDir"
} else {
    Write-Error "Could not find build in $SourcePath. Please run 'dotnet publish' first."
    exit
}

# 3. Create Desktop Shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\$AppName.lnk")
$Shortcut.TargetPath = "$InstallDir\$ExeName"
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Description = "Your Desktop Companion"
$Shortcut.Save()
Write-Host "[✓] Desktop shortcut created!"

# 4. Create Start Menu Shortcut
$StartMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\$AppName.lnk"
$Shortcut = $WshShell.CreateShortcut($StartMenuPath)
$Shortcut.TargetPath = "$InstallDir\$ExeName"
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Save()
Write-Host "[✓] Start Menu shortcut created!"

Write-Host ""
Write-Host "Installation Complete! You can now launch Desktop Pet from your Desktop or Start Menu." -ForegroundColor Green
Write-Host "Keep the character happy!" -ForegroundColor Yellow
