Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)

select @ScripName = 'BASE.ORG.chg_ddo_paises', @n_Version = 1 

exec dbo.sp_NMD_VerifScript @n_Version ,@ScripName  , 0 ,@Result OUTPUT

select @oi_Version =oi_Version from dbo.NMD_Scripts Where c_Version = @n_Version and d_script=@ScripName 

select @d_Paso ='sp_NMD_VerifScript Version ' + str(@n_Version)  

IF @Result=999
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName ,'ORGANIZACION',@d_Paso ,999 , 'ERROR: El script ya corrio'
	Select 'ERROR 1'
	return

END


EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'Comienza la ejecucion ',0 , ''

IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG19_ZONAS'  and xtype='u')
BEGIN
	drop table dbo.ORG19_ZONAS
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'drop table dbo.ORG19_ZONAS',@@error , ''
END

/*==============================================================*/
/* Table: ORG19_ZONAS                                           */
/*==============================================================*/
	create table dbo.ORG19_ZONAS (
	   oi_zona              int                  not null,
	   oi_localidad         int                  null,
	   c_zona               varchar(30)          not null,
	   d_zona               varchar(100)         not null,
	   constraint PK_ORG19_ZONAS primary key (oi_zona)
	)
	
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'Create table dbo.ORG19_ZONAS',@@error , ''
	
	IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE  NAME ='AK_AK1_ORG_ZONAS_ORG19_ZO')
	BEGIN
		alter table dbo.ORG19_ZONAS   add constraint AK_AK1_ORG_ZONAS_ORG19_ZO unique (c_zona, oi_localidad)
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'add constraint AK_AK1_ORG_ZONAS_ORG19_ZO ',@@error , ''
	END
	
	IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG19_LOCALIDADES'  and xtype='u')
	BEGIN 
		IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE  NAME ='FK_ORG19_ZO_REFERENCE_ORG19_LO')
		BEGIN
			alter table dbo.ORG19_ZONAS
			   add constraint FK_ORG19_ZO_REFERENCE_ORG19_LO foreign key (oi_localidad)
			      references dbo.ORG19_LOCALIDADES (oi_localidad)
	
			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'add constraint AK_AK1_ORG_ZONAS_ORG19_ZO ',@@error , ''
		END	
	END
	
	
	
/*==============================================================*/
/* Table: ORG31_REGIONES                                        */
/*==============================================================*/
	IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG31_REGIONES'  and xtype='u')
	BEGIN

		create table dbo.ORG31_REGIONES (
		   oi_region            int                  not null,
		   c_region             varchar(30)          not null,
		   d_region             varchar(100)         not null,
		   o_region             varchar(1000)        null,
		   constraint PK_ORG31_REGIONES primary key (oi_region)
		)

		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'create table dbo.ORG31_REGIONES',@@error , ''

		IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE  NAME ='AK_REGIONES_ORG31_RE')
		BEGIN
			alter table dbo.ORG31_REGIONES   add constraint AK_REGIONES_ORG31_RE unique (c_region)
			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'add constraint AK_REGIONES_ORG31_RE',@@error , ''
		END
	END


	IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG19_PROVINCIAS'  and xtype='u')
	BEGIN 
		IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE id in 
					(SELECT  id FROM  syscolumns NOLOCK WHERE name ='oi_region' )
				AND NAME ='ORG19_PROVINCIAS'
				)
		   BEGIN
			alter table dbo.ORG19_PROVINCIAS add oi_region int null
			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'ORG19_PROVINCIAS add oi_region ',@@error , ''
		   END
	
		IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE name ='FK_ORG19_PR_REFERENCE_ORG31_RE') 
		   BEGIN
			alter table dbo.ORG19_PROVINCIAS  add constraint FK_ORG19_PR_REFERENCE_ORG31_RE foreign key (oi_region)  references dbo.ORG31_REGIONES (oi_region)
			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'constraint FK_ORG19_PR_REFERENCE_ORG31_RE',@@error , ''

		   END

	
	
	END



/*==============================================================*/
/* Table: ORG32_MUNICIPIOS                                      */
/*==============================================================*/
	IF NOT EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG32_MUNICIPIOS'  and xtype='u')
	BEGIN
		create table dbo.ORG32_MUNICIPIOS (
		   oi_municipio         int                  not null,
		   c_municipio          varchar(30)          not null,
		   d_municipio          varchar(100)         not null,
		   o_municipio          varchar(1000)        null,
		   constraint PK_ORG32_MUNICIPIOS primary key (oi_municipio)
		)
		EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'create table dbo.ORG32_MUNICIPIOS ',@@error , ''
		IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE NAME ='AK_MUNICIPIOS_ORG32_MU')
		BEGIN 	
			alter table dbo.ORG32_MUNICIPIOS   add constraint AK_MUNICIPIOS_ORG32_MU unique (c_municipio)
			EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'constraint AK_MUNICIPIOS_ORG32_MU ',@@error , ''
		
		END
		
		IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='ORG19_LOCALIDADES'  and xtype='u')
		BEGIN 
			IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE id in 
						(SELECT  id FROM  syscolumns NOLOCK WHERE name ='oi_municipio' )
					AND NAME ='ORG19_LOCALIDADES'
					)
			   BEGIN
				alter table dbo.ORG19_LOCALIDADES add oi_municipio int null
				EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'dbo.ORG19_LOCALIDADES / oi_municipio ',@@error , ''
	
			   END
		
			IF NOT EXISTS (SELECT * FROM sysobjects NOLOCK WHERE name ='FK_ORG19_LO_REFERENCE_ORG32_MU') 
			   BEGIN
				alter table dbo.ORG19_LOCALIDADES   
				add constraint FK_ORG19_LO_REFERENCE_ORG32_MU foreign key (oi_municipio)     
				references dbo.ORG32_MUNICIPIOS (oi_municipio)

				EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'add FK_ORG19_LO_REFERENCE_ORG32_MU ',@@error , ''

			   END
		END
	END
		

EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'INICIA LOS INSERT',@@error , ''

INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (1,   'BA001', '25 de Mayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (2,   'BA002', '9 de Julio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (3,   'BA003', 'Adolfo Alsina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (4,   'BA004', 'Alberti')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (5,   'BA005', 'Alem')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (6,   'BA006', 'Alte.Brown')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (7,   'BA007', 'Alvarado')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (8,   'BA008', 'Alvear')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (9,   'BA009', 'Amheghino')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (10,  'BA010', 'Arenales')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (11,  'BA011', 'Arrecifes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (12,  'BA012', 'Avellaneda')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (13,  'BA013', 'Ayacucho')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (14,  'BA014', 'Azul')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (15,  'BA015', 'Bahia Blanca')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (16,  'BA016', 'Balcarse')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (17,  'BA017', 'Baradero')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (18,  'BA018', 'Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (19,  'BA019', 'Benito Juarez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (20,  'BA020', 'Berazategui')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (21,  'BA021', 'Beriso')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (22,  'BA022', 'Bolivar')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (23,  'BA023', 'Bragado')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (24,  'BA024', 'Brandsend')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (25,  'BA025', 'Campana')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (26,  'BA027', 'Carlos Casares')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (27,  'BA028', 'Carlos Tejedor')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (28,  'BA029', 'Carmen de Areco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (29,  'BA030', 'Castelli')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (30,  'BA031', 'Cnel. Rosales')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (31,  'BA032', 'Cnel. Suarez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (32,  'BA033', 'Colon')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (33,  'BA034', 'Chacabuco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (34,  'BA035', 'Chascomus')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (35,  'BA036', 'Chivilcoy')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (36,  'BA037', 'Daireaux')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (37,  'BA038', 'Dolores')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (38,  'BA039', 'Dorrego')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (39,  'BA040', 'Ensenada')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (40,  'BA041', 'Escobar')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (41,  'BA042', 'Esteban Echeverria')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (42,  'BA043', 'Exaltacion de la Cruz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (43,  'BA044', 'Ezeiza')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (44,  'BA045', 'Florencio Varela')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (45,  'BA047', 'Gonzalez Chaves')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (46,  'BA048', 'Gral. Guido')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (47,  'BA049', 'Gral. Paz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (48,  'BA050', 'Gral. Pinto')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (49,  'BA051', 'Gral. Pueyrredon')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (50,  'BA052', 'Gral. Rodriguez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (51,  'BA054', 'Gral. Villegas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (52,  'BA055', 'Guamini')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (53,  'BA056', 'Hurlighan')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (54,  'BA057', 'Ituzaingo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (55,  'BA058', 'Jose C. Paz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (56,  'BA059', 'Junin')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (57,  'BA060', 'La Costa')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (58,  'BA061', 'La Madrid')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (59,  'BA062', 'La Matanza')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (60,  'BA063', 'La Plata')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (61,  'BA064', 'Lanus')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (62,  'BA065', 'Laprida')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (63,  'BA066', 'Las Flores')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (64,  'BA067', 'Las Heras')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (65,  'BA068', 'Lavalle')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (66,  'BA069', 'Lincoln')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (67,  'BA070', 'Loberia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (68,  'BA071', 'Lobos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (69,  'BA072', 'Lomas de Zamora')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (70,  'BA073', 'Lujan')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (71,  'BA074', 'Madariaga')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (72,  'BA075', 'Magdalena')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (73,  'BA076', 'Maipu')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (74,  'BA077', 'Malvinas Argentinas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (75,  'BA078', 'Mar Chiquita')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (76,  'BA079', 'Marcos Paz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (77,  'BA080', 'Mercedes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (78,  'BA081', 'Merlo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (79,  'BA082', 'Monte Hermoso')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (80,  'BA083', 'Moreno')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (81,  'BA084', 'Moron')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (82,  'BA085', 'Navarro')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (83,  'BA086', 'Necochea')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (84,  'BA088', 'Patagones')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (85,  'BA089', 'Pehuajo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (86,  'BA090', 'Pellegrini')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (87,  'BA091', 'Pergamino')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (88,  'BA092', 'Pilar')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (89,  'BA093', 'Pinamar')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (90,  'BA094', 'Pringles')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (91,  'BA095', 'Pte. Peron')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (92,  'BA096', 'Puan')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (93,  'BA097', 'Punta Indio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (94,  'BA098', 'Quilmes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (95,  'BA099', 'Ramallo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (96,  'BA100', 'Rauch')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (97,  'BA101', 'Rivadavia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (98,  'BA102', 'Rojas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (99,  'BA103', 'Roque Perez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (100, 'BA104', 'Saavedra')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (101, 'BA105', 'Salto')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (102, 'BA106', 'Salliquelo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (103, 'BA107', 'San Andres de Giles')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (104, 'BA108', 'San Antonio de Areco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (105, 'BA109', 'San cayetano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (106, 'BA110', 'San Fernando')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (107, 'BA111', 'San Isidro')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (108, 'BA112', 'San Miguel')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (109, 'BA113', 'San Nicolas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (110, 'BA114', 'San Pedro')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (111, 'BA115', 'San Vicente')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (112, 'BA116', 'Sarmiento')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (113, 'BA117', 'Suipacha')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (114, 'BA118', 'Tandil')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (115, 'BA119', 'Tapalque')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (116, 'BA120', 'Tigre')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (117, 'BA121', 'Tordillo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (118, 'BA122', 'Tornquist')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (119, 'BA123', 'Trenque Lauquen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (120, 'BA124', 'Tres Arrollos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (121, 'BA125', 'Tres de Febrero')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (122, 'BA126', 'Tres Lomas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (123, 'BA127', 'Viamonte')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (124, 'BA128', 'Vicente Lopez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (125, 'BA129', 'Villa Gesel')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (126, 'BA130', 'Villarino')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (127, 'BA131', 'Yrigoyen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (128, 'BA132', 'Zarate')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (129, 'BA133', 'Canuelas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (130, 'BA134', 'Gral. San Martin')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (131, 'BA135', 'Olavarria')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (132, 'BA136', 'Pila')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (133, 'BA137', 'Saladillo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (134, 'CA001', 'Ambato')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (135, 'CA002', 'Ancasti')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (136, 'CA003', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (137, 'CA004', 'Antofagasta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (138, 'CA005', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (139, 'CA006', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (140, 'CA007', 'Capital Catamarca')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (141, 'CA008', 'El Alto')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (142, 'CA009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (143, 'CA010', 'La Paz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (144, 'CA011', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (145, 'CA012', 'Poman')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (146, 'CA013', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (147, 'CA014', 'Santa Rosa')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (148, 'CA015', 'Tinogasta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (149, 'CB001', 'Calamuchita')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (150, 'CB002', 'Cordoba')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (152, 'CB003', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (153, 'CB004', 'Cruz del Eje')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (154, 'CB006', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (155, 'CB007', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (156, 'CB008', 'Juarez Celman')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (157, 'CB009', 'Marcos Juarez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (158, 'CB010', 'Minas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (159, 'CB011', 'Pocho')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (160, 'CB013', 'Punilla')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (161, 'CB014', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (162, 'CB015', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (163, 'CB016', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (164, 'CB017', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (165, 'CB018', 'San Alberto')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (166, 'CB019', 'San Javier')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (167, 'CB020', 'San Justo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (168, 'CB021', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (169, 'CB022', 'Sobremonte')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (170, 'CB023', 'Tercero Arriba')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (171, 'CB024', 'Totoral')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (172, 'CB025', 'Tulumba')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (173, 'CB026', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (174, 'CF001', 'Capital Federal')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (175, 'CH001', '1 de Mayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (176, 'CH002', '12 de Octubre')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (177, 'CH004', '25 de Mayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (178, 'CH005', '9 de Julio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (179, 'CH006', 'Almte. Brown')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (180, 'CH007', 'Bermejo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (181, 'CH008', 'Comandante Fernandez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (182, 'CH011', 'General Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (183, 'CH012', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (184, 'CH013', 'General Guemas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (185, 'CH014', 'Independencia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (186, 'CH015', 'Libertad')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (187, 'CH016', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (188, 'CH017', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (189, 'CH018', 'Mayor Luis J. Fontana')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (190, 'CH019', 'O Higgins')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (191, 'CH020', 'Presidencia de la Pla')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (192, 'CH021', 'Quirtilipi')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (193, 'CH022', 'San Fernando')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (194, 'CH023', 'San Lorenzo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (195, 'CH024', 'Sgto. Cabral')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (196, 'CH025', 'Tapenaga')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (197, 'CO001', 'Bella Vista')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (198, 'CO002', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (199, 'CO003', 'Capital Corrientes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (200, 'CO004', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (201, 'CO005', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (202, 'CO006', 'Empedrado')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (203, 'CO007', 'Esquina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (204, 'CO008', 'Goya')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (205, 'CO009', 'Gral Paz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (206, 'CO010', 'Gral. Alvear')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (207, 'CO011', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (208, 'CO012', 'Ituzaingo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (209, 'CO013', 'Lavalle')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (210, 'CO014', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (211, 'CO015', 'Mercedes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (212, 'CO016', 'Monte Caseros')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (213, 'CO017', 'Paso de Los Libres')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (214, 'CO018', 'Saladas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (215, 'CO019', 'San Cosme')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (216, 'CO020', 'San Luis de Palmar')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (217, 'CO021', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (218, 'CO022', 'San Miguel')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (219, 'CO023', 'San Roque')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (220, 'CO024', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (221, 'CO025', 'Sauce')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (222, 'CT003', 'Cushamen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (223, 'CT004', 'Escalante')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (224, 'CT005', 'Florentino Ameghino')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (225, 'CT006', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (226, 'CT007', 'Gaiman')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (227, 'CT008', 'Gastre')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (228, 'CT009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (229, 'CT010', 'Martires')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (230, 'CT011', 'Paso de Indios')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (231, 'CT012', 'Rawson')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (232, 'CT013', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (233, 'CT014', 'Sarmiento')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (234, 'CT015', 'Tehuelches')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (235, 'CT016', 'Teisen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (236, 'ER001', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (237, 'ER002', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (238, 'ER004', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (239, 'ER005', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (240, 'ER006', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (241, 'ER007', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (242, 'ER008', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (243, 'ER009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (244, 'ER010', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (245, 'ER011', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (246, 'ER012', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (247, 'ER013', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (248, 'ER014', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (249, 'ER015', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (250, 'ER016', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (251, 'FZ001', 'Bermejo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (252, 'FZ002', 'laishi')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (253, 'FZ003', 'Matacos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (254, 'FZ004', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (255, 'FZ005', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (256, 'FZ006', 'Pilcomayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (257, 'FZ007', 'Pirane')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (258, 'FZ008', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (259, 'FZ009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (260, 'JU001', 'Cochinoca')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (261, 'JU002', 'Dr. Manuel Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (262, 'JU003', 'El carmen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (263, 'JU004', 'Humahuaca')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (264, 'JU005', 'Ledesma')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (265, 'JU006', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (266, 'JU007', 'Rinconada')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (267, 'JU008', 'San Antonio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (268, 'JU009', 'San Pedro')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (269, 'JU010', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (270, 'JU011', 'Sta. Catalina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (271, 'JU012', 'Susques')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (272, 'JU013', 'Tilcara')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (273, 'JU014', 'Tumbaya')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (274, 'JU015', 'Valle Grande')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (275, 'JU016', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (276, 'JU017', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (277, 'LP001', 'Atrueco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (278, 'LP002', 'Caleu Caleu')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (279, 'LP003', 'Santa Rosa')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (280, 'LP004', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (281, 'LP005', 'Conhelo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (282, 'LP006', 'Curaco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (283, 'LP007', 'Chalileo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (284, 'LP008', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (285, 'LP009', 'Chicalco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (286, 'LP010', 'Guatrache')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (287, 'LP011', 'Hucal')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (288, 'LP012', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (289, 'LP013', 'Limay Mahuida')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (290, 'LP014', 'Loventue')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (291, 'LP015', 'Maraco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (292, 'LP016', 'Puelen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (293, 'LP017', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (294, 'LP018', 'Rancul')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (295, 'LP019', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (296, 'LP020', 'Toay')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (297, 'LP021', 'Trenel')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (298, 'LP022', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (299, 'LR001', 'Arauco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (300, 'LR002', 'Capital La Rioja')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (301, 'LR003', 'Castro Barros')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (302, 'LR004', 'Cnel. Felipe Varela')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (303, 'LR005', 'Chamical')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (304, 'LR006', 'Chilecito')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (305, 'LR007', 'Famatina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (306, 'LR008', 'Gra. Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (307, 'LR009', 'Gral. J.F. Quiroga')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (308, 'LR011', 'Gral. Ocampo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (309, 'LR013', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (310, 'LR015', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (311, 'LR016', 'San Blas de los Sauce')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (312, 'LR017', 'Sanagasta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (313, 'LR018', 'Vinchina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (314, 'MI001', '25 de Mayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (315, 'MI002', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (316, 'MI003', 'Cainguas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (317, 'MI004', 'Candelaria')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (318, 'MI005', 'Capital Misiones')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (319, 'MI006', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (320, 'MI007', 'El Dorado')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (321, 'MI008', 'Gral. Manuel Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (322, 'MI009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (323, 'MI010', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (324, 'MI011', 'Leandro N. Alem')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (325, 'MI012', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (326, 'MI013', 'Montecarlo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (327, 'MI014', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (328, 'MI015', 'San Ignacio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (329, 'MI016', 'San Javier')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (330, 'MZ001', 'Capital Mendoza')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (331, 'MZ002', 'Godoy Cruz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (332, 'MZ003', 'Gral. Alvear')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (333, 'MZ004', 'Guaymallen')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (334, 'MZ005', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (335, 'MZ006', 'La Paz')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (336, 'MZ007', 'Las Heras')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (337, 'MZ008', 'Lavalle')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (338, 'MZ009', 'Lujan de Cuyo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (339, 'MZ010', 'Maipu')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (340, 'MZ011', 'Malargue')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (341, 'MZ012', 'Rivadavia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (342, 'MZ013', 'San Carlos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (343, 'MZ015', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (344, 'MZ016', 'San Rafael')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (345, 'MZ017', 'Sta. Rosa')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (346, 'MZ018', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (347, 'MZ019', 'Tupungato')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (348, 'NQ001', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (349, 'NQ002', 'Anelo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (350, 'NQ003', 'Catan-Lil')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (351, 'NQ004', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (352, 'NQ005', 'Confluencia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (353, 'NQ006', 'Chos-Malal')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (354, 'NQ007', 'Huiliches')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (355, 'NQ008', 'Lacar')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (356, 'NQ010', 'Los Lagos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (357, 'NQ011', 'Minas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (358, 'NQ012', 'Norquin')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (359, 'NQ013', 'Pehuenches')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (360, 'NQ014', 'Picunches')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (361, 'NQ015', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (362, 'NQ016', 'Zapala')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (363, 'RN001', '25 de Mayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (364, 'RN002', '9 de Julio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (365, 'RN003', 'Adolfo Alsina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (366, 'RN004', 'Avellaneda')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (367, 'RN005', 'Bariloche')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (368, 'RN006', 'Conesa')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (369, 'RN008', 'Gral. Roca')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (370, 'RN009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (371, 'RN011', 'Pilcaniyeu')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (372, 'RN012', 'San Antonio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (373, 'RN013', 'Valcheta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (374, 'SA004', 'Capital Salta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (375, 'SA005', 'Cerrillos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (376, 'SA006', 'Chicoana')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (377, 'SA007', 'Gral. Guemes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (378, 'SA008', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (379, 'SA009', 'Guachipas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (380, 'SA010', 'Iruya')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (381, 'SA011', 'La Caldera')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (382, 'SA012', 'La Candelaria')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (383, 'SA013', 'La Poma')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (384, 'SA014', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (385, 'SA016', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (386, 'SA018', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (387, 'SA019', 'Rivadavia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (388, 'SA020', 'Rosario de la Fronter')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (389, 'SA021', 'Rosario de Lerma')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (390, 'SA022', 'San Carlos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (391, 'SA023', 'Sta. Victoria')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (392, 'SC001', 'Corpen Aike')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (393, 'SC002', 'Deseado')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (394, 'SC003', 'Guer Aike')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (395, 'SC004', 'Lago Argentino')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (396, 'SC005', 'Lago Bs.As.')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (397, 'SC006', 'Magallanes')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (398, 'SC007', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (399, 'SE001', 'Aguirre')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (400, 'SE002', 'Alberti')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (401, 'SE003', 'Atamisqui')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (402, 'SE004', 'Avellaneda')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (403, 'SE005', 'Banda')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (404, 'SE006', 'Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (405, 'SE007', 'Capital Santiago del')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (406, 'SE008', 'Copo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (407, 'SE009', 'Choya')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (408, 'SE010', 'Figueroa')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (409, 'SE011', 'Gral.Taboada')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (410, 'SE012', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (411, 'SE013', 'Jimenez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (412, 'SE014', 'Juan F. Ibarra')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (413, 'SE015', 'Loreto')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (414, 'SE016', 'Mitre')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (415, 'SE017', 'Moreno')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (416, 'SE018', 'Ojo de Agua')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (417, 'SE019', 'Pellegrini')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (418, 'SE020', 'Quebrachos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (419, 'SE021', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (420, 'SE022', 'Rivadavia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (421, 'SE023', 'Robles')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (422, 'SE024', 'Salavina')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (423, 'SE025', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (424, 'SE026', 'Sarmiento')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (425, 'SE027', 'Silipica')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (426, 'SF001', '9 de Julio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (427, 'SF002', 'Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (428, 'SF003', 'Caseros')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (429, 'SF004', 'Castellanos')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (430, 'SF005', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (431, 'SF006', 'Garay')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (432, 'SF007', 'Gral. Lopez')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (433, 'SF008', 'Gral. Obligado')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (434, 'SF009', 'Iriondo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (435, 'SF010', 'La Capital Santa Fe')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (436, 'SF011', 'Las Colonias')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (437, 'SF012', 'Rosario')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (438, 'SF013', 'San Cristobal')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (439, 'SF014', 'San Javier')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (440, 'SF015', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (441, 'SF016', 'San Justo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (442, 'SF017', 'San Lorenzo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (443, 'SF018', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (444, 'SF019', 'Vera')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (445, 'SJ001', '25 de Mayo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (446, 'SJ002', '9 de Julio')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (447, 'SJ003', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (448, 'SJ004', 'Angaco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (449, 'SJ005', 'Calingasta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (450, 'SJ007', 'Caucete')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (451, 'SJ008', 'Chimbas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (452, 'SJ009', 'Iglesias')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (453, 'SJ010', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (454, 'SJ011', 'Porcito')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (455, 'SJ012', 'Rawson')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (456, 'SJ013', 'Rivadavia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (457, 'SJ014', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (458, 'SJ015', 'Sarmiento')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (459, 'SJ017', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (460, 'SJ018', 'Valle Fertil')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (461, 'SL001', 'Ayacucho')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (462, 'SL002', 'Belgrano')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (463, 'SL003', 'Capital San Luis')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (464, 'SL004', 'Cnel. Pringles')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (465, 'SL005', 'Chacabuco')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (466, 'SL006', 'Gobernador Dupuy')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (467, 'SL007', 'Gral.Pedernera')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (468, 'SL008', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (469, 'SL009', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (470, 'TF001', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (471, 'TF002', 'Ushuaia')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (472, 'TF004', 'Sin Descripción')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (473, 'TU001', 'Burruyacu')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (474, 'TU002', 'Tucuman')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (476, 'TU003', 'Cruz Alta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (477, 'TU004', 'Chicligasta')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (478, 'TU005', 'Faimalla')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (479, 'TU006', 'Graneros')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (480, 'TU007', 'Juan B. Alberdi')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (481, 'TU008', 'La Cocha')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (482, 'TU009', 'Leales')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (483, 'TU010', 'Lules')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (484, 'TU011', 'Monteros')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (485, 'TU012', 'Rio Chico')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (486, 'TU013', 'Simoca')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (487, 'TU014', 'Tafi del Valle')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (488, 'TU015', 'Tafi Viejo')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (489, 'TU016', 'Tranchas')
INSERT INTO ORG32_MUNICIPIOS (oi_municipio, c_municipio, d_municipio) VALUES (490, 'TU017', 'Yerba Buena')

declare @cant char(30) 
Select @cant =  'insertados :' +  convert(varchar(3),count(*) )    from ORG32_MUNICIPIOS 
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', @cant ,@@error , ''

exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , 'ORGANIZACION', 'Finalizo la ejecucion',@Result , ''	

