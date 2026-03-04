Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE_CYD_drop_add_constraints', @n_Version = 1 , @d_modulo ='CAPACITACION'

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
 
/*==============================================================*/
/* Table: CYD01_PROV_CURSO                                      */
/*==============================================================*/
IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CYD01_PROV_CURSO'  )
BEGIN 
	IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_CYD01_PR'  )
	
	BEGIN
		ALTER TABLE CYD01_PROV_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_PR
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_CYD01_PR' , @@error , ''
	
	END
	
	IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_CYD01_PR'  )
	BEGIN
		ALTER TABLE CYD01_PROV_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_PR unique (oi_proveedor, oi_curso)
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_CYD01_PR' , @@error , ''
	
	END

END
/*==============================================================*/
/* Table: CYD01_COMP_CURSO                                      */
/*==============================================================*/
IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CYD01_COMP_CURSO'  )
BEGIN
	IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_CYD01_CO'  )
	BEGIN
		ALTER TABLE CYD01_COMP_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_CO
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_CYD01_CO' , @@error , ''
	END
	
	IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_CYD01_CO'  )
	BEGIN
		ALTER TABLE CYD01_COMP_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_CO unique (oi_curso, oi_competencia)
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_CYD01_CO' , @@error , ''
	END
END
/*==============================================================*/
/* Table: CYD01_REC_CURSO                                       */
/*==============================================================*/
IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CYD01_REC_CURSO'  )
BEGIN
	IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_CYD01_RE'  )
	BEGIN
		ALTER TABLE CYD01_REC_CURSO DROP CONSTRAINT AK_KEY_2_CYD01_RE
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_CYD01_RE' , @@error , ''
	END
	
	IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_CYD01_RE'  )
	BEGIN
		ALTER TABLE CYD01_REC_CURSO ADD CONSTRAINT AK_KEY_2_CYD01_RE unique (oi_recurso, oi_curso)
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_CYD01_RE' , @@error , ''
	END
END


exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	







