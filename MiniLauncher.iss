[Setup]
AppId={{9A8B7C6D-5E4F-3A2B-1C0D-E9F8A7B6C5D4}
AppName=MiniLauncher
AppVersion=1.0.0
AppPublisher=Lyra Everlyn
AppPublisherURL=https://github.com/Lyra-Everlyn/MiniLauncher
AppSupportURL=https://github.com/Lyra-Everlyn/MiniLauncher/issues
AppUpdatesURL=https://github.com/Lyra-Everlyn/MiniLauncher/releases
DefaultDirName={autopf}\MiniLauncher
DefaultGroupName=MiniLauncher

SetupIconFile=MiniLauncher\Assets\Icons\icon.ico
UninstallDisplayIcon={app}\MiniLauncher.exe
OutputDir=Installer
OutputBaseFilename=MiniLauncher_Setup_v1.0.0
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "Publish\MiniLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\MiniLauncher"; Filename: "{app}\MiniLauncher.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\MiniLauncher"; Filename: "{app}\MiniLauncher.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{cmd}"; Parameters: "/c echo Installed > ""{app}\.installed"""; Flags: runhidden
Filename: "{app}\MiniLauncher.exe"; Description: "{cm:LaunchProgram,MiniLauncher}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\appsettings.json"
Type: files; Name: "{app}\.installed"
Type: files; Name: "{app}\MiniLauncher.exe.old"
Type: files; Name: "{app}\MiniLauncher.exe.new"