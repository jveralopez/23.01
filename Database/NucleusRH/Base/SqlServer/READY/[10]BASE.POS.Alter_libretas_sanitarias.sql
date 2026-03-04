Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.POS.Alter_libretas_sanitarias', @n_Version = 1 , @d_modulo ='POSTULANTES'

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

IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_LIB_SANIT' and id in 
		(select parent_obj from sysobjects nolock where name ='AK_KEY_2_POS01_LI') )
BEGIN

	
	ALTER TABLE POS01_LIB_SANIT
	DROP CONSTRAINT AK_KEY_2_POS01_LI

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'DROP CONSTRAINT AK_KEY_2_POS01_LI  ' , @@error , ''
END


IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_LIB_SANIT' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='e_lib_sanit') )
BEGIN

	
	ALTER TABLE POS01_LIB_SANIT
	ALTER COLUMN e_lib_sanit INT NULL

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'ALTER COLUMN e_lib_sanit  ' , @@error , ''
END

IF   EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_LIB_SANIT' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='f_vencimiento') )
BEGIN

	
	ALTER TABLE POS01_LIB_SANIT
	ALTER COLUMN f_vencimiento DATETIME NULL

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'ALTER COLUMN f_vencimiento DATETIME' , @@error , ''
END

IF    EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_LIB_SANIT' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='d_autoridad') )
BEGIN

	ALTER TABLE POS01_LIB_SANIT
	ALTER COLUMN d_autoridad VARCHAR(100) NULL

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ALTER COLUMN d_autoridad VARCHAR(100)' , @@error , ''
END








exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



