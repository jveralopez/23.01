alter table TTA04_PERSONAL
add e_version int null

alter table TTA10_LIQUIDACPERS
add e_version int null

alter table TTA11_ESPERANZAPER
add e_version int null

alter table dbo.TTA04_PERSONAL
   add constraint FK_TTA04_PE_REFERENCE_TTA12_ME foreign key (oi_mensaje)
      references dbo.TTA12_MEN_TERMINAL (oi_mensaje)
go

/*==============================================================*/
/* Table: TTA04_LICENCIAS                                       */
/*==============================================================*/
create table dbo.TTA04_LICENCIAS (
   oi_licencia_per      int                  not null,
   oi_personal_emp      int                  not null,
   l_aprobada           smallint             null,
   constraint PK_TTA04_LICENCIAS primary key (oi_licencia_per)
)
go

alter table dbo.TTA04_LICENCIAS
   add constraint FK_TTA04_LI_REFERENCE_TTA04_PE foreign key (oi_personal_emp)
      references dbo.TTA04_PERSONAL (oi_personal_emp)
go

alter table dbo.TTA04_LICENCIAS
   add constraint FK_TTA04_LI_REFERENCE_PER02_LI foreign key (oi_licencia_per)
      references dbo.PER02_LICEN_PER (oi_licencia_per)
go


alter table dbo.TTA01_TIPOHORAS
add oi_licencia int null
go

alter table dbo.TTA01_TIPOHORAS
   add constraint FK_TTA01_TI_REFERENCE_PER16_LI foreign key (oi_licencia)
      references dbo.PER16_LICENCIAS (oi_licencia)
go