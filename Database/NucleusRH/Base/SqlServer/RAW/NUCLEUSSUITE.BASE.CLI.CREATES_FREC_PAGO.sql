/*==============================================================*/
/* Table: CLI37_FREC_PAGO                                       */
/*==============================================================*/
create table dbo.CLI37_FREC_PAGO (
   oi_frec_pago         int                  not null,
   c_frec_pago          varchar(30)          not null,
   d_frec_pago          varchar(100)         not null,
   constraint PK_CLI37_FREC_PAGO primary key nonclustered (oi_frec_pago),
   constraint AK_CLI37_FREC_PAGO unique (c_frec_pago)
)
go
