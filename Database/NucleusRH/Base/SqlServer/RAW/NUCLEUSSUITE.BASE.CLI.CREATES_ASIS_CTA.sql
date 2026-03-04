/*==============================================================*/
/* Table: CLI38_ASISTENTE_CTA                                   */
/*==============================================================*/
create table dbo.CLI38_ASISTENTE_CTA (
   oi_asis_cta          int                  not null,
   c_asis_cta           varchar(30)          not null,
   d_asis_cta           varchar(100)         not null,
   d_mail               varchar(100)         null,
   constraint PK_CLI38_ASISTENTE_CTA primary key nonclustered (oi_asis_cta),
   constraint AK_CLI38_ASISTENTE_CTA unique (c_asis_cta)
)
go
