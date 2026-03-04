Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.ORG.cambio_paises_provincias', @n_Version = 1 , @d_modulo ='ORGANIZACION'

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

/*==============================================================*/
/* Table: ORG35_PAISES                                          */
/*==============================================================*/
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG35_PAISES'  and xtype='u')
BEGIN
	create table dbo.ORG35_PAISES (
	   oi_pais              int                  not null,
	   c_pais               varchar(30)          not null,
	   d_pais               varchar(100)         not null,
	   oi_idioma            int                  null,
	   oi_nacionalidad      int                  null,
	   o_pais               varchar(1000)        null,
	   constraint PK_ORG35_PAISES primary key (oi_pais),
	   constraint AK_AK1_ORG_PAISES_ORG35_PA unique (c_pais)
	)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.ORG35_PAISES ',@@error , ''

END

/*==============================================================*/
/* Table: ORG35_PROVINCIAS                                      */
/*==============================================================*/
IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG35_PROVINCIAS'  and xtype='u')
BEGIN

	create table dbo.ORG35_PROVINCIAS (
	   oi_provincia         int                  not null,
	   oi_region            int                  null,
	   oi_pais              int                  not null,
	   c_provincia          varchar(30)          not null,
	   d_provincia          varchar(100)         not null,
	   c_provincia_sicore   varchar(30)          null,
	   c_prov_expl_afjp     varchar(30)          null,
	   o_provincia          varchar(1000)        null,
	   constraint PK_ORG35_PROVINCIAS primary key (oi_provincia),
	   constraint AK_AK1_ORG_PROVINCIAS_ORG35_PR unique (c_provincia)
	)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' create table dbo.ORG35_PROVINCIAS ',@@error , ''

END


insert into ORG35_PAISES (oi_pais,c_pais ,d_pais ,oi_idioma ,oi_nacionalidad ,o_pais )
select oi_pais,c_pais ,d_pais ,oi_idioma ,oi_nacionalidad ,o_pais  from ORG19_PAISES

 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' insert into ORG35_PAISES ',@@error , ''

insert into ORG35_PROVINCIAS (oi_provincia,oi_pais ,c_provincia  ,d_provincia ,c_provincia_sicore ,o_provincia ,oi_region   )
select oi_provincia,oi_pais ,c_provincia  ,d_provincia ,c_provincia_sicore ,o_provincia ,oi_region   from ORG19_PROVINCIAS
 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' insert into ORG35_PROVINCIAS ',@@error , ''


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__PER_PAISES__oi_na__417A6027' )
BEGIN
	alter table dbo.ORG35_PAISES
	   add constraint FK__PER_PAISES__oi_na__417A6027 foreign key (oi_nacionalidad)
	      references dbo.ORG12_NACIONALID (oi_nacionalidad)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK__PER_PAISES__oi_na__417A6027 ',@@error , ''

END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__PER_PROVINCIA__oi_pa__37BBEBC3' )
BEGIN
	alter table dbo.ORG35_PROVINCIAS
	   add constraint FK__PER_PROVINCIA__oi_pa__37BBEBC3 foreign key (oi_pais)
	      references dbo.ORG35_PAISES (oi_pais)
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' FK__PER_PROVINCIA__oi_pa__37BBEBC3 ',@@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_ORG35_PROV_REFERENCE_ORG31_RE' )
BEGIN
	alter table dbo.ORG35_PROVINCIAS
	   add constraint FK_ORG35_PROV_REFERENCE_ORG31_RE foreign key (oi_region)
	      references dbo.ORG31_REGIONES (oi_region)
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' FK_ORG35_PROV_REFERENCE_ORG31_RE ',@@error , ''
END

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de países										*/
/*==============================================================*/

IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name in ('FK_POS01_CV_REFERENCE_ORG19_PAN' 
	,'FK_POS01_CV_REFERENCE_ORG19_PAR','FK_POS03_OF_REFERENCE_ORG19_PA' ,'FK_POS17_BU_REFERENCE_ORG19_PA1'
	,'FK_POS17_BU_REFERENCE_ORG19_PA2')  )
BEGIN
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' inicia 5 DROP CONSTRAINT de Países',@@error , ''
		ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PAN]
		ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PAR]
		ALTER TABLE [dbo].[POS03_OFERTAS_LAB] DROP CONSTRAINT [FK_POS03_OF_REFERENCE_ORG19_PA]
		ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PA1]
		ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PA2]
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' fin 5 DROP CONSTRAINT de Países',@@error , ''
END


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS01_CV_REFERENCE_ORG35_PAN' )
BEGIN

	ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35_PAN] FOREIGN KEY 
		(
			[oi_pais_nac]
		) REFERENCES [ORG35_PAISES] (
			[oi_pais]
		)
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_POS01_CV_REFERENCE_ORG35_PAN  ',@@error , ''

END 

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS01_CV_REFERENCE_ORG35PAR' )
BEGIN

	ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35PAR] FOREIGN KEY 
		(
			[oi_pais]
		) REFERENCES [ORG35_PAISES] (
			[oi_pais]
		)
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_POS01_CV_REFERENCE_ORG35PAR  ',@@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS03_OF_REFERENCE_ORG35_PA' )
BEGIN

	ALTER TABLE [dbo].[POS03_OFERTAS_LAB] ADD CONSTRAINT [FK_POS03_OF_REFERENCE_ORG35_PA] FOREIGN KEY 
		(
			[oi_pais]
		) REFERENCES [ORG35_PAISES] (
			[oi_pais]
		)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_POS03_OF_REFERENCE_ORG35_PA  ',@@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS17_BU_REFERENCE_ORG35_PA1' )
BEGIN
	ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PA1] FOREIGN KEY 
		(
			[oi_pais_1]
		) REFERENCES [ORG35_PAISES] (
			[oi_pais]
		)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_POS17_BU_REFERENCE_ORG35_PA1  ',@@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS17_BU_REFERENCE_ORG35_PA2' )
BEGIN

	ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PA2] FOREIGN KEY 
		(
			[oi_pais_2]
		) REFERENCES [ORG35_PAISES] (
			[oi_pais]
		)
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK_POS17_BU_REFERENCE_ORG35_PA2  ',@@error , ''
END

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-postulantes		*/
/*==============================================================*/

IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name in 
	('FK_POS01_CV_REFERENCE_ORG19_PRN'  ,'FK_POS01_CV_REFERENCE_ORG19_PRR' ,'FK_POS01_LI_REFERENCE_ORG19_PR'  
	,'FK_POS03_OF_REFERENCE_ORG19_PR'  ,'FK_POS17_BU_REFERENCE_ORG19_PR1' ,'FK_POS17_BU_REFERENCE_ORG19_PR2' )  )
BEGIN
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' inicia 5 DROP CONSTRAINT de provincias-postulantes',@@error , ''

	ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PRN] 
	ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PRR] 
	ALTER TABLE [dbo].[POS01_LIB_SANIT] DROP CONSTRAINT [FK_POS01_LI_REFERENCE_ORG19_PR]
	ALTER TABLE [dbo].[POS03_OFERTAS_LAB] DROP CONSTRAINT [FK_POS03_OF_REFERENCE_ORG19_PR] 
	ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PR1] 
	ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PR2] 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' inicia 5 DROP CONSTRAINT de provincias-postulantes',@@error , ''
	
END


IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS01_CV_REFERENCE_ORG35_PRN' )
BEGIN

	ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35_PRN] FOREIGN KEY 
		( 		[oi_provincia_nac] 	) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 	) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK_POS01_CV_REFERENCE_ORG35_PRN',@@error , ''

END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS01_CV_REFERENCE_ORG35_PRR' )
BEGIN

	ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35_PRR] FOREIGN KEY 
	( [oi_provincia] 	) REFERENCES [ORG35_PROVINCIAS] ( [oi_provincia] ) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK_POS01_CV_REFERENCE_ORG35_PRR',@@error , ''
END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS01_LI_REFERENCE_ORG35_PR' )
BEGIN

	ALTER TABLE [dbo].[POS01_LIB_SANIT] ADD CONSTRAINT [FK_POS01_LI_REFERENCE_ORG35_PR] FOREIGN KEY 
		( 
			[oi_provincia] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK_POS01_LI_REFERENCE_ORG35_PR' , @@error , ''

END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS03_OF_REFERENCE_ORG35_PR' )
BEGIN
	ALTER TABLE [dbo].[POS03_OFERTAS_LAB] ADD CONSTRAINT [FK_POS03_OF_REFERENCE_ORG35_PR] FOREIGN KEY 
		( 
			[oi_provincia] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK_POS03_OF_REFERENCE_ORG35_PR' , @@error , ''

END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS17_BU_REFERENCE_ORG35_PR1' )
BEGIN
	ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PR1] FOREIGN KEY 
		( 
			[oi_provincia_1] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK_POS17_BU_REFERENCE_ORG35_PR1' , @@error , ''

END

IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK_POS17_BU_REFERENCE_ORG35_PR2' )
BEGIN
	ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PR2] FOREIGN KEY 
		( 
			[oi_provincia_2] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' add FK_POS17_BU_REFERENCE_ORG35_PR2' , @@error , ''

END

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-organización		*/
/*==============================================================*/
IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__PER_LOCAL__oi_pr__0D0697E8' )
BEGIN
	ALTER TABLE [dbo].[ORG19_LOCALIDADES] DROP CONSTRAINT [FK__PER_LOCAL__oi_pr__0D0697E8]
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' DROP FK__PER_LOCAL__oi_pr__0D0697E8' , @@error , ''

END

IF NOT EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__PER_LOCAL__oi_pr__0D0697E8' )
BEGIN
	ALTER TABLE [dbo].[ORG19_LOCALIDADES] ADD CONSTRAINT [FK__PER_LOCAL__oi_pr__0D0697E8] FOREIGN KEY 
		( 
			[oi_provincia] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK__PER_LOCAL__oi_pr__0D0697E8' , @@error , ''

END
/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-personal				*/
/*==============================================================*/

IF EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__PER_DOCUM__oi_pr__3B3776EC' )
BEGIN
	ALTER TABLE [dbo].[PER01_DOCUM_PER] DROP CONSTRAINT [FK__PER_DOCUM__oi_pr__3B3776EC] 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' DROP FK__PER_DOCUM__oi_pr__3B3776EC' , @@error , ''

END

IF NOT EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__PER_DOCUM__oi_pr__3B3776EC' )
BEGIN
	ALTER TABLE [dbo].[PER01_DOCUM_PER] ADD CONSTRAINT [FK__PER_DOCUM__oi_pr__3B3776EC] FOREIGN KEY 
		( 
			[oi_provincia] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK__PER_DOCUM__oi_pr__3B3776EC' , @@error , ''

END
/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-seleccón				*/
/*==============================================================*/
IF EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__POS_DOCUM__oi_pr__3B3776EC' )
BEGIN
	ALTER TABLE [dbo].[SEL01_DOCUM_POS] DROP CONSTRAINT [FK__POS_DOCUM__oi_pr__3B3776EC] 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' DROP FK__POS_DOCUM__oi_pr__3B3776EC' , @@error , ''

END

IF NOT EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='FK__POS_DOCUM__oi_pr__3B3776EC' )
BEGIN

	ALTER TABLE [dbo].[SEL01_DOCUM_POS] ADD CONSTRAINT [FK__POS_DOCUM__oi_pr__3B3776EC] FOREIGN KEY 
		( 
			[oi_provincia] 
		) REFERENCES [ORG35_PROVINCIAS] ( 
			[oi_provincia] 
		) 
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' ADD FK__POS_DOCUM__oi_pr__3B3776EC' , @@error , ''

END


/*==============================================================*/
/* DROP TABLE ORG19_PROVINCIAS																	*/
/*==============================================================*/
IF  EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG19_PROVINCIAS' )
BEGIN
	DROP TABLE ORG19_PROVINCIAS
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' DROP TABLE ORG19_PROVINCIAS' , @@error , ''

END


/*==============================================================*/
/* DROP TABLE ORG19_PAISES																			*/
/*==============================================================*/
IF  EXISTS(SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG19_PAISES' )
BEGIN
	DROP TABLE ORG19_PAISES
 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, ' DROP TABLE ORG19_PAISES' , @@error , ''

END


exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	


