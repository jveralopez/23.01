using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Liquidacion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Liquidaciones
    public partial class LIQUIDACION
    {
        public static string GuardarLiquidacion (Nomad.NSystem.Proxy.NomadXML pobjParametros)
        {
            pobjParametros = pobjParametros.FirstChild();
            LIQUIDACION liq = new LIQUIDACION();
            try
            {
                NomadTransaction trans = NomadEnvironment.GetCurrentTransaction();
                trans.Begin();

                liq.l_ajuste = pobjParametros.GetAttrBool("l_ajuste");
                liq.l_debug = pobjParametros.GetAttrBool("l_debug");
                liq.l_simulacion = pobjParametros.GetAttrBool("l_simulacion");
                liq.l_retroactivo = pobjParametros.GetAttrBool("l_retroactivo");
                liq.c_estado = pobjParametros.GetAttr("c_estado");
                liq.l_debug = pobjParametros.GetAttrBool("l_debug");
                liq.descr = pobjParametros.GetAttr("desc");
                liq.c_clase = pobjParametros.GetAttr("c_clase");
                liq.c_liquidacion = pobjParametros.GetAttr("c_liquidacion");
                liq.d_titulo = pobjParametros.GetAttr("d_titulo");
                liq.l_confidencial = pobjParametros.GetAttrBool("l_confidencial");
                liq.e_periodo = pobjParametros.GetAttrInt("e_periodo");
                liq.f_liquidacion = pobjParametros.GetAttrDateTime("f_liquidacion");
                liq.e_quincena = pobjParametros.GetAttrInt("e_quincena");
                liq.f_pago = pobjParametros.GetAttrDateTime("f_pago");
                liq.c_mes_deposito = pobjParametros.GetAttr("c_mes_deposito");
                liq.f_deposito = pobjParametros.GetAttrDateTime("f_deposito");
                liq.f_f649 = pobjParametros.GetAttrDateTime("f_f649");
                liq.f_cierre = pobjParametros.GetAttrDateTime("f_cierre");
                liq.o_liquidacion = pobjParametros.GetAttr("o_liquidacion");
                liq.t_leyenda = pobjParametros.GetAttr("t_leyenda");
                liq.oi_empresa = new NomadXML(pobjParametros.GetElements("oi_empresa")[0].ToString()).FirstChild().GetAttr("value");
                liq.oi_forma_pago = new NomadXML(pobjParametros.GetElements("oi_forma_pago")[0].ToString()).FirstChild().GetAttr("value");
                liq.oi_tipo_liq = new NomadXML(pobjParametros.GetElements("oi_tipo_liq")[0].ToString()).FirstChild().GetAttr("value");
                liq.oi_banco = new NomadXML(pobjParametros.GetElements("oi_banco")[0].ToString()).FirstChild().GetAttr("value");

                if (pobjParametros.GetAttr("e_quincena") == "")
                    liq.e_quincenaNull = true;
                if (pobjParametros.GetAttr("f_cierre") == "")
                    liq.f_cierreNull = true;
                if (pobjParametros.GetAttr("f_deposito") == "")
                    liq.f_depositoNull = true;
                if (pobjParametros.GetAttr("f_f649") == "")
                    liq.f_f649Null = true;

                trans.SaveRefresh(liq);
                trans.Commit();

                trans.Begin();
                if (liq.l_confidencial)
                {
                    liq.Security.Policy = "./ROLE/ADMINCONF";
                }

               trans.SaveRefresh(liq);

               trans.Commit();

            }
            catch (Exception ex)
            {
                if (ex.Message == "DB.SQLSERVER.2627" || ex.Message == "DB.ORA.1")
                    throw NomadException.NewMessage("Liquidacion.VALIDACIONABM.ERR-CODIGODUPLICADO");
                else
                    throw ex;
            }

            return liq.id.ToString();
        }

        public static void CargarVariablesNovedades(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaNovedadLiquidacion(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void InterfaceSalida(string interfaceTIPO, string interfaceID, Nomad.NSystem.Document.NmdXmlDocument parametros)
        {
            Nomad.Base.Report.GeneralReport.REPORT.Generico(interfaceTIPO, interfaceID, parametros);
        }
		
		public static void InterfaceSalidaJson(string interfaceTIPO, string interfaceID, Nomad.NSystem.Document.NmdXmlDocument xmlParametros) {
			NomadXML xmlResult;
			Dictionary<string, object> dicResultado;
			string strFileName, strQueryName, strResultName;
			string strTMP;
			
			//Crea los nombres de los archivos final y de consulta
			strFileName = NomadProxy.GetProxy().RunPath + "\\NOMAD\\INTERFACES\\" + NomadProxy.GetProxy().Batch().ID;
			strQueryName = strFileName + ".Query.xml";
			strResultName = strFileName + ".Result.xml";
			strFileName = strFileName + ".txt";
			
			
			NomadBatch MyBatch=NomadBatch.GetBatch("Generico","Generico");
			MyBatch.Log("Comienza la interfaz de salida.");

			MyBatch.Log("Cargando la Tipo de Interfaz.");
			MyBatch.SetMess("Cargando la Tipo de Interfaz");
			NomadXML xmlInterfaceDef = NomadProxy.GetProxy().FileServiceIO().LoadFileXML("INTERFACE-DEF", interfaceTIPO+".def.xml");
			NomadXML interfaceOBJ = xmlInterfaceDef.FindElement2("INTERFACE", "code", interfaceID);
			if (interfaceOBJ==null)
				throw new Exception("Interface '"+interfaceID+"' no encontrada...");
			
			MyBatch.SetPro(5);

			MyBatch.Log("Cargando definiciones.");
			MyBatch.SetMess("Cargando definiciones");
			NomadXML xmlConsulta = NomadProxy.GetProxy().FileServiceIO().LoadFileXML("INTERFACE-DEF", interfaceOBJ.GetAttr("params"));		
			MyBatch.SetPro(10);

			strTMP = xmlConsulta.ToString(false);

			//Graba la consulta
			System.IO.StreamWriter swResult = new StreamWriter(strQueryName, false, System.Text.Encoding.UTF8);
			swResult.Write(strTMP);
			swResult.Close();
				
			MyBatch.Log("Ejecuta la consulta.");
			MyBatch.SetMess("Ejecuta la consulta");
			xmlResult = NomadEnvironment.QueryNomadXML(strTMP, xmlParametros.DocumentToString());		
			

			//Graba el resultado
			xmlResult.ToFile(strResultName);
			
			//Convierte el resultado en un Dictionary con formato JSON
			MyBatch.Log("Conviertiendo el resultado en JSON.");
			MyBatch.SetMess("Conviertiendo el resultado en JSON");
			xmlResult = xmlResult.FirstChild();
			dicResultado = xml2Dictionary(xmlResult);
			
			//Convierte el Dictionay en un string
			string stringJson = StringUtil.Object2JSON(dicResultado);
			
			//Graba el resultado final
			System.IO.StreamWriter sw=new StreamWriter(strFileName, false, System.Text.Encoding.UTF8);
			sw.Write(stringJson);
			sw.Close();
	  
			MyBatch.SetPro(100);
			MyBatch.Log("El proceso ha culminado correctamente.");
        }
       
		///Crea un Dictionary con "formato" Json desde un NomadXML
		private static Dictionary<string, object> xml2Dictionary(NomadXML pxmlData) {
			Dictionary<string, object> VALUES = new Dictionary<string, object>();
			List<Dictionary<string, object>> COLLECTION;
			NomadBatch MyBatch=NomadBatch.GetBatch("Generico","Generico");

			string strNodeName, strAttrName, strNodeType;
			string strRawValue;
			bool bolCollection;
			string strTipo = "caracter";

			//Recorre los atributos
			for (int a = 0; a < pxmlData.Attrs.Count; a++) {
				strNodeName = pxmlData.Attrs[a].ToString();
				strNodeType = strNodeName.Substring(0, 2).ToUpper();
				strAttrName = strNodeName.Substring(2, strNodeName.Length -2);

				strRawValue = pxmlData.GetAttr(strNodeName).ToUpper();
				if (strRawValue == "NULL") {
					VALUES[strAttrName] = null;
				} else {

					try {
						switch (strNodeType) {
							case "X_":
								strTipo = "NO se agrega";
								break;

							case "L_":
								strTipo = "lógico";
								try {
									VALUES[strAttrName] = pxmlData.GetAttrBool(strNodeName);
								} catch {
									VALUES[strAttrName] = false;
								}
								break;

							case "E_":
								strTipo = "entero";
								try {
									VALUES[strAttrName] = long.Parse(pxmlData.GetAttr(strNodeName));
								} catch {
									VALUES[strAttrName] = 0;
								}
								break;

							case "D_":
								strTipo = "decimal";
								try {
									VALUES[strAttrName] = pxmlData.GetAttrDecimal(strNodeName);
								} catch {
									VALUES[strAttrName] = 0.0M;
								}
								break;

							case "F_":
								strTipo = "fecha";
								try {
									VALUES[strAttrName] = pxmlData.GetAttrDateTime(strNodeName);
								} catch {
									VALUES[strAttrName] = DateTime.MinValue;
								}
								break;

							case "C_":
								strTipo = "caracter";
								VALUES[strAttrName] = pxmlData.GetAttr(strNodeName);
								break;

							default:
								strTipo = "caracter";
								VALUES[strNodeName] = pxmlData.GetAttr(strNodeName);
								break;
						}
					
					} catch (Exception ex) {
						throw new NomadAppException("El atributo '" + strNodeName + "' no se puede convertir en '" + strTipo + "'. El valor '" + pxmlData.GetAttr(strNodeName) + "' no es válido." + ex.Message);
					}
				}
			}
				
			for (Nomad.NSystem.Proxy.NomadXML xmlElement = pxmlData.FirstChild(); xmlElement != null; xmlElement = xmlElement.Next()) {
				strNodeName = xmlElement.Name;
				bolCollection = xmlElement.GetAttr("jsontype").ToUpper() == "ARRAY";
	
				if (bolCollection) {
					COLLECTION = new List<Dictionary<string, object>>();
					for (Nomad.NSystem.Proxy.NomadXML xmlItem = xmlElement.FirstChild(); xmlItem != null; xmlItem = xmlItem.Next()) {
						COLLECTION.Add(xml2Dictionary(xmlItem));
					}
					VALUES[strNodeName] = COLLECTION;
				} else {
					VALUES[strNodeName] = xml2Dictionary(xmlElement);
				}

			}
				
			return VALUES;
		}


        public static void NovPorVariables(Nomad.NSystem.Document.NmdXmlDocument xmlDoc)
        {
            try
            {

                NucleusRH.Base.Liquidacion.Liquidacion_Nov_Var.clsEntradaNovedades ObjNov;
                ObjNov = new NucleusRH.Base.Liquidacion.Liquidacion_Nov_Var.clsEntradaNovedades();
                ObjNov.ImportarVariablesPersona(xmlDoc);
            }
            catch (Exception ex)
            {
                throw new NomadAppException("error al ejecutar la carga de Novedades por variable en la liquidacion (ImportarVariablesPersona ). " + ex.Message);
            }
        }

        public static void EliminarLegajosNoLiquidados(string id)
        {
            NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Comienza el proceso", "Eliminar Legajos sin Recibo");
            NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Obteniendo Legajos sin Recibo", "Eliminar Legajos sin Recibo");

            try
            {
                // Obtiene legajos sin recibo
                string qryparam = "<DATOS id=\"" + id + "\"/>";
                NomadXML xmlLegajos = new NomadXML();
                xmlLegajos.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Resources.QRY_PER_SIN_RECIBO, qryparam));
                NomadEnvironment.GetTrace().Info(xmlLegajos.ToString());

                xmlLegajos = xmlLegajos.FirstChild();

                NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ per_liq;
                // Elimina cada legajo sin recibo
                foreach (NomadXML leg in xmlLegajos.GetChilds())
                {
                    per_liq = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(leg.GetAttr("oi_personal_liq"));
                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Eliminando legajo: " + leg.GetAttr("persona"), "Eliminar Legajos sin Recibo");
                    NomadEnvironment.GetCurrentTransaction().Delete(per_liq);
                }
                NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Proceso finalizado", "Eliminar Legajos sin Recibo");

            }
            catch (Exception e)
            {
                NomadProxy.GetProxy().Batch().Trace.Add("err", "Error: " + e.Message + '\n' + e.InnerException, "Eliminar Legajos sin Recibo");
                NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Proceso finalizado con error", "Eliminar Legajos sin Recibo");
            }
        }

        public static void AdministrarLegajos(Nomad.NSystem.Proxy.NomadXML param, string oi_liquidacion)
        {
            int c, t, p, tot = 0;
            NomadBatch MyBATCH = NomadBatch.GetBatch("Liquidación", "Liquidación");
            MyBATCH.Log("Inicia el Proceso...");

            //DOCUMENTO
            Hashtable perInit = new Hashtable();
            string[] parts;
            for (NomadXML MyCUR = param.FirstChild().FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
            {
                parts = MyCUR.GetAttr("values").Split(',');

                for (t = 0; t < parts.Length; t++)
                    perInit[parts[t]] = 1;
            }

            //BLOQUANDO LA LIQUIDACION.
            MyBATCH.SetMess("Bloqueando la Liquidacion...");
            if (!NomadProxy.GetProxy().Lock().LockOBJ("Liquidacion:id=" + oi_liquidacion))
            {
                MyBATCH.Err("La liquidación esta siendo **Procesada** en este momento.\\\\ Intente más tarde, si el problema persiste consulte con su administrador.");
                return;
            }
            MyBATCH.SetPro(5);

            //COMPROBANDO EL ESTADO.
            MyBATCH.SetMess("Comprobando el Estado...");
            LIQUIDACION liq = LIQUIDACION.Get(oi_liquidacion);
            if (liq.c_estado == "C") throw new NomadAppException("La liquidacion esta cerrada");

            //ARMO LA LISTA DE PERSONAS.
            MyBATCH.SetMess("Obtener la Lista de Personas Inicializadas...");
            Hashtable ht = NomadEnvironment.QueryHashtable(NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Resources.QRY_ADMINISTRAR, "<DATOS oi_liquidacion=\"" + oi_liquidacion + "\"/>", "id");
            MyBATCH.SetPro(10);

            NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO liq_ddo = null;

            //RECORRO LA HASH Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LOS ID QUE ENTRAN
            //DE SER ASI, ESTOS IDS HAY QUE ELIMINARLOS
            c = 0; t = 0;
            MyBATCH.SetMess("Eliminando Legajos...");
            foreach (string value in ht.Keys) { if (!perInit.ContainsKey(value)) { t++; } }
            foreach (string value in ht.Keys)
            {
                if (!perInit.ContainsKey(value))
                {
                    NomadXML MyLEG = (NomadXML)ht[value];

                    if (MyLEG.GetAttr("f_cierre") != "")
                    {
                        MyBATCH.Wrn("El legajo " + MyLEG.GetAttr("e_numero_legajo") + "-" + MyLEG.GetAttr("d_ape_y_nom") + " no puede ser eliminado porque tiene el Recibo Cerrado.");
                        tot++;
                    }
                    else
                    {
                        MyBATCH.SetMess("Eliminando Legajos (" + c.ToString() + "/" + t.ToString() + ")");
                        try
                        {
                            NomadXML xmlleg = (NomadXML)ht[value];
                            NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ leg;
                            leg = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(xmlleg.GetAttr("oi_personal_liq"));
                            NomadEnvironment.GetCurrentTransaction().Delete(leg);

                            if (xmlleg.GetAttr("oi_liq_ddo") != "")
                            {
                                if (liq_ddo == null) liq_ddo = NucleusRH.Base.Liquidacion.LiquidacionDDO.LIQUIDACION_DDO.Get(xmlleg.GetAttr("oi_liq_ddo"));

                                NucleusRH.Base.Liquidacion.LiquidacionDDO.RECIBO rec_ddo = (NucleusRH.Base.Liquidacion.LiquidacionDDO.RECIBO)liq_ddo.Recibos.GetById(xmlleg.GetAttr("oi_recibo"));

                                if (rec_ddo != null)
                                    liq_ddo.Recibos.Remove(rec_ddo);
                            }
                        }
                        catch (Exception e)
                        {
                            tot++;
                            MyBATCH.Wrn("No se pudo Eliminar el Legajo " + MyLEG.GetAttr("e_numero_legajo") + "-" + MyLEG.GetAttr("d_ape_y_nom") + " - " + e.Message);
                        }
                    }
                    c++; MyBATCH.SetPro(10, 30, t, c);
                }
            }
            MyBATCH.Log("Se eliminaron " + c.ToString() + " Legajos...");
            MyBATCH.SetPro(30);

            // GUARDA LIQ99_LIQUIDACION PARA BORRAR LOS LIQ99_RECIBOS
            if (liq_ddo != null) NomadEnvironment.GetCurrentTransaction().Save(liq_ddo);

            //RECORRO LA HASH Y PREGUNTO SI EN ELLA HAY IDS QUE NO ESTAN EN LOS ID QUE ENTRAN
            //DE SER ASI, ESTOS IDS HAY QUE ELIMINARLOS
            string idList;
            c = 0; t = 0; p = 1; idList = "";
            MyBATCH.SetMess("Agregando Legajos...");
            foreach (string newId in perInit.Keys) { if (!ht.ContainsKey(newId)) { t++; } }
            foreach (string newId in perInit.Keys)
            {
                if (!ht.ContainsKey(newId))
                {
                    idList += "," + newId;
                    c++; tot++;

                    if ((c % 100) == 0)
                    {
                        NomadXML paramIn = new NomadXML("DATOS");
                        paramIn.SetAttr("oi_liquidacion", oi_liquidacion);
                        paramIn.SetAttr("ids", idList.Substring(1));

                        MyBATCH.SetMess("Iniciando Legajos (" + p.ToString() + " al " + c.ToString() + " de " + t.ToString() + ")");
                        MyBATCH.SetSubBatch(30 + 60 * p / t, 30 + 60 * c / t);
                        Inicializar(paramIn);

                        idList = ""; p = c + 1;
                        MyBATCH.SetPro(30, 90, t, c);
                    }
                }
                else
                    tot++;
            }
            if (idList != "")
            {
                NomadXML paramIn = new NomadXML("DATOS");
                paramIn.SetAttr("oi_liquidacion", oi_liquidacion);
                paramIn.SetAttr("ids", idList.Substring(1));

                MyBATCH.SetMess("Iniciando Legajos (" + p.ToString() + " al " + c.ToString() + " de " + t.ToString() + ")");
                MyBATCH.SetSubBatch(30 + 60 * p / t, 30 + 60 * c / t);
                Inicializar(paramIn);
            }
            MyBATCH.Log("Se agregaron " + c.ToString() + " Legajos...");
            MyBATCH.Log("Sin cambios " + (tot - c).ToString() + " Legajos...");
            MyBATCH.SetPro(90);

            //Actualiza la liquidacion
            MyBATCH.SetMess("Actualizando el Estado a la Liquidacion...");
            NomadXML xmlCant = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Resources.QRY_CANT_INI, "<DATOS oi_liquidacion=\"" + oi_liquidacion + "\"/>");
            if (tot > 0 && liq.c_estado == "A")
            {
                liq.c_estado = "I";
                NomadEnvironment.GetCurrentTransaction().Save(liq);
            }
            else
                if (tot == 0 && liq.c_estado == "I")
                {
                    liq.c_estado = "A";
                    NomadEnvironment.GetCurrentTransaction().Save(liq);
                }
            MyBATCH.SetPro(95);

            //Desbloqueo
            MyBATCH.SetMess("Desbloqueando la Liquidacion...");
            NomadProxy.GetProxy().Lock().UnLockOBJ("Liquidacion:id=" + oi_liquidacion);

            //PROCESO FINALIZADO.
            MyBATCH.Log("Proceso Finalizado.");
        }

        public static void Inicializar(Nomad.NSystem.Proxy.NomadXML xmlParam)
        {
            // Obtiene la liquidacion
            NomadBatch MyBATCH = NomadBatch.GetBatch("Adm.Legajos", "Adm.Legajos");

            NomadLog.Info("xmlParam: " + xmlParam.ToString());

            string liq = xmlParam.GetAttr("oi_liquidacion");
            NomadXML dokum;

            MyBATCH.SetMess("Obteniendo Datos de Personas a Inicializar");
            dokum = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Resources.QRY_DATOS_INI, xmlParam.ToString(), false).FirstChild();
            MyBATCH.SetPro(10);

            MyBATCH.SetMess("Inicializando...");
            int c, t;

            c = 0;
            t = dokum.ChildLength;
            for (NomadXML leg = dokum.FirstChild(); leg != null; leg = leg.Next())
            {
                c++;
                MyBATCH.SetPro(10, 90, t, c);
                NomadLog.Info("Agregando legajo: " + leg.GetAttr("persona"));
                bool err = false;

                // Valida AFJP, Sindicato, Obra Social y Categoria
                if (leg.GetAttr("oi_afjp") == "") { MyBATCH.Err("**" + leg.GetAttr("persona") + "** no tiene asignada una AFJP"); err = true; }
                if (leg.GetAttr("oi_sindicato") == "") { MyBATCH.Err("**" + leg.GetAttr("persona") + "** no tiene asignado el Sindicato"); err = true; }
                if (leg.GetAttr("oi_obra_social") == "") { MyBATCH.Err("**" + leg.GetAttr("persona") + "** no tiene asignada una Obra Social"); err = true; }
                if (leg.GetAttr("oi_categoria") == "") { MyBATCH.Err("**" + leg.GetAttr("persona") + "** no tiene asignada una Categoria"); err = true; }
                if (leg.GetAttr("oi_forma_pago") == "") { MyBATCH.Err("**" + leg.GetAttr("persona") + "** no tiene asignada una Forma de Pago"); err = true; }
                if (leg.GetAttr("wrn_suc") == "1") { MyBATCH.Err("**" + leg.GetAttr("persona") + "** cobra por banco y debe asignarle la sucursal bancaria y una cuenta o CBU"); err = true; }
                if (err) continue;

                // Inicializa
                NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ nuevoDDO = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ();
                nuevoDDO.oi_liquidacion = liq;
                nuevoDDO.oi_personal_emp = leg.GetAttr("oi_personal_emp");
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(nuevoDDO);
                }
                catch (Exception e)
                {
                    MyBATCH.Err("Error inicializando legajo " + leg.GetAttr("persona") + " - " + e.Message);
                }
            }
        }

        public static void RepReportesGenericos(Nomad.NSystem.Proxy.NomadXML param)	
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Reportes por Concepto");

            objBatch.SetPro(0);
            objBatch.SetMess("Ejecutando el Reporte...");

            Nomad.NSystem.Html.NomadHtml nmdHtml = new Nomad.NSystem.Html.NomadHtml("NucleusRH.Base.Liquidacion.ReportesPorConceptos.rpt", param.ToString(), NomadProxy.GetProxy());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

			// Escribe el html al stream
			using (System.IO.StreamWriter sWriter = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8))
			{
				nmdHtml.GenerateStream(sWriter);
			}

        }

        public static void RepInterfazContable(Nomad.NSystem.Proxy.NomadXML param, string agrupar)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Reportes de Conceptos por Persona");
            Nomad.NomadHTML nmdHtml;
            objBatch.SetPro(0);
            objBatch.SetMess("Ejecutando el Reporte...");

            switch (agrupar)
            {
                case "Centro":
                    nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Liquidacion.InterfazContablePorCentro.rpt", param.ToString());
                    break;
                case "Cuenta":
                    nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Liquidacion.InterfazContablePorCuenta.rpt", param.ToString());
                    break;
                case "GCentro":
                    nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Liquidacion.InterfazContablePorGrupoCentro.rpt", param.ToString());
                    break;
                case "CtaGCentro":
                    nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Liquidacion.InterfazContablePorCtaGrupoCentro.rpt", param.ToString());
                    break;
                default:
                    nmdHtml = new Nomad.NomadHTML("NucleusRH.Base.Liquidacion.InterfazContablePorCentro.rpt", param.ToString());
                    break;
            }

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            nmdHtml.GenerateHTML(sw);

            sw.Close();
        }

        public static void Form572(Nomad.NSystem.Proxy.NomadXML param)
        {
            NucleusRH.Base.Liquidacion.Form572.Execute(param);
        }

        public static void Form649(Nomad.NSystem.Proxy.NomadXML param)
        {
            NucleusRH.Base.Liquidacion.Form649.Execute(param);
        }

        public static void LiqIG(Nomad.NSystem.Proxy.NomadXML param)
        {
          string reporte = "NucleusRH.Base.Liquidacion.LiquidacionIG.LiquidacionIG.rpt";
            NucleusRH.Base.Liquidacion.LiquidacionIG.Execute(reporte,param);
        }

        public static void LiqIGDesde2017(Nomad.NSystem.Proxy.NomadXML param)
        {
          string reporte = "NucleusRH.Base.Liquidacion.LiquidacionIGDesde2017.LiquidacionIGDesde2017.rpt";
            NucleusRH.Base.Liquidacion.LiquidacionIG.Execute(reporte,param);
        }

        public static void LiqIGDesde2018(Nomad.NSystem.Proxy.NomadXML param)
        {
          string reporte = "NucleusRH.Base.Liquidacion.LiquidacionIGDesde2018.LiquidacionIGDesde2018.rpt";
            NucleusRH.Base.Liquidacion.LiquidacionIG.Execute(reporte,param);
        }

		public static void LiqIGByReport(string strReportName, Nomad.NSystem.Proxy.NomadXML xmlParam) {
			NucleusRH.Base.Liquidacion.LiquidacionIG.Execute(strReportName, xmlParam);
		}

        ////////////////////////////////////////////////////////////////////////////////////////////
    //Los métodos siguientes se utilizan en WorkFlows y NO deberían estar en esta librería,
    //pero por cuestiones de definiciones previas se situo aquí.
    //El lugar correcto sería LegajoLiquidacion.PERSONAL_EMP. Sin embargo,
    //no se puede generar un controlador en una clase extendida
    ////////////////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////////
    //Este método guarda una solictud de vacaciones generada desde el WF solicitud de vacaciones.
    // Recibe los datos desde el formulario del EF.
    ////////////////////////////////////////////////////////////////////////////////////////////

    public static string AgregarAnticipo(string PER, SortedList<string, object> ANTICIPO)
    {
          NomadLog.Debug("-----------------------------------------");
          NomadLog.Debug("-----------AGREGAR ANTICIPO--------------");
         NomadLog.Debug("-----------------------------------------");

           NomadLog.Debug("AgregarAnticipo.oi_personal: " + PER);

           //Get PERSONAL
           NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO ANT;
           NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP PERSONAL_EMP;
           ANT = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO();

           PERSONAL_EMP = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(PER);
           if (PERSONAL_EMP == null) return "0";

           //Recorro la sortedList para dar valor al ANTICIPO
           foreach (KeyValuePair<string, object> kvp in ANTICIPO)
        {
             switch (kvp.Key)
             {
                  case "F_ANTICIPO":
                    if (kvp.Value != null)
                      ANT.f_anticipo = (DateTime)kvp.Value;
                        break;
                  case "IMPORTE":
                     if (kvp.Value != null)
                         ANT.n_importe = Convert.ToDouble(kvp.Value);
                        break;
                  case "CUOTAS":
                    if (kvp.Value != null)
                        ANT.e_cant_cuotas = Convert.ToInt32(kvp.Value);
                        break;
                  case "OBS":
                  if (kvp.Value != null)
                        ANT.o_anticipo = (string)kvp.Value;
                        break;
                  case "PERIODO":
                    if (kvp.Value != null)
                        ANT.e_periodo = Convert.ToInt32(kvp.Value);
                        break;
                  case "QUIN":
                    if (kvp.Value != null)
                        ANT.e_quincena = Convert.ToInt32(kvp.Value);
                        break;
                  case "LIQ":
                    if (kvp.Value != null)
                        ANT.l_liquida = (bool)kvp.Value;
                        break;
             }
        }

        ANT.f_solicitud = DateTime.Now;
        ANT.c_estado = "O";

        PERSONAL_EMP.ANTICIPOS.Add(ANT);

        try
        {
          NomadEnvironment.GetCurrentTransaction().Save(PERSONAL_EMP);
            return "1";
        }
        catch (Exception ex)
        {
          NomadLog.Debug("Error guardando ANTICIPO: " + ex);
           return "0";
        }
    }

    public static void ActualizarLiquidacion(string oi_liq_genera)
    {
        NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION liq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(oi_liq_genera);

        liq.oi_liq_generoNull = true;
        NomadEnvironment.GetCurrentTransaction().SaveRefresh(liq);
    }
  }
}


