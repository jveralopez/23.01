

-- actualiza la tabla tta04_personal

DECLARE @oi_personal_emp int
DECLARE @e_nro_legajo_reloj int
DECLARE @e_numero_legajo int

DECLARE cur_per CURSOR FOR 
SELECT oi_personal_emp from tta04_personal
FOR READ ONLY

OPEN cur_per
FETCH NEXT FROM cur_per INTO @oi_personal_emp

WHILE @@fetch_status = 0
BEGIN	

	select @e_nro_legajo_reloj = e_nro_legajo_reloj from tta04_personal 
		where oi_personal_emp = @oi_personal_emp
	
	if @e_nro_legajo_reloj is null
	begin
	
			select @e_numero_legajo = e_numero_legajo from per02_personal_emp 
				where oi_personal_emp = @oi_personal_emp
				
			update tta04_personal 
				set e_nro_legajo_reloj = @e_numero_legajo 
			where oi_personal_emp = @oi_personal_emp

	end

	select @e_nro_legajo_reloj = null
	select @e_numero_legajo = null
	
	FETCH NEXT FROM cur_per INTO @oi_personal_emp

END

CLOSE cur_per
DEALLOCATE cur_per