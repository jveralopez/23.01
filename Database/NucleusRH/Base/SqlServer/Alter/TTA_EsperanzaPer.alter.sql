ALTER TABLE [dbo].[TTA03_ESPERANZA] DROP CONSTRAINT [AK_KEY_2_TTA03_ES]

GO
ALTER TABLE [dbo].[TTA03_ESPERANZA] ADD CONSTRAINT [AK_KEY_2_TTA03_ES] UNIQUE  NONCLUSTERED 
	(
		[oi_empresa],
		[oi_calendario],
		[oi_horario],
		[oi_escuadra],
		[f_horainicio]
	)  ON [PRIMARY] 
GO

