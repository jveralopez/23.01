Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.POS.Alter_estudios', @n_Version = 1 , @d_modulo ='POSTULANTES'

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


IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_ESTUDIOS_CV'  and xtype='u')
BEGIN
	IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_ESTUDIOS_CV'  and xtype='u' 
			AND ID IN (SELECT  id FROM  sysColumns NOLOCK  WHERE name ='f_ini_estudio'  ))
	BEGIN
		ALTER TABLE POS01_ESTUDIOS_CV
		ALTER COLUMN f_ini_estudio DATETIME NULL

		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ALTER COLUMN f_ini_estudio ',@@error , ''

	END
END

exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

