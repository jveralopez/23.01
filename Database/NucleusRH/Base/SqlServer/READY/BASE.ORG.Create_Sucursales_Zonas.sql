
Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.ORG.Create_Sucursales_Zonas', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version  and d_script=@ScripName 
select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Organizacion',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR'
	return
END


EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Organizacion', 'Comienza la ejecucion ',0 , ''
	

 

	/*==============================================================*/
	/* Table: ORG33_SUCURSALES                                      */
	/*==============================================================*/
	create table dbo.ORG33_SUCURSALES (
	   oi_sucursal          int                  not null,
	   c_sucursal           varchar(30)          not null,
	   d_sucursal           varchar(100)         not null,
	   constraint PK_ORG33_SUCURSALES primary key (oi_sucursal),
	   constraint AK_AK_BINARIOS_ORG33_SU unique (oi_sucursal, c_sucursal)
	)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Organizacion', ' create table dbo.ORG33_SUCURSALES',@@error , ''	 

	
	/*==============================================================*/
	/* Table: ORG19_ZONAS                                           */
	/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='ORG19_ZONAS')
BEGIN
	create table dbo.ORG19_ZONAS (
	   oi_zona              int                  not null,
	   oi_localidad         int                  null,
	   c_zona               varchar(30)          not null,
	   d_zona               varchar(100)         not null,
	   constraint PK_ORG19_ZONAS primary key (oi_zona),
	   constraint AK_AK1_ORG_ZONAS_ORG19_ZO unique (c_zona)
	)



	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Organizacion', ' create table dbo.ORG19_ZONAS',@@error , ''	 

	
	alter table dbo.ORG19_ZONAS
	   add constraint FK_ORG19_ZO_REFERENCE_ORG19_LO foreign key (oi_localidad)
	      references dbo.ORG19_LOCALIDADES (oi_localidad)
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Organizacion', ' FK_ORG19_ZO_REFERENCE_ORG19_LO',@@error , ''	 


END




EXEC dbo.sp_NMD_VerifScript @n_Version , @ScripName  ,1 ,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Organizacion ', 'Finalizo la ejecucion',@Result , ''	



 




