Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.ORG.ALTER_AK_PUESTOS', @n_Version = 1 , @d_modulo ='ORG'

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Comienza la ejecucion ',0 , ''


IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo  ,@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR El script ya corrio'
	return
END



IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG04_PUESTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='AK_AK1_ORG_PUESTOS_ORG04_PU' ) )
BEGIN
	alter table ORG04_PUESTOS
	drop constraint AK_AK1_ORG_PUESTOS_ORG04_PU

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' drop constraint AK_AK1_ORG_PUESTOS_ORG04_PU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG04_PUESTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='AK_AK1_ORG_PUESTOS_ORG04_PU' ) )
BEGIN
	alter table ORG04_PUESTOS
	add constraint AK_AK1_ORG_PUESTOS_ORG04_PU unique (c_puesto, oi_empresa)


	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD AK_AK1_ORG_PUESTOS_ORG04_PU' , @@error , ''
END





exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



