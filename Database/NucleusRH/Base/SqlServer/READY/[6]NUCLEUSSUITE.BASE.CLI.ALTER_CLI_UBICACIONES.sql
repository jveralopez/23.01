Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'NUCLEUSSUITE.BASE.CLI.ALTER_CLI_UBICACIONES', @n_Version = 1 , @d_modulo ='CLI'

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



IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id in 
		(SELECT  parent_obj FROM  sysobjects NOLOCK  WHERE name ='FK_CLI31_UB_FK_CLI31__CLI31_EM') )
BEGIN
	alter table dbo.CLI31_UBICACIONES   
	drop constraint FK_CLI31_UB_FK_CLI31__CLI31_EM
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' drop FK_CLI31_UB_FK_CLI31__CLI31_EM null ' , @@error , ''
END


IF  EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='org_oi_empresa') )
BEGIN

	
	alter table dbo.CLI31_UBICACIONES   
	drop column org_oi_empresa

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' drop column org_oi_empresa' , @@error , ''
END



IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id in 
		(SELECT  id FROM  syscolumns NOLOCK  WHERE name ='oi_empresa') )
BEGIN

	alter table dbo.CLI31_UBICACIONES   
	add oi_empresa int null

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add oi_empresa  ' , @@error , ''
END



IF  NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='CLI31_UBICACIONES' and id in 
		(SELECT  parent_obj FROM  sysobjects NOLOCK  WHERE name ='FK_CLI31_UB_FK_CLI31__CLI31_EM') )
BEGIN
	alter table dbo.CLI31_UBICACIONES
	   add constraint FK_CLI31_UB_FK_CLI31__CLI31_EM foreign key (oi_empresa)
	      references dbo.CLI31_EMPRESAS (oi_empresa)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_CLI31_UB_FK_CLI31__CLI31_EM' , @@error , ''
END




exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



