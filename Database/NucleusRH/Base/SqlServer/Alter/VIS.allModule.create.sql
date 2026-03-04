/*==============================================================*/
/* Database name:  PHYSICALDATAMODEL_BASE                       */
/* DBMS name:      Microsoft SQL Server 7.x                     */
/* Created on:     26/02/2007 01:18:44 p.m.                     */
/*==============================================================*/


/*==============================================================*/
/* Table: VIS01_VISITAS                                         */
/*==============================================================*/
create table dbo.VIS01_VISITAS (
   oi_visita            int                  not null,
   f_fechahora_ent_est  datetime             not null,
   oi_personal_emp_vis  int                  not null,
   d_empresa_vis        varchar(30)          null,
   oi_estructura        int                  null,
   oi_personal_emp_aut  int                  not null,
   o_motivo             varchar(100)         null,
   c_patente            varchar(30)          null,
   e_cantidad           int                  not null
)
go


alter table dbo.VIS01_VISITAS
   add constraint PK_VIS01_VISITAS primary key  (oi_visita)
go


alter table dbo.VIS01_VISITAS
   add constraint AK_VIS01_VISITAS unique  (f_fechahora_ent_est, oi_personal_emp_vis)
go


/*==============================================================*/
/* Table: VIS01_VISITAS_VIS                                     */
/*==============================================================*/
create table dbo.VIS01_VISITAS_VIS (
   oi_visita_vis        int                  not null,
   oi_visita            int                  not null,
   oi_visitante         int                  not null,
   f_fechahora_sal_est  datetime             null,
   d_credencial         varchar(30)          null,
   f_fechahora_ent      datetime             null,
   f_fechahora_en_ent   datetime             null,
   f_fechahora_sal_ent  datetime             null,
   f_fechahora_sal      datetime             null,
   l_casco              smallint             null,
   l_anteojos           smallint             null
)
go


alter table dbo.VIS01_VISITAS_VIS
   add constraint PK_VIS01_VISITAS_VIS primary key  (oi_visita_vis)
go


alter table dbo.VIS01_VISITAS_VIS
   add constraint AK_AK_VIS01_VISITAS_V_VIS01_VI unique  (oi_visita, oi_visitante)
go


/*==============================================================*/
/* Table: VIS02_VISITANTES                                      */
/*==============================================================*/
create table dbo.VIS02_VISITANTES (
   oi_visitante         int                  not null,
   c_visitante          varchar(30)          not null,
   oi_tipo_documento    int                  not null,
   c_nro_documento      varchar(30)          not null,
   d_apellido           varchar(100)         not null,
   d_nombres            varchar(100)         not null,
   d_ape_y_nom          varchar(200)         not null,
   d_empresa            varchar(100)         null,
   oi_foto              int                  null
)
go


alter table dbo.VIS02_VISITANTES
   add constraint PK_VIS02_VISITANTES primary key  (oi_visitante)
go


alter table dbo.VIS02_VISITANTES
   add constraint AK_AK_VIS02_VISITANTE_VIS02_VI unique  (oi_tipo_documento, c_nro_documento)
go


alter table dbo.VIS02_VISITANTES
   add constraint AK_AK2_VIS02_VISITANT_VIS02_VI unique  (c_visitante)
go


alter table dbo.VIS01_VISITAS
   add constraint FK_VIS01_VI_REF_PER02_PE foreign key (oi_personal_emp_vis)
      references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
go


alter table dbo.VIS01_VISITAS
   add constraint FK_VIS01_VI_REFERENCE_PER02_PE foreign key (oi_personal_emp_aut)
      references dbo.PER02_PERSONAL_EMP (oi_personal_emp)
go


alter table dbo.VIS01_VISITAS
   add constraint FK_VIS01_VI_REFERENCE_ORG02_ES foreign key (oi_estructura)
      references dbo.ORG02_ESTRUCTURAS (oi_estructura)
go


alter table dbo.VIS01_VISITAS_VIS
   add constraint FK_VIS01_VI_REFERENCE_VIS01_VI foreign key (oi_visita)
      references dbo.VIS01_VISITAS (oi_visita)
go


alter table dbo.VIS01_VISITAS_VIS
   add constraint FK_VIS01_VI_REF_VIS02_VI foreign key (oi_visitante)
      references dbo.VIS02_VISITANTES (oi_visitante)
go


alter table dbo.VIS02_VISITANTES
   add constraint FK_VIS02_VI_REFERENCE_PER19_IM foreign key (oi_foto)
      references dbo.PER19_IMAGENES (oi_imagen_per)
go


alter table dbo.VIS02_VISITANTES
   add constraint FK_VIS02_VI_REF_PER20_TI foreign key (oi_tipo_documento)
      references dbo.PER20_TIPOS_DOC (oi_tipo_documento)
go

