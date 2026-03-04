
Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.POS.ALTER_FLIARES_POS', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version  and d_script=@ScripName 
select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'Postulantes',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR'
	return
END


EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'Comienza la ejecucion ',0 , ''
	
IF EXISTS(select *  from sysobjects nolock where name ='POS01_FLIARES_CV')
BEGIN

	-- verifico que no sea constraints
	IF NOT EXISTs(SELECT * FROM sysobjects NOLOCK WHERE id in (SELECT  id FROM  sysconstraints NOLOCK WHERE id in (SELECT  id FROM  syscolumns NOLOCK WHERE name ='l_vive' AND status in (1,2,3))))
	BEGIN

		alter table POS01_FLIARES_CV
			drop column l_vive
	
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'POS01_FLIARES_CV drop column l_vive ',@@error , ''	 
	END


	-- verifico que no exista el campo
	IF NOT EXISTs(SELECT * FROM sysobjects NOLOCK WHERE id in (SELECT  id FROM  SYSCOLUMNS NOLOCK WHERE name ='l_fallecido' ) and name ='POS01_FLIARES_CV')
	BEGIN


		alter table POS01_FLIARES_CV
		add l_fallecido smallint null
	
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Postulantes', 'POS01_FLIARES_CV add l_fallecido ',@@error , ''	 
	END



END


EXEC dbo.sp_NMD_VerifScript @n_Version , @ScripName  ,1 ,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'Organizacion ', 'Finalizo la ejecucion',@Result , ''	

  


