if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_NMD_LogScript]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[sp_NMD_LogScript]
GO
SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


CREATE Procedure dbo.sp_NMD_LogScript @oi_Version int ,@d_ScriptName varchar (100),@d_modulo varchar (100),@d_Paso varchar (100),@n_Error int ,@Observaciones varchar (500)
as




if Not exists (select * from dbo.sysobjects where id = object_id(N'[NMD_LogScripts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
		CREATE TABLE dbo.[NMD_LogScripts] (
			[oi_Version] [int] NOT NULL ,
			[f_Fecha] [DateTime] NULL ,
			[d_ScriptName] [varchar] (100),
			[d_modulo] [varchar] (100),
			[d_Paso] [varchar] (100),
			[n_Error] [int] ,
			[Observaciones] [varchar] (500),) 
END


INSERT INTO dbo.[NMD_LogScripts] 
(oi_Version,f_Fecha,d_ScriptName,d_modulo,d_Paso,n_Error,Observaciones )
SELECT @oi_Version , getdate() ,@d_ScriptName ,@d_modulo ,@d_Paso ,@n_Error ,@Observaciones 




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

