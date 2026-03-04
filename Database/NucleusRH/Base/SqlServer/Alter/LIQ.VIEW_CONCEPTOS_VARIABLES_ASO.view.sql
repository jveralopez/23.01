if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[VIEW_CONCEPTOS_VARIABLES_ASO]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[VIEW_CONCEPTOS_VARIABLES_ASO]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO


CREATE  VIEW dbo.VIEW_CONCEPTOS_VARIABLES_ASO
AS
SELECT     TOP 100 PERCENT dbo.LIQ14_CONCEPTOS.e_secuencia, dbo.LIQ14_CONCEPTOS.c_concepto, dbo.LIQ14_CONCEPTOS.d_concepto, 
                      dbo.LIQ09_VARIABLES.c_variable, dbo.LIQ09_VARIABLES.d_variable, dbo.LIQ14_CONC_VAR.c_tipo_parametro
FROM         dbo.LIQ14_CONCEPTOS INNER JOIN
                      dbo.LIQ14_CONC_VAR ON dbo.LIQ14_CONCEPTOS.oi_concepto = dbo.LIQ14_CONC_VAR.oi_concepto INNER JOIN
                      dbo.LIQ09_VARIABLES ON dbo.LIQ14_CONC_VAR.oi_variable = dbo.LIQ09_VARIABLES.oi_variable
WHERE     (dbo.LIQ14_CONCEPTOS.l_activo = 1) AND (dbo.LIQ09_VARIABLES.c_variable = 'np_ajustesalfli')
ORDER BY dbo.LIQ14_CONCEPTOS.e_secuencia, dbo.LIQ14_CONCEPTOS.c_concepto



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

