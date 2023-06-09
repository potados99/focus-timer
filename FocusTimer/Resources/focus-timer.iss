; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
; Key generator = a=[4, 4, 4, 4, 4, 3].map(c => Array.from({length: c}, (_, i) => Math.ceil(Math.random() * 15))); a[a.length-1].push(a.reduce((acc, cur) => acc.concat(cur)).reduce((acc, cur) => acc + (cur ^ 3) + 1, 3) % 16); a.map(i => i.map(n => n.toString(16).toUpperCase()).join('')).join('-');
; Test key = 3BCE-B983-D4E3-7B56-E575-471E

#define MyAppName "Focus Timer"
#define MyAppVersion "1.0"
#define MyAppPublisher "World Moment"
#define MyAppURL "https://tumblbug.com/worldmoment_focus"
#define MyAppExeName "FocusTimer.exe"
#define MyAppSolutionPath "C:\Users\Administrator\source\repos\FocusTimer"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{77E6B305-BB81-49FA-A456-6E485C0B0B95}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={code:GetProgramFiles}\{#MyAppPublisher}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline
OutputDir={#MyAppSolutionPath}\FocusTimer\bin\Release\Publish
OutputBaseFilename=FocusTimer
SetupIconFile={#MyAppSolutionPath}\FocusTimer\Resources\icon_small.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "Korean"; MessagesFile: "{#MyAppSolutionPath}\FocusTimer\Resources\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "{#MyAppSolutionPath}\FocusTimer\bin\Release\net472\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function GetProgramFiles(Param: string): string;
begin
  if IsWin64 then Result := ExpandConstant('{commonpf64}')
    else Result := ExpandConstant('{commonpf32}')
end;

function SetFocus(hWnd: HWND): HWND;
  external 'SetFocus@user32.dll stdcall';
function OpenClipboard(hWndNewOwner: HWND): BOOL;
  external 'OpenClipboard@user32.dll stdcall';
function GetClipboardData(uFormat: UINT): THandle;
  external 'GetClipboardData@user32.dll stdcall';
function CloseClipboard: BOOL;
  external 'CloseClipboard@user32.dll stdcall';
function GlobalLock(hMem: THandle): PAnsiChar;
  external 'GlobalLock@kernel32.dll stdcall';
function GlobalUnlock(hMem: THandle): BOOL;
  external 'GlobalUnlock@kernel32.dll stdcall';

var
  SerialPage: TWizardPage;
  SerialEdits: array of TEdit;

const
  CF_TEXT = 1;
  VK_BACK = 8;
  SC_EDITCOUNT = 6;
  SC_CHARCOUNT = 4;
  SC_DELIMITER = '-';

function GetClipboardText: string;
var
  Data: THandle;
begin
  Result := '';
  if OpenClipboard(0) then
  try
    Data := GetClipboardData(CF_TEXT);
    if Data <> 0 then
      Result := String(GlobalLock(Data));
  finally
    if Data <> 0 then
      GlobalUnlock(Data);
    CloseClipboard;
  end;
end;

function GetSerialNumber: string;
var
  I: Integer;
begin
  Result := '';
  for I := 0 to GetArrayLength(SerialEdits) - 1 do
    Result := Result + SerialEdits[I].Text;
end;

function AllTyped: Boolean;
begin
  Result := True;
  
  if Length(GetSerialNumber()) < SC_EDITCOUNT * SC_CHARCOUNT then
  begin
    Result := False;
  end
end;

function IsHexNumber(C: Char): Boolean;
begin 
  Result := False;

  if (Ord(C) >= 48) and (Ord(C) <= 57) then
  begin
    Result := True;
  end;
end;

function IsHexAlphabet(C: Char): Boolean;
begin 
  Result := False;

  if (Ord(C) >= 65) and (Ord(C) <= 70) then
  begin
    Result := True;
  end;
end;

function IsHex(C: Char): Boolean;
begin
  Result := IsHexNumber(C) or IsHexAlphabet(C);
end;

function HexToInt(C: Char): Integer;
begin
  if IsHexNumber(C) then
  begin
    Result := Ord(C) - 48;
  end;

  if IsHexAlphabet(C) then
  begin
    Result := (Ord(C) - 65) + 10;
  end;
end;

function IntToHex(I: Integer): Char;
begin
  if (I < 10) then
  begin
    Result := Chr(48 + I);
  end;

  if (I >= 10) then
  begin
    Result := Chr(65 + (I - 10));
  end;
end;

function AllHex: Boolean;
var
  I: Integer;
  S: String;
  C: Char;
begin
  Result := True;
  S := GetSerialNumber();
   
  for I := 1 to Length(S) do
    C := Char(S[I]);
    if IsHex(C) = False then
    begin
      Result := False;
    end;
end;

function ValidateSerialNumber: Boolean;
var
  I: Integer;
  S: String;
  Sum: Integer;
begin
  Result := False;
  Sum := 3;
  S := GetSerialNumber();
  for I := 1 to Length(S) - 1 do
    Sum := Sum + (HexToInt(Char(S[I])) xor 3) + 1;

  if (Sum mod 16) = HexToInt(S[Length(S)]) then
  begin
    Result := True;
  end;
end;

function IsValidInput: Boolean;
begin
  Result := True;

  if (AllTyped() = False) or (AllHex() = False) or (ValidateSerialNumber() = False) then
  begin
    Result := False;
  end;
end;

function TrySetSerialNumber(const ASerialNumber: string; ADelimiter: Char): Boolean;
var
  I: Integer;
  J: Integer;
begin
  Result := False;

  if Length(ASerialNumber) = ((SC_EDITCOUNT * SC_CHARCOUNT) + (SC_EDITCOUNT - 1)) then
  begin
    for I := 1 to SC_EDITCOUNT - 1 do
      if ASerialNumber[(I * SC_CHARCOUNT) + I] <> ADelimiter then
        Exit;

    for I := 0 to GetArrayLength(SerialEdits) - 1 do
    begin
      J := (I * SC_CHARCOUNT) + I + 1;
      SerialEdits[I].Text := Copy(ASerialNumber, J, SC_CHARCOUNT);
    end;

    Result := True;
  end;
end;

function TryPasteSerialNumber: Boolean;
begin
  Result := TrySetSerialNumber(GetClipboardText, SC_DELIMITER);
end;

procedure OnSerialEditChange(Sender: TObject);
begin
  WizardForm.NextButton.Enabled := IsValidInput;
end;

procedure OnSerialEditKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
var
  Edit: TEdit;
  EditIndex: Integer;
begin
  Edit := TEdit(Sender);
  EditIndex := Edit.TabOrder - SerialEdits[0].TabOrder;
  if (EditIndex = 0) and (Key = Ord('V')) and (Shift = [ssCtrl]) then
  begin
    if TryPasteSerialNumber then
      Key := 0;
  end
  else
  if (Key >= 32) and (Key <= 255) then
  begin
    if Length(Edit.Text) = SC_CHARCOUNT - 1 then
    begin
      if EditIndex < GetArrayLength(SerialEdits) - 1 then
        SetFocus(SerialEdits[EditIndex + 1].Handle)
      else
        SetFocus(WizardForm.NextButton.Handle);
    end;
  end
  else
  if Key = VK_BACK then
    if (EditIndex > 0) and (Edit.Text = '') and (Edit.SelStart = 0) then
      SetFocus(SerialEdits[EditIndex - 1].Handle);
end;

procedure CreateSerialNumberPage;
var
  I: Integer;
  Edit: TEdit;
  DescLabel: TLabel;
  EditWidth: Integer;
begin
  SerialPage := CreateCustomPage(wpWelcome, '제품 키 입력',
    '제품에 포함된 시리얼 키를 입력해 주세요.');

  DescLabel := TLabel.Create(SerialPage);
  DescLabel.Top := 16;
  DescLabel.Left := 0;
  DescLabel.Parent := SerialPage.Surface;
  DescLabel.Caption := '설치를 계속하기 위해 제품 키를 입력해 주세요.';

  SetArrayLength(SerialEdits, SC_EDITCOUNT);
  EditWidth := (SerialPage.SurfaceWidth - ((SC_EDITCOUNT - 1) * 8)) div SC_EDITCOUNT;

  for I := 0 to SC_EDITCOUNT - 1 do
  begin
    Edit := TEdit.Create(SerialPage);
    Edit.Top := 40;
    Edit.Left := I * (EditWidth + 8);
    Edit.Width := EditWidth;
    Edit.CharCase := ecUpperCase;
    Edit.MaxLength := SC_CHARCOUNT;
    Edit.Parent := SerialPage.Surface;
    Edit.OnChange := @OnSerialEditChange;
    Edit.OnKeyDown := @OnSerialEditKeyDown;
    SerialEdits[I] := Edit;
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = SerialPage.ID then
    WizardForm.NextButton.Enabled := IsValidInput;  
end;

procedure InitializeWizard;
begin
  CreateSerialNumberPage;
end;