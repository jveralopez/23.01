Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE_MED_create_modulo', @n_Version = 1 , @d_modulo ='MED'

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




/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED01_ESPECIALIDAD'  and xtype='u') 
 BEGIN
                                  

create table dbo.MED01_ESPECIALIDAD (
   oi_especialidad      int                  not null,
   c_especialidad       varchar(30)          not null,
   d_especialidad       varchar(100)         not null,
   constraint PK_MED_ESPECIALIDADES primary key (oi_especialidad),
   constraint AK_I_ESP_MED_ESPE unique (c_especialidad)
) 



 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED01_ESPECIALIDAD',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED02_EMPRESAS'  and xtype='u') 
 BEGIN
	                                      
	
	create table dbo.MED02_EMPRESAS (
	   oi_empresa_medica    int                  not null,
	   c_empresa_medica     varchar(30)          not null,
	   d_empresa_medica     varchar(100)         not null,
	   d_domicilio          varchar(100)         null,
	   oi_localidad         int                  not null,
	   te_empresa           varchar(50)          null,
	   te_fax               varchar(50)          null,
	   c_nro_cuit           varchar(30)          null,
	   d_responsable        varchar(100)         null,
	   d_cargo_resp         varchar(100)         null,
	   constraint PK_MED_EMPRESAS primary key (oi_empresa_medica),
	   constraint AK_I_EMP_MED_MED_EMPR unique (c_empresa_medica)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED02_EMPRESAS',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED03_MEDICOS'  and xtype='u') 
 BEGIN
	                                       
	
	create table dbo.MED03_MEDICOS (
	   oi_medico            int                  not null,
	   oi_tipo_documento    int                  not null,
	   c_nro_documento      varchar(30)          not null,
	   d_apellido           varchar(100)         not null,
	   d_nombres            varchar(100)         not null,
	   d_domicilio          varchar(100)         null,
	   oi_localidad         int                  null,
	   te_particular        varchar(50)          null,
	   oi_especialidad      int                  not null,
	   c_nro_matricula      varchar(50)          null,
	   oi_empresa_medica    int                  null,
	   constraint PK_MED_MEDICOS primary key (oi_medico),
	   constraint AK_I_MEDICO_MED_MEDI unique (oi_tipo_documento, c_nro_documento)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED03_MEDICOS',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_ANTEC_PER'  and xtype='u') 
 BEGIN
                                     
	
	create table dbo.MED04_ANTEC_PER (
	   oi_antec_per         int                  not null,
	   oi_personal_emp      int                  not null,
	   oi_antecedente       int                  not null,
	   f_antecedente        datetime             not null,
	   d_valor              varchar(100)         not null,
	   o_antec_per          varchar(1000)        null,
	   constraint PK_MED_ANTECED_PER primary key (oi_antec_per),
	   constraint AK_I_ANTEC_PER_MED_ANTE unique (oi_personal_emp, oi_antecedente, f_antecedente)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_ANTEC_PER',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_DETALLE_HIST'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED04_DETALLE_HIST (
	   oi_detalle_cons      int                  not null,
	   oi_consulta_per      int                  not null,
	   f_fechahora_visita   datetime             not null,
	   oi_medico            int                  null,
	   d_domicilio          varchar(100)         null,
	   oi_tipo_domicilio    int                  null,
	   oi_lugar_atencion    int                  null,
	   o_medicacion         varchar(1000)        null,
	   o_detalle_cons       varchar(1000)        null,
	   constraint PK_MED_DETALLE_HIST primary key (oi_detalle_cons),
	   constraint AK_KEY_2_MED04_DE unique (oi_consulta_per, f_fechahora_visita)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_DETALLE_HIST',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_EXAMEN_PER'  and xtype='u') 
 BEGIN
                                    
	
	create table dbo.MED04_EXAMEN_PER (
	   oi_examen_per        int                  not null,
	   oi_personal_emp      int                  not null,
	   oi_examen            int                  not null,
	   f_examen             datetime             not null,
	   f_vencimiento        datetime             null,
	   n_costo              numeric(11,3)        null,
	   o_examen_per         varchar(1000)        null,
	   constraint PK_MED_EXA_PER primary key (oi_examen_per),
	   constraint AK_I_EXAM_PER_MED_EXA_ unique (oi_personal_emp, oi_examen, f_examen)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_EXAMEN_PER',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_FACTORES_EVA'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED04_FACTORES_EVA (
	   oi_factor_eva        int                  not null,
	   oi_examen_per        int                  not null,
	   oi_factor_examen     int                  not null,
	   d_valor              varchar(100)         not null,
	   o_factor_eva         varchar(1000)        null,
	   constraint PK_MED_FAC_EXA primary key (oi_factor_eva),
	   constraint AK_I_FACT_EXA_MED_FAC_ unique (oi_examen_per, oi_factor_examen)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_FACTORES_EVA',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_HIST_CLINICA'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED04_HIST_CLINICA (
	   oi_consulta_per      int                  not null,
	   oi_personal_emp      int                  not null,
	   f_fechahora_cons     datetime             not null,
	   oi_motivo_consulta   int                  not null,
	   f_baja               datetime             null,
	   f_prev_alta          datetime             null,
	   f_alta               datetime             null,
	   e_dias_perdidos      int                  null,
	   oi_enfermedad        int                  null,
	   f_prox_consulta      datetime             null,
	   o_consulta_per       varchar(1000)        null,
	   constraint PK_MED_HIST_CLINICAS primary key (oi_consulta_per),
	   constraint AK_I_CONS_PER_MED_HIST unique (oi_personal_emp, f_fechahora_cons)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_HIST_CLINICA',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_MEDIC_SUMIN'  and xtype='u') 
 BEGIN
                                   
	
	create table dbo.MED04_MEDIC_SUMIN (
	   oi_medic_sumin       int                  not null,
	   oi_consulta_per      int                  not null,
	   oi_medicamento       int                  not null,
	   c_medicamento        varchar(30)          not null,
	   oi_motivo_med        int                  null,
	   o_medic_sumin        varchar(100)         null,
	   constraint PK_MED_MEDIC_SUMINIST primary key (oi_medic_sumin),
	   constraint AK_I_MED_SUMINIST_MED_MEDI unique (oi_consulta_per, oi_medicamento)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_MEDIC_SUMIN',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED04_PERSONAL'  and xtype='u') 
 BEGIN
	                                      
	
	create table dbo.MED04_PERSONAL (
	   oi_personal_emp      int                  not null,
	   constraint PK_MED04_PERSONAL primary key (oi_personal_emp)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED04_PERSONAL ',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED05_MOTIVOS_CONS'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED05_MOTIVOS_CONS (
	   oi_motivo_consulta   int                  not null,
	   c_motivo_consulta    varchar(30)          not null,
	   d_motivo_consulta    varchar(100)         not null,
	   constraint PK_MED_MOTIVOS_CONS primary key (oi_motivo_consulta),
	   constraint AK_I_MOT_CONS_MED_MOTI unique (c_motivo_consulta)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED05_MOTIVOS_CONS',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED06_ENFERMEDADES'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED06_ENFERMEDADES (
	   oi_enfermedad        int                  not null,
	   c_enfermedad         varchar(30)          not null,
	   d_enfermedad         varchar(100)         not null,
	   constraint PK_MED_ENFERMEDADES primary key (oi_enfermedad),
	   constraint AK_I_ENFER_MED_ENFE unique (c_enfermedad)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED06_ENFERMEDADES',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED07_LUGARES_ATEN'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED07_LUGARES_ATEN (
	   oi_lugar_atencion    int                  not null,
	   c_lugar_atencion     varchar(30)          not null,
	   d_lugar_atencion     varchar(100)         not null,
	   constraint PK_MED_LUGARES_ATENC primary key (oi_lugar_atencion),
	   constraint AK_I_LUG_ATENC_MED_LUGA unique (c_lugar_atencion)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED07_LUGARES_ATEN',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED08_MEDICAMENTOS'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED08_MEDICAMENTOS (
	   oi_medicamento       int                  not null,
	   c_medicamento        varchar(30)          not null,
	   d_medicamento        varchar(100)         not null,
	   o_medicamento        varchar(1000)        null,
	   constraint PK_MED_MEDICAMENTOS primary key (oi_medicamento),
	   constraint AK_I_MEDICA_MED_MEDI unique (c_medicamento)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED08_MEDICAMENTOS',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED09_MOTIVOS_MED'  and xtype='u') 
 BEGIN
                                   
	
	create table dbo.MED09_MOTIVOS_MED (
	   oi_motivo_med        int                  not null,
	   c_motivo_med         varchar(30)          not null,
	   d_motivo_med         varchar(100)         not null,
	   constraint PK_MED_MOTIVOS_MEDICA primary key (oi_motivo_med),
	   constraint AK_I_MOT_MEDICA_MED_MOTI unique (c_motivo_med)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED09_MOTIVOS_MED',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED10_EXAMENES'  and xtype='u') 
 BEGIN
                                      
	
	create table dbo.MED10_EXAMENES (
	   oi_examen            int                  not null,
	   oi_tipo_examen       int                  not null,
	   c_examen             varchar(30)          not null,
	   d_examen             varchar(100)         not null,
	   oi_categ_examen      int                  null,
	   e_dias_vencimiento   int                  null,
	   n_costo              numeric(11,3)        null,
	   o_examen             varchar(30)          null,
	   constraint PK_MED_EXAMENES primary key (oi_examen),
	   constraint AK_I_EX_MED_EXAM unique (oi_tipo_examen, c_examen)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED10_EXAMENES',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED10_FACTORES'  and xtype='u') 
 BEGIN
                                      
	
	create table dbo.MED10_FACTORES (
	   oi_factor_examen     int                  not null,
	   oi_examen            int                  not null,
	   c_factor_examen      varchar(30)          not null,
	   d_factor_examen      varchar(100)         not null,
	   constraint PK_MED_FACTORES primary key (oi_factor_examen),
	   constraint AK_I_FAC_MED_FACT unique (oi_examen, c_factor_examen)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED10_FACTORES',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED10_TIPOS_EXAMEN'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED10_TIPOS_EXAMEN (
	   oi_tipo_examen       int                  not null,
	   c_tipo_examen        varchar(30)          not null,
	   d_tipo_examen        varchar(100)         not null,
	   constraint PK_MED_TIPOS_EXAMEN primary key (oi_tipo_examen),
	   constraint AK_I_TIPO_EXA_MED_TIPO unique (c_tipo_examen)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED10_TIPOS_EXAMEN',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED11_CATEG_EXAMEN'  and xtype='u') 
 BEGIN
                                  
	
	create table dbo.MED11_CATEG_EXAMEN (
	   oi_categ_examen      int                  not null,
	   c_categ_examen       varchar(30)          not null,
	   d_categ_examen       varchar(100)         not null,
	   constraint PK_MED_CATEG_EXA primary key (oi_categ_examen),
	   constraint AK_I_CATEG_EXA_MED_CATE unique (c_categ_examen)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED11_CATEG_EXAMEN',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED12_ANTECEDENTES'  and xtype='u') 
 BEGIN
	                                  
	
	create table dbo.MED12_ANTECEDENTES (
	   oi_antecedente       int                  not null,
	   oi_tipo_antec        int                  not null,
	   c_antecedente        varchar(30)          not null,
	   d_antecedente        varchar(100)         not null,
	   o_antecedente        varchar(1000)        null,
	   constraint PK_MED_ANTECEDENTES primary key (oi_antecedente),
	   constraint AK_I_ANTECED_MED_ANTE unique (oi_tipo_antec, c_antecedente)
	) 
	
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table MED12_ANTECEDENTES',@@error , '' 
 END 
 
/*==============================================================*/ 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='MED12_TIPOS_ANTEC'  and xtype='u') 
 BEGIN
                                   
	
	create table dbo.MED12_TIPOS_ANTEC (
	   oi_tipo_antec        int                  not null,
	   c_tipo_antec         varchar(30)          not null,
	   d_tipo_antec         varchar(100)         not null,
	   constraint PK_MED_TIPOS_ANTECED primary key (oi_tipo_antec),
	   constraint AK_I_TIPO_ANT_MED_TIPO unique (c_tipo_antec)
	) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add Constraint  MED12_TIPOS_ANTEC',@@error , '' 
 END 
 
/****************************************************************/

 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_EMPRE__oi_lo__479D4DD1'   ) 
 BEGIN  
	 alter table dbo.MED02_EMPRESAS
	   add constraint FK__MED_EMPRE__oi_lo__479D4DD1 foreign key (oi_localidad)
	      references dbo.ORG19_LOCALIDADES (oi_localidad) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_EMPRE__oi_lo__479D4DD1 ',@@error , '' 
END 
 

 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_em__1C48DB78'   ) 
 BEGIN  
	 alter table dbo.MED03_MEDICOS
	   add constraint FK__MED_MEDIC__oi_em__1C48DB78 foreign key (oi_empresa_medica)
	      references dbo.MED02_EMPRESAS (oi_empresa_medica) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_MEDIC__oi_em__1C48DB78',@@error , '' 
END 
 


 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_es__1B54B73F'   ) 
 BEGIN  
	 alter table dbo.MED03_MEDICOS
	   add constraint FK__MED_MEDIC__oi_es__1B54B73F foreign key (oi_especialidad)
	      references dbo.MED01_ESPECIALIDAD (oi_especialidad) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_MEDIC__oi_es__1B54B73F',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_lo__1A609306'   ) 
 BEGIN  
	 alter table dbo.MED03_MEDICOS
	   add constraint FK__MED_MEDIC__oi_lo__1A609306 foreign key (oi_localidad)
	      references dbo.ORG19_LOCALIDADES (oi_localidad) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_MEDIC__oi_lo__1A609306 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_ti__196C6ECD'   ) 
 BEGIN  
	 alter table dbo.MED03_MEDICOS
	   add constraint FK__MED_MEDIC__oi_ti__196C6ECD foreign key (oi_tipo_documento)
	      references dbo.PER20_TIPOS_DOC (oi_tipo_documento) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_MEDIC__oi_ti__196C6ECD ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_ANTEC__oi_an__55615D43'   ) 
 BEGIN  
	 alter table dbo.MED04_ANTEC_PER
	   add constraint FK__MED_ANTEC__oi_an__55615D43 foreign key (oi_antecedente)
	      references dbo.MED12_ANTECEDENTES (oi_antecedente) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_ANTEC__oi_an__55615D43 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_MED04_AN_REFERENCE_MED04_PE'   ) 
 BEGIN  
	 alter table dbo.MED04_ANTEC_PER
	   add constraint FK_MED04_AN_REFERENCE_MED04_PE foreign key (oi_personal_emp)
	      references dbo.MED04_PERSONAL (oi_personal_emp) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK_MED04_AN_REFERENCE_MED04_PE ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_DETAL__OI_ME__254835CD'   ) 
 BEGIN  
	 alter table dbo.MED04_DETALLE_HIST
	   add constraint FK__MED_DETAL__OI_ME__254835CD foreign key (oi_medico)
	      references dbo.MED03_MEDICOS (oi_medico) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_DETAL__OI_ME__254835CD ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_DETAL__oi_co__226BC922'   ) 
 BEGIN  
	 alter table dbo.MED04_DETALLE_HIST
	   add constraint FK__MED_DETAL__oi_co__226BC922 foreign key (oi_consulta_per)
	      references dbo.MED04_HIST_CLINICA (oi_consulta_per) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_DETAL__oi_co__226BC922 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_DETAL__oi_lu__235FED5B'   ) 
 BEGIN  
	 alter table dbo.MED04_DETALLE_HIST
	   add constraint FK__MED_DETAL__oi_lu__235FED5B foreign key (oi_lugar_atencion)
	      references dbo.MED07_LUGARES_ATEN (oi_lugar_atencion) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_DETAL__oi_lu__235FED5B ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_DETAL__oi_ti__24541194'   ) 
 BEGIN  
	 alter table dbo.MED04_DETALLE_HIST
	   add constraint FK__MED_DETAL__oi_ti__24541194 foreign key (oi_tipo_domicilio)
	      references dbo.PER09_TIPOS_DOMIC (oi_tipo_domicilio) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_DETAL__oi_ti__24541194',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_EXA_P__oi_ex__19A178F7'   ) 
 BEGIN  
	 alter table dbo.MED04_EXAMEN_PER
	   add constraint FK__MED_EXA_P__oi_ex__19A178F7 foreign key (oi_examen)
	      references dbo.MED10_EXAMENES (oi_examen) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_EXA_P__oi_ex__19A178F7 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_MED04_EX_REFERENCE_MED04_PE'   ) 
 BEGIN  
	 alter table dbo.MED04_EXAMEN_PER
	   add constraint FK_MED04_EX_REFERENCE_MED04_PE foreign key (oi_personal_emp)
	      references dbo.MED04_PERSONAL (oi_personal_emp) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK_MED04_EX_REFERENCE_MED04_PE ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_FAC_E__oi_ex__391A2450'   ) 
 BEGIN  
	 alter table dbo.MED04_FACTORES_EVA
	   add constraint FK__MED_FAC_E__oi_ex__391A2450 foreign key (oi_examen_per)
	      references dbo.MED04_EXAMEN_PER (oi_examen_per) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_FAC_E__oi_ex__391A2450 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_FAC_E__oi_fa__3A0E4889'   ) 
 BEGIN  
	 alter table dbo.MED04_FACTORES_EVA
	   add constraint FK__MED_FAC_E__oi_fa__3A0E4889 foreign key (oi_factor_examen)
	      references dbo.MED10_FACTORES (oi_factor_examen) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_FAC_E__oi_fa__3A0E4889 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_HIST___oi_en__53CE1A8C'   ) 
 BEGIN  
	 alter table dbo.MED04_HIST_CLINICA
	   add constraint FK__MED_HIST___oi_en__53CE1A8C foreign key (oi_enfermedad)
	      references dbo.MED06_ENFERMEDADES (oi_enfermedad) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_HIST___oi_en__53CE1A8C',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_HIST___oi_mo__50F1ADE1'   ) 
 BEGIN  
	 alter table dbo.MED04_HIST_CLINICA
	   add constraint FK__MED_HIST___oi_mo__50F1ADE1 foreign key (oi_motivo_consulta)
	      references dbo.MED05_MOTIVOS_CONS (oi_motivo_consulta) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_HIST___oi_mo__50F1ADE1 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_MED04_HI_REFERENCE_MED04_PE'   ) 
 BEGIN  
	 alter table dbo.MED04_HIST_CLINICA
	   add constraint FK_MED04_HI_REFERENCE_MED04_PE foreign key (oi_personal_emp)
	      references dbo.MED04_PERSONAL (oi_personal_emp) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK_MED04_HI_REFERENCE_MED04_PE ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_co__16900222'   ) 
 BEGIN  
	 alter table dbo.MED04_MEDIC_SUMIN
	   add constraint FK__MED_MEDIC__oi_co__16900222 foreign key (oi_consulta_per)
	      references dbo.MED04_HIST_CLINICA (oi_consulta_per) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_MEDIC__oi_co__16900222',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_me__1784265B'   ) 
 BEGIN  
	 alter table dbo.MED04_MEDIC_SUMIN
	   add constraint FK__MED_MEDIC__oi_me__1784265B foreign key (oi_medicamento)
	      references dbo.MED08_MEDICAMENTOS (oi_medicamento) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_MEDIC__oi_me__1784265B ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_MEDIC__oi_mo__18784A94'   ) 
 BEGIN  
	 alter table dbo.MED04_MEDIC_SUMIN
	   add constraint FK__MED_MEDIC__oi_mo__18784A94 foreign key (oi_motivo_med)
	      references dbo.MED09_MOTIVOS_MED (oi_motivo_med) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK__MED_MEDIC__oi_mo__18784A94 ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_MED04_PE_REFERENCE_PER02_PE'   ) 
 BEGIN  
	 alter table dbo.MED04_PERSONAL
	   add constraint FK_MED04_PE_REFERENCE_PER02_PE foreign key (oi_personal_emp)
	      references dbo.PER02_PERSONAL_EMP (oi_personal_emp) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK_MED04_PE_REFERENCE_PER02_PE ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_EXAME__oi_ca__17B93085'   ) 
 BEGIN  
	 alter table dbo.MED10_EXAMENES
	   add constraint FK__MED_EXAME__oi_ca__17B93085 foreign key (oi_categ_examen)
	      references dbo.MED11_CATEG_EXAMEN (oi_categ_examen) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_EXAME__oi_ca__17B93085',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_MED10_EX_REFERENCE_MED10_TI'   ) 
 BEGIN  
	 alter table dbo.MED10_EXAMENES
	   add constraint FK_MED10_EX_REFERENCE_MED10_TI foreign key (oi_tipo_examen)
	      references dbo.MED10_TIPOS_EXAMEN (oi_tipo_examen) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint FK_MED10_EX_REFERENCE_MED10_TI ',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_FACTO__oi_ex__3731DBDE'   ) 
 BEGIN  
	 alter table dbo.MED10_FACTORES
	   add constraint FK__MED_FACTO__oi_ex__3731DBDE foreign key (oi_examen)
	      references dbo.MED10_EXAMENES (oi_examen) 
	
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_FACTO__oi_ex__3731DBDE',@@error , '' 
END 
 
 IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__MED_ANTEC__oi_ti__48076225'   ) 
 BEGIN  
	 alter table dbo.MED12_ANTECEDENTES
	   add constraint FK__MED_ANTEC__oi_ti__48076225 foreign key (oi_tipo_antec)
	      references dbo.MED12_TIPOS_ANTEC (oi_tipo_antec) 
	
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD Constraint  FK__MED_ANTEC__oi_ti__48076225',@@error , '' 
END 

exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 




