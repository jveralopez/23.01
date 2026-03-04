/*==============================================================*/
/* Table: ORG34_ACTIVIDADES                                     */
/*==============================================================*/
create table dbo.ORG34_ACTIVIDADES (
   oi_actividad         int                  not null,
   c_actividad          varchar(30)          not null,
   d_actividad          varchar(100)         not null,
   constraint PK_ORG34_ACTIVIDADES primary key (oi_actividad),
   constraint AK_AK_KEY_2_ORG34_AC_ORG34_AC unique (c_actividad)
)
go

alter table  dbo.ORG03_EMPRESAS
ADD oi_actividad int null
go

alter table dbo.ORG03_EMPRESAS
   add constraint FK_ORG03_EM_REFERENCE_ORG34_AC foreign key (oi_actividad)
      references dbo.ORG34_ACTIVIDADES (oi_actividad)
go