/******************************************************************************
 *** INICIO ARCHIVO: BASE.POS.Creates.sql
 ******************************************************************************/

Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
declare @cant int 

select @ScripName = 'BASE.POS.Creates', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT




select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version  and d_script=@ScripName 
select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR el scrip ya corrio'
	return
END





EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Comienza la ejecucion ',0 , ''
	


/*********************   Validacion de Tablas y COnstraints creados ***********************************/

 Select @cant = Count(*) from SYSOBJECTS (NOLOCK) WHERE NAME IN (  'POS01_AFJP_CV'                                                               
 ,  'POS01_ANTEC_CV' ,  'POS01_COMPET_CV' ,  'POS01_CONOC_CV'                                                                            
 ,  'POS01_CURSOS_CV' ,  'POS01_CV' ,  'POS01_DOCUM_CV' ,  'POS01_DOC_DIG_CV'                                                            
 ,  'POS01_DOMIC_CV' ,  'POS01_ENTREVISTAS' ,  'POS01_ESTUDIOS_CV' ,  'POS01_FLIARES_CV'                                                 
 ,  'POS01_HRS_CV' ,  'POS01_IDIOMAS_CV' ,  'POS01_IMAGENES_CV' ,  'POS01_LIB_SANIT'                                                     
 ,  'POS01_POSTU_CV' ,  'POS01_PREF_CV' ,  'POS01_REFERENCIAS' ,  'POS01_TEST_CV'                                                        
 ,  'POS02_BINARIOS' ,  'POS02_DOC_DIG' ,  'POS03_OFERTAS_LAB' ,  'POS04_SENIORITY'                                                      
 ,  'POS05_AREAS_LAB' ,  'POS06_TPO_ENTRE' ,  'POS07_RES_MEDICO' ,  'POS08_MOTIV_BUSQ'                                                   
 ,  'POS09_RES_POLICIAL' ,  'POS10_TEST_AMB' ,  'POS11_TESTS' ,  'POS12_RES_TEST'                                                        
 ,  'POS14_PREFERENCIAS' ,  'POS15_CONT_WEB' ,  'POS16_CONOCIMIENTOS' ,  'POS17_BUSQUEDAS'                                               
 ,  'POS17_RESUL_BUSQ' ,  'POS18_COLOR_OJOS' ,  'POS19_COLOR_PELO' ,  'POS20_TIPO_CUTIS'                                                 
 ,  'POS21_TIPO_MOVI' ,  'POS22_TIPO_LICENC' ,  'POS23_TIPO_REMUN' ,  'POS24_TIPO_RELA')                                                 
if @cant > 0 
BEGIN                                                                                            
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Ya existe una Tabla de Igual nombre',@@error , ''	 
	Select 'ERROR Ya existe una Tabla de Igual nombre'
	 Select 'tabla duplicada : ' + name from SYSOBJECTS (NOLOCK) WHERE NAME IN (  'POS01_AFJP_CV'                                                               
	 ,  'POS01_ANTEC_CV' ,  'POS01_COMPET_CV' ,  'POS01_CONOC_CV'                                                                            
	 ,  'POS01_CURSOS_CV' ,  'POS01_CV' ,  'POS01_DOCUM_CV' ,  'POS01_DOC_DIG_CV'                                                            
	 ,  'POS01_DOMIC_CV' ,  'POS01_ENTREVISTAS' ,  'POS01_ESTUDIOS_CV' ,  'POS01_FLIARES_CV'                                                 
	 ,  'POS01_HRS_CV' ,  'POS01_IDIOMAS_CV' ,  'POS01_IMAGENES_CV' ,  'POS01_LIB_SANIT'                                                     
	 ,  'POS01_POSTU_CV' ,  'POS01_PREF_CV' ,  'POS01_REFERENCIAS' ,  'POS01_TEST_CV'                                                        
	 ,  'POS02_BINARIOS' ,  'POS02_DOC_DIG' ,  'POS03_OFERTAS_LAB' ,  'POS04_SENIORITY'                                                      
	 ,  'POS05_AREAS_LAB' ,  'POS06_TPO_ENTRE' ,  'POS07_RES_MEDICO' ,  'POS08_MOTIV_BUSQ'                                                   
	 ,  'POS09_RES_POLICIAL' ,  'POS10_TEST_AMB' ,  'POS11_TESTS' ,  'POS12_RES_TEST'                                                        
	 ,  'POS14_PREFERENCIAS' ,  'POS15_CONT_WEB' ,  'POS16_CONOCIMIENTOS' ,  'POS17_BUSQUEDAS'                                               
	 ,  'POS17_RESUL_BUSQ' ,  'POS18_COLOR_OJOS' ,  'POS19_COLOR_PELO' ,  'POS20_TIPO_CUTIS'                                                 
	 ,  'POS21_TIPO_MOVI' ,  'POS22_TIPO_LICENC' ,  'POS23_TIPO_REMUN' ,  'POS24_TIPO_RELA')                                                 
	
	return

END





BEGIN
/*==============================================================*/
/* Table: POS01_AFJP_CV                                         */
/*==============================================================*/
create table dbo.POS01_AFJP_CV (
   oi_afjp_cv           int                  not null,
   oi_cv                int                  not null,
   oi_afjp              int                  null,
   f_ingreso            datetime             not null,
   f_egreso             datetime             null,
   c_jubilacion         varchar(30)          null,
   o_egreso_afjp        varchar(1000)        null,
   -- ***   constraint PK_POS_AFJP_CV primary key (oi_afjp_cv),
   -- ***   constraint AK_POS_AFJP_POS unique (f_ingreso, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_AFJP_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_ANTEC_CV                                        */
/*==============================================================*/
create table dbo.POS01_ANTEC_CV (
   oi_antecedente_cv    int                  not null,
   oi_cv                int                  not null,
   d_empresa            varchar(100)         not null,
   d_puesto             varchar(100)         not null,
   f_ingreso            datetime             not null,
   f_egreso             datetime             null,
   oi_motivo_eg_per     int                  null,
   oi_unidad_tiempo     int                  null,
   e_experiencia        int                  null,
   o_tareas             varchar(1000)        null,
   d_domicilio_empresa  varchar(100)         null,
   te_empresa           varchar(50)          null,
   d_contacto           varchar(100)         null,
   n_ultimo_sueldo      numeric(11,3)        null,
   o_antecedente_cv     varchar(1000)        null,
   -- ***   constraint PK_POS_ANTECEDENTE primary key (oi_antecedente_cv),
   -- ***   constraint AK_POS_ANTECEDENTE unique (oi_cv, f_ingreso)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_ANTEC_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_COMPET_CV                                       */
/*==============================================================*/
create table dbo.POS01_COMPET_CV (
   oi_competencia_cv    int                  not null,
   oi_cv                int                  not null,
   oi_competencia       int                  null,
   c_nivel_comp         varchar(30)          not null,
   o_competencia        varchar(1000)        null,
   -- ***   constraint PK_COMPETENCIA_POS primary key nonclustered (oi_competencia_cv),
   -- ***   constraint AK_COMPETENCIA_POS unique (oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_COMPET_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_CONOC_CV                                        */
/*==============================================================*/
create table dbo.POS01_CONOC_CV (
   oi_conoc_cv          int                  not null,
   oi_cv                int                  null,
   oi_conocimiento      int                  null,
   c_nivel              varchar(30)          null      
   -- ***   constraint PK_POS01_CONOC_CV primary key (oi_conoc_cv),
   -- ***   constraint AK_AK_CONOC_CV_POS01_CO unique (oi_conocimiento, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_CONOC_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_CURSOS_CV                                       */
/*==============================================================*/
create table dbo.POS01_CURSOS_CV (
   oi_curso_ext_cv      int                  not null,
   oi_cv                int                  not null,
   oi_curso             int                  null,
   oi_unidad_tiempo     int                  null,
   d_curso_ext_cv       varchar(100)         not null,
   f_fin_curso          datetime             null,
   e_duracion           int                  null,
   d_lugar              varchar(100)         null,
   l_certificado        smallint             null,
   l_externo            smallint             null,
   o_curso              varchar(1000)        null,
   -- ***   constraint PK_POS01_CURSOS_CV primary key (oi_curso_ext_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_CURSOS_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_CV                                              */
/*==============================================================*/
create table dbo.POS01_CV (
   oi_cv                int                  not null,
   c_cv                 varchar(30)          not null,
   d_apellido           varchar(100)         not null,
   d_nombres            varchar(100)         not null,
   d_ape_y_nom          varchar(200)         null,
   d_apellido_materno   varchar(100)         null,
   c_nro_doc            varchar(30)          not null,
   c_sexo               varchar(30)          null,      
   f_nacim              datetime             null,
   c_nro_cuil           varchar(30)          null,
   c_codigo_postal      varchar(30)          null,
   d_calle              varchar(100)         null,
   c_nro                varchar(30)          null,
   c_piso               varchar(30)          null,
   c_departamento       varchar(30)          null,
   c_pais_cel           varchar(30)          null,
   c_area_cel           varchar(30)          null,
   te_celular           varchar(50)          null,
   te_celular_alt       varchar(50)          null,
   d_email              varchar(100)         null,
   d_email_alternativo  varchar(100)         null,
   f_casamiento         datetime             null,
   f_ingreso_pais       datetime             null,
   oi_foto              int                  null,
   oi_firma             int                  null,
   d_localidad_nac      varchar(100)         null,
   d_localidad          varchar(30)          null,
   oi_grupo_sanguineo   int                  null,
   l_jubilado           smallint             null,
   f_estado_jubi        datetime             null,
   c_nro_jubilacion     varchar(30)          null,
   f_desde_afjp         datetime             null,
   f_fallecimiento      datetime             null,
   o_cv                 varchar(1000)        null,
   c_nombre_usuario     varchar(30)          null,
   c_password           varchar(30)          null,
   c_estado             varchar(30)          null,
   c_tipo_cv            varchar(30)          null,
   l_profesional        smallint             not null,
   f_alta_cv            datetime             null,
   f_actualizacion      datetime             null,
   l_alta_web           smallint             not null,
   te_laboral           varchar(50)          null,
   te_mensaje           varchar(50)          null,
   oi_color_ojos        int                  null,
   oi_color_pelo        int                  null,
   l_cabello_largo      smallint             null,
   c_cadera             varchar(30)          null,
   c_cintura            varchar(30)          null,
   c_altura             varchar(30)          null,
   c_peso               varchar(30)          null,
   c_busto              varchar(30)          null,
   c_nro_camisa         varchar(30)          null,
   c_nro_pantalon       varchar(30)          null,
   c_nro_calzado        varchar(30)          null,
   oi_motivacion_busqueda int                  null,
   l_examen_medico      smallint             null,
   f_examen_medico      datetime             null,
   oi_resultado_medico  int                  null,
   l_inf_policial       smallint             null,
   f_inf_policial       datetime             null,
   oi_res_inf_policial  int                  null,
   l_test_ambiental     smallint             null,
   f_test_ambiental     datetime             null,
   oi_test_ambiental    int                  null,
   l_seguro_desem       smallint             null,
   f_seguro_desem       datetime             null,
   oi_tipo_movil        int                  null,
   d_marca_movil        varchar(100)         null,
   d_modelo_movil       varchar(100)         null,
   e_anio_movil         int                  null,
   l_licencia           smallint             null,
   oi_tipo_licencia     int                  null,
   oi_tipo_remun        int                  null,
   n_min_remun          numeric(11,3)        null,
   n_ideal_remun        numeric(11,3)        null,
   n_actual_remun       numeric(11,3)        null,
   f_disponibilidad     datetime             null,
   oi_tipo_relacion     int                  null,
   oi_cont_web          int                  null,
   oi_nacionalidad      int                  null,
   oi_localidad_nac     int                  null,
   oi_localidad         int                  null,
   oi_pais_nac          int                  null,
   oi_pais              int                  null,
   oi_provincia_nac     int                  null,
   oi_provincia         int                  null,
   oi_idioma            int                  null,
   oi_estado_civil      int                  null,
   oi_afjp              int                  null,
   oi_titulo            int                  null,
   oi_estado_jubi       int                  null,
   oi_tipo_documento    int                  null,
   oi_calificacion      int                  null,
   d_nom_con_eme        varchar(100)         null,
   c_pais_emer          varchar(30)          null,
   c_area_emer          varchar(30)          null,
   te_nro_emer          varchar(100)         null,
   c_pais               varchar(30)          null,
   c_area               varchar(30)          null,
   te_nro               varchar(100)         null,
   e_cant_hijos         int                  null,
   c_tipo_cv_interno    varchar(30)          null,      
   -- ***   constraint PK_POS01_CV primary key (oi_cv),
   -- ***   constraint AK_AK_KEY_2_POS01_CV_POS01_CV unique (c_nro_doc, l_alta_web)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_DOCUM_CV                                        */
/*==============================================================*/
create table dbo.POS01_DOCUM_CV (
   oi_documento_cv      int                  not null,
   oi_cv                int                  not null,
   oi_tipo_documento    int                  null,
	 oi_localidad			    int                  null,   
   c_documento          varchar(30)          not null,
   d_resp_expedicion    varchar(100)         null,
   d_origen_documento   varchar(100)         null,
   f_vencimiento_doc    datetime             null,
   o_documento_cv       varchar(1000)        null,
   -- ***   constraint PK_POS01_DOCUM_POS primary key (oi_documento_cv),
   -- ***   constraint AK_POS01_DOCUM_POS unique (oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_DOCUM_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_DOC_DIG_CV                                      */
/*==============================================================*/
create table dbo.POS01_DOC_DIG_CV (
   oi_docum_dig_cv      int                  not null,
   oi_cv                int                  not null,
   oi_doc_digital       int                  null,
   d_docum_dig_cv       varchar(100)         null,
   o_docum_dig_cv       varchar(1000)        null,
   -- ***   constraint PK_DOC_DIG_POS primary key nonclustered (oi_docum_dig_cv),
   -- ***   constraint AK_DOC_DIG_POS unique (oi_cv, oi_doc_digital)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_DOC_DIG_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_DOMIC_CV                                        */
/*==============================================================*/
create table dbo.POS01_DOMIC_CV (
   oi_domic_cv          int                  not null,
   oi_cv                int                  not null,
   oi_tipo_domicilio    int                  not null,
   oi_localidad         int                  null,
   d_calle              varchar(100)         not null,
   e_numero             int                  not null,
   d_piso               varchar(100)         null,
   d_departamento       varchar(100)         null,
   d_entre_calle_1      varchar(100)         null,
   d_entre_calle_2      varchar(100)         null,
   c_postal             varchar(30)          null,
   te_1                 varchar(50)          null,
   te_2                 varchar(50)          null,
   d_partido            varchar(100)         null,
   l_domic_fiscal       smallint             null,
   o_domicilio          varchar(1000)        null,
   -- ***   constraint PK_POS01_DOMIC_POS primary key (oi_domic_cv),
   -- ***   constraint AK_POS01_DOMIC_POS unique (oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_DOMIC_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_ENTREVISTAS                                     */
/*==============================================================*/
create table dbo.POS01_ENTREVISTAS (
   oi_entrevistas       int                  not null,
   oi_personal_emp      int                  null,
   oi_tipo_cutis        int                  null,
   oi_tipo_entrevistas  int                  null,
   oi_cv                int                  null,
   oi_sucursal          int                  null,
   f_entrevista         datetime             null,
   d_salud_cabello      varchar(100)         null,
   d_manos              varchar(100)         null,
   l_simpatica          smallint             null,
   e_fisico             int                  null,
   e_actitud            int                  null,
   d_ideal_para         varchar(100)         null,
   c_estado             varchar(30)          null,
   t_entrevista         varchar(4000)        null,
   -- ***   constraint PK_POS01_ENTREVISTAS primary key (oi_entrevistas),
   -- ***   constraint AK_KEY_2_POS01_EN unique (oi_cv, f_entrevista)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_ENTREVISTAS',@@error , ''	 


/*==============================================================*/
/* Table: POS01_ESTUDIOS_CV                                     */
/*==============================================================*/
create table dbo.POS01_ESTUDIOS_CV (
   oi_estudio_cv        int                  not null,
   oi_cv                int                  not null,
   f_ini_estudio        datetime             not null,
   f_fin_estudio        datetime             null,
   oi_estado_est        int                  null,
   oi_nivel_estudio     int                  null,
   oi_tipo_establecim   int                  null,
   l_est_extranjero     smallint             null,
   d_establ_educacional varchar(100)         null,
   e_duracion_estudio   int                  null,
   oi_estudio           int                  null,
   oi_unidad_tiempo     int                  null,
   e_periodo_en_curso   int                  null,
   e_cant_materias      int                  null,
   e_mat_adeudadas      int                  null,
   f_actualiz           datetime             null,
   n_promedio           numeric(11,3)        null,
   o_estudio            varchar(1000)        null,
   -- ***   constraint PK_POS01_ESTUDIOS_POS primary key (oi_estudio_cv),
   -- ***   constraint AK_POS01_ESTUDIOS_POS unique (oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_ESTUDIOS_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_FLIARES_CV                                      */
/*==============================================================*/
create table dbo.POS01_FLIARES_CV (
   oi_familiar_cv       int                  not null,
   oi_cv                int                  not null,
   oi_tipo_familiar     int                  not null,
   d_apellido           varchar(100)         not null,
   d_nombres            varchar(100)         not null,
   d_ape_y_nom          varchar(200)         not null,
   c_nro_documento      varchar(30)          null,
   c_sexo               varchar(30)          null,      
   c_nro_cuil           varchar(30)          null,
   f_nacimiento         datetime             null,
   l_vive               smallint             null,
   f_fallecimiento      datetime             null,
   l_discapacidad       smallint             null,
   o_ocupacion          varchar(1000)        null,
   oi_grado_escol       int                  null,
   oi_nivel_escol       int                  null,
   e_anio_inic_esc      int                  null,
   e_anio_fin_esc       int                  null,
   e_duracion_estudio   int                  null,
   e_periodo_en_curso   int                  null,
   oi_ocupacion_fam     int                  null,
   oi_estudio           int                  null,
   oi_nacionalidad      int                  null,
   oi_estado_civil      int                  null,
   oi_unidad_tiempo     int                  null,
   oi_tipo_documento    int                  null,
   l_reside_pais        smallint             null,
   l_acargo_af          smallint             null,
   o_acargo_af          varchar(1000)        null,
   l_acargo_os          smallint             null,
   o_acargo_os          varchar(1000)        null,
   l_acargo_ig          smallint             null,
   o_acargo_ig          varchar(1000)        null,
   f_desde_ig           datetime             null,
   f_hasta_ig           datetime             null,
   c_nro_seguro_soc     varchar(30)          null,
   d_resp_exp_doc       varchar(100)         null,
   e_sit_fam_presta     int                  null,
   c_nro_prestadora     varchar(30)          null,
   n_porc_prestadora    numeric(11,3)        null,
   o_familiar           varchar(1000)        null,
   -- ***   constraint PK_POS_FAMILIARES_CV primary key (oi_familiar_cv),
   -- ***   constraint AK_POS_FAMILIARES_POS unique (c_nro_documento)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_FLIARES_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_HRS_CV                                          */
/*==============================================================*/
create table dbo.POS01_HRS_CV (
   oi_horar_disp        int                  not null,
   oi_cv                int                  not null,
   c_dia                varchar(30)          not null,      
   f_desde              datetime             not null,
   f_hasta              datetime             null,
   -- ***   constraint PK_POS_AFJP_POS_HR primary key (oi_horar_disp),
   -- ***   constraint AK_POS_AFJP_POS_HR unique (oi_cv, c_dia, f_desde)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_HRS_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_IDIOMAS_CV                                      */
/*==============================================================*/
create table dbo.POS01_IDIOMAS_CV (
   oi_idioma_cv         int                  not null,
   oi_cv                int                  not null,
   oi_nivel_habla       int                  not null,
   oi_nivel_lee         int                  not null,
   oi_nivel_escribe     int                  not null,
   l_certificado        smallint             null,
   d_certificado        varchar(100)         null,
   e_tiempo_cursado     int                  null,
   oi_idioma            int                  null,
   oi_unidad_tiempo     int                  null,
   d_lugar_cursado      varchar(100)         null,
   o_idioma             varchar(1000)        null,
   -- ***   constraint PK_POS_IDIOMAS_CV primary key (oi_idioma_cv),
   -- ***   constraint AK_POS_IDIOMAS_POS unique (oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_IDIOMAS_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_IMAGENES_CV                                     */
/*==============================================================*/
create table dbo.POS01_IMAGENES_CV (
   oi_imagen_cv         int                  not null,
   oi_doc_digital       int                  not null,
   oi_cv                int                  not null,
   d_docum_dig_cv       varchar(100)         not null,
   o_docum_dig_cv       varchar(1000)        null,
   -- ***   constraint PK_POS_IMAGENES_POS primary key nonclustered (oi_imagen_cv),
   -- ***   constraint AK_POS_IMAGENES_POS unique (oi_doc_digital, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_IMAGENES_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_LIB_SANIT                                       */
/*==============================================================*/
create table dbo.POS01_LIB_SANIT (
   oi_lib_sanit         int                  not null,
   oi_cv                int                  not null,
   oi_provincia         int                  null,
   oi_localidad         int                  null,
   c_tipo_lib           varchar(30)          not null,      
   e_lib_sanit          int                  not null,
   f_vencimiento        datetime             not null,
   d_autoridad          varchar(100)         not null,
   d_control            varchar(1000)        null,
   o_lib_sanit          varchar(1000)        null,
   -- ***   constraint PK_POS01_LIB_SANIT primary key (oi_lib_sanit)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_LIB_SANIT',@@error , ''	 


/*==============================================================*/
/* Table: POS01_POSTU_CV                                        */
/*==============================================================*/
create table dbo.POS01_POSTU_CV (
   oi_postulaciones     int                  not null,
   oi_cv                int                  not null,
   oi_oferta_lab        int                  not null,
   f_postulacion        datetime             not null,
   -- ***   constraint PK_DOC_DIG_POS_PO primary key nonclustered (oi_postulaciones),
   -- ***   constraint AK_DOC_DIG_POS_PO unique (oi_cv, oi_oferta_lab)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_POSTU_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_PREF_CV                                         */
/*==============================================================*/
create table dbo.POS01_PREF_CV (
   oi_preferencia_cv    int                  not null,
   oi_preferencias      int                  not null,
   oi_cv                int                  not null,
   -- ***   constraint PK_DOC_DIG_POS_PR primary key nonclustered (oi_preferencia_cv),
   -- ***   constraint AK_DOC_DIG_POS_PR unique (oi_preferencias, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_PREF_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS01_REFERENCIAS                                     */
/*==============================================================*/
create table dbo.POS01_REFERENCIAS (
   oi_referencia        int                  not null,
   d_nombres            varchar(100)         not null,
   d_apellido           varchar(100)         not null,
   d_empresa            varchar(100)         null,
   te_cod_pais_part     numeric(4,0)         null,
   te_cod_area_part     numeric(4,0)         null,
   te_nro_part          numeric(15,0)        null,
   te_cod_pais_lab      numeric(4,0)         null,
   te_cod_area_lab      numeric(4,0)         null,
   te_nro_lab           numeric(15,0)        null,
   oi_tipo_remun        int                  null,
   n_remun              numeric(11,3)        null,
   oi_tipo_relacion     int                  null,
   oi_cv                int                  not null,
   t_referencia         varchar(4000)        null,
   f_confirmacion       datetime             null,
   -- ***   constraint PK_POS01_REFERENCIAS primary key (oi_referencia),
   -- ***   constraint AK_KEY_2_POS01_RE unique (d_nombres, d_apellido, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_REFERENCIAS',@@error , ''	 


/*==============================================================*/
/* Table: POS01_TEST_CV                                         */
/*==============================================================*/
create table dbo.POS01_TEST_CV (
   oi_test_cv           int                  not null,
   oi_res_test          int                  null,
   oi_test              int                  null,
   oi_cv                int                  null,
   f_test               datetime             not null,
   t_test               varchar(4000)        null,
   -- ***   constraint PK_POS_AFJP_POS_TE primary key (oi_test_cv),
   -- ***   constraint AK_POS_AFJP_POS_TE unique (f_test, oi_test, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS01_TEST_CV',@@error , ''	 


/*==============================================================*/
/* Table: POS02_BINARIOS                                        */
/*==============================================================*/
create table dbo.POS02_BINARIOS (
   oi_doc_digital       int                  not null,
   c_bloque             int                  not null,
   t_datos              varchar(4000)        null,
   -- ***   constraint AK_AK_BINARIOS_POS02_BI unique (oi_doc_digital, c_bloque)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS02_BINARIOS',@@error , ''	 


print ' aca estaba el error'
/*==============================================================*/
/* Table: POS02_DOC_DIG                                         */
/*==============================================================*/
create table dbo.POS02_DOC_DIG (
   oi_doc_digital       int                  not null,
   e_tamanio            int                  null,
   f_creacion           datetime             null,
   o_doc_digital        varchar(1000)        null,
   -- ***   constraint PK_Documento_Digital primary key nonclustered (oi_doc_digital)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS02_DOC_DIG',@@error , ''	 


/*==============================================================*/
/* Table: POS03_OFERTAS_LAB                                     */
/*==============================================================*/
create table dbo.POS03_OFERTAS_LAB (
   oi_oferta_lab        int                  not null,
   oi_seniority         int                  null,
   oi_area_lab          int                  null,
   oi_zona              int                  null,
   oi_localidad         int                  null,
   oi_pais              int                  null,
   oi_provincia         int                  null,
   c_oferta_lab         varchar(30)          not null,
   d_oferta_lab         varchar(100)         not null,
   f_oferta_lab         datetime             null,
   o_oferta_lab         varchar(1000)        null,
   f_cierre             datetime             null,
   c_sexo               varchar(1)           null,      
   l_profesional        smallint             not null,
   -- ***   constraint PK_OFERTAS_LAB primary key (oi_oferta_lab),
   -- ***   constraint AK_I_OFERTAS_LAB unique (c_oferta_lab)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS03_OFERTAS_LAB',@@error , ''	 


/*==============================================================*/
/* Table: POS04_SENIORITY                                       */
/*==============================================================*/
create table dbo.POS04_SENIORITY (
   oi_seniority         int                  not null,
   c_seniority          varchar(30)          not null,
   d_seniority          varchar(100)         not null,
   l_profesional        smallint             not null,
   -- ***   constraint PK_SENIORITY primary key (oi_seniority),
   -- ***   constraint AK_SENIORITY unique (c_seniority)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS04_SENIORITY',@@error , ''	 


/*==============================================================*/
/* Table: POS05_AREAS_LAB                                       */
/*==============================================================*/
create table dbo.POS05_AREAS_LAB (
   oi_area_lab          int                  not null,
   c_area_lab           varchar(30)          not null,
   d_area_lab           varchar(100)         not null,
   -- ***   constraint PK_POS05_AREAS_LAB primary key (oi_area_lab),
   -- ***   constraint AK_POS05_AREAS_LAB unique (c_area_lab)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS05_AREAS_LAB',@@error , ''	 


/*==============================================================*/
/* Table: POS06_TPO_ENTRE                                       */
/*==============================================================*/
create table dbo.POS06_TPO_ENTRE (
   oi_tipo_entrevistas  int                  not null,
   c_tipo_entrevistas   varchar(30)          not null,
   d_tipo_entrevistas   varchar(100)         not null,
   -- ***   constraint PK_POS06_TPO_ENTRE primary key (oi_tipo_entrevistas),
   -- ***   constraint AK_KEY_2_POS06_TP unique (c_tipo_entrevistas)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS06_TPO_ENTRE',@@error , ''	 


/*==============================================================*/
/* Table: POS07_RES_MEDICO                                      */
/*==============================================================*/
create table dbo.POS07_RES_MEDICO (
   oi_resultado_medico  int                  not null,
   c_resultado_medico   varchar(30)          not null,
   d_resultado_medico   varchar(100)         not null,
   -- ***   constraint PK_POS07_RES_MEDICO primary key (oi_resultado_medico),
   -- ***   constraint AK_KEY_2_POS07_RE unique (c_resultado_medico)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS07_RES_MEDICO',@@error , ''	 


/*==============================================================*/
/* Table: POS08_MOTIV_BUSQ                                      */
/*==============================================================*/
create table dbo.POS08_MOTIV_BUSQ (
   oi_motivacion_busqueda int                  not null,
   c_motivacion_busqueda varchar(30)          not null,
   d_motivacion_busqueda varchar(100)         not null,
   -- ***   constraint PK_POS08_MOTIV_BUSQ primary key (oi_motivacion_busqueda),
   -- ***   constraint AK_KEY_2_POS08_MO unique (c_motivacion_busqueda)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS08_MOTIV_BUSQ',@@error , ''	 


/*==============================================================*/
/* Table: POS09_RES_POLICIAL                                    */
/*==============================================================*/
create table dbo.POS09_RES_POLICIAL (
   oi_res_inf_policial  int                  not null,
   c_res_inf_policial   varchar(30)          not null,
   d_res_inf_policial   varchar(100)         not null,
   -- ***   constraint PK_POS09_RES_POLICIAL primary key (oi_res_inf_policial),
   -- ***   constraint AK_KEY_2_POS09_RE unique (c_res_inf_policial)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS09_RES_POLICIAL',@@error , ''	 


/*==============================================================*/
/* Table: POS10_TEST_AMB                                        */
/*==============================================================*/
create table dbo.POS10_TEST_AMB (
   oi_test_ambiental    int                  not null,
   c_test_ambiental     varchar(30)          not null,
   d_test_ambiental     varchar(100)         not null,
   -- ***   constraint PK_POS10_TEST_AMB primary key (oi_test_ambiental),
   -- ***   constraint AK_KEY_2_POS10_TE unique (c_test_ambiental)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS10_TEST_AMB',@@error , ''	 


/*==============================================================*/
/* Table: POS11_TESTS                                           */
/*==============================================================*/
create table dbo.POS11_TESTS (
   oi_test              int                  not null,
   c_test               varchar(30)          not null,
   d_test               varchar(100)         not null,
   -- ***   constraint PK_POS11_TESTS primary key (oi_test),
   -- ***   constraint AK_KEY_2_POS11_TE unique (c_test)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS11_TESTS',@@error , ''	 


/*==============================================================*/
/* Table: POS12_RES_TEST                                        */
/*==============================================================*/
create table dbo.POS12_RES_TEST (
   oi_res_test          int                  not null,
   c_res_test           varchar(30)          not null,
   d_res_test           varchar(100)         not null,
   -- ***   constraint PK_POS12_RES_TEST primary key (oi_res_test),
   -- ***   constraint AK_KEY_2_POS12_RE unique (c_res_test)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS12_RES_TEST',@@error , ''	 


/*==============================================================*/
/* Table: POS14_PREFERENCIAS                                    */
/*==============================================================*/
create table dbo.POS14_PREFERENCIAS (
   oi_preferencias      int                  not null,
   c_preferencias       varchar(30)          not null,
   d_preferencias       varchar(100)         not null,
   -- ***   constraint PK_POS14_PREFERENCIAS primary key (oi_preferencias),
   -- ***   constraint AK_KEY_2_POS14_PR unique (c_preferencias)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS14_PREFERENCIAS',@@error , ''	 


/*==============================================================*/
/* Table: POS15_CONT_WEB                                        */
/*==============================================================*/
create table dbo.POS15_CONT_WEB (
   oi_cont_web          int                  not null,
   c_cont_web           varchar(30)          not null,
   d_cont_web           varchar(100)         not null,
   o_cont_web           varchar(1000)        null,
   -- ***   constraint PK_POS15_CONT_WEB primary key (oi_cont_web),
   -- ***   constraint AK_AK_CONT_WEB_POS15_CO unique (c_cont_web)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS15_CONT_WEB',@@error , ''	 


/*==============================================================*/
/* Table: POS16_CONOCIMIENTOS                                   */
/*==============================================================*/
create table dbo.POS16_CONOCIMIENTOS (
   oi_conocimiento      int                  not null,
   c_conocimiento       varchar(30)          not null,
   d_conocimiento       varchar(100)         not null,
   l_administrativo     smallint             null,
   l_industrial         smallint             null,
   l_informatico        smallint             null,
   l_promotora          smallint             null,
   l_tecnico            smallint             null,
   l_merchandiser       smallint             null,
   o_conocimiento       varchar(1000)        null,
   -- ***   constraint PK_POS16_CONOCIMIENTOS primary key (oi_conocimiento),
   -- ***   constraint AK_AK_CONOCIMIENTOS_POS16_CO unique (c_conocimiento)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS16_CONOCIMIENTOS',@@error , ''	 


/*==============================================================*/
/* Table: POS17_BUSQUEDAS                                       */
/*==============================================================*/
create table dbo.POS17_BUSQUEDAS (
   oi_busqueda          int                  not null,
   c_busqueda           varchar(30)          not null,
   d_nombre_busq        varchar(100)         not null,
   f_fechora_busq       datetime             not null,
   d_usuario            varchar(100)         not null,
   c_tipo_cv            varchar(30)          null,      
   c_sexo               varchar(30)          null,      
   f_nac_desde          datetime             null,
   f_nac_hasta          datetime             null,
   c_postal_desde       varchar(30)          null,
   c_postal_hasta       varchar(30)          null,
   c_jubilado           varchar(30)          null,      
   f_cv_desde           datetime             null,
   f_cv_hasta           datetime             null,
   f_act_desde          datetime             null,
   f_act_hasta          datetime             null,
   c_ingresado_desde    varchar(30)          null,      
   c_largo_cabello      varchar(30)          null,      
   c_cadera_desde       varchar(30)          null,
   c_cadera_hasta       varchar(30)          null,
   c_cintura_desde      varchar(30)          null,
   c_cintura_hasta      varchar(30)          null,
   c_altura_desde       varchar(30)          null,
   c_altura_hasta       varchar(30)          null,
   c_peso_desde         varchar(30)          null,
   c_peso_hasta         varchar(30)          null,
   c_busto_desde        varchar(30)          null,
   c_busto_hasta        varchar(30)          null,
   c_lic_conducir       varchar(30)          null,      
   n_min_remun          numeric(11,3)        null,
   n_ideal_remun        numeric(11,3)        null,
   n_actual_remun       numeric(11,3)        null,
   f_disp_desde         datetime             null,
   f_disp_hasta         datetime             null,
   e_cant_hijos         int                  null,
   oi_color_ojos_1      int                  null,
   oi_color_ojos_2      int                  null,
   oi_color_ojos_3      int                  null,
   oi_color_pelo_1      int                  null,
   oi_color_pelo_2      int                  null,
   oi_color_pelo_3      int                  null,
   oi_tipo_movil_1      int                  null,
   oi_tipo_movil_2      int                  null,
   oi_tipo_licencia_1   int                  null,
   oi_tipo_licencia_2   int                  null,
   oi_tipo_remun_1      int                  null,
   oi_tipo_remun_2      int                  null,
   oi_tipo_relacion_1   int                  null,
   oi_tipo_relacion_2   int                  null,
   oi_conocimiento_1    int                  null,
   oi_conocimiento_2    int                  null,
   oi_conocimiento_3    int                  null,
   oi_conocimiento_4    int                  null,
   oi_nacionalidad_1    int                  null,
   oi_nacionalidad_2    int                  null,
   oi_nacionalidad_3    int                  null,
   oi_curso_1           int                  null,
   oi_curso_2           int                  null,
   oi_curso_3           int                  null,
   oi_curso_4           int                  null,
   oi_localidad_1       int                  null,
   oi_localidad_2       int                  null,
   oi_pais_1            int                  null,
   oi_pais_2            int                  null,
   oi_provincia_2       int                  null,
   oi_provincia_1       int                  null,
   oi_idioma_1          int                  null,
   oi_idioma_2          int                  null,
   oi_idioma_3          int                  null,
   oi_idioma_4          int                  null,
   oi_competencia_2     int                  null,
   oi_competencia_1     int                  null,
   oi_estado_civil_2    int                  null,
   oi_estado_civil_1    int                  null,
   oi_titulo            int                  null,
   oi_estado_jubi       int                  null,
   oi_calificacion      int                  null,
   -- ***   constraint PK_POS17_BUSQUEDAS primary key (oi_busqueda),
   -- ***   constraint AK_AK_KEY_2_POS17_BU_POS17_BU unique (c_busqueda, d_nombre_busq)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS17_BUSQUEDAS',@@error , ''	 


/*==============================================================*/
/* Table: POS17_RESUL_BUSQ                                      */
/*==============================================================*/
create table dbo.POS17_RESUL_BUSQ (
   oi_resul_busq        int                  not null,
   oi_busqueda          int                  not null,
   oi_cv                int                  not null,
   l_marcado            smallint             not null,
   -- ***   constraint PK_POS17_RESUL_BUSQ primary key (oi_resul_busq),
   -- ***   constraint AK_AK_KEY_2_POS17_RE_POS17_RE unique (oi_busqueda, oi_cv)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS17_RESUL_BUSQ',@@error , ''	 


/*==============================================================*/
/* Table: POS18_COLOR_OJOS                                      */
/*==============================================================*/
create table dbo.POS18_COLOR_OJOS (
   oi_color_ojos        int                  not null,
   c_color_ojos         varchar(30)          not null,
   d_color_ojos         varchar(100)         not null,
   -- ***   constraint PK_POS18_COLOR_OJOS primary key (oi_color_ojos),
   -- ***   constraint AK_KEY_2_POS18_CO unique (c_color_ojos)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS18_COLOR_OJOS',@@error , ''	 


/*==============================================================*/
/* Table: POS19_COLOR_PELO                                      */
/*==============================================================*/
create table dbo.POS19_COLOR_PELO (
   oi_color_pelo        int                  not null,
   c_color_pelo         varchar(30)          not null,
   d_color_pelo         varchar(100)         not null,
   -- ***   constraint PK_POS19_COLOR_PELO primary key (oi_color_pelo),
   -- ***   constraint AK_KEY_2_POS19_CO unique (c_color_pelo)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS19_COLOR_PELO',@@error , ''	 


/*==============================================================*/
/* Table: POS20_TIPO_CUTIS                                      */
/*==============================================================*/
create table dbo.POS20_TIPO_CUTIS (
   oi_tipo_cutis        int                  not null,
   c_tipo_cutis         varchar(30)          not null,
   d_tipo_cutis         varchar(100)         not null,
   -- ***   constraint PK_POS20_TIPO_CUTIS primary key (oi_tipo_cutis),
   -- ***   constraint AK_KEY_2_POS20_TI unique (c_tipo_cutis)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS20_TIPO_CUTIS',@@error , ''	 


/*==============================================================*/
/* Table: POS21_TIPO_MOVI                                       */
/*==============================================================*/
create table dbo.POS21_TIPO_MOVI (
   oi_tipo_movi         int                  not null,
   c_tipo_movi          varchar(30)          not null,
   d_tipo_movi          varchar(100)         not null,
   -- ***   constraint PK_POS21_TIPO_MOVI primary key (oi_tipo_movi),
   -- ***   constraint AK_KEY_2_POS21_TI unique (c_tipo_movi)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS21_TIPO_MOVI',@@error , ''	 


/*==============================================================*/
/* Table: POS22_TIPO_LICENC                                     */
/*==============================================================*/
create table dbo.POS22_TIPO_LICENC (
   oi_tipo_licencia     int                  not null,
   c_tipo_licencia      varchar(30)          not null,
   d_tipo_licencia      varchar(100)         not null,
   -- ***   constraint PK_POS22_TIPO_LICENC primary key (oi_tipo_licencia),
   -- ***   constraint AK_KEY_2_POS22_TI unique (c_tipo_licencia)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS22_TIPO_LICENC',@@error , ''	 


/*==============================================================*/
/* Table: POS23_TIPO_REMUN                                      */
/*==============================================================*/
create table dbo.POS23_TIPO_REMUN (
   oi_tipo_remun        int                  not null,
   c_tipo_remun         varchar(30)          not null,
   d_tipo_remun         varchar(100)         not null,
   -- ***   constraint PK_POS23_TIPO_REMUN primary key (oi_tipo_remun),
   -- ***   constraint AK_KEY_2_POS23_TI unique (c_tipo_remun)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS23_TIPO_REMUN',@@error , ''	 


/*==============================================================*/
/* Table: POS24_TIPO_RELA                                       */
/*==============================================================*/
create table dbo.POS24_TIPO_RELA (
   oi_tipo_relacion     int                  not null,
   c_tipo_relacion      varchar(30)          not null,
   d_tipo_relacion      varchar(100)         not null,
   -- ***   constraint PK_POS24_TIPO_RELA primary key (oi_tipo_relacion),
   -- ***   constraint AK_KEY_2_POS24_TI unique (c_tipo_relacion)
)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Create Table POS24_TIPO_RELA',@@error , ''	 
/***************************************************************************************/

insert into POS03_OFERTAS_LAB   select * from COL03_OFERTAS_LAB  
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into  POS03_OFERTAS_LAB',@@error , ''	 
insert into POS04_SENIORITY     select * from COL04_SENIORITY    
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS04_SENIORITY ',@@error , ''	 
insert into POS05_AREAS_LAB     select * from COL05_AREAS_LAB    
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS05_AREAS_LAB ',@@error , ''	 
insert into POS06_TPO_ENTRE     select * from COL06_TPO_ENTRE    
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS06_TPO_ENTRE ',@@error , ''	 
insert into POS07_RES_MEDICO    select * from COL07_RES_MEDICO   
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS07_RES_MEDICO ',@@error , ''	 
insert into POS08_MOTIV_BUSQ    select * from COL08_MOTIV_BUSQ   
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into  POS08_MOTIV_BUSQ',@@error , ''	 
insert into POS09_RES_POLICIAL  select * from COL09_RES_POLICIAL 
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS09_RES_POLICIAL ',@@error , ''	 
insert into POS10_TEST_AMB      select * from COL10_TEST_AMB     
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS10_TEST_AMB ',@@error , ''	 
insert into POS11_TESTS         select * from COL11_TESTS        
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS11_TESTS ',@@error , ''	 
insert into POS12_RES_TEST      select * from COL12_RES_TEST     
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS12_RES_TEST ',@@error , ''	 
insert into POS14_PREFERENCIAS  select * from COL14_PREFERENCIAS 
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into  POS14_PREFERENCIAS',@@error , ''	 
insert into POS15_CONT_WEB      select * from COL15_CONT_WEB     
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS15_CONT_WEB ',@@error , ''	 
insert into POS16_CONOCIMIENTOS select * from COL16_CONOCIMIENTOS
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'insert into POS16_CONOCIMIENTOS ',@@error , ''	 

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Fin de INSERTS',@@error , ''	 
/***************************************************************************************/
/*==============================================================*/
/* ELIMINO LAS TABLAS DE COL                                    */
/*==============================================================*/

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'ELIMINO LAS TABLAS DE COL',@@error , ''	 
drop table COL01_AFJP_CV        
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_AFJP_CV'      ,@@error ,''
drop table COL01_ANTEC_CV       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_ANTEC_CV'     ,@@error ,''
drop table COL01_COMPET_CV      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_COMPET_CV'    ,@@error ,''
drop table COL01_CONOC_CV       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_CONOC_CV'     ,@@error ,''
drop table COL01_CURSOS_CV      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_CURSOS_CV'    ,@@error ,''
drop table COL01_DOC_DIG_CV     
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_DOC_DIG_CV'   ,@@error ,''
drop table COL01_DOCUM_CV       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_DOCUM_CV'     ,@@error ,''
drop table COL01_DOMIC_COL      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_DOMIC_COL'    ,@@error ,''
drop table COL01_ENTREVISTAS    
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_ENTREVISTAS'  ,@@error ,''
drop table COL01_ESTUDIOS_CV    
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_ESTUDIOS_CV'  ,@@error ,''
drop table COL01_FLIARES_CV     
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_FLIARES_CV'   ,@@error ,''
drop table COL01_HRS_CV         
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_HRS_CV'       ,@@error ,''
drop table COL01_IDIOMAS_CV     
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_IDIOMAS_CV'   ,@@error ,''
drop table COL01_IMAGENES_CV    
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_IMAGENES_CV'  ,@@error ,''
drop table COL01_LIB_SANIT      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_LIB_SANIT'    ,@@error ,''
drop table COL01_POSTU_CV       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_POSTU_CV'     ,@@error ,''
drop table COL01_PREF_CV        
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_PREF_CV'      ,@@error ,''
drop table COL01_REFERENCIAS    
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_REFERENCIAS'  ,@@error ,''
drop table COL01_TEST_CV        
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_TEST_CV'      ,@@error ,''
drop table COL02_BINARIOS       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL02_BINARIOS'     ,@@error ,''
drop table COL03_OFERTAS_LAB    
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL03_OFERTAS_LAB'  ,@@error ,''
drop table COL04_SENIORITY      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL04_SENIORITY'    ,@@error ,''
drop table COL05_AREAS_LAB      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL05_AREAS_LAB'    ,@@error ,''
drop table COL06_TPO_ENTRE      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL06_TPO_ENTRE'    ,@@error ,''

drop table COL11_TESTS          
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL11_TESTS'        ,@@error ,''
drop table COL12_RES_TEST       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL12_RES_TEST'     ,@@error ,''
drop table COL14_PREFERENCIAS   
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL14_PREFERENCIAS' ,@@error ,'' 
drop table COL17_RESUL_BUSQ     
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL17_RESUL_BUSQ'    ,@@error ,''
drop table COL01_CV             
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL01_CV'           ,@@error ,''
drop table COL02_DOC_DIG        
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL02_DOC_DIG'      ,@@error ,''
drop table COL07_RES_MEDICO     
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL07_RES_MEDICO'   ,@@error ,''
drop table COL08_MOTIV_BUSQ     
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL08_MOTIV_BUSQ'   ,@@error ,''
drop table COL09_RES_POLICIAL   
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL09_RES_POLICIAL' ,@@error ,'' 
drop table COL10_TEST_AMB       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL10_TEST_AMB'     ,@@error ,''
drop table COL15_CONT_WEB       
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL15_CONT_WEB'     ,@@error ,''
drop table COL17_BUSQUEDAS      
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL17_BUSQUEDAS'    ,@@error ,''

drop table COL16_CONOCIMIENTOS  
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes','drop table COL16_CONOCIMIENTOS',@@error ,''  

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'fin ELIMINO LAS TABLAS DE COL',@@error , ''	 
/******************************* Comienzan los CONSTRAINTS MAnuales*****************************/
/****************************************************************************************/

 Select @cant = Count(*) from SYSOBJECTS nolock where name in ('FK_AFJP_POS_CV'
,'FK_POS01_AF_REFERENCE_PER03_AF','FK_ANTECEDENTE_POS_CV ','FK_ANTECEDENTES_POS__MOT_EG_PER '
,'FK_POS01_AN_REFERENCE_ORG25_UN','FK_COMPET_POS_CV','FK_CV__COMPETENCIAS '
,'FK_POS01_CO_REFERENCE_POS16_CO','FK_POS01_CO_REFERENCE_POS01_CV','FK_CURSOS_POS__CURSOS '
,'FK_CURSOS_POS__UNIDADES_TPO ','FK_CURSOS_POS_CV','FK_POS01_CV_REFERENCE_POS23_TI'
,'FK_POS01_CV_REFERENCE_POS15_CO','FK_POS01_CV_REFERENCE_POS24_TI','FK_POS01_CV_REFERENCE_ORG19_LON '
,'FK_POS01_CV_REFERENCE_ORG19_LOR ','FK_POS01_CV_REFERENCE_ORG19_PAN ','FK_POS01_CV_REFERENCE_ORG19_PAR '
,'FK_POS01_CV_REFERENCE_ORG19_PRN ','FK_POS01_CV_REFERENCE_ORG19_PRR ','FK_POS01_CV_REFERENCE_CAL '
,'FK_POS01_CV_REFERENCE_EJ','FK_POS01_CV_REFERENCE_EC','FK_POS01_CV_REFERENCE_ID'
,'FK_POS01_CV_REFERENCE_NAC ','FK_POS01_CV_REFERENCE_GS','FK_POS01_CV_REFERENCE_TD'
,'FK_POS01_CV_REFERENCE_AF','FK_POS01_CV_REFERENCE_PER04_TI','FK_POS01_CV_REFERENCE_POS02_DO2 '
,'FK_POS01_CV_REFERENCE_POS02_DO1 ','FK_POS01_CV_REFERENCE_POS18_CO','FK_POS01_CV_REFERENCE_POS19_CO'
,'FK_POS01_CV_REFERENCE_POS07_RE','FK_POS01_CV_REFERENCE_POS10_TE','FK_POS01_CV_REFERENCE_POS08_MO'
,'FK_POS01_CV_REFERENCE_POS09_RE','FK_POS01_CV_REFERENCE_POS21_TI','FK_POS01_CV_REFERENCE_POS22_TI'
,'FK_DOCUM_POS_CV ','FK_POS01_DO_REFERENCE_ORG19_LOC ','FK_POS01_DO_REFERENCE_PER20_TI'
,'FK_DOC_DIG_POS_DOC_DIG','FK_POS01_DO_REFERENCE_POS01_CV','FK_POS01_DO_FK_PER01__TD'
,'FK_DOMIC_POS_CV ','FK_POS01_DO_REFERENCE_ORG19_LO','FK_POS01_EN_REFERENCE_ORG33_SU'
,'FK_POS01_EN_REFERENCE_POS20_TI','FK_POS01_EN_REFERENCE_POS06_TP','FK_POS01_EN_REFERENCE_POS01_CV'
,'FK_ORG01_ES_FK_POS01__EST ','FK_POS01_ES_FK_POS01__UT','FK_POS01_ES_FK_PER12__NE'
,'FK_POS01_ES_FK_PER13__EE','FK_POS01_ES_FK_POS01__TEE ','FK_ESTUDIOS_POS_CV'
,'FK_FLIARES_POS__ESTUDIOS','FK_FLIARES_POS__GRADOS_ESCOL','FK_FLIARES_POS__NIVELES_ESC '
,'FK_FLIARES_POS__OCUP_FAM','FK_FLIARES_POS__TIPOS_FAM ','FK_FLIARES_POS__UNIDAD_TPO'
,'FK_FLIARES_POS_CV ','FK_POS01_FL_REFERENCE_NAC ','FK_POS01_FL_REFERENCE_EC'
,'FK_POS01_FL_REFERENCE_TD','FK_POS01_HR_REFERENCE_POS01_CV','FK_FLIARES_POS__NIV_IDIOMA'
,'FK_FLIARES_POS__NIV_IDIOMA1 ','FK_FLIARES_POS__NIV_IDIOMA11','FK_IDIOMA_POS_CV'
,'FK_IDIOMAS_POS__UNIDADES_TPO','FK_POS01_ID_REFERENCE_ORG20_ID','FK_CV_IMAGENES'
,'FK_IMAGENES_POS_DOC_DIG ','FK_POS01_LI_REFERENCE_POS01_CV','FK_POS01_LI_REFERENCE_ORG19_PR'
,'FK_POS01_LI_REFERENCE_ORG19_LO','FK_POS01_PO_REFERENCE_POS01_CV','FK_POS01_PO_REFERENCE_POS03_OF'
,'FK_POS01_PR_REFERENCE_POS14_PR','FK_POS01_PR_REFERENCE_POS01_CV','FK_POS01_RE_REFERENCE_POS24_TI'
,'FK_POS01_RE_REFERENCE_POS01_CV','FK_POS01_RE_REFERENCE_POS23_TI','FK_POS01_TE_REFERENCE_POS12_RE'
,'FK_POS01_TE_REFERENCE_POS11_TE','FK_POS01_TE_REFERENCE_POS01_CV','FK_POS_BINARIOS__DOC_DIG'
,'FK_POS03_OF_REFERENCE_ORG19_ZO','FK_POS03_OF_REFERENCE_ORG19_PR','FK_POS03_OF_REFERENCE_ORG19_PA'
,'FK_POS03_OF_REFERENCE_ORG19_LO','FK_POS03_OF_REFERENCE_POS04_SE','FK_POS03_OF_REFERENCE_POS05_AR'
,'FK_POS17_BU_REFERENCE_PER04_TI','FK_POS17_BU_REFERENCE_PER27_CA1 ','FK_POS17_BU_REFERENCE_PER14_ES2 '
,'FK_POS17_BU_REFERENCE_ORG22_ES1 ','FK_POS17_BU_REFERENCE_ORG22_ES2 ','FK_POS17_BU_REFERENCE_ORG20_ID1 '
,'FK_POS17_BU_REFERENCE_ORG20_ID3 ','FK_POS17_BU_REFERENCE_ORG20_ID4 ','FK_POS17_BU_REFERENCE_ORG20_ID2 '
,'FK_POS17_BU_REFERENCE_ORG12_NA1 ','FK_POS17_BU_REFERENCE_ORG12_NA2 ','FK_POS17_BU_REFERENCE_ORG12_NA3 '
,'FK_POS17_BU_REFERENCE_PER01C_C1 ','FK_POS17_BU_REFERENCE_PER01C_C2 ','FK_POS17_BU_REFERENCE_PER01C_C3 '
,'FK_POS17_BU_REFERENCE_PER02C_C1 ','FK_POS17_BU_REFERENCE_PER02C_C2 ','FK_POS17_BU_REFERENCE_PER02C_C3 '
,'FK_POS17_BU_REFERENCE_POS21_TI','FK_POS17_BU_REFERENCE_PER04C_T2 ','FK_POS17_BU_REFERENCE_PER05C_T1 '
,'FK_POS17_BU_REFERENCE_PER05C_T2 ','FK_POS17_BU_REFERENCE_PER06C_T1 ','FK_POS17_BU_REFERENCE_PER06C_T2 '
,'FK_POS17_BU_REFERENCE_PER07C_T2 ','FK_POS17_BU_REFERENCE_PER07C_T1 ','FK_POS17_BU_REFERENCE_ORG06_CU1 '
,'FK_POS17_BU_REFERENCE_ORG06_CU2 ','FK_POS17_BU_REFERENCE_ORG06_CU3 ','FK_POS17_BU_REFERENCE_ORG06_CU4 '
,'FK_POS17_BU_REFERENCE_POS16_CONOC2','FK_POS17_BU_REFERENCE_POS16_CONOC4','FK_POS17_BU_REFERENCE_POS16_CONOC1'
,'FK_POS17_BU_REFERENCE_POS16_CONOC3','FK_POS17_BU_REFERENCE_ORG21_COM1','FK_POS17_BU_REFERENCE_ORG21_COM2'
,'FK_POS17_BU_REFERENCE_ORG19_LO1 ','FK_POS17_BU_REFERENCE_ORG19_LO2 ','FK_POS17_BU_REFERENCE_ORG19_PA1 '
,'FK_POS17_BU_REFERENCE_ORG19_PA2 ','FK_POS17_BU_REFERENCE_ORG19_PR1 ','FK_POS17_BU_REFERENCE_ORG19_PR2 '
,'FK_POS17_RE_REFERENCE_POS17_BU','FK_POS17_RE_REFERENCE_POS01_CV')

if @cant > 0 
BEGIN                                                                                            
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Ya existe un Const de Igual nombre',@@error , ''	 
	Select 'ERROR Ya existe un Const de Igual nombre'
 Select 'Constraint duplicado : ' + name  from SYSOBJECTS nolock where name in ('FK_AFJP_POS_CV'
,'FK_POS01_AF_REFERENCE_PER03_AF','FK_ANTECEDENTE_POS_CV ','FK_ANTECEDENTES_POS__MOT_EG_PER '
,'FK_POS01_AN_REFERENCE_ORG25_UN','FK_COMPET_POS_CV','FK_CV__COMPETENCIAS '
,'FK_POS01_CO_REFERENCE_POS16_CO','FK_POS01_CO_REFERENCE_POS01_CV','FK_CURSOS_POS__CURSOS '
,'FK_CURSOS_POS__UNIDADES_TPO ','FK_CURSOS_POS_CV','FK_POS01_CV_REFERENCE_POS23_TI'
,'FK_POS01_CV_REFERENCE_POS15_CO','FK_POS01_CV_REFERENCE_POS24_TI','FK_POS01_CV_REFERENCE_ORG19_LON '
,'FK_POS01_CV_REFERENCE_ORG19_LOR ','FK_POS01_CV_REFERENCE_ORG19_PAN ','FK_POS01_CV_REFERENCE_ORG19_PAR '
,'FK_POS01_CV_REFERENCE_ORG19_PRN ','FK_POS01_CV_REFERENCE_ORG19_PRR ','FK_POS01_CV_REFERENCE_CAL '
,'FK_POS01_CV_REFERENCE_EJ','FK_POS01_CV_REFERENCE_EC','FK_POS01_CV_REFERENCE_ID'
,'FK_POS01_CV_REFERENCE_NAC ','FK_POS01_CV_REFERENCE_GS','FK_POS01_CV_REFERENCE_TD'
,'FK_POS01_CV_REFERENCE_AF','FK_POS01_CV_REFERENCE_PER04_TI','FK_POS01_CV_REFERENCE_POS02_DO2 '
,'FK_POS01_CV_REFERENCE_POS02_DO1 ','FK_POS01_CV_REFERENCE_POS18_CO','FK_POS01_CV_REFERENCE_POS19_CO'
,'FK_POS01_CV_REFERENCE_POS07_RE','FK_POS01_CV_REFERENCE_POS10_TE','FK_POS01_CV_REFERENCE_POS08_MO'
,'FK_POS01_CV_REFERENCE_POS09_RE','FK_POS01_CV_REFERENCE_POS21_TI','FK_POS01_CV_REFERENCE_POS22_TI'
,'FK_DOCUM_POS_CV ','FK_POS01_DO_REFERENCE_ORG19_LOC ','FK_POS01_DO_REFERENCE_PER20_TI'
,'FK_DOC_DIG_POS_DOC_DIG','FK_POS01_DO_REFERENCE_POS01_CV','FK_POS01_DO_FK_PER01__TD'
,'FK_DOMIC_POS_CV ','FK_POS01_DO_REFERENCE_ORG19_LO','FK_POS01_EN_REFERENCE_ORG33_SU'
,'FK_POS01_EN_REFERENCE_POS20_TI','FK_POS01_EN_REFERENCE_POS06_TP','FK_POS01_EN_REFERENCE_POS01_CV'
,'FK_ORG01_ES_FK_POS01__EST ','FK_POS01_ES_FK_POS01__UT','FK_POS01_ES_FK_PER12__NE'
,'FK_POS01_ES_FK_PER13__EE','FK_POS01_ES_FK_POS01__TEE ','FK_ESTUDIOS_POS_CV'
,'FK_FLIARES_POS__ESTUDIOS','FK_FLIARES_POS__GRADOS_ESCOL','FK_FLIARES_POS__NIVELES_ESC '
,'FK_FLIARES_POS__OCUP_FAM','FK_FLIARES_POS__TIPOS_FAM ','FK_FLIARES_POS__UNIDAD_TPO'
,'FK_FLIARES_POS_CV ','FK_POS01_FL_REFERENCE_NAC ','FK_POS01_FL_REFERENCE_EC'
,'FK_POS01_FL_REFERENCE_TD','FK_POS01_HR_REFERENCE_POS01_CV','FK_FLIARES_POS__NIV_IDIOMA'
,'FK_FLIARES_POS__NIV_IDIOMA1 ','FK_FLIARES_POS__NIV_IDIOMA11','FK_IDIOMA_POS_CV'
,'FK_IDIOMAS_POS__UNIDADES_TPO','FK_POS01_ID_REFERENCE_ORG20_ID','FK_CV_IMAGENES'
,'FK_IMAGENES_POS_DOC_DIG ','FK_POS01_LI_REFERENCE_POS01_CV','FK_POS01_LI_REFERENCE_ORG19_PR'
,'FK_POS01_LI_REFERENCE_ORG19_LO','FK_POS01_PO_REFERENCE_POS01_CV','FK_POS01_PO_REFERENCE_POS03_OF'
,'FK_POS01_PR_REFERENCE_POS14_PR','FK_POS01_PR_REFERENCE_POS01_CV','FK_POS01_RE_REFERENCE_POS24_TI'
,'FK_POS01_RE_REFERENCE_POS01_CV','FK_POS01_RE_REFERENCE_POS23_TI','FK_POS01_TE_REFERENCE_POS12_RE'
,'FK_POS01_TE_REFERENCE_POS11_TE','FK_POS01_TE_REFERENCE_POS01_CV','FK_POS_BINARIOS__DOC_DIG'
,'FK_POS03_OF_REFERENCE_ORG19_ZO','FK_POS03_OF_REFERENCE_ORG19_PR','FK_POS03_OF_REFERENCE_ORG19_PA'
,'FK_POS03_OF_REFERENCE_ORG19_LO','FK_POS03_OF_REFERENCE_POS04_SE','FK_POS03_OF_REFERENCE_POS05_AR'
,'FK_POS17_BU_REFERENCE_PER04_TI','FK_POS17_BU_REFERENCE_PER27_CA1 ','FK_POS17_BU_REFERENCE_PER14_ES2 '
,'FK_POS17_BU_REFERENCE_ORG22_ES1 ','FK_POS17_BU_REFERENCE_ORG22_ES2 ','FK_POS17_BU_REFERENCE_ORG20_ID1 '
,'FK_POS17_BU_REFERENCE_ORG20_ID3 ','FK_POS17_BU_REFERENCE_ORG20_ID4 ','FK_POS17_BU_REFERENCE_ORG20_ID2 '
,'FK_POS17_BU_REFERENCE_ORG12_NA1 ','FK_POS17_BU_REFERENCE_ORG12_NA2 ','FK_POS17_BU_REFERENCE_ORG12_NA3 '
,'FK_POS17_BU_REFERENCE_PER01C_C1 ','FK_POS17_BU_REFERENCE_PER01C_C2 ','FK_POS17_BU_REFERENCE_PER01C_C3 '
,'FK_POS17_BU_REFERENCE_PER02C_C1 ','FK_POS17_BU_REFERENCE_PER02C_C2 ','FK_POS17_BU_REFERENCE_PER02C_C3 '
,'FK_POS17_BU_REFERENCE_POS21_TI','FK_POS17_BU_REFERENCE_PER04C_T2 ','FK_POS17_BU_REFERENCE_PER05C_T1 '
,'FK_POS17_BU_REFERENCE_PER05C_T2 ','FK_POS17_BU_REFERENCE_PER06C_T1 ','FK_POS17_BU_REFERENCE_PER06C_T2 '
,'FK_POS17_BU_REFERENCE_PER07C_T2 ','FK_POS17_BU_REFERENCE_PER07C_T1 ','FK_POS17_BU_REFERENCE_ORG06_CU1 '
,'FK_POS17_BU_REFERENCE_ORG06_CU2 ','FK_POS17_BU_REFERENCE_ORG06_CU3 ','FK_POS17_BU_REFERENCE_ORG06_CU4 '
,'FK_POS17_BU_REFERENCE_POS16_CONOC2','FK_POS17_BU_REFERENCE_POS16_CONOC4','FK_POS17_BU_REFERENCE_POS16_CONOC1'
,'FK_POS17_BU_REFERENCE_POS16_CONOC3','FK_POS17_BU_REFERENCE_ORG21_COM1','FK_POS17_BU_REFERENCE_ORG21_COM2'
,'FK_POS17_BU_REFERENCE_ORG19_LO1 ','FK_POS17_BU_REFERENCE_ORG19_LO2 ','FK_POS17_BU_REFERENCE_ORG19_PA1 '
,'FK_POS17_BU_REFERENCE_ORG19_PA2 ','FK_POS17_BU_REFERENCE_ORG19_PR1 ','FK_POS17_BU_REFERENCE_ORG19_PR2 '
,'FK_POS17_RE_REFERENCE_POS17_BU','FK_POS17_RE_REFERENCE_POS01_CV')

	return

end



/****************************************************************************************/
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Init Constraints Manuales',@@error , ''	 

Alter  table dbo.POS01_AFJP_CV       add constraint PK_POS_AFJP_CV primary key (oi_afjp_cv  )                            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_AFJP_CV'                , @@error ,''
 Alter  Table DBO.POS01_AFJP_CV       add constraint AK_POS_AFJP_POS unique (f_ingreso, oi_cv)                           
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_AFJP_POS'               , @@error ,''
 Alter  table dbo.POS01_ANTEC_CV       add constraint PK_POS_ANTECEDENTE primary key (oi_antecedente_cv  )                
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_ANTECEDENTE'            , @@error ,''
 Alter  Table DBO.POS01_ANTEC_CV      add constraint AK_POS_ANTECEDENTE unique (oi_cv, f_ingreso)                         
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_ANTECEDENTE'            , @@error ,''
 Alter  table dbo.POS01_COMPET_CV        add constraint PK_COMPETENCIA_POS primary key nonclustered (oi_competencia_cv )  
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_COMPETENCIA_POS'            , @@error ,''
 Alter  Table DBO.POS01_COMPET_CV      add constraint AK_COMPETENCIA_POS unique (oi_cv)                                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_COMPETENCIA_POS'            , @@error ,''
 Alter  table dbo.POS01_CONOC_CV       add constraint PK_POS01_CONOC_CV primary key (oi_conoc_cv )                        
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_CONOC_CV'             , @@error ,''
 Alter  Table DBO.POS01_CONOC_CV        add constraint AK_AK_CONOC_CV_POS01_CO unique (oi_conocimiento, oi_cv)            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_CONOC_CV_POS01_CO'       , @@error ,''
 Alter  table dbo.POS01_CURSOS_CV      add constraint PK_POS01_CURSOS_CV primary key (oi_curso_ext_cv)                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_CURSOS_CV'            , @@error ,''
 Alter  table dbo.POS01_CV       			add constraint PK_POS01_CV primary key (oi_cv )                                     
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_CV'                   , @@error ,''
 Alter  Table DBO.POS01_CV      			add constraint AK_AK_KEY_2_POS01_CV_POS01_CV unique (c_nro_doc, l_alta_web)         
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_KEY_2_POS01_CV_POS01_CV' , @@error ,''
 Alter  table dbo.POS01_DOCUM_CV       add constraint PK_POS01_DOCUM_POS primary key (oi_documento_cv )                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_DOCUM_POS'            , @@error ,''
 Alter  Table DBO.POS01_DOCUM_CV      add constraint AK_POS01_DOCUM_POS unique (oi_cv)                                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS01_DOCUM_POS'            , @@error ,''
 Alter  table dbo.POS01_DOC_DIG_CV       add constraint PK_DOC_DIG_POS primary key nonclustered (oi_docum_dig_cv )        
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_DOC_DIG_POS'                , @@error ,''
 Alter  Table DBO.POS01_DOC_DIG_CV       add constraint AK_DOC_DIG_POS unique (oi_cv, oi_doc_digital)                     
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_DOC_DIG_POS'                , @@error ,''
 Alter  table dbo.POS01_DOMIC_CV       add constraint PK_POS01_DOMIC_POS primary key (oi_domic_cv )                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_DOMIC_POS'            , @@error ,''
 Alter  Table DBO.POS01_DOMIC_CV       add constraint AK_POS01_DOMIC_POS unique (oi_cv)                                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS01_DOMIC_POS'            , @@error ,''
 Alter  table dbo.POS01_ENTREVISTAS       add constraint PK_POS01_ENTREVISTAS primary key (oi_entrevistas )               
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_ENTREVISTAS'          , @@error ,''
 Alter  Table DBO.POS01_ENTREVISTAS       add constraint AK_KEY_2_POS01_EN unique (oi_cv, f_entrevista)                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS01_EN'             , @@error ,''
 Alter  table dbo.POS01_ESTUDIOS_CV       add constraint PK_POS01_ESTUDIOS_POS primary key (oi_estudio_cv )               
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_ESTUDIOS_POS'         , @@error ,''
 Alter  Table DBO.POS01_ESTUDIOS_CV       add constraint AK_POS01_ESTUDIOS_POS unique (oi_cv)                             
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS01_ESTUDIOS_POS'         , @@error ,''
 Alter  table dbo.POS01_FLIARES_CV       add constraint PK_POS_FAMILIARES_CV primary key (oi_familiar_cv )                
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_FAMILIARES_CV'          , @@error ,''
 Alter  Table DBO.POS01_FLIARES_CV       add constraint AK_POS_FAMILIARES_POS unique (c_nro_documento)                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_FAMILIARES_POS'         , @@error ,''
 Alter  table dbo.POS01_HRS_CV       add constraint PK_POS_AFJP_POS_HR primary key (oi_horar_disp )                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_AFJP_POS_HR'            , @@error ,''
 Alter  Table DBO.POS01_HRS_CV       add constraint AK_POS_AFJP_POS_HR unique (oi_cv, c_dia, f_desde)                     
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_AFJP_POS_HR'            , @@error ,''
 Alter  table dbo.POS01_IDIOMAS_CV       add constraint PK_POS_IDIOMAS_CV primary key (oi_idioma_cv )                     
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_IDIOMAS_CV'             , @@error ,''
 Alter  Table DBO.POS01_IDIOMAS_CV       add constraint AK_POS_IDIOMAS_POS unique (oi_cv)                                 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_IDIOMAS_POS'            , @@error ,''
 Alter  table dbo.POS01_IMAGENES_CV       add constraint PK_POS_IMAGENES_POS primary key nonclustered (oi_imagen_cv )     
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_IMAGENES_POS'           , @@error ,''
 Alter  Table DBO.POS01_IMAGENES_CV       add constraint AK_POS_IMAGENES_POS unique (oi_doc_digital, oi_cv)               
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_IMAGENES_POS'           , @@error ,''
 Alter  table dbo.POS01_LIB_SANIT       add constraint PK_POS01_LIB_SANIT primary key (oi_lib_sanit)                      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_LIB_SANIT'            , @@error ,''
 Alter  table dbo.POS01_POSTU_CV       add constraint PK_DOC_DIG_POS_PO primary key nonclustered (oi_postulaciones )      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_DOC_DIG_POS_PO'             , @@error ,''
 Alter  Table DBO.POS01_POSTU_CV      add constraint AK_DOC_DIG_POS_PO unique (oi_cv, oi_oferta_lab)                      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_DOC_DIG_POS_PO'             , @@error ,''
 Alter  table dbo.POS01_PREF_CV       add constraint PK_DOC_DIG_POS_PR primary key nonclustered (oi_preferencia_cv )      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_DOC_DIG_POS_PR'             , @@error ,''
 Alter  Table DBO.POS01_PREF_CV       add constraint AK_DOC_DIG_POS_PR unique (oi_preferencias, oi_cv)                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_DOC_DIG_POS_PR'             , @@error ,''
 Alter  table dbo.POS01_REFERENCIAS       add constraint PK_POS01_REFERENCIAS primary key (oi_referencia )                
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS01_REFERENCIAS'          , @@error ,''
 Alter  Table DBO.POS01_REFERENCIAS       add constraint AK_KEY_2_POS01_RE unique (d_nombres, d_apellido, oi_cv)          
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS01_RE'             , @@error ,''
 Alter  table dbo.POS01_TEST_CV       add constraint PK_POS_AFJP_POS_TE primary key (oi_test_cv )                         
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS_AFJP_POS_TE'            , @@error ,''
 Alter  Table DBO.POS01_TEST_CV       add constraint AK_POS_AFJP_POS_TE unique (f_test, oi_test, oi_cv)                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS_AFJP_POS_TE'            , @@error ,''
 Alter  table dbo.POS02_BINARIOS      add constraint AK_AK_BINARIOS_POS02_BI unique (oi_doc_digital, c_bloque)            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_BINARIOS_POS02_BI'       , @@error ,''
 Alter  table dbo.POS02_DOC_DIG       add constraint PK_Documento_Digital primary key nonclustered (oi_doc_digital)       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_Documento_Digital'          , @@error ,''
 Alter  table dbo.POS03_OFERTAS_LAB       add constraint PK_OFERTAS_LAB primary key (oi_oferta_lab )                      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_OFERTAS_LAB'                , @@error ,''
 Alter  Table DBO.POS03_OFERTAS_LAB       add constraint AK_I_OFERTAS_LAB unique (c_oferta_lab)                           
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_I_OFERTAS_LAB'              , @@error ,''
 Alter  table dbo.POS04_SENIORITY      add constraint PK_SENIORITY primary key (oi_seniority )                            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_SENIORITY'                  , @@error ,''
 Alter  Table DBO.POS04_SENIORITY      add constraint AK_SENIORITY unique (c_seniority)                                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_SENIORITY'                  , @@error ,''
 Alter  table dbo.POS05_AREAS_LAB      add constraint PK_POS05_AREAS_LAB primary key (oi_area_lab )                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS05_AREAS_LAB'            , @@error ,''
 Alter  Table DBO.POS05_AREAS_LAB      add constraint AK_POS05_AREAS_LAB unique (c_area_lab)                              
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_POS05_AREAS_LAB'            , @@error ,''
 Alter  table dbo.POS06_TPO_ENTRE       add constraint PK_POS06_TPO_ENTRE primary key (oi_tipo_entrevistas  )             
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS06_TPO_ENTRE'            , @@error ,''
 Alter  Table DBO.POS06_TPO_ENTRE      add constraint AK_KEY_2_POS06_TP unique (c_tipo_entrevistas)                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS06_TP'             , @@error ,''
 Alter  table dbo.POS07_RES_MEDICO      add constraint PK_POS07_RES_MEDICO primary key (oi_resultado_medico )             
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS07_RES_MEDICO'           , @@error ,''
 Alter  Table DBO.POS07_RES_MEDICO      add constraint AK_KEY_2_POS07_RE unique (c_resultado_medico)                      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS07_RE'             , @@error ,''
 Alter  table dbo.POS08_MOTIV_BUSQ      add constraint PK_POS08_MOTIV_BUSQ primary key (oi_motivacion_busqueda  )         
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS08_MOTIV_BUSQ'           , @@error ,''
 Alter  Table DBO.POS08_MOTIV_BUSQ      add constraint AK_KEY_2_POS08_MO unique (c_motivacion_busqueda)                   
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS08_MO'             , @@error ,''
 Alter  table dbo.POS09_RES_POLICIAL       add constraint PK_POS09_RES_POLICIAL primary key (oi_res_inf_policial )        
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS09_RES_POLICIAL'         , @@error ,''
 Alter  Table DBO.POS09_RES_POLICIAL      add constraint AK_KEY_2_POS09_RE unique (c_res_inf_policial)                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS09_RE'             , @@error ,''
 Alter  table dbo.POS10_TEST_AMB      add constraint PK_POS10_TEST_AMB primary key (oi_test_ambiental  )                  
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS10_TEST_AMB'             , @@error ,''
 Alter  Table DBO.POS10_TEST_AMB      add constraint AK_KEY_2_POS10_TE unique (c_test_ambiental)                          
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS10_TE'             , @@error ,''
 Alter  table dbo.POS11_TESTS      		add constraint PK_POS11_TESTS primary key (oi_test  )                               
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS11_TESTS'                , @@error ,''
 Alter  Table DBO.POS11_TESTS      		add constraint AK_KEY_2_POS11_TE unique (c_test)                                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS11_TE'             , @@error ,''
 Alter  table dbo.POS12_RES_TEST       add constraint PK_POS12_RES_TEST primary key (oi_res_test  )                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS12_RES_TEST'             , @@error ,''
 Alter  Table DBO.POS12_RES_TEST      	add constraint AK_KEY_2_POS12_RE unique (c_res_test)                              
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS12_RE'             , @@error ,''
 Alter  table dbo.POS14_PREFERENCIAS       add constraint PK_POS14_PREFERENCIAS primary key (oi_preferencias  )           
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS14_PREFERENCIAS'         , @@error ,''
 Alter  Table DBO.POS14_PREFERENCIAS       add constraint AK_KEY_2_POS14_PR unique (c_preferencias)                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS14_PR'             , @@error ,''
 Alter  table dbo.POS15_CONT_WEB      add constraint PK_POS15_CONT_WEB primary key (oi_cont_web  )                        
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS15_CONT_WEB'             , @@error ,''
 Alter  Table DBO.POS15_CONT_WEB      add constraint AK_AK_CONT_WEB_POS15_CO unique (c_cont_web)                          
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_CONT_WEB_POS15_CO'       , @@error ,''
 Alter  table dbo.POS16_CONOCIMIENTOS      add constraint PK_POS16_CONOCIMIENTOS primary key (oi_conocimiento  )          
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS16_CONOCIMIENTOS'        , @@error ,''
 Alter  Table DBO.POS16_CONOCIMIENTOS      add constraint AK_AK_CONOCIMIENTOS_POS16_CO unique (c_conocimiento)            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_CONOCIMIENTOS_POS16_CO'  , @@error ,''
 Alter  table dbo.POS17_BUSQUEDAS      add constraint PK_POS17_BUSQUEDAS primary key (oi_busqueda  )                      
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS17_BUSQUEDAS'            , @@error ,''
 Alter  Table DBO.POS17_BUSQUEDAS      add constraint AK_AK_KEY_2_POS17_BU_POS17_BU unique (c_busqueda, d_nombre_busq)    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_KEY_2_POS17_BU_POS17_BU' , @@error ,''
 Alter  table dbo.POS17_RESUL_BUSQ       add constraint PK_POS17_RESUL_BUSQ primary key (oi_resul_busq  )                 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS17_RESUL_BUSQ'           , @@error ,''
 Alter  Table DBO.POS17_RESUL_BUSQ       add constraint AK_AK_KEY_2_POS17_RE_POS17_RE unique (oi_busqueda, oi_cv)         
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_AK_KEY_2_POS17_RE_POS17_RE' , @@error ,''
 Alter  table dbo.POS18_COLOR_OJOS       add constraint PK_POS18_COLOR_OJOS primary key (oi_color_ojos )                  
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS18_COLOR_OJOS'           , @@error ,''
 Alter  Table DBO.POS18_COLOR_OJOS      add constraint AK_KEY_2_POS18_CO unique (c_color_ojos)                            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS18_CO'             , @@error ,''
 Alter  table dbo.POS19_COLOR_PELO       add constraint PK_POS19_COLOR_PELO primary key (oi_color_pelo )                  
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS19_COLOR_PELO'           , @@error ,''
 Alter  Table DBO.POS19_COLOR_PELO       add constraint AK_KEY_2_POS19_CO unique (c_color_pelo)                           
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS19_CO'             , @@error ,''
 Alter  table dbo.POS20_TIPO_CUTIS      add constraint PK_POS20_TIPO_CUTIS primary key (oi_tipo_cutis  )                  
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS20_TIPO_CUTIS'           , @@error ,''
 Alter  Table DBO.POS20_TIPO_CUTIS      add constraint AK_KEY_2_POS20_TI unique (c_tipo_cutis)                            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS20_TI'             , @@error ,''
 Alter  table dbo.POS21_TIPO_MOVI       add constraint PK_POS21_TIPO_MOVI primary key (oi_tipo_movi  )                    
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS21_TIPO_MOVI'            , @@error ,''
 Alter  Table DBO.POS21_TIPO_MOVI       add constraint AK_KEY_2_POS21_TI unique (c_tipo_movi)                             
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS21_TI'             , @@error ,''
 Alter  table dbo.POS22_TIPO_LICENC       add constraint PK_POS22_TIPO_LICENC primary key (oi_tipo_licencia  )            
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS22_TIPO_LICENC'          , @@error ,''
 Alter  Table DBO.POS22_TIPO_LICENC       add constraint AK_KEY_2_POS22_TI unique (c_tipo_licencia)                       
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS22_TI'             , @@error ,''
 Alter  table dbo.POS23_TIPO_REMUN       add constraint PK_POS23_TIPO_REMUN primary key (oi_tipo_remun  )                 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS23_TIPO_REMUN'           , @@error ,''
 Alter  Table DBO.POS23_TIPO_REMUN       add constraint AK_KEY_2_POS23_TI unique (c_tipo_remun)                           
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS23_TI'             , @@error ,''
 Alter  table dbo.POS24_TIPO_RELA        add constraint PK_POS24_TIPO_RELA primary key (oi_tipo_relacion  )               
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'PK_POS24_TIPO_RELA'            , @@error ,''
 Alter  Table DBO.POS24_TIPO_RELA       add constraint AK_KEY_2_POS24_TI unique (c_tipo_relacion)                         
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'AK_KEY_2_POS24_TI'             , @@error ,''

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Fin Constraints Manuales',@@error , ''	 

/******************************* Comienzan los CONSTRAINTS Automaticos*****************************/
/****************************************************************************************/

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Init Constraints Auto',@@error , ''	 

alter table dbo.POS01_AFJP_CV
   add    constraint FK_AFJP_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_AFJP_POS_CV ',@@error , ''	 


alter table dbo.POS01_AFJP_CV
   add    constraint FK_POS01_AF_REFERENCE_PER03_AF foreign key (oi_afjp)
      references dbo.PER03_AFJP (oi_afjp)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_AF_REFERENCE_PER03_AF ',@@error , ''	 


alter table dbo.POS01_ANTEC_CV
   add    constraint FK_ANTECEDENTE_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_ANTECEDENTE_POS_CV ',@@error , ''	 


alter table dbo.POS01_ANTEC_CV
   add    constraint FK_ANTECEDENTES_POS__MOT_EG_PER foreign key (oi_motivo_eg_per)
      references dbo.PER22_MOT_EG_PER (oi_motivo_eg_per)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_ANTECEDENTES_POS__MOT_EG_PER ',@@error , ''	 


alter table dbo.POS01_ANTEC_CV
   add    constraint FK_POS01_AN_REFERENCE_ORG25_UN foreign key (oi_unidad_tiempo)
      references dbo.ORG25_UNIDADES_TPO (oi_unidad_tiempo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_AN_REFERENCE_ORG25_UN',@@error , ''	 


alter table dbo.POS01_COMPET_CV
   add    constraint FK_COMPET_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_COMPET_POS_CV ',@@error , ''	 


alter table dbo.POS01_COMPET_CV
   add    constraint FK_CV__COMPETENCIAS foreign key (oi_competencia)
      references dbo.ORG21_COMPETENCIAS (oi_competencia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_CV__COMPETENCIAS ',@@error , ''	 


alter table dbo.POS01_CONOC_CV
   add    constraint FK_POS01_CO_REFERENCE_POS16_CO foreign key (oi_conocimiento)
      references dbo.POS16_CONOCIMIENTOS (oi_conocimiento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CO_REFERENCE_POS16_CO ',@@error , ''	 


alter table dbo.POS01_CONOC_CV
   add    constraint FK_POS01_CO_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CO_REFERENCE_POS01_CV ',@@error , ''	 


alter table dbo.POS01_CURSOS_CV
   add    constraint FK_CURSOS_POS__CURSOS foreign key (oi_curso)
      references dbo.ORG06_CURSOS (oi_curso)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_CURSOS_POS__CURSOS',@@error , ''	 


alter table dbo.POS01_CURSOS_CV
   add    constraint FK_CURSOS_POS__UNIDADES_TPO foreign key (oi_unidad_tiempo)
      references dbo.ORG25_UNIDADES_TPO (oi_unidad_tiempo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_CURSOS_POS__UNIDADES_TPO',@@error , ''	 


alter table dbo.POS01_CURSOS_CV
   add    constraint FK_CURSOS_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_CURSOS_POS_CV',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS23_TI foreign key (oi_tipo_remun)
      references dbo.POS23_TIPO_REMUN (oi_tipo_remun)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS23_TI ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS15_CO foreign key (oi_cont_web)
      references dbo.POS15_CONT_WEB (oi_cont_web)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_CV_REFERENCE_POS15_CO',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS24_TI foreign key (oi_tipo_relacion)
      references dbo.POS24_TIPO_RELA (oi_tipo_relacion)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS24_TI ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ORG19_LON foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ORG19_LON ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ORG19_LOR foreign key (oi_localidad_nac)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ORG19_LOR ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ORG19_PAN foreign key (oi_pais_nac)
      references dbo.ORG19_PAISES (oi_pais)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ORG19_PAN ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ORG19_PAR foreign key (oi_pais)
      references dbo.ORG19_PAISES (oi_pais)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ORG19_PAR ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ORG19_PRN foreign key (oi_provincia_nac)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ORG19_PRN ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ORG19_PRR foreign key (oi_provincia)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ORG19_PRR ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_CAL foreign key (oi_calificacion)
      references dbo.PER27_CALIFICACIONES (oi_calificacion)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_CAL ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_EJ foreign key (oi_estado_jubi)
      references dbo.PER14_ESTADOS_JUBI (oi_estado_jubi)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_CV_REFERENCE_EJ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_EC foreign key (oi_estado_civil)
      references dbo.ORG22_EST_CIVIL (oi_estado_civil)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_EC ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_ID foreign key (oi_idioma)
      references dbo.ORG20_IDIOMAS (oi_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_ID ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_NAC foreign key (oi_nacionalidad)
      references dbo.ORG12_NACIONALID (oi_nacionalidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_NAC ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_GS foreign key (oi_grupo_sanguineo)
      references dbo.PER10_GRUPOS_SANG (oi_grupo_sanguineo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_GS ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_TD foreign key (oi_tipo_documento)
      references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_TD ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_AF foreign key (oi_afjp)
      references dbo.PER03_AFJP (oi_afjp)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_CV_REFERENCE_AF',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_PER04_TI foreign key (oi_titulo)
      references dbo.PER04_TITULOS (oi_titulo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_PER04_TI ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS02_DO2 foreign key (oi_foto)
      references dbo.POS02_DOC_DIG (oi_doc_digital)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_CV_REFERENCE_POS02_DO2',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS02_DO1 foreign key (oi_firma)
      references dbo.POS02_DOC_DIG (oi_doc_digital)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS02_DO1 ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS18_CO foreign key (oi_color_ojos)
      references dbo.POS18_COLOR_OJOS (oi_color_ojos)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS18_CO ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS19_CO foreign key (oi_color_pelo)
      references dbo.POS19_COLOR_PELO (oi_color_pelo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS19_CO ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS07_RE foreign key (oi_resultado_medico)
      references dbo.POS07_RES_MEDICO (oi_resultado_medico)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_CV_REFERENCE_POS07_RE',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS10_TE foreign key (oi_test_ambiental)
      references dbo.POS10_TEST_AMB (oi_test_ambiental)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS10_TE ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS08_MO foreign key (oi_motivacion_busqueda)
      references dbo.POS08_MOTIV_BUSQ (oi_motivacion_busqueda)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS08_MO ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS09_RE foreign key (oi_res_inf_policial)
      references dbo.POS09_RES_POLICIAL (oi_res_inf_policial)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS09_RE ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS21_TI foreign key (oi_tipo_movil)
      references dbo.POS21_TIPO_MOVI (oi_tipo_movi)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS09_RE ',@@error , ''	 


alter table dbo.POS01_CV
   add    constraint FK_POS01_CV_REFERENCE_POS22_TI foreign key (oi_tipo_licencia)
      references dbo.POS22_TIPO_LICENC (oi_tipo_licencia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_CV_REFERENCE_POS22_TI ',@@error , ''	 


alter table dbo.POS01_DOCUM_CV
   add    constraint FK_DOCUM_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_DOCUM_POS_CV',@@error , ''	 


alter table dbo.POS01_DOCUM_CV
   add    constraint FK_POS01_DO_REFERENCE_ORG19_LOC foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_DO_REFERENCE_ORG19_LOC ',@@error , ''	 


alter table dbo.POS01_DOCUM_CV
   add    constraint FK_POS01_DO_REFERENCE_PER20_TI foreign key (oi_tipo_documento)
      references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_DO_REFERENCE_PER20_TI ',@@error , ''	 


alter table dbo.POS01_DOC_DIG_CV
   add    constraint FK_DOC_DIG_POS_DOC_DIG foreign key (oi_doc_digital)
      references dbo.POS02_DOC_DIG (oi_doc_digital)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_DOC_DIG_POS_DOC_DIG ',@@error , ''	 


alter table dbo.POS01_DOC_DIG_CV
   add    constraint FK_POS01_DO_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_DO_REFERENCE_POS01_CV',@@error , ''	 


alter table dbo.POS01_DOMIC_CV
   add    constraint FK_POS01_DO_FK_PER01__TD foreign key (oi_tipo_domicilio)
      references dbo.PER09_TIPOS_DOMIC (oi_tipo_domicilio)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_DO_FK_PER01__TD ',@@error , ''	 


alter table dbo.POS01_DOMIC_CV
   add    constraint FK_DOMIC_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_DOMIC_POS_CV ',@@error , ''	 


alter table dbo.POS01_DOMIC_CV
   add    constraint FK_POS01_DO_REFERENCE_ORG19_LO foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_DO_REFERENCE_ORG19_LO ',@@error , ''	 


alter table dbo.POS01_ENTREVISTAS
   add    constraint FK_POS01_EN_REFERENCE_ORG33_SU foreign key (oi_sucursal)
      references dbo.ORG33_SUCURSALES (oi_sucursal)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_EN_REFERENCE_ORG33_SU ',@@error , ''	 


alter table dbo.POS01_ENTREVISTAS
   add    constraint FK_POS01_EN_REFERENCE_POS20_TI foreign key (oi_tipo_cutis)
      references dbo.POS20_TIPO_CUTIS (oi_tipo_cutis)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_EN_REFERENCE_POS20_TI',@@error , ''	 


alter table dbo.POS01_ENTREVISTAS
   add    constraint FK_POS01_EN_REFERENCE_POS06_TP foreign key (oi_tipo_entrevistas)
      references dbo.POS06_TPO_ENTRE (oi_tipo_entrevistas)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_EN_REFERENCE_POS06_TP ',@@error , ''	 


alter table dbo.POS01_ENTREVISTAS
   add    constraint FK_POS01_EN_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_EN_REFERENCE_POS01_CV ',@@error , ''	 


alter table dbo.POS01_ESTUDIOS_CV
   add    constraint FK_ORG01_ES_FK_POS01__EST foreign key (oi_estudio)
      references dbo.ORG11_ESTUDIOS (oi_estudio)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_ORG01_ES_FK_POS01__EST ',@@error , ''	 


alter table dbo.POS01_ESTUDIOS_CV
   add    constraint FK_POS01_ES_FK_POS01__UT foreign key (oi_unidad_tiempo)
      references dbo.ORG25_UNIDADES_TPO (oi_unidad_tiempo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_ES_FK_POS01__UT ',@@error , ''	 


alter table dbo.POS01_ESTUDIOS_CV
   add    constraint FK_POS01_ES_FK_PER12__NE foreign key (oi_nivel_estudio)
      references dbo.PER12_NIVELES_EST (oi_nivel_estudio)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_ES_FK_PER12__NE ',@@error , ''	 


alter table dbo.POS01_ESTUDIOS_CV
   add    constraint FK_POS01_ES_FK_PER13__EE foreign key (oi_estado_est)
      references dbo.PER13_ESTADOS_EST (oi_estado_est)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_ES_FK_PER13__EE ',@@error , ''	 


alter table dbo.POS01_ESTUDIOS_CV
   add    constraint FK_POS01_ES_FK_POS01__TEE foreign key (oi_tipo_establecim)
      references dbo.PER23_T_EST_EDUC (oi_tipo_establecim)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_ES_FK_POS01__TEE ',@@error , ''	 


alter table dbo.POS01_ESTUDIOS_CV
   add    constraint FK_ESTUDIOS_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_ESTUDIOS_POS_CV ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS__ESTUDIOS foreign key (oi_estudio)
      references dbo.ORG11_ESTUDIOS (oi_estudio)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS__ESTUDIOS ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS__GRADOS_ESCOL foreign key (oi_grado_escol)
      references dbo.PER24_GRADOS_ESCOL (oi_grado_escol)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS__GRADOS_ESCOL ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS__NIVELES_ESC foreign key (oi_nivel_escol)
      references dbo.PER08_NIVELES_ESC (oi_nivel_escol)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_FLIARES_POS__NIVELES_ESC',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS__OCUP_FAM foreign key (oi_ocupacion_fam)
      references dbo.PER18_OCUP_FAM (oi_ocupacion_fam)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS__OCUP_FAM ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS__TIPOS_FAM foreign key (oi_tipo_familiar)
      references dbo.PER15_TIPOS_FAM (oi_tipo_familiar)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_FLIARES_POS__TIPOS_FAM',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS__UNIDAD_TPO foreign key (oi_unidad_tiempo)
      references dbo.ORG25_UNIDADES_TPO (oi_unidad_tiempo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS__UNIDAD_TPO ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_FLIARES_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS_CV ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_POS01_FL_REFERENCE_NAC foreign key (oi_nacionalidad)
      references dbo.ORG12_NACIONALID (oi_nacionalidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_FL_REFERENCE_NAC ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_POS01_FL_REFERENCE_EC foreign key (oi_estado_civil)
      references dbo.ORG22_EST_CIVIL (oi_estado_civil)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_FL_REFERENCE_EC ',@@error , ''	 


alter table dbo.POS01_FLIARES_CV
   add    constraint FK_POS01_FL_REFERENCE_TD foreign key (oi_tipo_documento)
      references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_FL_REFERENCE_TD ',@@error , ''	 


alter table dbo.POS01_HRS_CV
   add    constraint FK_POS01_HR_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_HR_REFERENCE_POS01_CV',@@error , ''	 


alter table dbo.POS01_IDIOMAS_CV
   add    constraint FK_FLIARES_POS__NIV_IDIOMA foreign key (oi_nivel_escribe)
      references dbo.ORG24_NIV_IDIOMA (oi_nivel_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS__NIV_IDIOMA ',@@error , ''	 


alter table dbo.POS01_IDIOMAS_CV
   add    constraint FK_FLIARES_POS__NIV_IDIOMA1 foreign key (oi_nivel_habla)
      references dbo.ORG24_NIV_IDIOMA (oi_nivel_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_FLIARES_POS__NIV_IDIOMA1',@@error , ''	 


alter table dbo.POS01_IDIOMAS_CV
   add    constraint FK_FLIARES_POS__NIV_IDIOMA11 foreign key (oi_nivel_lee)
      references dbo.ORG24_NIV_IDIOMA (oi_nivel_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_FLIARES_POS__NIV_IDIOMA11 ',@@error , ''	 


alter table dbo.POS01_IDIOMAS_CV
   add    constraint FK_IDIOMA_POS_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_IDIOMA_POS_CV',@@error , ''	 


alter table dbo.POS01_IDIOMAS_CV
   add    constraint FK_IDIOMAS_POS__UNIDADES_TPO foreign key (oi_unidad_tiempo)
      references dbo.ORG25_UNIDADES_TPO (oi_unidad_tiempo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_IDIOMAS_POS__UNIDADES_TPO',@@error , ''	 


alter table dbo.POS01_IDIOMAS_CV
   add    constraint FK_POS01_ID_REFERENCE_ORG20_ID foreign key (oi_idioma)
      references dbo.ORG20_IDIOMAS (oi_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_ID_REFERENCE_ORG20_ID ',@@error , ''	 


alter table dbo.POS01_IMAGENES_CV
   add    constraint FK_CV_IMAGENES foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_CV_IMAGENES ',@@error , ''	 


alter table dbo.POS01_IMAGENES_CV
   add    constraint FK_IMAGENES_POS_DOC_DIG foreign key (oi_doc_digital)
      references dbo.POS02_DOC_DIG (oi_doc_digital)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_IMAGENES_POS_DOC_DIG ',@@error , ''	 


alter table dbo.POS01_LIB_SANIT
   add    constraint FK_POS01_LI_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_LI_REFERENCE_POS01_CV ',@@error , ''	 


alter table dbo.POS01_LIB_SANIT
   add    constraint FK_POS01_LI_REFERENCE_ORG19_PR foreign key (oi_provincia)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_LI_REFERENCE_ORG19_PR',@@error , ''	 


alter table dbo.POS01_LIB_SANIT
   add    constraint FK_POS01_LI_REFERENCE_ORG19_LO foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_LI_REFERENCE_ORG19_LO ',@@error , ''	 


alter table dbo.POS01_POSTU_CV
   add    constraint FK_POS01_PO_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_PO_REFERENCE_POS01_CV ',@@error , ''	 


alter table dbo.POS01_POSTU_CV
   add    constraint FK_POS01_PO_REFERENCE_POS03_OF foreign key (oi_oferta_lab)
      references dbo.POS03_OFERTAS_LAB (oi_oferta_lab)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_PO_REFERENCE_POS03_OF ',@@error , ''	 


alter table dbo.POS01_PREF_CV
   add    constraint FK_POS01_PR_REFERENCE_POS14_PR foreign key (oi_preferencias)
      references dbo.POS14_PREFERENCIAS (oi_preferencias)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_PR_REFERENCE_POS14_PR ',@@error , ''	 


alter table dbo.POS01_PREF_CV
   add    constraint FK_POS01_PR_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_PR_REFERENCE_POS01_CV',@@error , ''	 


alter table dbo.POS01_REFERENCIAS
   add    constraint FK_POS01_RE_REFERENCE_POS24_TI foreign key (oi_tipo_relacion)
      references dbo.POS24_TIPO_RELA (oi_tipo_relacion)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_RE_REFERENCE_POS24_TI ',@@error , ''	 


alter table dbo.POS01_REFERENCIAS
   add    constraint FK_POS01_RE_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_RE_REFERENCE_POS01_CV ',@@error , ''	 


alter table dbo.POS01_REFERENCIAS
   add    constraint FK_POS01_RE_REFERENCE_POS23_TI foreign key (oi_tipo_remun)
      references dbo.POS23_TIPO_REMUN (oi_tipo_remun)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_RE_REFERENCE_POS23_TI ',@@error , ''	 


alter table dbo.POS01_TEST_CV
   add    constraint FK_POS01_TE_REFERENCE_POS12_RE foreign key (oi_res_test)
      references dbo.POS12_RES_TEST (oi_res_test)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS01_TE_REFERENCE_POS12_RE',@@error , ''	 


alter table dbo.POS01_TEST_CV
   add    constraint FK_POS01_TE_REFERENCE_POS11_TE foreign key (oi_test)
      references dbo.POS11_TESTS (oi_test)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_TE_REFERENCE_POS11_TE ',@@error , ''	 


alter table dbo.POS01_TEST_CV
   add    constraint FK_POS01_TE_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS01_TE_REFERENCE_POS01_CV ',@@error , ''	 


alter table dbo.POS02_BINARIOS
   add    constraint FK_POS_BINARIOS__DOC_DIG foreign key (oi_doc_digital)
      references dbo.POS02_DOC_DIG (oi_doc_digital)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS_BINARIOS__DOC_DIG',@@error , ''	 


delete POS03_OFERTAS_LAB where oi_zona is not null
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'delete POS03_OFERTAS_LAB ',@@error , ''	 

alter table dbo.POS03_OFERTAS_LAB
   add    constraint FK_POS03_OF_REFERENCE_ORG19_ZO foreign key (oi_zona)
      references dbo.ORG19_ZONAS (oi_zona)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS03_OF_REFERENCE_ORG19_ZO ',@@error , ''	 


alter table dbo.POS03_OFERTAS_LAB
   add    constraint FK_POS03_OF_REFERENCE_ORG19_PR foreign key (oi_provincia)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS03_OF_REFERENCE_ORG19_PR',@@error , ''	 


alter table dbo.POS03_OFERTAS_LAB
   add    constraint FK_POS03_OF_REFERENCE_ORG19_PA foreign key (oi_pais)
      references dbo.ORG19_PAISES (oi_pais)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS03_OF_REFERENCE_ORG19_PA',@@error , ''	 


alter table dbo.POS03_OFERTAS_LAB
   add    constraint FK_POS03_OF_REFERENCE_ORG19_LO foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS03_OF_REFERENCE_ORG19_LO ',@@error , ''	 


alter table dbo.POS03_OFERTAS_LAB
   add    constraint FK_POS03_OF_REFERENCE_POS04_SE foreign key (oi_seniority)
      references dbo.POS04_SENIORITY (oi_seniority)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS03_OF_REFERENCE_POS04_SE ',@@error , ''	 


alter table dbo.POS03_OFERTAS_LAB
   add    constraint FK_POS03_OF_REFERENCE_POS05_AR foreign key (oi_area_lab)
      references dbo.POS05_AREAS_LAB (oi_area_lab)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS03_OF_REFERENCE_POS05_AR',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER04_TI foreign key (oi_titulo)
      references dbo.PER04_TITULOS (oi_titulo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER04_TI ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER27_CA1 foreign key (oi_calificacion)
      references dbo.PER27_CALIFICACIONES (oi_calificacion)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER27_CA1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER14_ES2 foreign key (oi_estado_jubi)
      references dbo.PER14_ESTADOS_JUBI (oi_estado_jubi)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_PER14_ES2',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG22_ES1 foreign key (oi_estado_civil_1)
      references dbo.ORG22_EST_CIVIL (oi_estado_civil)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG22_ES1',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG22_ES2 foreign key (oi_estado_civil_2)
      references dbo.ORG22_EST_CIVIL (oi_estado_civil)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG22_ES2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG20_ID1 foreign key (oi_idioma_3)
      references dbo.ORG20_IDIOMAS (oi_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG20_ID1',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG20_ID3 foreign key (oi_idioma_1)
      references dbo.ORG20_IDIOMAS (oi_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG20_ID3',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG20_ID4 foreign key (oi_idioma_4)
      references dbo.ORG20_IDIOMAS (oi_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG20_ID4 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG20_ID2 foreign key (oi_idioma_2)
      references dbo.ORG20_IDIOMAS (oi_idioma)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG20_ID2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG12_NA1 foreign key (oi_nacionalidad_1)
      references dbo.ORG12_NACIONALID (oi_nacionalidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG12_NA1',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG12_NA2 foreign key (oi_nacionalidad_3)
      references dbo.ORG12_NACIONALID (oi_nacionalidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG12_NA2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG12_NA3 foreign key (oi_nacionalidad_2)
      references dbo.ORG12_NACIONALID (oi_nacionalidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG12_NA3 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER01C_C1 foreign key (oi_color_ojos_1)
      references dbo.POS18_COLOR_OJOS (oi_color_ojos)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER01C_C1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER01C_C2 foreign key (oi_color_ojos_2)
      references dbo.POS18_COLOR_OJOS (oi_color_ojos)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER01C_C2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER01C_C3 foreign key (oi_color_ojos_3)
      references dbo.POS18_COLOR_OJOS (oi_color_ojos)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER01C_C3 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER02C_C1 foreign key (oi_color_pelo_1)
      references dbo.POS19_COLOR_PELO (oi_color_pelo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER02C_C1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER02C_C2 foreign key (oi_color_pelo_2)
      references dbo.POS19_COLOR_PELO (oi_color_pelo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_PER02C_C2',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER02C_C3 foreign key (oi_color_pelo_3)
      references dbo.POS19_COLOR_PELO (oi_color_pelo)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER02C_C3 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_POS21_TI foreign key (oi_tipo_movil_1)
      references dbo.POS21_TIPO_MOVI (oi_tipo_movi)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_POS21_TI',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER04C_T2 foreign key (oi_tipo_movil_2)
      references dbo.POS21_TIPO_MOVI (oi_tipo_movi)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_PER04C_T2',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER05C_T1 foreign key (oi_tipo_licencia_1)
      references dbo.POS22_TIPO_LICENC (oi_tipo_licencia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_PER05C_T1',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER05C_T2 foreign key (oi_tipo_licencia_2)
      references dbo.POS22_TIPO_LICENC (oi_tipo_licencia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER05C_T2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER06C_T1 foreign key (oi_tipo_remun_1)
      references dbo.POS23_TIPO_REMUN (oi_tipo_remun)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_PER06C_T1',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER06C_T2 foreign key (oi_tipo_remun_2)
      references dbo.POS23_TIPO_REMUN (oi_tipo_remun)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_PER06C_T2',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER07C_T2 foreign key (oi_tipo_relacion_1)
      references dbo.POS24_TIPO_RELA (oi_tipo_relacion)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER07C_T2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_PER07C_T1 foreign key (oi_tipo_relacion_2)
      references dbo.POS24_TIPO_RELA (oi_tipo_relacion)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_PER07C_T1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG06_CU1 foreign key (oi_curso_1)
      references dbo.ORG06_CURSOS (oi_curso)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG06_CU1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG06_CU2 foreign key (oi_curso_2)
      references dbo.ORG06_CURSOS (oi_curso)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG06_CU2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG06_CU3 foreign key (oi_curso_3)
      references dbo.ORG06_CURSOS (oi_curso)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG06_CU3 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG06_CU4 foreign key (oi_curso_4)
      references dbo.ORG06_CURSOS (oi_curso)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG06_CU4',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_POS16_CONOC2 foreign key (oi_conocimiento_3)
      references dbo.POS16_CONOCIMIENTOS (oi_conocimiento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_POS16_CONOC2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_POS16_CONOC4 foreign key (oi_conocimiento_1)
      references dbo.POS16_CONOCIMIENTOS (oi_conocimiento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_POS16_CONOC4 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_POS16_CONOC1 foreign key (oi_conocimiento_4)
      references dbo.POS16_CONOCIMIENTOS (oi_conocimiento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_POS16_CONOC1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_POS16_CONOC3 foreign key (oi_conocimiento_2)
      references dbo.POS16_CONOCIMIENTOS (oi_conocimiento)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_POS16_CONOC3 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG21_COM1 foreign key (oi_competencia_1)
      references dbo.ORG21_COMPETENCIAS (oi_competencia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG21_COM1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG21_COM2 foreign key (oi_competencia_2)
      references dbo.ORG21_COMPETENCIAS (oi_competencia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG21_COM2 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG19_LO1 foreign key (oi_localidad_2)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG19_LO1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG19_LO2 foreign key (oi_localidad_1)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG19_LO2',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG19_PA1 foreign key (oi_pais_1)
      references dbo.ORG19_PAISES (oi_pais)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG19_PA1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG19_PA2 foreign key (oi_pais_2)
      references dbo.ORG19_PAISES (oi_pais)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_BU_REFERENCE_ORG19_PA2',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG19_PR1 foreign key (oi_provincia_1)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG19_PR1 ',@@error , ''	 


alter table dbo.POS17_BUSQUEDAS
   add    constraint FK_POS17_BU_REFERENCE_ORG19_PR2 foreign key (oi_provincia_2)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'FK_POS17_BU_REFERENCE_ORG19_PR2 ',@@error , ''	 


alter table dbo.POS17_RESUL_BUSQ
   add    constraint FK_POS17_RE_REFERENCE_POS17_BU foreign key (oi_busqueda)
      references dbo.POS17_BUSQUEDAS (oi_busqueda)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_RE_REFERENCE_POS17_BU',@@error , ''	 


alter table dbo.POS17_RESUL_BUSQ
   add    constraint FK_POS17_RE_REFERENCE_POS01_CV foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
 	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', ' FK_POS17_RE_REFERENCE_POS01_CV',@@error , ''	 


END




EXEC dbo.sp_NMD_VerifScript @n_Version , @ScripName  ,1 ,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Finalizo la ejecucion',@Result , ''	






/******************************************************************************
 *** FIN DEL ARCHIVO: BASE.POS.Creates.sql
 ******************************************************************************/



