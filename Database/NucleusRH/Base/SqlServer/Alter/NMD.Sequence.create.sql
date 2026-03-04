create table [dbo].[NMD_SEQUENCE] (
  [oi_sequence] [int] NOT NULL,
  [c_sequence] [varchar] (60) NULL,
  [d_sequence] [varchar] (60) NULL,
  [value] [int] NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[NMD_SEQUENCE] WITH NOCHECK ADD 
	CONSTRAINT [PK_NMD_SEQUENCE] PRIMARY KEY CLUSTERED 
	(
		[oi_sequence]
	)  ON [PRIMARY] 
GO

ALTER TABLE [dbo].[NMD_SEQUENCE] ADD 
	CONSTRAINT [AK_NMD_SEQUENCE_C_SEQ] UNIQUE  NONCLUSTERED 
	(
		[c_sequence]
	)  ON [PRIMARY] 
GO
