#define MyAppName "Cold Turkey"
#define MyAppVersion "0.9"
#define MyAppPublisher "Felix Belzile"
#define MyAppURL "http://www.getcoldturkey.com/"
#define MyAppExeName "Cold Turkey.exe"

[Setup]
AppId={{6498E673-B9C2-4544-A722-1E854B5B573E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableDirPage=yes
UsePreviousAppDir=no
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=C:\Users\Felix\Desktop\Setup_Files
OutputBaseFilename=Cold Turkey Setup 0.9
SetupIconFile=C:\Users\Felix\Desktop\Setup_Files\ct.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "C:\Program Files\Cold Turkey\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\Felix\Desktop\Setup_Files\dotNetFx40_Client_setup.exe"; DestDir: {tmp}; Check: FrameworkIsNotInstalled

[InstallDelete]
Name: {pf}\ColdTurkey; Type: filesandordirs

[Run]
Filename: "{tmp}\dotNetFx40_Client_setup.exe"; Flags: runascurrentuser; Check: FrameworkIsNotInstalledMessage
Filename: "{app}\CTInstallService.exe"; Flags: nowait runascurrentuser

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKLM; Subkey: "Software\Policies";
Root: HKLM; Subkey: "Software\Policies\Microsoft";
Root: HKLM; Subkey: "Software\Policies\Microsoft\Windows Defender"; 
Root: HKLM; Subkey: "Software\Policies\Microsoft\Windows Defender\Exclusions"; 
Root: HKLM; Subkey: "Software\Policies\Microsoft\Windows Defender\Exclusions\Paths"; ValueType: dword; ValueName: "C:\Windows\system32\drivers\etc\hosts"; ValueData: "0"; Flags: uninsdeletekey

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function CreateCTConfigServer(): boolean;
begin
  SaveStringToFile(ExpandConstant('{pf}\Cold Turkey\CTConfig.conf'), 'cgi_interpreter ' + ExpandConstant('{pf}\Cold Turkey') + '\php-cgi.exe' + #13#10 + 'listening_ports 1990' + #13#10 + 'document_root ' + ExpandConstant('{pf}\Cold Turkey') + '\CTConfig', False);
  exit;
end;

type
	SERVICE_STATUS = record
    	dwServiceType				: cardinal;
    	dwCurrentState			: cardinal;
    	dwControlsAccepted	: cardinal;
    	dwWin32ExitCode			: cardinal;
    	dwServiceSpecificExitCode	: cardinal;
    	dwCheckPoint				: cardinal;
    	dwWaitHint					: cardinal;
	end;
	HANDLE = cardinal;

const
	SC_MANAGER_ALL_ACCESS		= $f003f;
  SERVICE_DELETE          = $10000;

function OpenSCManager(lpMachineName, lpDatabaseName: string; dwDesiredAccess :cardinal): HANDLE;
external 'OpenSCManagerA@advapi32.dll stdcall';

function OpenService(hSCManager :HANDLE;lpServiceName: string; dwDesiredAccess :cardinal): HANDLE;
external 'OpenServiceA@advapi32.dll stdcall';

function CloseServiceHandle(hSCObject :HANDLE): boolean;
external 'CloseServiceHandle@advapi32.dll stdcall';

function DeleteService(hService :HANDLE): boolean;
external 'DeleteService@advapi32.dll stdcall';

function OpenServiceManager() : HANDLE;
begin
	if UsingWinNT() = true then begin
		Result := OpenSCManager('','ServicesActive',SC_MANAGER_ALL_ACCESS);
		if Result = 0 then
			MsgBox('The servicemanager is not available', mbError, MB_OK)
	end
	else begin
			MsgBox('Only nt based systems support services', mbError, MB_OK)
			Result := 0;
	end
end;

function RemoveService(ServiceName: string) : boolean;
var
	hSCM	: HANDLE;
	hService: HANDLE;
begin
	hSCM := OpenServiceManager();
	Result := false;
	if hSCM <> 0 then begin
		hService := OpenService(hSCM,ServiceName,SERVICE_DELETE);
        if hService <> 0 then begin
            Result := DeleteService(hService);
            CloseServiceHandle(hService)
		end
        CloseServiceHandle(hSCM)
	end
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  LineCount: Integer;
  SectionLine: Integer;    
  Lines: TArrayOfString;
  ErrorCode: Integer;
begin
  case CurUninstallStep of
  usAppMutexCheck:
    begin
      if LoadStringsFromFile(ExpandConstant('{sys}\drivers\etc\hosts'), Lines) then
      begin
        LineCount := GetArrayLength(Lines);
        for SectionLine := 0 to LineCount - 1 do
        begin
          if CompareText('## Cold Turkey Entries ##', Lines[SectionLine]) = 0 then begin
            MsgBox('Sorry, you can not uninstall Cold Turkey while you are being blocked.', mbError, MB_OK);
            Abort();
          end;
        end;
        ShellExec('open', 'taskkill.exe', '/f /im CTService.exe','',SW_HIDE,ewNoWait,ErrorCode);
        ShellExec('open', 'taskkill.exe', '/f /im CTConfigServer.exe','',SW_HIDE,ewNoWait,ErrorCode);
        Sleep(1000);
      end;
    end;
  usPostUninstall:
    begin
      RemoveService('CTService');
    end;
  end;
end;
/////////////////////////////////////////////////////////////////////
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;


/////////////////////////////////////////////////////////////////////
function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;


/////////////////////////////////////////////////////////////////////
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

function InitializeSetup(): Boolean;
var
  LineCount: Integer;
  SectionLine: Integer;    
  Lines: TArrayOfString;
  ErrorCode: Integer;
begin
  if not LoadStringsFromFile(ExpandConstant('{sys}\drivers\etc\hosts'), Lines) then
  begin
    MsgBox('Sorry, you can not install Cold Turkey while you are being blocked by an older version. If no older version is installed, Cold Turkey might be conflicting with your antivirus software.', mbError, MB_OK);
    Result := False;
  end else begin
  ShellExec('open', 'taskkill.exe', '/f /im ct_notify.exe','',SW_HIDE,ewNoWait,ErrorCode);
  ShellExec('open', 'taskkill.exe', '/f /im ct_notify2.exe','',SW_HIDE,ewNoWait,ErrorCode);
  Sleep(1000);
  Result := True;
  end;
end;
/////////////////////////////////////////////////////////////////////
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
  if (CurStep=ssPostInstall) then
  begin
      CreateCTConfigServer();
  end;
end;

function FrameworkIsNotInstalled: Boolean;
begin
  Result := not RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\.NETFramework\policy\v4.0');
end;

function FrameworkIsNotInstalledMessage: Boolean;
var
  ShouldNotify: Boolean;
begin
  ShouldNotify := not RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\.NETFramework\policy\v4.0');
  if ShouldNotify then begin
    MsgBox('Setup will now install .NET verson 4.0. This version of the .NET framework is required in order for Cold Turkey to run.', mbInformation, MB_OK);
  end;
  Result := ShouldNotify;
end;

