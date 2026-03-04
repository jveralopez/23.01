
Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.POS.Alternative_keys', @n_Version = 1 , @d_modulo ='POSTULANTES'

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


IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS01_DOCUM_POS'  )
BEGIN
	ALTER TABLE POS01_DOCUM_CV
	DROP CONSTRAINT AK_POS01_DOCUM_POS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS01_DOCUM_POS' , @@error , ''
END 

IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_COMPETENCIA_POS'  )
BEGIN
	ALTER TABLE POS01_COMPET_CV
	DROP CONSTRAINT AK_COMPETENCIA_POS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_COMPETENCIA_POS' , @@error , ''
END 


IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS_FAMILIARES_POS'  )
BEGIN
	ALTER TABLE POS01_FLIARES_CV
	DROP CONSTRAINT AK_POS_FAMILIARES_POS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS_FAMILIARES_POS' , @@error , ''
END 

IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS_IDIOMAS_POS'  )
BEGIN
	ALTER TABLE POS01_IDIOMAS_CV
	DROP CONSTRAINT AK_POS_IDIOMAS_POS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS_IDIOMAS_POS' , @@error , ''
END 


IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS01_ESTUDIOS_POS'  )
BEGIN
	ALTER TABLE POS01_ESTUDIOS_CV
	DROP CONSTRAINT AK_POS01_ESTUDIOS_POS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS01_ESTUDIOS_POS' , @@error , ''
END 


IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_AK_POS01_CURSOS_POS01_CU'  )
BEGIN
	ALTER TABLE POS01_CURSOS_CV 
	ADD CONSTRAINT AK_AK_POS01_CURSOS_POS01_CU UNIQUE (oi_cv, oi_curso, d_curso_ext_cv)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_AK_POS01_CURSOS_POS01_CU' , @@error , ''
END 


IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS01_DOCUM_POS'  )
BEGIN
	ALTER TABLE POS01_DOCUM_CV 
	ADD CONSTRAINT AK_POS01_DOCUM_POS unique (oi_cv, oi_tipo_documento)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS01_DOCUM_POS' , @@error , ''
END 

IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS01_DOCUM_POS'  )
BEGIN
	ALTER TABLE POS01_DOCUM_CV 
	ADD CONSTRAINT AK_POS01_DOCUM_POS unique (oi_cv, oi_tipo_documento)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS01_DOCUM_POS' , @@error , ''
END 

IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS01_DOMIC_POS'  )
BEGIN
	ALTER TABLE POS01_DOMIC_CV
	ADD CONSTRAINT AK_POS01_DOMIC_POS unique (oi_cv, oi_tipo_domicilio)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS01_DOMIC_POS' , @@error , ''
END 

IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_COMPETENCIA_POS'  )
BEGIN
	ALTER TABLE POS01_COMPET_CV
	ADD CONSTRAINT AK_COMPETENCIA_POS unique (oi_cv, oi_competencia)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_COMPETENCIA_POS' , @@error , ''
END 


IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS_FAMILIARES_POS'  )
BEGIN
	ALTER TABLE POS01_FLIARES_CV
	ADD CONSTRAINT AK_POS_FAMILIARES_POS unique (oi_cv, oi_tipo_documento, c_nro_documento)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS_FAMILIARES_POS' , @@error , ''
END 

IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS_IDIOMAS_POS'  )
BEGIN
	ALTER TABLE POS01_IDIOMAS_CV
	ADD CONSTRAINT AK_POS_IDIOMAS_POS unique (oi_cv, oi_idioma)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS_IDIOMAS_POS' , @@error , ''
END 

IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_KEY_2_POS01_LI'  )
BEGIN
	ALTER TABLE POS01_LIB_SANIT
	ADD CONSTRAINT AK_KEY_2_POS01_LI unique (oi_cv, c_tipo_lib)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_KEY_2_POS01_LI' , @@error , ''
END 

IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='AK_POS01_ESTUDIOS_POS '  )
BEGIN
	ALTER TABLE POS01_ESTUDIOS_CV 
	ADD CONSTRAINT AK_POS01_ESTUDIOS_POS UNIQUE (oi_cv, oi_estudio)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'AK_POS01_ESTUDIOS_POS' , @@error , ''
END 



exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

