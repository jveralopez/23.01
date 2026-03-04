if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[VIEW_CONCEPTOS_LIQ_MANUALMENTE]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[VIEW_CONCEPTOS_LIQ_MANUALMENTE]
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


CREATE  VIEW dbo.VIEW_CONCEPTOS_LIQ_MANUALMENTE
AS
SELECT     dbo.LIQ19_LIQUIDACION.c_liquidacion, dbo.LIQ14_CONCEPTOS.c_concepto, dbo.LIQ14_CONCEPTOS.d_concepto, 
                      dbo.LIQ25_CONC_MAN_PER.n_valor, dbo.LIQ25_CONC_MAN_PER.n_cantidad
FROM         dbo.LIQ19_LIQUIDACION INNER JOIN
                      dbo.LIQ25_PERSONAL_LIQ ON dbo.LIQ19_LIQUIDACION.oi_liquidacion = dbo.LIQ25_PERSONAL_LIQ.oi_liquidacion INNER JOIN
                      dbo.LIQ25_CONC_MAN_PER ON dbo.LIQ25_PERSONAL_LIQ.oi_personal_liq = dbo.LIQ25_CONC_MAN_PER.oi_personal_liq INNER JOIN
                      dbo.LIQ14_CONCEPTOS ON dbo.LIQ25_CONC_MAN_PER.oi_concepto = dbo.LIQ14_CONCEPTOS.oi_concepto
WHERE     (dbo.LIQ19_LIQUIDACION.c_liquidacion = '20060620')


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

