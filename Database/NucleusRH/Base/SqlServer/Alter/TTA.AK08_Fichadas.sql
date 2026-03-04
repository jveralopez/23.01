if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[AK_KEY_3_TTA08_FI]') and OBJECTPROPERTY(id, N'IsConstraint') = 1)
begin
	alter table dbo.TTA08_FICHADAS 
   		drop constraint AK_KEY_3_TTA08_FI
end

alter table dbo.TTA08_FICHADAS 
	add constraint AK_KEY_3_TTA08_FI unique (oi_personal_emp, f_fechahora, oi_liquidacion)
