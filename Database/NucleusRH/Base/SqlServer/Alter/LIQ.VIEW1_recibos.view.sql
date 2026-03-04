if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[VIEW1_recibos]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[VIEW1_recibos]
GO


SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO



CREATE   VIEW dbo.VIEW1_recibos
AS
SELECT     TOP 100 PERCENT dbo.ORG03_EMPRESAS.c_empresa, dbo.ORG03_EMPRESAS.d_empresa, dbo.PER01_PERSONAL.c_personal, 
                      dbo.PER01_PERSONAL.d_ape_y_nom, dbo.PER02_PERSONAL_EMP.e_numero_legajo, dbo.LIQ19_LIQUIDACION.c_liquidacion, 
                      dbo.LIQ20_TOT_LIQ_PER.oi_tot_liq_per, dbo.LIQ20_TOT_LIQ_PER.n_sbasico, dbo.LIQ20_TOT_LIQ_PER.n_totpag, dbo.LIQ20_TOT_LIQ_PER.n_totsret, 
                      dbo.LIQ20_TOT_LIQ_PER.n_totnsret, dbo.LIQ20_TOT_LIQ_PER.n_tret, dbo.LIQ20_TOT_LIQ_PER.n_tcont, dbo.LIQ20_TOT_LIQ_PER.n_total_ret, 
                      dbo.LIQ20_TOT_LIQ_PER.n_totcont, dbo.LIQ20_TOT_LIQ_PER.n_totsac, dbo.LIQ20_TOT_LIQ_PER.n_totasig, dbo.LIQ12_TIPOS_CONC.c_tipo_concepto,
                       dbo.LIQ12_TIPOS_CONC.d_tipo_concepto, dbo.LIQ14_CONCEPTOS.c_concepto, dbo.LIQ14_CONCEPTOS.d_concepto, 
                      dbo.LIQ20_CONC_LIQ_PER.n_valor, dbo.LIQ20_CONC_LIQ_PER.n_cantidad, dbo.ORG08_CS_COSTO.c_centro_costo, 
                      dbo.ORG08_CS_COSTO.d_centro_costo, dbo.LIQ14_CONCEPTOS.oi_tipo_concepto
FROM         dbo.PER02_PERSONAL_EMP INNER JOIN
                      dbo.LIQ07_PERSONAL ON dbo.PER02_PERSONAL_EMP.oi_personal_emp = dbo.LIQ07_PERSONAL.oi_personal_emp INNER JOIN
                      dbo.PER01_PERSONAL ON dbo.PER02_PERSONAL_EMP.oi_personal = dbo.PER01_PERSONAL.oi_personal INNER JOIN
                      dbo.ORG03_EMPRESAS ON dbo.PER02_PERSONAL_EMP.oi_empresa = dbo.ORG03_EMPRESAS.oi_empresa INNER JOIN
                      dbo.LIQ25_PERSONAL_LIQ ON dbo.LIQ07_PERSONAL.oi_personal_emp = dbo.LIQ25_PERSONAL_LIQ.oi_personal_emp INNER JOIN
                      dbo.LIQ19_LIQUIDACION ON dbo.LIQ25_PERSONAL_LIQ.oi_liquidacion = dbo.LIQ19_LIQUIDACION.oi_liquidacion INNER JOIN
                      dbo.LIQ20_TOT_LIQ_PER ON dbo.LIQ25_PERSONAL_LIQ.oi_tot_liq_pers = dbo.LIQ20_TOT_LIQ_PER.oi_tot_liq_per INNER JOIN
                      dbo.LIQ20_CONC_LIQ_PER ON dbo.LIQ20_TOT_LIQ_PER.oi_tot_liq_per = dbo.LIQ20_CONC_LIQ_PER.oi_tot_liq_per INNER JOIN
                      dbo.LIQ14_CONCEPTOS ON dbo.LIQ20_CONC_LIQ_PER.oi_concepto = dbo.LIQ14_CONCEPTOS.oi_concepto INNER JOIN
                      dbo.ORG08_CS_COSTO ON dbo.LIQ20_CONC_LIQ_PER.oi_centro_costo = dbo.ORG08_CS_COSTO.oi_centro_costo INNER JOIN
                      dbo.LIQ12_TIPOS_CONC ON dbo.LIQ14_CONCEPTOS.oi_tipo_concepto = dbo.LIQ12_TIPOS_CONC.oi_tipo_concepto
WHERE     (dbo.ORG03_EMPRESAS.c_empresa = '01')
ORDER BY dbo.LIQ14_CONCEPTOS.e_secuencia



GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

