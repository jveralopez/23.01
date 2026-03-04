/*==============================================================*/
/* Table: POS25_IMAGENES                                        */
/*==============================================================*/
create table dbo.POS25_IMAGENES (
   oi_imagen            int                  not null,
   e_tamanio            int                  null,
   f_creacion           datetime             null,
   o_imagen             varchar(1000)        null,
   constraint PK_POS25_IMAGENES primary key (oi_imagen)
)
go

/*==============================================================*/
/* Table: POS25_BINARIOS                                        */
/*==============================================================*/
create table dbo.POS25_BINARIOS (
   oi_imagen            int                  null,
   c_bloque             int                  not null,
   t_datos              varchar(4000)        null,
   constraint AK_AK_BINARIOS_POS25_BI unique (oi_imagen, c_bloque)
)
go

alter table dbo.POS25_BINARIOS
   add constraint FK_POS25_BI_REFERENCE_POS25_IM foreign key (oi_imagen)
      references dbo.POS25_IMAGENES (oi_imagen)
go

DROP TABLE POS01_IMAGENES_CV

/*==============================================================*/
/* Table: POS01_IMAGENES_CV                                     */
/*==============================================================*/
create table dbo.POS01_IMAGENES_CV (
   oi_imagen_cv         int                  not null,
   oi_cv                int                  not null,
   oi_imagen            int                  not null,
   d_imagen_cv          varchar(100)         not null,
   o_imagen_cv          varchar(1000)        null,
   constraint PK_POS_IMAGENES_POS primary key nonclustered (oi_imagen_cv)
)
go

alter table dbo.POS01_IMAGENES_CV
   add constraint FK_POS01_IM_REFERENCE_POS25_IM foreign key (oi_imagen)
      references dbo.POS25_IMAGENES (oi_imagen)
go

alter table dbo.POS01_IMAGENES_CV
   add constraint FK_CV_IMAGENES foreign key (oi_cv)
      references dbo.POS01_CV (oi_cv)
go
