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
go

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
go

/*==============================================================*/
/* Table: SAN03_PERSONAL                                        */
/*==============================================================*/
create table dbo.SAN03_PERSONAL (
   oi_personal_emp      int                  not null,
   constraint PK_SAN03_PERSONAL primary key (oi_personal_emp)
)
go

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
go

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
go

alter table dbo.SAN01_MOTIVOS_SANC
   add constraint FK_SAN01_MO_REFERENCE_SAN02_SA foreign key (oi_sancion)
      references dbo.SAN02_SANCIONES (oi_sancion)
go

alter table dbo.SAN03_PERSONAL
   add constraint FK_SAN03_PE_REFERENCE_PER02_PE foreign key (oi_personal_emp)
      references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
go

alter table dbo.SAN03_SANCION_PER
   add constraint FK_SAN03_SA_REFERENCE_SAN01_MO foreign key (oi_motivo_sancion)
      references dbo.SAN01_MOTIVOS_SANC (oi_motivo_sancion)
go

alter table dbo.SAN03_SANCION_PER
   add constraint FK_SAN03_SA_REFERENCE_SAN02_SA foreign key (oi_sancion)
      references dbo.SAN02_SANCIONES (oi_sancion)
go

alter table dbo.SAN03_SANCION_PER
   add constraint FK_SAN03_SA_REFERENCE_SAN03_PE foreign key (oi_personal_emp)
      references dbo.SAN03_PERSONAL (oi_personal_emp)
go

alter table dbo.SAN03_SUSP_PER
   add constraint FK_SAN03_SU_REFERENCE_SAN03_SA foreign key (oi_sancion_per)
      references dbo.SAN03_SANCION_PER (oi_sancion_per)
go
