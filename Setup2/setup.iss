; Based on Modular InnoSetup Dependency Installer:
; github.com/stfx/innodependencyinstaller
; codeproject.com/Articles/20868/NET-Framework-1-1-2-0-3-5-Installer-for-InnoSetup

#define MyAppSetupName 'MyProgram'
#define MyAppVersion '5.0'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright=Copyright © stfx 2007-2014
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=stfx
AppPublisher=stfx
;AppPublisherURL=http://...
;AppSupportURL=http://...
;AppUpdatesURL=http://...
OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={pf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\MyProgram.exe
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
;SetupIconFile=MyProgramIcon
SolidCompression=yes

;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
;MinVersion=0,5.0
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "src\MyProgram-x64.exe"; DestDir: "{app}"; DestName: "MyProgram.exe"; Check: IsX64
Source: "src\MyProgram-IA64.exe"; DestDir: "{app}"; DestName: "MyProgram.exe"; Check: IsIA64
Source: "src\MyProgram.exe"; DestDir: "{app}"; Check: not Is64BitInstallMode

[Icons]
Name: "{group}\MyProgram"; Filename: "{app}\MyProgram"
Name: "{group}\{cm:UninstallProgram,MyProgram}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\MyProgram"; Filename: "{app}\MyProgram.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\MyProgram"; Filename: "{app}\MyProgram.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\MyProgram.exe"; Description: "{cm:LaunchProgram,MyProgram}"; Flags: nowait postinstall skipifsilent

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
