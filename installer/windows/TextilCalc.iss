#ifndef SourceDir
  #define SourceDir "..\..\artifacts\windows\publish"
#endif

#ifndef OutputDir
  #define OutputDir "..\..\artifacts\windows"
#endif

#ifndef AppVersion
  #define AppVersion "1.1.0"
#endif

[Setup]
AppId={{D8AD33B8-4E95-4C5E-8AB3-3F768E8C3F72}
AppName=TextilCalc
AppVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\TextilCalc
DefaultGroupName=TextilCalc
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename=TextilCalc-Setup-{#AppVersion}-x64
SetupIconFile={#SourceDir}\appicon.ico
UninstallDisplayIcon={app}\TextilCalc.App.exe
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos adicionales:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\TextilCalc"; Filename: "{app}\TextilCalc.App.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\TextilCalc"; Filename: "{app}\TextilCalc.App.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\TextilCalc.App.exe"; Description: "Abrir TextilCalc"; Flags: nowait postinstall skipifsilent
