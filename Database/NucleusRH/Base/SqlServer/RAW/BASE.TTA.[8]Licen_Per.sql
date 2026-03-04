/*==============================================================*/
/* Table: TTA04_LICEN_PER                                       */
/*==============================================================*/
create table dbo.TTA04_LICEN_PER (
   oi_licencia_per      int                  not null,
   oi_personal_emp      int                  not null,
   l_aprobada           smallint             null,
   constraint PK_TTA04_LICEN_PER primary key (oi_licencia_per)
)
go

alter table dbo.TTA04_LICEN_PER
   add constraint FK_TTA04_LI_REFERENCE_TTA04_PE foreign key (oi_personal_emp)
      references dbo.TTA04_PERSONAL (oi_personal_emp)
go

alter table dbo.TTA04_LICEN_PER
   add constraint FK_TTA04_LI_REFERENCE_PER02_LI foreign key (oi_licencia_per)
      references dbo.PER02_LICEN_PER (oi_licencia_per)
go
