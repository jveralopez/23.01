create table dbo.ORG30_RESPONSABILIDADES (
   oi_responsabilidad   int                  not null,
   c_responsabilidad    varchar(30)          not null,
   d_responsabilidad    varchar(100)         not null,
   constraint PK_ORG30_RESPONSABILIDADES primary key (oi_responsabilidad),
   constraint AK_KEY_2_ORG30_RE unique (c_responsabilidad)
)
go


create table dbo.ORG30_RESPONSABLES (
   oi_responsable       int                  not null,
   oi_responsabilidad   int                  not null,
   oi_personal          int                  not null,
   oi_estructura        int                  not null,
   constraint PK_ORG30_RESPONSABLES primary key (oi_responsable),
   constraint AK_KEY_2_ORG30_RES unique (oi_responsabilidad, oi_personal, oi_estructura)
)
go


alter table dbo.ORG30_RESPONSABLES
   add constraint FK_ORG30_RE_REFERENCE_PER01_PE foreign key (oi_personal)
      references dbo.PER01_PERSONAL (oi_personal)
go

alter table dbo.ORG30_RESPONSABLES
   add constraint FK_ORG30_RE_REFERENCE_ORG30_RE foreign key (oi_responsabilidad)
      references dbo.ORG30_RESPONSABILIDADES (oi_responsabilidad)
go

alter table dbo.ORG30_RESPONSABLES
   add constraint FK_ORG30_RE_REFERENCE_ORG02_ES foreign key (oi_estructura)
      references dbo.ORG02_ESTRUCTURAS (oi_estructura)
go