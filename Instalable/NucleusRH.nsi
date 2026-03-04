; Nomad.nsi
; Instalable de Todos los componentes Asociados con Nomad

;------------------------------------------------------------------------------------------------

!include "WordFunc.nsh"
!insertmacro WordReplace
!insertmacro WordFind

;------------------------------------------------------------------------------------------------

Name "Nucleus RH"
Caption "Nucleus RH"
Icon "install.ico"
OutFile "NucleusRH.exe"

SetDateSave on
SetDatablockOptimize on
CRCCheck on
SilentInstall normal
BGGradient 000000 0080A0 FFFFFF
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

var DB_PROVIDER
var DB_SERVER
var DB_NAME
var DB_USER
var DB_PASS

var ActVer


;------------------------------------------------------------------------------------------------
var fr 
var fa
var ln
var fw
var ret

var a1
var a2
var a3

function InternalDeleteFile
	pop $a2
	pop $a1
	
	FileOpen $fr $a1 "r"
	FileOpen $fw $INSTDIR\temp.dat "w"
	
	READLINE:
		FileRead $fr $ln
		StrCmp $ln "" CLOSEDFILES

		Push $ln 
		Push "START:$a2"
		Push "*" 
		Call WordFind
		Pop $ret
		
		StrCmp $ret "1" INGOREBLOCK
		FileWrite $fw $ln
	Goto READLINE
	
	INGOREBLOCK:
		FileRead $fr $ln
		StrCmp $ln "" CLOSEDFILES

		Push $ln 
		Push "END:$a2"
		Push "*" 
		Call WordFind
		Pop $ret
		
		StrCmp $ret "1" READLINE
	Goto INGOREBLOCK

	CLOSEDFILES:
	
	FileClose $fr
	FileClose $fw
	
	delete $a1
	rename $INSTDIR\temp.dat $a1
FunctionEnd

function InternalMergeFiles
	pop $a3
	pop $a2
	pop $a1
	
	FileOpen $fr $a1 "r"
	FileOpen $fw $INSTDIR\temp.dat "w"
	
	READLINE:
		FileRead $fr $ln
		StrCmp $ln "" CLOSEDFILES

		Push $ln 
		Push $a3 
		Push "*" 
		Call WordFind
		Pop $ret
		
		StrCmp $ret "1" WRITECHILD
		FileWrite $fw $ln
	Goto READLINE
	WRITECHILD:

		FileOpen $fa $a2 "r"
		READCHILD:
			FileRead $fa $ln
			StrCmp $ln "" CLOSEDCHILD
			FileWrite $fw $ln
		Goto READCHILD
		CLOSEDCHILD:		
		FileClose $fa
		
	Goto READLINE
	CLOSEDFILES:
	
	FileClose $fr
	FileClose $fw
	
	delete $a1
	rename $INSTDIR\temp.dat $a1
FunctionEnd


function InternalReplaceFile
	pop $a3
	pop $a2
	pop $a1
	
	FileOpen $fr $a1 "r"
	FileOpen $fw $INSTDIR\temp.dat "w"
	
	READLINE:
		FileRead $fr $ln
		StrCmp $ln "" CLOSEDFILES

		Push $ln
		Push $a2
		Push $a3
		Push "+"
		Call WordReplace
		Pop $ret
		
		FileWrite $fw $ret
	Goto READLINE
	CLOSEDFILES:
	
	FileClose $fr
	FileClose $fw
	
	delete $a1
	rename $INSTDIR\temp.dat $a1
FunctionEnd



!macro DeleteFile filesource find
	Push ${filesource}
	Push ${find}
	Call InternalDeleteFile
!macroend

!macro ReplaceFile filesource find replace
	Push ${filesource}
	Push ${find}
	Push ${replace}
	Call InternalReplaceFile
!macroend

!macro ConfigureFile filesource inst label
	!insertmacro ReplaceFile ${filesource} "(APP-GROUP)" "$APP_GROUP"
	!insertmacro ReplaceFile ${filesource} "(APP-ID)"    "$APP_NAME-${inst}"
	!insertmacro ReplaceFile ${filesource} "(APP-LABEL)" "$APP_LABEL-${label}"
	!insertmacro ReplaceFile ${filesource} "(APP-SKIN)"  "$APP_SKIN"
	
	!insertmacro ReplaceFile ${filesource} "(DATABASE-PROVIDER)" $DB_PROVIDER
	!insertmacro ReplaceFile ${filesource} "(DATABASE-SERVER)"   $DB_SERVER
	!insertmacro ReplaceFile ${filesource} "(DATABASE-NAME)"     $DB_NAME
	!insertmacro ReplaceFile ${filesource} "(DATABASE-USER)"     $DB_USER
	!insertmacro ReplaceFile ${filesource} "(DATABASE-PASSWORD)" $DB_PASS
!macroend

!macro MergeFiles filesource fileappend filetoken
	Push ${filesource}
	Push ${fileappend}
	Push ${filetoken}
	Call InternalMergeFiles
!macroend



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
	File "NucleusRHInstallParams.exe"
	File "StopService.vbs"
	File "StartService.vbs"
	File "NomadWait.exe"
	ExecWait "$INSTDIR\Temp\NucleusRHInstallParams.exe"
	
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


SubSection "Test/Desarrollo"
	Section "Actualizar Archivos de Aplicacion"
		LogSet off
		StrCmp $APP_CLIENT '1' TEST DESA
		DESA:
			StrCpy $APP_NAME_ADD 'DESA'
			StrCpy $APP_LABEL_ADD 'Desarrollo'
			Goto FIN
		TEST:
			StrCpy $APP_NAME_ADD 'TEST'
			StrCpy $APP_LABEL_ADD 'Testing'
		FIN:
		LogSet on
	
		;----------------------------------------------------------------------------------------------------------------
		LogText "--------	Copia la Aplicacion --------"
		CreateDirectory "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD"
		CopyFiles "$EXEDIR\APP\*.*" "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD" 64000
	SectionEnd
	
	Section "Actualizar Archivos de Servidor"
		LogSet off
		StrCmp $APP_CLIENT '1' TEST DESA
		DESA:
			StrCpy $APP_NAME_ADD 'DESA'
			StrCpy $APP_LABEL_ADD 'Desarrollo'
			Goto FIN
		TEST:
			StrCpy $APP_NAME_ADD 'TEST'
			StrCpy $APP_LABEL_ADD 'Testing'
		FIN:
		LogSet on

		;----------------------------------------------------------------------------------------------------------------
		LogText "--------	Actualiza el Servidor --------"
		CopyFiles "$EXEDIR\SERVER\*.*" "$INSTDIR" 64000
	SectionEnd

	Section /o "Agregar Aplicacion"
		LogSet off
		StrCmp $APP_CLIENT '1' TEST DESA
		DESA:
			StrCpy $APP_NAME_ADD 'DESA'
			StrCpy $APP_LABEL_ADD 'Desarrollo'
			Goto FIN
		TEST:
			StrCpy $APP_NAME_ADD 'TEST'
			StrCpy $APP_LABEL_ADD 'Testing'
		FIN:
		LogSet on

		;----------------------------------------------------------------------------------------------------------------
		LogText "--------	eliminar aplicacion (si ya existe) --------"
		LogSet off
		!insertmacro DeleteFile "$INSTDIR\Config\Context.cfg.xml"                "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\APP\Config\ApplicationsConfig.cfg.xml" "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\SERVICES\COMMON\DB.obj.xml"            "$APP_NAME-$APP_NAME_ADD"
		LogSet on
	
		LogText "--------	LEER VARIABLES DESDE LA REGISTRY --------"
		ReadRegStr $DB_PROVIDER   HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB" "PROVIDER"
		ReadRegStr $DB_SERVER     HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB" "SERVER"
		ReadRegStr $DB_NAME       HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB" "NAME"
		ReadRegStr $DB_USER       HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB" "USER"
		ReadRegStr $DB_PASS       HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB" "PASS"

		LogText "----Base de Datos----"
		LogText "Proveedor: $DB_PROVIDER"
		LogText "Servidor: $DB_SERVER"
		LogText "Nombre: $DB_NAME"
		LogText "Usuario: $DB_USER"
		LogText "Password: $DB_PASS"

		LogText "--------	agregar aplicacion --------"
		CopyFiles /SILENT /FILESONLY "$INSTDIR\Config\NewAPP.cfg.xml" "$INSTDIR\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml"
		LogSet off
		!insertmacro ConfigureFile "$INSTDIR\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" $APP_NAME_ADD $APP_LABEL_ADD
		!insertmacro MergeFiles "$INSTDIR\Config\Context.cfg.xml" "$INSTDIR\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" "(NEW-APP-INFO)"
		LogSet on
		
		CopyFiles /SILENT /FILESONLY "$INSTDIR\APP\Config\ApplicationNew.cfg.xml" "$INSTDIR\APP\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml"
		LogSet off
		!insertmacro ConfigureFile "$INSTDIR\APP\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" $APP_NAME_ADD $APP_LABEL_ADD
		!insertmacro MergeFiles "$INSTDIR\APP\Config\ApplicationsConfig.cfg.xml" "$INSTDIR\APP\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" "(NEW-APP-DATA)"
		LogSet on
		
		CopyFiles /SILENT /FILESONLY "$INSTDIR\SERVICES\COMMON\DB.new.xml" "$INSTDIR\SERVICES\COMMON\$APP_NAME-$APP_NAME_ADD.new.xml"
		LogSet off
		!insertmacro ConfigureFile "$INSTDIR\SERVICES\COMMON\$APP_NAME-$APP_NAME_ADD.new.xml" $APP_NAME_ADD $APP_LABEL_ADD
		!insertmacro MergeFiles "$INSTDIR\SERVICES\COMMON\DB.obj.xml" "$INSTDIR\SERVICES\COMMON\$APP_NAME-$APP_NAME_ADD.new.xml" "(NEW-DATABASE-INFO)"
		LogSet on
	SectionEnd
	
	Section /o "Eliminar Aplicacion"
		LogSet off
		StrCmp $APP_CLIENT '1' TEST DESA
		DESA:
			StrCpy $APP_NAME_ADD 'DESA'
			StrCpy $APP_LABEL_ADD 'Desarrollo'
			Goto FIN
		TEST:
			StrCpy $APP_NAME_ADD 'TEST'
			StrCpy $APP_LABEL_ADD 'Testing'
		FIN:
		LogSet on

		LogText "--------	eliminar aplicacion (si existe) --------"
		LogSet off
		!insertmacro DeleteFile "$INSTDIR\Config\Context.cfg.xml"                "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\APP\Config\ApplicationsConfig.cfg.xml" "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\SERVICES\COMMON\DB.obj.xml"            "$APP_NAME-$APP_NAME_ADD"
		LogSet on
	SectionEnd
SubSectionEnd

SubSection "Produccion/Integracion"
	Section /o "Actualizar Archivos de Aplicacion"
		LogSet off
		StrCmp $APP_CLIENT '1' PROD INTEG
		INTEG:
			StrCpy $APP_NAME_ADD 'INTEG'
			StrCpy $APP_LABEL_ADD 'Integracion'
			Goto FIN
		PROD:
			StrCpy $APP_NAME_ADD 'PROD'
			StrCpy $APP_LABEL_ADD 'Produccion'
		FIN:
		LogSet on
	
		;----------------------------------------------------------------------------------------------------------------
		LogText "--------	Copia la Aplicacion --------"
		CreateDirectory "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD"
		CopyFiles "$EXEDIR\APP\*.*" "$INSTDIR\App\$APP_NAME-$APP_NAME_ADD" 64000
	SectionEnd
	
	Section /o "Actualizar Archivos de Servidor"
		LogSet off
		StrCmp $APP_CLIENT '1' PROD INTEG
		INTEG:
			StrCpy $APP_NAME_ADD 'INTEG'
			StrCpy $APP_LABEL_ADD 'Integracion'
			Goto FIN
		PROD:
			StrCpy $APP_NAME_ADD 'PROD'
			StrCpy $APP_LABEL_ADD 'Produccion'
		FIN:
		LogSet on

		;----------------------------------------------------------------------------------------------------------------
		LogText "--------	Actualiza el Servidor --------"
		CopyFiles "$EXEDIR\SERVER\*.*" "$INSTDIR" 64000
	SectionEnd
	
	Section /o "Agregar Aplicacion"
		LogSet off
		StrCmp $APP_CLIENT '1' PROD INTEG
		INTEG:
			StrCpy $APP_NAME_ADD 'INTEG'
			StrCpy $APP_LABEL_ADD 'Integracion'
			Goto FIN
		PROD:
			StrCpy $APP_NAME_ADD 'PROD'
			StrCpy $APP_LABEL_ADD 'Produccion'
		FIN:
		LogSet on

		;----------------------------------------------------------------------------------------------------------------
		LogText "--------	eliminar aplicacion (si ya existe) --------"
		LogSet off
		!insertmacro DeleteFile "$INSTDIR\Config\Context.cfg.xml"                "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\APP\Config\ApplicationsConfig.cfg.xml" "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\SERVICES\COMMON\DB.obj.xml"            "$APP_NAME-$APP_NAME_ADD"
		LogSet on
	
		LogText "--------	LEER VARIABLES DESDE LA REGISTRY --------"
		ReadRegStr $DB_PROVIDER   HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB2" "PROVIDER"
		ReadRegStr $DB_SERVER     HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB2" "SERVER"
		ReadRegStr $DB_NAME       HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB2" "NAME"
		ReadRegStr $DB_USER       HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB2" "USER"
		ReadRegStr $DB_PASS       HKEY_LOCAL_MACHINE "SOFTWARE\NUCLEUS\NucleusRHInstall\DB2" "PASS"

		LogText "----Base de Datos----"
		LogText "Proveedor: $DB_PROVIDER"
		LogText "Servidor: $DB_SERVER"
		LogText "Nombre: $DB_NAME"
		LogText "Usuario: $DB_USER"
		LogText "Password: $DB_PASS"

		LogText "--------	agregar aplicacion --------"
		CopyFiles /SILENT /FILESONLY "$INSTDIR\Config\NewAPP.cfg.xml" "$INSTDIR\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml"
		LogSet off
		!insertmacro ConfigureFile "$INSTDIR\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" $APP_NAME_ADD $APP_LABEL_ADD
		!insertmacro MergeFiles "$INSTDIR\Config\Context.cfg.xml" "$INSTDIR\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" "(NEW-APP-INFO)"
		LogSet on
		
		CopyFiles /SILENT /FILESONLY "$INSTDIR\APP\Config\ApplicationNew.cfg.xml" "$INSTDIR\APP\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml"
		LogSet off
		!insertmacro ConfigureFile "$INSTDIR\APP\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" $APP_NAME_ADD $APP_LABEL_ADD
		!insertmacro MergeFiles "$INSTDIR\APP\Config\ApplicationsConfig.cfg.xml" "$INSTDIR\APP\Config\$APP_NAME-$APP_NAME_ADD.cfg.xml" "(NEW-APP-DATA)"
		LogSet on
		
		CopyFiles /SILENT /FILESONLY "$INSTDIR\SERVICES\COMMON\DB.new.xml" "$INSTDIR\SERVICES\COMMON\$APP_NAME-$APP_NAME_ADD.new.xml"
		LogSet off
		!insertmacro ConfigureFile "$INSTDIR\SERVICES\COMMON\$APP_NAME-$APP_NAME_ADD.new.xml" $APP_NAME_ADD $APP_LABEL_ADD
		!insertmacro MergeFiles "$INSTDIR\SERVICES\COMMON\DB.obj.xml" "$INSTDIR\SERVICES\COMMON\$APP_NAME-$APP_NAME_ADD.new.xml" "(NEW-DATABASE-INFO)"
		LogSet on
	SectionEnd
	
	Section /o "Eliminar Aplicacion"
		LogSet off
		StrCmp $APP_CLIENT '1' PROD INTEG
		INTEG:
			StrCpy $APP_NAME_ADD 'INTEG'
			StrCpy $APP_LABEL_ADD 'Integracion'
			Goto FIN
		PROD:
			StrCpy $APP_NAME_ADD 'PROD'
			StrCpy $APP_LABEL_ADD 'Produccion'
		FIN:
		LogSet on

		LogText "--------	eliminar aplicacion (si existe) --------"
		LogSet off
		!insertmacro DeleteFile "$INSTDIR\Config\Context.cfg.xml"                "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\APP\Config\ApplicationsConfig.cfg.xml" "$APP_NAME-$APP_NAME_ADD"
		!insertmacro DeleteFile "$INSTDIR\SERVICES\COMMON\DB.obj.xml"            "$APP_NAME-$APP_NAME_ADD"
		LogSet on
	SectionEnd
SubSectionEnd




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
