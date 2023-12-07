; -- setup.iss --
;
; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Sapphire FL Capture Software"
#define MyAppVersion "1.3.3.1129"
#define MyAppPublisher "Azure Biosystems, Inc."
#define MyCompany "Azure Biosystems"
#define MyAppURL "http://www.azurebiosystems.com/"
#define MyAppExeName "Azure.LaserScanner.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{A867CC8F-AB9E-4239-A45B-1240CCFE2F15}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf64}\{#MyCompany}\Sapphire FL
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=EULA.rtf
OutputBaseFilename=Sapphire_FL_setup
Compression=lzma
SolidCompression=yes
DisableWelcomePage=no
DisableDirPage=no
OutputDir=.
PrivilegesRequired=admin
WindowVisible=yes
WindowShowCaption=no

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
; Intel IPP
Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\*; DestDir: {app}\Ipp; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\em64t\ippcoreem64t-6.0.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\em64t\ippcvmx-6.0.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\em64t\ippimx-6.0.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\em64t\libiomp5md.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\ia32\ippcore-6.0.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\ia32\ippcvpx-6.0.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\ia32\ippipx-6.0.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Ipp\ia32\libiomp5md.dll; DestDir: {app}; Flags: ignoreversion
; AForge.NET Framework
Source: ..\Projects\Azure.LaserScanner\Build\Release\AForge.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\AForge.Imaging.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\AForge.Math.dll; DestDir: {app}; Flags: ignoreversion
; AvalonDock
Source: ..\Projects\Azure.LaserScanner\Build\Release\AvalonDock.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\AvalonDock.Themes.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\AvalonDock.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\AvalonDockMVVM.dll; DestDir: {app}; Flags: ignoreversion
; Azure sapphire capture application
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.Adorners.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.CommandLib.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.Common.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.CommunicationLib.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.EthernetCommLib.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.Image.Processing.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.ImagingSystem.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.Interfaces.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.IppImaging.NET.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.LaserScanner.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.LaserScanner.exe.config; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.MotionLib.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.Resources.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.SettingsManager.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.Utilities.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.UpdateConfig.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Azure.WPF.Framework.dll; DestDir: {app}; Flags: ignoreversion
; Copy config.xml to dest app folder and temp folder (to be edit base on installation options selected).
Source: ..\Projects\Azure.LaserScanner\Build\Release\config.xml; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\config.xml; DestDir: {tmp}; Flags: dontcopy
; rename existing config.xml ProgramData\Azure Biosystems\Sapphire\ to config.old
; Source: "{commonappdata}\Azure Biosystems\Sapphire\config.xml"; DestDir: "{commonappdata}\Azure Biosystems\Sapphire"; DestName: "config.old"; Flags: external skipifsourcedoesntexist
Source: ..\Projects\Azure.LaserScanner\Build\Release\config.xml; DestDir: "{commonappdata}\Azure Biosystems\Sapphire FL"; Flags: ignoreversion onlyifdestfileexists uninsneveruninstall
Source: ..\Projects\Azure.LaserScanner\Build\Release\CroppingImageLibrary.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\CyUSB.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\DrawToolsLib.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\DynamicDataDisplay.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\DynamicDataDisplay.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Gif.Components.dll; DestDir: {app}; Flags: ignoreversion
; Material Design
Source: ..\Projects\Azure.LaserScanner\Build\Release\MaterialDesignColors.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\MaterialDesignThemes.Wpf.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Microsoft.Xaml.Behaviors.dll; DestDir: {app}; Flags: ignoreversion
; OpenCvSharp
Source: ..\Projects\Azure.LaserScanner\Build\Release\OpenCvSharp.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\OpenCvSharp.WpfExtensions.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\dll\x64\OpenCvSharpExtern.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Protocols.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Sapphire_FL_User_Manual.pdf; DestDir: {app}\UserManual; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Secure.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\SimpleWifi.dll; DestDir: {app}; Flags: ignoreversion
; Copy SysSettings.xml to dest app folder and temp folder (to be edit base on installation options selected).
Source: ..\Projects\Azure.LaserScanner\Build\Release\SysSettings.xml; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\SysSettings.xml; DestDir: {tmp}; Flags: dontcopy
;Source: ..\Projects\Azure.LaserScanner\Build\Release\System.Windows.Interactivity.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\TaskDialog.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\TaskDialog.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\ToggleSwitch.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.LaserScanner\Build\Release\Utilities.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\WPFFolderBrowser.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\Xceed.Wpf.Toolkit.dll; DestDir: {app}; Flags: ignoreversion
; Animated GIF
Source: ..\Projects\Azure.LaserScanner\Build\Release\AnimatedGif.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.LaserScanner\Build\Release\WpfAnimatedGif.dll; DestDir: {app}; Flags: ignoreversion

;Source: Drivers\*; DestDir: {app}\Drivers; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: Simulation\*; DestDir: {app}\Simulation; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: Utilities\*; DestDir: {app}\Utilities; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

; EUI
Source: ..\Projects\Azure.ScannerEUI\Build\Release\Azure.ScannerEUI.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.ScannerEUI\Build\Release\Azure.ScannerEUI.exe.config; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.ScannerEUI\Build\Release\EUIAuthen.xml; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.ScannerEUI\Build\Release\log4net.config; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.ScannerEUI\Build\Release\log4net.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.ScannerEUI\Build\Release\LogW.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.ScannerEUI\Build\Release\OpenCvSharp.dll; DestDir: {app}; Flags: ignoreversion
;Source: ..\Projects\Azure.ScannerEUI\Build\Release\OpenCvSharp.Extensions.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\Projects\Azure.ScannerEUI\Build\Release\RotatProcess.dll; DestDir: {app}; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}
Name: {commondesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
;Name: {commonstartup}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: quicklaunchicon

[Registry]
;Root: HKLM; Subkey: "SOFTWARE\Microsoft\TabletTip\1.7"; ValueType: dword; ValueName: "EnableDesktopModeAutoInvoke"; ValueData: "00000001"

[Run]
;Filename: {app}\Drivers\CP210x_Windows_Drivers\CP210xVCPInstaller_x64.exe; Check: IsWin64; Flags: waituntilterminated; Tasks: ; Languages: 
;Filename: {app}\Drivers\Cypress_FX3\Win10\x64\DPInst.exe; Check: IsWin64; Flags: waituntilterminated;
;Filename: {app}\Drivers\GalilTools-1.6.4.580-Win-x64.exe; Flags: waituntilterminated;
;Filename: {app}\Drivers\vcredist\vc_redist.x86.exe; Parameters: "/q /norestart"; Check: not IsWin64; Flags: waituntilterminated; Tasks: ; Languages: 
;Filename: {app}\Drivers\vcredist\vc_redist.x64.exe; Parameters: "/q /norestart"; Check: IsWin64; Flags: waituntilterminated; Tasks: ; Languages: 
; Recommended driver for QI815 & QI695
;Filename: {app}\Drivers\PVCam_3.9.0.4-PMQI_Release_Setup.exe; Check: IsChemiSelected; Flags: waituntilterminated;
;Filename: {commonpf64}\Photometrics\PVCam\utilities\PowerStates\PowerStatesCLI.exe; Parameters: 0; Flags: waituntilterminated skipifdoesntexist;
;Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}; Flags: nowait postinstall skipifsilent

[Code]
//var
  //InstallationType: string;
  //InstallationModule: string;
  //InstallationTypePage: TInputOptionWizardPage;
  //ModuleSelectionPage: TInputOptionWizardPage;

procedure RenameConfigXmlFile;
var
  ConfigXmlFile: string;
  ConfigXmlFileRenamed: string;
  //SysSettingsXmlFile: string;
  //SysSettingsXmlFileRenamed: string;
begin
  ConfigXmlFile := ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire FL\config.xml');
  ConfigXmlFileRenamed :=  ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire FL\config.old');
  //SysSettingsXmlFile := ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire FL\SysSettings.xml');
  //SysSettingsXmlFileRenamed :=  ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire FL\SysSettings.old');

  // delete config.old if exists
  if FileExists(ConfigXmlFileRenamed) then
  begin
    DeleteFile(ConfigXmlFileRenamed);
  end;
  // rename config.xml to config.old
  if FileExists(ConfigXmlFile) then
  begin
    //RenameFile(ConfigXmlFile, ConfigXmlFileRenamed);
    FileCopy(ConfigXmlFile, ConfigXmlFileRenamed, false);
  end;

  // delete SysSettings.old if exists
  //if FileExists(SysSettingsXmlFileRenamed) then
  //begin
  //  DeleteFile(SysSettingsXmlFileRenamed);
  //end;
  // rename SysSettings.xml to SysSettings.old
  //if FileExists(SysSettingsXmlFile) then
  //begin
  //  FileCopy(SysSettingsXmlFile, SysSettingsXmlFileRenamed, false);
  //end;
end;

function InitializeSetup: Boolean;
Begin
  RenameConfigXmlFile();
  Result := True;
end;

{procedure InitializeWizard;
begin
  InstallationType := '';
  InstallationModule := '';

  InstallationTypePage := CreateInputOptionPage(wpWelcome,
    'Sapphire image capture software installation options', 'Select your installation option',
    'Please select the installation option, then click Next.',
    True, False);
  // Add items
  InstallationTypePage.Add('RGB');
  InstallationTypePage.Add('RGBNIR');
  InstallationTypePage.Add('NIR');
  InstallationTypePage.Add('NIR-Q');
  InstallationTypePage.Add('PHOSPHOR IMAGING (ONLY)');
  // Set initial value
  InstallationTypePage.SelectedValueIndex := 0;

  // Create the Sapphire module selection page
  ModuleSelectionPage := CreateInputOptionPage(InstallationTypePage.ID,
    'Sapphire image capture software installation options',
    'Select your Sapphire module',
    'Please select a module, then click Next.',
    True, False);
  ModuleSelectionPage.Add('Chemi');
  ModuleSelectionPage.Add('None');

  // Set default selection
  ModuleSelectionPage.SelectedValueIndex := 1;
end;}

{procedure CurPageChanged(CurPageID: Integer);
//var
  //DefaultInstallPath: String;
begin
  if CurPageID = ModuleSelectionPage.ID then
  begin
    case InstallationTypePage.SelectedValueIndex of
      0: begin
           InstallationType := 'RGB';
         end;
      1: begin
           InstallationType := 'RGBNIR';
         end;
      2: begin
           InstallationType := 'NIR';
         end;
      3: begin
           InstallationType := 'NIR-Q';
         end;
      4: begin
           InstallationType := 'PHOSPHOR-IMAGING';
         end;
    end;
  end else if CurPageID = wpLicense then
  begin
    case ModuleSelectionPage.SelectedValueIndex of
      0: begin
           InstallationModule := 'Chemi';
         end;
      1: begin
           InstallationModule := 'None';
         end;

    end;
    // update the config.xml in the temp folder
    //UpdateConfigXml(InstallationType, InstallationModule);
    //WizardForm.DirEdit.Text := DefaultInstallPath;
  end
  else if CurPageID = wpFinished then
  begin
    //UpdateConfigXml(InstallationType, InstallationModule);

    // Copy the modified config file to the program folder
    //if (FileExists(ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire\config.xml'))) then
    //begin
    //  FileCopy(ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire\config.xml'), ExpandConstant('{app}\config.xml'), false);
    //end
    //else begin
    //  FileCopy(ExpandConstant('{tmp}\config.xml'), ExpandConstant('{app}\config.xml'), false);
    //end
    // Copy the modified syssettings file to the program folder
    //if (FileExists(ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire\SysSettings.xml'))) then
    //begin
    //  FileCopy(ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire\SysSettings.xml'), ExpandConstant('{app}\SysSettings.xml'), false);
    //end
    //else begin
    //  FileCopy(ExpandConstant('{commonappdata}\Azure Biosystems\Sapphire\SysSettings.xml'), ExpandConstant('{app}\SysSettings.xml'), false);
    //end
  end;
end;}

{function IsChemiSelected: Boolean;
Begin
  if InstallationModule = 'Chemi' then
  begin
    Result := True;
  end  
  else
  begin 
    Result := False;
  end;
end;}
