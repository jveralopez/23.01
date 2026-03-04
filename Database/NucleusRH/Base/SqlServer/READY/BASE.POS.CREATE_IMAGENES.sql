Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.POS.CREATE_IMAGENES', @n_Version = 1 , @d_modulo ='POSTULANTES'

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

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS25_IMAGENES'  and xtype='u' )
BEGIN
	/*==============================================================*/
	/* Table: POS25_IMAGENES                                        */
	/*==============================================================*/
	create table dbo.POS25_IMAGENES (
	   oi_imagen            int                  not null,
	   e_tamanio            int                  null,
	   f_creacion           datetime             null,
	   o_imagen             varchar(1000)        null,
	   constraint PK_POS25_IMAGENES primary key (oi_imagen)
	)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.POS25_IMAGENES ' ,@@error , ''


 END		



IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS25_BINARIOS'  and xtype='u' )
BEGIN
	/*==============================================================*/
	/* Table: POS25_BINARIOS                                        */
	/*==============================================================*/
	create table dbo.POS25_BINARIOS (
	   oi_imagen            int                  null,
	   c_bloque             int                  not null,
	   t_datos              varchar(4000)        null,
	   constraint AK_AK_BINARIOS_POS25_BI unique (c_bloque)
	)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.POS25_BINARIOS ' ,@@error , ''


 END		



IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS25_BINARIOS' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_POS25_BI_REFERENCE_POS25_IM' ) )
BEGIN
	alter table dbo.POS25_BINARIOS
	   add constraint FK_POS25_BI_REFERENCE_POS25_IM foreign key (oi_imagen)
	      references dbo.POS25_IMAGENES (oi_imagen)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_POS25_BI_REFERENCE_POS25_IM ' ,@@error , ''

END


IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_IMAGENES_CV' and xtype ='U' )
BEGIN
	DROP TABLE POS01_IMAGENES_CV
	
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' DROP TABLE POS01_IMAGENES_CV ' ,@@error , ''

END



IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_IMAGENES_CV'  and xtype='u' )
BEGIN
		
	/*==============================================================*/
	/* Table: POS01_IMAGENES_CV                                     */
	/*==============================================================*/
	create table dbo.POS01_IMAGENES_CV (
	   oi_imagen_cv         int                  not null,
	   oi_cv                int                  not null,
	   oi_imagen            int                  not null,
	   d_imagen_cv          varchar(100)         not null,
	   o_imagen_cv          varchar(1000)        null,
	   constraint PK_POS_IMAGENES_POS primary key nonclustered (oi_imagen_cv)
	)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.POS01_IMAGENES_CV ' ,@@error , ''


 END		



IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_IMAGENES_CV' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_POS01_IM_REFERENCE_POS25_IM' ) )
BEGIN
	alter table dbo.POS01_IMAGENES_CV
	   add constraint FK_POS01_IM_REFERENCE_POS25_IM foreign key (oi_imagen)
	      references dbo.POS25_IMAGENES (oi_imagen)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_POS01_IM_REFERENCE_POS25_IM ' ,@@error , ''

END


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_IMAGENES_CV' and id in 
		(SELECT  parent_obj FROM  sysobjects  NOLOCK  WHERE name ='FK_CV_IMAGENES' ) )
BEGIN
	alter table dbo.POS01_IMAGENES_CV
	   add constraint FK_CV_IMAGENES foreign key (oi_cv)
	      references dbo.POS01_CV (oi_cv)

	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add constraint FK_CV_IMAGENES ' ,@@error , ''

END


   






exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

select @Result = l_Finalizo from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

IF @Result = 1 
	PRINT '***** El SCRIPT TERMINO BIEN ******'
else
	PRINT '***** VERIFICAR HAY ERRORES!!!!! ******'
	select * from dbo.NMD_LogScripts Where oi_Version = @oi_Version 




