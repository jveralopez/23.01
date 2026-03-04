/*==============================================================*/
/* Table: CLI35_ITEM_FAC                                        */
/*==============================================================*/
create table dbo.CLI35_ITEM_FAC (
   oi_item_fac          int                  not null,
   c_item_fac           varchar(30)          not null,
   d_item_fac           varchar(100)         not null,
   o_item_fac           varchar(1000)        null,
   constraint PK_CLI35_ITEM_FAC primary key (oi_item_fac),
   constraint AK_CLI35_ITEM_FAC unique (c_item_fac)
)
go
