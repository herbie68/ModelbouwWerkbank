#define AppName "Modelbuilder"
#define AppExeName "Modelbouwer.exe"
#define AppPublisher "HN Software development"
#define AppVersion GetVersionNumbersString("..\Builds\Publish\" + AppExeName)

[Setup]
AppId={{F03368A6-429E-4683-910D-F1DB7F54B380}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://hnsoftwaredevelopment.nl/
AppSupportURL=https://hnsoftwaredevelopment.nl/
AppUpdatesURL=https://hnsoftwaredevelopment.nl/
DefaultDirName={autopf}\HnSoftwaredevelopment\Modelbuilder
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableReadyMemo=yes
DisableFinishedPage=yes
DisableWelcomePage=yes
AllowNoIcons=yes
UsePreviousAppDir=no
OutputDir=..\Builds\Installer
OutputBaseFilename=ModelbuilderSetup-{#AppVersion}
SetupIconFile=..\Resources\applogo.ico
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardImageFile=Assets\developer-logo-wizard.bmp
WizardSmallImageFile=Assets\app-logo-small.bmp
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} installer
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
VersionInfoVersion={#AppVersion}

[Languages]
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"

[Messages]
SetupAppTitle=Modelbuilder installeren of bijwerken
SetupWindowTitle=Modelbuilder installeren of bijwerken
ButtonInstall=Installeren
ReadyLabel1=Modelbuilder wordt geinstalleerd of bijgewerkt.
ReadyLabel2a=Klik op Installeren om te beginnen.
ReadyLabel2b=Klik op Installeren om te beginnen.

[InstallDelete]
Type: files; Name: "{app}\*.exe"
Type: files; Name: "{app}\*.dll"
Type: files; Name: "{app}\*.pdb"
Type: files; Name: "{app}\*.deps.json"
Type: files; Name: "{app}\*.runtimeconfig.json"

[Files]
Source: "..\Builds\Publish\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Builds\Publish\Resources\Config\modelbouwer.config"; DestDir: "{app}\Resources\Config"; Flags: ignoreversion onlyifdoesntexist

[Icons]
Name: "{autoprograms}\Modelbouwer"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#AppExeName}"
