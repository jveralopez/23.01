using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Tiempos_Trabajados.FichadasIng
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Fichadas Ingresadas
    public partial class FICHADASING : Nomad.NSystem.Base.NomadObject
    {
        public static void Verificar(Nomad.NSystem.Document.NmdXmlDocument pobjParams)
        {
            //TIRO EL METODO DE MATIAS PARA VALIDAR FICHADAS Y CAPTURO EL RESULTADO EN "str"Result"
            string strResult = "";

            NucleusRH.Base.Tiempos_Trabajados.Procesos objValidador;
            objValidador = new NucleusRH.Base.Tiempos_Trabajados.Procesos();

            //Crea el objeto que ejecuta el método
            strResult = objValidador.ValidarFichadas(pobjParams.ToString(), false);

            NomadEnvironment.GetTrace().Info("RES -- " + strResult);

            //GRABO EL ARCHIVO CON EL ID DEL BATCH EN LA CRPETA NOMAD/TEMP PARA LUEGO DESDE EL FLOW RECUPERARLA Y ARMAR LOS REPORTES
            NomadEnvironment.GetProxy().FileServiceIO().SaveFile("TEMP", "VerificarFichadas_" + NomadEnvironment.GetProxy().Batch().ID + ".xml", strResult);

            //return new Nomad.NSystem.Document.NmdXmlDocument(strResult);
        }
        public static void ReporteFichadas(Nomad.NSystem.Proxy.NomadXML xmlParam)
        {
            string outtype = xmlParam.FirstChild().GetAttr("salida");
            string anuladas = xmlParam.FirstChild().GetAttr("anuladas");
            string f_desde = xmlParam.FirstChild().GetAttr("f_desde");
            string f_hasta = xmlParam.FirstChild().GetAttr("f_hasta");
            // QRY1 - Trae la lista de estructuras
            // QRY2 - Trae la lista de legajos de una estructura
            NomadXML XMLQRY1 = new NomadXML(NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.Resources.ReporteFichadas1);
            NomadXML XMLQRY2 = new NomadXML(NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING.Resources.ReporteFichadas2);
            string QRY1 = XMLQRY1.ToString();
            string QRY2 = XMLQRY2.ToString();

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Ejecución de Reporte");
            objBatch.SetMess("Obteniendo datos...");
            objBatch.SetPro(0);

            NomadEnvironment.GetTrace().Info("inicio");

            objBatch.SetPro(10);

            // Ejecuta los querys
            NomadXML qryOutHead = NomadEnvironment.QueryNomadXML(QRY1, xmlParam.ToString());
            NomadEnvironment.GetTrace().Info(qryOutHead.ToString());

            // Ejecuta los querys !!
            // Obtiene la lista de estructuras organizativas
            ArrayList childs_est = qryOutHead.FirstChild().FindElement("DATA").FindElement("ORG").GetChilds();

            // Obtiene la lista de personas del filtro
            ArrayList childs_leg = xmlParam.FirstChild().FirstChild().GetChilds();

            int childs_est_count = childs_est.Count;
            int childs_leg_count = childs_leg.Count;

            int cant_leg = 50;

            // Por cada estructura...
            for (int i = 0; i < childs_est_count; i++)
            {
                NomadXML result = new NomadXML("ROWS");
                NomadXML estr = (NomadXML)childs_est[i];
                string oi_estructura = estr.GetAttr("oi_estructura");

                // Trae las personas de a pedazos
                for (int k = 0; k < childs_leg_count; k += cant_leg)
                {
                    System.Text.StringBuilder a = new System.Text.StringBuilder("<ROWS>");
                    for (int j = k; j < k + cant_leg && j < childs_leg_count; j++)
                        a.Append(childs_leg[j].ToString());
                    a.Append("</ROWS>");

                    string param = "<FILTRO oi_estructura=\"" + oi_estructura + "\" anuladas=\"" + anuladas + "\" f_desde=\"" + f_desde + "\" f_hasta=\"" + f_hasta + "\">" + a.ToString() + "</FILTRO>";

                    NomadXML qryout = NomadEnvironment.QueryNomadXML(QRY2, param);

                    for (NomadXML row = qryout.FirstChild().FirstChild(); row != null; row = row.Next())
                        estr.AddXML(row);

                }
                NomadEnvironment.GetTrace().Info("estr-" + i.ToString());
                if (estr.FirstChild() == null)
                    estr.SetAttr("visible", "0");

                if (childs_est_count > 0)
                    objBatch.SetPro((i + 1) * 70 / childs_est_count + 10);
            }

            // Generando Reporte
            Nomad.NSystem.Html.NomadHtml nmdHtml = new Nomad.NSystem.Html.NomadHtml("NucleusRH.Base.Tiempos_Trabajados.FichadasIncorporadas.rpt", "", NomadProxy.GetProxy());

            string outFileName = NomadProxy.GetProxy().Batch().ID + ".htm";
            string outFilePath = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\";
            NomadBatch.Trace("Generando Reporte HTML en path:'" + outFilePath + outFileName + "'");

            StreamWriter sw = new System.IO.StreamWriter(outFilePath + "\\" + outFileName, false, System.Text.Encoding.UTF8);

            NomadEnvironment.GetTrace().Info("OUT XML-" + qryOutHead.ToString());

            objBatch.SetPro(80);
            nmdHtml.GenerateHtml(qryOutHead.ToString(), sw.BaseStream);

            objBatch.SetPro(100);
            sw.Close();
        }
        public static void CargarFichadas(Nomad.NSystem.Proxy.NomadXML param, bool EditMyFic)
        {
            int C = 0;
            int E = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Carga de Fichadas");

            ArrayList lista = (ArrayList)param.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando Fichadas " + (xml + 1) + " de " + lista.Count);

                DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(row.GetAttr("fecha"));

                //Cargo el Legajo
                NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP ddoLEG = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(row.GetAttr("oi_personal_emp"));

                if (EditMyFic)
                {
                    //cargo la persona
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(ddoLEG.oi_personal);

                    if (ddoPER.oi_usuario_sistema == NomadProxy.GetProxy().UserEtty)
                    {
                        objBatch.Err("No se permite agregar fichadas para el propio usuario. Legajo " + ddoLEG.descr);
                        E++;
                        continue;
                    }
                }

                NomadEnvironment.GetTrace().Info("LEG -- " + ddoLEG.SerializeAll());
                if (!ddoLEG.e_nro_legajo_relojNull)
                {
                    //Creo el DDO
                    NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFIC = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();
                    string code = fecha.ToString("yyyyMMddHHmm") + ddoLEG.e_nro_legajo_reloj.ToString() + "M" + row.GetAttr("c_tipo");

                    //consulto si ya existe una fichada con el codigo calculado
                    NomadXML xmlcode = NomadEnvironment.QueryNomadXML(FICHADASING.Resources.QRY_CODEFICHADA, "<PARAM code=\"" + code + "\"/>");
                    if (xmlcode.FirstChild().GetAttr("flag") != "1")
                    {
                        ddoFIC.c_fichadasing = code;
                        ddoFIC.c_estado = "P";
                        ddoFIC.c_origen = "M";
                        ddoFIC.c_tipo = row.GetAttr("c_tipo");
                        ddoFIC.e_numero_legajo = ddoLEG.e_nro_legajo_reloj;
                        ddoFIC.f_fechahora = fecha;
                        ddoFIC.l_entrada = row.GetAttr("c_tipo") == "E";
                        ddoFIC.oi_personal_emp = ddoLEG.Id;
                        ddoFIC.oi_terminal = row.GetAttr("oi_terminal");
                        ddoFIC.d_user_create = NomadProxy.GetProxy().UserName;
                        ddoFIC.f_create = DateTime.Now;

                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(ddoFIC);
                            C++;
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Se ha producido un error al incorporar la fichada para el Legajo: " + ddoLEG.descr + " con Fecha-Hora: " + fecha.ToString() + ": " + e.Message);
                            E++;
                        }
                    }
                    else
                    {
                        objBatch.Err("Fichada duplicada para el Legajo: " + ddoLEG.descr + " con Fecha-Hora: " + fecha.ToString());
                        E++;
                    }
                }
                else
                {
                    objBatch.Err("El legajo " + ddoLEG.descr + " no está habilitado para fichar");
                    E++;
                }
            }

            if (C != 0)
            {
                objBatch.Log("Se incorporaron " + C.ToString() + " fichada/s.");
            }
            if (E != 0)
            {
                objBatch.Log("Se rechazaron " + E.ToString() + " fichada/s.");
            }
        }

        public static void IngresoMasivo(Nomad.NSystem.Proxy.NomadXML param, bool EditMyFic)
        {
            int C = 0;
            int E = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Ingreso Masivo de Fichadas");

            DateTime fecha = Nomad.NSystem.Functions.StringUtil.str2date(param.FirstChild().GetAttr("f_fechahora"));
            string tipo = param.FirstChild().GetAttr("c_tipo");
            string terminal = param.FirstChild().GetAttr("oi_terminal");

            ArrayList lista = (ArrayList)param.FirstChild().FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando Fichadas " + (xml + 1) + " de " + lista.Count);

                //Cargo el Legajo
                NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP ddoLEG = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(row.GetAttr("id"));

                if (EditMyFic)
                {
                    //cargo la persona
                    NucleusRH.Base.Personal.Legajo.PERSONAL ddoPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(ddoLEG.oi_personal);

                    NomadEnvironment.GetTrace().Info("ddoPER.oi_usuario_sistema -- " + ddoPER.oi_usuario_sistema);
                    NomadEnvironment.GetTrace().Info("NomadProxy.GetProxy().UserEtty -- " + NomadProxy.GetProxy().UserEtty);
                    if (ddoPER.oi_usuario_sistema == NomadProxy.GetProxy().UserEtty)
                    {
                        objBatch.Err("No se permite agregar fichadas para el propio usuario. Legajo " + ddoLEG.descr);
                        E++;
                        continue;
                    }
                }

                NomadEnvironment.GetTrace().Info("LEG -- " + ddoLEG.SerializeAll());
                if (!ddoLEG.e_nro_legajo_relojNull)
                {
                    //Creo el DDO
                    NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING ddoFIC = new NucleusRH.Base.Tiempos_Trabajados.FichadasIng.FICHADASING();
                    string code = fecha.ToString("yyyyMMddHHmm") + ddoLEG.e_nro_legajo_reloj.ToString() + "M" + tipo;

                    //consulto si ya existe una fichada con el codigo calculado
                    NomadXML xmlcode = NomadEnvironment.QueryNomadXML(FICHADASING.Resources.QRY_CODEFICHADA, "<PARAM code=\"" + code + "\"/>");
                    if (xmlcode.FirstChild().GetAttr("flag") != "1")
                    {
                        ddoFIC.c_fichadasing = code;
                        ddoFIC.c_estado = "P";
                        ddoFIC.c_origen = "M";
                        ddoFIC.c_tipo = tipo;
                        ddoFIC.e_numero_legajo = ddoLEG.e_nro_legajo_reloj;
                        ddoFIC.f_fechahora = fecha;
                        ddoFIC.l_entrada = tipo == "E";
                        ddoFIC.oi_personal_emp = ddoLEG.Id;
                        ddoFIC.oi_terminal = terminal;
                        ddoFIC.d_user_create = NomadProxy.GetProxy().UserName;
                        ddoFIC.f_create = DateTime.Now;

                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(ddoFIC);
                            C++;
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Se ha producido un error al incorporar la fichada para el Legajo: " + ddoLEG.descr + ": " + e.Message);
                            E++;
                        }
                    }
                    else
                    {
                        objBatch.Err("Fichada duplicada para el Legajo: " + ddoLEG.descr);
                        E++;
                    }
                }
                else
                {
                    objBatch.Err("El legajo " + ddoLEG.descr + " no está habilitado para fichar");
                    E++;
                }
            }

            if (C != 0)
            {
                objBatch.Log("Se incorporaron " + C.ToString() + " fichada/s.");
            }
            if (E != 0)
            {
                objBatch.Log("Se rechazaron " + E.ToString() + " fichada/s.");
            }
        }
        public static int GetPersonalEmpresaID(string legajo)
        {
            legajo = int.Parse(legajo).ToString();
            System.Collections.Hashtable retval = (System.Collections.Hashtable)NomadProxy.GetProxy().CacheGetObj("PEREMP");
            if (retval == null)
            {
                //Ejecutando el QUERY
                NomadXML MyXML = new NomadXML(NomadProxy.GetProxy().SQLService().Get(
                      @"
      <qry:main doc=""PARAM"" xmlns:qry=""XXX"">
        <qry:insert-select name=""sql1""/>
      </qry:main>

      <qry:select doc=""PARAM"" name=""sql1"" xmlns:qry=""XXX"">
        <qry:xquery>
          for $r in sql('
            SELECT TTA04_PERSONAL.oi_personal_emp as OI_PERSONAL_EMP, TTA04_PERSONAL.e_nro_legajo_reloj as e_nro_legajo_reloj
            FROM TTA04_PERSONAL
            WHERE isNotNull(TTA04_PERSONAL.e_nro_legajo_reloj)
          ')/ROWS
        </qry:xquery>
        <qry:out>
          <qry:insert-element doc-path=""$r"" />
        </qry:out>
      </qry:select>", ""));

                //Cargando la Lista
                retval = new System.Collections.Hashtable();
                for (NomadXML cur = MyXML.FindElement("ROWS").FirstChild(); cur != null; cur = cur.Next())
                    if (cur.Name.ToUpper() == "ROW")
                        retval[cur.GetAttr("e_nro_legajo_reloj")] = cur.GetAttrInt("OI_PERSONAL_EMP");

                //Agregando al CACHE
                NomadProxy.GetProxy().CacheAdd("PEREMP", retval);
            }

            //Existe
            if (retval.ContainsKey(legajo))
                return (int)retval[legajo];

            //No encontrado
            return 0;
        }

        public static int GetPersonalEmpresaIDFromCard(string tarjeta)
        {
            System.Collections.Hashtable retval = (System.Collections.Hashtable)NomadProxy.GetProxy().CacheGetObj("PEREMP");
            if (retval == null)
            {
                //Ejecutando el QUERY
                NomadXML MyXML = new NomadXML(NomadProxy.GetProxy().SQLService().Get(
                      @"
      <qry:main doc=""PARAM"" xmlns:qry=""XXX"">
        <qry:insert-select name=""sql1""/>
      </qry:main>

      <qry:select doc=""PARAM"" name=""sql1"" xmlns:qry=""XXX"">
        <qry:xquery>
          for $r in sql('
            SELECT TTA04_PERSONAL.oi_personal_emp as OI_PERSONAL_EMP, TTA04_PERSONAL.d_nro_tarjeta as d_nro_tarjeta
            FROM TTA04_PERSONAL
            WHERE isNotNull(TTA04_PERSONAL.d_nro_tarjeta)
            AND TTA04_PERSONAL.d_nro_tarjeta != \'\'
          ')/ROWS
        </qry:xquery>
        <qry:out>
          <qry:insert-element doc-path=""$r"" />
        </qry:out>
      </qry:select>", ""));

                //Cargando la Lista
                retval = new System.Collections.Hashtable();
                for (NomadXML cur = MyXML.FindElement("ROWS").FirstChild(); cur != null; cur = cur.Next())
                    if (cur.Name.ToUpper() == "ROW")
                        retval[cur.GetAttr("d_nro_tarjeta")] = cur.GetAttrInt("OI_PERSONAL_EMP");

                //Agregando al CACHE
                NomadProxy.GetProxy().CacheAdd("PEREMP", retval);
            }

            //Existe
            if (retval.ContainsKey(tarjeta))
                return (int)retval[tarjeta];

            //No encontrado
            return 0;
        }

        public static bool FinchadaExist(string code_fichada)
        {
            string code_date = code_fichada.Substring(0, 8);

            System.Collections.Hashtable retval = (System.Collections.Hashtable)NomadProxy.GetProxy().CacheGetObj("FICHADAS:" + code_date);
            if (retval == null)
            {
                //Ejecutando el QUERY
                NomadXML MyXML = new NomadXML(NomadProxy.GetProxy().SQLService().Get(
                      @"
      <qry:main doc=""PARAM"" xmlns:qry=""XXX"">
        <qry:insert-select name=""sql1""/>
      </qry:main>

      <qry:select doc=""PARAM"" name=""sql1"" xmlns:qry=""XXX"">
        <qry:xquery>
          for $r in sql('
            SELECT TTA07_FICHADASING.c_fichadasing as c_fichadasing
            FROM TTA07_FICHADASING
            WHERE STARTWITH(TTA07_FICHADASING.c_fichadasing,\'" + code_date + @"\')
          ')/ROWS
        </qry:xquery>
        <qry:out>
          <qry:insert-element doc-path=""$r"" />
        </qry:out>
      </qry:select>", ""));

                //Cargando la Lista
                retval = new System.Collections.Hashtable();
                for (NomadXML cur = MyXML.FindElement("ROWS").FirstChild(); cur != null; cur = cur.Next())
                    if (cur.Name.ToUpper() == "ROW")
                        retval[cur.GetAttr("c_fichadasing")] = 0;

                //Agregando al CACHE
                NomadProxy.GetProxy().CacheAdd("FICHADAS:" + code_date, retval);
            }

            //Existe
            if (retval.ContainsKey(code_fichada))
                return true;

            //No encontrado
            retval[code_fichada] = 0;
            return false;
        }

        public static void ImportarFichadas(int oi_terminal)
        {
            NomadBatch b = NomadBatch.GetBatch("Importar Fichadas", "Importar Fichadas");
            NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL TERM = NucleusRH.Base.Tiempos_Trabajados.Terminales.TERMINAL.Get(oi_terminal);

            b.Log("Importando fichadas desde " + TERM.d_archivo_reg + " .");

            //Bloqueo la Instancia.
            if (!NomadProxy.GetProxy().Lock().LockOBJ("IncorporarFichadas"))
                throw new Exception("Se estan incorporando Fichadas en este Momento.");

            b.SetPro(5);
            b.SetSubBatch(5, 100);
            switch (TERM.c_formato_reg.ToUpper())
            {
                case "SRF":
                    { //SEEBEK Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.SRFInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "RWN":
                    { //REIWINReloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.RWNInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "MRF":
                    { //MACRONET Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.MRFInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "CRO":
                    { //CRONOS Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.CROInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "HRF":
                    { //HORUS Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.HRFInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ERT":
                    { //ER-TECKNO Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ERTInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ASE":
                    { //ASENSIO Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ASEInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ASEV2":
                    { //ASENSIO Version 2 - Segundos - Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ASEV2InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;
                 case "ASEV3":
                   { //ASENSIO Version 3 generica - Segundos - Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ASEV3InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;
                 case "ARSEMET":
                    { //ARSEMET - Evento long 3 - dd/mm/yyyy formato fecha.
                        NucleusRH.Base.Tiempos_Trabajados.ARSEMETInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;
                case "SOFI":
                    { //SOFIA Interface - Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.SOFInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;
                case "SOF2":
                    { //SOFIA Interface - Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.SOF2InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ZK":
                    { //ZK Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ZKInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ZKE9":
                    { //ZK Modelo E9 Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ZKE9InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ZKIN":
                    { //ZK Modelo IN01-A+ID Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.ZKINInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "CSVR":
                    { //CSV Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.CSVRInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "CSVA":
                    { //CSV Acceso Format.
                        NucleusRH.Base.Tiempos_Trabajados.CSVAInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ST":
                    { //SISTEMICA Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.SISTEMICAInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "GAF":
                    { //GENERICO ANCHO FIJO Reloj Format.
                        NucleusRH.Base.Tiempos_Trabajados.GAFInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "CSVC":
                    { //CSV Clasificado Format.
                        NucleusRH.Base.Tiempos_Trabajados.CSVCInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "IO":
                    { //Formato Login/Logout.
                        NucleusRH.Base.Tiempos_Trabajados.IOInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "AN":
                    { //Formato ANVIZ (TSM).
                        NucleusRH.Base.Tiempos_Trabajados.ANInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ANV2":
                    { //Formato ANVIZ (TSM).
                        NucleusRH.Base.Tiempos_Trabajados.ANV2InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

        case "GAFV2":
                    { //GENERICO ANCHO FIJO Reloj Format V2 (LILIANA)
                        NucleusRH.Base.Tiempos_Trabajados.GAFV2InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "RWN2":
                    { //REIWINReloj Format Ancho fijo (HSJdeD).
                        NucleusRH.Base.Tiempos_Trabajados.RWN2InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ASEGO":
                    { //AS Reloj Format Ancho fijo (Grupo Oroño) - Empresa ICR y Sanatorio de niños.
                        NucleusRH.Base.Tiempos_Trabajados.ASEGOInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "ME":
                    { //Format Campos separados por ; (Grupo Oroño) - Empresa ME - Medicina Escencial.
                        NucleusRH.Base.Tiempos_Trabajados.MEInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "DMO":
                    { //Format Campos separados por , (Grupo Oroño) - Empresa DMO.
                        NucleusRH.Base.Tiempos_Trabajados.DMOInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

        case "DMO2":
          { //Format Campos separados por , (Grupo Oroño) - Empresa DMO2 nuevo formato.
            NucleusRH.Base.Tiempos_Trabajados.DMO2InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
          } break;

                case "GOFUNES":
           { //Format Campos separados por , (Grupo Oroño) - Fichadas GO Funes.
            NucleusRH.Base.Tiempos_Trabajados.GOFUNESInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
           } break;

                case "SP":
                    { //Format Campos separados por ; (Grupo Oroño) - Empresa SP (Sanatorio Parque)
                        NucleusRH.Base.Tiempos_Trabajados.SPInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;

                case "FAM":
                    { //Format Campos separados por ;  (Alperovich)
                        NucleusRH.Base.Tiempos_Trabajados.FAMInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;
                 case "ZKTECO":
                    {
                        NucleusRH.Base.Tiempos_Trabajados.ZKTecoInterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                    } break;
                 case "GAFV3":
                     {
                    //GENERICO ANCHO FIJO Reloj Format V3 (LILIANA)
                        NucleusRH.Base.Tiempos_Trabajados.GAFV3InterfaceFichadas.FICHADASING.ImportarFichadas(oi_terminal);
                   } break;

                case "SCF":
                    throw new Exception("Formato SCF NO implementado.");

                case "USF":
                    throw new Exception("Formato USF NO implementado.");

                case "SNF":
                    throw new Exception("No se puede importar desde un Archivo de NOMINA.");

                default:
                    throw new Exception("Formato de ARCHIVO Desconocido.");
            }
        }
    }
}


