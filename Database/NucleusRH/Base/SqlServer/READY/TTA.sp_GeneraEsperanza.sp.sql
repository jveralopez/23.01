if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_GeneraEsperanza]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[sp_GeneraEsperanza]
GO

 SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO




SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO



CREATE    procedure dbo.sp_GeneraEsperanza @nHorario   int, @nEmpresa  int
AS




--, @@Result varchar (500) OUTPUT
declare @@Result varchar (500) 
declare @nDias int  
declare @Fecha_inicio datetime
declare @e_avance int
declare @CantPasadas int
declare @CantDias int
declare @oi_horario int
declare @oi_EsperanzaMax int
declare @e_dias int
declare @d_tipohorario varchar (100)
declare @oi_escuadra int
declare @CantVeces  int 
declare @rangoDias int 
declare @CantHasta int
declare @MaxEsperanza int
declare @nroFecha_inicio int --- lo utilizamos para fijar la posicion del NO Rotativo en la fecha de comienzo
declare @nRowsCant int --- se utiliza para la actualizacion del tmpEsperanza ñsolo para el horario insertado
declare @e_iniciodia int 
declare @PosRotAnt int  -- para rot incio dia <0 ,guarda ultima posicion en la secuencia, esta debe insertarla como primer registro
declare @Fec_iniRotAnt datetime -- para rot incio dia <0 ,guarda la fecha de inicio - 1, esta debe insertarla como primer registro
declare @EscRotAnt int  -- para rot incio dia <0 , guarda la escuadra perteneciente al avance 1 para insertar el 1| reg
declare @Paso Varchar(100)
declare @MaxPos int
SET NOCOUNT on




CREATE TABLE #tmpSecuencia (
	NroRow int IDENTITY(0,1),
	d_tipohorario varchar(100),
	Fecha datetime NULL ,
	oi_horario int, 
	oi_escuadra int, 
	e_posicion int NULL ,
	f_hora_inicio datetime NULL ,
	f_hora_fin datetime NULL ,
	oi_tipo_hs_norm int NULL ,
	oi_tipo_hs_dom int NULL ,
	oi_tipo_hs_fer int NULL ,
	oi_tipo_hs_domfer int NULL ,
	oi_estructura int NULL ,
	c_eval varchar (100)  NULL ,
	l_updated int 
)


CREATE TABLE #tmpEsperanza (
	oi_empresa int , 
	oi_calendario int ,
	d_tipohorario varchar(100),
	NroRow_t2 int IDENTITY(1,1),
	nDia int  NULL ,
	Fecha datetime NULL ,
	oi_hora int  NULL ,
	oi_horario int, 
	oi_escuadra int, 
	e_posicion int NULL ,
	f_hora_inicio datetime NULL ,
	f_hora_fin datetime NULL ,
	oi_tipo_hs_norm int NULL ,
	oi_tipo_hs_dom int NULL ,
	oi_tipo_hs_fer int NULL ,
	oi_tipo_hs_domfer int NULL ,
	oi_estructura int NULL ,
	c_eval varchar (100)  NULL ,
	l_updated int
)




CREATE TABLE #tmpSecuenciaDias (
	NroRow3 int ,
	d_tipohorario varchar(100),
	Fecha datetime NULL ,
	oi_horario int, 
	oi_escuadra int, 
	e_posicion int NULL ,
	f_hora_inicio datetime NULL ,
	f_hora_fin datetime NULL ,
	oi_tipo_hs_norm int NULL ,
	oi_tipo_hs_dom int NULL ,
	oi_tipo_hs_fer int NULL ,
	oi_tipo_hs_domfer int NULL ,
	oi_estructura int NULL ,
	c_eval varchar (100)  NULL ,
	l_updated int 
)



CREATE TABLE #tmpDiaAntRot (
	oi_empresa int , 
	oi_calendario int ,
	d_tipohorario varchar(100),
	NroRow_t2 int IDENTITY(1,1),
	nDia int  NULL ,
	Fecha datetime NULL ,
	oi_hora int  NULL ,
	oi_horario int, 
	oi_escuadra int, 
	e_posicion int NULL ,
	f_hora_inicio datetime NULL ,
	f_hora_fin datetime NULL ,
	oi_tipo_hs_norm int NULL ,
	oi_tipo_hs_dom int NULL ,
	oi_tipo_hs_fer int NULL ,
	oi_tipo_hs_domfer int NULL ,
	oi_estructura int NULL ,
	c_eval varchar (100)  NULL ,
	l_updated int
)






begin transaction

SELECT @nDias = d_valor from dbo.ORG26_PARAMETROS  where c_parametro='DiasGenEsperanza'

select @Paso ='1- Seleciona los datos LINEA 139'

SELECT Hor.oi_horario, f_Fechainicio, e_dias, d_tipohorario, oi_escuadra oi_esc ,  --isnull(oi_escuadra,0) oi_esc ,  
	isnull(e_avance,1) e_av , datediff(d,f_Fechainicio , getdate() + @nDias ) CantDias, e_iniciodia
into #tmpCursor 
FROM dbo.TTA02_HORARIOS Hor 
left JOIN dbo.TTA02_ESCUADRAS Esc on 
Hor.oi_horario  =  Esc.oi_horario


/*
update dbo.TTA02_HORARIOSDET 
	set  f_hora_fin = '18991230 23:59:59'
from dbo.TTA02_HORARIOSDET H ,
	(select oi_horario  ,e_posicion 
	FROM dbo.TTA02_HORARIOSDET 
	where datepart(hh, f_hora_fin) =0 
	group by oi_horario  ,e_posicion
	having count(*) = 1 ) HH 
where h.oi_horario = hh.oi_horario  and h.e_posicion =hh.e_posicion
and  datepart(h.hh, f_hora_fin) =0 
*/



select @Paso ='borra [dbo].[TTA03_ESPERANZA] LINEA 164'

IF @nHorario   <> 0
	BEGIN
		IF @nEmpresa  <> 9999
			delete [dbo].[TTA03_ESPERANZA] 	where oi_horario = @nHorario   AND OI_EMPRESA = @nEmpresa  
		ELSE
		BEGIN
			delete [dbo].[TTA03_ESPERANZA] 	where oi_horario = @nHorario   
		END
		delete #tmpCursor  	where oi_horario <> @nHorario   
	END
ELSE
	BEGIN
		TRUNCATE TABLE [dbo].[TTA03_ESPERANZA] 
	END


---para auto incrementar la el oi de la tabla de esperanza
select @MaxEsperanza = isnull(max(oi_esperanza),0) + 1 from [dbo].[TTA03_ESPERANZA]


select @Paso ='Declara el cursor'

--- ====================================================================
--- Toma todos los horarios disponibles con sus escuadras (si las tiene)
--- ====================================================================
DECLARE Horarios CURSOR FOR 
SELECT oi_horario, f_Fechainicio, e_dias, d_tipohorario, oi_esc ,  e_av , CantDias, e_iniciodia
FROM #tmpCursor 

open  Horarios 
--print 'cursor2'
FETCH NEXT FROM Horarios 
into  @oi_horario,@Fecha_inicio,@e_dias
,@d_tipohorario,@oi_escuadra,@e_avance , @rangoDias, @e_iniciodia


select @Fec_iniRotAnt = @Fecha_inicio - 1 , @PosRotAnt =0

WHILE @@FETCH_STATUS = 0  
BEGIN
select @Paso ='Comienzo del bucle LINEA 200'	 
--print 'entra en while'
	set @nroFecha_inicio = 0

	if @d_tipohorario <> 'R'
	begin
		select @nroFecha_inicio = datepart(dw,@Fecha_inicio)
 	
		if @nroFecha_inicio < 7
		begin
			select @Fecha_inicio = @Fecha_inicio - @nroFecha_inicio 
		end

	end

 



	--- ====================================================================
	--- inserta en tabla temporal por horario/escuadras las posiciones en orden
	--- ====================================================================


	truncate table #tmpSecuencia
	
select @Paso ='insert into  #tmpSecuencia LINEA 226' 	 	
	insert into  #tmpSecuencia
	(Fecha,oi_horario, d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin  ,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer   ,oi_estructura, c_eval ,	l_updated  )
	
	SELECT @Fecha_inicio ,  --@Fecha_inicio + e_posicion as Fecha,  
		oi_horario , @d_tipohorario ,
		@oi_escuadra ,
		e_posicion, 
		f_hora_inicio, 
		f_hora_fin, oi_tipo_hs_norm, oi_tipo_hs_dom, 
		oi_tipo_hs_fer, oi_tipo_hs_domfer,  oi_estructura, 
		c_eval ,	l_updated =0 
	FROM dbo.TTA02_HORARIOSDET where oi_horario = @oi_horario
	and e_posicion > = @e_avance 
	order by e_posicion 


select @Paso ='update #tmpSecuencia LINEA 245' 

	if @e_avance > 1 -- reindexa las posiciones
	begin 
		update #tmpSecuencia set e_posicion = e_posicion - (@e_avance - 1)
		where e_posicion >= @e_avance

	end

select @MaxPos = max(e_posicion) from #tmpSecuencia

select @Paso ='Insert #tmpSecuencia LINEA 256' 
	insert into  #tmpSecuencia
	(Fecha,oi_horario , d_tipohorario, oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin ,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval,	l_updated  )
	SELECT 
	@Fecha_inicio ,  
	oi_horario , @d_tipohorario ,
	@oi_escuadra, 
	e_posicion, 
	f_hora_inicio, 
	f_hora_fin, oi_tipo_hs_norm, oi_tipo_hs_dom, 
	oi_tipo_hs_fer, oi_tipo_hs_domfer,  oi_estructura, 
	c_eval ,	l_updated =1 
	FROM dbo.TTA02_HORARIOSDET where oi_horario=@oi_horario
	and e_posicion <  @e_avance 
	order by e_posicion 



	if @e_avance > 1 -- reindexa las posiciones
	begin 
		update #tmpSecuencia set e_posicion = e_posicion + @MaxPos
		where e_posicion < @e_avance and l_updated =1

	end

	--- ====================================================================
	--- Este paso se utiliza para los horarios con la misma posicion
	--- para dos rangos de horas diferentes, se pasa a una tmp sin Autoincremental
	--- y se actualiza el campo nroRow que será usado posteriormente para la insercion 
	--- de fechas
	--- ====================================================================

	truncate table #tmpSecuenciaDias


select @Paso ='Insert #tmpSecuenciaDias LINEA 291' 
	insert into #tmpSecuenciaDias 
		(Fecha,oi_horario , d_tipohorario ,oi_escuadra, e_posicion  ,f_hora_inicio
		,f_hora_fin ,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
		,oi_tipo_hs_domfer ,oi_estructura, c_eval,	l_updated  )
	select 
		Fecha,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
		,f_hora_fin  ,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
		,oi_tipo_hs_domfer ,oi_estructura, c_eval,	l_updated  
	 from #tmpSecuencia
	



	update #tmpSecuenciaDias set nroRow3 = e_posicion


	-- horarios rotativo que arranquen con el día anterior
	IF @d_tipohorario = 'R' and @e_iniciodia < 0 and @e_avance=1
	begin 
		select  @EscRotAnt  =@oi_escuadra, @PosRotAnt =max(nrorow3) from #tmpSecuenciaDias 
	end 

	
	--- ====================================================================
	--- @CantDias  = dias que abarca el horario
	--- @CantPasadas  = cantidad de insert en bloques 
	--- @CantHasta  = veces que debe hacer los insert
	--- ====================================================================


	select @CantDias= max(nroRow3) from #tmpSecuenciaDias



	if @CantDias > 0
	BEGIN
		select @CantPasadas =0 , @CantHasta = @rangoDias/@CantDias from #tmpSecuenciaDias
	
		while @CantPasadas < @CantHasta
		begin

select @Paso ='Insert #tmpEsperanza LINEA 333' 
			insert into  #tmpEsperanza
			(Fecha,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
			,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
			,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  )
			select 
			(Fecha-1) + nroRow3 + (@CantPasadas * @CantDias)
			,oi_tipo_hs_norm as hora
			,oi_horario , d_tipohorario
			,oi_escuadra 
			,e_posicion  
			,f_hora_inicio
			,f_hora_fin
			,oi_tipo_hs_norm 
			,oi_tipo_hs_dom 
			,oi_tipo_hs_fer 
			,oi_tipo_hs_domfer   
			,oi_estructura
			, c_eval  
			, 0
			from #tmpSecuenciaDias 
		
			set @CantPasadas = @CantPasadas  + 1
		end

	END	

	select @nRowsCant= max(NroRow_t2) from  #tmpEsperanza
--- ====================================================================
--- elimina los dias creados para los horarios NO ROTATIVOS que la fecha del día de inicio
--  no coincidan con la posicion 1 ( para esto se generaron días previos hasta completar la 
--  pos inicial partiendo del día de inicio, ejmplo incio 01/09/2006 cae en la posicion 6..
--  entonces cambia la fecha de inicio por 6 menos , se genera toda la esperanza y se eliminan los 
--  registros previos al 01/09/2006)
--- ====================================================================
/*	if @nroFecha_inicio <> 0 
	begin
		delete  #tmpEsperanza where Fecha < (@Fecha_inicio  + @nroFecha_inicio -1 )
			and oi_horario = @oi_horario --and NroRow_t2 >= @nRowsCant
	end 

*/





	FETCH NEXT FROM Horarios 
	into  @oi_horario,@Fecha_inicio,@e_dias
	,@d_tipohorario,@oi_escuadra,@e_avance , @rangoDias, @e_iniciodia

END 


CLOSE Horarios
DEALLOCATE Horarios

select @Paso ='Cerro el cursor' 

--===========================================================================
-- control del comienzo del día anterior para rotativos
-- esto es por ejemplo para horarios que empiezan la noche anterio
-- turno de 22.00 a 6.00 am
--===========================================================================


IF @d_tipohorario = 'R' and @e_iniciodia < 0 and @PosRotAnt <>0
BEGIN 
select @Paso ='entro tipohorario = R LINEA 401'
	truncate table #tmpDiaAntRot 


select @Paso ='  INSERT #tmpDiaAntRot  LINEA 405' 
	---- Inserta en temporal la ultima posicion pero con un día anterior al comienzo del horario
	-----------------------------------------------------------------------------------------
	insert into #tmpDiaAntRot 
	(Fecha ,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  )
	
	select distinct 
	Fecha = @Fec_iniRotAnt  ,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  
	 from #tmpEsperanza  where e_posicion = @PosRotAnt
	and oi_escuadra=@EscRotAnt  

select @Paso ='  INSERT #tmpDiaAntRot LINEA 420'  
	-- Inserta el resto de los registros para completar los autonumericos
	-----------------------------------------------------------------------------------------
	insert into #tmpDiaAntRot 
	(Fecha,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  )
	
	select --distinct 
	Fecha,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  
	 from #tmpEsperanza order by NroRow_t2
	
	-- Limpia la tabla e inserta los datos con los rows renumerados a partir del día anterior
	-- al comiewnzo del horario (la posicion 1 va a tener el día con hora de inicio < 0...el día anterior )
	-- ejemplo (el horario comienza el 02/01/07 y tiene hora inicio < 0, entonces se debe insertar como primer
	-- row ---NO POSICION, ROW -- los datos para el día 01/01/07 )
	truncate table #tmpEsperanza 
	select @Paso ='  INSERT #tmpDiaAntRot--- #tmpEsperanza 439' 
	insert into #tmpEsperanza 
	(Fecha,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  )
	
	select Fecha,oi_hora   ,oi_horario , d_tipohorario , oi_escuadra, e_posicion  ,f_hora_inicio
	,f_hora_fin,oi_tipo_hs_norm ,oi_tipo_hs_dom ,oi_tipo_hs_fer 
	,oi_tipo_hs_domfer ,oi_estructura, c_eval, l_updated  
	 from #tmpDiaAntRot 

END

--===========================================================================
--===========================================================================








--- ====================================================================
--- actualiza los domingos
--- ====================================================================
select @Paso ='  actualiza los domingos ' 
update #tmpEsperanza
set oi_hora = oi_tipo_hs_dom , nDia = 1
where datepart(dw,Fecha) = 7

select @Paso ='  INSERT #tmpEsperanza2 LINEA 470' 
select nDia ,Fecha, d_tipohorario ,oi_hora ,oi_horario ,oi_escuadra ,e_posicion 
,f_hora_inicio ,f_hora_fin  ,oi_tipo_hs_norm 
,oi_tipo_hs_dom ,oi_tipo_hs_fer ,oi_tipo_hs_domfer ,oi_estructura 
,c_eval ,l_updated 
into #tmpEsperanza2 from #tmpEsperanza



if @nEmpresa <> 9999
BEGIN
select @Paso ='  INSERT #tmpEsperanza lINEA 475 FERIADOS POR EMPRESA' 
	insert into #tmpEsperanza
	(oi_empresa , oi_calendario  ,nDia ,Fecha ,oi_hora ,oi_horario , d_tipohorario,oi_escuadra ,e_posicion 
	,f_hora_inicio ,f_hora_fin ,oi_tipo_hs_norm 
	,oi_tipo_hs_dom ,oi_tipo_hs_fer ,oi_tipo_hs_domfer ,oi_estructura 
	,c_eval ,l_updated )
	select  EMP.oi_empresa , EMP.oi_calendario  ,nDia ,Fecha ,oi_hora ,oi_horario , d_tipohorario,oi_escuadra ,e_posicion 
	,f_hora_inicio ,f_hora_fin ,oi_tipo_hs_norm 
	,oi_tipo_hs_dom ,oi_tipo_hs_fer ,oi_tipo_hs_domfer ,oi_estructura 
	,c_eval ,1 as l_updated 
	FROM  #tmpEsperanza2 ,  dbo.ORG03_FERIADOS_EMP EMP
	where l_updated =0 and EMP.oi_calendario  is not null
	and EMP.oi_empresa = @nEmpresa
END
ELSE
BEGIN
select @Paso ='  INSERT #tmpEsperanza lINEA 493 FERIADOS POR EMPRESA' 
	insert into #tmpEsperanza
	(oi_empresa , oi_calendario  ,nDia ,Fecha ,oi_hora ,oi_horario , d_tipohorario,oi_escuadra ,e_posicion 
	,f_hora_inicio ,f_hora_fin ,oi_tipo_hs_norm 
	,oi_tipo_hs_dom ,oi_tipo_hs_fer ,oi_tipo_hs_domfer ,oi_estructura 
	,c_eval ,l_updated )
	select  EMP.oi_empresa , EMP.oi_calendario  ,nDia ,Fecha ,oi_hora ,oi_horario , d_tipohorario,oi_escuadra ,e_posicion 
	,f_hora_inicio ,f_hora_fin ,oi_tipo_hs_norm 
	,oi_tipo_hs_dom ,oi_tipo_hs_fer ,oi_tipo_hs_domfer ,oi_estructura 
	,c_eval ,1 as l_updated 
	FROM  #tmpEsperanza2 ,  dbo.ORG03_FERIADOS_EMP EMP
	where l_updated =0 and EMP.oi_calendario  is not null
END

delete #tmpEsperanza where l_updated =0



--- ====================================================================
--- actualiza los feriados
--- ====================================================================
select @Paso ='  actualiza los feriados lINEA 514'
update #tmpEsperanza
set oi_hora = case nDia when 1 then isnull(oi_tipo_hs_Domfer,oi_tipo_hs_norm ) 
		else isnull(oi_tipo_hs_fer,oi_tipo_hs_norm ) end
	, nDia = 99 
where fecha in (
	SELECT 
	ltrim(rtrim(str(e_anio) + right(str(e_mes+100),2) +right(str(e_dia+100),2))) 
	--, FER.oi_calendario, oi_empresa  
	FROM dbo.ORG03_FERIADOS_EMP EMP
	JOIN  dbo.ORG27_DIAS_FERIADOS FER on 
	EMP.oi_calendario = FER.oi_calendario)
 

select @Paso ='  Actualiza los dias posteriores lINEA 528'
--------------------------------------------------------
-- Actualiza los dias posteriores a feriados que tengan definido en el 
-- campo Evalua  como 'P'
-- nDia = 99  ---> el registro fue modificado porque correponde a un FERIADO
-- nDia = 100 ---> el registro fue modificado porque correponde a un Evalua FERIADO
-------------------------------------------------------
update #tmpEsperanza
set oi_hora = case nDia when 1 then isnull(oi_tipo_hs_Domfer,oi_tipo_hs_norm ) 
		else isnull(oi_tipo_hs_fer,oi_tipo_hs_norm ) end
, nDia = 100
where NroRow_t2 in (
select T2.NroRow_t2
from #tmpEsperanza tmp                      -- Contiene los Feriados
JOIN #tmpEsperanza  T2 ON                   -- Contiene los días Posteriore a feriados Feriados
tmp.fecha + 1 = T2.fecha and 
tmp.oi_empresa = T2.oi_empresa  and 
tmp.oi_calendario = T2.oi_calendario  and 
tmp.oi_horario = T2.oi_horario  and 
tmp.oi_escuadra = T2.oi_escuadra  
where tmp.nDia =99 and                      -- Filtra los feriados
      T2.c_eval = 'Pos' )                   -- Filtra los evalua Posterior


update #tmpEsperanza set oi_hora = oi_tipo_hs_norm
where oi_hora is null and ndia= 1 --- este caso se da para los horarios NO ROTATIVOS
--- ====================================================================
--- Horarios cortados en dos días ejemplo entra a las 22.00 y sale a las 6.00 del otro día
--- ====================================================================

----- T2 contiene los horarios con entrada al comienzo del día (hora menor a la 1 am)
----- y devuelve el row anterior para ver si la posicion es la misma, si el row anterior
----- tiene la misma posicion significa que son dos días diferentes y al día de hora <1 se 
----- le suma un día
select @Paso ='  Horarios cortados lINEA 562'
select n_row = NroRow_t2+1 ,
tmp.oi_empresa ,
tmp.oi_calendario 
into #tmpHorarioCortados
from #tmpEsperanza tmp 
JOIN  (select NroRow_t2-1 nRow, e_posicion  
	from #tmpEsperanza where datepart(hh,f_hora_inicio) < 1) as T2 ON
    tmp.NroRow_t2 = nRow and 
    tmp.e_posicion = T2.e_posicion  


select @Paso ='  update #tmpEsperanza lINEA 574'
update #tmpEsperanza set fecha=fecha + 1 
from #tmpHorarioCortados tmp 
where NroRow_t2= n_row
and tmp.oi_empresa = #tmpEsperanza.oi_empresa 
and tmp.oi_calendario = #tmpEsperanza.oi_calendario 




--- ====================================================================
--- Inserta en la tabla de Esperanza
--- ====================================================================
select @Paso ='  INSERT TTA03_ESPERANZA lINEA 590' 

select @oi_EsperanzaMax = max (oi_esperanza) from [dbo].[TTA03_ESPERANZA]


INSERT INTO [dbo].[TTA03_ESPERANZA]
([oi_esperanza], [f_fecha], [f_horainicio], [f_horafin], [oi_tipoHora], 
[oi_horario], 
 e_posicion , [oi_estructura], [oi_empresa], oi_calendario, [oi_escuadra])




SELECT NroRow_t2 + @MaxEsperanza, fecha,
f_hora_inicio - datediff(dd,fecha,f_hora_inicio)
,f_hora_fin - datediff(dd,fecha,f_hora_fin) , 
oi_hora,oi_horario ,e_posicion ,oi_estructura,oi_empresa,oi_calendario,oi_escuadra 
FROM #tmpEsperanza 




UPDATE dbo.TTA03_ESPERANZA
set f_horainicio = DATEADD(Hour, e_iniciodia ,  f_horainicio )
, f_horafin = DATEADD(Hour, e_iniciodia ,  f_horafin )
FROM dbo.tta02_horarios Hor 
where Hor.d_tipohorario ='L' and 
dbo.TTA03_ESPERANZA.oi_horario = Hor.oi_horario  and
dbo.TTA03_ESPERANZA.oi_esperanza > @oi_EsperanzaMax 




if @@error=0
  begin
	select @@Result = RTRIM(@Paso )
	commit
   end
else
   begin
		select @@Result = RTRIM(@Paso ) + 'nError: ' + STR(@@error)
	        Rollback
   end





GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO






