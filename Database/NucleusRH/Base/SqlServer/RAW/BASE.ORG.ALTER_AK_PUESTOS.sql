alter table ORG04_PUESTOS
drop constraint AK_AK1_ORG_PUESTOS_ORG04_PU
go

alter table ORG04_PUESTOS
add constraint AK_AK1_ORG_PUESTOS_ORG04_PU unique (c_puesto, oi_empresa)
go