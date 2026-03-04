using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarVariables(string xmlParam)
        {
        }

        public static void ImportarArchivo(Nomad.NSystem.Proxy.NomadXML xmlParam, ref Nomad.NSystem.Proxy.NomadXML xmlPagina)
        {

            NomadBatch MyBATCH = NomadBatch.GetBatch("Importar desde Archivo", "Importar desde Archivo");
            MyBATCH.Log("Inicia la Importacion.");

            NomadProxy MyProxy = NomadProxy.GetProxy();

            NomadXML outerPAGE = xmlPagina.FirstChild();
            NomadXML outerPARAM = xmlParam.FirstChild();

            NomadXML MyDOC, MyPAGE, MyROWS, MyROW;
            NomadXML RS, cur;

            //Creando el Documento.
            MyDOC = new NomadXML("PAGES");
            MyDOC.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
            MyDOC.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));

            //Analizar Separadores
            string sep = "";
            if (outerPARAM.GetAttr("modeDE") == "DE")
            {
                if (outerPARAM.GetAttrBool("sepTAB")) sep += "\t";
                if (outerPARAM.GetAttrBool("sepPYC")) sep += ";";
                if (outerPARAM.GetAttrBool("sepCOM")) sep += ",";
                if (outerPARAM.GetAttrBool("sepSPC")) sep += " ";
                if (outerPARAM.GetAttrBool("sepOTR")) sep += outerPARAM.GetAttr("txtOTR");
            }
            NomadLog.Debug("Soporte: " + sep);
            char[] csep = sep.ToCharArray();

            //Recorro los elementos
            int p = 0, h = 0, numPage, t;
            NomadXML data;
            string LEG, VAR, PER, Line, SubLine;
            double dVAL;
            int year, month, min, len, s, totCount;
            string ErrPasoDescr;
            Hashtable DNI = null;

            if (outerPARAM.GetAttr("lblLEG") == "DNI")
            {
                data = new NomadXML("row");
                data.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
                RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getLegajoDNI, data.ToString());

                DNI = new Hashtable();
                for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
                    DNI[cur.GetAttr("c_nro_documento")] = cur.GetAttr("e_numero_legajo");
            }

            do
            {
                data = new NomadXML("DATA");
                data.SetAttr("start", p + 1);
                data.SetAttr("end", p + 1000);

                RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_LINEAS, data.ToString());

                totCount = RS.GetAttrInt("count");
                for (cur = RS.FirstChild(); cur != null; cur = cur.Next())
                {

                    MyBATCH.SetMess("Procesando Linea " + (p + 1) + " de " + totCount);
                    MyBATCH.SetPro(0, 90, totCount, p);

                    ErrPasoDescr = "";
                    LEG = "";
                    VAR = "";
                    PER = "";
                    dVAL = 0;

                    if (p + 1 >= outerPARAM.GetAttrInt("modeLI"))
                    {
                        numPage = (p - h) / 20;

                        //Obtengo la PAGINA
                        ErrPasoDescr = "Obteniendo la Pagina";
                        MyPAGE = MyDOC.FindElement2("PAGE", "num", numPage.ToString());
                        if (MyPAGE == null)
                        {
                            MyPAGE = MyDOC.AddTailElement("PAGE");
                            MyPAGE.SetAttr("num", numPage);
                            MyPAGE.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
                            MyPAGE.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));
                            MyROWS = MyPAGE.AddTailElement("ROWS");

                            for (t = 0; t < 20; t++) MyROWS.AddTailElement("ROW").SetAttr("id", t);
                        }
                        else
                        {
                            MyROWS = MyPAGE.FirstChild();
                        }

                        try
                        {
                            //Linea
                            ErrPasoDescr = "Obteniendo la Linea del Archivo";
                            Line = cur.GetAttr("line");

                            if (sep == "")
                            { //ANCHO FIJO

                                ErrPasoDescr = "Obteniendo las Columnas del Archivo";
                                min = 1;
                                for (t = 1; t <= 6; t++)
                                {
                                    if (outerPARAM.GetAttr("fixCOL" + t + "st") != "")
                                        min = outerPARAM.GetAttrInt("fixCOL" + t + "st");

                                    len = 0;
                                    if (outerPARAM.GetAttr("fixCOL" + t + "ed") != "")
                                        len = outerPARAM.GetAttrInt("fixCOL" + t + "ed");

                                    if (len > 0) data.SetAttr("C" + t, Line.Substring(min - 1, len));
                                    else data.SetAttr("C" + t, "");

                                    min += len;
                                }

                            }
                            else
                            {  //DELIMITADOR

                                ErrPasoDescr = "Obteniendo las Columnas del Archivo";
                                for (t = 1; t <= 6; t++)
                                {
                                    min = -1;
                                    for (s = 0; s < csep.Length; s++)
                                    {

                                        len = Line.IndexOf(csep[s]);
                                        if (min == -1 || min > len) min = len;
                                    }

                                    if (min == -1)
                                    {
                                        SubLine = Line;
                                        Line = "";
                                    }
                                    else
                                    {
                                        SubLine = Line.Substring(0, min);
                                        Line = Line.Substring(min + 1);
                                    }
                                    data.SetAttr("C" + t, SubLine);
                                }
                            }

                            ErrPasoDescr = "Obteniendo el LEGAJO";
                            switch (outerPARAM.GetAttr("colLEG"))
                            {
                                case "NO": LEG = ""; break;
                                case "FI": LEG = outerPARAM.GetAttr("numLEG"); break;
                                default: LEG = data.GetAttr(outerPARAM.GetAttr("colLEG")); break;
                            }

                            int i = 0;
                            bool result;
                            if (result = int.TryParse(LEG, out i) == false)
                            {
                                ErrPasoDescr = "El legajo no tiene un formato numérico";
                                throw new Exception("El legajo no tiene un formato numérico");
                            }

                            ErrPasoDescr = "Obteniendo la VARIABLE";
                            switch (outerPARAM.GetAttr("colVAR"))
                            {
                                case "NO": VAR = ""; break;
                                case "FI": VAR = outerPARAM.GetAttr("txtVAR"); break;
                                default: VAR = data.GetAttr(outerPARAM.GetAttr("colVAR")); break;
                            }

                            ErrPasoDescr = "Obteniendo el VALOR";
                            switch (outerPARAM.GetAttr("colVAL"))
                            {
                                case "NO": dVAL = 0; break;
                                case "FI": dVAL = outerPARAM.GetAttrDouble("numVAL"); break;
                                default:
                                    dVAL = Nomad.NSystem.Functions.StringUtil.str2dbl(data.GetAttr(outerPARAM.GetAttr("colVAL")).Replace(",", "."));
                                    switch (outerPARAM.GetAttr("forVAL"))
                                    {
                                        case "1": dVAL = dVAL / 10; break;
                                        case "2": dVAL = dVAL / 100; break;
                                        case "3": dVAL = dVAL / 1000; break;
                                    }
                                    break;
                            }

                            ErrPasoDescr = "Obteniendo el PERIODO";
                            switch (outerPARAM.GetAttr("colPER"))
                            {
                                case "NO": PER = ""; break;
                                case "FI": PER = outerPARAM.GetAttr("numPER"); PER = PER.Substring(4, 2) + "/" + PER.Substring(0, 4); break;
                                default:
                                    year = 0; month = 0;
                                    PER = data.GetAttr(outerPARAM.GetAttr("colPER"));
                                    if (PER != "")
                                    {
                                        switch (outerPARAM.GetAttr("forPER"))
                                        {
                                            case "YYYYMM":
                                                if (PER.IndexOf("/") >= 0)
                                                {
                                                    year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('/')[0]);
                                                    month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('/')[1]);
                                                }
                                                else
                                                    if (PER.IndexOf("-") >= 0)
                                                    {
                                                        year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('-')[0]);
                                                        month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('-')[1]);
                                                    }
                                                    else
                                                        if (PER.Length == 6)
                                                        {
                                                            year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(0, 4));
                                                            month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(4, 2));
                                                        }
                                                        else
                                                            if (PER.Length == 4)
                                                            {
                                                                year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(0, 2));
                                                                month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(2, 2));
                                                            }
                                                break;

                                            case "MMYYYY":
                                                if (PER.IndexOf("/") >= 0)
                                                {
                                                    month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('/')[0]);
                                                    year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('/')[1]);
                                                }
                                                else
                                                    if (PER.IndexOf("-") >= 0)
                                                    {
                                                        month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('-')[0]);
                                                        year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Split('-')[1]);
                                                    }
                                                    else
                                                        if (PER.Length == 6)
                                                        {
                                                            month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(0, 4));
                                                            year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(4, 2));
                                                        }
                                                        else
                                                            if (PER.Length == 4)
                                                            {
                                                                month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(0, 2));
                                                                year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(PER.Substring(2, 2));
                                                            }
                                                break;
                                        }
                                    }
                                    if (month == 0)
                                        PER = "";
                                    else
                                    {
                                        if (year >= 0 && year < 50)
                                            year = 2000 + year;
                                        else
                                            if (year >= 50 && year < 100)
                                                year = 1900 + year;

                                        PER = (month + year * 100).ToString();
                                        PER = PER.Substring(4, 2) + "/" + PER.Substring(0, 4);
                                    }
                                    break;
                            }

                            if (DNI != null)
                            {
                                ErrPasoDescr = "Obteniendo el DNI";
                                if (!DNI.ContainsKey(LEG)) throw new Exception("Documento no encontrado");
                                LEG = (string)DNI[LEG];
                            }

                            //Obtengo el ROW
                            MyROW = MyROWS.FindElement2("ROW", "id", ((p - h) % 20).ToString());
                            MyROW.SetAttr("VAR", VAR.Trim());
                            MyROW.SetAttr("LEG", LEG.Trim());
                            MyROW.SetAttr("VAL", dVAL);
                            MyROW.SetAttr("PER", PER);
                            MyROW.SetAttr("EST", "1");
                        }
                        catch (Exception)
                        {
                            MyBATCH.Err("Error en la línea " + (p + 1) + " - " + ErrPasoDescr);

                            MyROW = MyROWS.FindElement2("ROW", "id", ((p - h) % 20).ToString());
                            MyROW.SetAttr("VAR", VAR.Trim());
                            MyROW.SetAttr("LEG", LEG.Trim());
                            MyROW.SetAttr("VAL", dVAL);
                            MyROW.SetAttr("PER", PER);
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("ERR", ErrPasoDescr);
                        }

                    }
                    else
                    {
                        h++;
                    }
                    p++;
                }

            } while (RS.ChildLength == 1000);

            //Limpiando el Archivo
            MyBATCH.Log("Limpiando el Archivo intermedio.");
            MyProxy.FileServiceIO().SaveFile("INTERFACES", MyProxy.UserName + ".intvar.xml", MyDOC.ToString());

            //Limpiando la Pagina
            MyBATCH.Log("Limpiando la Pagina.");
            while (outerPAGE.FirstChild() != null) outerPAGE.DeleteChild(outerPAGE.FirstChild());
            outerPAGE.SetAttr("num", 0);
            outerPAGE.SetAttr("max", MyDOC.ChildLength);

            //Sincronizar Pagina
            MyBATCH.Log("SincronicarPagina.");
            SincronizarPagina(ref xmlPagina);

            MyBATCH.Log("Importacion Finalizada.");
        }
        public static void SincronizarPagina(ref Nomad.NSystem.Proxy.NomadXML xmlPagina)
        {
            NomadXML outerPAGE = xmlPagina.FirstChild();

            int numPage = outerPAGE.GetAttrInt("num");

            NomadXML MyDOC, MyPAGE, MyROWS, MyROW, RS, MyDATA;
            NomadProxy MyProxy = NomadProxy.GetProxy();

            NomadXML MyLeg, MyVAR;
            string legajo, varname, value;

            int ivalue;
            double dValue;

            //Obtengo el DOCUMENTO
            NomadLog.Debug("Obtengo el DOCUMENTO");
            if (MyProxy.FileServiceIO().ExistsFile("INTERFACES", MyProxy.UserName + ".intvar.xml"))
            {
                MyDOC = MyProxy.FileServiceIO().LoadFileXML("INTERFACES", MyProxy.UserName + ".intvar.xml");
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

                    MyROW.SetAttr("LEG", cur.GetAttr("LEG"));
                    MyROW.SetAttr("LEG_DESC", cur.GetAttr("LEG_DESC"));
                    MyROW.SetAttr("VAR", cur.GetAttr("VAR"));
                    MyROW.SetAttr("VAR_DESC", cur.GetAttr("VAR_DESC"));
                    MyROW.SetAttr("VAL", cur.GetAttr("VAL"));
                    MyROW.SetAttr("PER", cur.GetAttr("PER"));
                    MyROW.SetAttr("ERR", cur.GetAttr("ERR"));
                    MyROW.SetAttr("EST", cur.GetAttr("EST"));
                }
            }
            else
            {
                outerPAGE.SetAttr("oi_empresa", MyDOC.GetAttr("oi_empresa"));
                outerPAGE.SetAttr("oi_liquidacion", MyDOC.GetAttr("oi_liquidacion"));
            }

            //Actualizo los ROWS
            NomadLog.Debug("Actualizo los ROWS");
            for (MyROW = MyROWS.FirstChild(); MyROW != null; MyROW = MyROW.Next())
            {
                if (MyROW.GetAttr("EST") == "") continue;

                //Limpiar?
                if ((MyROW.GetAttr("LEG") == "") && (MyROW.GetAttr("VAR") == "") && (MyROW.GetAttr("VAL") == "") && (MyROW.GetAttr("PER") == ""))
                {
                    MyROW.SetAttr("ERR", "");
                    MyROW.SetAttr("EST", "");
                    MyROW.SetAttr("LEG", "");
                    MyROW.SetAttr("LEG_DESC", "");
                    MyROW.SetAttr("VAR", "");
                    MyROW.SetAttr("VAR_DESC", "");
                    MyROW.SetAttr("VAL", "");
                    MyROW.SetAttr("PER", "");
                    MyROW.SetAttr("ERR", "");
                    continue;
                }

                //Actualizo los Registros
                if (MyROW.GetAttr("EST") == "1" || MyPAGE.GetAttr("oi_empresa") != outerPAGE.GetAttr("oi_empresa") || MyPAGE.GetAttr("oi_liquidacion") != outerPAGE.GetAttr("oi_liquidacion")) //Modificado
                {
                    //Limpio los Estados
                    MyROW.SetAttr("ERR", "");
                    MyROW.SetAttr("EST", "2");

                    //Valido la Variable
                    varname = MyROW.GetAttr("VAR");
                    if (varname == "")
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "El nombre de Variable es REQUERIDO.");
                        MyROW.SetAttr("VAR_DESC", "Error");
                        continue;
                    }
                    else
                    {
                        RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getVariable, MyROW);
                        MyVAR = RS.FirstChild();

                        if (MyVAR == null)
                        {
                            MyROW.SetAttr("ERR", "Variable no encontrada.");
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("VAR_DESC", "Error");
                            continue;
                        }
                        else
                        {
                            MyROW.SetAttr("VAR_DESC", MyVAR.GetAttr("d_variable"));
                            MyROW.SetAttr("VMD", MyVAR.GetAttr("c_tipo_variable"));
                            MyROW.SetAttr("VTY", MyVAR.GetAttr("c_tipo_dato"));
                        }
                    }
                    NomadLog.Debug(MyVAR.ToString());
                    if (MyVAR.GetAttr("used") == "")
                    {
                        MyROW.SetAttr("ERR", "La variable no es utilizada en ningun concepto activo.");
                        MyROW.SetAttr("EST", "3");
                    }

                    value = MyROW.GetAttr("VAL");
                    if (value == "")
                    {
                        MyROW.SetAttr("EST", "4");
                        MyROW.SetAttr("ERR", "El Valor es REQUERIDO.");
                        continue;
                    }

                    switch (MyVAR.GetAttr("c_tipo_dato").ToUpper())
                    {
                        case "DATETIME":
                            {
                                int year = 0, month = 0, day = 0;

                                if (value.ToUpper() == "HOY")
                                {
                                    year = System.DateTime.Today.Year;
                                    month = System.DateTime.Today.Month;
                                    day = System.DateTime.Today.Day;
                                }
                                else
                                    if ((value.IndexOf("-") >= 0) || (value.IndexOf("/") >= 0))
                                    {
                                        string[] aValue;

                                        if (value.IndexOf("-") >= 0) aValue = value.Split('-');
                                        else aValue = value.Split('/');

                                        if (aValue.Length == 3)
                                        {
                                            year = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[2]);
                                            month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[1]);
                                            day = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[0]);
                                        }
                                        else
                                            if (aValue.Length == 2)
                                            {
                                                year = System.DateTime.Today.Year;
                                                month = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[1]);
                                                day = (int)Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[0]);
                                            }
                                            else
                                                year = 0;

                                        if (month == 0 && aValue.Length >= 2)
                                        {
                                            month = "|ENE|FEB|MAR|ABR|MAY|JUN|JUL|AGO|SEP|OCT|NOV|DIC|".IndexOf("|" + aValue[0].ToUpper() + "|");
                                            if (month == -1)
                                                year = 0;
                                            else
                                                month = month / 4 + 1;
                                        }
                                    }
                                    else
                                    {
                                        ivalue = MyVAR.GetAttrInt("VAL");

                                        if (ivalue >= 010100 && ivalue <= 311299)
                                        {
                                            year = ivalue % 100; ivalue = ivalue / 100;
                                            month = ivalue % 100; ivalue = ivalue / 100;
                                            day = ivalue % 100; ivalue = ivalue / 100;
                                        }
                                        else
                                            if (ivalue >= 01011000 && ivalue <= 31129999)
                                            {
                                                year = ivalue % 10000; ivalue = ivalue / 10000;
                                                month = ivalue % 100; ivalue = ivalue / 100;
                                                day = ivalue % 100; ivalue = ivalue / 100;
                                            }
                                            else
                                                if (ivalue >= 101 && ivalue <= 3112)
                                                {
                                                    year = System.DateTime.Today.Year;
                                                    month = ivalue % 100; ivalue = ivalue / 100;
                                                    day = ivalue % 100; ivalue = ivalue / 100;
                                                }
                                                else
                                                    if (ivalue >= 1 && ivalue <= 31)
                                                    {
                                                        year = System.DateTime.Today.Year;
                                                        month = System.DateTime.Today.Month;
                                                        day = ivalue % 100; ivalue = ivalue / 100;
                                                    }
                                                    else
                                                        year = 0;
                                    }

                                if (year == 0)
                                    value = "";
                                else
                                {
                                    if (year < 50)
                                    {
                                        year += 2000;
                                    }
                                    else
                                        if (year < 100)
                                        {
                                            year += 1900;
                                        }
                                }

                                if (year != 0)
                                {
                                    if (month < 1) year = 0;
                                    if (month > 12) year = 0;
                                }

                                value = day.ToString().PadLeft(2, '0') + "/" + month.ToString().PadLeft(2, '0') + "/" + year.ToString().PadLeft(4, '0');
                            } break;

                        case "INT":
                            value = MyROW.GetAttrInt("VAL").ToString();
                            break;

                        case "DOUBLE":
                            dValue = Math.Round(MyROW.GetAttrDouble("VAL") * 100);
                            value = Math.Abs(dValue).ToString();
                            while (value.Length < 3) value = "0" + value;
                            value = (dValue < 0 ? "-" : "") + value.Substring(0, value.Length - 2) + "." + value.Substring(value.Length - 2);
                            break;

                        case "BOOL":
                            value = MyROW.GetAttrInt("VAL") == 0 ? "0" : "1";
                            break;
                    }
                    MyROW.SetAttr("VAL", value);

                    //Validar el Legajo
                    MyLeg = null;
                    legajo = MyROW.GetAttr("LEG");
                    if (legajo != "")
                    {
                        if (outerPAGE.GetAttr("oi_empresa") == "")
                        {
                            MyROW.SetAttr("EST", "4");
                            MyROW.SetAttr("ERR", "No tiene especificada la EMPRESA.");
                            MyROW.SetAttr("LEG_DESC", "Error");
                            continue;
                        }
                        else
                        {
                            MyROW.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
                            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getLegajo, MyROW);
                            MyLeg = RS.FirstChild();

                            if (MyLeg == null)
                            {
                                MyROW.SetAttr("ERR", "Legajo no encontrada.");
                                MyROW.SetAttr("EST", "4");
                                MyROW.SetAttr("LEG_DESC", "Error");
                                continue;
                            }
                            else
                            {
                                MyROW.SetAttr("LEG_DESC", MyLeg.GetAttr("d_ape_y_nom"));
                            }
                        }
                    }
                    if (legajo == "")
                    {
                        if ("|1|10|3|5|9|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0)
                        {
                            MyROW.SetAttr("ERR", "El Legajo es Requerido para este tipo de Variable.");
                            MyROW.SetAttr("EST", "4");
                            continue;
                        }
                    }
                    else
                    {
                        if ("|2|4|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0)
                        {
                            MyROW.SetAttr("ERR", "El legajo no es Valido, en Variables de Empresa.");
                            MyROW.SetAttr("EST", "4");
                            continue;
                        }
                    }

                    //Validar el Periodo
                    if (MyROW.GetAttr("PER") == "")
                    {
                        if ("|10|9|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0)
                        {
                            MyROW.SetAttr("ERR", "El periodo es Requerido para este tipo de Variable.");
                            MyROW.SetAttr("EST", "4");
                            continue;
                        }
                    }
                    else
                    {
                        if ("|1|2|3|5|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0)
                        {
                            MyROW.SetAttr("ERR", "El periodo no es Valido, para este tipo de Variables.");
                            MyROW.SetAttr("EST", "4");
                            continue;
                        }
                    }

                    //Validar el 1/2/10
                    if ((outerPAGE.GetAttr("oi_liquidacion") == "") && ("|1|2|10|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0))
                    {
                        MyROW.SetAttr("ERR", "La liquidacion es Requerido para este tipo de Variable.");
                        MyROW.SetAttr("EST", "4");
                        continue;
                    }
                    if ((outerPAGE.GetAttr("oi_liquidacion") != "") && ("|1|10|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0))
                    {
                        MyROW.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));
                        RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getLegajoLiquidacion, MyROW);
                        MyLeg = RS.FirstChild();

                        if (MyLeg == null)
                        {
                            MyROW.SetAttr("ERR", "Legajo Liquidacion no encontrado.");
                            MyROW.SetAttr("EST", "4");
                            continue;
                        }
                    }

                    if ("|6|7|8|".IndexOf("|" + MyVAR.GetAttr("c_tipo_variable") + "|") >= 0)
                    {
                        MyROW.SetAttr("ERR", "Las variables de Sistema no pueden ser Asignadas.");
                        MyROW.SetAttr("EST", "4");
                        continue;
                    }

                }
            }

            //Cargo los Valores
            NomadLog.Debug("Cargo los Valores");
            for (MyROW = MyROWS.FirstChild(); MyROW != null; MyROW = MyROW.Next())
            {
                if (MyROW.GetAttr("EST") == "") continue;

                //Obtener el Valor en DB
                MyDATA = new NomadXML("row");
                MyDATA.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
                MyDATA.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));
                MyDATA.SetAttr("LEG", MyROW.GetAttr("LEG"));
                MyDATA.SetAttr("VAR", MyROW.GetAttr("VAR"));
                MyDATA.SetAttr("VMD", MyROW.GetAttr("VMD"));
                MyDATA.SetAttr("PER", MyROW.GetAttr("PER") == "" ? "" : (MyROW.GetAttr("PER").Split('/')[1] + MyROW.GetAttr("PER").Split('/')[0]));

                //ROWs
                NomadLog.Debug("GET-VALOR:" + MyDATA.ToString());
                RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getValor, MyDATA.ToString());
                NomadLog.Debug("VALOR:" + RS.ToString());
                if (RS.GetAttr("n_valor") == "")
                {
                    value = "-";
                }
                else
                {
                    dValue = RS.GetAttrDouble("n_valor");
                    switch (MyROW.GetAttr("VTY").ToUpper())
                    {
                        case "DATETIME":
                            {
                                int year = 0, month = 0, day = 0;

                                ivalue = (int)Math.Round(dValue);

                                day = ivalue % 100; ivalue = ivalue / 100;
                                month = ivalue % 100; ivalue = ivalue / 100;
                                year = ivalue % 10000; ivalue = ivalue / 10000;

                                value = day.ToString().PadLeft(2, '0') + "/" + month.ToString().PadLeft(2, '0') + "/" + year.ToString().PadLeft(4, '0');
                            } break;

                        case "INT":
                            value = Math.Round(dValue).ToString();
                            break;

                        case "DOUBLE":
                            dValue = dValue * 100;
                            value = Math.Abs(dValue).ToString();
                            while (value.Length < 3) value = "0" + value;
                            value = (dValue < 0 ? "-" : "") + value.Substring(0, value.Length - 2) + "." + value.Substring(value.Length - 2);
                            break;

                        case "BOOL":
                            value = (dValue == 0 ? "0" : "1");
                            break;

                        default:
                            value = "?";
                            break;

                    }
                }
                MyROW.SetAttr("VAN", value);
            }

            //Actualizar los Child
            NomadLog.Debug("Actualizar los Child");
            while (MyROWS.ChildLength < 20) MyROWS.AddTailElement("ROW").SetAttr("id", MyROWS.ChildLength - 1);
            while (outerPAGE.FirstChild() != null) outerPAGE.DeleteChild(outerPAGE.FirstChild());
            outerPAGE.AddText(MyROWS.ToString());
            outerPAGE.SetAttr("max", MyDOC.ChildLength);

            //Guardando el Archivo
            NomadLog.Debug("Guardando el Archivo");
            MyPAGE.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
            MyPAGE.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));

            MyDOC.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
            MyDOC.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));
            MyProxy.FileServiceIO().SaveFile("INTERFACES", MyProxy.UserName + ".intvar.xml", MyDOC.ToString());
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
            MyPAGE.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
            MyPAGE.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));

            MyDOC.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
            MyDOC.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));
            MyProxy.FileServiceIO().SaveFile("INTERFACES", MyProxy.UserName + ".intvar.xml", MyDOC.ToString());
        }
        public static void Execute()
        {

            NomadBatch MyBATCH = NomadBatch.GetBatch("Carga Masiva", "Carga Masiva");
            MyBATCH.Log("Inicia la Carga Masiva de Variables.");

            NomadProxy MyProxy = NomadProxy.GetProxy();

            //Obtengo el DOCUMENTO
            NomadXML MyDOC;
            MyBATCH.Log("Obtener el Archivo de Datos...");
            MyBATCH.SetMess("Obtener el Archivo de Datos...");
            if (MyProxy.FileServiceIO().ExistsFile("INTERFACES", MyProxy.UserName + ".intvar.xml"))
            {
                MyDOC = MyProxy.FileServiceIO().LoadFileXML("INTERFACES", MyProxy.UserName + ".intvar.xml");
            }
            else
            {
                MyBATCH.Err("No se encontró el archivo de datos...");
                return;
            }
            MyBATCH.SetPro(15);

            NomadXML RS, ROW;

            string oi_liquidacion = MyDOC.GetAttr("oi_liquidacion");
            string oi_empresa = MyDOC.GetAttr("oi_empresa");
            string oi_personal_emp, oi_personal_liq;
            int ePeriodo;
            double dValue;
            string[] aValue;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Obtengo las Variables
            Hashtable MyVARS = new Hashtable();
            NomadXML MyVAR;

            MyBATCH.Log("Obtener el Variables...");
            MyBATCH.SetMess("Obtener el Variables...");
            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getVariables, "");
            for (ROW = RS.FirstChild(); ROW != null; ROW = ROW.Next())
                MyVARS[ROW.GetAttr("c_variable").ToUpper()] = ROW;
            MyBATCH.SetPro(20);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Obtengo las PERSONAS
            Hashtable MyPERS = new Hashtable();

            MyBATCH.Log("Obtener las Personas...");
            MyBATCH.SetMess("Obtener las Personas...");
            RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getLegajos, "<DATA oi_empresa=\"" + oi_empresa + "\" />");
            for (ROW = RS.FirstChild(); ROW != null; ROW = ROW.Next())
                MyPERS[ROW.GetAttr("e_numero_legajo")] = ROW.GetAttr("oi_personal_emp");
            MyBATCH.SetPro(25);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Obtengo las PERSONAS en la LIQUIDACION
            Hashtable MyLIQS = new Hashtable();
            if (oi_liquidacion != "")
            {
                MyBATCH.Log("Obtener las Personas de la Liquidacion...");
                MyBATCH.SetMess("Obtener las Personas de la Liquidacion...");

                RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.getLegajosLiquidacion, "<DATA oi_empresa=\"" + oi_empresa + "\" oi_liquidacion=\"" + oi_liquidacion + "\" />");
                for (ROW = RS.FirstChild(); ROW != null; ROW = ROW.Next())
                    MyLIQS[ROW.GetAttr("oi_personal_emp")] = ROW.GetAttr("oi_personal_liq");
            }
            MyBATCH.SetPro(30);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Empresa y Liquidacion
            NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION ddoL = null;
            NucleusRH.Base.Liquidacion.Liquidacion.VAL_VARGN ddoLVARGN;

            NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA ddoE = null;
            NucleusRH.Base.Liquidacion.Empresa_Liquidacion.VAL_VARGF ddoEVARGF;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Personal Liquidacion
            NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ ddoLIQ;
            NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN ddoVAREN;
            NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VARPN ddoVARPN;
            Hashtable ddoLIQS = new Hashtable();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Personal
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP ddoPER;
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREA ddoVAREA;
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREF ddoVAREF;
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VARPA ddoVARPA;
            Hashtable ddoPERS = new Hashtable();

            int TotItems = 0;
            int PosItems = 0;
            Hashtable ItemList = null;
            Hashtable RootList = new Hashtable();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Recorrer las Paginas e Incorporar las Variables.
            MyBATCH.Log("Analizar las Variables...");
            int p = 0;
            int n = 0;
            for (NomadXML MyPAGE = MyDOC.FirstChild(); MyPAGE != null; MyPAGE = MyPAGE.Next(), n++)
            {
                NomadLog.Debug(MyPAGE.ToString());
                MyBATCH.SetMess("Analizando Pagina " + (n + 1).ToString() + " de " + MyDOC.ChildLength.ToString());

                for (NomadXML MyCUR = MyPAGE.FirstChild().FirstChild(); MyCUR != null; MyCUR = MyCUR.Next(), p++)
                {
                    try
                    {
                        MyBATCH.SetPro(30, 40, MyDOC.ChildLength * 20, p);

                        //Hay algo cargado?
                        if (MyCUR.GetAttr("EST") == "") continue;

                        NomadLog.Debug(MyCUR.ToString());

                        //Existe la Variable
                        if (MyCUR.GetAttr("VAR") == "") { MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - El código de variable es requerido."); continue; }

                        //Busco la Variable
                        if (!MyVARS.ContainsKey(MyCUR.GetAttr("VAR").ToUpper())) { MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no encontrada."); continue; }
                        MyVAR = (NomadXML)MyVARS[MyCUR.GetAttr("VAR").ToUpper()];

                        //Calculo el VALOR
                        dValue = 0;
                        switch (MyVAR.GetAttr("c_tipo_dato").ToUpper())
                        {
                            case "DATETIME":
                                aValue = MyCUR.GetAttr("VAL").Split('/');
                                if (aValue.Length != 3)
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Formato de la Variable " + MyCUR.GetAttr("VAR") + " de tipo 'Fecha' es incorrecto. (dd/mm/yyyy) (" + MyCUR.GetAttr("VAL") + ")");
                                    continue;
                                }
                                dValue = Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[2]) * 10000 + Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[1]) * 100 + Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[0]);
                                break;

                            case "BOOL":
                            case "INT":
                            case "DOUBLE":
                                dValue = MyCUR.GetAttrDouble("VAL");
                                break;

                            default:
                                MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Tipo de la Variable " + MyCUR.GetAttr("VAR") + " no reconocido.");
                                continue;
                        }
                        NomadLog.Debug("Value: " + dValue.ToString());

                        //Calculo el Periodo
                        ePeriodo = 0;
                        if (MyCUR.GetAttr("PER") != "")
                        {
                            aValue = MyCUR.GetAttr("PER").Split('/');
                            if (aValue.Length != 2)
                            {
                                MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Formato de Período en la Variable " + MyCUR.GetAttr("VAR") + " es incorrecto. (mm/yyyy) (" + MyCUR.GetAttr("VAL") + ")");
                                continue;
                            }

                            ePeriodo = (int)(Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[1]) * 100 + Nomad.NSystem.Functions.StringUtil.str2dbl(aValue[0]));
                            NomadLog.Debug("Periodo: " + ePeriodo.ToString());
                        }

                        //Analizo el Tipo de Variable
                        switch (MyVAR.GetAttr("c_tipo_variable"))
                        {
                            //Novedad - Empleado
                            case "1":
                                NomadLog.Debug("Novedad - Empleado");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");
                                    continue;
                                }
                                if (oi_liquidacion == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique una liquidación.");
                                    continue;
                                }
                                if (MyCUR.GetAttr("LEG") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un legajo.");
                                    continue;
                                }
                                if (!MyPERS.ContainsKey(MyCUR.GetAttr("LEG")))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no encontrado.");
                                    continue;
                                }
                                oi_personal_emp = (string)MyPERS[MyCUR.GetAttr("LEG")];
                                NomadLog.Debug("oi_personal_emp: " + oi_personal_emp);

                                if (!MyLIQS.ContainsKey(oi_personal_emp))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no pertenece a la Liquidación.");
                                    continue;
                                }
                                oi_personal_liq = (string)MyLIQS[oi_personal_emp];
                                NomadLog.Debug("oi_personal_liq: " + oi_personal_liq);

                                if (ddoLIQS.ContainsKey(oi_personal_liq))
                                {
                                    ItemList = (Hashtable)ddoLIQS[oi_personal_liq];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    ddoLIQS[oi_personal_liq] = ItemList;
                                }

                                TotItems++;
                                ItemList["1:" + MyVAR.GetAttr("oi_variable")] = dValue;
                                break;

                            //Novedad - Periodo - Empleado
                            case "10":
                                NomadLog.Debug("Novedad - Periodo - Empleado");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");
                                    continue;
                                }
                                if (oi_liquidacion == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique una liquidación.");
                                    continue;
                                }
                                if (MyCUR.GetAttr("LEG") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un legajo.");
                                    continue;
                                }
                                if (MyCUR.GetAttr("PER") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un período.");
                                    continue;
                                }
                                if (!MyPERS.ContainsKey(MyCUR.GetAttr("LEG")))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no encontrado.");
                                    continue;
                                }
                                oi_personal_emp = (string)MyPERS[MyCUR.GetAttr("LEG")];

                                if (!MyLIQS.ContainsKey(oi_personal_emp))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no pertenece a la Liquidación.");
                                    continue;
                                }
                                oi_personal_liq = (string)MyLIQS[oi_personal_emp];

                                if (ddoLIQS.ContainsKey(oi_personal_liq))
                                {
                                    ItemList = (Hashtable)ddoLIQS[oi_personal_liq];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    ddoLIQS[oi_personal_liq] = ItemList;
                                }

                                TotItems++;
                                ItemList["10:" + MyVAR.GetAttr("oi_variable") + ":" + ePeriodo] = dValue;
                                break;

                            //Fijo - Empleado
                            case "3":
                                NomadLog.Debug("Fijo - Empleado");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                    MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");

                                if (MyCUR.GetAttr("LEG") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un legajo.");
                                    continue;
                                }
                                if (!MyPERS.ContainsKey(MyCUR.GetAttr("LEG")))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no encontrado.");
                                    continue;
                                }
                                oi_personal_emp = (string)MyPERS[MyCUR.GetAttr("LEG")];
                                NomadLog.Debug("oi_personal_emp: " + oi_personal_emp);

                                if (ddoPERS.ContainsKey(oi_personal_emp))
                                {
                                    ItemList = (Hashtable)ddoPERS[oi_personal_emp];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    ddoPERS[oi_personal_emp] = ItemList;
                                }

                                TotItems++;
                                ItemList["3:" + MyVAR.GetAttr("oi_variable") + ":" + ePeriodo] = dValue;
                                break;

                            //Acumulador - Empleado
                            case "5":
                                NomadLog.Debug("Acumulador - Empleado");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                    MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");

                                if (MyCUR.GetAttr("LEG") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un legajo.");
                                    continue;
                                }
                                if (!MyPERS.ContainsKey(MyCUR.GetAttr("LEG")))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no encontrado.");
                                    continue;
                                }
                                oi_personal_emp = (string)MyPERS[MyCUR.GetAttr("LEG")];
                                NomadLog.Debug("oi_personal_emp: " + oi_personal_emp);

                                if (ddoPERS.ContainsKey(oi_personal_emp))
                                {
                                    ItemList = (Hashtable)ddoPERS[oi_personal_emp];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    ddoPERS[oi_personal_emp] = ItemList;
                                }

                                TotItems++;
                                ItemList["5:" + MyVAR.GetAttr("oi_variable")] = dValue;
                                break;

                            //Acumulador - Periodo - Empleado
                            case "9":
                                NomadLog.Debug("Acumulador - Periodo - Empleado");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                    MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");

                                if (MyCUR.GetAttr("LEG") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un legajo.");
                                    continue;
                                }
                                if (MyCUR.GetAttr("PER") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique un periodo.");
                                    continue;
                                }
                                if (!MyPERS.ContainsKey(MyCUR.GetAttr("LEG")))
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Legajo " + MyCUR.GetAttr("LEG") + " no encontrado.");
                                    continue;
                                }
                                oi_personal_emp = (string)MyPERS[MyCUR.GetAttr("LEG")];
                                NomadLog.Debug("oi_personal_emp: " + oi_personal_emp);

                                if (ddoPERS.ContainsKey(oi_personal_emp))
                                {
                                    ItemList = (Hashtable)ddoPERS[oi_personal_emp];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    ddoPERS[oi_personal_emp] = ItemList;
                                }

                                TotItems++;
                                ItemList["9:" + MyVAR.GetAttr("oi_variable") + ":" + ePeriodo] = dValue;
                                break;

                            //Novedad - Empresa
                            case "2":
                                NomadLog.Debug("Novedad - Empresa");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");
                                    continue;
                                }
                                if (oi_liquidacion == "")
                                {
                                    MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " requiere que se especifique una liquidación.");
                                    continue;
                                }

                                //Asigno el Valor
                                if (RootList.ContainsKey("NE"))
                                {
                                    ItemList = (Hashtable)RootList["NE"];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    RootList["NE"] = ItemList;
                                }

                                TotItems++;
                                ItemList["2:" + MyVAR.GetAttr("oi_variable")] = dValue;
                                break;

                            //Fijo - Empresa
                            case "4":
                                NomadLog.Debug("Fijo - Empresa");

                                //Analizo si la variable es utilizada en el Sistema
                                if (MyVAR.GetAttr("used") == "")
                                    MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " no se utiliza en ningun concepto.");

                                //Asigno el Valor
                                if (RootList.ContainsKey("FE"))
                                {
                                    ItemList = (Hashtable)RootList["FE"];
                                }
                                else
                                {
                                    ItemList = new Hashtable();
                                    RootList["FE"] = ItemList;
                                }

                                TotItems++;
                                ItemList["4:" + MyVAR.GetAttr("oi_variable")] = dValue;
                                break;

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            // TIPOS DE VARIABLES QUE NO SE PUEDEN CARGAR
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            //Sistema
                            case "6":
                                MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " es de Sistema y no puede ser Cargada.");
                                continue;

                            //Auxiliar
                            case "7":
                                MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " es Auxiliar y no puede ser Cargada.");
                                continue;

                            //Calculada
                            case "8":
                                MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " es Calculada y no puede ser Cargada.");
                                continue;

                            //Otros Casos
                            default:
                                MyBATCH.Wrn("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Variable " + MyCUR.GetAttr("VAR") + " es de un tipo desconocido.");
                                continue;
                        }

                    }
                    catch (Exception Ex)
                    {
                        MyBATCH.Err("(Pag: " + ((p / 20) + 1).ToString() + "-Reg: " + (p % 20 + 1).ToString() + ") - Problema al Leer el Registro - " + Ex.Message);
                    }
                }
            }

            string varID, typeID;

            MyBATCH.Log("Guardar los Cambios en la Base de Datos...");
            if (RootList.ContainsKey("FE"))
            {
                try
                {
                    MyBATCH.SetMess("Guardando la Variables Fijas de la Empresa...");
                    ItemList = (Hashtable)RootList["FE"];

                    //Cargo el DDO de Empresa
                    ddoE = NucleusRH.Base.Liquidacion.Empresa_Liquidacion.EMPRESA.Get(oi_empresa);

                    //Recorre las variables
                    foreach (string MyITEM in ItemList.Keys)
                    {
                        PosItems++; MyBATCH.SetPro(40, 90, TotItems, PosItems);

                        varID = MyITEM.Split(':')[1];
                        dValue = (double)ItemList[MyITEM];

                        //Asigno el Valor
                        ddoEVARGF = (NucleusRH.Base.Liquidacion.Empresa_Liquidacion.VAL_VARGF)ddoE.VAL_VARGF.GetByAttribute("oi_variable", varID);
                        if (ddoEVARGF == null)
                        {
                            if (dValue != 0)
                            {
                                ddoEVARGF = new NucleusRH.Base.Liquidacion.Empresa_Liquidacion.VAL_VARGF();
                                ddoEVARGF.oi_variable = varID;
                                ddoEVARGF.n_valor = dValue;
                                ddoE.VAL_VARGF.Add(ddoEVARGF);
                            }
                        }
                        else
                        {
                            if (dValue != 0) ddoEVARGF.n_valor = dValue;
                            else ddoE.VAL_VARGF.Remove(ddoEVARGF);
                        }
                    }

                    //Guardar
                    NomadLog.Debug("SAVE: Empresa:" + ddoE.c_empresa);
                    NomadLog.Debug(ddoE.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().Save(ddoE);
                }
                catch (Exception E)
                {
                    MyBATCH.Err("No se pudieron Guardar las Variables Fijas de la Empresa.... - " + E.Message);
                    NomadLog.Error("ERROR", E);
                }
            }

            if (RootList.ContainsKey("NE"))
            {
                try
                {
                    MyBATCH.SetMess("Guardando la Variables de Novedad de la Liquidacion...");
                    ItemList = (Hashtable)RootList["FE"];

                    //Cargo el DDO de Empresa
                    ddoL = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(oi_liquidacion);

                    //Recorre las variables
                    foreach (string MyITEM in ItemList.Keys)
                    {
                        PosItems++; MyBATCH.SetPro(40, 90, TotItems, PosItems);

                        varID = MyITEM.Split(':')[1];
                        dValue = (double)ItemList[MyITEM];

                        //Asigno el Valor
                        ddoLVARGN = (NucleusRH.Base.Liquidacion.Liquidacion.VAL_VARGN)ddoL.VAL_VARGN.GetByAttribute("oi_variable", varID);
                        if (ddoLVARGN == null)
                        {
                            if (dValue != 0)
                            {
                                ddoLVARGN = new NucleusRH.Base.Liquidacion.Liquidacion.VAL_VARGN();
                                ddoLVARGN.oi_variable = varID;
                                ddoLVARGN.n_valor = dValue;
                                ddoL.VAL_VARGN.Add(ddoLVARGN);
                            }
                        }
                        else
                        {
                            if (dValue != 0) ddoLVARGN.n_valor = dValue;
                            else ddoL.VAL_VARGN.Remove(ddoLVARGN);
                        }
                    }

                    //Guardar
                    NomadLog.Debug("SAVE: Liquidacion:" + ddoL.c_liquidacion);
                    NomadLog.Debug(ddoL.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().Save(ddoL);
                }
                catch (Exception E)
                {
                    MyBATCH.Err("No se pudieron guardar las variables de novedad de la liquidación.... - " + E.Message);
                    NomadLog.Error("ERROR", E);
                }
            }

            string NameID;

            MyBATCH.SetMess("Guardando las Novedades del Legajo...");
            foreach (string MyID1 in ddoLIQS.Keys)
            {
                NameID = "PERLIQID:" + MyID1;
                try
                {
                    ItemList = (Hashtable)ddoLIQS[MyID1];

                    //Cargo el DDO de Empresa
                    ddoLIQ = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(MyID1);
                    NameID = ddoLIQ.descr;

                    //Recorre las variables
                    foreach (string MyITEM in ItemList.Keys)
                    {
                        PosItems++; MyBATCH.SetPro(40, 90, TotItems, PosItems);

                        typeID = MyITEM.Split(':')[0];
                        varID = MyITEM.Split(':')[1];
                        dValue = (double)ItemList[MyITEM];

                        switch (typeID)
                        {
                            case "1":
                                //Asigno el Valor
                                ddoVAREN = (NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN)ddoLIQ.VAL_VAREN.GetByAttribute("oi_variable", varID);
                                if (ddoVAREN == null)
                                {
                                    if (dValue != 0)
                                    {
                                        ddoVAREN = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN();
                                        ddoVAREN.oi_variable = varID;
                                        ddoVAREN.n_valor = dValue;
                                        ddoLIQ.VAL_VAREN.Add(ddoVAREN);
                                    }
                                }
                                else
                                {
                                    if (dValue != 0) ddoVAREN.n_valor = dValue;
                                    else ddoLIQ.VAL_VAREN.Remove(ddoVAREN);
                                }
                                break;

                            case "10":
                                ePeriodo = int.Parse(MyITEM.Split(':')[2]);

                                //Asigno el Valor
                                ddoVARPN = null;
                                foreach (NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VARPN ddoAux2 in ddoLIQ.VAL_VARPN)
                                {
                                    if ((ddoAux2.e_periodo == ePeriodo) && (ddoAux2.oi_variable == varID))
                                    {
                                        ddoVARPN = ddoAux2;
                                        break;
                                    }
                                }
                                if (ddoVARPN == null)
                                {
                                    if (dValue != 0)
                                    {
                                        ddoVARPN = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VARPN();
                                        ddoVARPN.oi_variable = varID;
                                        ddoVARPN.n_valor = dValue;
                                        ddoVARPN.e_periodo = ePeriodo;
                                        ddoLIQ.VAL_VARPN.Add(ddoVARPN);
                                    }
                                }
                                else
                                {
                                    if (dValue != 0) ddoVARPN.n_valor = dValue;
                                    else ddoLIQ.VAL_VARPN.Remove(ddoVARPN);
                                }
                                break;
                        }
                    }

                    //Guardar
                    NomadLog.Debug("SAVE: Legajo-Liquidacion:" + ddoLIQ.oi_personal_emp);
                    NomadLog.Debug(ddoLIQ.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().Save(ddoLIQ);
                }
                catch (Exception E)
                {
                    MyBATCH.Err("No se pudieron guardar las novedades de la persona " + NameID + ".... - " + E.Message);
                    NomadLog.Error("ERROR", E);
                }
            }

            MyBATCH.SetMess("Guardando las Variables Fijas y Acumuladores del Legajo...");
            foreach (string MyID2 in ddoPERS.Keys)
            {
                NameID = "PERID:" + MyID2;
                try
                {
                    ItemList = (Hashtable)ddoPERS[MyID2];

                    //Cargo el DDO de Empresa
                    ddoPER = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(MyID2);
                    NameID = ddoPER.descr;

                    //Recorre las variables
                    foreach (string MyITEM in ItemList.Keys)
                    {
                        PosItems++; MyBATCH.SetPro(40, 90, TotItems, PosItems);

                        typeID = MyITEM.Split(':')[0];
                        varID = MyITEM.Split(':')[1];
                        dValue = (double)ItemList[MyITEM];

                        switch (typeID)
                        {
                            case "3":
                                ePeriodo = int.Parse(MyITEM.Split(':')[2]);

                                //Asigno el Valor
                                ddoVAREF = (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREF)ddoPER.VAL_VAREF.GetByAttribute("oi_variable", varID);
                                if (ddoVAREF == null)
                                {
                                    if (dValue != 0)
                                    {
                                        ddoVAREF = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREF();
                                        ddoVAREF.oi_variable = varID;
                                        ddoVAREF.n_valor = dValue;
                                        if (ePeriodo != 0) ddoVAREF.e_periodo = ePeriodo;
                                        ddoPER.VAL_VAREF.Add(ddoVAREF);
                                    }
                                }
                                else
                                {
                                    if (dValue != 0)
                                    {
                                        ddoVAREF.n_valor = dValue;
                                        if (ePeriodo != 0) ddoVAREF.e_periodo = ePeriodo;
                                        else ddoVAREF.e_periodoNull = true;
                                    }
                                    else
                                        ddoPER.VAL_VAREF.Remove(ddoVAREF);
                                }
                                break;

                            case "5":
                                //Asigno el Valor
                                ddoVAREA = (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREA)ddoPER.VAL_VAREA.GetByAttribute("oi_variable", varID);
                                if (ddoVAREA == null)
                                {
                                    if (dValue != 0)
                                    {
                                        ddoVAREA = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREA();
                                        ddoVAREA.oi_variable = varID;
                                        ddoVAREA.n_valor = dValue;
                                        ddoPER.VAL_VAREA.Add(ddoVAREA);
                                    }
                                }
                                else
                                {
                                    if (dValue != 0) ddoVAREA.n_valor = dValue;
                                    else ddoPER.VAL_VAREA.Remove(ddoVAREA);
                                }
                                break;

                            case "9":
                                ePeriodo = int.Parse(MyITEM.Split(':')[2]);

                                //Asigno el Valor
                                ddoVARPA = null;
                                foreach (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VARPA ddoAux in ddoPER.VAL_VARPA)
                                {
                                    if ((ddoAux.e_periodo == ePeriodo) && (ddoAux.oi_variable == varID))
                                    {
                                        ddoVARPA = ddoAux;
                                        break;
                                    }
                                }
                                if (ddoVARPA == null)
                                {
                                    if (dValue != 0)
                                    {
                                        ddoVARPA = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VARPA();
                                        ddoVARPA.oi_variable = varID;
                                        ddoVARPA.n_valor = dValue;
                                        ddoVARPA.e_periodo = ePeriodo;
                                        ddoPER.VAL_VARPA.Add(ddoVARPA);
                                    }
                                }
                                else
                                {
                                    if (dValue != 0) ddoVARPA.n_valor = dValue;
                                    else ddoPER.VAL_VARPA.Remove(ddoVARPA);
                                }
                                break;
                        }

                    }

                    //Guardar
                    NomadLog.Debug("SAVE: Legajo:" + ddoPER.id);
                    NomadLog.Debug(ddoPER.SerializeAll());
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
                }
                catch (Exception E)
                {
                    MyBATCH.Err("No se pudieron guardar las variables de la persona " + NameID + ".... - " + E.Message);
                    NomadLog.Error("ERROR", E);
                }
            }

            MyBATCH.SetPro(100);
            MyBATCH.Log("Finalizo la Carga Masiva de Variables.");
        }

        public static void CargarVariables(string Consulta, Nomad.NSystem.Proxy.NomadXML xmlParam, ref Nomad.NSystem.Proxy.NomadXML xmlPagina)
        {
            NomadProxy MyProxy = NomadProxy.GetProxy();

            NomadXML outerPAGE = xmlPagina.FirstChild();
            NomadXML outerPARAM = xmlParam.FirstChild();
            NomadXML RS, CUR;

            NomadXML MyDOC, MyPAGE, MyROWS, MyROW;

            //Creando el Documento.
            MyDOC = new NomadXML("PAGES");
            MyDOC.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
            MyDOC.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));

            //SWITCH
            switch (Consulta)
            {
                case "LOAD_FE":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_FE, outerPARAM);
                    break;

                case "LOAD_NE":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_NE, outerPARAM);
                    break;

                case "LOAD_FL":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_FL, outerPARAM);
                    break;

                case "LOAD_AL":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_AL, outerPARAM);
                    break;

                case "LOAD_APL":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_APL, outerPARAM);
                    break;

                case "LOAD_NL":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_NL, outerPARAM);
                    break;

                case "LOAD_NLR":
                    RS = MyProxy.SQLService().GetXML(NucleusRH.Base.Liquidacion.InterfaceVariablesGenerica.ENTRADA.Resources.LOAD_NLR, outerPARAM);
                    break;

                default:
                    throw new Exception("Tipo de Consulta no Reconocida");
            }

            //Recorro los elementos
            int p = 0, numPage, t;
            for (CUR = RS.FirstChild(); CUR != null; CUR = CUR.Next(), p++)
            {
                numPage = p / 20;

                //Obtengo la PAGINA
                MyPAGE = MyDOC.FindElement2("PAGE", "num", numPage.ToString());
                if (MyPAGE == null)
                {
                    MyPAGE = MyDOC.AddTailElement("PAGE");
                    MyPAGE.SetAttr("num", numPage);
                    MyPAGE.SetAttr("oi_empresa", outerPAGE.GetAttr("oi_empresa"));
                    MyPAGE.SetAttr("oi_liquidacion", outerPAGE.GetAttr("oi_liquidacion"));
                    MyROWS = MyPAGE.AddTailElement("ROWS");

                    for (t = 0; t < 20; t++) MyROWS.AddTailElement("ROW").SetAttr("id", t);
                }
                else
                {
                    MyROWS = MyPAGE.FirstChild();
                }

                //Obtengo el ROW
                MyROW = MyROWS.FindElement2("ROW", "id", (p % 20).ToString());
                MyROW.SetAttr("VAR", CUR.GetAttr("VAR"));
                MyROW.SetAttr("LEG", CUR.GetAttr("LEG"));
                MyROW.SetAttr("VAL", CUR.GetAttr("VAL"));
                MyROW.SetAttr("PER", CUR.GetAttr("PER"));
                MyROW.SetAttr("EST", "1");
            }

            //Limpiando el Archivo
            MyProxy.FileServiceIO().SaveFile("INTERFACES", MyProxy.UserName + ".intvar.xml", MyDOC.ToString());

            //Limpiando la Pagina
            while (outerPAGE.FirstChild() != null) outerPAGE.DeleteChild(outerPAGE.FirstChild());
            outerPAGE.SetAttr("num", 0);
            outerPAGE.SetAttr("max", MyDOC.ChildLength);

            //Sincronizar Pagina
            SincronizarPagina(ref xmlPagina);

            NomadLog.Debug(xmlPagina.ToString());
        }
    }
}

