Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.PER.ALTER_CUENTAS_PER', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'PERSONAL',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR 1'
	return

END


EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'Comienza la ejecucion ',0 , ''


IF EXISTs(select *  from sysobjects nolock where name ='PER01_PERSONAL')
BEGIN

	IF EXISTs( select * from sysobjects nolock where name ='PER01_PERSONAL' and id in (select id   from syscolumns nolock where name ='oi_usuario_sistema'))
	   BEGIN
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'PERSONAL','alter table dbo.PER01_PERSONAL add oi_usuario_sistema ' ,999 , 'ERROR: itentó agregar una columna existente'
		Select 'ERROR'
	   END
	ELSE
	   BEGIN
		alter table dbo.PER01_PERSONAL add oi_usuario_sistema varchar(128) null
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'alter table dbo.PER01_PERSONAL add oi_usuario_sistema ',@@error , ''
	   END

END


IF EXISTs(select *  from sysobjects nolock where name ='PER01_CUENTAS_PER')
BEGIN
	
	delete PER01_CUENTAS_PER
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'delete PER01_CUENTAS_PER',@@error , ''

	drop table PER01_CUENTAS_PER
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'drop table PER01_CUENTAS_PER',@@error , ''
END

exec dbo.sp_NMD_VerifScript @n_Version , 'BASE.PER.ALTER_CUENTAS_PER', 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'Finalizo la ejecucion',@Result , ''	






