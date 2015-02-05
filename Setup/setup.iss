; Based on Modular InnoSetup Dependency Installer:
; github.com/stfx/innodependencyinstaller
; codeproject.com/Articles/20868/NET-Framework-1-1-2-0-3-5-Installer-for-InnoSetup

#define QuestVersion '5.6'
#define SetupVersion '560'

[Setup]
AppName=Quest
AppVersion={#QuestVersion}
AppVerName=Quest {#QuestVersion}
AppCopyright=Copyright © 2015 Alex Warren
VersionInfoVersion={#QuestVersion}
AppPublisher=Alex Warren
AppPublisherURL=http://textadventures.co.uk/
AppSupportURL=http://textadventures.co.uk/help
AppUpdatesURL=http://textadventures.co.uk/quest/desktop
OutputBaseFilename=quest{#SetupVersion}
DefaultGroupName=Quest
DefaultDirName={pf}\Quest 5
UninstallDisplayIcon={app}\Quest.exe
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
SolidCompression=yes
PrivilegesRequired=admin
ChangesAssociations=yes
MinVersion=5.1sp3
UsePreviousSetupType=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\Quest\bin\x86\Release\*.*"; Excludes: "*.vshost.*,*.pdb,\*.xml"; DestDir: "{app}"; Flags: recursesubdirs
Source: "..\Dependencies\vcredist_x86.exe"; DestDir: "{tmp}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Icons]
Name: "{group}\Quest"; Filename: "{app}\Quest.exe"
Name: "{commondesktop}\Quest"; Filename: "{app}\Quest.exe"; Tasks: desktopicon; WorkingDir: {app}

[Run]
Filename: "{tmp}\vcredist_x86.exe"; Parameters: "/quiet"; StatusMsg: "Installing components..."
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


#include "scripts\products.iss"
#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2

[Code]
function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();

	if (not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then
		dotnetfx40client();

	Result := true;
end;
