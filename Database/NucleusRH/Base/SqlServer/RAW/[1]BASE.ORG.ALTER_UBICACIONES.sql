/*==============================================================*/
/* Table: ORG03_UBICACIONES                                     */
/*==============================================================*/
alter table dbo.ORG03_UBICACIONES
add
   n_pais               numeric(4)           null,
   n_area               numeric(4)           null,
   n_telefono           numeric(15)          null,   
   c_afip               varchar(30)          null
   c_estado             varchar(30)          null,   
   oi_juris_afip        int                  null
go

alter table dbo.ORG03_UBICACIONES
   add constraint FK_ORG03_UB_REFERENCE_ORG36_JU foreign key (oi_juris_afip)
      references dbo.ORG36_JURIS_AFIP (oi_juris_afip)
go