ALTER TABLE [dbo].[MED12_TIPOS_ANTEC] DROP CONSTRAINT [AK_I_TIPO_ANT_MED_TIPO]

GO

alter table med12_tipos_antec
drop column i_tipo_antec
go

alter table med12_tipos_antec
add c_tipo_antec varchar(30) null 
go

alter table med12_tipos_antec
alter column c_tipo_antec varchar(30) not null 
go

ALTER TABLE [dbo].[MED12_TIPOS_ANTEC] ADD CONSTRAINT [AK_I_TIPO_ANT_MED_TIPO] UNIQUE  NONCLUSTERED 
	(
		[c_tipo_antec]
	)  ON [PRIMARY] 
GO

