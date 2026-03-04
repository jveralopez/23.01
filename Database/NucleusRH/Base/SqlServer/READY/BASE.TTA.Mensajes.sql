Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.TTA.Mensajes', @n_Version = 1 , @d_modulo ='TTA'

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Comienza la ejecucion ',0 , ''


IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo  ,@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR El script ya corrio'
	return
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA12_MEN_TERMINAL'  ) 
BEGIN
	IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='PK_TTA12_MEN_TERMINAL'  ) 
	BEGIN
		/*==============================================================*/
		/* Table: TTA12_MEN_TERMINAL																		*/
		/*==============================================================*/
		create table dbo.TTA12_MEN_TERMINAL (
		oi_mensaje      int                  not null,
		c_mensaje      varchar(30)                  not null,
		d_mensaje           varchar(100)             null,
		constraint PK_TTA12_MEN_TERMINAL primary key (oi_mensaje))
	
	
		
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table  TTA12_MEN_TERMINAL' , @@error , ''
	END
END


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA04_PERSONAL' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='oi_mensaje' ) )
BEGIN
	
	
	alter table dbo.TTA04_PERSONAL
	add oi_mensaje int null

	
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add oi_mensaje' , @@error , ''

END
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='TTA04_PERSONAL' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_TTA04_PE_REFERENCE_TTA12_ME' ) )
BEGIN

	alter table dbo.TTA04_PERSONAL
	   add constraint FK_TTA04_PE_REFERENCE_TTA12_ME foreign key (oi_mensaje)
	      references dbo.TTA12_MEN_TERMINAL (oi_mensaje)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_TTA04_PE_REFERENCE_TTA12_ME' , @@error , ''

END





exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



