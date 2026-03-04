

Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.TTA.Alter_TTA01_TipoHoras', @n_Version = 1 , @d_modulo ='TTA'

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo  ,@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR El script ya corrio'
	return
END

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Comienza la ejecucion ',0 , ''
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA01_TipoHoras'  and xtype='u')
BEGIN
	IF NOT EXISTS (select * from sysobjects NOLOCK where id in (SELECT  id  FROM  syscolumns  NOLOCK  WHERE name ='e_sumariza_aus' )and xtype='u' and name='TTA01_TipoHoras') 
	   BEGIN
		--Crea los campos nuevos
		ALTER TABLE TTA01_TipoHoras 	add e_sumariza_aus int null
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add e_sumariza_aus ',@@error , ''
	   END	

	IF NOT EXISTS (select * from sysobjects NOLOCK where id in (SELECT  id  FROM  syscolumns  NOLOCK  WHERE name ='e_sumariza_pre' )and xtype='u' and name='TTA01_TipoHoras') 
	   BEGIN
		ALTER TABLE TTA01_TipoHoras 			add e_sumariza_pre int null
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'add e_sumariza_pre ',@@error , ''
	   END	
	
END
go

---- esto esta porque no me ejecutaba el script porque tiene updates sobre campos que crea en los primeros pasos


Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.TTA.Alter_TTA01_TipoHoras', @n_Version = 1 , @d_modulo ='TTA'

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  





IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA01_TipoHoras'  and xtype='u')
BEGIN
	
	--Corrije los datos actuales
	IF EXISTS (select * from sysobjects NOLOCK where id in (SELECT  id  FROM  syscolumns  NOLOCK  WHERE name ='l_sumariza' )and xtype='u' and name='TTA01_TipoHoras') 
	   BEGIN

		UPDATE TTA01_TipoHoras
		SET l_sumariza = 0
		WHERE l_sumariza is null
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'SET l_sumariza = 0',@@error , ''
	   END	
	IF EXISTS (select * from sysobjects NOLOCK where id in (SELECT  id  FROM  syscolumns  NOLOCK  WHERE name ='l_sumariza_aus' )and xtype='u' and name='TTA01_TipoHoras') 
	   BEGIN
	
		UPDATE TTA01_TipoHoras
		SET l_sumariza_aus = 0
		WHERE l_sumariza_aus is null
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' SET l_sumariza_aus = 0',@@error , ''
	
		--Actualiza los nuevos valores basandose en los anteriores
		IF EXISTS (select * from sysobjects NOLOCK where id in (SELECT  id  FROM  syscolumns  NOLOCK  WHERE name ='e_sumariza_pre' )and xtype='u' and name='TTA01_TipoHoras') 
		   BEGIN
	
			UPDATE TTA01_TipoHoras
			SET  e_sumariza_pre = 0, e_sumariza_aus = 0
			WHERE l_sumariza = 0

			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'SET  e_sumariza_pre = 0, e_sumariza_aus = 0',@@error , ''
	
	
			UPDATE TTA01_TipoHoras
			SET e_sumariza_pre = 1, e_sumariza_aus = 0
			WHERE l_sumariza = 1 and l_sumariza_aus = 0

			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'SET e_sumariza_pre = 1, e_sumariza_aus = 0',@@error , ''
	
		
			UPDATE TTA01_TipoHoras
			SET e_sumariza_pre = 1, e_sumariza_aus = 2 
			WHERE l_sumariza = 1 and l_sumariza_aus = 1

			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'SET e_sumariza_pre = 1, e_sumariza_aus = 2',@@error , ''	
		
	--Actualiza los campos con las restricciones. No elimina los campos viejos, serán eliminados en otra actualización.

			ALTER TABLE TTA01_TipoHoras
			ALTER COLUMN e_sumariza_aus int not null
			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'ALTER COLUMN e_sumariza_aus int not null',@@error , ''	

			ALTER TABLE TTA01_TipoHoras
			ALTER COLUMN e_sumariza_pre int not null

			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'ALTER COLUMN e_sumariza_pre int not null',@@error , ''


		   END

	    END	
	

END
exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	



