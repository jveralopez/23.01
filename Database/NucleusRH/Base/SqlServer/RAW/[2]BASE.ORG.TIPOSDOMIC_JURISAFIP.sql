/*==============================================================*/
/* Table: ORG33_TIPOS_DOMIC                                     */
/*==============================================================*/
create table dbo.ORG33_TIPOS_DOMIC (
   oi_tipo_dom          int                  not null,
   c_tipo_dom           varchar(30)          not null,
   d_tipo_dom           varchar(100)         not null,
   constraint PK_ORG33_TIPOS_DOMIC primary key (oi_tipo_dom),
   constraint AK_AK_KEY_2_ORG33_TIP_ORG33_TI unique (c_tipo_dom)
)
go

/*==============================================================*/
/* Table: ORG36_JURIS_AFIP                                      */
/*==============================================================*/
create table dbo.ORG36_JURIS_AFIP (
   oi_juris_afip        int                  not null,
   c_juris_afip         varchar(30)          not null,
   d_juris_afip         varchar(100)         not null,
   n_porc_dto_cp        numeric(11,3)        null,
   constraint PK_ORG36_JURIS_AFIP primary key (oi_juris_afip),
   constraint AK_AK__ORG36_JURIS_AF_ORG36_JU unique (c_juris_afip)
)
go
