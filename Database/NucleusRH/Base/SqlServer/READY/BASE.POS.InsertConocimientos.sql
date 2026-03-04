Declare @n_Version int 
Declare @oi_Version  int 
Declare @Result int 
declare @ScripName varchar (100)
declare @d_Paso varchar (100)
DECLARE @d_modulo varchar (100)

select @ScripName = 'BASE.POS.InsertConocimientos', @n_Version = 1 , @d_modulo ='POSTULANTES'

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


IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS01_CONOC_CV'  and xtype='u')
BEGIN
	DELETE POS01_CONOC_CV
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'DELETE POS01_CONOC_CV ',@@error , ''
END
ELSE
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'la tabla POS01_CONOC_CV no existe',@@error , ''
END


IF EXISTS (SELECT  * FROM  sysobjects NOLOCK  WHERE name ='POS16_CONOCIMIENTOS'  and xtype='u')
BEGIN
	DELETE POS16_CONOCIMIENTOS
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'DELETE POS16_CONOCIMIENTOS ',@@error , ''

	
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(1  , 'AAG001', 'Agenda'                                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '1   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(2  , 'AAR001', 'Armado de Legajos'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '2   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(3  , 'AAS001', 'Asientos de Diarios'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '3   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(4  , 'AAT001', 'Atencion de Proveedores'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '4   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(5  , 'ABA001', 'Base Datos'                                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '5   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(6  , 'ACA001', 'Cajero de Bancos'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '6   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(7  , 'ACA002', 'Cajero de Salon'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '7   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(8  , 'ACA003', 'Cajero de Supermercado'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '8   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(9  , 'ACA005', 'Campañas Promocionales'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '9   ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(10 , 'ACA007', 'Canal Corporativo'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '10  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(11 , 'ACA009', 'Canal Distribucion'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '11  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(12 , 'ACA013', 'Canal Mayorista'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '12  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(13 , 'ACA014', 'Canal Minorista'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '13  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(14 , 'ACA017', 'Canal Puerta a Puerta'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '14  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(15 , 'ACA020', 'Cartas de Credito'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '15  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(16 , 'ACA023', 'Cartera de Clientes'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '16  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(17 , 'ACE003', 'Ceremonial y Protocolo'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '17  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(18 , 'ACE008', 'Certific de Trabajo y Servicio'                 ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '18  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(19 , 'ACO001', 'Conmutador'                                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '19  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(20 , 'ACO003', 'Concurso de Precios'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '20  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(21 , 'ACO008', 'Confeccion de Cheques'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '21  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(22 , 'ACO009', 'Confeccion de Depositos'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '22  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(23 , 'ACO022', 'Contratos Vigentes'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '23  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(24 , 'ACO027', 'Contratacion de Seguros'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '24  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(25 , 'ACO032', 'Control Ausentismo'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '25  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(26 , 'ACO033', 'Control Contenedores'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '26  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(27 , 'ACO034', 'Control Hs Tarjeta Magnetic'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '27  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(28 , 'ACO035', 'Control Hs Tarjeta Reloj'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '28  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(29 , 'ACO038', 'Confecci=n de Encuestas'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '29  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(30 , 'ADE005', 'Desarrollo de Zonas'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '30  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(31 , 'ADI003', 'Digitacion Alfanumerica'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '31  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(32 , 'ADI004', 'Digitacion 12000 Digitos'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '32  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(33 , 'ADI005', 'Digitacion Muy Buena'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '33  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(34 , 'AFO004', 'Fondo Fijo'                                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '34  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(35 , 'AGE003', 'Gestion Aduanera'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '35  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(36 , 'AGE004', 'Gestion Medios Comun'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '36  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(37 , 'AGE005', 'Gestion Post Venta'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '37  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(38 , 'AIM003', 'Imputaciones'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '38  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(39 , 'ALE005', 'Legislacion Depos Fiscales'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '39  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(40 , 'ALL003', 'Llamados Entrantes'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '40  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(41 , 'ALL004', 'Llamados Salientes'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '41  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(42 , 'AMA004', 'Mailing'                                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '42  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(43 , 'AMA012', 'Manejo Operacion Portuarias'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '43  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(44 , 'ANN005', 'Nomenclador Nacional'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '44  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(45 , 'AOR003', 'Ordenes de Pago'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '45  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(46 , 'AOR008', 'Organizacion Cursos'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '46  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(47 , 'AOR009', 'Organizacion Eventos'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '47  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(48 , 'APA001', 'Pautas Publicitaria'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '48  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(49 , 'APA003', 'Pagemaker'                                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '49  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(50 , 'APA008', 'Pago a Proveedores'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '50  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(51 , 'APE005', 'Pegadoras de Canto'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '51  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(52 , 'APL003', 'Planificacion de Cuentas'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '52  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(53 , 'APO005', 'Posnet'                                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '53  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(54 , 'APR001', 'Productos Bancarios'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '54  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(55 , 'APR003', 'Presupuestos'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '55  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(56 , 'APR008', 'Pronosticos de Venta'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '56  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(57 , 'ARE003', 'Recepc y Distrib Correspond'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '57  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(58 , 'ARE008', 'Redaccion Gacetilla'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '58  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(59 , 'ARE009', 'Redaccion Propia'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '59  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(60 , 'ARE014', 'Registraciones Contables'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '60  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(61 , 'ARE019', 'Relaciones Bancarias'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '61  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(62 , 'ARE022', 'Relevamiento de datos'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '62  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(63 , 'ARE028', 'Realización de encuestas'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '63  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(64 , 'ASE004', 'Seguimiento de Saldos'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '64  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(65 , 'ASE010', 'Seleccion de Material POP'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '65  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(66 , 'ASR005', 'Stradivarius'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '66  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(67 , 'ATE003', 'Atención telef=nica'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '67  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(68 , 'ATR003', 'Tramites Bancarios'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '68  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(69 , 'ATR004', 'Tramites Generales'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '69  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(70 , 'AVE001', 'Ventas'                                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '70  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(71 , 'AVE003', 'Venta de Servicios'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '71  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(72 , 'AVE004', 'Venta de Productos'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '72  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(73 , 'AVE005', 'Ventas Salon'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '73  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(74 , 'AVE008', 'Venta telefónica servicios'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '74  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(75 , 'AVE009', 'Venta telefónica productos'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '75  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(76 , 'CAU005', 'Autocad'                                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '76  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(77 , 'CCI005', 'Cierre de Cajas'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '77  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(78 , 'CCO004', 'Corel Draw'                                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '78  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(79 , 'CCO006', 'Confeccion Remitos'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '79  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(80 , 'CCO014', 'Control de Entrega Tickets'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '80  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(81 , 'CCO015', 'Control Ing Egr Mercaderias'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '81  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(82 , 'CCO016', 'Control de Pedidos'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '82  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(83 , 'CCO017', 'Control de Stock'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '83  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(84 , 'CCU005', 'Cuotas Hilton'                                  ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '84  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(85 , 'CDE005', 'Deteccion Billetes Falsos'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '85  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(86 , 'CHA005', 'Handhell para Pedido'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '86  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(87 , 'CIL005', 'Ilustrator'                                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '87  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(88 , 'CNO003', 'Normas GMP'                                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '88  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(89 , 'CTO001', 'Toma de Pedidos'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '89  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(90 , 'FAC003', 'Acreditacion'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '90  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(91 , 'FAR004', 'Armado de Ruteos'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '91  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(92 , 'FCA005', 'Cartas de Presentac'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '92  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(93 , 'FCO003', 'Compras Almacen'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '93  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(94 , 'FCO004', 'Compras Bebidas'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '94  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(95 , 'FCO005', 'Compras Carne'                                  ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '95  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(96 , 'FCO006', 'Compras Fiambres'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '96  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(97 , 'FCO007', 'Compras Frutas y Verduras'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '97  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(98 , 'FCO008', 'Compras Higiene'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '98  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(99 , 'FCO009', 'Compras Lacteos'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '99  ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(100, 'FCO010', 'Compras Productos de Granja'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '100 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(101, 'FCO015', 'Coordinac Eventos'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '101 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(102, 'FCO016', 'Coordinac Promocion'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '102 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(103, 'GAL003', 'Alinear Capacit con Obj Cia'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '103 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(104, 'GAL004', 'Alinear RRHH con Obj Estrat'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '104 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(105, 'GAN003', 'Analisis FODA'                                  ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '105 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(106, 'GCR003', 'CRM'                                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '106 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(107, 'GDE001', 'Desarr Politicas de RRHH'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '107 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(108, 'GDE002', 'Desarr Presupuesto Anual'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '108 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(109, 'GDE003', 'Desarr Estrategia Ccial'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '109 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(110, 'GDE004', 'Desarr Estruct Beneficios'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '110 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(111, 'GDE005', 'Desarr Estruct Incentivos'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '111 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(112, 'GDE006', 'Desarr Estruct Remunerac'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '112 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(113, 'GDE007', 'Desarr Politica Precios'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '113 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(114, 'GDE008', 'Desarr Politica Producto'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '114 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(115, 'GDE009', 'Desarr Politica Publicidad'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '115 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(116, 'GDE010', 'Desarr Procesos de RRHH'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '116 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(117, 'GDE020', 'Desarr Cadena Sumunistros'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '117 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(118, 'GDE024', 'Desarr Negocios'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '118 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(119, 'GDE026', 'Desarr Nuevos Mercados'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '119 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(120, 'GDE027', 'Desarr Nuevos Programas'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '120 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(121, 'GDE028', 'Desarr Politic de Descuent'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '121 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(122, 'GDE029', 'Desarr Politica Inventario'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '122 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(123, 'GDE031', 'Desarr Politicas Trade MKT'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '123 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(124, 'GDE032', 'Desarr Productos'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '124 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(125, 'GDE033', 'Desarr Sistema Almacenam'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '125 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(126, 'GGE005', 'Gestion Estrategia Ccial'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '126 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(127, 'GGE007', 'Gestion Politica Seguridad'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '127 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(128, 'GPL005', 'Planemiento Financiero'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '128 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(129, 'GRE004', 'Respons Circuitos Producc'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '129 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(130, 'GRE005', 'Respons Comercial Productos'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '130 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(131, 'GRE006', 'Respons Ctrol Presupuestar'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '131 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(132, 'GRE007', 'Respons Fuerza de Ventas'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '132 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(133, 'GRE008', 'Respons Generar de Negocios'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '133 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(134, 'GRE009', 'Respons Insumos a Sectores'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '134 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(135, 'GRE011', 'Respons Politica Ccial Cia'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '135 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(136, 'GRE012', 'Respons Presupuesto Producc'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '136 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(137, 'GRE013', 'Respons Procesos Producc'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '137 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(138, 'GRE014', 'Respons Seleccion Productos'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '138 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(139, 'GRE015', 'Respons Sist y Comunicacion'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '139 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(140, 'IAB001', 'Abastecimiento a Sectores'                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '140 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(141, 'IAB005', 'Abertura Madera'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '141 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(142, 'IAB006', 'Abertura Metalica'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '142 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(143, 'IAC003', 'Acero Inoxidable'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '143 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(144, 'IAL005', 'Albañileria'                                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '144 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(145, 'IAR003', 'Argon'                                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '145 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(146, 'IAR008', 'Armado de Andamios'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '146 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(147, 'IAS005', 'Ascensores'                                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '147 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(148, 'IAU003', 'Autoelev Electricos'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '148 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(149, 'IAU005', 'Autoelev Gasoleros'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '149 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(150, 'IAU007', 'Autoelev Nafteros'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '150 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(151, 'IAU009', 'Autoelev Uña Frontal'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '151 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(152, 'IAU012', 'Autoelev Uña Transv'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '152 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(153, 'IAU014', 'Autoelevador'                                   ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '153 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(154, 'IAU017', 'Autogena'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '154 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(155, 'IBA002', 'Barnizadora'                                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '155 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(156, 'IBA003', 'Balancin Automatico'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '156 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(157, 'IBA004', 'Balancin Matriz Embutida'                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '157 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(158, 'IBA005', 'Balancin Matriz Plana'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '158 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(159, 'IBA006', 'Balancin Pedal'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '159 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(160, 'ICA001', 'Carnet de 1ra'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '160 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(161, 'ICA002', 'Carnet de 2da'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '161 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(162, 'ICA003', 'Carga y Desc Bultos - 50 Kg'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '162 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(163, 'ICA004', 'Carga y Desc Bultos + 50 Kg'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '163 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(164, 'ICA005', 'Carga y Desc Containers'                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '164 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(165, 'ICA006', 'Carga y Desc Estibaje'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '165 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(166, 'ICE001', 'Cerrajeria'                                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '166 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(167, 'ICH003', 'Chapista de Taller'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '167 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(168, 'ICH004', 'Chapista Industria Automotr'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '168 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(169, 'ICH006', 'Chofer de Presidencia'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '169 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(170, 'ICN003', 'CNC'                                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '170 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(171, 'ICO001', 'Coladora'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '171 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(172, 'ICO002', 'Contrapisos'                                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '172 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(173, 'ICO003', 'CO2'                                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '173 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(174, 'ICO008', 'Colocacion Azulejos Ceramic'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '174 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(175, 'ICO009', 'Colocacion Moldes'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '175 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(176, 'ICO010', 'Colocacion de Vidrios'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '176 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(177, 'ICO039', 'Cortador'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '177 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(178, 'ICO047', 'Cosedor con Hilo'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '178 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(179, 'IDE003', 'Desagues Cloacales Sanitar'                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '179 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(180, 'IDE004', 'Despostador'                                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '180 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(181, 'IDI003', 'Distribucion Correspondencia'                   ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '181 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(182, 'IDO003', 'Dosificacion Prod Quimicos'                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '182 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(183, 'IEL005', 'Electricidad'                                   ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '183 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(184, 'IEM005', 'Empaquetadoras'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '184 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(185, 'IEM008', 'Empaque'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '185 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(186, 'IEN003', 'Encimado Manual'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '186 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(187, 'IEN004', 'Encimado con Maquina'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '187 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(188, 'IEN007', 'Encuadernacion'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '188 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(189, 'IEN010', 'Ensambles'                                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '189 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(190, 'IEX004', 'Experiencia en Saca Bollos'                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '190 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(191, 'IEX009', 'Extrusion Film Polietileno'                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '191 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(192, 'IEX010', 'Extrusion PVC'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '192 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(193, 'IFO003', 'Fotocromia'                                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '193 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(194, 'IFR003', 'Fraccionamiento'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '194 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(195, 'IGA004', 'Garlopa'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '195 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(196, 'IGR004', 'Gruas Automaticas'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '196 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(197, 'IGR008', 'Gruas Manuales'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '197 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(198, 'IIM003', 'Impresor Flexografico'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '198 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(199, 'IIN003', 'Instalac Agua Fria Caliente'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '199 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(200, 'IIN004', 'Instalac Maquinas Inyectora'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '200 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(201, 'IIN010', 'Inventario'                                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '201 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(202, 'IIN015', 'Inyectoras Polietileno'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '202 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(203, 'IIN016', 'Inyectoras Polipropileno'                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '203 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(204, 'IIN017', 'Inyectoras PVC'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '204 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(205, 'ILA001', 'Laqueados'                                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '205 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(206, 'ILA003', 'Laminado Aluminio'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '206 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(207, 'ILA004', 'Laminado Madera'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '207 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(208, 'ILA005', 'Laminado Plastico'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '208 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(209, 'ILI001', 'Lijadora de Contacto'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '209 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(210, 'ILI003', 'Lijadora'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '210 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(211, 'ILI008', 'Limpieza de Edificios'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '211 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(212, 'ILI009', 'Limpieza de Oficinas'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '212 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(213, 'ILI010', 'Limpieza de Planta'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '213 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(214, 'ILI015', 'Linea Producc Laboratorio'                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '214 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(215, 'ILI016', 'Linea Producc Metalurgica'                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '215 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(216, 'IMA003', 'Machimbradora'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '216 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(217, 'IMA008', 'Maquina Automatica'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '217 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(218, 'IMA009', 'Maquina Blistera'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '218 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(219, 'IMA010', 'Maquina Bordadora'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '219 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(220, 'IMA011', 'Maquina Cepilladora'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '220 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(221, 'IMA012', 'Maquina Collareta'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '221 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(222, 'IMA013', 'Maquina Enconadora'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '222 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(223, 'IMA014', 'Maquina Corrugadora'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '223 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(224, 'IMA015', 'Maquina Fresadora'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '224 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(225, 'IMA016', 'Maquina Electrica'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '225 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(226, 'IMA017', 'Maquina Envasadora'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '226 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(227, 'IMA018', 'Maquina Fraccionados'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '227 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(228, 'IMA019', 'Maquina Ind Limpieza'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '228 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(229, 'IMA020', 'Maquina Limadora'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '229 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(230, 'IMA021', 'Maquina Moladora'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '230 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(231, 'IMA022', 'Maquina OFF Set'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '231 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(232, 'IMA023', 'Impresor'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '232 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(233, 'IMA024', 'Maquina Prensadora'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '233 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(234, 'IMA025', 'Maquina Punzonadora'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '234 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(235, 'IMA026', 'Maquina Racla'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '235 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(236, 'IMA027', 'Maquina Recta'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '236 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(237, 'IMA028', 'Maquina Tupi'                                   ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '237 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(238, 'IMA029', 'Maquina Zunchadora'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '238 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(239, 'IMA042', 'Manejo Automoviles'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '239 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(240, 'IMA044', 'Manejo Camion'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '240 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(241, 'IMA045', 'Manejo Camion Acoplado'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '241 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(242, 'IMA046', 'Manejo Carga Peligrosa'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '242 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(243, 'IMA063', 'Manejo Transporte Pasajeros'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '243 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(244, 'IMA064', 'Mantenimiento Edilicio'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '244 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(245, 'IME001', 'Mecanica'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '245 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(246, 'IME002', 'Mezcladora'                                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '246 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(247, 'IME003', 'Mecanica Automotor'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '247 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(248, 'IME004', 'Mecanica de Aviones'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '248 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(249, 'IMI004', 'Mig Mag'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '249 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(250, 'IMT003', 'Matriz de Corte'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '250 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(251, 'INP002', 'Nivel con Plomada'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '251 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(252, 'IOV004', 'Overlok'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '252 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(253, 'IPI001', 'Pintura'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '253 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(254, 'IPI003', 'Pintura de Paredes'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '254 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(255, 'IPI004', 'Pintura con Soplete'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '255 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(256, 'IPI005', 'Pintura en Cabina'                              ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '256 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(257, 'IPI006', 'Pintura Trabajo de Altura'                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '257 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(258, 'IPL003', 'Plomeria'                                       ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '258 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(259, 'IRE001', 'Repuestos'                                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '259 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(260, 'IRE003', 'Rebarbado de Piezas'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '260 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(261, 'IRE008', 'Revoque Fino y Grueso'                          ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '261 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(262, 'IRE013', 'Reparacion de Matrices'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '262 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(263, 'ISA004', 'Samping'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '263 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(264, 'ISE004', 'Seriografia'                                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '264 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(265, 'ISI004', 'Sierra'                                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '265 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(266, 'ISO004', 'Soldadura'                                      ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '266 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(267, 'ISO006', 'Soldadura Stick'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '267 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(268, 'ISO009', 'Soldadura Tig'                                  ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '268 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(269, 'ITA003', 'Tapador'                                        ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '269 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(270, 'ITA008', 'Tareas Manuales'                                ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '270 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(271, 'ITI003', 'Tizado'                                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '271 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(272, 'ITO003', 'Torno Automatico'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '272 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(273, 'ITO004', 'Torno Paralelo'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '273 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(274, 'ITO005', 'Torno Revolver'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '274 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(275, 'ITR003', 'Trab de Altura'                                 ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '275 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(276, 'ITR004', 'Trab de Exteriores'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '276 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(277, 'ITR005', 'Linea Empaquetadora'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '277 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(278, 'ITR006', 'Linea Envasadora'                               ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '278 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(279, 'ITR007', 'Linea Etiquetadora'                             ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '279 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(280, 'ITR008', 'Linea Producc Plastica'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '280 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(281, 'ITR009', 'Linea de Produccion'                            ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '281 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(282, 'ITR010', 'Linea Producc Fraccionadora'                    ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '282 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(283, 'ITR011', 'Linea Producc Mecanica'                         ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '283 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(284, 'IVI003', 'Vigilancia'                                     ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '284 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(285, 'IZO004', 'Zorras Electr o Manu'                           ,0,	1,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '285 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(286, 'MAN001', 'Analisis Inventarios'                           ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '286 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(287, 'MAN002', 'Analisis Desvios'                               ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '287 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(288, 'MAN003', 'Analisis Morosidad'                             ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '288 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(289, 'MAN004', 'Analisis Mejoras en Servic'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '289 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(290, 'MAN005', 'Analisis Orden Fabricacion'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '290 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(291, 'MAN006', 'Analisis Requerimien Compras'                   ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '291 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(292, 'MAN007', 'Analisis Resultados Producc'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '292 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(293, 'MAN008', 'Analisis Riesgo Crediticio'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '293 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(294, 'MAN009', 'Analisis Material para PCP'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '294 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(295, 'MAN010', 'Analisis y Descripc Puestos'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '295 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(296, 'MCA003', 'Capacitacion en Higiene'                        ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '296 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(297, 'MCA004', 'Capacitacion en Seguridad'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '297 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(298, 'MCO003', 'Control Cumpl Contratacion'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '298 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(299, 'MCO004', 'Coordinac Inventario Fisico'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '299 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(300, 'MCO005', 'Control Norma M Ambie'                          ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '300 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(301, 'MCO006', 'Coordinac Distrib Material'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '301 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(302, 'MCO007', 'Control Normas Higiene'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '302 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(303, 'MCO008', 'Coordinac Solicitud Compras'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '303 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(304, 'MCO009', 'Control Normas Seg'                             ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '304 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(305, 'MCO010', 'Coordinac Cotizaciones'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '305 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(306, 'MCO011', 'Control Impuestos'                              ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '306 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(307, 'MCO012', 'Coordinac Cobranzas'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '307 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(308, 'MCO013', 'Confeccion de Planos'                           ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '308 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(309, 'MCO014', 'Conduccion y Ctrol de Obras'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '309 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(310, 'MCO015', 'Coordinac Licitaciones'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '310 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(311, 'MCO016', 'Coordinac Manual Calidad'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '311 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(312, 'MCO017', 'Control Liquidacion Sueldos'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '312 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(313, 'MCO018', 'Control Pliegos Licitacion'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '313 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(314, 'MCO019', 'Control Manten Equipamiento'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '314 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(315, 'MCO020', 'Control Mantenimien Edilicio'                   ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '315 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(316, 'MCO021', 'Control Medios Pago Internac'                   ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '316 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(317, 'MCO022', 'Control Costo Exportaciones'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '317 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(318, 'MCO023', 'Control Ordenes de Compras'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '318 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(319, 'MCO024', 'Control Plazo Flete Interna'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '319 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(320, 'MCO025', 'Control Precio Mater Importac'                  ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '320 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(321, 'MDE003', 'Desarr Disposit para Mejora'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '321 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(322, 'MDE006', 'Desarr House Organ'                             ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '322 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(323, 'MDE009', 'Desarr Plan Capacitacion'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '323 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(324, 'MDE011', 'Desarr Plan Induccion'                          ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '324 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(325, 'MDE014', 'Desarr Plan Jovenes Prof'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '325 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(326, 'MDE016', 'Desarr Tableros de Control'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '326 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(327, 'MDI003', 'Diseño de Catalogos'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '327 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(328, 'MDI004', 'Diseño de Avisos'                               ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '328 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(329, 'MDI005', 'Diseño de Productos'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '329 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(330, 'MDI006', 'Diseño de Folletos'                             ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '330 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(331, 'MDO003', 'Documentacion de Obras'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '331 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(332, 'MEV003', 'Evaluac Contrat Vtas Intern'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '332 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(333, 'MEV004', 'Evaluac Proveedores'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '333 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(334, 'MEV005', 'Evaluac Proyectos de Invers'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '334 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(335, 'MIN003', 'Informes de Gestion'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '335 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(336, 'MIN006', 'Informes de Inventario'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '336 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(337, 'MLE003', 'Legislacion Contratos'                          ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '337 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(338, 'MLE006', 'Legislacion Laboral'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '338 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(339, 'MLE009', 'Legislacion Poderes'                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '339 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(340, 'MMA003', 'Mantenim Sist de Calidad'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '340 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(341, 'MMA006', 'Maquetas'                                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '341 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(342, 'MMR004', 'MRP'                                            ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '342 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(343, 'MNU003', 'Nutricion'                                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '343 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(344, 'MPR002', 'Presupuesto Cobranzas'                          ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '344 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(345, 'MPR003', 'Programacion de la Producc'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '345 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(346, 'MPR004', 'Presupuesto Costo Personal'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '346 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(347, 'MPR008', 'Proyecc Result y su Impacto'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '347 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(348, 'MRE001', 'Respons Analisis de Ctas'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '348 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(349, 'MRE002', 'Respons Calculo de Costo'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '349 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(350, 'MRE003', 'Respons Devoluciones'                           ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '350 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(351, 'MRE004', 'Respons Juicio Comercial'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '351 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(352, 'MRE005', 'Respons Mantenim Maquinas'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '352 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(353, 'MRE006', 'Respons Pago a Proveedores'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '353 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(354, 'MRE007', 'Respons Sistema de Calidad'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '354 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(355, 'MRE008', 'Respons Stock Materiales'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '355 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(356, 'MRE009', 'Respons y Ctrol Inventario'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '356 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(357, 'MRE010', 'Respons Arqueo de Valores'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '357 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(358, 'MRE011', 'Respons Atenc Audit Externa'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '358 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(359, 'MRE012', 'Respons Auditorias Calidad'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '359 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(360, 'MRE013', 'Respons Compra Insumos'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '360 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(361, 'MRE014', 'Respons Compra Materiales'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '361 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(362, 'MRE015', 'Respons Costo de Producto'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '362 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(363, 'MRE016', 'Respons Aseguram Calidad'                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '363 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(364, 'MRE017', 'Respons Despacho Productos'                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '364 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(365, 'MRE018', 'Respons Fijacion Creditos'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '365 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(366, 'MRE019', 'Respons Gestion Repuestos'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '366 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(367, 'MRE020', 'Respons Liberacion de Pagos'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '367 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(368, 'MRE021', 'Respons Mejoras a Equipos'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '368 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(369, 'MRE022', 'Respons Montaje Instalacion'                    ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '369 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(370, 'MRE023', 'Respons Pricing'                                ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '370 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(371, 'MRE024', 'Respons Rentabilidad'                           ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '371 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(372, 'OAT003', 'Atención Pasajeros Vuelos'                      ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '372 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(373, 'OAV003', 'Aviación'                                       ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '373 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(374, 'OEX003', 'Experiencia en Altamar'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '374 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(375, 'OEX004', 'Experiencia en Dragado'                         ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '375 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(376, 'OEX005', 'Experiencia en Rio'                             ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '376 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(377, 'OEX006', 'Experiencia en Ultramar'                        ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '377 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(378, 'OEX007', 'Experiencia Bibliotecas'                        ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '378 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(379, 'OME003', 'Medicina Laboral'                               ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '379 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(380, 'OPO004', 'Portavalor'                                     ,0,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '380 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(381, 'PAN003', 'Animacion'                                      ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '381 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(382, 'PBU004', 'Buffet Froid'                                   ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '382 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(383, 'PCH004', 'Changarin'                                      ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '383 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(384, 'PCO002', 'Cocina'                                         ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '384 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(385, 'PCO003', 'Cocina Caliente'                                ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '385 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(386, 'PCO004', 'Cocina Fria'                                    ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '386 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(387, 'PCO005', 'Cocina Internacional'                           ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '387 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(388, 'PCO006', 'Cocina Vegetariana'                             ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '388 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(389, 'PCO016', 'Cosmetica'                                      ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '389 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(390, 'PCO019', 'Coordinación de Mozos/Camarera'                 ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '390 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(391, 'PDE004', 'Degustaciones'                                  ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '391 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(392, 'PDI003', 'Diagrama de Gondola'                            ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '392 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(393, 'PDI004', 'Diagrama de Zona'                               ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '393 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(394, 'PEL003', 'Elab Comid Base Pastas'                         ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '394 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(395, 'PEL004', 'Elab Emparedados'                               ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '395 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(396, 'PEL005', 'Elab Platos Mariscos Pescad'                    ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '396 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(397, 'PEL006', 'Elab Saladitos'                                 ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '397 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(398, 'PEL007', 'Elab Sop Crem Omel Tortilla'                    ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '398 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(399, 'PEV003', 'Eventos'                                        ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '399 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(400, 'PIN003', 'Indumentaria Femenina'                          ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '400 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(401, 'PIN004', 'Indumentaria Masculina'                         ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '401 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(402, 'PIS004', 'Islas'                                          ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '402 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(403, 'PMA001', 'Material POP'                                   ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '403 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(404, 'PMA002', 'Mayoristas'                                     ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '404 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(405, 'PMA003', 'Manejo Locales Gastronom'                       ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '405 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(406, 'PMA008', 'Mantenimiento de Toilletes'                     ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '406 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(407, 'PMI004', 'Minorista'                                      ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '407 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(408, 'PMO003', 'Mozos Evento con Bandeja'                       ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '408 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(409, 'PMO004', 'Mozos'                                          ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '409 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(410, 'PMO005', 'Mozos Evento sin Bandeja'                       ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '410 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(411, 'PPA003', 'Parrilla'                                       ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '411 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(412, 'PPA005', 'Pasteleria'                                     ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '412 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(413, 'PPL004', 'Planilla Precio Porcentaje'                     ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '413 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(414, 'PPR004', 'Presentacion Catering'                          ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '414 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(415, 'PPR007', 'Promoventa'                                     ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '415 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(416, 'PPR010', 'Preparación de Tragos'                          ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '416 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(417, 'PPU004', 'Puntera de Gondola'                             ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '417 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(418, 'PRE002', 'Respons Hipermercado'                           ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '418 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(419, 'PRE004', 'Respons Mayorista'                              ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '419 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(420, 'PRE005', 'Respons Supermercado'                           ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '420 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(421, 'PRE006', 'Recupero de Valijas'                            ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '421 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(422, 'PRE008', 'Reposicion Bebidas'                             ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '422 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(423, 'PRE009', 'Reposicion Camping'                             ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '423 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(424, 'PRE010', 'Reposicion Almacen'                             ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '424 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(425, 'PRE011', 'Reposicion Bazar'                               ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '425 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(426, 'PRE012', 'Reposicion Blanco'                              ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '426 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(427, 'PRE013', 'Reposicion Carniceria'                          ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '427 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(428, 'PRE014', 'Reposicion Disqueria'                           ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '428 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(429, 'PRE015', 'Reposicion Feteado'                             ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '429 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(430, 'PRE016', 'Reposicion Frescos'                             ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '430 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(431, 'PRE017', 'Reposicion Jardineria'                          ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '431 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(432, 'PRE018', 'Reposicion Jugueteria'                          ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '432 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(433, 'PRE019', 'Reposicion Lacteos'                             ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '433 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(434, 'PRE020', 'Reposicion Perfum Limpiez'                      ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '434 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(435, 'PRE021', 'Reposicion Pescaderia'                          ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '435 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(436, 'PRE022', 'Reposicion Textil Zapateria'                    ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '436 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(437, 'PRE023', 'Reposicion Verduleria'                          ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '437 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(438, 'PRE024', 'Reposicion'                                     ,0,	0,	0,	1,	0,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '438 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(439, 'PRE030', 'Restaurant'                                     ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '439 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(440, 'PRO005', 'Roller'                                         ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '440 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(441, 'PTU003', 'Turismo Carretera'                              ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '441 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(442, 'PVI004', 'Via Publica'                                    ,0,	0,	0,	1,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '442 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(443, 'SAB004', 'ABAP'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '443 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(444, 'SAD004', 'ADA'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '444 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(445, 'SAI004', 'AIX'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '445 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(446, 'SAL004', 'Altamira'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '446 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(447, 'SAR004', 'Arc Server'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '447 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(448, 'SAS003', 'AS400'                                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '448 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(449, 'SAS006', 'ASP'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '449 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(450, 'SAS009', 'Assembler'                                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '450 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(451, 'SBA001', 'Baan'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '451 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(452, 'SBA002', 'Backup Exec'                                    ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '452 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(453, 'SBA003', 'Backoffice Arc Server'                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '453 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(454, 'SBA004', 'Backoffice Backup Exec'                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '454 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(455, 'SBA005', 'Backoffice Dolar Universe'                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '455 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(456, 'SBA006', 'Backoffice HP Open View'                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '456 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(457, 'SBA007', 'Backoffice Legato'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '457 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(458, 'SBA008', 'Backoffice Lotus Notes'                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '458 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(459, 'SBA009', 'Backoffice Maestro'                             ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '459 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(460, 'SBA010', 'Backoffice MS Exchange'                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '460 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(461, 'SBA011', 'Backoffice MS SMS'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '461 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(462, 'SBA012', 'Backoffice Omniback'                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '462 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(463, 'SBA013', 'Backoffice Patrol'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '463 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(464, 'SBA018', 'Base Datos DB2'                                 ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '464 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(465, 'SBA019', 'Base Datos Informix'                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '465 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(466, 'SBA020', 'Base Datos MS Access'                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '466 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(467, 'SBA021', 'Base Datos MS SQL Server'                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '467 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(468, 'SBA022', 'Base Datos MYSQL'                               ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '468 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(469, 'SBA023', 'Base Datos ODBC'                                ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '469 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(470, 'SBA024', 'Base Datos Oracle'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '470 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(471, 'SBA025', 'Base Datos Posgre SQL'                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '471 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(472, 'SBD003', 'B D Sybase Anywhere'                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '472 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(473, 'SCC004', 'C++'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '473 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(474, 'SCL004', 'Clarify'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '474 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(475, 'SCO004', 'Cobol'                                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '475 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(476, 'SCR002', 'Script'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '476 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(477, 'SCR003', 'CRM Baan'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '477 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(478, 'SCR004', 'CRM Clarify'                                    ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '478 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(479, 'SCR005', 'CRM Lotus Brief Case'                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '479 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(480, 'SCR006', 'CRM Oracle'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '480 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(481, 'SCR007', 'CRM SAP'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '481 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(482, 'SCR008', 'CRM Siebel'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '482 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(483, 'SCR010', 'CRM Trilogy'                                    ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '483 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(484, 'SCR011', 'CRM Vantive'                                    ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '484 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(485, 'SDA003', 'Dataminig'                                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '485 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(486, 'SDB004', 'DB2'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '486 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(487, 'SDE004', 'Delphi'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '487 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(488, 'SDI005', 'Director'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '488 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(489, 'SDR004', 'Dream Weaver'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '489 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(490, 'SER003', 'ERP Altamira'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '490 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(491, 'SER004', 'ERP Baan'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '491 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(492, 'SER007', 'ERP Oracle'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '492 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(493, 'SER008', 'ERP People Soft'                                ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '493 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(494, 'SER009', 'ERP SAP'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '494 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(495, 'SER010', 'ERP Siebel'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '495 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(496, 'SFI004', 'Fire Work'                                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '496 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(497, 'SFL004', 'Flash'                                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '497 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(498, 'SFO004', 'Fortran'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '498 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(499, 'SHP004', 'HP Open View'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '499 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(500, 'SHP006', 'HP- UX'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '500 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(501, 'SIC003', 'E-Commerce'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '501 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(502, 'SIN004', 'Informix'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '502 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(503, 'SJA004', 'Java'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '503 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(504, 'SJA006', 'Java Script'                                    ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '504 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(505, 'SLI004', 'Linux'                                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '505 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(506, 'SLO004', 'Lotus Brief Case'                               ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '506 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(507, 'SMA004', 'Macros'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '507 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(508, 'SMA006', 'Maestro'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '508 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(509, 'SMA009', 'Mainframe'                                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '509 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(510, 'SMS004', 'MS Exchange'                                    ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '510 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(511, 'SMS006', 'MS SMS'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '511 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(512, 'SMS009', 'MS SQL Server'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '512 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(513, 'SMY004', 'MYSQL'                                          ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '513 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(514, 'SOB003', 'Object Oriented Programing'                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '514 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(515, 'SOD004', 'ODBC'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '515 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(516, 'SOM004', 'Omniback'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '516 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(517, 'SON004', 'Novel Netware'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '517 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(518, 'SOR004', 'Oracle'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '518 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(519, 'SOS004', 'OS 2'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '519 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(520, 'SOS006', 'OS 400'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '520 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(521, 'SPA004', 'Palm OS'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '521 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(522, 'SPA006', 'Pascal'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '522 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(523, 'SPA009', 'Patrol'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '523 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(524, 'SPC004', 'PC Compatibles'                                 ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '524 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(525, 'SPE004', 'Perl'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '525 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(526, 'SPH004', 'PHP'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '526 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(527, 'SPL003', 'Plataforma Mixta'                               ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '527 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(528, 'SPL004', 'Plataforma Unix'                                ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '528 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(529, 'SPL005', 'Plataforma Windows'                             ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '529 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(530, 'SPO004', 'Posgre SQL'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '530 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(531, 'SPO006', 'Power Builder'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '531 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(532, 'SPR004', 'Prolog'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '532 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(533, 'SRE003', 'Redes Cableado'                                 ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '533 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(534, 'SRE004', 'Redes Chat Servers'                             ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '534 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(535, 'SRE005', 'Redes Fibra Optica'                             ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '535 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(536, 'SRE006', 'Redes Firewall'                                 ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '536 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(537, 'SRE007', 'Redes Frame Relay'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '537 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(538, 'SRE008', 'Redes FTP Servers'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '538 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(539, 'SRE009', 'Redes Hubs & Swithes'                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '539 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(540, 'SRE010', 'Redes IPX SPX'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '540 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(541, 'SRE011', 'Redes Mail Servers'                             ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '541 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(542, 'SRE012', 'Redes Modems'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '542 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(543, 'SRE013', 'Redes Netbeui'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '543 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(544, 'SRE014', 'Redes Netbios'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '544 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(545, 'SRE015', 'Redes Proxy Servers'                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '545 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(546, 'SRE016', 'Redes Radio Enlaces'                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '546 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(547, 'SRE017', 'Redes Routers'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '547 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(548, 'SRE018', 'Redes SNA IBM'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '548 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(549, 'SRE019', 'Redes TCP IP'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '549 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(550, 'SRE020', 'Redes Telefonia'                                ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '550 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(551, 'SRE021', 'Redes Voice P'                                  ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '551 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(552, 'SRE022', 'Redes VPN'                                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '552 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(553, 'SRE023', 'Redes VSAT'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '553 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(554, 'SRE024', 'Redes Wap'                                      ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '554 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(555, 'SRE025', 'Redes Web Servers'                              ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '555 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(556, 'SRE026', 'Redes X 25 X 400'                               ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '556 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(557, 'SRE027', 'Reparacion de PC'                               ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '557 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(558, 'SSA004', 'SAP'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '558 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(559, 'SSE003', 'Seguridad en informßtica'                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '559 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(560, 'SSE004', 'Servicio de Housing'                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '560 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(561, 'SSI004', 'Siebel'                                         ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '561 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(562, 'SSM004', 'Small Talk'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '562 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(563, 'SSO003', 'Solaris'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '563 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(564, 'SSQ004', 'SQL'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '564 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(565, 'STO004', 'Toolbook'                                       ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '565 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(566, 'STR004', 'Trilogy'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '566 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(567, 'SUN004', 'Unix'                                           ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '567 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(568, 'SVA004', 'Vantive'                                        ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '568 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(569, 'SVI004', 'Visual Basic'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '569 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(570, 'SWI004', 'Windows 2000'                                   ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '570 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(571, 'SWI006', 'Windows 3X'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '571 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(572, 'SWI009', 'Windows 9X'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '572 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(573, 'SWI012', 'Windows ME'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '573 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(574, 'SWI015', 'Windows NT'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '574 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(575, 'SWI017', 'Windows XP'                                     ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '575 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(576, 'SXM004', 'XML'                                            ,0,	0,	1,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '576 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(577, 'TAB003', 'Abastecimiento Herramientas'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '577 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(578, 'TAN003', 'Analisis Agua'                                  ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '578 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(579, 'TAN004', 'Analisis Materia Prima'                         ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '579 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(580, 'TAN005', 'Analisis Product Terminados'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '580 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(581, 'TAN006', 'Analisis Detecc Fallas PLC'                     ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '581 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(582, 'TAU003', 'Automatizacion Industrial'                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '582 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(583, 'TBI003', 'Biotecnologia'                                  ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '583 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(584, 'TCA003', 'Calefaccion'                                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '584 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(585, 'TCI003', 'Circuitos Analogicos'                           ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '585 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(586, 'TCI004', 'Circuitos Digitales'                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '586 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(587, 'TCO003', 'Convertidor de Frecuencia'                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '587 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(588, 'TCU003', 'Curtidor'                                       ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '588 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(589, 'TEL004', 'Electroerosion'                                 ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '589 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(590, 'TES004', 'Esterilizacion'                                 ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '590 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(591, 'TGU003', 'Guillotinas Manuales'                           ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '591 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(592, 'TGU004', 'Guillotinas Automaticas'                        ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '592 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(593, 'TIN004', 'Instalacion Trifasica'                          ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '593 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(594, 'TLA004', 'Layout'                                         ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '594 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(595, 'TMI004', 'Microbiologia'                                  ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '595 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(596, 'TPI004', 'Piping'                                         ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '596 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(597, 'TPL003', 'Plegado de Plastico'                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '597 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(598, 'TPL004', 'Plegado de Metal'                               ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '598 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(599, 'TPR003', 'Programacion de PLC'                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '599 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(600, 'TRE003', 'Refrigeracion'                                  ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '600 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(601, 'TRE004', 'Rebajador'                                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '601 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(602, 'TRE008', 'Repar Aparat Alarm TV Video'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '602 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(603, 'TRO003', 'Robotica'                                       ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '603 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(604, 'TTA003', 'Tableros Alta Tension'                          ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '604 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(605, 'TTA004', 'Tableros Baja Tension'                          ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '605 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(606, 'TTA005', 'Tableros Media Tension'                         ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '606 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(607, 'TTR003', 'Trinchero'                                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '607 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(608, 'XAJ003', 'Ajuste de Matrices'                             ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '608 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(609, 'XBA002', 'Balanzas'                                       ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '609 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(610, 'XBO003', 'Bobinado de Motores'                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '610 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(611, 'XBO004', 'Bobinado de Potencia'                           ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '611 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(612, 'XBO005', 'Bobinado de Transformadores'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '612 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(613, 'XCA003', 'Cableado'                                       ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '613 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(614, 'XCA004', 'Cableado Aereo'                                 ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '614 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(615, 'XCA005', 'Cableado Tableros'                              ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '615 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(616, 'XCA006', 'Cableado Transf Baja Tens'                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '616 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(617, 'XCA007', 'Cableado Transf Media Tens'                     ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '617 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(618, 'XCA012', 'Calculos'                                       ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '618 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(619, 'XCA017', 'Cambio de Matrices'                             ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '619 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(620, 'XCA018', 'Cambio de Piezas Mecanicas'                     ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '620 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(621, 'XEN004', 'Envolvedoras'                                   ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '621 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(622, 'XET004', 'Etiquetadoras'                                  ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '622 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(623, 'XFA004', 'Fabricacion de Matrices'                        ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '623 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(624, 'XHI004', 'Hidraulica'                                     ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '624 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(625, 'XIN004', 'Interpretacion Planos'                          ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '625 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(626, 'XMA003', 'Manten Balancines'                              ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '626 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(627, 'XMA004', 'Manten Correct y Preventivo'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '627 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(628, 'XMA005', 'Manten Electri Linea Produc'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '628 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(629, 'XMA006', 'Manten Electrom Linea Produc'                   ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '629 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(630, 'XMA007', 'Manten Herramientas'                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '630 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(631, 'XMA008', 'Manten Linea Produc'                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '631 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(632, 'XMA009', 'Manten Maquinas Hidraulicas'                    ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '632 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(633, 'XMA010', 'Manten Maquinas Neumaticas'                     ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '633 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(634, 'XMA011', 'Manten Mecan Linea Produc'                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '634 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(635, 'XMA012', 'Manten Predictivo'                              ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '635 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(636, 'XMA020', 'Maq y Herramienta'                              ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '636 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(637, 'XMO003', 'Montaje'                                        ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '637 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(638, 'XMO004', 'Montaje de obra'                                ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '638 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(639, 'XNE004', 'Neumatica'                                      ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '639 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(640, 'XPL004', 'PLC'                                            ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '640 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(641, 'XRE003', 'Rectificado de Motores'                         ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '641 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(642, 'XTI004', 'Tirado de Cables'                               ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '642 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(643, 'XTR004', 'Transformadores'                                ,0,	0,	0,	0,	1,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '643 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(644, 'YAC002', 'Accidentes de Trabajo'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '644 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(645, 'YAC003', 'Actualizacion Libros Cont'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '645 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(646, 'YAC004', 'Actualizacion Registro Cont'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '646 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(647, 'YAC014', 'Acuerdo Automotriz'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '647 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(648, 'YAC015', 'Acuerdos ante el Ministerio'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '648 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(649, 'YAD003', 'Administracion Personal'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '649 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(650, 'YAD007', 'Administracion Test Habilid'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '650 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(651, 'YAD008', 'Administracion ART'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '651 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(652, 'YAD009', 'Administracion de Deposito'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '652 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(653, 'YAN003', 'Analisis de Contratos'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '653 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(654, 'YAN004', 'Analisis de Costos'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '654 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(655, 'YAN005', 'Analisis de Ctas'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '655 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(656, 'YAN006', 'Analisis de Proveedores'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '656 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(657, 'YAN007', 'Analisis de Inventarios'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '657 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(658, 'YAN008', 'Analisis de Mercado'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '658 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(659, 'YAN009', 'Analisis de Remuneraciones'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '659 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(660, 'YAN010', 'Analisis de Resultados'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '660 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(661, 'YAN011', 'Analisis de Seguros'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '661 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(662, 'YAN012', 'Analisis Econom Financiero'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '662 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(663, 'YAN013', 'Analisis Rentabilidad'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '663 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(664, 'YBI003', 'Bilingüe'                                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '664 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(665, 'YCO002', 'Control Caja y Fondo Fijo'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '665 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(666, 'YCO004', 'Control Compras al Exterior'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '666 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(667, 'YCO006', 'Control Despachos Impo Expo'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '667 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(668, 'YCO008', 'Control Entrega Materiales'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '668 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(669, 'YCO010', 'Control Entrega Productos'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '669 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(670, 'YCO012', 'Control Limpieza'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '670 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(671, 'YCO014', 'Control Servicio Cafeteria'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '671 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(672, 'YCO016', 'Control Servicio de Comedor'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '672 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(673, 'YCO018', 'Control Tesoreria Impuestos'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '673 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(674, 'YCO020', 'Control Vigilancia'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '674 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(675, 'YCO022', 'Compras al Exterior'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '675 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(676, 'YCO024', 'Compras Materias Primas'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '676 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(677, 'YCO026', 'Compras Productivas'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '677 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(678, 'YCO028', 'Compras Tecnicas'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '678 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(679, 'YCO030', 'Concil Ctas Patrimoniales'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '679 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(680, 'YCO032', 'Concil y Analisis Ctas Ctes'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '680 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(681, 'YCO034', 'Concil Bancarias'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '681 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(682, 'YCO036', 'Confeccion de Avisos'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '682 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(683, 'YCO038', 'Confeccion Reporte Contable'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '683 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(684, 'YCO040', 'Confeccion Estadisticas'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '684 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(685, 'YCO042', 'Control de Costos'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '685 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(686, 'YCO043', 'Convenio Bancario'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '686 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(687, 'YCO044', 'Convenio Empl de Comercio'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '687 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(688, 'YCO045', 'Convenio Gastronomicos'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '688 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(689, 'YCO046', 'Convenio Graficos'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '689 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(690, 'YCO047', 'Convenio Indust Alimenticia'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '690 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(691, 'YCO048', 'Convenio Indust del Caucho'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '691 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(692, 'YCO049', 'Convenio Maderero'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '692 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(693, 'YCO050', 'Convenio Metalurgico'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '693 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(694, 'YCO051', 'Convenio Plasticos'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '694 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(695, 'YCO066', 'Costos de Produccion'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '695 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(696, 'YCO067', 'Costos de Vtas'                                 ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '696 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(697, 'YCO068', 'Costos Mercaderia Vendida'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '697 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(698, 'YCO084', 'Compras de Materiales'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '698 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(699, 'YCO085', 'Compras Improductivas'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '699 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(700, 'YCO093', 'Confec Indicador de Calidad'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '700 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(701, 'YCO100', 'Coordinac Embarque Terrestre'                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '701 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(702, 'YCO101', 'Coordinac Ingreso Materiales'                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '702 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(703, 'YCO102', 'Coordinac Embarque Maritimo'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '703 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(704, 'YCO103', 'Coordinac Embarques Aereos'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '704 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(705, 'YCO111', 'Confeccion Cash Flows'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '705 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(706, 'YCO119', 'Cotizac Impo Expo Aerea'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '706 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(707, 'YCO120', 'Cotizac Impo Expo Maritima'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '707 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(708, 'YDE004', 'Desarr Plan Comunicacion'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '708 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(709, 'YEV004', 'Evaluac Desempe±o'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '709 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(710, 'YEV006', 'Evaluac Potencial'                              ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '710 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(711, 'YIN003', 'Induccion'                                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '711 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(712, 'YIN011', 'Informe Gastos'                                 ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '712 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(713, 'YIN012', 'Informe Ventas'                                 ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '713 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(714, 'YIN020', 'Ingresos Brutos'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '714 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(715, 'YIN027', 'Investigacion de Mercado'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '715 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(716, 'YIV004', 'IVA'                                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '716 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(717, 'YLI003', 'Liquidacion Impuestos'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '717 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(718, 'YLI004', 'Licitaciones'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '718 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(719, 'YLI006', 'Liquidacion 4ta Categoria'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '719 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(720, 'YNE003', 'Negociacion Condic de Pago'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '720 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(721, 'YPL003', 'Planificac Entrega a Cltes'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '721 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(722, 'YPL005', 'Plan de Carrera'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '722 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(723, 'YPR003', 'Presentacion ante DGI e IGJ'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '723 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(724, 'YPS004', 'Psicotecnicos'                                  ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '724 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(725, 'YRE004', 'Respons Armado Hoja Ruta'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '725 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(726, 'YRE006', 'Respons Entrega Material'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '726 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(727, 'YRE009', 'Relacion con Despach Aduana'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '727 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(728, 'YRE011', 'Relevamiento de Perfiles'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '728 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(729, 'YSA004', 'Sanciones Disciplinarias'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '729 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(730, 'YTR003', 'Trato con Delegados'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '730 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(731, 'ZAN003', 'Analisis Plan Fabricacion'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '731 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(732, 'ZAN004', 'Analisis Proceso de Producc'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '732 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(733, 'ZAN005', 'Analisis Pto de Equilibrio'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '733 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(734, 'ZCO003', 'Confeccion Presupuesto RRHH'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '734 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(735, 'ZCO004', 'Confeccion Balance'                             ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '735 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(736, 'ZCO005', 'Confeccion Cuadro Reemplazo'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '736 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(737, 'ZCO013', 'Control Creditos Cobranzas'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '737 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(738, 'ZCO015', 'Control de Facturacion'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '738 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(739, 'ZCO016', 'Control Proceso Productivo'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '739 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(740, 'ZCO017', 'Control Estrateg Comercial'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '740 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(741, 'ZCO018', 'Control Balance General'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '741 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(742, 'ZCO019', 'Control Presupues Marketing'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '742 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(743, 'ZCO021', 'Control Redes Informaticas'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '743 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(744, 'ZCO027', 'Coordinac Entrevista Egreso'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '744 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(745, 'ZCO028', 'Coordinac Incorpor Personal'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '745 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(746, 'ZCO029', 'Coordinac Segurid e Higiene'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '746 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(747, 'ZCO030', 'Coordinac Svcio Medico'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '747 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(748, 'ZCO031', 'Coordinac Implemen Software'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '748 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(749, 'ZDE001', 'Desarr Estructura Salarial'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '749 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(750, 'ZDE002', 'Desarr Nuevas Tecnologias'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '750 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(751, 'ZDE003', 'Desarr Proceso Productivo'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '751 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(752, 'ZDE008', 'Desarr de Instructivos'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '752 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(753, 'ZDE009', 'Desarr de Procedimientos'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '753 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(754, 'ZDE010', 'Desarr de Productos'                            ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '754 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(755, 'ZDE011', 'Desarr Nuevos Canales'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '755 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(756, 'ZDE012', 'Desarr Nuevos Clientes'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '756 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(757, 'ZDE013', 'Desarr Nuevos Segmentos'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '757 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(758, 'ZDE014', 'Desarr Plan Manten Correcti'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '758 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(759, 'ZDE015', 'Desarr Plan Manten Preventi'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '759 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(760, 'ZDE016', 'Desarr Plan Seguridad'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '760 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(761, 'ZDE017', 'Desarr Politic de Calidad'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '761 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(762, 'ZDE018', 'Desarr Soluciones Usuarios'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '762 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(763, 'ZDE025', 'Deteccion Necesidades Capac'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '763 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(764, 'ZEV003', 'Evaluacion Clima Organizac'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '764 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(765, 'ZEV004', 'Evaluacion Creditos Clientes'                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '765 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(766, 'ZEV005', 'Evaluacion Nuevos Negocios'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '766 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(767, 'ZGE004', 'Gestion Politica Back Up'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '767 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(768, 'ZIN003', 'Informes de Gestion RRHH'                       ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '768 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(769, 'ZIN011', 'Instalacion de Software'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '769 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(770, 'ZIN019', 'Instrumentac Nvas Tecnologi'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '770 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(771, 'ZLA004', 'Lanzamiento Productos'                          ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '771 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(772, 'ZMA003', 'Mantenimiento de Redes'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '772 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(773, 'ZNE003', 'Negociac con Distribuidores'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '773 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(774, 'ZNE004', 'Negociac con Grandes Cltes'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '774 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(775, 'ZNE005', 'Negociac Lineas de Financia'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '775 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(776, 'ZNE006', 'Negociac CCT'                                   ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '776 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(777, 'ZNE007', 'Negociac Operac Financieras'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '777 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(778, 'ZNE008', 'Negociac Plazo de Pagos'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '778 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(779, 'ZNE009', 'Negociac Quita de Intereses'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '779 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(780, 'ZPL003', 'Plan Beneficios'                                ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '780 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(781, 'ZPL004', 'Plan Mejora Continua'                           ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '781 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(782, 'ZPL005', 'Planes de Ventas'                               ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '782 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(783, 'ZPO004', 'Posicionamiento Marcas'                         ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '783 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(784, 'ZRE001', 'Respons Instalac Sistemas'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '784 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(785, 'ZRE002', 'Respons Relac con Proveedor'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '785 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(786, 'ZRE003', 'Resolucion Conflictos Lab'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '786 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(787, 'ZRE008', 'Respons Canales Comerciales'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '787 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(788, 'ZRE009', 'Respons Certificac Calidad'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '788 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(789, 'ZRE010', 'Respons Manten Linea Produc'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '789 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(790, 'ZRE011', 'Respons Evalua Proyec Inver'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '790 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(791, 'ZRE012', 'Respons Evaluar Lay Out'                        ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '791 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(792, 'ZRE013', 'Respons Fijac Pautas Financ'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '792 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(793, 'ZRE014', 'Respons Juicios Laborales'                      ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '793 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(794, 'ZRE015', 'Respons Lanzamien Productos'                    ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '794 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(795, 'ZRE016', 'Respons Optimizac Procesos'                     ,1,	0,	0,	0,	0,	0) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '795 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(796, 'CAD003', 'Adobe'                                          ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '796 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(797, 'CAT005', 'Atencion Clientes'                              ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '797 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(798, 'CBE003', 'Bejerman'                                       ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '798 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(799, 'CBI003', 'Bilingüe'                                       ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '799 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(800, 'CBS003', 'Bs As Soft'                                     ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '800 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(801, 'CCA003', 'Capacitacion de Personal'                       ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '801 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(802, 'CCO002', 'Conduccion de Personal'                         ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '802 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(803, 'CCO012', 'Control de Calidad'                             ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '803 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(804, 'CEX005', 'Excel'                                          ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '804 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(805, 'CGR005', 'Groupwise'                                      ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '805 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(806, 'CHT005', 'HTML'                                           ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '806 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(807, 'CIN001', 'Internet'                                       ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '807 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(808, 'CIN003', 'Ingles Avanzado'                                ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '808 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(809, 'CIN004', 'Ingles Basico'                                  ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '809 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(810, 'CIN005', 'Ingles Intermedio'                              ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '810 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(811, 'CJD003', 'Jdedwards'                                      ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '811 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(812, 'CLI003', 'Liderazgo de Grupos'                            ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '812 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(813, 'CLO005', 'Lotus Notes'                                    ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '813 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(814, 'CMA003', 'Maria'                                          ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '814 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(815, 'CMA005', 'Mac'                                            ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '815 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(816, 'CMS005', 'MS Access'                                      ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '816 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(817, 'CMS007', 'MS Project'                                     ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '817 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(818, 'CNE003', 'Negociador'                                     ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '818 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(819, 'CNO004', 'Normas ISO'                                     ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '819 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(820, 'COU005', 'Outlook'                                        ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '820 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(821, 'CPA003', 'Atención de consultas'                          ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '821 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(822, 'CPA004', 'Atención de reclamos'                           ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '822 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(823, 'CPC005', 'Pc'                                             ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '823 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(824, 'CPH005', 'Photoshop'                                      ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '824 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(825, 'CPO005', 'Power Point'                                    ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '825 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(826, 'CPS003', 'People Soft'                                    ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '826 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(827, 'CSE003', 'Servicio al Cliente'                            ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '827 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(828, 'CSP003', 'Seleccion de Personal'                          ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '828 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(829, 'CTA003', 'Tango'                                          ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '829 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(830, 'CTO003', 'Toma de Decisiones'                             ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '830 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(831, 'CWI005', 'Windows'                                        ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '831 ',@@error , ''
	insert into POS16_CONOCIMIENTOS (oi_conocimiento, c_conocimiento, d_conocimiento, l_administrativo, l_industrial, l_informatico, l_promotora, l_tecnico, l_merchandiser) VALUES(832, 'CWO005', 'Word'                                           ,1,	1,	1,	1,	1,	1) 
	 EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, '832 ',@@error , ''



END
ELSE
BEGIN
	EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'la tabla POS16_CONOCIMIENTOS no existe',@@error , ''
END

exec dbo.sp_NMD_VerifScript @n_Version , @ScripName, 1,@Result OUTPUT
EXEC dbo.sp_NMD_LogScript @oi_Version  ,@ScripName , @d_modulo, 'Finalizo la ejecucion',@Result , ''	

