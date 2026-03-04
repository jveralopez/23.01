set dateformat ymd

insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (38,'TTA','Fichadas','TolPreIng','Tolerancia PreIngreso (Minutos)','A',NULL,'60',1,'Indica la tolerancia en minutos a la que una fichada puede ser v&#225;lida. La diferencia con la tolerancia por tipo de hora es que &#233;sta es m&#225;s amplia porque debe abarcar las horas posibles de fichadas reales.');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (39,'TTA','Fichadas','MinMinutosLiq','M&#237;nima cantidad de minutos v&#225;lidos para la liquidaci&#243;n','A',NULL,'15',NULL,'Indica la m&#237;nima cantidad de minutos que deben pasar desde que una persona ficha la entrada hasta que ficha la salida para que se contabilicen las horas trabajadas. ');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (40,'TTA','Fichadas','MinMinutosHora','M&#237;nima cantidad de minutos v&#225;lidos para cada hora','A',NULL,'45',NULL,'M&#237;nima cantidad de minutos v&#225;lidos para cada hora.');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (41,'TTA','Fichadas','TipoHoraAus','Tipo Hora Ausencia','A',NULL,'5',NULL,'Tipo Hora Ausencia');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (42,'TTA','Fichadas','TipoHoraFueraHor','Tipo Hora Fuera Horario','A',NULL,'2',NULL,'Tipo Hora Fuera Horario');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (43,'TTA','Fichadas','TipoHoraAusPar','Tipo Hora Ausencia Parcial','A',NULL,'9',NULL,'Tipo Hora Ausencia Parcial');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (44,'TTA','Fichadas','RedondeoA','Redondeo de horas (excluyente con MinMinutosHora)','A',NULL,'15',NULL,'Redondeo de horas (excluyente con MinMinutosHora)');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (45,'TTA','Fichadas','MaxDifIngEgr','M&#225;xima diferencia entre una entrada y su salida expresada en minutos','A',NULL,'720',NULL,'M&#225;xima diferencia entre una entrada y su salida expresada en minutos');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (46,'TTA','Fichadas','DifEgrIng','Diferencia entre una salida y una entrada para que se tomen como anuladas exprada en minutos','A',NULL,'15',NULL,'Diferencia entre una salida y una entrada para que se tomen como anuladas exprada en minutos');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (47,'TTA','Fichadas','MinRange','Minima diferencia de marcada de dos fichadas seguidas del mismo tipo. Expreda en minutos','A',NULL,'15',NULL,'Minima diferencia de marcada de dos fichadas seguidas del mismo tipo. Expreda en minutos');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (48,'TTA','Esperanza','DiasGenEsperanza','Cantidad de d&#237;as a generar en el proceso de generacion de esperanza','A',NULL,'60',NULL,'Cantidad de d&#237;as a generar en el proceso de generacion de esperanza');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (49,'TTA','Fichadas','TolPosIng','Tolerancia PostIngreso (Minutos)','A',NULL,'540',NULL,'');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (50,'TTA','Fichadas','TolPreEgr','Tolerancia PreEgreso (Minutos)','A',NULL,'540',NULL,'');
insert into ORG26_PARAMETROS (oi_parametro,c_modulo,d_clase,c_parametro,d_parametro,d_tipo_parametro,e_longitud,d_valor,l_bloqueado,o_parametro) values (51,'TTA','Fichadas','TolPosEgr','Tolerancia PostEgreso (Minutos)','A',NULL,'180',NULL,NULL);


--BEGIN: TTA01_TIPOHORAS
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (1,'HoraNormal','Hora Normal Ordinaria',1.0,10,15,10,40,0,1,1,'4444',0,'',1);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (2,'HE','Hora Extra 100%',2.0,5,5,5,5,1,1,NULL,'5555',1,'',0);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (3,'HNT','Hora Nocturna',1.3,10,10,10,10,1,1,1,'8888',NULL,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (4,'HE80','Hora Extra 80%',1.8,5,5,5,5,1,1,0,'10024',NULL,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (5,'Ausencia','Aus',1.0,10,10,10,10,1,1,1,'5555',NULL,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (6,'AusJust','Ausencia Justificada',1.0,10,10,10,10,1,1,1,'777777',1,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (7,'DT','Descan. Trabajado',3.0,10,10,10,10,0,1,NULL,'',1,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (8,'DF','Domingo Feriados',1.5,NULL,NULL,NULL,NULL,NULL,1,1,'',1,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (9,'AusParcial','Ausencia Parcial',0,NULL,NULL,NULL,NULL,1,1,NULL,'',NULL,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (10,'HN Trabajada en HA','Hora normal trabajada en horario de almuerzo',1.0,0,0,0,0,NULL,1,1,'',NULL,'',0);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (11,'AusFall','Ausencia por fallecimiento de un familiar',1.0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'',NULL,'',NULL);
insert into TTA01_TIPOHORAS (oi_tipohora,c_tipohora,d_tipohora,n_Coeficiente,e_TolPreIng,e_TolPosIng,e_TolPreEgr,e_TolPosEgr,l_afectanov,l_sumariza,l_controla_fichada,c_liquidador,l_requiereaut,o_tipohora,l_sumariza_aus) values (12,'AusExa','Ausencia por examen',1.0,15,15,15,15,1,0,1,'',NULL,'',1);
--END: TTA01_TIPOHORAS

--BEGIN: TTA02_ESCUADRAS
--END: TTA02_ESCUADRAS

--BEGIN: TTA02_HORARIOS
--END: TTA02_HORARIOS

--BEGIN: TTA02_HORARIOSDET
--END: TTA02_HORARIOSDET

--BEGIN: TTA03_ESPERANZA
--END: TTA03_ESPERANZA

--BEGIN: TTA04_CAMBIOSTURNO
--END: TTA04_CAMBIOSTURNO

--BEGIN: TTA04_HORARIOSPERS
--END: TTA04_HORARIOSPERS

--BEGIN: TTA04_NOVEDADES
--END: TTA04_NOVEDADES

--BEGIN: TTA04_PERSONAL
--END: TTA04_PERSONAL

--BEGIN: TTA04_TIPOSHORAAUT
--END: TTA04_TIPOSHORAAUT

--BEGIN: TTA05_TERMINALES
--END: TTA05_TERMINALES

--BEGIN: TTA06_ARCHIVOS
insert into TTA06_ARCHIVOS (oi_archivo,c_archivo,d_archivo,d_tipoarchivo,ca_separador,e_cantcol,c_formato) values (1,'1','regist.dat','DC',';',6,'SRF');
--END: TTA06_ARCHIVOS

--BEGIN: TTA06_ARCHIVOSDET
insert into TTA06_ARCHIVOSDET (oi_archivosdet,oi_archivo,e_poscol,e_anchocol,d_nomcolum) values (1,1,0,2,'sec');
insert into TTA06_ARCHIVOSDET (oi_archivosdet,oi_archivo,e_poscol,e_anchocol,d_nomcolum) values (2,1,3,5,'leg');
insert into TTA06_ARCHIVOSDET (oi_archivosdet,oi_archivo,e_poscol,e_anchocol,d_nomcolum) values (3,1,9,8,'tar');
insert into TTA06_ARCHIVOSDET (oi_archivosdet,oi_archivo,e_poscol,e_anchocol,d_nomcolum) values (4,1,18,10,'fec');
insert into TTA06_ARCHIVOSDET (oi_archivosdet,oi_archivo,e_poscol,e_anchocol,d_nomcolum) values (5,1,29,5,'hor');
insert into TTA06_ARCHIVOSDET (oi_archivosdet,oi_archivo,e_poscol,e_anchocol,d_nomcolum) values (6,1,35,1,'es');
--END: TTA06_ARCHIVOSDET

--BEGIN: TTA07_FICHADASING
--END: TTA07_FICHADASING

--BEGIN: TTA08_FICHADAS
--END: TTA08_FICHADAS

--BEGIN: TTA09_LIQUIDACION
--END: TTA09_LIQUIDACION

--BEGIN: TTA10_LIQUIDACJOR
--END: TTA10_LIQUIDACJOR

--BEGIN: TTA10_LIQUIDACPERS
--END: TTA10_LIQUIDACPERS

--BEGIN: TTA10_LIQUIDACPROC
--END: TTA10_LIQUIDACPROC

--BEGIN: TTA11_ESPERANZAPER
--END: TTA11_ESPERANZAPER

--BEGIN: TTA12_MOV_VISITA
--END: TTA12_MOV_VISITA

--BEGIN: TTA12_VISITAS
--END: TTA12_VISITAS



