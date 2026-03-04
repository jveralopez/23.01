Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'NUCLEUSSUITE.BASE.CLI.CREATES_CLIENTE', @n_Version = 1 , @d_modulo ='NUCLEUSSUITE' 
EXEC dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)   
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Comienza la ejecucion ',0 , ''


IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo  ,@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR El script ya corrio'
	return
END
--//////////////////////////////////////////////////////////////////////////////////////////////--

/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_ACTIV_CLI') 
BEGIN

create table dbo.CLI01_ACTIV_CLI (
   oi_activ_cli         int                  not null,
   oi_cliente           int                  not null,
   oi_actividad         int                  not null,
   constraint PK_CLI01_ACTIV_CLI primary key (oi_activ_cli),
   constraint AK_KEY_2_CLI01_ACT_CLI01_AC unique (oi_cliente, oi_actividad))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_ACTIV_CLI',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_CCOSTO   ') 
BEGIN

create table dbo.CLI01_CCOSTO (
   oi_ccosto            int                  not null,
   oi_cliente           int                  null,
   c_ccosto             varchar(30)          not null,
   d_ccosto             varchar(100)         not null,
   o_ccosto             varchar(1000)        null,
   constraint PK_CLI01_CCOSTO primary key (oi_ccosto),
   constraint AK_CLI01_CCOSTO unique (oi_cliente, c_ccosto, d_ccosto))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_CCOSTO ',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_CLIENTES ') 
BEGIN

create table dbo.CLI01_CLIENTES (
   oi_cliente           int                  not null,
   c_cliente            varchar(30)          not null,
   d_razon_social       varchar(100)         not null,
   d_nom_fantasia       varchar(100)         null,
   oi_grupo_emp         int                  null,
   oi_actividad         int                  not null,
   c_cuit               varchar(30)          not null,
   c_estado             varchar(30)          null,
   oi_motivo_cambio_est int                  null,
   d_sitio_web          varchar(100)         null,
   oi_ejecutivo_alta    int                  not null,
   oi_ejecutivo_asignado int                  not null,
   oi_ult_ejecutivo_contacto int                  not null,
   f_alta               datetime             null,
   l_corporativa        smallint             null,
   l_multinacional      smallint             null,
   l_centralizada       smallint             null,
   oi_tam_comp          int                  null,
   e_cant_emp_int       int                  null,
   oi_tipo_dom          int                  null,
   l_temporada          smallint             null,
   c_mes_desde          varchar(30)          null,
   c_mes_hasta          varchar(30)          null,
   d_calle_legal        varchar(100)         null,
   c_nro_legal          varchar(30)          null,
   c_piso_legal         varchar(30)          null,
   c_dpto_legal         varchar(30)          null,
   oi_localidad         int                  null,
   c_postal             varchar(30)          null,
   n_pais_legal         numeric(4)           null,
   n_area_legal         numeric(4)           null,
   n_tel_legal          numeric(15)          null,
   n_pais_fax_legal     numeric(4)           null,
   n_area_fax_legal     numeric(4)           null,
   n_fax_legal          numeric(15)          null,
   d_comen_dom          varchar(100)         null,
   oi_cond_iva          int                  null,
   oi_cond_ig           int                  null,
   oi_cond_ib           int                  null,
   oi_juris_ib          int                  null,
   c_nro_ib             varchar(30)          null,
   l_retencion          smallint             null,
   l_percepcion         smallint             null,
   l_inf_bienes_per     smallint             null,
   n_pais_fax           numeric(4)           null,
   n_area_fax           numeric(4)           null,
   n_fax                numeric(15)          null,
   n_pais_1             numeric(4)           null,
   n_area_1             numeric(4)           null,
   n_tel_1              numeric(15)          null,
   n_pais_2             numeric(4)           null,
   n_area_2             numeric(4)           null,
   n_tel_2              numeric(15)          null,
   n_pais_3             numeric(4)           null,
   n_area_3             numeric(4)           null,
   n_tel_3              numeric(15)          null,
   n_fact_anual         numeric(11,3)        null,
   o_cliente            varchar(1000)        null,
   constraint PK_CLI01_CLIENTES primary key (oi_cliente),
   constraint AK_KEY_2_CLI01_CL_CLI01_CL unique (c_cliente),
   constraint AK_Razon_Social unique (d_razon_social))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_CLIENTES',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_COMPETIDORES') 
 Begin 

create table dbo.CLI01_COMPETIDORES (
   oi_competidor_cli    int                  not null,
   oi_cliente           int                  not null,
   oi_competidor        int                  not null,
   oi_cond_pago         int                  null,
   l_promociones        smallint             null,
   l_merchandising      smallint             null,
   l_eventuales         smallint             null,
   l_busq_directa       smallint             null,
   n_coef_ev            numeric(11,3)        null,
   t_beneficios         varchar(4000)        null,
   constraint PK_CLI01_COMPETIDORES primary key (oi_competidor_cli),
   constraint AK_CLI01_COMPETIDORES unique (oi_cliente, oi_competidor))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_COMPETIDORES',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_CONT_CLIENTE') 
 Begin 

create table dbo.CLI01_CONT_CLIENTE (
   oi_contacto_cli      int                  not null,
   oi_cliente           int                  not null,
   oi_contacto          int                  not null,
   oi_tipo_cont         int                  not null,
   c_prioridad          varchar(30)          not null,
   c_estado             varchar(30)          not null,
   l_recibir_info       smallint             null,
   oi_dom_cliente       int                  null,
   oi_puesto            int                  null,
   n_pais_lab           numeric(4)           null,
   n_area_lab           numeric(4)           null,
   n_telefono_lab       numeric(15)          null,
   d_email              varchar(100)         not null,
   d_asistente          varchar(100)         null,
   n_pais_as            numeric(4)           null,
   n_area_as            numeric(4)           null,
   n_telefono_as        numeric(15)          null,
   f_utl_contacto       datetime             null,
   constraint PK_CLI01_CONT_CLIENTE primary key (oi_contacto_cli),
   constraint AK_CLI01_CONT_CLIENTE unique (oi_cliente, oi_contacto))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_CONT_CLIENTE',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_DOC_DIG_CLI') 
BEGIN

create table dbo.CLI01_DOC_DIG_CLI (
   oi_doc_dig_cli       int                  not null,
   oi_cliente           int                  not null,
   oi_doc_digital       int                  not null,
   d_doc_dig_cli        varchar(100)         not null,
   f_alta               datetime             not null,
   o_doc_dig_cli        varchar(1000)        null,
   constraint PK_CLI01_DOC_DIG_CLI primary key (oi_doc_dig_cli),
   constraint ak_doc_dig_cli unique (oi_doc_digital))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_DOC_DIG_CLI',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_DOMICILIOS ') 
BEGIN

create table dbo.CLI01_DOMICILIOS (
   oi_dom_cliente       int                  not null,
   oi_cliente           int                  not null,
   c_dom_cliente        varchar(30)          not null,
   d_dom_cliente        varchar(100)         not null,
   c_estado             varchar(30)          not null,
   n_pais               numeric(4)           null,
   n_area               numeric(4)           null,
   n_telefono           numeric(15)          null,
   d_calle              varchar(100)         not null,
   c_numero             varchar(30)          not null,
   c_piso               varchar(30)          null,
   c_dpto               varchar(30)          null,
   oi_localidad         int                  not null,
   c_postal             varchar(30)          not null,
   d_domicilio          varchar(100)         null,
   c_afip               varchar(30)          null,
   oi_juris_afip        int                  null,
   oi_juris_ib          int                  null,
   o_dom_cliente        varchar(1000)        null,
   constraint PK_CLI01_DOMICILIOS primary key (oi_dom_cliente),
   constraint AK_KEY_2_CLI01_DO_CLI01_DO unique (c_dom_cliente, oi_cliente))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_DOMICILIOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_ENTR_CONT_CL') 
 Begin 

create table dbo.CLI01_ENTR_CONT_CL (
   oi_entr_cont_cl      int                  not null,
   oi_contacto_cli      int                  null,
   f_entrevista         datetime             not null,
   f_duracion           datetime             null,
   d_ubicacion          varchar(100)         null,
   t_notas              varchar(4000)        null,
   constraint PK_CLI01_ENTR_CONT_CL primary key (oi_entr_cont_cl),
   constraint AK_KEY_2_CLI01_EN_CLI01_EN unique (f_entrevista, oi_contacto_cli))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_ENTR_CONT_CL',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_LLAM_CONT_CL') 
 Begin 

create table dbo.CLI01_LLAM_CONT_CL (
   oi_llam_cont_cl      int                  not null,
   oi_contacto_cli      int                  not null,
   f_llamada            datetime             not null,
   t_notas              varchar(4000)        null,
   constraint PK_CLI01_LLAM_CONT_CL primary key (oi_llam_cont_cl),
   constraint AK_CLI01_LL_CLI01_LL unique (f_llamada, oi_contacto_cli))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_LLAM_CONT_CL',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_MARKETING') 
BEGIN

create table dbo.CLI01_MARKETING (
   oi_cli_market        int                  not null,
   oi_cliente           int                  not null,
   oi_tipos_serv        int                  not null,
   e_cant_perm          int                  null,
   e_cant_ev            int                  null,
   n_jornal_hora        numeric(11,3)        null,
   n_rem_mes            numeric(11,3)        null,
   constraint PK_CLI01_MARKETING primary key (oi_cli_market),
   constraint AK_CLI01_MA_CLI01_MA unique (oi_cliente, oi_tipos_serv))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_MARKETING',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_OBRAS_SOC') 
BEGIN

create table dbo.CLI01_OBRAS_SOC (
   oi_obra_soc_cli      int                  not null,
   oi_cliente           int                  not null,
   oi_obra_social       int                  not null,
   constraint PK_CLI01_OBRAS_SOC primary key (oi_obra_soc_cli))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_OBRAS_SOC',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_REF_BANCOS ') 
BEGIN

create table dbo.CLI01_REF_BANCOS (
   oi_ref_bancos        int                  not null,
   oi_cliente           int                  not null,
   oi_banco             int                  not null,
   oi_sucursal          int                  null,
   c_cuenta             varchar(30)          not null,
   nombre_contacto      varchar(100)         null,
   apellido_contacto    varchar(100)         null,
   n_pais               numeric(4)           null,
   n_area               numeric(4)           null,
   n_telefono           numeric(15)          null,
   c_estado             varchar(30)          null,
   constraint PK_CLI01_REF_BANCOS primary key (oi_ref_bancos),
   constraint AK_KEY_2_CLI01_RE_CLI01_RE unique (c_cuenta))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_REF_BANCOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_REF_COMER') 
BEGIN

create table dbo.CLI01_REF_COMER (
   oi_ref_comer         int                  not null,
   oi_cliente           int                  not null,
   d_empresa            varchar(100)         not null,
   d_nombre_cont        varchar(100)         not null,
   d_apellido_cont      varchar(100)         not null,
   d_cargo              varchar(100)         null,
   n_pais               numeric(4)           null,
   n_area               numeric(4)           null,
   n_telefono           numeric(15)          null,
   c_estado             varchar(30)          null,
   constraint PK_CLI01_REF_COMER primary key (oi_ref_comer),
   constraint AK_KEY_2_CLI01_RERC unique (oi_cliente, d_nombre_cont, d_apellido_cont))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_REF_COMER',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_SEGMENTACION') 
 Begin 

create table dbo.CLI01_SEGMENTACION (
   oi_segmentacion      int                  not null,
   oi_rubro             int                  not null,
   oi_cliente           int                  not null,
   constraint PK_CLI01_SEGMENTACION primary key (oi_segmentacion),
   constraint AK_CLI01_SEGMENTAC_CLI01_SE unique (oi_rubro, oi_cliente))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_SEGMENTACION',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI01_SINDICATOS ') 
BEGIN

create table dbo.CLI01_SINDICATOS (
   oi_sindicato_cli     int                  not null,
   oi_cliente           int                  not null,
   oi_sindicato         int                  not null,
   constraint PK_CLI01_SINDICATOS primary key (oi_sindicato_cli),
   constraint AK_KEY_2_CLI01_SI_CLI01_SI unique (oi_cliente, oi_sindicato))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI01_SINDICATOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI02_CONTACTOS') 
BEGIN

create table dbo.CLI02_CONTACTOS (
   oi_contacto          int                  not null,
   c_contacto           varchar(30)          not null,
   d_apellido           varchar(100)         not null,
   d_nombres            varchar(100)         not null,
   d_ape_y_nom          varchar(100)         not null,
   n_pais               numeric(4)           null,
   n_area               numeric(4)           null,
   n_telefono           numeric(15)          null,
   n_pais_cel           numeric(4)           null,
   n_area_cel           numeric(4)           null,
   n_tel_cel            numeric(15)          null,
   d_calle              varchar(100)         null,
   c_nro                varchar(30)          null,
   c_piso               varchar(30)          null,
   c_depto              varchar(30)          null,
   oi_localidad         int                  null,
   c_postal             varchar(30)          null,
   d_email              varchar(100)         null,
   t_particularidades   varchar(4000)        null,
   f_nacim              datetime             null,
   f_cumpleanos         datetime             null,
   e_cant_hijos         int                  null,
   n_tiempo_resid       numeric(11,3)        null,
   oi_unidad_tiempo     int                  null,
   oi_nivel_desicion    int                  null,
   oi_nivel_econom      int                  null,
   oi_nivel_cultural    int                  null,
   d_tipologia          varchar(1000)        null,
   d_inf_socio          varchar(1000)        null,
   constraint PK_CLI02_CONTACTOS primary key (oi_contacto),
   constraint AK_KEY_2_CLIE02_CONT unique (c_contacto))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI02_CONTACTOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI02_HOBBIES_CONT') 
 Begin 

create table dbo.CLI02_HOBBIES_CONT (
   oi_cont_hobby        int                  not null,
   oi_contacto          int                  not null,
   oi_hobby             int                  not null,
   constraint PK_CLI02_HOBBIES_CONT primary key (oi_cont_hobby),
   constraint AK_KEY_2_CLI02_HO_CLI02_HO unique (oi_contacto, oi_hobby))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI02_HOBBIES_CONT',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_CLIENTE_EMP') 
BEGIN

create table dbo.CLI03_CLIENTE_EMP (
   oi_cliente_emp       int                  not null,
   oi_cliente           int                  not null,
   oi_empresa           int                  not null,
   oi_ejecutivo         int                  not null,
   c_estado             varchar(30)          not null,
   oi_motivo_cambio_est int                  null,
   l_fact_hab_aut       smallint             null,
   d_corte_fact         varchar(100)         null,
   d_orden_pfact        varchar(100)         null,
   c_comen_fact         varchar(100)         null,
   oi_cond_pago         int                  null,
   n_porc_os            numeric(11,3)        null,
   n_porc_ss            numeric(11,3)        null,
   c_modelo_fact        varchar(30)          null,
   l_fig_fec_asig_rec   smallint             null,
   n_coef_bas           numeric(11,3)        null,
   f_ult_fec_act_coef   datetime             null,
   oi_cli_ticke         int                  null,
   oi_grupo_cli         int                  null,
   n_porc_os_jub        numeric(11,3)        null,
   l_salto_pag_pref     smallint             null,
   n_porc_ss_jub        numeric(11,3)        null,
   o_cliente_emp        varchar(1000)        null,
   constraint PK_CLI03_CLIENTE_EMP primary key (oi_cliente_emp),
   constraint AK_KEY_2_CLI03_CL_CLI03_CL unique (oi_cliente_emp, oi_cliente, oi_empresa))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_CLIENTE_EMP',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_CLI_EMP_TS ') 
BEGIN

create table dbo.CLI03_CLI_EMP_TS (
   oi_cliente_emp_ts    int                  not null,
   oi_cliente_emp       int                  not null,
   oi_tipos_serv        int                  not null,
   oi_ejecutivo         int                  null,
   oi_ult_ej_solicitud  int                  null,
   f_ult_solicitud      datetime             null,
   n_monto_solicitado   numeric(11,3)        null,
   n_monto_credito      numeric(11,3)        null,
   f_monto              datetime             null,
   d_usuario_ultimo_estado varchar(100)         null,
   e_monto_disp         int                  null,
   o_credito            varchar(1000)        null,
   constraint PK_CLI03_CLI_EMP_TS primary key (oi_cliente_emp_ts),
   constraint AK_CLI_EMP_TS_CLI03_CL unique (oi_cliente_emp, oi_tipos_serv))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_CLI_EMP_TS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_DOC_DIG_CLEM') 
 Begin 

create table dbo.CLI03_DOC_DIG_CLEM (
   oi_doc_dig_clem      int                  not null,
   oi_cliente_emp       int                  not null,
   oi_doc_digital       int                  not null,
   d_docum_dig_cv       varchar(100)         not null,
   o_docum_dig_cv       varchar(1000)        null,
   constraint PK_DOC_DIG_CLEM primary key nonclustered (oi_doc_dig_clem),
   constraint ak_doc_dig_clem unique (oi_cliente_emp, oi_doc_digital))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_DOC_DIG_CLEM',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_DOMIC_EXP') 
BEGIN

create table dbo.CLI03_DOMIC_EXP (
   oi_domic_exp         int                  not null,
   oi_cliente_emp       int                  not null,
   c_domic_exp          varchar(30)          not null,
   oi_domic_fact        int                  not null,
   oi_dom_cliente       int                  not null,
   oi_ejecutivo         int                  not null,
   oi_ubicacion         int                  null,
   codigo_afip          varchar(30)          null,
   o_domic_exp          varchar(1000)        null,
   constraint PK_CLI03_DOMIC_EXP primary key (oi_domic_exp),
   constraint AK_KEY_2_CLI03_DO_CLI03_DO unique (oi_cliente_emp, oi_dom_cliente))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_DOMIC_EXP',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_DOMIC_FACT ') 
BEGIN

create table dbo.CLI03_DOMIC_FACT (
   oi_domic_fact        int                  not null,
   oi_cliente_emp       int                  not null,
   oi_dom_cliente       int                  not null,
   oi_contacto          int                  null,
   o_domic_fact         varchar(1000)        null,
   constraint PK_CLI03_DOMIC_FACT primary key (oi_domic_fact),
   constraint AK_KEY_2_CLI03_DO_FAC unique (oi_cliente_emp, oi_dom_cliente))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_DOMIC_FACT',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_DOM_EXP_TS ') 
BEGIN

create table dbo.CLI03_DOM_EXP_TS (
   oi_dom_exp_ts        int                  not null,
   oi_tipos_serv        int                  not null,
   oi_domic_exp         int                  not null,
   oi_ejecutivo         int                  not null,
   constraint PK_CLI03_DOM_EXP_TS primary key (oi_dom_exp_ts),
   constraint AK_CLI03_DOM_EXP_T_CLI03_DO unique (oi_tipos_serv, oi_domic_exp, oi_ejecutivo))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_DOM_EXP_TS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI03_EJEC_ADIC') 
BEGIN

create table dbo.CLI03_EJEC_ADIC (
   oi_ejec_adic         int                  not null,
   oi_dom_exp_ts        int                  not null,
   oi_ejecutivo         int                  not null,
   d_division           varchar(100)         not null,
   constraint PK_CLI03_EJEC_ADIC primary key (oi_ejec_adic),
   constraint AK_CLI03_EJEC_ADIC_CLI03_EJ unique (oi_ejec_adic, oi_dom_exp_ts, oi_ejecutivo, d_division))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI03_EJEC_ADIC',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI04_COND_IVA ') 
BEGIN

create table dbo.CLI04_COND_IVA (
   oi_cond_iva          int                  not null,
   c_cond_iva           varchar(30)          not null,
   d_cond_iva           varchar(100)         not null,
   c_tpo_formulario     varchar(30)          null,
   l_discrimina         smallint             null,
   n_porc_insc          numeric(11,3)        null,
   n_porc_no_insc       numeric(11,3)        null,
   e_cant_copias        int                  null,
   constraint PK_CLI04_COND_IVA primary key (oi_cond_iva),
   constraint AK_CLI04_CO_CLI04_CO unique (c_cond_iva))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI04_COND_IVA',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI05_COND_IG  ') 
BEGIN

create table dbo.CLI05_COND_IG (
   oi_cond_ig           int                  not null,
   c_cond_ig            varchar(30)          not null,
   d_cond_ig            varchar(100)         not null,
   constraint PK_CLI05_COND_IG primary key (oi_cond_ig),
   constraint AK_KEY_2_CLI05_CO_CLI05_CO unique (c_cond_ig))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI05_COND_IG',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI06_COND_IB  ') 
BEGIN

create table dbo.CLI06_COND_IB (
   oi_cond_ib           int                  not null,
   c_cond_ib            varchar(30)          not null,
   d_cond_ib            varchar(100)         not null,
   constraint PK_CLI06_COND_IB primary key (oi_cond_ib),
   constraint AK_KEY_2_CLI06_CO_CLI06_CO unique (c_cond_ib))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI06_COND_IB',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI07_JURIS_IB ') 
BEGIN

create table dbo.CLI07_JURIS_IB (
   oi_juris_ib          int                  not null,
   c_juris_ib           varchar(30)          not null,
   d_juris_ib           varchar(100)         not null,
   n_juris_ib           numeric(11,3)        null,
   constraint PK_CLI07_JURIS_IB primary key (oi_juris_ib),
   constraint AK_KEY_2_CLI07_JU_CLI07_JU unique (c_juris_ib))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI07_JURIS_IB',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI08_GRUPOS_EMP ') 
BEGIN

create table dbo.CLI08_GRUPOS_EMP (
   oi_grupo_emp         int                  not null,
   c_grupo_emp          varchar(30)          not null,
   d_grupo_emp          varchar(100)         not null,
   constraint PK_CLI08_GRUPOS_EMP primary key (oi_grupo_emp),
   constraint AK_KEY_2_CLI08_GR_CLI08_GR unique (c_grupo_emp))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI08_GRUPOS_EMP',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI09_EJECUTIVOS ') 
BEGIN

create table dbo.CLI09_EJECUTIVOS (
   oi_ejecutivo         int                  not null,
   c_ejecutivo          varchar(30)          not null,
   d_ejecutivo          varchar(100)         not null,
   constraint PK_CLI09_EJECUTIVOS primary key (oi_ejecutivo),
   constraint AK_KEY_2_CLI09_EJ_CLI09_EJ unique (c_ejecutivo))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI09_EJECUTIVOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI10_GRUPOS_CLI ') 
BEGIN

create table dbo.CLI10_GRUPOS_CLI (
   oi_grupo_cli         int                  not null,
   c_grupo_cli          varchar(30)          not null,
   d_grupo_cli          varchar(100)         not null,
   constraint PK_CLI10_GRUPOS_CLI primary key (oi_grupo_cli),
   constraint AK_CLI11_CAD_BASE_CLI10_GR unique (c_grupo_cli))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI10_GRUPOS_CLI',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI11_CADENAS  ') 
BEGIN

create table dbo.CLI11_CADENAS (
   oi_cadena            int                  not null,
   oi_cadena_base       int                  null,
   c_cadena             varchar(30)          not null,
   d_cadena             varchar(100)         not null,
   constraint PK_CLI11_CADENAS primary key nonclustered (oi_cadena),
   constraint AK_KEY_2_CLI11_CA_CLI11_CA unique (oi_cadena_base, c_cadena))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI11_CADENAS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI11_CAD_BASE ') 
BEGIN

create table dbo.CLI11_CAD_BASE (
   oi_cadena_base       int                  not null,
   c_cadena_base        varchar(30)          not null,
   d_cadena_base        varchar(100)         not null,
   constraint PK_CLI11_CAD_BASE primary key nonclustered (oi_cadena_base),
   constraint ak_cli11_cad_base unique (c_cadena_base))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI11_CAD_BASE',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI11_PDV      ') 
BEGIN

create table dbo.CLI11_PDV (
   oi_punto_venta       int                  not null,
   oi_cadena            int                  not null,
   c_punto_venta        varchar(30)          not null,
   d_punto_venta        varchar(100)         not null,
   oi_tipo_pdv          int                  not null,
   oi_localidad         int                  not null,
   d_calle              varchar(100)         not null,
   c_numero             varchar(30)          not null,
   c_piso               varchar(30)          null,
   c_departamento       varchar(30)          null,
   constraint PK_CLI11_PDV primary key nonclustered (oi_punto_venta),
   constraint AK_KEY_2_CLI11_PD_CLI11_PD unique (oi_cadena, c_punto_venta))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI11_PDV',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI12_RUBROS   ') 
BEGIN

create table dbo.CLI12_RUBROS (
   oi_rubro             int                  not null,
   c_rubro              varchar(30)          not null,
   d_rubro              varchar(100)         not null,
   constraint PK_CLI12_RUBROS primary key (oi_rubro),
   constraint AK_KEY_2_CLI12_RU_CLI12_RU unique (c_rubro))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI12_RUBROS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI13_TIPOS_PDV') 
BEGIN

create table dbo.CLI13_TIPOS_PDV (
   oi_tipo_pdv          int                  not null,
   c_tipo_pdv           varchar(30)          not null,
   d_tipo_pdv           varchar(100)         not null,
   constraint PK_CLI13_TIPOS_PDV primary key nonclustered (oi_tipo_pdv),
   constraint AK_KEY_2_CLI13_TI_CLI13_TI unique (c_tipo_pdv))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI13_TIPOS_PDV',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI14_BANCOS   ') 
BEGIN

create table dbo.CLI14_BANCOS (
   oi_banco             int                  not null,
   c_banco              varchar(30)          not null,
   d_banco              varchar(100)         not null,
   c_clave_unif         varchar(30)          null,
   o_banco              varchar(1000)        null,
   constraint PK_CLI14_BANCOS primary key (oi_banco),
   constraint AK_KEY_2_CLI14_BA_CLI14_BA unique (c_banco, d_banco))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI14_BANCOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI14_BANCOS_SUCUR') 
 Begin 

create table dbo.CLI14_BANCOS_SUCUR (
   oi_sucursal          int                  not null,
   oi_banco             int                  null,
   c_sucursal           varchar(30)          not null,
   d_sucursal           varchar(100)         not null,
   c_suc_bancaria       varchar(30)          null,
   o_sucursal           varchar(1000)        null,
   constraint PK_CLI14_BANCOS_SUCUR primary key (oi_sucursal),
   constraint AK_BSUC_CLI1_CLI14_BA unique (c_sucursal, oi_banco, d_sucursal))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI14_BANCOS_SUCUR',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI15_TIPOS_SERV ') 
BEGIN

create table dbo.CLI15_TIPOS_SERV (
   oi_tipos_serv        int                  not null,
   c_tipos_serv         varchar(30)          not null,
   d_tipos_serv         varchar(100)         not null,
   constraint PK_CLI15_TIPOS_SERV primary key (oi_tipos_serv),
   constraint AK_CLI15_TIPO_SERV_CLI15_TI unique (c_tipos_serv))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI15_TIPOS_SERV',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI16_CLI_TICKET ') 
BEGIN

create table dbo.CLI16_CLI_TICKET (
   oi_cli_ticket        int                  not null,
   c_cli_ticket         varchar(30)          not null,
   d_cli_ticket         varchar(100)         not null,
   constraint PK_CLI16_CLI_TICKET primary key (oi_cli_ticket),
   constraint AK_BINARIOS_CLI16_CL unique (oi_cli_ticket, c_cli_ticket),
   constraint AK_CLI16_CL_CLI16_CL unique (c_cli_ticket))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI16_CLI_TICKET',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI17_DIAS_PAGO') 
BEGIN

create table dbo.CLI17_DIAS_PAGO (
   oi_dia_pago          int                  not null,
   c_dia_pago           varchar(30)          not null,
   d_dia_pago           varchar(100)         not null,
   constraint PK_CLI17_DIAS_PAGO primary key nonclustered (oi_dia_pago),
   constraint AK_KEY_CLI17_DIAS_PAGO unique (c_dia_pago))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI17_DIAS_PAGO',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI18_NIVELES_DEC') 
BEGIN

create table dbo.CLI18_NIVELES_DEC (
   oi_nivel_decision    int                  not null,
   c_nivel_decision     varchar(30)          not null,
   d_nivel_decision     varchar(100)         not null,
   constraint PK_CLI18_NIVELES_DEC primary key (oi_nivel_decision),
   constraint AK_NIVELES_DECISIO_CLI18_NI unique (c_nivel_decision))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI18_NIVELES_DEC',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI19_TIPOS_CONT ') 
BEGIN

create table dbo.CLI19_TIPOS_CONT (
   oi_tipos_cont        int                  not null,
   c_tipos_cont         varchar(30)          not null,
   d_tipos_cont         varchar(100)         not null,
   constraint PK_CLI19_TIPOS_CONT primary key (oi_tipos_cont),
   constraint AK_CLI19_TI_CLI19_TI unique (c_tipos_cont))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI19_TIPOS_CONT',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI20_NIVELES_CUL') 
BEGIN

create table dbo.CLI20_NIVELES_CUL (
   oi_nivel_cultural    int                  not null,
   c_nivel_cultural     varchar(30)          not null,
   d_nivel_cultural     varchar(100)         not null,
   constraint PK_CLI20_NIVELES_CUL primary key (oi_nivel_cultural),
   constraint AK_KEY_2_CLI20_NI_CLI20_NI unique (c_nivel_cultural))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI20_NIVELES_CUL',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI21_NIVELES_ECO') 
BEGIN

create table dbo.CLI21_NIVELES_ECO (
   oi_nivel_econom      int                  not null,
   c_nivel_econom       varchar(30)          not null,
   d_nivel_econom       varchar(100)         not null,
   constraint PK_CLI21_NIVELES_ECO primary key (oi_nivel_econom),
   constraint AK_KEY_2_CLI21_NI_CLI21_NI unique (c_nivel_econom))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI21_NIVELES_ECO',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI22_HOBBIES  ') 
BEGIN

create table dbo.CLI22_HOBBIES (
   oi_hobby             int                  not null,
   c_hobby              varchar(30)          not null,
   d_hobby              varchar(100)         not null,
   constraint PK_CLI22_HOBBIES primary key (oi_hobby),
   constraint AK_KEY_2_CLI24_CO_CLI22_HO unique (c_hobby))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI22_HOBBIES',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI23_PUESTOS_EXT') 
BEGIN

create table dbo.CLI23_PUESTOS_EXT (
   oi_puesto            int                  not null,
   c_puesto             varchar(30)          not null,
   d_puesto             varchar(100)         not null,
   l_convenio           smallint             null,
   oi_banda_sal         int                  null,
   oi_categoria         int                  null,
   o_puesto             varchar(1000)        null,
   constraint PK_CLI23_PUESTOS_EXT primary key (oi_puesto),
   constraint AK1_ORG_PUESTOS_CLI23_PU unique (c_puesto))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI23_PUESTOS_EXT',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI24_COND_PAGO') 
BEGIN

create table dbo.CLI24_COND_PAGO (
   oi_cond_pago         int                  not null,
   c_cond_pago          varchar(30)          not null,
   d_cond_pago          varchar(100)         not null,
   e_cant_dias          int                  null,
   constraint PK_CLI24_COND_PAGO primary key nonclustered (oi_cond_pago),
   constraint AK_KEY_2_CLI24_CO_CLI24_CO unique (c_cond_pago))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI24_COND_PAGO',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI25_BINARIOS ') 
BEGIN

create table dbo.CLI25_BINARIOS (
   oi_doc_digital       int                  not null,
   c_bloque             int                  not null,
   t_datos              varchar(4000)        null,
   constraint AK_BINARIOS_CLI25_BI unique (oi_doc_digital, c_bloque))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI25_BINARIOS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI25_DOC_DIGITAL') 
BEGIN

create table dbo.CLI25_DOC_DIGITAL (
   oi_doc_digital       int                  not null,
   e_tamanio            int                  null,
   f_creacion           datetime             null,
   o_doc_digital        varchar(1000)        null,
   constraint PK_Documento_Digital_CLI primary key nonclustered (oi_doc_digital))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI25_DOC_DIGITAL',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI26_MOT_CAMB_EST') 
 Begin 

create table dbo.CLI26_MOT_CAMB_EST (
   oi_motivo_cambio_est int                  not null,
   c_motivo_cambio_est  varchar(30)          not null,
   d_motivo_cambio_est  varchar(100)         not null,
   o_motivo_cambio_est  varchar(1000)        null,
   constraint PK_CLI26_MOT_CAMB_EST primary key (oi_motivo_cambio_est),
   constraint AK_CLI26_MOT_CAMB_EST unique (c_motivo_cambio_est))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI26_MOT_CAMB_EST',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI27_TAM_COMP ') 
BEGIN

create table dbo.CLI27_TAM_COMP (
   oi_tam_comp          int                  not null,
   c_tam_comp           varchar(30)          not null,
   d_tam_comp           varchar(100)         not null,
   constraint PK_CLI27_TAM_COMP primary key (oi_tam_comp),
   constraint AK_TAM_COMP unique (c_tam_comp))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI27_TAM_COMP',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI28_COMPETIDORES') 
 Begin 

create table dbo.CLI28_COMPETIDORES (
   oi_competidor        int                  not null,
   c_competidor         varchar(30)          not null,
   d_competidor         varchar(100)         not null,
   constraint PK_CLI28_COMPETIDORES primary key (oi_competidor),
   constraint AK_COMPETIDOR unique (c_competidor))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI28_COMPETIDORES',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI29_SUC_COMER') 
BEGIN

create table dbo.CLI29_SUC_COMER (
   oi_suc_comer         int                  not null,
   c_suc_comer          varchar(30)          not null,
   d_suc_comer          varchar(100)         not null,
   c_estado             varchar(30)          not null,
   constraint PK_CLI29_SUC_COMER primary key (oi_suc_comer),
   constraint AK_CLI29_SUC_COMER_CLI29_SU unique (c_suc_comer, d_suc_comer))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI29_SUC_COMER',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI30_MOT_CAM_CRED') 
 Begin 

create table dbo.CLI30_MOT_CAM_CRED (
   oi_mot_cam_cred      int                  not null,
   c_mot_cam_cred       varchar(30)          not null,
   d_mot_cam_cred       varchar(100)         not null,
   o_mot_cam_cred       varchar(1000)        null,
   constraint PK_CLI30_MOT_CAM_CRED primary key (oi_mot_cam_cred),
   constraint AK_CLI30_MOT_CAM_C_CLI30_MO unique (c_mot_cam_cred, d_mot_cam_cred))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI30_MOT_CAM_CRED',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI31_EMPRESAS ') 
BEGIN

create table dbo.CLI31_EMPRESAS (
   oi_empresa           int                  not null,
   o_empresa            varchar(1000)        null,
   constraint PK_CLI31_EMPRESAS primary key nonclustered (oi_empresa))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI31_EMPRESAS',@@error , '' 
 END 


/*==============================================================*/
IF NOT EXISTS(select *  from sysobjects nolock where name ='CLI31_UBICACIONES') 
BEGIN

create table dbo.CLI31_UBICACIONES (
   oi_ubicacion         int                  not null,
   oi_juris_ib          int                  null,
   oi_suc_comer         int                  null,
   oi_ejecutivo         int                  null,
   org_oi_empresa       int                  null,
   n_num_activa         numeric(11,3)        null,
   constraint PK_CLI_UBICACIONES primary key (oi_ubicacion))
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table CLI31_UBICACIONES',@@error , '' 
 END 



--//////////////////////////////////////////////////////////////////////////////////////////////--


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_ACTIV_CLI' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_AC_REFERENCE_ACCLI' ) )
BEGIN
alter table dbo.CLI01_ACTIV_CLI
   add constraint FK_CLI01_AC_REFERENCE_ACCLI foreign key (oi_actividad)
      references ORG34_ACTIVIDADES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_AC_REFERENCE_ACCLI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_ACTIV_CLI' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_AC_FK_CLI01__CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_ACTIV_CLI
   add constraint FK_CLI01_AC_FK_CLI01__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_AC_FK_CLI01__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CCOSTO' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CC_FK_CLI01__CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_CCOSTO
   add constraint FK_CLI01_CC_FK_CLI01__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CC_FK_CLI01__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_REFERENCE_ACT' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_REFERENCE_ACT foreign key (oi_actividad)
      references ORG34_ACTIVIDADES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_REFERENCE_ACT' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI04_CO' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI04_CO foreign key (oi_cond_iva)
      references dbo.CLI04_COND_IVA (oi_cond_iva)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_TTA04_PE_REFERENCE_TTA12_ME' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI05_CO' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI05_CO foreign key (oi_cond_ig)
      references dbo.CLI05_COND_IG (oi_cond_ig)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI05_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI06_CO' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI06_CO foreign key (oi_cond_ib)
      references dbo.CLI06_COND_IB (oi_cond_ib)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI06_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI07_JU' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI07_JU foreign key (oi_juris_ib)
      references dbo.CLI07_JURIS_IB (oi_juris_ib)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI07_JU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI08_GR' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI08_GR foreign key (oi_grupo_emp)
      references dbo.CLI08_GRUPOS_EMP (oi_grupo_emp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI08_GR' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI09_EJ' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI09_EJ foreign key (oi_ejecutivo_alta)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_REFERENCE_CLI09_EJ1' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_REFERENCE_CLI09_EJ1 foreign key (oi_ejecutivo_asignado)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_REFERENCE_CLI09_EJ1' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_REFERENCE_CLI09_EJ2' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_REFERENCE_CLI09_EJ2 foreign key (oi_ult_ejecutivo_contacto)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_REFERENCE_CLI09_EJ2' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI26_MO' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI26_MO foreign key (oi_motivo_cambio_est)
      references dbo.CLI26_MOT_CAMB_EST (oi_motivo_cambio_est)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI26_MO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_FK_CLI01__CLI27_TA' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_FK_CLI01__CLI27_TA foreign key (oi_tam_comp)
      references dbo.CLI27_TAM_COMP (oi_tam_comp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_FK_CLI01__CLI27_TA' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_REFERENCE_ORG19_LOC' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_REFERENCE_ORG19_LOC foreign key (oi_localidad)
      references ORG19_LOCALIDADES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_REFERENCE_ORG19_LOC' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CLIENTES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CL_REFERENCE_TDOM' ) )
BEGIN

alter table dbo.CLI01_CLIENTES
   add constraint FK_CLI01_CL_REFERENCE_TDOM foreign key (oi_tipo_dom)
      references ORG33_TIPOS_DOMIC

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CL_REFERENCE_TDOM' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_COMPETIDORES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CO_FK_CLI01__CLI24_CO' ) )
BEGIN

alter table dbo.CLI01_COMPETIDORES
   add constraint FK_CLI01_CO_FK_CLI01__CLI24_CO foreign key (oi_cond_pago)
      references dbo.CLI24_COND_PAGO (oi_cond_pago)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CO_FK_CLI01__CLI24_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_COMPETIDORES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CO_FK_CLI01__CLI28_CO' ) )
BEGIN

alter table dbo.CLI01_COMPETIDORES
   add constraint FK_CLI01_CO_FK_CLI01__CLI28_CO foreign key (oi_competidor)
      references dbo.CLI28_COMPETIDORES (oi_competidor)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CO_FK_CLI01__CLI28_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_COMPETIDORES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_COM_REFERENCE_CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_COMPETIDORES
   add constraint FK_CLI01_COM_REFERENCE_CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_COM_REFERENCE_CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CONT_CLIENTE' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CO_FK_CLI01__CLI01_DO' ) )
BEGIN

alter table dbo.CLI01_CONT_CLIENTE
   add constraint FK_CLI01_CO_FK_CLI01__CLI01_DO foreign key (oi_dom_cliente)
      references dbo.CLI01_DOMICILIOS (oi_dom_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CO_FK_CLI01__CLI01_DO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CONT_CLIENTE' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CO_FK_CLI01__CLI02_CO' ) )
BEGIN

alter table dbo.CLI01_CONT_CLIENTE
   add constraint FK_CLI01_CO_FK_CLI01__CLI02_CO foreign key (oi_contacto)
      references dbo.CLI02_CONTACTOS (oi_contacto)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CO_FK_CLI01__CLI02_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CONT_CLIENTE' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CO_FK_CLI01__CLI19_TI' ) )
BEGIN

alter table dbo.CLI01_CONT_CLIENTE
   add constraint FK_CLI01_CO_FK_CLI01__CLI19_TI foreign key (oi_tipo_cont)
      references dbo.CLI19_TIPOS_CONT (oi_tipos_cont)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CO_FK_CLI01__CLI19_TI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CONT_CLIENTE' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CO_REFERENCE_PU' ) )
BEGIN

alter table dbo.CLI01_CONT_CLIENTE
   add constraint FK_CLI01_CO_REFERENCE_PU foreign key (oi_puesto)
      references dbo.CLI23_PUESTOS_EXT (oi_puesto)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CO_REFERENCE_PU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_CONT_CLIENTE' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_CON_REFERENCE_CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_CONT_CLIENTE
   add constraint FK_CLI01_CON_REFERENCE_CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_CON_REFERENCE_CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_DOC_DIG_CLI' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_DO_FK_CLI01__CLI01_DOCDIG' ) )
BEGIN

alter table dbo.CLI01_DOC_DIG_CLI
   add constraint FK_CLI01_DO_FK_CLI01__CLI01_DOCDIG foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_DO_FK_CLI01__CLI01_DOCDIG' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_DOC_DIG_CLI' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_DOC_DIG_CLI_DOC_DIG' ) )
BEGIN

alter table dbo.CLI01_DOC_DIG_CLI
   add constraint FK_DOC_DIG_CLI_DOC_DIG foreign key (oi_doc_digital)
      references dbo.CLI25_DOC_DIGITAL (oi_doc_digital)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_DOC_DIG_CLI_DOC_DIG' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_DOMICILIOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_DO_FK_CLI01__CLI01_DOM' ) )
BEGIN

alter table dbo.CLI01_DOMICILIOS
   add constraint FK_CLI01_DO_FK_CLI01__CLI01_DOM foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_DO_FK_CLI01__CLI01_DOM' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_DOMICILIOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_DO_FK_CLI01__CLI07_JU' ) )
BEGIN

alter table dbo.CLI01_DOMICILIOS
   add constraint FK_CLI01_DO_FK_CLI01__CLI07_JU foreign key (oi_juris_ib)
      references dbo.CLI07_JURIS_IB (oi_juris_ib)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_DO_FK_CLI01__CLI07_JU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_DOMICILIOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_DO_REFERENCE_JUAFIP' ) )
BEGIN

alter table dbo.CLI01_DOMICILIOS
   add constraint FK_CLI01_DO_REFERENCE_JUAFIP foreign key (oi_juris_afip)
      references ORG36_JURIS_AFIP

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_DO_REFERENCE_JUAFIP' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_DOMICILIOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_DO_REFERENCE_LOC' ) )
BEGIN

alter table dbo.CLI01_DOMICILIOS
   add constraint FK_CLI01_DO_REFERENCE_LOC foreign key (oi_localidad)
      references ORG19_LOCALIDADES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_DO_REFERENCE_LOC' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_ENTR_CONT_CL' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_EN_FK_CLI01__CLI01_CO' ) )
BEGIN

alter table dbo.CLI01_ENTR_CONT_CL
   add constraint FK_CLI01_EN_FK_CLI01__CLI01_CO foreign key (oi_contacto_cli)
      references dbo.CLI01_CONT_CLIENTE (oi_contacto_cli)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_EN_FK_CLI01__CLI01_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_LLAM_CONT_CL' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_LL_FK_CLI01__CLI01_CO' ) )
BEGIN

alter table dbo.CLI01_LLAM_CONT_CL
   add constraint FK_CLI01_LL_FK_CLI01__CLI01_CO foreign key (oi_contacto_cli)
      references dbo.CLI01_CONT_CLIENTE (oi_contacto_cli)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_LL_FK_CLI01__CLI01_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_MARKETING' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_MA_FK_CLI01__CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_MARKETING
   add constraint FK_CLI01_MA_FK_CLI01__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_MA_FK_CLI01__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_MARKETING' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_MA_FK_CLI01__CLI15_TI' ) )
BEGIN

alter table dbo.CLI01_MARKETING
   add constraint FK_CLI01_MA_FK_CLI01__CLI15_TI foreign key (oi_tipos_serv)
      references dbo.CLI15_TIPOS_SERV (oi_tipos_serv)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_MA_FK_CLI01__CLI15_TI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_OBRAS_SOC' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_OB_FK_CLI01__CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_OBRAS_SOC
   add constraint FK_CLI01_OB_FK_CLI01__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_OB_FK_CLI01__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_OBRAS_SOC' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_OB_REFERENCE_OS' ) )
BEGIN

alter table dbo.CLI01_OBRAS_SOC
   add constraint FK_CLI01_OB_REFERENCE_OS foreign key (oi_obra_social)
      references PER06_OBRAS_SOC

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_OB_REFERENCE_OS' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_REF_BANCOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_RE_FK_CLI01__CLI14_BA' ) )
BEGIN

alter table dbo.CLI01_REF_BANCOS
   add constraint FK_CLI01_RE_FK_CLI01__CLI14_BA foreign key (oi_banco)
      references dbo.CLI14_BANCOS (oi_banco)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_RE_FK_CLI01__CLI14_BA' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_REF_BANCOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_RE_REFERENCE_ORG04C_BS' ) )
BEGIN

alter table dbo.CLI01_REF_BANCOS
   add constraint FK_CLI01_RE_REFERENCE_ORG04C_BS foreign key (oi_sucursal)
      references dbo.CLI14_BANCOS_SUCUR (oi_sucursal)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_RE_REFERENCE_ORG04C_BS' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_REF_BANCOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_REB_REFERENCE_CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_REF_BANCOS
   add constraint FK_CLI01_REB_REFERENCE_CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_REB_REFERENCE_CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_REF_COMER' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_REC_REFERENCE_CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_REF_COMER
   add constraint FK_CLI01_REC_REFERENCE_CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_REC_REFERENCE_CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_SEGMENTACION' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_SE_FK_CLI01__CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_SEGMENTACION
   add constraint FK_CLI01_SE_FK_CLI01__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_SE_FK_CLI01__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_SEGMENTACION' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_SE_FK_CLI01__CLI12_RU' ) )
BEGIN

alter table dbo.CLI01_SEGMENTACION
   add constraint FK_CLI01_SE_FK_CLI01__CLI12_RU foreign key (oi_rubro)
      references dbo.CLI12_RUBROS (oi_rubro)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_SE_FK_CLI01__CLI12_RU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_SINDICATOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_SI_REFERENCE_CLI' ) )
BEGIN

alter table dbo.CLI01_SINDICATOS
   add constraint FK_CLI01_SI_REFERENCE_CLI foreign key (oi_sindicato)
      references PER30_SINDICATOS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_SI_REFERENCE_CLI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI01_SINDICATOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI01_SI_FK_CLI01__CLI01_CL' ) )
BEGIN

alter table dbo.CLI01_SINDICATOS
   add constraint FK_CLI01_SI_FK_CLI01__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI01_SI_FK_CLI01__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_CONTACTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_CO_FK_CLI02__CLI18_NI' ) )
BEGIN

alter table dbo.CLI02_CONTACTOS
   add constraint FK_CLI02_CO_FK_CLI02__CLI18_NI foreign key (oi_nivel_desicion)
      references dbo.CLI18_NIVELES_DEC (oi_nivel_decision)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_CO_FK_CLI02__CLI18_NI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_CONTACTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_CO_REFERENCE_ORG25_UN' ) )
BEGIN

alter table dbo.CLI02_CONTACTOS
   add constraint FK_CLI02_CO_REFERENCE_ORG25_UN foreign key (oi_unidad_tiempo)
      references dbo.ORG25_UNIDADES_TPO (oi_unidad_tiempo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_CO_REFERENCE_ORG25_UN' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_CONTACTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_CO_FK_CLI02__CLI20_NI' ) )
BEGIN

alter table dbo.CLI02_CONTACTOS
   add constraint FK_CLI02_CO_FK_CLI02__CLI20_NI foreign key (oi_nivel_cultural)
      references dbo.CLI20_NIVELES_CUL (oi_nivel_cultural)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_CO_FK_CLI02__CLI20_NI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_CONTACTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_CO_FK_CLI02__CLI21_NI' ) )
BEGIN

alter table dbo.CLI02_CONTACTOS
   add constraint FK_CLI02_CO_FK_CLI02__CLI21_NI foreign key (oi_nivel_econom)
      references dbo.CLI21_NIVELES_ECO (oi_nivel_econom)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_CO_FK_CLI02__CLI21_NI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_CONTACTOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_CO_REFERENCE_LOC' ) )
BEGIN

alter table dbo.CLI02_CONTACTOS
   add constraint FK_CLI02_CO_REFERENCE_LOC foreign key (oi_localidad)
      references ORG19_LOCALIDADES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_CO_REFERENCE_LOC' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_HOBBIES_CONT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_HO_FK_CLI02__CLI02_CO' ) )
BEGIN

alter table dbo.CLI02_HOBBIES_CONT
   add constraint FK_CLI02_HO_FK_CLI02__CLI02_CO foreign key (oi_contacto)
      references dbo.CLI02_CONTACTOS (oi_contacto)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_HO_FK_CLI02__CLI02_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI02_HOBBIES_CONT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI02_HO_FK_CLI02__CLI22_HO' ) )
BEGIN

alter table dbo.CLI02_HOBBIES_CONT
   add constraint FK_CLI02_HO_FK_CLI02__CLI22_HO foreign key (oi_hobby)
      references dbo.CLI22_HOBBIES (oi_hobby)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI02_HO_FK_CLI02__CLI22_HO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI01_CL' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CL_FK_CLI03__CLI01_CL foreign key (oi_cliente)
      references dbo.CLI01_CLIENTES (oi_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI01_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI10_GR' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CL_FK_CLI03__CLI10_GR foreign key (oi_grupo_cli)
      references dbo.CLI10_GRUPOS_CLI (oi_grupo_cli)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI10_GR' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI16_CL' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CL_FK_CLI03__CLI16_CL foreign key (oi_cli_ticke)
      references dbo.CLI16_CLI_TICKET (oi_cli_ticket)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI16_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI24_CO' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CL_FK_CLI03__CLI24_CO foreign key (oi_cond_pago)
      references dbo.CLI24_COND_PAGO (oi_cond_pago)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI24_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI26_MO' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CL_FK_CLI03__CLI26_MO foreign key (oi_motivo_cambio_est)
      references dbo.CLI26_MOT_CAMB_EST (oi_motivo_cambio_est)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI26_MO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_REFERENCE_EMP' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CL_REFERENCE_EMP foreign key (oi_empresa)
      references ORG03_EMPRESAS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_REFERENCE_EMP' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLIENTE_EMP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CLEM_REFERENCE_CLI09_EJ' ) )
BEGIN

alter table dbo.CLI03_CLIENTE_EMP
   add constraint FK_CLI03_CLEM_REFERENCE_CLI09_EJ foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CLEM_REFERENCE_CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLI_EMP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI03_CL' ) )
BEGIN

alter table dbo.CLI03_CLI_EMP_TS
   add constraint FK_CLI03_CL_FK_CLI03__CLI03_CL foreign key (oi_cliente_emp)
      references dbo.CLI03_CLIENTE_EMP (oi_cliente_emp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI03_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLI_EMP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI09_EJ' ) )
BEGIN

alter table dbo.CLI03_CLI_EMP_TS
   add constraint FK_CLI03_CL_FK_CLI03__CLI09_EJ foreign key (oi_ult_ej_solicitud)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLI_EMP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CL_FK_CLI03__CLI15_TI' ) )
BEGIN

alter table dbo.CLI03_CLI_EMP_TS
   add constraint FK_CLI03_CL_FK_CLI03__CLI15_TI foreign key (oi_tipos_serv)
      references dbo.CLI15_TIPOS_SERV (oi_tipos_serv)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CL_FK_CLI03__CLI15_TI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_CLI_EMP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_CLTS_REFERENCE_CLI09_EJ' ) )
BEGIN

alter table dbo.CLI03_CLI_EMP_TS
   add constraint FK_CLI03_CLTS_REFERENCE_CLI09_EJ foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_CLTS_REFERENCE_CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOC_DIG_CLEM' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_FK_CLI03__CLI25_DO' ) )
BEGIN

alter table dbo.CLI03_DOC_DIG_CLEM
   add constraint FK_CLI03_DO_FK_CLI03__CLI25_DO foreign key (oi_doc_digital)
      references dbo.CLI25_DOC_DIGITAL (oi_doc_digital)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_FK_CLI03__CLI25_DO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOC_DIG_CLEM' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DOC_REFERENCE_CLI03_CL' ) )
BEGIN

alter table dbo.CLI03_DOC_DIG_CLEM
   add constraint FK_CLI03_DOC_REFERENCE_CLI03_CL foreign key (oi_cliente_emp)
      references dbo.CLI03_CLIENTE_EMP (oi_cliente_emp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DOC_REFERENCE_CLI03_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_EXP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_REFERENCE_CLI03_DOFAC' ) )
BEGIN

alter table dbo.CLI03_DOMIC_EXP
   add constraint FK_CLI03_DO_REFERENCE_CLI03_DOFAC foreign key (oi_domic_fact)
      references dbo.CLI03_DOMIC_FACT (oi_domic_fact)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_REFERENCE_CLI03_DOFAC' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_EXP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_FK_CLI03__CLI09_EJ' ) )
BEGIN

alter table dbo.CLI03_DOMIC_EXP
   add constraint FK_CLI03_DO_FK_CLI03__CLI09_EJ foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_FK_CLI03__CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_EXP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_REFERENCE_EXP' ) )
BEGIN

alter table dbo.CLI03_DOMIC_EXP
   add constraint FK_CLI03_DO_REFERENCE_EXP foreign key (oi_ubicacion)
      references ORG03_UBICACIONES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_REFERENCE_EXP' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_EXP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DOEX_REFERENCE_CLI01_DO' ) )
BEGIN

alter table dbo.CLI03_DOMIC_EXP
   add constraint FK_CLI03_DOEX_REFERENCE_CLI01_DO foreign key (oi_dom_cliente)
      references dbo.CLI01_DOMICILIOS (oi_dom_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DOEX_REFERENCE_CLI01_DO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_EXP' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DOMEXP_REFERENCE_CLI03_CL' ) )
BEGIN

alter table dbo.CLI03_DOMIC_EXP
   add constraint FK_CLI03_DOMEXP_REFERENCE_CLI03_CL foreign key (oi_cliente_emp)
      references dbo.CLI03_CLIENTE_EMP (oi_cliente_emp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DOMEXP_REFERENCE_CLI03_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_FACT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_FK_CLI03__CLI02_CO' ) )
BEGIN

alter table dbo.CLI03_DOMIC_FACT
   add constraint FK_CLI03_DO_FK_CLI03__CLI02_CO foreign key (oi_contacto)
      references dbo.CLI02_CONTACTOS (oi_contacto)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_FK_CLI03__CLI02_CO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_FACT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_FK_CLI03__CLI03_CL' ) )
BEGIN

alter table dbo.CLI03_DOMIC_FACT
   add constraint FK_CLI03_DO_FK_CLI03__CLI03_CL foreign key (oi_cliente_emp)
      references dbo.CLI03_CLIENTE_EMP (oi_cliente_emp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_FK_CLI03__CLI03_CL' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOMIC_FACT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DOFA_REFERENCE_CLI01_DO' ) )
BEGIN

alter table dbo.CLI03_DOMIC_FACT
   add constraint FK_CLI03_DOFA_REFERENCE_CLI01_DO foreign key (oi_dom_cliente)
      references dbo.CLI01_DOMICILIOS (oi_dom_cliente)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DOFA_REFERENCE_CLI01_DO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOM_EXP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_REFERENCE_CLI03_DOEXP' ) )
BEGIN

alter table dbo.CLI03_DOM_EXP_TS
   add constraint FK_CLI03_DO_REFERENCE_CLI03_DOEXP foreign key (oi_domic_exp)
      references dbo.CLI03_DOMIC_EXP (oi_domic_exp)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_REFERENCE_CLI03_DOEXP' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOM_EXP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_FK_CLI03_CLI09_EJTS' ) )
BEGIN

alter table dbo.CLI03_DOM_EXP_TS
   add constraint FK_CLI03_DO_FK_CLI03_CLI09_EJTS foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_FK_CLI03_CLI09_EJTS' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_DOM_EXP_TS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_DO_FK_CLI03__CLI15_TI' ) )
BEGIN

alter table dbo.CLI03_DOM_EXP_TS
   add constraint FK_CLI03_DO_FK_CLI03__CLI15_TI foreign key (oi_tipos_serv)
      references dbo.CLI15_TIPOS_SERV (oi_tipos_serv)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_DO_FK_CLI03__CLI15_TI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_EJEC_ADIC' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_EJ_FK_CLI03__CLI03_DO' ) )
BEGIN

alter table dbo.CLI03_EJEC_ADIC
   add constraint FK_CLI03_EJ_FK_CLI03__CLI03_DO foreign key (oi_dom_exp_ts)
      references dbo.CLI03_DOM_EXP_TS (oi_dom_exp_ts)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_EJ_FK_CLI03__CLI03_DO' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI03_EJEC_ADIC' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI03_EJ_FK_CLI03__CLI09_EJ' ) )
BEGIN

alter table dbo.CLI03_EJEC_ADIC
   add constraint FK_CLI03_EJ_FK_CLI03__CLI09_EJ foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI03_EJ_FK_CLI03__CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI11_CADENAS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI11_CAD_CLI11_CB' ) )
BEGIN

alter table dbo.CLI11_CADENAS
   add constraint FK_CLI11_CAD_CLI11_CB foreign key (oi_cadena_base)
      references dbo.CLI11_CAD_BASE (oi_cadena_base)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI11_CAD_CLI11_CB' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI11_PDV' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI11_PD_FK_CLI11__CLI11_CA' ) )
BEGIN

alter table dbo.CLI11_PDV
   add constraint FK_CLI11_PD_FK_CLI11__CLI11_CA foreign key (oi_cadena)
      references dbo.CLI11_CADENAS (oi_cadena)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI11_PD_FK_CLI11__CLI11_CA' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI11_PDV' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI11_PD_FK_CLI11__CLI13_TI' ) )
BEGIN

alter table dbo.CLI11_PDV
   add constraint FK_CLI11_PD_FK_CLI11__CLI13_TI foreign key (oi_tipo_pdv)
      references dbo.CLI13_TIPOS_PDV (oi_tipo_pdv)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI11_PD_FK_CLI11__CLI13_TI' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI11_PDV' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI11_PD_REFERENCE_LOC' ) )
BEGIN

alter table dbo.CLI11_PDV
   add constraint FK_CLI11_PD_REFERENCE_LOC foreign key (oi_localidad)
      references ORG19_LOCALIDADES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI11_PD_REFERENCE_LOC' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI14_BANCOS_SUCUR' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI14_BA_FK_CLI14__CLI14_BA' ) )
BEGIN

alter table dbo.CLI14_BANCOS_SUCUR
   add constraint FK_CLI14_BA_FK_CLI14__CLI14_BA foreign key (oi_banco)
      references dbo.CLI14_BANCOS (oi_banco)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI14_BA_FK_CLI14__CLI14_BA' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI23_PUESTOS_EXT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI23_PU_REFERENCE_BS' ) )
BEGIN

alter table dbo.CLI23_PUESTOS_EXT
   add constraint FK_CLI23_PU_REFERENCE_BS foreign key (oi_banda_sal)
      references ORG17_BANDAS_SAL

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI23_PU_REFERENCE_BS' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI23_PUESTOS_EXT' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI23_PU_REFERENCE_CA' ) )
BEGIN

alter table dbo.CLI23_PUESTOS_EXT
   add constraint FK_CLI23_PU_REFERENCE_CA foreign key (oi_categoria)
      references ORG18_CATEGORIAS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI23_PU_REFERENCE_CA' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI25_BINARIOS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI_BINARIOS__DOC_DIG' ) )
BEGIN

alter table dbo.CLI25_BINARIOS
   add constraint FK_CLI_BINARIOS__DOC_DIG foreign key (oi_doc_digital)
      references dbo.CLI25_DOC_DIGITAL (oi_doc_digital)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI_BINARIOS__DOC_DIG' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_EMPRESAS' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI31_EM_REFERENCE_EM' ) )
BEGIN

alter table dbo.CLI31_EMPRESAS
   add constraint FK_CLI31_EM_REFERENCE_EM foreign key (oi_empresa)
      references ORG03_EMPRESAS

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI31_EM_REFERENCE_EM' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI31_UB_FK_CLI31__CLI07_JU' ) )
BEGIN

alter table dbo.CLI31_UBICACIONES
   add constraint FK_CLI31_UB_FK_CLI31__CLI07_JU foreign key (oi_juris_ib)
      references dbo.CLI07_JURIS_IB (oi_juris_ib)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI31_UB_FK_CLI31__CLI07_JU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI31_UB_FK_CLI31__CLI09_EJ' ) )
BEGIN

alter table dbo.CLI31_UBICACIONES
   add constraint FK_CLI31_UB_FK_CLI31__CLI09_EJ foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI31_UB_FK_CLI31__CLI09_EJ' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI31_UB_FK_CLI31__CLI29_SU' ) )
BEGIN

alter table dbo.CLI31_UBICACIONES
   add constraint FK_CLI31_UB_FK_CLI31__CLI29_SU foreign key (oi_suc_comer)
      references dbo.CLI29_SUC_COMER (oi_suc_comer)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI31_UB_FK_CLI31__CLI29_SU' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI31_UB_FK_CLI31__CLI31_EM' ) )
BEGIN

alter table dbo.CLI31_UBICACIONES
   add constraint FK_CLI31_UB_FK_CLI31__CLI31_EM foreign key (org_oi_empresa)
      references dbo.CLI31_EMPRESAS (oi_empresa)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI31_UB_FK_CLI31__CLI31_EM' , @@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id 
in (SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CLI31_UB_REFERENCE_UB' ) )
BEGIN

alter table dbo.CLI31_UBICACIONES
   add constraint FK_CLI31_UB_REFERENCE_UB foreign key (oi_ubicacion)
      references ORG03_UBICACIONES

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_CLI31_UB_REFERENCE_UB' , @@error , ''
END
 


--///////////////////////////////////////////////////////////////////////////////////////////////--
exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



