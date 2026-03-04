-- drop constraints

alter table dbo.TTA05_TERMINALES
DROP CONSTRAINT [FK_TTA05_TE_REFERENCE_TTA06_AR]
GO


-- drop columns

alter table dbo.TTA05_TERMINALES
drop column d_paramsoft
go

alter table dbo.TTA05_TERMINALES
drop column d_ubicacionsoft
go

alter table dbo.TTA05_TERMINALES
drop column oi_archivo
go

alter table dbo.TTA05_TERMINALES
drop column d_ubicacion
go


-- add / alters columns

alter table dbo.TTA05_TERMINALES
add d_soft_terminal varchar(100) null 
go


alter table dbo.TTA05_TERMINALES
add d_ubicacion_soft varchar(100) null 
go


alter table dbo.TTA05_TERMINALES
add oi_archivo_nom int null 
go

alter table dbo.TTA05_TERMINALES
alter column oi_archivo_nom int not null 
go

alter table dbo.TTA05_TERMINALES
add d_param_soft_nom varchar(100) null 
go

alter table dbo.TTA05_TERMINALES
add d_archivo_nom varchar(100) null 
go

alter table dbo.TTA05_TERMINALES
add oi_archivo_reg int null 
go

alter table dbo.TTA05_TERMINALES
alter column oi_archivo_reg int not null 
go

alter table dbo.TTA05_TERMINALES
add d_param_soft_reg varchar(100) null 
go

alter table dbo.TTA05_TERMINALES
add d_archivo_reg varchar(100) null 
go



-- add constraints

alter table dbo.TTA05_TERMINALES
   add constraint FK_TTA05_TE_REFERENCE_TTA06_AR foreign key (oi_archivo_reg)
      references dbo.TTA06_ARCHIVOS (oi_archivo)
go


alter table dbo.TTA05_TERMINALES
   add constraint FK_TTA05_TE_REFERENCE_TTA06_ARN foreign key (oi_archivo_nom)
      references dbo.TTA06_ARCHIVOS (oi_archivo)
go

