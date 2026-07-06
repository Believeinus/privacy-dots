; Inno Setup script for Privacy Dots
; Per-user install by default (no admin/UAC needed), clean uninstall from
; Windows "Apps & features".

#define MyAppName "Privacy Dots"
#define MyAppVersion "1.1.1"
#define MyAppExeName "PrivacyDots.exe"

[Setup]
AppId={{7E1B3C6A-9A44-4F1D-B7D2-52A6C0F3D0E1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Hiteshwar Singh
AppCopyright=Developed by Hiteshwar Singh
DefaultDirName={autopf}\Privacy Dots
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=..\dist
OutputBaseFilename=PrivacyDots-Setup-{#MyAppVersion}
SetupIconFile=..\assets\PrivacyDots.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0

[Messages]
; Shown at the bottom-left of every wizard page
BeveledLabel=Developed by Hiteshwar Singh

[Tasks]
Name: "startup"; Description: "Start {#MyAppName} automatically when Windows starts"
Name: "desktopicon"; Description: "Create a &desktop shortcut"; Flags: unchecked

[Files]
Source: "..\dist\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; \
    ValueName: "PrivacyDots"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: startup; \
    Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName} now"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
; user settings
Type: filesandordirs; Name: "{userappdata}\PrivacyDots"

[Code]
procedure KillApp();
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/IM PrivacyDots.exe /F', '',
       SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// Close a running instance before installing (upgrade) or uninstalling
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  KillApp();
  Result := '';
end;

function InitializeUninstall(): Boolean;
begin
  KillApp();
  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Remove autostart even if it was enabled from the app instead of the installer
    RegDeleteValue(HKEY_CURRENT_USER,
      'Software\Microsoft\Windows\CurrentVersion\Run', 'PrivacyDots');
  end;
end;
