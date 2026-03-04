/*==============================================================*/
/* Database name:  PHYSICALDATAMODEL_BASE                       */
/* DBMS name:      Microsoft SQL Server 7.x                     */
/* Created on:     19/02/2007 11:18:52                          */
/*==============================================================*/
/* las zonas se agregaron para el manejo de zonas de buenos aires */

/*==============================================================*/
/* Table: ORG19_ZONAS                                           */
/*==============================================================*/
create table dbo.ORG19_ZONAS (
   oi_zona              int                  not null,
   oi_provincia         int                  null,
   c_zona               varchar(30)          not null,
   d_zona               varchar(100)         not null
)
go


alter table dbo.ORG19_ZONAS
   add constraint AK_AK1_ORG_ZONAS_ORG19_ZO unique  (c_zona, oi_provincia)
go


alter table dbo.ORG19_ZONAS
   add constraint PK_ORG19_ZONAS primary key  (oi_zona)
go


alter table dbo.ORG19_ZONAS
   add constraint FK_ORG19_ZO_REFERENCE_ORG19_PR foreign key (oi_provincia)
      references dbo.ORG19_PROVINCIAS (oi_provincia)
go
