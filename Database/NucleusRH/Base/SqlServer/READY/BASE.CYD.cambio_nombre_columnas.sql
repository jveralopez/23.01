Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.CYP.cambio_nombre_columnas', @n_Version = 1 , @d_modulo ='CAPACITACION'

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 


select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo  ,@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR El script ya corrio'
	return
END

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Comienza la ejecucion ',0 , ''



IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CYD01_CURSOS'  and xtype='u' 
		AND ID IN (SELECT  id FROM  sysColumns NOLOCK  WHERE name ='o_objetivos'  ))
BEGIN
	EXEC sp_rename 'CYD01_CURSOS.[o_objetivos]', 'o_objetivos_cyd', 'COLUMN'
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' sp_rename CYD01_CURSOS.o_objetivos ',@@error , ''

END
/****************************************************************************************************************/
IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CYD01_CURSOS'  and xtype='u' 
		AND ID IN (SELECT  id FROM  sysColumns NOLOCK  WHERE name ='o_temas'  ))
BEGIN
	EXEC sp_rename 'CYD01_CURSOS.[o_temas]', 'o_temas_cyd', 'COLUMN'
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' sp_rename CYD01_CURSOS.o_temas ',@@error , ''

END

/****************************************************************************************************************/
IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CYD01_CURSOS'  and xtype='u' 
		AND ID IN (SELECT  id FROM  sysColumns NOLOCK  WHERE name ='o_curso'  ))
BEGIN
	EXEC sp_rename 'CYD01_CURSOS.[o_curso]', 'o_curso_cyd', 'COLUMN'
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' sp_rename CYD01_CURSOS.o_curso ',@@error , ''

END







exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



