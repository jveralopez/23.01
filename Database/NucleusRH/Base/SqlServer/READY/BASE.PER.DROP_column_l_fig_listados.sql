Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.PER.DROP_column_l_fig_listados', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version  and d_script=@ScripName 
select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'PERSONAL',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR'
	return
END







EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'Comienza la ejecucion ',0 , ''

-- verifico que exista
IF EXISTs(SELECT * FROM sysobjects NOLOCK WHERE id in (SELECT  id FROM  syscolumns NOLOCK WHERE name ='l_fig_listados') AND NAME ='PER01_DOCUM_PER')
BEGIN
	-- verifico que no sea constraints
	IF NOT EXISTs(SELECT * FROM sysobjects NOLOCK WHERE id in (SELECT  id FROM  sysconstraints NOLOCK WHERE id in (SELECT  id FROM  syscolumns NOLOCK WHERE name ='l_fig_listados' AND status in (1,2,3))))
	BEGIN
		ALTER TABLE PER01_DOCUM_PER DROP COLUMN l_fig_listados
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'DROP COLUMN l_fig_listados ',@@error , ''	

	END
END



EXEC dbo.sp_NMD_VerifScript @n_Version , @ScripName  ,1 ,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'PERSONAL', 'Finalizo la ejecucion',@Result , ''	



