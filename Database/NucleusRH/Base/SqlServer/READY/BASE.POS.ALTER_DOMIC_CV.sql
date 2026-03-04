Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.POS.ALTER_DOMIC_CV', @n_Version = 1 , @d_modulo ='POSTULANTES'

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



IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='pos01_domic_cv'  and xtype='u' 
	AND ID IN (SELECT  id FROM  sysColumns NOLOCK  WHERE name in ( 'te_1', 'te_2') ))
 BEGIN
         
	alter table pos01_domic_cv
	drop  column te_1, column te_2

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' drop  column te_1, column te_2 ' ,@@error , ''


 END		





IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='pos01_domic_cv'  and xtype='u' 
	AND ID IN (SELECT  id FROM  sysColumns NOLOCK  WHERE name in ('n_pais','n_area','n_telefono','n_pais_alt','n_area_alt','n_telefono_alt')))
 BEGIN
         
	alter table pos01_domic_cv
	add n_pais               numeric(4)           null,
	 n_area               numeric(4)           null,
	 n_telefono           numeric(15)          null,
	 n_pais_alt           numeric(4)           null,
	 n_area_alt           numeric(4)           null,
	 n_telefono_alt       numeric(15)          null

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add n_pais,n_area,n_telefono,n_pais_alt,n_area_alt,n_telefono_alt' ,@@error , ''


 END		






exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 




