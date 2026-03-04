Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.TTA.Licencias_version_Liq', @n_Version = 1 , @d_modulo ='TTA'

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

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA04_PERSONAL' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='e_version' ) )
BEGIN
	
	alter table TTA04_PERSONAL
	add e_version int null

	
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD e_version TTA04_PERSONAL' , @@error , ''

END


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA10_LIQUIDACPERS' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='e_version' ) )
BEGIN
	
	
	alter table TTA10_LIQUIDACPERS
	add e_version int null

	
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD e_version TTA10_LIQUIDACPERS' , @@error , ''

END


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA11_ESPERANZAPER' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='e_version' ) )
BEGIN
	
	alter table TTA11_ESPERANZAPER
	add e_version int null

	
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD e_version TTA11_ESPERANZAPER' , @@error , ''

END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA04_LICENCIAS'   )
BEGIN
/*==============================================================*/
/* Table: TTA04_LICENCIAS                                       */
/*==============================================================*/
	create table dbo.TTA04_LICENCIAS (
	   oi_licencia_per      int                  not null,
	   oi_personal_emp      int                  not null,
	   l_aprobada           smallint             null,
	   constraint PK_TTA04_LICENCIAS primary key (oi_licencia_per)
	)
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.TTA04_LICENCIAS' , @@error , ''

END



IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA04_LICENCIAS' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_TTA04_LI_REFERENCE_TTA04_PE' ) )
BEGIN

	alter table dbo.TTA04_LICENCIAS
	   add constraint FK_TTA04_LI_REFERENCE_TTA04_PE foreign key (oi_personal_emp)
	      references dbo.TTA04_PERSONAL (oi_personal_emp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_TTA04_LI_REFERENCE_TTA04_PE' , @@error , ''

END




IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA04_LICENCIAS' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_TTA04_LI_REFERENCE_PER02_LI' ) )
BEGIN

	alter table dbo.TTA04_LICENCIAS
	   add constraint FK_TTA04_LI_REFERENCE_PER02_LI foreign key (oi_licencia_per)
	      references dbo.PER02_LICEN_PER (oi_licencia_per)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_TTA04_LI_REFERENCE_PER02_LI' , @@error , ''

END



IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA01_TIPOHORAS' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='oi_licencia' ) )
BEGIN
	alter table dbo.TTA01_TIPOHORAS
	add oi_licencia int null
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add oi_licencia  TTA01_TIPOHORAS ' , @@error , ''

END
 




IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA01_TIPOHORAS' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_TTA01_TI_REFERENCE_PER16_LI' ) )
BEGIN
	alter table dbo.TTA01_TIPOHORAS
	   add constraint FK_TTA01_TI_REFERENCE_PER16_LI foreign key (oi_licencia)
	      references dbo.PER16_LICENCIAS (oi_licencia)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_TTA01_TI_REFERENCE_PER16_LI' , @@error , ''

END


exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



