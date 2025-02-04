#define QuestVersion '5.9.0'
#define SetupVersion '590'

[Setup]
AppName=Quest
AppVersion={#QuestVersion}
AppVerName=Quest {#QuestVersion}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppCopyright=Copyright © 2025 Alex Warren, Andy Joel and contributors
VersionInfoVersion={#QuestVersion}
AppPublisher=Alex Warren
AppPublisherURL=https://textadventures.co.uk/
AppSupportURL=https://textadventures.co.uk/help
AppUpdatesURL=https://textadventures.co.uk/quest/desktop
OutputBaseFilename=quest{#SetupVersion}
DefaultGroupName=Quest
DefaultDirName={autopf}\Quest 5
UninstallDisplayIcon={app}\Quest.exe
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
SolidCompression=yes
PrivilegesRequired=admin
ChangesAssociations=yes
UsePreviousSetupType=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\Quest\bin\Release\*.*"; Excludes: "*.vshost.*,*.pdb,\*.xml,\x86\*"; DestDir: "{app}"; Flags: recursesubdirs
Source: "..\Dependencies\VC_redist.x64.exe"; DestDir: "{tmp}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\Quest"; Filename: "{app}\Quest.exe"
Name: "{autodesktop}\Quest"; Filename: "{app}\Quest.exe"; Tasks: desktopicon; WorkingDir: {app}

[Run]
Filename: "{tmp}\VC_redist.x64.exe"; Parameters: "/quiet"; StatusMsg: "Installing components..."
Filename: "{app}\Quest.exe"; Description: "Launch Quest"; Flags: nowait postinstall skipifsilent

[Registry]
; File association: ASLX
Root: HKCR; Subkey: ".aslx"; ValueType: string; ValueName: ""; ValueData: "Quest-aslx"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "Quest-aslx"; ValueType: string; ValueName: ""; ValueData: "Quest Game Source"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Quest-aslx\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\QUEST.EXE,0"
Root: HKCR; Subkey: "Quest-aslx\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\QUEST.EXE"" ""%1"""
; File association: QUEST
Root: HKCR; Subkey: ".quest"; ValueType: string; ValueName: ""; ValueData: "Quest-quest"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "Quest-quest"; ValueType: string; ValueName: ""; ValueData: "Quest Game"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Quest-quest\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\QUEST.EXE,0"
Root: HKCR; Subkey: "Quest-quest\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\QUEST.EXE"" ""%1"""
; File association: QUEST-SAVE
Root: HKCR; Subkey: ".quest-save"; ValueType: string; ValueName: ""; ValueData: "Quest-quest-save"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "Quest-quest-save"; ValueType: string; ValueName: ""; ValueData: "Quest Saved Game"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Quest-quest-save\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\QUEST.EXE,0"
Root: HKCR; Subkey: "Quest-quest-save\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\QUEST.EXE"" ""%1"""
; File association: ASL
Root: HKCR; Subkey: ".asl"; ValueType: string; ValueName: ""; ValueData: "Quest-asl"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "Quest-asl"; ValueType: string; ValueName: ""; ValueData: "Quest Game"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Quest-asl\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\QUEST.EXE,0"
Root: HKCR; Subkey: "Quest-asl\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\QUEST.EXE"" ""%1"""
; File association: CAS
Root: HKCR; Subkey: ".cas"; ValueType: string; ValueName: ""; ValueData: "Quest-cas"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "Quest-cas"; ValueType: string; ValueName: ""; ValueData: "Quest Game"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Quest-cas\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\QUEST.EXE,0"
Root: HKCR; Subkey: "Quest-cas\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\QUEST.EXE"" ""%1"""
; File association: QSG
Root: HKCR; Subkey: ".qsg"; ValueType: string; ValueName: ""; ValueData: "Quest-qsg"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "Quest-qsg"; ValueType: string; ValueName: ""; ValueData: "Quest Saved Game"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Quest-qsg\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\QUEST.EXE,0"
Root: HKCR; Subkey: "Quest-qsg\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\QUEST.EXE"" ""%1"""
