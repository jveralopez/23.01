Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE_ACC_create_modulo', @n_Version = 1 , @d_modulo ='ACC'

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



 

 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC01_ACCIDENTES'  and xtype='u') 
 BEGIN              


create table dbo.ACC01_ACCIDENTES (
   oi_accidente         int                  not null,
   oi_personal_emp      int                  not null,
   f_fechahora_acc      datetime             not null,
   e_condicion_riesgo   int                  not null,
   oi_tipo_acc          int                  null,
   oi_acto_inseg        int                  null,
   oi_agente_caus       int                  null,
   oi_condic_pelig      int                  null,
   oi_factor_cont       int                  null,
   oi_reg_cuerpo        int                  null,
   oi_natur_lesion      int                  null,
   oi_incapacidad       int                  null,
   oi_elemento_pp       int                  null,
   oi_ocupacion         int                  null,
   f_fechahora_revis    datetime             null,
   e_dias_perdidos      int                  null,
   f_fechahora_baja     datetime             null,
   f_fechahora_alta     datetime             null,
   o_accion_aconsej     varchar(1000)        null,
   o_accidente          varchar(1000)        null,
   constraint PK_SEG_ACCIDENTES primary key (oi_accidente),
   constraint AK_AK_I_ACC_SEG_ACCI_ACC01_AC unique (oi_personal_emp, f_fechahora_acc)
)
 

 		         EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC01_ACCIDENTES',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC01_PERSONAL'  and xtype='u') 
 BEGIN                

	create table dbo.ACC01_PERSONAL (
	   oi_personal_emp      int                  not null,
	   constraint PK_ACC01_PERSONAL primary key (oi_personal_emp)
	)
	 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC01_PERSONAL',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC01_TESTIGOS'  and xtype='u') 
 BEGIN                

	create table dbo.ACC01_TESTIGOS (
	   oi_testigo           int                  not null,
	   oi_accidente         int                  not null,
	   oi_tipo_documento    int                  null,
	   e_testigo            int                  not null,
	   c_nro_documento      varchar(30)          null,
	   d_apellido           varchar(100)         not null,
	   d_nombres            varchar(100)         not null,
	   d_domicilio          varchar(100)         null,
	   o_testigo            varchar(1000)        null,
	   constraint PK_ACC01_TESTIGOS primary key (oi_testigo),
	   constraint AK_KEY_2_ACC01_TE unique (oi_accidente, e_testigo)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC01_TESTIGOS',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC02_ACTOS_INSEG'  and xtype='u') 
 BEGIN             

	create table dbo.ACC02_ACTOS_INSEG (
	   oi_acto_inseg        int                  not null,
	   c_acto_inseg         varchar(30)          not null,
	   d_acto_inseg         varchar(100)         not null,
	   constraint PK_ACC02_ACTOS_INSEG primary key (oi_acto_inseg),
	   constraint AK_AK_I_ACTO_INS_SEG__ACC02_AC unique (c_acto_inseg)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC02_ACTOS_INSEG',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC03_AGENTES_CAUS'  and xtype='u') 
 BEGIN            

	create table dbo.ACC03_AGENTES_CAUS (
	   oi_agente_caus       int                  not null,
	   c_agente_caus        varchar(30)          not null,
	   d_agente_caus        varchar(100)         not null,
	   constraint PK_ACC03_AGENTES_CAUS primary key (oi_agente_caus),
	   constraint AK_AK_I_AGEN_CAU_SEG__ACC03_AG unique (c_agente_caus)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC03_AGENTES_CAUS',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC04_CONDIC_PELIG'  and xtype='u') 
 BEGIN            

	create table dbo.ACC04_CONDIC_PELIG (
	   oi_condic_pelig      int                  not null,
	   c_condic_pelig       varchar(30)          not null,
	   d_condic_pelig       varchar(100)         not null,
	   constraint PK_ACC04_CONDIC_PELIG primary key (oi_condic_pelig),
	   constraint AK_AK_I_COND_PEL_SEG__ACC04_CO unique (c_condic_pelig)
	)
	 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC04_CONDIC_PELIG',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC05_FACTOR_CONT'  and xtype='u') 
 BEGIN             

	create table dbo.ACC05_FACTOR_CONT (
	   oi_factor_cont       int                  not null,
	   c_factor_cont        varchar(30)          not null,
	   d_factor_cont        varchar(100)         not null,
	   constraint PK_ACC05_FACTOR_CONT primary key (oi_factor_cont),
	   constraint AK_AK_I_FAC_CONT_SEG__ACC05_FA unique (c_factor_cont)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC05_FACTOR_CONT',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC06_INCAPACIDAD'  and xtype='u') 
 BEGIN             

	create table dbo.ACC06_INCAPACIDAD (
	   oi_incapacidad       int                  not null,
	   c_incapacidad        varchar(30)          not null,
	   d_incapacidad        varchar(100)         not null,
	   constraint PK_ACC06_INCAPACIDAD primary key (oi_incapacidad),
	   constraint AK_AK_I_INCAPAC_SEG_I_ACC06_IN unique (c_incapacidad)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC06_INCAPACIDAD',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC07_NATUR_LESION'  and xtype='u') 
 BEGIN            

	create table dbo.ACC07_NATUR_LESION (
	   oi_natur_lesion      int                  not null,
	   c_natur_lesion       varchar(30)          not null,
	   d_natur_lesion       varchar(100)         not null,
	   constraint PK_ACC07_NATUR_LESION primary key (oi_natur_lesion),
	   constraint AK_AK_I_NATUR_LES_SEG_ACC07_NA unique (c_natur_lesion)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC07_NATUR_LESION',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC08_OCUPACIONES'  and xtype='u') 
 BEGIN             

	create table dbo.ACC08_OCUPACIONES (
	   oi_ocupacion         int                  not null,
	   c_ocupacion          varchar(30)          not null,
	   d_ocupacion          varchar(100)         not null,
	   constraint PK_ACC08_OCUPACIONES primary key (oi_ocupacion),
	   constraint AK_AK_I_OCUP_SEG_OCUP_ACC08_OC unique (c_ocupacion)
	)
	 
	
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC08_OCUPACIONES',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC09_REG_CUERPO'  and xtype='u') 
 BEGIN              

	create table dbo.ACC09_REG_CUERPO (
	   oi_reg_cuerpo        int                  not null,
	   c_reg_cuerpo         varchar(30)          not null,
	   d_reg_cuerpo         varchar(100)         not null,
	   constraint PK_ACC09_REG_CUERPO primary key nonclustered (oi_reg_cuerpo),
	   constraint AK_AK_I_REG_CUERPO_SE_ACC09_RE unique (c_reg_cuerpo)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC09_REG_CUERPO',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC10_TIPOS_ACC'  and xtype='u') 
 BEGIN               

	create table dbo.ACC10_TIPOS_ACC (
	   oi_tipo_acc          int                  not null,
	   c_tipo_acc           varchar(30)          not null,
	   d_tipo_acc           varchar(100)         not null,
	   constraint PK_ACC10_TIPOS_ACC primary key (oi_tipo_acc),
	   constraint AK_AK_I_TIPO_ACC_SEG__ACC10_TI unique (c_tipo_acc)
	)
 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC10_TIPOS_ACC',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC12_ACC_CORREC'  and xtype='u') 
 BEGIN              

	create table dbo.ACC12_ACC_CORREC (
	   oi_acc_correc        int                  not null,
	   oi_acontecimiento    int                  not null,
	   c_acc_correc         varchar(30)          not null,
	   d_acc_correc         varchar(100)         not null,
	   f_prog_accion        datetime             not null,
	   oi_personal_resp     int                  null,
	   f_ejec_accion        datetime             null,
	   o_acc_correc         varchar(1000)        null,
	   constraint PK_ACC12_ACC_CORREC primary key (oi_acc_correc),
	   constraint AK_AK_I_ACC_CORREC_SE_ACC12_AC unique (oi_acontecimiento, c_acc_correc)
	)
	 

          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC12_ACC_CORREC',@@error , '' 
 END 
 /*--------------------------------------------------------------------------------*/
  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ACC12_ACONTEC'  and xtype='u') 
 BEGIN                 

	create table dbo.ACC12_ACONTEC (
	   oi_acontecimiento    int                  not null,
	   c_acontecimiento     varchar(30)          not null,
	   d_acontecimiento     varchar(100)         not null,
	   f_fechahora_acont    datetime             not null,
	   o_causa_acontec      varchar(1000)        null,
	   e_gravedad           int                  null,
	   n_probabilidad       numeric(11,3)        null,
	   o_acontecimiento     varchar(1000)        null,
	   constraint PK_SEG_ACONTEC primary key (oi_acontecimiento),
	   constraint AK_I_ACONT_SEG_ACON unique (c_acontecimiento)
	)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table ACC12_ACONTEC',@@error , '' 
 END 

 
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' FIN DE LOS CREATE',@@error , '' 


  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ORG07_EL'   ) 
 BEGIN 
	      alter table dbo.ACC01_ACCIDENTES
	       add constraint FK_ACC01_AC_REFERENCE_ORG07_EL foreign key (oi_elemento_pp)
	          references dbo.ORG07_ELEMENTOS_PP (oi_elemento_pp)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ORG07_EL ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC09_RE'   ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC09_RE foreign key (oi_reg_cuerpo)
                  references dbo.ACC09_REG_CUERPO (oi_reg_cuerpo)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC09_RE ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC10_TI'   ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC10_TI foreign key (oi_tipo_acc)
                  references dbo.ACC10_TIPOS_ACC (oi_tipo_acc)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC10_TI ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC02_AC'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC02_AC foreign key (oi_acto_inseg)
                  references dbo.ACC02_ACTOS_INSEG (oi_acto_inseg)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC02_AC ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC03_AG'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC03_AG foreign key (oi_agente_caus)
                  references dbo.ACC03_AGENTES_CAUS (oi_agente_caus)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint  FK_ACC01_AC_REFERENCE_ACC03_AG',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC04_CO'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC04_CO foreign key (oi_condic_pelig)
                  references dbo.ACC04_CONDIC_PELIG (oi_condic_pelig)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC04_CO ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC05_FA'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC05_FA foreign key (oi_factor_cont)
                  references dbo.ACC05_FACTOR_CONT (oi_factor_cont)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC05_FA ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC06_IN'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC06_IN foreign key (oi_incapacidad)
                  references dbo.ACC06_INCAPACIDAD (oi_incapacidad)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint  FK_ACC01_AC_REFERENCE_ACC06_IN',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC07_NA'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC07_NA foreign key (oi_natur_lesion)
                  references dbo.ACC07_NATUR_LESION (oi_natur_lesion)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC07_NA ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC08_OC'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC08_OC foreign key (oi_ocupacion)
                  references dbo.ACC08_OCUPACIONES (oi_ocupacion)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint  FK_ACC01_AC_REFERENCE_ACC08_OC',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_AC_REFERENCE_ACC01_PE'  ) 
 BEGIN 
              alter table dbo.ACC01_ACCIDENTES
               add constraint FK_ACC01_AC_REFERENCE_ACC01_PE foreign key (oi_personal_emp)
                  references dbo.ACC01_PERSONAL (oi_personal_emp)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_AC_REFERENCE_ACC01_PE ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_PE_REFERENCE_PER02_PE'  ) 
 BEGIN 
              alter table dbo.ACC01_PERSONAL
               add constraint FK_ACC01_PE_REFERENCE_PER02_PE foreign key (oi_personal_emp)
                  references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC01_PE_REFERENCE_PER02_PE ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_TE_REFERENCE_PER20_TI'  ) 
 BEGIN 
              alter table dbo.ACC01_TESTIGOS
               add constraint FK_ACC01_TE_REFERENCE_PER20_TI foreign key (oi_tipo_documento)
                  references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint  FK_ACC01_TE_REFERENCE_PER20_TI',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC01_TE_REFERENCE_ACC01_AC'  ) 
 BEGIN 
              alter table dbo.ACC01_TESTIGOS
               add constraint FK_ACC01_TE_REFERENCE_ACC01_AC foreign key (oi_accidente)
                  references dbo.ACC01_ACCIDENTES (oi_accidente)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint  FK_ACC01_TE_REFERENCE_ACC01_AC',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC12_AC_REFERENCE_PER02_PE'  ) 
 BEGIN 
              alter table dbo.ACC12_ACC_CORREC
               add constraint FK_ACC12_AC_REFERENCE_PER02_PE foreign key (oi_personal_resp)
                  references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC12_AC_REFERENCE_PER02_PE ',@@error , ' ' 
 END 
 

  IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ACC12_AC_REFERENCE_ACC12_AC'  ) 
 BEGIN 
              alter table dbo.ACC12_ACC_CORREC
               add constraint FK_ACC12_AC_REFERENCE_ACC12_AC foreign key (oi_acontecimiento)
                  references dbo.ACC12_ACONTEC (oi_acontecimiento)
          EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_ACC12_AC_REFERENCE_ACC12_AC ',@@error , ' ' 
 END 
 



exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



