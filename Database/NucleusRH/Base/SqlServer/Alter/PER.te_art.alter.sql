alter table dbo.PER33_ART
drop column te_telefono
go

alter table dbo.PER33_ART
add te_art varchar(50) null 
go