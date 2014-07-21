; Based on Modular InnoSetup Dependency Installer:
; github.com/stfx/innodependencyinstaller
; codeproject.com/Articles/20868/NET-Framework-1-1-2-0-3-5-Installer-for-InnoSetup

#define QuestVersion '5.5.1'
#define SetupVersion '551'

[Setup]
AppName=Quest
AppVersion={#QuestVersion}
AppVerName=Quest {#QuestVersion}
AppCopyright=Copyright © 2014 Alex Warren
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
;ChangesAssociations=yes
MinVersion=5.1sp3

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Files]
Source: "..\Quest\bin\x86\Release\*.*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Quest"; Filename: "{app}\Quest.exe"

[Run]
Filename: "{app}\Quest.exe"; Description: "Launch Quest"; Flags: nowait postinstall skipifsilent

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
