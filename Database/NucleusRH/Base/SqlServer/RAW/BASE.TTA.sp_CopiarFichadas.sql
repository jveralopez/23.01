SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


ALTER      procedure dbo.sp_CopiaFichadas 
	@OI_Liquidacion integer,
	@FechaDesde varchar(19), @FechaHasta varchar(19),
	@OI_Personal_Emp integer, @User varchar(64) 
	
AS

DECLARE @oi_Fichada 		int

DECLARE @f_fechahora		datetime
DECLARE @e_numero_legajo	int
DECLARE @l_entrada		smallint
DECLARE @oi_terminal		int
DECLARE @oi_estructura		int
DECLARE @oi_fichadasing		int
DECLARE @o_observaciones     	varchar(1000)

SET NOCOUNT on

/*
sp_CopiaFichadas 7, '2006-08-01 00:00:00','2006-12-01 00:00:00', 2, 'matiasSQL'
*/

---------------------------------------------------------------------------------------
--Elimina los registros de FICHADAS de la tabla de SEGURIDAD
---------------------------------------------------------------------------------------
DELETE FROM NMD_Security
WHERE class = 'NucleusRH.Base.Tiempos_Trabajados.Fichadas.FICHADAS' 
AND id in (
	SELECT oi_fichada
	FROM TTA08_Fichadas
	WHERE oi_liquidacion = @OI_Liquidacion
		AND f_FechaHora >= convert(datetime, @FechaDesde, 120)
		AND f_FechaHora <= convert(datetime, @FechaHasta, 120)
		AND oi_personal_emp = @OI_Personal_Emp
)

---------------------------------------------------------------------------------------
--Elimina los registros de FICHADAS
---------------------------------------------------------------------------------------
DELETE FROM TTA08_Fichadas
WHERE oi_liquidacion = @OI_Liquidacion
	AND f_FechaHora >= convert(datetime, @FechaDesde, 120)
	AND f_FechaHora <= convert(datetime, @FechaHasta, 120)
	AND oi_personal_emp = @OI_Personal_Emp

---------------------------------------------------------------------------------------
--Copia los registros desde la tabla de FICHADASING
---------------------------------------------------------------------------------------

SELECT @oi_Fichada = isnull(max(TTA08_Fichadas.oi_fichada), 0) from TTA08_Fichadas

	print 'ASIGNADO ' + convert(varchar, @oi_Fichada)

DECLARE curFichadasIng CURSOR FOR 
SELECT 	fi.f_fechahora,
	fi.e_numero_legajo,
	fi.l_entrada,
	fi.oi_terminal,
	t.oi_estructura,
	fi.oi_fichadasing,
	fi.o_observaciones     
FROM TTA07_FichadasIng fi, TTA05_Terminales t
WHERE 
	fi.oi_Terminal = t.oi_Terminal
	AND f_FechaHora >= convert(datetime, @FechaDesde, 120)
	AND f_FechaHora <= convert(datetime, @FechaHasta, 120)
	AND fi.oi_personal_emp = @OI_Personal_Emp
	AND fi.c_estado = 'C'

OPEN curFichadasIng

FETCH NEXT FROM curFichadasIng 
INTO 	@f_fechahora, 
	@e_numero_legajo, @l_entrada, @oi_terminal,
	@oi_estructura, @oi_fichadasing, 
	@o_observaciones

WHILE @@FETCH_STATUS = 0  
BEGIN
	SELECT @oi_Fichada = @oi_Fichada + 1
	
	print 'insertando ' + convert(varchar, @oi_Fichada) + ' - ' + convert(varchar, @f_fechahora, 120)
	
	INSERT INTO dbo.TTA08_FICHADAS
	(oi_Fichada, oi_personal_emp, f_fechahora, 
	e_numero_legajo, l_entrada, oi_terminal, 
	oi_estructura, oi_liquidacion, oi_fichadasing,
	o_observaciones)
	
	VALUES(@oi_Fichada, @OI_Personal_Emp, @f_fechahora,
		@e_numero_legajo, @l_entrada, @oi_terminal,
		@oi_estructura, @OI_Liquidacion, @oi_fichadasing,
		@o_observaciones)


	FETCH NEXT FROM curFichadasIng 
	INTO @f_fechahora, 
		@e_numero_legajo, @l_entrada, @oi_terminal,
		@oi_estructura, @oi_fichadasing, 
		@o_observaciones

END

CLOSE curFichadasIng
DEALLOCATE curFichadasIng

---------------------------------------------------------------------------------------
--Copia los registros recien insertos a la tabla de SEGURIDAD
---------------------------------------------------------------------------------------
INSERT INTO NMD_Security
SELECT 	'NucleusRH.Base.Tiempos_Trabajados.Fichadas.FICHADAS' class,
	f.oi_fichada,
	1 version,
	@User new_user,
	getdate() new_time,
	null edit_user,
	null edit_time,
	null del_user,
	null del_time,
	null policy
FROM TTA08_Fichadas f
WHERE oi_liquidacion = @OI_Liquidacion
	AND f_FechaHora >= convert(datetime, @FechaDesde, 120)
	AND f_FechaHora <= convert(datetime, @FechaHasta, 120)
	AND oi_personal_emp = @OI_Personal_Emp


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
