
/*==============================================================*/
/* Table: PER33_ART                                             */
/*==============================================================*/
create table dbo.PER33_ART (
   oi_art               int                  not null,
   c_art                varchar(30)          not null,
   d_art                varchar(100)         not null,
   d_domicilio          varchar(100)         null,
   te_telefono          varchar(50)          null
)
go


alter table dbo.PER33_ART
   add constraint PK_PER33_ART primary key  (oi_art)
go


alter table dbo.PER33_ART
   add constraint AK_AK_KEY_2_PER33_AR_PER33_AR unique  (c_art)
go

