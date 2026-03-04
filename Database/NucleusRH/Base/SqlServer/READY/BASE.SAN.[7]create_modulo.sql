Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE_SAN_create_modulo', @n_Version = 1 , @d_modulo ='SAN'

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

 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='SAN01_MOTIVOS_SANC'  and xtype='u') 
 BEGIN
       
	/*==============================================================*/
	/* Table: SAN01_MOTIVOS_SANC                                    */
	/*==============================================================*/
	create table dbo.SAN01_MOTIVOS_SANC (
	   oi_motivo_sancion    int                  not null,
	   c_motivo_sancion     varchar(30)          not null,
	   d_motivo_sancion     varchar(100)         not null,
	   d_motivo_corta       varchar(100)         null,
	   oi_sancion           int                  null,
	   e_cant_repeticion    int                  null,
	   constraint PK_SAN_MOTIVOS_SANC primary key (oi_motivo_sancion),
	   constraint AK_I_MOT_SAN_SAN_MOTI unique (c_motivo_sancion)
	)


	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table SAN01_MOTIVOS_SANC',@@error , '' 
 END 
 	 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='SAN02_SANCIONES'  and xtype='u') 
 BEGIN
	       
	/*==============================================================*/
	/* Table: SAN02_SANCIONES                                       */
	/*==============================================================*/
	create table dbo.SAN02_SANCIONES (
	   oi_sancion           int                  not null,
	   c_sancion            varchar(30)          not null,
	   d_sancion            varchar(100)         not null,
	   c_tipo_sancion       varchar(30)          not null
	      constraint CKC_C_TIPO_SANCION_SAN02_SA check (c_tipo_sancion in ('L','G')),
	   l_suspension         smallint             not null,
	   e_dias_suspension    int                  null,
	   e_dias_max_susp      int                  null,
	   l_notificacion       smallint             not null,
	   o_sancion            varchar(1000)        null,
	   constraint PK_SAN_SANCIONES primary key (oi_sancion),
	   constraint AK_I_MED_SAN_SANC unique (c_sancion)
	)

 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table SAN02_SANCIONES',@@error , '' 
 END 
 
	 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='SAN03_PERSONAL'  and xtype='u') 
 BEGIN
       
	/*==============================================================*/
	/* Table: SAN03_PERSONAL                                        */
	/*==============================================================*/
	create table dbo.SAN03_PERSONAL (
	   oi_personal_emp      int                  not null,
	   constraint PK_SAN03_PERSONAL primary key (oi_personal_emp)
	)


 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table SAN03_PERSONAL',@@error , '' 
 END 
 	 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='SAN03_SANCION_PER'  and xtype='u') 
 BEGIN
	       
	/*==============================================================*/
	/* Table: SAN03_SANCION_PER                                     */
	/*==============================================================*/
	create table dbo.SAN03_SANCION_PER (
	   oi_sancion_per       int                  not null,
	   oi_personal_emp      int                  not null,
	   f_fechahora_sanc     datetime             not null,
	   oi_motivo_sancion    int                  not null,
	   oi_sancion           int                  not null,
	   e_dias_susp_prop     int                  null,
	   e_dias_susp_real     int                  null,
	   f_inicio_susp        datetime             null,
	   f_fin_susp           datetime             null,
	   o_motivo_ajuste      varchar(1000)        null,
	   c_estado             varchar(30)          not null
	      constraint CKC_C_ESTADO_SAN03_SA check (c_estado in ('P','A','R','C')),
	   f_estado             datetime             not null,
	   o_sancion            varchar(1000)        null,
	   constraint PK__SAN_SANCIONES_PE__76F75B38 primary key (oi_sancion_per),
	   constraint AK_AK_SANC_PER_SAN03_SA unique (oi_personal_emp, f_fechahora_sanc)
	)


 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table SAN03_SANCION_PER',@@error , '' 
 END 
 	 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='SAN03_SUSP_PER'  and xtype='u') 
 BEGIN
	       
	/*==============================================================*/
	/* Table: SAN03_SUSP_PER                                        */
	/*==============================================================*/
	create table dbo.SAN03_SUSP_PER (
	   oi_suspension_per    int                  not null,
	   oi_sancion_per       int                  not null,
	   f_fechahora_ini      datetime             not null,
	   f_fechahora_fin      datetime             null,
	   e_dias_susp          int                  null,
	   f_fechahora_int      datetime             null,
	   constraint PK_SAN_SUSPENSION_PER primary key (oi_suspension_per),
	   constraint AK_I_SUSP_PER_SAN_SUSP unique (oi_sancion_per, f_fechahora_ini)
	)
 

 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table SAN03_SUSP_PER',@@error , '' 
 END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_SAN01_MO_REFERENCE_SAN02_SA'   ) 
 BEGIN
	       
	alter table dbo.SAN01_MOTIVOS_SANC
	   add constraint FK_SAN01_MO_REFERENCE_SAN02_SA foreign key (oi_sancion)
	      references dbo.SAN02_SANCIONES (oi_sancion)
	 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add Constraint FK_SAN01_MO_REFERENCE_SAN02_SA',@@error , '' 
 END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_SAN03_PE_REFERENCE_PER02_PE'   ) 
 BEGIN
 
	alter table dbo.SAN03_PERSONAL
	   add constraint FK_SAN03_PE_REFERENCE_PER02_PE foreign key (oi_personal_emp)
	      references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
	 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add Constraint FK_SAN03_PE_REFERENCE_PER02_PE',@@error , '' 
 END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_SAN03_SA_REFERENCE_SAN01_MO'   ) 
 BEGIN
 
	alter table dbo.SAN03_SANCION_PER
	   add constraint FK_SAN03_SA_REFERENCE_SAN01_MO foreign key (oi_motivo_sancion)
	      references dbo.SAN01_MOTIVOS_SANC (oi_motivo_sancion)
	 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add Constraint FK_SAN03_SA_REFERENCE_SAN01_MO',@@error , '' 
 END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_SAN03_SA_REFERENCE_SAN02_SA'   ) 
 BEGIN
 
	alter table dbo.SAN03_SANCION_PER
	   add constraint FK_SAN03_SA_REFERENCE_SAN02_SA foreign key (oi_sancion)
	      references dbo.SAN02_SANCIONES (oi_sancion)
	 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add Constraint FK_SAN03_SA_REFERENCE_SAN02_SA',@@error , '' 
 END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_SAN03_SA_REFERENCE_SAN03_PE'   ) 
 BEGIN
	 
	alter table dbo.SAN03_SANCION_PER
	   add constraint FK_SAN03_SA_REFERENCE_SAN03_PE foreign key (oi_personal_emp)
	      references dbo.SAN03_PERSONAL (oi_personal_emp)
	 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add Constraint FK_SAN03_SA_REFERENCE_SAN03_PE',@@error , '' 
 END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_SAN03_SU_REFERENCE_SAN03_SA'   ) 
 BEGIN
 
	alter table dbo.SAN03_SUSP_PER
	   add constraint FK_SAN03_SU_REFERENCE_SAN03_SA foreign key (oi_sancion_per)
	      references dbo.SAN03_SANCION_PER (oi_sancion_per)
	 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add Constraint FK_SAN03_SU_REFERENCE_SAN03_SA',@@error , '' 
 END 
 
 






exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 




