Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.TTA.ALTER_FICHADAS', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version  and d_script=@ScripName 
select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'TTA',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR'
	return
END





EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'TTA', 'Comienza la ejecucion ',0 , ''
	
IF EXISTs(select *  from sysobjects nolock where name ='AK_KEY_3_TTA08_FI')
BEGIN
	alter table dbo.TTA08_FICHADAS drop constraint AK_KEY_3_TTA08_FI 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'TTA', 'drop constraint AK_KEY_3_TTA08_FI ',@@error , ''	

END


IF  EXISTS (select count(*) from dbo.TTA08_FICHADAS group by oi_personal_emp, f_fechahora, oi_liquidacion, l_entrada having count(*)  > 1)
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'TTA', 'add constraint AK_KEY_3_TTA08_FI ',999 , 'ERROR:No se ejecuta por clave duplicada'

ELSE
   BEGIN	

	alter table dbo.TTA08_FICHADAS add constraint AK_KEY_3_TTA08_FI unique (oi_personal_emp, f_fechahora, oi_liquidacion, l_entrada)		
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'TTA ', 'add constraint AK_KEY_3_TTA08_FI ',@@error , ''

END


EXEC dbo.sp_NMD_VerifScript @n_Version , @ScripName  ,1 ,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'TTA ', 'Finalizo la ejecucion',@Result , ''	


select * from dbo.NMD_LogScripts
SELECT [oi_Version], [c_Version], [oi_Fecha], [d_usuario], [d_script], [l_finalizo] FROM [dbo].[NMD_Scripts]

