; NucleusRHHF.nsi
; Instalable de Todos los componentes Asociados con Nomad

;------------------------------------------------------------------------------------------------

!include "WordFunc.nsh"

;------------------------------------------------------------------------------------------------

Name "Nucleus RH - Actualización"
Caption "Nucleus RH - Actualización"
Icon "install.ico"
OutFile "NucleusRHHF.exe"

SetDateSave on
SetDatablockOptimize on
CRCCheck on
SilentInstall normal
;BGGradient 000000 0080A0 FFFFFF
InstallColors A0A0A0 000030
XPStyle on


InstallDir "C:\NomadServer"
InstallDirRegKey HKLM "Software\NucleusRH" ""

CheckBitmap "modern.bmp"

LoadLanguageFile "${NSISDIR}\Contrib\Language files\Spanish.nlf"
;AddBrandingImage left 70

;------------------------------------------------------------------------------------------------

Page components 
Page directory  
Page instfiles  

;------------------------------------------------------------------------------------------------

AutoCloseWindow false
ShowInstDetails hide
;------------------------------------------------------------------------------------------------
var APP_GROUP
var APP_NAME
var APP_LABEL
var APP_SKIN
var APP_CLIENT
var APP_NAME_ADD
var APP_LABEL_ADD

var ActVer


;------------------------------------------------------------------------------------------------
var a1


;------------------------------------------------------------------------------------------------
Section ""
	;Eliminando Archivo de los Viejos
	Delete $INSTDIR\Install.Log
	Delete $INSTDIR\Install_2.Log
	LogSet on

	;Leyendo la Version de Instalacion
	ReadRegStr $ActVer  HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall" "VERSION"
	StrCmp $ActVer "" SetActVer NoSetActVer
	SetActVer:
		StrCpy $ActVer 0
	NoSetActVer:
	IntOp $ActVer $ActVer + 1
	WriteRegStr HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall" "VERSION" $ActVer

	;Carpeta de instalacion
	WriteRegStr HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NomadInstall" "PATH" $INSTDIR

	SetOutPath $INSTDIR\Temp
	File "StopService.vbs"
	File "StartService.vbs"
	File "NomadWait.exe"
	
	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	LEER VARIABLES DESDE LA REGISTRY --------"
	ReadRegStr $APP_GROUP  HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\APP" "GROUP"
	ReadRegStr $APP_NAME   HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\APP" "NAME"
	ReadRegStr $APP_LABEL  HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\APP" "LABEL"
	ReadRegStr $APP_SKIN   HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\APP" "SKIN"
	ReadRegStr $APP_CLIENT HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\APP" "CLIENT"
	
	LogText "----Aplicacion----"
	LogText "Grupo: $APP_GROUP"
	LogText "Nombre: $APP_NAME"
	LogText "Etiqueta: $APP_LABEL"
	LogText "Skin: $APP_SKIN"
	LogText "Cliente: $APP_CLIENT"
	
	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	Detener los Servicios --------"
	SetOutPath  $INSTDIR\Temp
	ExecWait "$SYSDIR\WScript.exe StopService.vbs"
	ExecWait "$INSTDIR\Temp\NomadWait.exe NomadADMIN.exe;NomadCONTEXT.exe;NomadTRACE.exe;NomadSTORE.exe;NomadDDO.exe;NomadENGINE.exe;NomadBATCH.exe;NomadBATCHEXE.exe"
SectionEnd


Section "Test"
	;----------------------------------------------------------------------------------------------------------------
	StrCpy $APP_NAME_ADD 'TEST'
	StrCpy $APP_LABEL_ADD 'Testing'
	
	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	Copia la Aplicacion --------"
	CreateDirectory "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD"
	CopyFiles "$EXEDIR\APP\*.*" "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD" 64000

	;----------------------------------------------------------------------------------------------------------------
	StrCpy $APP_NAME_ADD 'TEST'
	StrCpy $APP_LABEL_ADD 'Testing'

	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	Actualiza el Servidor --------"
	CopyFiles "$EXEDIR\SERVER\*.*" "$INSTDIR" 64000
SectionEnd

Section /o "Produccion"
	;----------------------------------------------------------------------------------------------------------------
	StrCpy $APP_NAME_ADD 'PROD'
	StrCpy $APP_LABEL_ADD 'Produccion'
	
	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	Copia la Aplicacion --------"
	CreateDirectory "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD"
	CopyFiles "$EXEDIR\APP\*.*" "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD" 64000
	
	;----------------------------------------------------------------------------------------------------------------
	StrCpy $APP_NAME_ADD 'PROD'
	StrCpy $APP_LABEL_ADD 'Produccion'

	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	Actualiza el Servidor --------"
	CopyFiles "$EXEDIR\SERVER\*.*" "$INSTDIR" 64000
	
SectionEnd




Function .onInit
	ReadRegStr $a1 HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NomadInstall" "PATH"
	StrCmp $a1 "" INGOREPATH
		StrCpy $INSTDIR $a1
	INGOREPATH:
FunctionEnd


;------------------------------------------------------------------------------------------------
Section ""
	;----------------------------------------------------------------------------------------------------------------
	LogText "--------	SERVICIOS INICIAR --------"

	;--------	Iniciar los Servicios --------
	SetOutPath  $INSTDIR\Temp
	ExecWait "$SYSDIR\WScript.exe StartService.vbs"
SectionEnd
