ALTER TABLE [dbo].[MED04_DETALLE_HIST] 
DROP CONSTRAINT [AK_KEY_2_MED04_DE]
GO

alter table med04_detalle_hist
alter column f_fechahora_visita datetime not null
go

ALTER TABLE [dbo].[MED04_DETALLE_HIST] 
ADD CONSTRAINT [AK_KEY_2_MED04_DE] UNIQUE  NONCLUSTERED 
	(
		[oi_consulta_per],
		[f_fechahora_visita]
	)  ON [PRIMARY] 
GO

