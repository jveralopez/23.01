;// AppInstall.nsi
;//------------------------------------------------------------------------------------------------

Name "#APPNAME#"
Caption "#APPLABEL#"
Icon "install.ico"
OutFile "#FILENAME#.exe"

SetDateSave on
SetDatablockOptimize on
CRCCheck on
SilentInstall normal
BGGradient 000000 0080A0 FFFFFF
InstallColors A0A0A0 000030
XPStyle on

InstallDir "C:\NomadServer"
InstallDirRegKey HKLM "Software\NucleusRH" ""

LoadLanguageFile "${NSISDIR}\Contrib\Language files\Spanish.nlf"

;//------------------------------------------------------------------------------------------------
Page directory  
Page instfiles  

;//------------------------------------------------------------------------------------------------
var a1

;//------------------------------------------------------------------------------------------------
AutoCloseWindow false
ShowInstDetails hide

;//------------------------------------------------------------------------------------------------
Section ""
	SetOutPath  $INSTDIR\INSTALL\APP\#REVISION#
	File /r /x _svn /x .svn /x .GENDATA /x .DEPDATA ".\#APPPATH#\APP"
	
	SetOutPath  $INSTDIR\INSTALL\DB\#REVISION#
	File /r /x _svn /x .svn /x .GENDATA /x .DEPDATA ".\#APPPATH#\DB"

	;//Carpeta de instalacion
	WriteRegStr HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NomadInstall" "PATH" $INSTDIR
SectionEnd

Function .onInit
	ReadRegStr $a1 HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NomadInstall" "PATH"
	StrCmp $a1 "" INGOREPATH
		StrCpy $INSTDIR $a1
	INGOREPATH:
FunctionEnd
