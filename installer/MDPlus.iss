; =====================================================================
; MDPlus - Native Windows Markdown Viewer Setup Installer Script
; Inno Setup 6.x Configuration with .NET 8 Desktop Runtime Bootstrapper
; =====================================================================

#define MyAppName "MDPlus"
#define MyAppVersion "1.07"
#define MyAppPublisher "MDPlus"
#define MyAppURL "https://github.com/nickf-sudomania/mdplusplus"
#define MyAppExeName "MDPlus.exe"
#define MyAppAssocName "Markdown Document"
#define MyAppAssocKey "MDPlus.Document"

[Setup]
; Unique application GUID for uninstallation & updates
AppId={{E67BD82D-C178-43B3-9F93-78B43DF331B2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=..\dist
OutputBaseFilename=MDPlus-Setup
SetupIconFile=..\src\Resources\AppIcon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern dynamic
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
ChangesAssociations=yes
VersionInfoVersion=1.0.7.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=MDPlus Setup Installer
VersionInfoProductVersion=1.0.7.0
VersionInfoProductName=MDPlus Markdown Viewer
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "fileassoc_md"; Description: "Associate Markdown files (.md) with MDPlus"; GroupDescription: "File associations:"; Flags: checkedonce
Name: "fileassoc_markdown"; Description: "Associate Markdown files (.markdown) with MDPlus"; GroupDescription: "File associations:"; Flags: checkedonce

[Files]
; Primary MDPlus application files
Source: "..\dist\MDPlus.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\Resources\AppIcon.ico"; DestDir: "{app}\Resources"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\sample_docs\*"; DestDir: "{app}\sample_docs"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; App Paths Registration (enables Win+R -> mdplus)
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#MyAppExeName}"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#MyAppExeName}"; ValueType: string; ValueName: "Path"; ValueData: "{app}"; Flags: uninsdeletekey

; Capabilities Registration for Windows "Default Apps"
Root: HKA; Subkey: "Software\MDPlus\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "MDPlus - Native Windows Markdown Viewer inspired by Notepad and Notepad++"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\MDPlus\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\MDPlus\Capabilities\FileAssociations"; ValueType: string; ValueName: ".md"; ValueData: "{#MyAppAssocKey}"; Flags: uninsdeletekey; Tasks: fileassoc_md
Root: HKA; Subkey: "Software\MDPlus\Capabilities\FileAssociations"; ValueType: string; ValueName: ".markdown"; ValueData: "{#MyAppAssocKey}"; Flags: uninsdeletekey; Tasks: fileassoc_markdown
Root: HKA; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: "Software\MDPlus\Capabilities"; Flags: uninsdeletevalue

; Document ProgID
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey

; File Extension Associations (.md)
Root: HKA; Subkey: "Software\Classes\.md"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocKey}"; Flags: uninsdeletevalue; Tasks: fileassoc_md
Root: HKA; Subkey: "Software\Classes\.md\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc_md

; File Extension Associations (.markdown)
Root: HKA; Subkey: "Software\Classes\.markdown"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocKey}"; Flags: uninsdeletevalue; Tasks: fileassoc_markdown
Root: HKA; Subkey: "Software\Classes\.markdown\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue; Tasks: fileassoc_markdown

; Shell Context Menu: "Open with MDPlus" on right-click
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\.md\shell\OpenWithMDPlus"; ValueType: string; ValueName: ""; ValueData: "Open with MDPlus"; Flags: uninsdeletekey; Tasks: fileassoc_md
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\.md\shell\OpenWithMDPlus"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppExeName}"",0"; Flags: uninsdeletekey; Tasks: fileassoc_md
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\.md\shell\OpenWithMDPlus\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: fileassoc_md

Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\.markdown\shell\OpenWithMDPlus"; ValueType: string; ValueName: ""; ValueData: "Open with MDPlus"; Flags: uninsdeletekey; Tasks: fileassoc_markdown
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\.markdown\shell\OpenWithMDPlus"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppExeName}"",0"; Flags: uninsdeletekey; Tasks: fileassoc_markdown
Root: HKA; Subkey: "Software\Classes\SystemFileAssociations\.markdown\shell\OpenWithMDPlus\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: fileassoc_markdown

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
const
  DotNetRuntimeUrl = 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe';
  DotNetRuntimeFileName = 'windowsdesktop-runtime-8.0-win-x64.exe';

var
  DownloadPage: TDownloadWizardPage;
  DotNetNeeded: Boolean;

function HasMatchingDirectory(const BasePattern: String): Boolean;
var
  FindRec: TFindRec;
begin
  Result := False;
  if FindFirst(BasePattern, FindRec) then
  begin
    try
      repeat
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
        begin
          if (FindRec.Name <> '.') and (FindRec.Name <> '..') then
          begin
            Result := True;
            Break;
          end;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function IsDotNet8DesktopInstalled(): Boolean;
var
  DisplayName: String;
  I: Integer;
  UninstallKey: String;
  SubKeys: TArrayOfString;
  DotNetRoot: String;
begin
  Result := False;

  // 1. Check Registry: 64-bit Uninstall keys for "Microsoft Windows Desktop Runtime - 8."
  if RegGetSubkeyNames(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall', SubKeys) then
  begin
    for I := 0 to GetArrayLength(SubKeys) - 1 do
    begin
      UninstallKey := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' + SubKeys[I];
      if RegQueryStringValue(HKLM, UninstallKey, 'DisplayName', DisplayName) then
      begin
        if Pos('Windows Desktop Runtime - 8.', DisplayName) > 0 then
        begin
          Log('Detected .NET 8 Desktop Runtime in Registry: ' + DisplayName);
          Result := True;
          Exit;
        end;
      end;
    end;
  end;

  // Check 32-bit / WOW6432Node Uninstall keys
  if RegGetSubkeyNames(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall', SubKeys) then
  begin
    for I := 0 to GetArrayLength(SubKeys) - 1 do
    begin
      UninstallKey := 'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\' + SubKeys[I];
      if RegQueryStringValue(HKLM, UninstallKey, 'DisplayName', DisplayName) then
      begin
        if Pos('Windows Desktop Runtime - 8.', DisplayName) > 0 then
        begin
          Log('Detected .NET 8 Desktop Runtime in WOW64 Registry: ' + DisplayName);
          Result := True;
          Exit;
        end;
      end;
    end;
  end;

  // 2. Check Registry: dotnet sharedfx Microsoft.WindowsDesktop.App
  if RegGetValueNames(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', SubKeys) then
  begin
    for I := 0 to GetArrayLength(SubKeys) - 1 do
    begin
      if Pos('8.', SubKeys[I]) = 1 then
      begin
        Log('Detected .NET 8 Desktop Runtime in sharedfx registry: ' + SubKeys[I]);
        Result := True;
        Exit;
      end;
    end;
  end;

  // 3. Check 64-bit Program Files: %ProgramFiles%\dotnet\shared\Microsoft.WindowsDesktop.App\8.*
  if HasMatchingDirectory(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*')) then
  begin
    Log('Detected .NET 8 Desktop Runtime directory in 64-bit Program Files');
    Result := True;
    Exit;
  end;

  // 4. Check Program Files (default): %ProgramFiles%\dotnet\shared\Microsoft.WindowsDesktop.App\8.*
  if HasMatchingDirectory(ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*')) then
  begin
    Log('Detected .NET 8 Desktop Runtime directory in Program Files');
    Result := True;
    Exit;
  end;

  // 5. Check LocalAppData for user-level dotnet installation
  if HasMatchingDirectory(ExpandConstant('{localappdata}\Microsoft\dotnet\shared\Microsoft.WindowsDesktop.App\8.*')) then
  begin
    Log('Detected .NET 8 Desktop Runtime directory in LocalAppData');
    Result := True;
    Exit;
  end;

  // 6. Check custom DOTNET_ROOT environment variable if set
  DotNetRoot := GetEnv('DOTNET_ROOT');
  if DotNetRoot <> '' then
  begin
    if HasMatchingDirectory(DotNetRoot + '\shared\Microsoft.WindowsDesktop.App\8.*') then
    begin
      Log('Detected .NET 8 Desktop Runtime directory via DOTNET_ROOT: ' + DotNetRoot);
      Result := True;
      Exit;
    end;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  DotNetNeeded := not IsDotNet8DesktopInstalled();
  if DotNetNeeded then
    Log('Prerequisite: Microsoft .NET 8 Windows Desktop Runtime is required.')
  else
    Log('Prerequisite: Microsoft .NET 8 Windows Desktop Runtime is already installed.');
end;

procedure InitializeWizard();
begin
  DownloadPage := CreateDownloadPage(
    'Prerequisite Download',
    'Downloading Microsoft .NET 8 Windows Desktop Runtime...',
    nil);
  DownloadPage.ShowBaseNameInsteadOfUrl := True;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  LocalInstallerPath: String;
begin
  Result := True;

  if CurPageID = wpReady then
  begin
    DotNetNeeded := not IsDotNet8DesktopInstalled();

    if DotNetNeeded then
    begin
      // Check if runtime installer is bundled locally alongside setup
      LocalInstallerPath := ExpandConstant('{src}\' + DotNetRuntimeFileName);
      if FileExists(LocalInstallerPath) then
      begin
        Log('Found local prerequisite installer: ' + LocalInstallerPath);
        Exit;
      end;

      // Check if already downloaded in temporary directory
      LocalInstallerPath := ExpandConstant('{tmp}\' + DotNetRuntimeFileName);
      if FileExists(LocalInstallerPath) then
      begin
        Log('Prerequisite installer already downloaded at: ' + LocalInstallerPath);
        Exit;
      end;

      if not WizardSilent then
      begin
        if SuppressibleMsgBox(
          'MDPlus requires the Microsoft .NET 8 Windows Desktop Runtime to run.' + #13#10#13#10 +
          'The setup wizard will now automatically download and install this official prerequisite from Microsoft.' + #13#10#13#10 +
          'Click Yes to proceed with the download and installation.',
          mbInformation, MB_YESNO, IDYES) <> IDYES then
        begin
          Result := False;
          Exit;
        end;
      end;

      DownloadPage.Clear;
      DownloadPage.Add(DotNetRuntimeUrl, DotNetRuntimeFileName, '');
      DownloadPage.Show;
      try
        try
          DownloadPage.Download;
          Result := True;
        except
          if DownloadPage.AbortedByUser then
            Log('Prerequisite download cancelled by user.')
          else
            SuppressibleMsgBox(
              'Failed to download the Microsoft .NET 8 Desktop Runtime:' + #13#10 +
              GetExceptionMessage + #13#10#13#10 +
              'Please verify your internet connection or download it manually from:' + #13#10 +
              'https://dotnet.microsoft.com/download/dotnet/8.0',
              mbCriticalError, MB_OK, IDOK);
          Result := False;
        end;
      finally
        DownloadPage.Hide;
      end;
    end;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  InstallerPath: String;
  ResultCode: Integer;
  ExecSuccess: Boolean;
begin
  Result := '';

  DotNetNeeded := not IsDotNet8DesktopInstalled();
  if DotNetNeeded then
  begin
    // 1. Check local source directory first
    InstallerPath := ExpandConstant('{src}\' + DotNetRuntimeFileName);

    // 2. Check temporary download directory
    if not FileExists(InstallerPath) then
      InstallerPath := ExpandConstant('{tmp}\' + DotNetRuntimeFileName);

    // 3. If missing (e.g. silent installation where NextButtonClick was skipped), download it now
    if not FileExists(InstallerPath) then
    begin
      Log('Prerequisite installer not found locally. Downloading prerequisite in PrepareToInstall...');
      try
        DownloadTemporaryFile(DotNetRuntimeUrl, DotNetRuntimeFileName, '', nil);
        InstallerPath := ExpandConstant('{tmp}\' + DotNetRuntimeFileName);
      except
        Result := 'Failed to download the Microsoft .NET 8 Windows Desktop Runtime prerequisite: ' +
                  GetExceptionMessage + #13#10 +
                  'Please install it manually from https://dotnet.microsoft.com/download/dotnet/8.0';
        Exit;
      end;
    end;

    if not FileExists(InstallerPath) then
    begin
      Result := 'Prerequisite installer not found at: ' + InstallerPath;
      Exit;
    end;

    Log('Installing prerequisite: ' + InstallerPath);

    // 4. Launch Microsoft runtime installer:
    // If running in elevated mode, use Exec directly.
    // If running unelevated, use ShellExec with 'runas' to trigger UAC elevation prompt for the prerequisite.
    if IsAdminInstallMode then
      ExecSuccess := Exec(InstallerPath, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
    else
      ExecSuccess := ShellExec('runas', InstallerPath, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);

    if not ExecSuccess then
    begin
      if ResultCode = 1223 then
        Result := 'Administrator privileges are required to install the Microsoft .NET 8 Desktop Runtime prerequisite.'
      else
        Result := 'Failed to launch .NET 8 Desktop Runtime installer: ' + SysErrorMessage(ResultCode);
      Exit;
    end;

    // 0 = Success, 3010 = Success (Reboot Required)
    if (ResultCode <> 0) and (ResultCode <> 3010) then
    begin
      Result := 'Microsoft .NET 8 Desktop Runtime installation failed with code: ' + IntToStr(ResultCode) +
                '.'#13#10'Please install it manually from https://dotnet.microsoft.com/download/dotnet/8.0';
      Exit;
    end;

    if ResultCode = 3010 then
      NeedsRestart := True;

    Log('.NET 8 Desktop Runtime installation completed successfully.');
  end;
end;
