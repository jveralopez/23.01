
Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.ORG.AK_UBICACIONES', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT
 
select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version  and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'ORGANIZACION',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR'
	return

END


EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION ', 'Comienza la ejecucion ',0 , ''
	
IF EXISTs(select *  from sysobjects nolock where name ='AK_AK1_ORG_UBICACIONE_ORG03_UB')
BEGIN
	IF  EXISTS (Select count(*)  from [dbo].[ORG03_UBICACIONES] group by oi_empresa, c_ubicacion having Count(*) >1)
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION ', 'DROP CONSTRAINT [AK_AK1_ORG_UBICACIONE_ORG03_UB]',999 , 'ERROR:No se ejecuta por clave duplicada'
	
	ELSE
	   BEGIN	
	
		ALTER TABLE [dbo].[ORG03_UBICACIONES] DROP CONSTRAINT [AK_AK1_ORG_UBICACIONE_ORG03_UB] 
		
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION ', 'DROP CONSTRAINT [AK_AK1_ORG_UBICACIONE_ORG03_UB]',@@error , ''
		
		
		ALTER TABLE [dbo].[ORG03_UBICACIONES] ADD CONSTRAINT [AK_AK1_ORG_UBICACIONE_ORG03_UB] UNIQUE  NONCLUSTERED
		(
			[oi_empresa], [c_ubicacion]
		)  ON [PRIMARY] 
	
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION ', 'ADD CONSTRAINT [AK_AK1_ORG_UBICACIONE_ORG03_UB]',@@error , ''	
	
	END
END	

exec dbo.sp_NMD_VerifScript @n_Version , 'BASE.ORG.AK_UBICACIONES', 1 ,@Result OUTPUT

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION ', 'Finalizo la ejecucion',@Result , ''	




