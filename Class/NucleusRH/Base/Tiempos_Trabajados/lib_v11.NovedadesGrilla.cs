using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Tiempos_Trabajados.Esperanzaper;
using NucleusRH.Base.Tiempos_Trabajados.Liquidacion_Personas;
using NucleusRH.Base.Tiempos_Trabajados.Interfaces;

namespace NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarNovedades(string oi_terminal, ref Nomad.NSystem.Proxy.NomadXML xmlPagina)
        {
            int Linea;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Tiempos_Trabajados.Interfaces.NOVEDADES objRead;
            string EstructuraOI,ErrPasoDescr = "";
            NomadXML MyDOC, MyPAGE = null, MyROWS = new NomadXML(), MyROW;
            int h = 0, numPage = 0, t;
            NomadBatch objBatch;
            string FECDESDE = "", FECHASTA = "";
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importacion de Novedades");

            EstructuraOI = NomadEnvironment.QueryValue("TTA05_TERMINALES", "oi_estructura", "oi_terminal", oi_terminal, "", true);
            if (EstructuraOI == null)
            {
                objBatch.Err("La terminal especificada no existe, se cancela la importacion");
                return;
            }

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(NOVEDADES.Resources.qry_rows, ""));

            //Creando el Documento.
            MyDOC = new NomadXML("PAGES");
            MyDOC.SetAttr("oi_terminal", oi_terminal);

            for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
            {
                objBatch.SetPro(0, 100, IDList.FirstChild().ChildLength, Linea);
                objBatch.SetMess("Importando novedades " + Linea + " de " + IDList.FirstChild().ChildLength);
                objRead = NucleusRH.Base.Tiempos_Trabajados.Interfaces.NOVEDADES.Get(IDCur.GetAttr("id"));

                if (Linea > 20 * (numPage + 1) || Linea == 1)
                {
                    /*if (MyPAGE != null)
                    {
                        NomadProxy.GetProxy().FileServiceIO().SaveFile("INTERFACES", NomadProxy.GetProxy().UserName + ".intnovedades.xml", MyDOC.ToString());
                        MyPAGE = MyPAGE.FirstChild();
                        SincronizarPagina(ref MyPAGE);
                    }*/

                    numPage = Linea / 20;

                    //Obtengo la PAGINA
                    ErrPasoDescr = "Obteniendo la Pagina";
                    MyPAGE = MyDOC.FindElement2("PAGE", "num", numPage.ToString());
                    if (MyPAGE == null)
                    {
                        h = 0;
                        MyPAGE = MyDOC.AddTailElement("PAGE");
                        MyPAGE.SetAttr("num", numPage);
                        MyPAGE.SetAttr("oi_terminal", oi_terminal);
                        MyROWS = MyPAGE.AddTailElement("ROWS");

                        for (t = 0; t < 20; t++) MyROWS.AddTailElement("ROW").SetAttr("id", t);
                    }
                    else
                    {
                        MyROWS = MyPAGE.FirstChild();
                    }
                }
                else
                {
                    h++;
                }

                try
                {
                    if (!objRead.f_desdeNull && !objRead.f_hastaNull)
                    {
                        FECDESDE = objRead.f_desde.ToString("dd/MM/yyyy HH:mm");
                        FECHASTA = objRead.f_hasta.ToString("dd/MM/yyyy HH:mm");
                    }

                    //Obtengo el ROW
                    MyROW = MyROWS.FindElement2("ROW", "id", h.ToString());
                    MyROW.SetAttr("EMP", objRead.empresa.Trim());
                    MyROW.SetAttr("LEG", objRead.legajo == 0 ? "": objRead.legajo.ToString() );
                    MyROW.SetAttr("HOR", objRead.horario.Trim());
                    MyROW.SetAttr("FECDESDE", FECDESDE.Trim());
                    MyROW.SetAttr("FECHASTA", FECHASTA.Trim());
                    MyROW.SetAttr("DES", objRead.descripcion.Trim());
                    MyROW.SetAttr("OBS", objRead.observaciones.Trim());
                 
                    MyROW.SetAttr("EST", "1");
                }
                catch (Exception)
                {
                    objBatch.Err("Error en la línea " + Linea + " - " + ErrPasoDescr);
                }

            }
            //Limpiando el Archivo
            objBatch.Log("Limpiando el Archivo intermedio.");
            NomadProxy.GetProxy().FileServiceIO().SaveFile("INTERFACES", NomadProxy.GetProxy().UserName + ".intnovedades.xml", MyDOC.ToString());

            //Limpiando la Pagina
            //MyBATCH.Log("Limpiando la Pagina.");
            //while (xmlPagina != null)
              //  xmlPagina.DeleteChild(xmlPagina.FirstChild().FirstChild());
            xmlPagina = new NomadXML("<PAGE num=\"0\"/>");
            xmlPagina.FirstChild().SetAttr("num", 0);
            xmlPagina.FirstChild().SetAttr("max", MyDOC.ChildLength);

            //SincronizarPagina(ref xmlPagina);
            objBatch.Log("Importacion Finalizada.");
        }

        public static void SincronizarPagina(ref Nomad.NSystem.Proxy.NomadXML xmlPagina)
        {
            NomadXML outerPAGE = xmlPagina.FirstChild();

            int numPage = outerPAGE.GetAttrInt("num");

            NomadXML MyDOC, MyPAGE, MyROWS, MyROW, RS;//, MyDATA;
            NomadProxy MyProxy = NomadProxy.GetProxy();

            NomadXML MyLeg, MyHOR, MyEmp,MyLegReloj;
            Hashtable Horarios = new Hashtable();
            Hashtable Empresas = new Hashtable();
            Hashtable Legajos = new Hashtable();
            Hashtable LegajosReloj = new Hashtable();
            Hashtable RowsHash = new Hashtable();
            string legajo, horname,empresa,legajoReloj;

            //Obtengo el DOCUMENTO
            NomadLog.Debug("Obtengo el DOCUMENTO");
            if (MyProxy.FileServiceIO().ExistsFile("INTERFACES", MyProxy.UserName + ".intnovedades.xml"))
            {
                MyDOC = MyProxy.FileServiceIO().LoadFileXML("INTERFACES", MyProxy.UserName + ".intnovedades.xml");
            }
            else
            {
                MyDOC = new NomadXML("PAGES");
            }

            //Obtengo la PAGINA
            NomadLog.Debug("Obtengo la PAGINA");
            MyPAGE = MyDOC.FindElement2("PAGE", "num", numPage.ToString());
            if (MyPAGE == null)
            {
                MyPAGE = MyDOC.AddTailElement("PAGE");
                MyPAGE.SetAttr("num", numPage);
                MyROWS = MyPAGE.AddTailElement("ROWS");
            }
            else
            {
                MyROWS = MyPAGE.FindElement("ROWS");
            }

            //Analizo el outerPAGE
            NomadLog.Debug("Analizo el outerPAGE");
            if (outerPAGE.FindElement("ROWS") != null)
            {
                //Actualizo los ROW
                for (NomadXML cur = outerPAGE.FindElement("ROWS").FirstChild(); cur != null; cur = cur.Next())
                {
                    //Obtengo el ROW
                    MyROW = MyROWS.FindElement2("ROW", "id", cur.GetAttr("id"));
                    if (MyROW == null)
                    {
                        MyROW = MyROWS.AddTailElement("ROW");
                        MyROW.SetAttr("id", cur.GetAttr("id"));
                    }
                    MyROW.SetAttr("EMP", cur.GetAttr("EMP"));
                    MyROW.SetAttr("EMP_DESC", cur.GetAttr("EMP_DESC"));
                    MyROW.SetAttr("LEG", cur.GetAttr("LEG"));
                    MyROW.SetAttr("LEG_DESC", cur.GetAttr("LEG_DESC"));
                    MyROW.SetAttr("HOR", cur.GetAttr("HOR"));
                    MyROW.SetAttr("HOR_DESC", cur.GetAttr("HOR_DESC"));
                    MyROW.SetAttr("FECDESDE", cur.GetAttr("FECDESDE"));
                    MyROW.SetAttr("FECHASTA", cur.GetAttr("FECHASTA"));
                    MyROW.SetAttr("DES", cur.GetAttr("DES"));
                    MyROW.SetAttr("OBS", cur.GetAttr("OBS"));
                    MyROW.SetAttr("ERR", cur.GetAttr("ERR"));
                    MyROW.SetAttr("EST", cur.GetAttr("EST"));
               
                }
            }
            else
            {
                outerPAGE.SetAttr("oi_terminal", MyDOC.GetAttr("oi_terminal"));
            }

            //Actualizo los ROWS
            NomadLog.Debug("Actualizo los ROWS");
            for (MyROW = MyROWS.FirstChild(); MyROW != null; MyROW = MyROW.Next())
            {
                if (MyROW.GetAttr("EST") == "") continue;

                //Limpiar?
                if ((MyROW.GetAttr("LEG") == "") && (MyROW.GetAttr("HOR") == "") && (MyROW.GetAttr("FECDESDE") == "") && (MyROW.GetAttr("FECHASTA") == ""))// && (MyROW.GetAttr("PER") == ""))
                {
                    MyROW.SetAttr("ERR", "");
                    MyROW.SetAttr("EST", "");
                    MyROW.SetAttr("EMP", "");
                    MyROW.SetAttr("EMP_DESC", "");
                    MyROW.SetAttr("LEG", "");
                    MyROW.SetAttr("LEG_DESC", "");
                    MyROW.SetAttr("FECDESDE", "");
                    MyROW.SetAttr("FECHASTA", "");
                    MyROW.SetAttr("HOR", "");
                    MyROW.SetAttr("HOR_DESC", "");
                    MyROW.SetAttr("ERR", "");
                    MyROW.SetAttr("DES", "");
                    MyROW.SetAttr("OBS", "");
                  
                    continue;
                }

                //Actualizo los Registros
                //if (MyROW.GetAttr("EST") == "1" || MyPAGE.GetAttr("oi_terminal") != outerPAGE.GetAttr("oi_terminal")) //Modificado
                //{
                    //Limpio los Estados
                    MyROW.SetAttr("ERR", "");
                    MyROW.SetAttr("EST", "2");

                    //valido la empresa
                    MyEmp = null;
                    empresa = MyROW.GetAttr("EMP");
                    if (empresa != "")
                    {
                        if (Empresas.ContainsKey(empresa))
                        {
                            RS = (NomadXML)Empresas[empresa];
                        }
                        else
                        {
                            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla.ENTRADA.Resources.getEmpresa, MyROW);
                            Empresas.Add(empresa, RS);
                        }

                        MyEmp = RS.FirstChild();

                        if (MyEmp == null)
                        {
                            MyROW.SetAttr("ERR", "Empresa no encontrada.");
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("EMP_DESC", "Error");
                            continue;
                        }
                        else
                        {
                            MyROW.SetAttr("EMP_DESC", MyEmp.GetAttr("d_empresa"));
                            MyROW.SetAttr("oi_empresa", MyEmp.GetAttr("oi_empresa"));
                        }
                    }
                    else
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "La empresa es Requerida.");
                        MyROW.SetAttr("EMP_DESC", "Error");
                        continue;
                    }

                    //Validar el Legajo
                    MyLeg = null;
                    legajo = MyROW.GetAttr("LEG");
                    if (legajo != "")
                    {
                        if (Legajos.ContainsKey(empresa + "_" + legajo))
                        {
                            RS = (NomadXML)Legajos[empresa + "_" + legajo];
                        }
                        else
                        {
                            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla.ENTRADA.Resources.getLegajo, MyROW);
                            Legajos.Add(empresa + "_" + legajo, RS);
                        }

                        MyLeg = RS.FirstChild();

                        if (MyLeg == null)
                        {
                            MyROW.SetAttr("ERR", "Legajo no encontrado.");
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("LEG_DESC", "Error");
                            continue;
                        }
                        else
                        {
                            MyROW.SetAttr("LEG_DESC", MyLeg.GetAttr("d_ape_y_nom"));
                        }
                    }
                    else
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "El Legajo es Requerido.");
                        MyROW.SetAttr("LEG_DESC", "Error");
                        continue;
                    }

                    //Validar el Legajo reloj
                    MyLegReloj = null;
                    legajoReloj = MyLeg.GetAttr("oi_personal_emp");
                    if (legajoReloj != "")
                    {
                        if (LegajosReloj.ContainsKey(legajoReloj))
                        {
                            RS = (NomadXML)LegajosReloj[legajoReloj];
                        }
                        else
                        {
                            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla.ENTRADA.Resources.getLegajoReloj, MyLeg);
                            LegajosReloj.Add(legajoReloj, RS);
                        }

                        MyLegReloj = RS.FirstChild();

                        if (MyLegReloj == null)
                        {
                            MyROW.SetAttr("ERR", "Legajo Reloj no Encontrado.");
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("LEG_DESC", "Error");
                            continue;
                        }
                        else
                        {
                            MyROW.SetAttr("LEG_DESC", MyLegReloj.GetAttr("d_ape_y_nom"));
                        }
                    }
                    else
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "El Legajo es Requerido.");
                        MyROW.SetAttr("LEG_DESC", "Error");
                        continue;
                    }

                    //Valido la Variable
                    horname = MyROW.GetAttr("HOR");
                    if (horname == "")
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "El Tipo de Hora es REQUERIDO.");
                        MyROW.SetAttr("HOR_DESC", "Error");
                        continue;
                    }
                    else
                    {
                        if (Horarios.ContainsKey(horname))
                        {
                            RS = (NomadXML)Horarios[horname];
                        }
                        else
                        {
                            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla.ENTRADA.Resources.getHorario, MyROW);
                            Horarios.Add(horname, RS);
                        }

                        MyHOR = RS.FirstChild();

                        if (MyHOR == null)
                        {
                            MyROW.SetAttr("ERR", "Tipo de Hora no encontrado.");
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("HOR_DESC", "Error");
                            continue;
                        }
                        else
                        {
                            MyROW.SetAttr("HOR_DESC", MyHOR.GetAttr("d_tipohora"));
                            MyROW.SetAttr("oi_tipohora", MyHOR.GetAttr("oi_tipohora"));
                        }
                    }

                    //Validar la Fecha hora
                    //MyLeg = null;
                    string fechaDesde = MyROW.GetAttr("FECDESDE");
                    string fechaHasta = MyROW.GetAttr("FECHASTA");
 

                    if (fechaDesde != "" && fechaHasta != "")
                    {
                        MyROW.SetAttr("oi_personal_emp", MyLeg.GetAttr("oi_personal_emp"));
                        
                        string error = "";
                        try
                        {
                            error = ValidarNovedad(MyROW);
                        }
                        catch (NomadException e)
                        {
                            error = e.Message;
                        }

                        if (error != "")
                        {
                            MyROW.SetAttr("ERR", error);
                            MyROW.SetAttr("EST", "4");
                            continue;
                        }
                        else
                        {
                            error = ValidarContraGrilla(MyROWS, MyROW);
                            if (error != "")
                            {                               
                                MyROW.SetAttr("ERR", error);
                                MyROW.SetAttr("EST", "4");
                                continue;
                            }                            
                        }
                    }
                    else
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "La fecha y hora es requerida.");
                        continue;
                    }
           //}
            }
            //Actualizar los Child
            NomadLog.Debug("Actualizar los Child");
            while (MyROWS.ChildLength < 20) MyROWS.AddTailElement("ROW").SetAttr("id", MyROWS.ChildLength - 1);
            while (outerPAGE.FirstChild() != null) outerPAGE.DeleteChild(outerPAGE.FirstChild());
            outerPAGE.AddText(MyROWS.ToString());
            outerPAGE.SetAttr("max", MyDOC.ChildLength);

            //Guardando el Archivo
            NomadLog.Debug("Guardando el Archivo");
            MyPAGE.SetAttr("oi_terminal", outerPAGE.GetAttr("oi_terminal"));

            MyDOC.SetAttr("oi_terminal", outerPAGE.GetAttr("oi_terminal"));

            MyProxy.FileServiceIO().SaveFile("INTERFACES", MyProxy.UserName + ".intnovedades.xml", MyDOC.ToString());
        }

        private static string ValidarContraGrilla(NomadXML MyROWS, NomadXML MyROWActual)
        {
            DateTime fechaDesdeActual = Convert.ToDateTime(MyROWActual.GetAttr("FECDESDE"));
            DateTime fechaHastaActual = Convert.ToDateTime(MyROWActual.GetAttr("FECHASTA"));
            for (NomadXML MyROW = MyROWS.FirstChild(); MyROW != null; MyROW = MyROW.Next())
            {
                if (MyROW.GetAttr("id") == MyROWActual.GetAttr("id"))
                    return "";
                if (MyROW.GetAttr("EST") != "4" && MyROW.GetAttr("oi_personal_emp") == MyROWActual.GetAttr("oi_personal_emp"))
                {
                    if (MyROW.GetAttr("FECDESDE") != "" && MyROW.GetAttr("FECHASTA") != "")
                    {
                        DateTime fechaDesde = Convert.ToDateTime(MyROW.GetAttr("FECDESDE"));
                        DateTime fechaHasta = Convert.ToDateTime(MyROW.GetAttr("FECHASTA"));
                        if ((fechaDesdeActual >= fechaDesde && fechaDesdeActual < fechaHasta) ||
                            (fechaHastaActual > fechaDesde && fechaHastaActual <= fechaHasta) ||
                            (fechaDesdeActual <= fechaDesde && fechaHastaActual >= fechaHasta))
                            return "Existe un solapamiento de fechas con otras Novedades cargadas en la grilla para el Legajo";
                    }
               
                }
            }
            return "";
        }

        

        private static string ValidarNovedad(NomadXML MyROW)
        {
            string oi_personal_emp = MyROW.GetAttr("oi_personal_emp");
            DateTime f_fecha = NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDateHope(oi_personal_emp, Convert.ToDateTime(MyROW.GetAttr("FECDESDE")));
            int e_horainicio = Convert.ToInt32(Convert.ToDateTime(MyROW.GetAttr("FECDESDE")).Subtract(f_fecha).TotalMinutes);
            int e_horafin = Convert.ToInt32(Convert.ToDateTime(MyROW.GetAttr("FECHASTA")).Subtract(f_fecha).TotalMinutes);
            DateTime f_desde = f_fecha.AddMinutes(e_horainicio);
            DateTime f_hasta = f_fecha.AddMinutes(e_horafin);
            DateTime fecha_1 = ESPERANZAPER.GetDateHope(oi_personal_emp, f_desde);
            DateTime fecha_2 = ESPERANZAPER.GetDateHope(oi_personal_emp, f_hasta.AddMinutes(-1));

            //if (fecha_1 != fecha_2)
                //return "La Novedad se superpone en dos días diferentes según el horario asignado al Legajo";

            //Validar que hora inicio no sea mayor a hora fin
            if (Convert.ToInt32(f_desde.Subtract(fecha_1).TotalMinutes) > Convert.ToInt32(f_hasta.Subtract(fecha_1).TotalMinutes))
                return "La hora inicio ingresada no puede ser mayor a la hora fin";

            //Validar que el dia no este bloqueado
            if (LIQUIDACIONPERS.EnLiquidacionCerrada(oi_personal_emp, fecha_1))
                return "La fecha indicada corresponde a un procesamiento de horas cerrado";

            //validar solapamiento de licencias
            NomadXML RS =  NomadProxy.GetProxy().SQLService().GetXML(NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla.ENTRADA.Resources.getLicencias, MyROW);
            //RS = RS.FirstChild();

            DateTime f_fin_real;
            for (NomadXML MyR = RS.FirstChild(); MyR != null; MyR = MyR.Next())
            {
                string f_interrupcionNull = MyR.GetAttr("f_interrupcion");
                DateTime f_fin = MyR.GetAttrDateTime("f_fin");
                if (f_interrupcionNull == "")
                {
                    f_fin_real = f_fin;
                }
                else
                {
                    DateTime f_interrupcion = MyR.GetAttrDateTime("f_interrupcion");
                    f_fin_real = f_interrupcion;
                }
                DateTime f_inicio = MyR.GetAttrDateTime("f_inicio");

                if (fecha_1 >= f_inicio && fecha_1 <= f_fin_real)
                    return "Existe un solapamiento de fechas con Licencias cargadas para el Legajo";
            }

            RS = NomadProxy.GetProxy().SQLService().GetXML(NucleusRH.Base.Tiempos_Trabajados.NovedadesGrilla.ENTRADA.Resources.getNovedades, MyROW);
            //RS = RS.FirstChild();

            for (NomadXML MyR = RS.FirstChild(); MyR != null; MyR = MyR.Next())
            {
                DateTime f_fec = MyR.GetAttrDateTime("f_fecha");
                int e_horaini = MyR.GetAttrInt("e_horainicio");
                int e_horaf = MyR.GetAttrInt("e_horafin");

                DateTime novFecDesde = f_fec.AddMinutes(e_horaini);
                DateTime novFecHasta = f_fec.AddMinutes(e_horafin);

                if ((f_desde >= novFecDesde && f_desde < novFecHasta) ||
                  (f_hasta > novFecDesde && f_hasta <= novFecHasta) ||
                  (f_desde <= novFecDesde && f_hasta >= novFecHasta))
                    return "Existe un solapamiento de fechas con otras Novedades cargadas para el Legajo";
            }
            return "";
        }

        public static void Clean(ref Nomad.NSystem.Proxy.NomadXML xmlPagina)
        {
            NomadXML outerPAGE = xmlPagina.FirstChild();

            NomadXML MyDOC, MyPAGE, MyROWS;
            NomadProxy MyProxy = NomadProxy.GetProxy();

            //Obtengo el DOCUMENTO
            MyDOC = new NomadXML("PAGES");
            MyPAGE = MyDOC.AddTailElement("PAGE");
            MyPAGE.SetAttr("num", 0);
            MyROWS = MyPAGE.AddTailElement("ROWS");

            //Actualizar los Child
            while (MyROWS.ChildLength < 20)
            {
                MyROWS.AddTailElement("ROW").SetAttr("id", MyROWS.ChildLength - 1);
            }
            while (outerPAGE.FirstChild() != null)
            {
                outerPAGE.DeleteChild(outerPAGE.FirstChild());
            }

            outerPAGE.AddText(MyROWS.ToString());
            outerPAGE.SetAttr("max", 1);
            outerPAGE.SetAttr("num", 0);

            //Guardando el Archivo
            MyPAGE.SetAttr("oi_terminal", outerPAGE.GetAttr("oi_terminal"));

            MyDOC.SetAttr("oi_terminal", outerPAGE.GetAttr("oi_terminal"));

            MyProxy.FileServiceIO().SaveFile("INTERFACES", MyProxy.UserName + ".intnovedades.xml", MyDOC.ToString());
        }

        public static void Execute()
        {
            NomadBatch MyBATCH = NomadBatch.GetBatch("Carga Masiva", "Carga Masiva");
            MyBATCH.Log("Inicia la Carga Masiva de Novedades.");

            NomadProxy MyProxy = NomadProxy.GetProxy();
            //Obtengo el DOCUMENTO
            NomadXML MyDOC;
            MyBATCH.Log("Obtener el Archivo de Datos...");
            MyBATCH.SetMess("Obtener el Archivo de Datos...");
            if (MyProxy.FileServiceIO().ExistsFile("INTERFACES", MyProxy.UserName + ".intnovedades.xml"))
            {
                MyDOC = MyProxy.FileServiceIO().LoadFileXML("INTERFACES", MyProxy.UserName + ".intnovedades.xml");
            }
            else
            {
                MyBATCH.Err("No se encontró el archivo de datos...");
                return;
            }
            MyBATCH.SetPro(15);

            string oi_terminal = MyDOC.GetAttr("oi_terminal");
            string EstructuraOI = NomadEnvironment.QueryValue("TTA05_TERMINALES", "oi_estructura", "oi_terminal", oi_terminal, "", true);
            
            MyBATCH.Log("Analizar las novedades...");

            int Importada = 0, Errores = 0, p = 0, n = 0;
            for (NomadXML MyPAGE = MyDOC.FirstChild(); MyPAGE != null; MyPAGE = MyPAGE.Next(), n++)
            {
                NomadLog.Debug(MyPAGE.ToString());
                MyBATCH.SetMess("Analizando Pagina " + (n + 1).ToString() + " de " + MyDOC.ChildLength.ToString());

                //MyPAGE = new NomadXML(MyPAGE.ToString());
                SincronizarPagina(ref MyPAGE);

                for (NomadXML MyCUR = MyPAGE.FirstChild().FirstChild().FirstChild(); MyCUR != null; MyCUR = MyCUR.Next(), p++)
                {
                    try
                    {
                         //Hay algo cargado?
                        if (MyCUR.GetAttr("EST") == "2")
                        {
                            Tiempos_Trabajados.Personal.NOVEDAD novedad = new Personal.NOVEDAD();
                           
                            novedad.f_fecha = NucleusRH.Base.Tiempos_Trabajados.Esperanzaper.ESPERANZAPER.GetDateHope(MyCUR.GetAttr("oi_personal_emp"), Convert.ToDateTime(MyCUR.GetAttr("FECDESDE")));
                            novedad.e_horainicio = Convert.ToInt32(Convert.ToDateTime(MyCUR.GetAttr("FECDESDE")).Subtract(novedad.f_fecha).TotalMinutes);
                            novedad.e_horafin = Convert.ToInt32(Convert.ToDateTime(MyCUR.GetAttr("FECHASTA")).Subtract(novedad.f_fecha).TotalMinutes);
                            novedad.oi_personal_emp = MyCUR.GetAttrInt("oi_personal_emp");
                            novedad.oi_tipohora = MyCUR.GetAttr("oi_tipohora");
                            novedad.d_novedad = MyCUR.GetAttr("DES");
                            novedad.o_novedad = MyCUR.GetAttr("OBS");
                            novedad.oi_estructura = EstructuraOI;

                            Tiempos_Trabajados.Personal.NOVEDAD.AltaNovedad(MyCUR.GetAttr("oi_personal_emp"), novedad);
                            Importada++;
                        }
                        MyBATCH.SetPro(30, 40, MyDOC.ChildLength * 20, p);
                    }
                    catch (Exception Ex)
                    {
                        MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Problema al Leer el Registro - " + Ex.Message);
                        Errores++;
                    }
                }
            }

            MyBATCH.Log("Novedades Importadas: "+Importada);
            MyBATCH.Log("Novedades con error: "+Errores);
            MyBATCH.SetPro(100);
            MyBATCH.Log("Finalizo la Carga Masiva de Novedades.");
        }

    }
}


