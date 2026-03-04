alter table dbo.CLI31_UBICACIONES   
drop constraint FK_CLI31_UB_FK_CLI31__CLI31_EM

alter table dbo.CLI31_UBICACIONES   
drop column org_oi_empresa

alter table dbo.CLI31_UBICACIONES   
add oi_empresa int null
go

alter table dbo.CLI31_UBICACIONES
   add constraint FK_CLI31_UB_FK_CLI31__CLI31_EM foreign key (oi_empresa)
      references dbo.CLI31_EMPRESAS (oi_empresa)
go

