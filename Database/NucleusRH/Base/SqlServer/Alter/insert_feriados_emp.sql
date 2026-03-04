

-- actualiza la tabla org03_feriados_emp

DECLARE @oi_emp int

DECLARE @oid int

DECLARE @oi_feriado_emp int

select @oid = max(oi_feriado_emp) from org03_feriados_emp


DECLARE cur_emp CURSOR FOR 
SELECT oi_empresa from org03_empresas
FOR READ ONLY

OPEN cur_emp
FETCH NEXT FROM cur_emp INTO @oi_emp

WHILE @@fetch_status = 0
BEGIN	

	select @oi_feriado_emp = oi_feriado_emp from org03_feriados_emp 
		where oi_calendario = 1 and oi_empresa = @oi_emp
	
	if @oi_feriado_emp is null
	begin
			select @oid = @oid + 1
		
			insert into org03_feriados_emp (oi_feriado_emp, oi_calendario, oi_empresa) 
			values (@oid, 1, @oi_emp)

	end

	select @oi_feriado_emp = null
	
	FETCH NEXT FROM cur_emp INTO @oi_emp

END

CLOSE cur_emp
DEALLOCATE cur_emp