ALTER TABLE [dbo].[PER02_PERSONAL_EMP] 
DROP CONSTRAINT [ak_per_personal_emp] 
go

ALTER TABLE [dbo].[PER02_PERSONAL_EMP] 
ADD CONSTRAINT [ak_per_personal_emp] UNIQUE  NONCLUSTERED 
	(
		[oi_empresa],
		[oi_personal]
	)  ON [PRIMARY] 
GO

