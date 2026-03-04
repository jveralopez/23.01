
/*==============================================================*/
/* Table: CLI39_SELECTORAS                                      */
/*==============================================================*/
create table dbo.CLI39_SELECTORAS (
   oi_selectora         int                  not null,
   c_selectora          varchar(100)         not null,
   d_selectora          varchar(100)         not null,
   d_mail               varchar(100)         null,
   constraint PK_CLI39_SELECTORAS primary key nonclustered (oi_selectora),
   constraint AK_KEY_CLI39_SELECTORAS unique (c_selectora)
)
go
