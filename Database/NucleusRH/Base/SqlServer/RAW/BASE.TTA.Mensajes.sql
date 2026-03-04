/*==============================================================*/
/* Table: TTA12_MEN_TERMINAL																		*/
/*==============================================================*/
create table dbo.TTA12_MEN_TERMINAL (
oi_mensaje      int                  not null,
c_mensaje      varchar(30)                  not null,
d_mensaje           varchar(100)             null,
constraint PK_TTA12_MEN_TERMINAL primary key (oi_mensaje))

alter table dbo.TTA04_PERSONAL
add oi_mensaje int null

alter table dbo.TTA04_PERSONAL
   add constraint FK_TTA04_PE_REFERENCE_TTA12_ME foreign key (oi_mensaje)
      references dbo.TTA12_MEN_TERMINAL (oi_mensaje)
go

