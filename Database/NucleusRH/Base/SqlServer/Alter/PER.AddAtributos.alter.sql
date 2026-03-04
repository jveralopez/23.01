--

alter table dbo.PER01_PERSONAL
add c_nro_chomba varchar(30) null
go


alter table dbo.PER01_PERSONAL
add c_nro_buzo varchar(30) null
go


alter table dbo.PER01_PERSONAL
add c_nro_campera varchar(30) null
go


alter table dbo.PER01_PERSONAL
add c_nro_documento varchar(30) null
go

update dbo.PER01_PERSONAL
set c_nro_documento = c_dni


alter table dbo.PER01_PERSONAL
drop column c_dni
go

alter table dbo.PER01_PERSONAL
add oi_tipo_documento int null
go

update dbo.PER01_PERSONAL
set oi_tipo_documento = 1

update dbo.PER01_PERSONAL
set c_personal = 'DNI' + ISNULL(c_nro_documento, c_personal)

alter table dbo.PER01_PERSONAL
alter column c_sexo varchar(30) null
go

alter table dbo.PER01_FLIARES_PER
add oi_localidad_nac int null
go

-- add constraints

alter table dbo.PER01_PERSONAL
   add constraint FK_PER01_PE_REFERENCE_PER20_TI foreign key (oi_tipo_documento)
      references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
go

alter table dbo.PER01_FLIARES_PER
   add constraint FK_PER01_FL_REFERENCE_ORG19_LO foreign key (oi_localidad_nac)
      references dbo.ORG19_LOCALIDADES (oi_localidad)
go








