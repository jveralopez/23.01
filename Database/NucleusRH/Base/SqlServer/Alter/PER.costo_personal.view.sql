if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[costo_personal]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view dbo.costo_personal
GO


SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO








CREATE        VIEW DBO.costo_personal
AS

-- CALCULA COSTO DE PERSONAS CON LIQUIDACION
select oi_personal_emp, n_ult_remun costo 
from PER02_PERSONAL_EMP
where  n_ult_remun is not null

UNION all

-- CALCULA COSTO DE PUESTO DE PERSONAS SIN LIQUIDACION
select PER02_PERSONAL_EMP.oi_personal_emp, ORG04_PUESTOS.n_costo costo
from PER02_PERSONAL_EMP, ORG04_PUESTOS
where PER02_PERSONAL_EMP.oi_puesto_ult= ORG04_PUESTOS.oi_puesto
and n_ult_remun is null
UNION all

-- CALCULA COSTO DE PERSONAS SIN LIQUIDACION NI PUESTO
select PER02_PERSONAL_EMP.oi_personal_emp, 0 costo
from PER02_PERSONAL_EMP
where PER02_PERSONAL_EMP.oi_puesto_ult is null
and n_ult_remun is null





GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

