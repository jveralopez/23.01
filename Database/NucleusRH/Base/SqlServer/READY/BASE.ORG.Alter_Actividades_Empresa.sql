Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.ORG.Alter_Actividades_Empresa', @n_Version = 1 , @d_modulo ='ORGANIZACION'

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
/* Table: ORG34_ACTIVIDADES                                     */
/*==============================================================*/
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG34_ACTIVIDADES'  and xtype='u')
BEGIN
	create table dbo.ORG34_ACTIVIDADES (
	   oi_actividad         int                  not null,
	   c_actividad          varchar(30)          not null,
	   d_actividad          varchar(100)         not null,
	   constraint PK_ORG34_ACTIVIDADES primary key (oi_actividad),
	   constraint AK_AK_KEY_2_ORG34_AC_ORG34_AC unique (c_actividad)
	)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.ORG34_ACTIVIDADES ',@@error , ''

END 


IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG03_EMPRESAS'  and xtype='u')
BEGIN

	IF NOT EXISTS (select * from sysobjects NOLOCK where id in (SELECT  id  FROM  syscolumns  NOLOCK  WHERE name ='oi_actividad' )and xtype='u' and name='ORG03_EMPRESAS') 
	BEGIN
		alter table  dbo.ORG03_EMPRESAS
		ADD oi_actividad int null
		 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' alter table  dbo.ORG03_EMPRESAS ',@@error , ''
	END 


	IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ORG03_EM_REFERENCE_ORG34_AC'  )
	BEGIN
		alter table dbo.ORG03_EMPRESAS
		   add constraint FK_ORG03_EM_REFERENCE_ORG34_AC foreign key (oi_actividad)
		      references dbo.ORG34_ACTIVIDADES (oi_actividad)
		 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' FK_ORG03_EM_REFERENCE_ORG34_AC ',@@error , ''
	
	END
 
END

exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

