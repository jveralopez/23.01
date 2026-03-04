Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.ORG.TIPOSDOMIC_JURISAFIP', @n_Version = 1 , @d_modulo ='ORG'

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

/*==============================================================*/
/* Table: ORG33_TIPOS_DOMIC                                     */
/*==============================================================*/
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG33_TIPOS_DOMIC'  and xtype='u' )
BEGIN

create table dbo.ORG33_TIPOS_DOMIC (
   oi_tipo_dom          int                  not null,
   c_tipo_dom           varchar(30)          not null,
   d_tipo_dom           varchar(100)         not null,
   constraint PK_ORG33_TIPOS_DOMIC primary key (oi_tipo_dom),
   constraint AK_AK_KEY_2_ORG33_TIP_ORG33_TI unique (c_tipo_dom)
)
 
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' CREATE ORG33_TIPOS_DOMIC  ' , @@error , ''
END



	/*==============================================================*/
	/* Table: ORG36_JURIS_AFIP                                      */
	/*==============================================================*/
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG36_JURIS_AFIP'  and xtype='u' )
BEGIN

	create table dbo.ORG36_JURIS_AFIP (
	   oi_juris_afip        int                  not null,
	   c_juris_afip         varchar(30)          not null,
	   d_juris_afip         varchar(100)         not null,
	   n_porc_dto_cp        numeric(11,3)        null,
	   constraint PK_ORG36_JURIS_AFIP primary key (oi_juris_afip),
	   constraint AK_AK__ORG36_JURIS_AF_ORG36_JU unique (c_juris_afip)
	)
 

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' CREATE ORG36_JURIS_AFIP  ' , @@error , ''
END


exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 



