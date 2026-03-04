if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_HorariosAModificar]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[sp_HorariosAModificar]
GO


SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO




CREATE       procedure dbo.sp_HorariosAModificar 
	@OI_Personal_Emp integer
	
AS

DECLARE @f_inicio     	varchar(30)
DECLARE @f_fin     	varchar(30)

DECLARE curFechasNovedades CURSOR FOR 
	SELECT 	convert(varchar, f_fecInicio, 120) fecInicio,
		convert(varchar, f_fecfin, 120) fecfin
	FROM	tta04_novedades 
	WHERE 	oi_personal_emp = @OI_Personal_Emp 
		AND l_actualizado = 0


SET NOCOUNT on


---------------------------------------------------------------------------------------
-- Obtiene las novedades nuevas que necesitarán modificar TTA11_ESPERANZAPER
---------------------------------------------------------------------------------------

OPEN curFechasNovedades

FETCH NEXT FROM curFechasNovedades 
INTO 	@f_inicio, 
	@f_fin

WHILE @@FETCH_STATUS = 0  
BEGIN
	
	--print 'horarios ' + @f_inicio + ' ----- ' + @f_fin
	

	UPDATE tta04_horariospers 
	SET l_actualizado = 0
	WHERE oi_personal_emp = @OI_Personal_Emp
	AND
	(
	  (f_fechaInicio >= CONVERT(DateTime, @f_inicio, 120) AND f_fechaInicio <= CONVERT(DateTime, @f_fin, 120) )
	    OR
	  (f_fechaFin >= CONVERT(DateTime, @f_inicio, 120) AND f_fechaFin <= CONVERT(DateTime, @f_fin, 120) )
	    OR
	  (f_fechaInicio < CONVERT(DateTime, @f_inicio, 120) AND f_fechaFin >  CONVERT(DateTime, @f_fin, 120) )
	    OR
	  (f_fechaInicio < CONVERT(DateTime, @f_inicio, 120) AND f_fechaFin is null)
	)

	
	FETCH NEXT FROM curFechasNovedades 
	INTO 	@f_inicio, 
		@f_fin

END

CLOSE curFechasNovedades
DEALLOCATE curFechasNovedades


---------------------------------------------------------------------------------------
-- Actualiza las novedades como ya procesadas 
---------------------------------------------------------------------------------------
UPDATE tta04_novedades
SET l_actualizado = 1
WHERE oi_personal_emp = @OI_Personal_Emp 
	AND l_actualizado = 0




GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

