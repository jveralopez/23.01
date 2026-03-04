/*==============================================================*/
/* Table: CLI36_COAD_COEF                                       */
/*==============================================================*/
create table dbo.CLI36_COAD_COEF (
   oi_coadi_coef        int                  not null,
   oi_coeficiente       int                  not null,
   n_coadi_coef         numeric(11,3)        null,
   o_coadi_coef         varchar(1000)        null,
   oi_concepto_adic     int                  not null,
   constraint PK_CLI36_COAD_COEF primary key (oi_coadi_coef),
   constraint AK_CLI36_COAD_COEF unique (oi_coeficiente, oi_concepto_adic)
)
go

/*==============================================================*/
/* Table: CLI36_COEFICIENTES                                    */
/*==============================================================*/
create table dbo.CLI36_COEFICIENTES (
   oi_coeficiente       int                  not null,
   oi_version           int                  not null,
   n_rem_prod           numeric(11,3)        not null,
   n_rem_imp            numeric(11,3)        not null,
   n_no_rem_prod        numeric(11,3)        not null,
   n_no_rem_imp         numeric(11,3)        not null,
   o_coef               varchar(1000)        null,
   constraint PK_CLI36_COEFICIENTES primary key (oi_coeficiente),
   constraint AK_CLI36_COEFICIENTES unique (oi_version)
)
go

/*==============================================================*/
/* Table: CLI36_DOC_DIGITAL                                     */
/*==============================================================*/
create table dbo.CLI36_DOC_DIGITAL (
   oi_doc_dig_neg       int                  not null,
   oi_negociacion       int                  not null,
   oi_doc_digital       int                  null,
   d_docum_dig_cv       varchar(100)         not null,
   f_alta               datetime             not null,
   o_docum_dig_cv       varchar(1000)        null,
   constraint PK_CLI36_DOC_DIGITAL primary key (oi_doc_dig_neg),
   constraint ak_CLI36_DOC_DIGITAL unique (oi_negociacion)
)
go

/*==============================================================*/
/* Table: CLI36_EXCEP_CLASE                                     */
/*==============================================================*/
create table dbo.CLI36_EXCEP_CLASE (
   oi_exec_clase        int                  not null,
   oi_coeficiente       int                  not null,
   l_factura            smallint             null,
   n_coef               numeric(11,3)        null,
   o_exec_clase         varchar(1000)        null,
   oi_clase_concepto    int                  not null,
   constraint PK_CLI36_EXCEP_CLASE primary key (oi_exec_clase),
   constraint AK_CLI36_EXCEP_CLASE unique (oi_coeficiente, oi_clase_concepto)
)
go

/*==============================================================*/
/* Table: CLI36_EXCEP_CONC                                      */
/*==============================================================*/
create table dbo.CLI36_EXCEP_CONC (
   oi_exec_conc         int                  not null,
   oi_coeficiente       int                  null,
   l_factura            smallint             null,
   n_coef               numeric(11,3)        null,
   c_tipo               varchar(30)          null,
   o_exec_clase         varchar(1000)        null,
   oi_concepto          int                  not null,
   constraint PK_CLI36_EXCEP_CONC primary key (oi_exec_conc),
   constraint AK_CLI36_EXCEP_CONC unique (oi_coeficiente, oi_concepto)
)
go

/*==============================================================*/
/* Table: CLI36_ITEM_NEG                                        */
/*==============================================================*/
create table dbo.CLI36_ITEM_NEG (
   oi_item_fac_neg      int                  not null,
   oi_negociacion       int                  null,
   oi_item_fac          int                  null,
   n_valor              numeric(11,3)        null,
   e_cantidad           int                  null,
   e_cant_min           int                  null,
   e_cant_max           int                  null,
   constraint PK_CLI36_ITEM_NEG primary key (oi_item_fac_neg),
   constraint ak_CLI36_ITEM_NEG unique (oi_negociacion, oi_item_fac)
)
go

/*==============================================================*/
/* Table: CLI36_NEGOCIACION                                     */
/*==============================================================*/
create table dbo.CLI36_NEGOCIACION (
   oi_negociacion       int                  not null,
   oi_cliente_emp       int                  not null,
   oi_tipo_serv         int                  not null,
   c_negociacion        varchar(30)          not null,
   d_negociacion        varchar(100)         not null,
   oi_ejecutivo         int                  not null,
   f_alta               datetime             not null,
   oi_cond_pago         int                  null,
   oi_dia_pago          int                  null,
   o_negociacion        varchar(1000)        null,
   c_estado             varchar(30)          not null,
   l_orden_compra       smallint             null,
   c_entrega_comp       varchar(30)          null,
   c_entrega_ropa       varchar(30)          null,
   d_entrega_ropa       varchar(100)         null,
   l_lib_sanitaria      smallint             null,
   c_liquidacion        varchar(30)          null,
   n_dias               int                  null,
   n_horas_dias         int                  null,
   n_horas_mes          int                  null,
   l_empresa            smallint             null,
   l_cliente            smallint             null,
   n_dot_proy           int                  null,
   n_suel_prom          numeric(11,3)        null,
   n_valor_proy         numeric(11,3)        null,
   c_apr_coeficientes   varchar(30)          null,
   oi_mot_cam_coef      int                  null,
   o_coeficiente        varchar(1000)        null,
   c_apr_credito        varchar(30)          null,
   oi_mot_cam_cred      int                  null,
   o_credito            varchar(1000)        null,
   c_apr_comercial      varchar(30)          null,
   oi_mot_cam_com       int                  null,
   oi_domic_exp         int                  null,
   oi_provincia         int                  null,
   o_comercial          varchar(1000)        null,
   l_licitacion         smallint             null,
   f_ult_asignacion     datetime             null,
   constraint PK_CLI36_NEGOCIACION primary key (oi_negociacion),
   constraint AK_CLI36_NEGOCIACION unique (c_negociacion)
)
go

/*==============================================================*/
/* Table: CLI36_TPOCOS_COE                                      */
/*==============================================================*/
create table dbo.CLI36_TPOCOS_COE (
   oi_tipo_cos_coef     int                  not null,
   oi_coeficiente       int                  not null,
   n_tipo_cos_coef      numeric(11,3)        null,
   o_tipo_cos_coef      varchar(1000)        null,
   l_iva                smallint             null,
   oi_tipo_costo        int                  not null,
   constraint PK_TCosto_Coeficientes primary key (oi_tipo_cos_coef),
   constraint aK_TCosto_Coeficientes unique (oi_coeficiente, oi_tipo_costo)
)
go

/*==============================================================*/
/* Table: CLI36_VERSIONES                                       */
/*==============================================================*/
create table dbo.CLI36_VERSIONES (
   oi_version           int                  not null,
   oi_negociacion       int                  not null,
   n_version            numeric(11,3)        not null,
   f_creacion           datetime             null,
   c_usua_creacion      varchar(30)          null,
   o_crea_ver           varchar(1000)        null,
   c_estado_ver         varchar(30)          null,
   f_ult_modif          datetime             null,
   c_usua_modif         varchar(30)          null,
   c_estado_aprov       varchar(30)          null,
   f_aprobacion         datetime             null,
   c_usua_aprob         varchar(30)          null,
   o_est_aprob          varchar(1000)        null,
   constraint PK_CLI36_VERSIONES primary key (oi_version),
   constraint ak_CLI36_VERSIONES unique (oi_negociacion, n_version)
)
go

alter table dbo.CLI36_COAD_COEF
   add constraint FK_CLI36_COAD_COEF_FK_CLI36_COEFICIENTES foreign key (oi_coeficiente)
      references dbo.CLI36_COEFICIENTES (oi_coeficiente)
go

alter table dbo.CLI36_COEFICIENTES
   add constraint FK_CLI36_COEFICIENTES_FK_CLI36_VERSIONES foreign key (oi_version)
      references dbo.CLI36_VERSIONES (oi_version)
go

alter table dbo.CLI36_DOC_DIGITAL
   add constraint FK_CLI36_DOC_DIGITAL_FK_CLI25_DOC_DIGITAL foreign key (oi_doc_digital)
      references dbo.CLI25_DOC_DIGITAL (oi_doc_digital)
go

alter table dbo.CLI36_DOC_DIGITAL
   add constraint FK_CLI36_DOC_DIGITAL_FK_CLI36_NEGOCIACION foreign key (oi_negociacion)
      references dbo.CLI36_NEGOCIACION (oi_negociacion)
go

alter table dbo.CLI36_EXCEP_CLASE
   add constraint FK_CLI36_EXCEP_CLASE_FK_CLI36_COEFICIENTES foreign key (oi_coeficiente)
      references dbo.CLI36_COEFICIENTES (oi_coeficiente)
go

alter table dbo.CLI36_EXCEP_CONC
   add constraint FK_CLI36_EXCEP_CONC_FK_CLI36_COEFICIENTES foreign key (oi_coeficiente)
      references dbo.CLI36_COEFICIENTES (oi_coeficiente)
go

alter table dbo.CLI36_ITEM_NEG
   add constraint FK_CLI36_ITEM_NEG_FK_CLI35_ITEM_FAC foreign key (oi_item_fac)
      references dbo.CLI35_ITEM_FAC (oi_item_fac)
go

alter table dbo.CLI36_ITEM_NEG
   add constraint FK_CLI36_ITEM_NEG_FK_CLI36_NEGOCIACION foreign key (oi_negociacion)
      references dbo.CLI36_NEGOCIACION (oi_negociacion)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI03_CLIENTE_EMP foreign key (oi_cliente_emp)
      references dbo.CLI03_CLIENTE_EMP (oi_cliente_emp)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI09_EJECUTIVOS foreign key (oi_ejecutivo)
      references dbo.CLI09_EJECUTIVOS (oi_ejecutivo)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI15_TIPOS_SERV foreign key (oi_tipo_serv)
      references dbo.CLI15_TIPOS_SERV (oi_tipos_serv)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI17_DIAS_PAGO foreign key (oi_dia_pago)
      references dbo.CLI17_DIAS_PAGO (oi_dia_pago)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI24_COND_PAGO foreign key (oi_cond_pago)
      references dbo.CLI24_COND_PAGO (oi_cond_pago)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI30_MOT_CAM_CRED foreign key (oi_mot_cam_cred)
      references dbo.CLI30_MOT_CAM_CRED (oi_mot_cam_cred)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI33_MOT_COMERCIAL foreign key (oi_mot_cam_com)
      references dbo.CLI33_MOT_COMERCIAL (oi_mot_cam_com)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI34_MOT_COEFICIENTE foreign key (oi_mot_cam_coef)
      references dbo.CLI34_MOT_COEFICIENTE (oi_mot_cam_coef)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_CLI03_DOMIC_EXP foreign key (oi_domic_exp)
      references dbo.CLI03_DOMIC_EXP (oi_domic_exp)
go

alter table dbo.CLI36_NEGOCIACION
   add constraint FK_CLI36_NEGOCIACION_FK_ORG35_PROVINCIAS foreign key (oi_provincia)
      references ORG35_PROVINCIAS
go

alter table dbo.CLI36_TPOCOS_COE
   add constraint FK_CLI36_TPOCOS_COE_FK_CLI36_COEFICIENTES foreign key (oi_coeficiente)
      references dbo.CLI36_COEFICIENTES (oi_coeficiente)
go

alter table dbo.CLI36_VERSIONES
   add constraint FK_CLI36_VERSIONES_FK_CLI36_NEGOCIACION foreign key (oi_negociacion)
      references dbo.CLI36_NEGOCIACION (oi_negociacion)
go
