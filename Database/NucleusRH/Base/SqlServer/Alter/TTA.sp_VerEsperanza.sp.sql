if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_VerEsperanza]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[sp_VerEsperanza]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO





ALTER      PROCEDURE dbo.sp_VerEsperanza @nPersona int , @sFecDesde varchar(8), @sFecHasta varchar(8)
AS
declare @horario int  
declare @oi_Empresa int 
declare @oi_calendario int 
DECLARE @oi_escuadra INT 
DECLARE @oi_horario INT 
declare @dFecDesde datetime
declare @dFecHasta datetime
declare @nDias int
declare @f_fechaIni datetime
DECLARE @e_avance   int 
DECLARE @D_escuadra   varchar(100)



SET NOCOUNT ON

 
CREATE TABLE [#temp ] (
	[NombreDia] [varchar] (9)  NULL ,
	[Dia] [int] NULL ,
	[Hora_Entrada] [varchar] (5)  NULL ,
	[Hora_Salida] [varchar] (5) NULL ,
	[Hora] [varchar] (100) NOT NULL ,
	[oi_escuadra] [varchar] (30) NULL ,
	[UnidadOrg] [varchar] (100) NOT NULL ,
	[DiaInicio] [int] NULL ,
	[DiaFin] [int] NULL ,
	[oi_horario] [int] NULL ,
	[Fecha] [datetime]  NULL 
)

CREATE TABLE [#temp2] (
	[NombreDia] [varchar] (9) NULL ,
	[Dia] [int] NULL ,
	[Hora_Entrada] [varchar] (5) NULL ,
	[Hora_Salida] [varchar] (5) NULL ,
	[Hora] [varchar] (100) NOT NULL ,
	[oi_escuadra] [varchar] (30) NULL ,
	[UnidadOrg] [varchar] (100) NOT NULL ,
	[DiaInicio] [int] NULL ,
	[DiaFin] [int] NULL ,
	[oi_horario] [int] NULL ,
	[Fecha] [datetime]  NULL 
)



--- Obtengo las EMPRESA y CALENDARIO 
select @oi_empresa = oi_empresa , @oi_calendario = oi_calendario_ult
from per02_personal_emp where oi_personal_emp = @nPersona 


--- Obtengo los días + - a buscar
SELECT @nDias = d_valor from dbo.ORG26_PARAMETROS where c_parametro='DiasGenEsperanza'


--- defino las fechas en base a los parametros de entrada
if @sFecDesde='' 
begin
	select @dFecDesde=convert(datetime, getdate() - @nDias ,120) , @dFecHasta= convert(datetime, getdate() + @nDias ,120)
end 
else
begin
		select @dFecDesde=convert(datetime, @sFecDesde ,120) , @dFecHasta= convert(datetime, @sFecHasta,120)
end 

if @sFecDesde=@sFecHasta and @sFecDesde<>''
begin
	select @dFecDesde =  dateadd(day,-1 ,@dFecDesde )  , @dFecHasta= dateadd(day,1 ,@dFecHasta )  
end




-- ==============================================================
-- Inserta en Temporal los registros de la esperanza de la persona
-- que tienen para la misma posicion entrada y salida en días diferentes
-- dentro del rango de búqueda fijado anteriormente
-- ==============================================================


INSERT INTO #temp 
(NombreDia,Dia	,Hora_Entrada	,Hora_Salida	,Hora	,oi_escuadra
,UnidadOrg	,DiaInicio	,DiaFin		,oi_horario )
select 
NombreDia = case datepart(dw,EZ.f_fechorainicio)  when 1 then 'Lunes' 
					when 2 then 'Martes' 
					when 3 then 'Miercoles' 
					when 4 then 'Jueves' 
					when 5 then 'Viernes' 
					when 6 then 'Sabado' 
					when 7 then 'Domingo' end
,Dia = datepart(year, EZ.f_fechorainicio  )* 10000 +  datepart(month, EZ.f_fechorainicio  )  *100 +    datepart(day, EZ.f_fechorainicio  ) 
,Hora_Entrada = right(str(datepart(hh, EZ.f_fechorainicio  )+ 100),2) + ':' + right(str(datepart(mi, EZ.f_fechorainicio )+ 100),2) 
,Hora_Salida  = right(str(datepart(hh,EZ.f_fechorafin )+ 100),2) + ':' + right(str(datepart(mi, EZ.f_fechorafin )+ 100),2) 
,Hora = TH.d_tipohora
,oi_escuadra =0
,UnidadOrg = UO.d_unidad_org
,DiaInicio = datepart(year, EZ.f_fechorainicio  )* 10000 +  datepart(month, EZ.f_fechorainicio  )  *100 +    datepart(day, EZ.f_fechorainicio  ) 
,DiaFin = datepart(year, EZ.f_fechorafin  )* 10000 +  datepart(month, EZ.f_fechorafin  )  *100 +    datepart(day, EZ.f_fechorafin  ) 
,EZ.oi_horario
FROM TTA11_ESPERANZAPER   EZ
JOIN TTA01_TipoHoras   TH on
	EZ.oi_TipoHora = TH.oi_TipoHora
JOIN ORG02_ESTRUCTURAS EST on
	EZ.oi_estructura = EST.oi_estructura

JOIN ORG01_UNI_ORG     UO on
	EST.oi_unidad_org = UO.oi_unidad_org
WHERE f_fechorainicio >= @dFecDesde and f_fechorafin <=  @dFecHasta
and oi_personal_emp = @nPersona
and datepart(year, EZ.f_fechorainicio  )* 10000 +  datepart(month, EZ.f_fechorainicio  )  *100 +    datepart(day, EZ.f_fechorainicio  )  <> datepart(year, EZ.f_fechorafin  )* 10000 +  datepart(month, EZ.f_fechorafin  )  *100 +    datepart(day, EZ.f_fechorafin  ) 



--- declaracion del CURSOR utilizado para desglosar en dos registros la entrada y salida de días diferentes 
declare @DiaInicio varchar(8)

DECLARE Horarios CURSOR FOR 
select   DiaInicio from #temp 

open  Horarios 

FETCH NEXT FROM Horarios 
into  @DiaInicio 


-- ==============================================================
-- Los registros que inserto en el paso anterior los desglosa en dos nuevos sobre otra tabla temporal
-- el primero tiene el día y fecha de entrada y como la salida es el día siguente el campo de salida 
-- esta en blanco. El segundo registro contiene el día siguiente con la entrada en blanco y la salida con el horario
-- ==============================================================

WHILE @@FETCH_STATUS = 0  
BEGIN


	INSERT INTO #temp2
	(NombreDia,Dia	,Hora_Entrada	,Hora_Salida	,Hora	,oi_escuadra
	,UnidadOrg	,DiaInicio	,DiaFin		,oi_horario )
	select NombreDia ,Dia = DiaInicio , Hora_Entrada ,Hora_Salida ='' ,
	Hora,  oi_escuadra  ,UnidadOrg ,  DiaInicio ,  DiaFin  ,oi_horario  
	from #temp where DiaInicio = @DiaInicio 
	

	INSERT INTO #temp2
	(NombreDia,Dia	,Hora_Entrada	,Hora_Salida	,Hora	,oi_escuadra
	,UnidadOrg	,DiaInicio	,DiaFin		,oi_horario )
	select NombreDia ,Dia = DiaFin , Hora_Entrada ='' ,Hora_Salida ,
	Hora,  oi_escuadra  ,UnidadOrg ,  DiaInicio = DiaFin ,  DiaFin  ,oi_horario  
	from #temp where DiaInicio = @DiaInicio 

FETCH NEXT FROM Horarios 
into  @DiaInicio 
end 

CLOSE Horarios
DEALLOCATE Horarios


------------------------------------------------------------------
---- INSERTA LOS REGISTROS restantes
-------------------------------------------------------------------
INSERT INTO #temp2
(NombreDia,Dia	,Hora_Entrada	,Hora_Salida	,Hora	,oi_escuadra
,UnidadOrg	,DiaInicio	,DiaFin		,oi_horario )

select NombreDia = case datepart(dw,EZ.f_fechorainicio)  when 1 then 'Lunes' 
					when 2 then 'Martes' 
					when 3 then 'Miercoles' 
					when 4 then 'Jueves' 
					when 5 then 'Viernes' 
					when 6 then 'Sabado' 
					when 7 then 'Domingo' end
,Dia = datepart(year, EZ.f_fechorainicio  )* 10000 +  datepart(month, EZ.f_fechorainicio  )  *100 +    datepart(day, EZ.f_fechorainicio  ) 
,Hora_Entrada = right(str(datepart(hh, EZ.f_fechorainicio  )+ 100),2) + ':' + right(str(datepart(mi, EZ.f_fechorainicio )+ 100),2) 
,Hora_Salida  = right(str(datepart(hh,EZ.f_fechorafin )+ 100),2) + ':' + right(str(datepart(mi, EZ.f_fechorafin )+ 100),2) 
,Hora = TH.d_tipohora
,oi_escuadra = 0 
,UnidadOrg = UO.d_unidad_org
,DiaInicio = datepart(year, EZ.f_fechorainicio  )* 10000 +  datepart(month, EZ.f_fechorainicio  )  *100 +    datepart(day, EZ.f_fechorainicio  ) 
,DiaFin = datepart(year, EZ.f_fechorafin  )* 10000 +  datepart(month, EZ.f_fechorafin  )  *100 +    datepart(day, EZ.f_fechorafin  ) 
,EZ.oi_horario
FROM TTA11_ESPERANZAPER   EZ
JOIN TTA01_TipoHoras   TH on
	EZ.oi_TipoHora = TH.oi_TipoHora
JOIN ORG02_ESTRUCTURAS EST on
	EZ.oi_estructura = EST.oi_estructura
JOIN ORG01_UNI_ORG     UO on
	EST.oi_unidad_org = UO.oi_unidad_org
WHERE  f_fechorainicio >= @dFecDesde and f_fechorafin <=  @dFecHasta
and oi_personal_emp = @nPersona
and datepart(year, EZ.f_fechorainicio  )* 10000 +  datepart(month, EZ.f_fechorainicio  )  *100 +    datepart(day, EZ.f_fechorainicio  )  = datepart(year, EZ.f_fechorafin  )* 10000 +  datepart(month, EZ.f_fechorafin  )  *100 +    datepart(day, EZ.f_fechorafin  ) 



--- aCTUALIZA LAS FECHAS PARA UTILIZARLA EN EL JOIN CON LA TABLA DE ESPERANZA HORARIA PARA OBTENER LA POCICIO
UPDATE #temp2 SET Fecha = CONVERT(DATETIME, rtrim(DiaInicio) ) 


--- aCTUALIZA LA oi_escuadra PARA UTILIZARLA EN EL JOIN CON LA TABLA DE ESPERANZA HORARIA PARA OBTENER LA POCICIO
update #temp2 
set oi_escuadra  = HorPer.oi_escuadra 
from #temp2 tmp
join tta04_horariospers HorPer ON 
	tmp.oi_horario = HorPer.oi_horario
	and  HorPer.oi_personal_emp = @nPersona
where  (convert(datetime,str(year( f_fechaInicio  )* 10000 +  month( f_fechaInicio  )  *100 +    day( f_fechaInicio  )),120)  <=  convert(datetime, tmp.Fecha,120)
	and (convert(datetime,str(year( f_fechaFin  )* 10000 +  month( f_fechaFin  )  *100 +    day( f_fechaFin  )),120) >= convert(datetime, tmp.Fecha ,120) 
		or HorPer.f_fechaFin is null)
	)
	and c_estado ='A'
-- PASA EL RESULTADO A UNA TEMPORAL 

SELECT DISTINCT ESP.oi_horario oi_horario 
,NombreDia 
,Dia         
,Hora_Entrada 
,Hora_Salida 
,Hora                                                                                                 
,#temp2.oi_escuadra                    
,UnidadOrg                                                                                            
,DiaInicio   
,DiaFin      
,Fecha                                                  
,escuadra =  isnull(c_escuadra ,'') 
,Posicion = ESP.e_posicion 
INTO #temp3 
FROM   #temp2 
JOIN dbo.TTA03_ESPERANZA   ESP on 
	ESP.oi_empresa =@oi_empresa and
	ESP.oi_calendario= @oi_calendario and 
	ESP.oi_horario   = #temp2.oi_horario and 
	isnull(ESP.oi_escuadra,0) = isnull(#temp2.oi_escuadra,0) and 
	ESP.f_fecha= #temp2.Fecha 
LEFT JOIN TTA02_escuadras Esc ON 
	#temp2.oi_escuadra = Esc.oi_escuadra

ORDER BY  fecha,Hora_Entrada,Hora_Salida



DECLARE ESCUADRAS CURSOR FOR 
select distinct TTA02_escuadras.oi_escuadra ,TTA02_escuadras.oi_horario , e_avance  , d_escuadra  
from #temp2 
JOIN TTA02_escuadras on 
#temp2.oi_horario=TTA02_escuadras.oi_horario order by e_avance 

open  ESCUADRAS 

FETCH NEXT FROM ESCUADRAS 
into  @oi_escuadra ,@oi_horario , @e_avance  , @d_escuadra  


-- ==============================================================
-- Los registros que inserto en el paso anterior los desglosa en dos nuevos sobre otra tabla temporal
-- el primero tiene el día y fecha de entrada y como la salida es el día siguente el campo de salida 
-- esta en blanco. El segundo registro contiene el día siguiente con la entrada en blanco y la salida con el horario
-- ==============================================================

WHILE @@FETCH_STATUS = 0  
BEGIN

	UPDATE  #temp3 SET oi_escuadra =  @oi_escuadra , escuadra=@d_escuadra
	WHERE Posicion >= @e_avance and @oi_horario= oi_horario

	FETCH NEXT FROM ESCUADRAS 
	into  @oi_escuadra ,@oi_horario , @e_avance  , @d_escuadra  
end


CLOSE ESCUADRAS
DEALLOCATE ESCUADRAS

select * , c_horario from #temp3 join TTA02_HORARIOS on
#temp3.oi_horario = TTA02_HORARIOS.oi_horario





GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

