/*==============================================================*/
/* Table: ORG35_PAISES                                          */
/*==============================================================*/
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
go

alter table dbo.ORG35_PAISES
   add constraint FK__PER_PAISES__oi_na__417A6027 foreign key (oi_nacionalidad)
      references dbo.ORG12_NACIONALID (oi_nacionalidad)
go

/*==============================================================*/
/* Table: ORG35_PROVINCIAS                                      */
/*==============================================================*/
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
go

alter table dbo.ORG35_PROVINCIAS
   add constraint FK__PER_PROVINCIA__oi_pa__37BBEBC3 foreign key (oi_pais)
      references dbo.ORG35_PAISES (oi_pais)
go

alter table dbo.ORG35_PROVINCIAS
   add constraint FK_ORG35_PROV_REFERENCE_ORG31_RE foreign key (oi_region)
      references dbo.ORG31_REGIONES (oi_region)
go

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de países										*/
/*==============================================================*/

ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PAN]

GO

ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PAR]

GO

ALTER TABLE [dbo].[POS03_OFERTAS_LAB] DROP CONSTRAINT [FK_POS03_OF_REFERENCE_ORG19_PA]

GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PA1]

GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PA2]

GO

ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35_PAN] FOREIGN KEY 
	(
		[oi_pais_nac]
	) REFERENCES [ORG35_PAISES] (
		[oi_pais]
	)
GO

ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35PAR] FOREIGN KEY 
	(
		[oi_pais]
	) REFERENCES [ORG35_PAISES] (
		[oi_pais]
	)
GO

ALTER TABLE [dbo].[POS03_OFERTAS_LAB] ADD CONSTRAINT [FK_POS03_OF_REFERENCE_ORG35_PA] FOREIGN KEY 
	(
		[oi_pais]
	) REFERENCES [ORG35_PAISES] (
		[oi_pais]
	)
GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PA1] FOREIGN KEY 
	(
		[oi_pais_1]
	) REFERENCES [ORG35_PAISES] (
		[oi_pais]
	)
GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PA2] FOREIGN KEY 
	(
		[oi_pais_2]
	) REFERENCES [ORG35_PAISES] (
		[oi_pais]
	)
GO

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-postulantes		*/
/*==============================================================*/

ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PRN] 
 
GO

ALTER TABLE [dbo].[POS01_CV] DROP CONSTRAINT [FK_POS01_CV_REFERENCE_ORG19_PRR] 
 
GO

ALTER TABLE [dbo].[POS01_LIB_SANIT] DROP CONSTRAINT [FK_POS01_LI_REFERENCE_ORG19_PR]
 
GO

ALTER TABLE [dbo].[POS03_OFERTAS_LAB] DROP CONSTRAINT [FK_POS03_OF_REFERENCE_ORG19_PR] 
 
GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PR1] 
 
GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] DROP CONSTRAINT [FK_POS17_BU_REFERENCE_ORG19_PR2] 
 
GO

ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35_PRN] FOREIGN KEY 
	( 
		[oi_provincia_nac] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

ALTER TABLE [dbo].[POS01_CV] ADD CONSTRAINT [FK_POS01_CV_REFERENCE_ORG35_PRR] FOREIGN KEY 
	( 
		[oi_provincia] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

ALTER TABLE [dbo].[POS01_LIB_SANIT] ADD CONSTRAINT [FK_POS01_LI_REFERENCE_ORG35_PR] FOREIGN KEY 
	( 
		[oi_provincia] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

ALTER TABLE [dbo].[POS03_OFERTAS_LAB] ADD CONSTRAINT [FK_POS03_OF_REFERENCE_ORG35_PR] FOREIGN KEY 
	( 
		[oi_provincia] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PR1] FOREIGN KEY 
	( 
		[oi_provincia_1] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

ALTER TABLE [dbo].[POS17_BUSQUEDAS] ADD CONSTRAINT [FK_POS17_BU_REFERENCE_ORG35_PR2] FOREIGN KEY 
	( 
		[oi_provincia_2] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-organización		*/
/*==============================================================*/
ALTER TABLE [dbo].[ORG19_LOCALIDADES] DROP CONSTRAINT [FK__PER_LOCAL__oi_pr__0D0697E8]
 
GO 

ALTER TABLE [dbo].[ORG19_LOCALIDADES] ADD CONSTRAINT [FK__PER_LOCAL__oi_pr__0D0697E8] FOREIGN KEY 
	( 
		[oi_provincia] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO 

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-personal				*/
/*==============================================================*/

ALTER TABLE [dbo].[PER01_DOCUM_PER] DROP CONSTRAINT [FK__PER_DOCUM__oi_pr__3B3776EC] 
 
GO

ALTER TABLE [dbo].[PER01_DOCUM_PER] ADD CONSTRAINT [FK__PER_DOCUM__oi_pr__3B3776EC] FOREIGN KEY 
	( 
		[oi_provincia] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

/*==============================================================*/
/* DROPs y ADDs de los CONSTRAINTs de provincias-seleccón				*/
/*==============================================================*/

ALTER TABLE [dbo].[SEL01_DOCUM_POS] DROP CONSTRAINT [FK__POS_DOCUM__oi_pr__3B3776EC] 
 
GO 

ALTER TABLE [dbo].[SEL01_DOCUM_POS] ADD CONSTRAINT [FK__POS_DOCUM__oi_pr__3B3776EC] FOREIGN KEY 
	( 
		[oi_provincia] 
	) REFERENCES [ORG35_PROVINCIAS] ( 
		[oi_provincia] 
	) 
GO

/*==============================================================*/
/* DROP TABLE ORG19_PAISES																			*/
/*==============================================================*/

DROP TABLE ORG19_PAISES

/*==============================================================*/
/* DROP TABLE ORG19_PROVINCIAS																	*/
/*==============================================================*/

DROP TABLE ORG19_PROVINCIAS