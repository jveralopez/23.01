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
go

/*==============================================================*/
/* Table: ORG19_ZONAS                                           */
/*==============================================================*/
create table dbo.ORG19_ZONAS (
   oi_zona              int                  not null,
   oi_localidad         int                  null,
   c_zona               varchar(30)          not null,
   d_zona               varchar(100)         not null,
   constraint PK_ORG19_ZONAS primary key (oi_zona),
   constraint AK_AK1_ORG_ZONAS_ORG19_ZO unique (c_zona)
)
go

alter table dbo.ORG19_ZONAS
   add constraint FK_ORG19_ZO_REFERENCE_ORG19_LO foreign key (oi_localidad)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
go