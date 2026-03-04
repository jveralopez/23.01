create table dbo.ORG37_PUESTOS_AFIP (
   oi_puesto_afip       int                  not null,
   c_puesto_afip        varchar(30)          not null,
   d_puesto_afip        varchar(100)         not null,
   constraint PK_ORG37_Puesto_AFIP primary key (oi_puesto_afip),
   constraint AK_ORG37_Puesto_AFIP unique (c_puesto_afip)
)
go
