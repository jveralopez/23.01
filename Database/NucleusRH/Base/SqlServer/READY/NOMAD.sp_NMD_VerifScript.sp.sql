if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_NMD_VerifScript]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[sp_NMD_VerifScript]
GO
SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


CREATE Procedure dbo.sp_NMD_VerifScript @n_Version int, @d_script varchar(50), @l_Resultado int, @l_ResultadoSP int OUTPUT
as


declare @nCant int 
declare @l_finalizo  int 
declare @oi_Version_Max  int 

if Not exists (select * from dbo.sysobjects where id = object_id(N'[NMD_Scripts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
		CREATE TABLE dbo.[NMD_Scripts] (
			[oi_Version] [int] NOT NULL ,
			[c_Version] [int] NOT NULL ,
			[oi_Fecha] [DateTime] NULL ,
			[d_usuario] [varchar] (50),
			[d_script] [varchar] (50),
			[l_finalizo] [int] ,
		) 
END



SELECT @nCant = isnull(count (*),0 ) FROM dbo.NMD_Scripts WHERE c_Version = @n_Version and d_script = @d_script 
SELECT @l_finalizo  = isnull(l_finalizo ,0) FROM dbo.NMD_Scripts WHERE c_Version = @n_Version and d_script = @d_script 

SELECT @l_finalizo  = isnull(@l_finalizo   ,0) 

SELECT @oi_Version_Max = isnull( MAX(oi_Version  ),0) FROM dbo.NMD_Scripts
 
-- No se corrio nunca
IF @nCant = 0 and @l_finalizo  =0
BEGIN

		INSERT INTO dbo.NMD_Scripts
			(oi_Version  ,c_Version   ,oi_Fecha    ,d_usuario   ,d_script    ,l_finalizo  )
		Select oi_Version = @oi_Version_Max + 1 ,
			c_Version = @n_Version  ,
			oi_Fecha  = getdate()  ,
			d_usuario =  SYSTEM_USER ,
			d_script  = @d_script   ,
			l_finalizo = 0 
			SELECT @l_ResultadoSP =0 -- El script esta iniciado
END

IF @nCant = 1 
BEGIN
	IF @l_finalizo  =0	-- EL script inicio su ejecucion
	BEGIN

		IF @l_Resultado = 0	-- El script corrio antes pero no termino bien
		    BEGIN

			UPDATE  dbo.NMD_Scripts
			SET 	oi_Fecha  = getdate()  ,
				d_usuario =  SYSTEM_USER 
			WHERE oi_Version = @oi_Version_Max 
		    END
		ELSE 
		    BEGIN		-- Cierra la ejecucion del script para que
					-- no se vuelva a correr	
			UPDATE  dbo.NMD_Scripts
			SET 	l_finalizo = 1
			WHERE oi_Version = @oi_Version_Max 
			SELECT @l_ResultadoSP =0 -- El script se cerro
		    END	
	END
	ELSE
		BEGIN

			SELECT @l_ResultadoSP = 999 -- El script ya corrio 
		END
			
END








GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


