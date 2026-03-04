alter table dbo.TTA08_FICHADAS
   drop constraint AK_KEY_3_TTA08_FI 
go

alter table dbo.TTA08_FICHADAS
   add constraint AK_KEY_3_TTA08_FI unique (oi_personal_emp, f_fechahora, oi_liquidacion, l_entrada)
go
