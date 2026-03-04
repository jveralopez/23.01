if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[VIEW_CONCEPTOSaCTIVOS]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[VIEW_CONCEPTOSaCTIVOS]
GO


SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO


CREATE  VIEW dbo.VIEW_CONCEPTOSaCTIVOS
AS
SELECT  TOP 100 PERCENT dbo.LIQ14_CONCEPTOS.c_concepto, dbo.LIQ14_CONCEPTOS.d_concepto, dbo.LIQ14_CONCEPTOS.e_secuencia,dbo.LIQ12_TIPOS_CONC.c_tipo_concepto, 
                      dbo.LIQ12_TIPOS_CONC.d_tipo_concepto, dbo.LIQ14_CONCEPTOS.l_activo
FROM         dbo.LIQ14_CONCEPTOS INNER JOIN
                      dbo.LIQ12_TIPOS_CONC ON dbo.LIQ14_CONCEPTOS.oi_tipo_concepto = dbo.LIQ12_TIPOS_CONC.oi_tipo_concepto
WHERE     (dbo.LIQ14_CONCEPTOS.l_activo = 1)
ORDER BY dbo.LIQ14_CONCEPTOS.e_secuencia



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

