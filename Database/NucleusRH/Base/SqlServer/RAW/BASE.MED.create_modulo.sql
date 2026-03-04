/*==============================================================*/
/* Table: MED01_ESPECIALIDAD                                    */
/*==============================================================*/
create table dbo.MED01_ESPECIALIDAD (
   oi_especialidad      int                  not null,
   c_especialidad       varchar(30)          not null,
   d_especialidad       varchar(100)         not null,
   constraint PK_MED_ESPECIALIDADES primary key (oi_especialidad),
   constraint AK_I_ESP_MED_ESPE unique (c_especialidad)
)
go

/*==============================================================*/
/* Table: MED02_EMPRESAS                                        */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED03_MEDICOS                                         */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED04_ANTEC_PER                                       */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED04_DETALLE_HIST                                    */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED04_EXAMEN_PER                                      */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED04_FACTORES_EVA                                    */
/*==============================================================*/
create table dbo.MED04_FACTORES_EVA (
   oi_factor_eva        int                  not null,
   oi_examen_per        int                  not null,
   oi_factor_examen     int                  not null,
   d_valor              varchar(100)         not null,
   o_factor_eva         varchar(1000)        null,
   constraint PK_MED_FAC_EXA primary key (oi_factor_eva),
   constraint AK_I_FACT_EXA_MED_FAC_ unique (oi_examen_per, oi_factor_examen)
)
go

/*==============================================================*/
/* Table: MED04_HIST_CLINICA                                    */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED04_MEDIC_SUMIN                                     */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED04_PERSONAL                                        */
/*==============================================================*/
create table dbo.MED04_PERSONAL (
   oi_personal_emp      int                  not null,
   constraint PK_MED04_PERSONAL primary key (oi_personal_emp)
)
go

/*==============================================================*/
/* Table: MED05_MOTIVOS_CONS                                    */
/*==============================================================*/
create table dbo.MED05_MOTIVOS_CONS (
   oi_motivo_consulta   int                  not null,
   c_motivo_consulta    varchar(30)          not null,
   d_motivo_consulta    varchar(100)         not null,
   constraint PK_MED_MOTIVOS_CONS primary key (oi_motivo_consulta),
   constraint AK_I_MOT_CONS_MED_MOTI unique (c_motivo_consulta)
)
go

/*==============================================================*/
/* Table: MED06_ENFERMEDADES                                    */
/*==============================================================*/
create table dbo.MED06_ENFERMEDADES (
   oi_enfermedad        int                  not null,
   c_enfermedad         varchar(30)          not null,
   d_enfermedad         varchar(100)         not null,
   constraint PK_MED_ENFERMEDADES primary key (oi_enfermedad),
   constraint AK_I_ENFER_MED_ENFE unique (c_enfermedad)
)
go

/*==============================================================*/
/* Table: MED07_LUGARES_ATEN                                    */
/*==============================================================*/
create table dbo.MED07_LUGARES_ATEN (
   oi_lugar_atencion    int                  not null,
   c_lugar_atencion     varchar(30)          not null,
   d_lugar_atencion     varchar(100)         not null,
   constraint PK_MED_LUGARES_ATENC primary key (oi_lugar_atencion),
   constraint AK_I_LUG_ATENC_MED_LUGA unique (c_lugar_atencion)
)
go

/*==============================================================*/
/* Table: MED08_MEDICAMENTOS                                    */
/*==============================================================*/
create table dbo.MED08_MEDICAMENTOS (
   oi_medicamento       int                  not null,
   c_medicamento        varchar(30)          not null,
   d_medicamento        varchar(100)         not null,
   o_medicamento        varchar(1000)        null,
   constraint PK_MED_MEDICAMENTOS primary key (oi_medicamento),
   constraint AK_I_MEDICA_MED_MEDI unique (c_medicamento)
)
go

/*==============================================================*/
/* Table: MED09_MOTIVOS_MED                                     */
/*==============================================================*/
create table dbo.MED09_MOTIVOS_MED (
   oi_motivo_med        int                  not null,
   c_motivo_med         varchar(30)          not null,
   d_motivo_med         varchar(100)         not null,
   constraint PK_MED_MOTIVOS_MEDICA primary key (oi_motivo_med),
   constraint AK_I_MOT_MEDICA_MED_MOTI unique (c_motivo_med)
)
go

/*==============================================================*/
/* Table: MED10_EXAMENES                                        */
/*==============================================================*/
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
go

/*==============================================================*/
/* Table: MED10_FACTORES                                        */
/*==============================================================*/
create table dbo.MED10_FACTORES (
   oi_factor_examen     int                  not null,
   oi_examen            int                  not null,
   c_factor_examen      varchar(30)          not null,
   d_factor_examen      varchar(100)         not null,
   constraint PK_MED_FACTORES primary key (oi_factor_examen),
   constraint AK_I_FAC_MED_FACT unique (oi_examen, c_factor_examen)
)
go

/*==============================================================*/
/* Table: MED10_TIPOS_EXAMEN                                    */
/*==============================================================*/
create table dbo.MED10_TIPOS_EXAMEN (
   oi_tipo_examen       int                  not null,
   c_tipo_examen        varchar(30)          not null,
   d_tipo_examen        varchar(100)         not null,
   constraint PK_MED_TIPOS_EXAMEN primary key (oi_tipo_examen),
   constraint AK_I_TIPO_EXA_MED_TIPO unique (c_tipo_examen)
)
go

/*==============================================================*/
/* Table: MED11_CATEG_EXAMEN                                    */
/*==============================================================*/
create table dbo.MED11_CATEG_EXAMEN (
   oi_categ_examen      int                  not null,
   c_categ_examen       varchar(30)          not null,
   d_categ_examen       varchar(100)         not null,
   constraint PK_MED_CATEG_EXA primary key (oi_categ_examen),
   constraint AK_I_CATEG_EXA_MED_CATE unique (c_categ_examen)
)
go

/*==============================================================*/
/* Table: MED12_ANTECEDENTES                                    */
/*==============================================================*/
create table dbo.MED12_ANTECEDENTES (
   oi_antecedente       int                  not null,
   oi_tipo_antec        int                  not null,
   c_antecedente        varchar(30)          not null,
   d_antecedente        varchar(100)         not null,
   o_antecedente        varchar(1000)        null,
   constraint PK_MED_ANTECEDENTES primary key (oi_antecedente),
   constraint AK_I_ANTECED_MED_ANTE unique (oi_tipo_antec, c_antecedente)
)
go

/*==============================================================*/
/* Table: MED12_TIPOS_ANTEC                                     */
/*==============================================================*/
create table dbo.MED12_TIPOS_ANTEC (
   oi_tipo_antec        int                  not null,
   c_tipo_antec         varchar(30)          not null,
   d_tipo_antec         varchar(100)         not null,
   constraint PK_MED_TIPOS_ANTECED primary key (oi_tipo_antec),
   constraint AK_I_TIPO_ANT_MED_TIPO unique (c_tipo_antec)
)
go

alter table dbo.MED02_EMPRESAS
   add constraint FK__MED_EMPRE__oi_lo__479D4DD1 foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
go

alter table dbo.MED03_MEDICOS
   add constraint FK__MED_MEDIC__oi_em__1C48DB78 foreign key (oi_empresa_medica)
      references dbo.MED02_EMPRESAS (oi_empresa_medica)
go

alter table dbo.MED03_MEDICOS
   add constraint FK__MED_MEDIC__oi_es__1B54B73F foreign key (oi_especialidad)
      references dbo.MED01_ESPECIALIDAD (oi_especialidad)
go

alter table dbo.MED03_MEDICOS
   add constraint FK__MED_MEDIC__oi_lo__1A609306 foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
go

alter table dbo.MED03_MEDICOS
   add constraint FK__MED_MEDIC__oi_ti__196C6ECD foreign key (oi_tipo_documento)
      references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
go

alter table dbo.MED04_ANTEC_PER
   add constraint FK__MED_ANTEC__oi_an__55615D43 foreign key (oi_antecedente)
      references dbo.MED12_ANTECEDENTES (oi_antecedente)
go

alter table dbo.MED04_ANTEC_PER
   add constraint FK_MED04_AN_REFERENCE_MED04_PE foreign key (oi_personal_emp)
      references dbo.MED04_PERSONAL (oi_personal_emp)
go

alter table dbo.MED04_DETALLE_HIST
   add constraint FK__MED_DETAL__OI_ME__254835CD foreign key (oi_medico)
      references dbo.MED03_MEDICOS (oi_medico)
go

alter table dbo.MED04_DETALLE_HIST
   add constraint FK__MED_DETAL__oi_co__226BC922 foreign key (oi_consulta_per)
      references dbo.MED04_HIST_CLINICA (oi_consulta_per)
go

alter table dbo.MED04_DETALLE_HIST
   add constraint FK__MED_DETAL__oi_lu__235FED5B foreign key (oi_lugar_atencion)
      references dbo.MED07_LUGARES_ATEN (oi_lugar_atencion)
go

alter table dbo.MED04_DETALLE_HIST
   add constraint FK__MED_DETAL__oi_ti__24541194 foreign key (oi_tipo_domicilio)
      references dbo.PER09_TIPOS_DOMIC (oi_tipo_domicilio)
go

alter table dbo.MED04_EXAMEN_PER
   add constraint FK__MED_EXA_P__oi_ex__19A178F7 foreign key (oi_examen)
      references dbo.MED10_EXAMENES (oi_examen)
go

alter table dbo.MED04_EXAMEN_PER
   add constraint FK_MED04_EX_REFERENCE_MED04_PE foreign key (oi_personal_emp)
      references dbo.MED04_PERSONAL (oi_personal_emp)
go

alter table dbo.MED04_FACTORES_EVA
   add constraint FK__MED_FAC_E__oi_ex__391A2450 foreign key (oi_examen_per)
      references dbo.MED04_EXAMEN_PER (oi_examen_per)
go

alter table dbo.MED04_FACTORES_EVA
   add constraint FK__MED_FAC_E__oi_fa__3A0E4889 foreign key (oi_factor_examen)
      references dbo.MED10_FACTORES (oi_factor_examen)
go

alter table dbo.MED04_HIST_CLINICA
   add constraint FK__MED_HIST___oi_en__53CE1A8C foreign key (oi_enfermedad)
      references dbo.MED06_ENFERMEDADES (oi_enfermedad)
go

alter table dbo.MED04_HIST_CLINICA
   add constraint FK__MED_HIST___oi_mo__50F1ADE1 foreign key (oi_motivo_consulta)
      references dbo.MED05_MOTIVOS_CONS (oi_motivo_consulta)
go

alter table dbo.MED04_HIST_CLINICA
   add constraint FK_MED04_HI_REFERENCE_MED04_PE foreign key (oi_personal_emp)
      references dbo.MED04_PERSONAL (oi_personal_emp)
go

alter table dbo.MED04_MEDIC_SUMIN
   add constraint FK__MED_MEDIC__oi_co__16900222 foreign key (oi_consulta_per)
      references dbo.MED04_HIST_CLINICA (oi_consulta_per)
go

alter table dbo.MED04_MEDIC_SUMIN
   add constraint FK__MED_MEDIC__oi_me__1784265B foreign key (oi_medicamento)
      references dbo.MED08_MEDICAMENTOS (oi_medicamento)
go

alter table dbo.MED04_MEDIC_SUMIN
   add constraint FK__MED_MEDIC__oi_mo__18784A94 foreign key (oi_motivo_med)
      references dbo.MED09_MOTIVOS_MED (oi_motivo_med)
go

alter table dbo.MED04_PERSONAL
   add constraint FK_MED04_PE_REFERENCE_PER02_PE foreign key (oi_personal_emp)
      references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
go

alter table dbo.MED10_EXAMENES
   add constraint FK__MED_EXAME__oi_ca__17B93085 foreign key (oi_categ_examen)
      references dbo.MED11_CATEG_EXAMEN (oi_categ_examen)
go

alter table dbo.MED10_EXAMENES
   add constraint FK_MED10_EX_REFERENCE_MED10_TI foreign key (oi_tipo_examen)
      references dbo.MED10_TIPOS_EXAMEN (oi_tipo_examen)
go

alter table dbo.MED10_FACTORES
   add constraint FK__MED_FACTO__oi_ex__3731DBDE foreign key (oi_examen)
      references dbo.MED10_EXAMENES (oi_examen)
go

alter table dbo.MED12_ANTECEDENTES
   add constraint FK__MED_ANTEC__oi_ti__48076225 foreign key (oi_tipo_antec)
      references dbo.MED12_TIPOS_ANTEC (oi_tipo_antec)
go
