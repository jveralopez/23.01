using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Legajo_Liquidacion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Datos de Liquidación por Personal
    public partial class PERSONAL_EMP
    {
        public static void CargarVariablesFijas(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaFijasPersona(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarVarFijasCargo(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaFijasCargo(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarVarPeriodoCargo(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargarVarPeriodoCargos(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarVariablesAcumuladoras(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaAcumuladorasPersona(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarVariablesNovedades(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaNovedadPersona(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarVariablesNovedadesXVar(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaNovedadPersonaXVar(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public void ValidarCuotasAnticipos()
        {
            DateTime DateTimeNull = new DateTime(100, 1, 1);

            foreach (ANTICIPO anticipo in this.ANTICIPOS)
            {
                if (anticipo.DESC_ANTI.Count > 0)
                {
                    // Valida que la fecha de descuento de la 1er cuota sea mayor o igual
                    // al periodo en que se comienza a descontar el anticipo
                    String periodo = anticipo.e_periodo.ToString();
                    DateTime desde_periodo;
                    if (!anticipo.e_quincenaNull && anticipo.e_quincena == 2)
                    {
                        desde_periodo = new DateTime(int.Parse(periodo.Substring(0, 4)), int.Parse(periodo.Substring(4, 2)), 16);
                    }
                    else
                    {
                        desde_periodo = new DateTime(int.Parse(periodo.Substring(0, 4)), int.Parse(periodo.Substring(4, 2)), 01);
                    }
                    DESC_ANTICIPO primer_descuento = (DESC_ANTICIPO)anticipo.DESC_ANTI.GetByAttribute("e_cuota", 1);
                    if (!primer_descuento.f_descuentoNull && primer_descuento.f_descuento < desde_periodo)
                    {
                        throw new NomadAppException("Error: La fecha de descuento de la primer cuota del anticipo con fecha " + anticipo.f_anticipo.ToString("dd/MM/yyyy") + " es anterior al período en que se comienza a liquidar el anticipo");
                    }

                    // Valida que las fechas de las cuotas sean de mayor a menor
                    // Valida que si una cuota tiene fecha de descuento, todas las que siguen tambien tengan
                    NomadEnvironment.GetTrace().Info("DateTimeNull:: " + DateTimeNull.ToString());
                    DateTime f_descuento_ant = DateTimeNull;
                    foreach (DESC_ANTICIPO descuento in anticipo.DESC_ANTI)
                    {
                        if (f_descuento_ant != DateTimeNull)
                        {
                            if (descuento.f_descuentoNull)
                            {
                                throw new NomadAppException("Alguna couta del anticipo con fecha " + anticipo.f_anticipo.ToString("dd/MM/yyyy") + " no tiene cargada la fecha de descuento");
                            }
                            else if (descuento.f_descuento <= f_descuento_ant)
                            {
                                throw new NomadAppException("Las cuotas del anticipo con fecha " + anticipo.f_anticipo.ToString("dd/MM/yyyy") + " no están en orden cronológico ascendente");
                            }
                        }
                        f_descuento_ant = descuento.f_descuentoNull ? DateTimeNull : descuento.f_descuento;
                    }
                }
            }
        }

        public void RECHAZARANTICIPOS()
        {
            //Este metodo pasa todos las cuotas que esten programadas  a rechazadas
            NomadEnvironment.GetTrace().Info("Start");
            foreach (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO anticipo in this.ANTICIPOS)
            {
                NomadEnvironment.GetTrace().Info("Por Cada Anticipo");
                if (anticipo.c_estado == "R")
                {
                    NomadEnvironment.GetTrace().Info("Si esta Rechazado");
                    foreach (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.DESC_ANTICIPO desc in anticipo.DESC_ANTI)
                    {
                        NomadEnvironment.GetTrace().Info("Por Cada Cuota");
                        if (desc.c_estado == "P")
                        {
                            NomadEnvironment.GetTrace().Info("Si esta Planeado");
                            desc.c_estado = "A";
                            NomadEnvironment.GetTrace().Info("La paso a Rechazada");
                        }
                    }
                }
            }
        }

        public void CANCELARANTICIPOS()
        {
            //Este metodo pasa todos las cuotas que esten programadas  a Cancelados
            NomadEnvironment.GetTrace().Info("Start");
            foreach (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO anticipo in this.ANTICIPOS)
            {
                NomadEnvironment.GetTrace().Info("Por Cada Anticipo");
                if (anticipo.c_estado == "C")
                {
                    NomadEnvironment.GetTrace().Info("Si esta Rechazado");
                    foreach (NucleusRH.Base.Liquidacion.Legajo_Liquidacion.DESC_ANTICIPO desc in anticipo.DESC_ANTI)
                    {
                        NomadEnvironment.GetTrace().Info("Por Cada Cuota");
                        if (desc.c_estado == "P")
                        {
                            NomadEnvironment.GetTrace().Info("Si esta Planeado");
                            desc.c_estado = "C";
                            NomadEnvironment.GetTrace().Info("La paso a Rechazada");
                        }
                    }
                }
            }
        }

        public static void GuardarCargaMasiva(Nomad.NSystem.Document.NmdXmlDocument paramANTICIPOS, Nomad.NSystem.Document.NmdXmlDocument paramPER)
        {
            NomadEnvironment.GetTrace().Info("****** Agregar recursos a Proy_req ******");
            NomadEnvironment.GetTrace().Info("Parametros de Entrada1: " + paramANTICIPOS.ToString());
            NomadEnvironment.GetTrace().Info("Parametros de Entrada2: " + paramPER.ToString());

            Nomad.NSystem.Document.NmdXmlDocument xml_anticipo = (Nomad.NSystem.Document.NmdXmlDocument)paramANTICIPOS.GetFirstChildDocument();
            Nomad.NSystem.Document.NmdXmlDocument xml_personal;

            //Creo una transaction de base de datos
            Nomad.NSystem.Base.NomadTransaction transaction = NomadEnvironment.GetCurrentTransaction();

            //Creo un objeto Proyecto
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP personal_emp;

            for (xml_personal = (Nomad.NSystem.Document.NmdXmlDocument)paramPER.GetFirstChildDocument(); xml_personal != null; xml_personal = (Nomad.NSystem.Document.NmdXmlDocument)paramPER.GetNextChildDocument())
            {
                personal_emp = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(xml_personal.GetAttribute("oi_personal_emp").Value);
                try
                {
                    transaction.Begin();

                    //if ((Nomad.NSystem.Functions.StringUtil.str2date(xml_anticipo.GetAttribute("f_solicitud").Value)) > (Nomad.NSystem.Functions.StringUtil.str2date(xml_anticipo.GetAttribute("f_anticipo").Value)))
                    //throw new NomadAppException("Ocurrio superposicion de fechas");

                    NucleusRH.Base.Liquidacion.Legajo_Liquidacion.DESC_ANTICIPO new_desc_anticipo = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.DESC_ANTICIPO();
                    new_desc_anticipo.e_cuota = 1;
                    new_desc_anticipo.n_importe = Nomad.NSystem.Functions.StringUtil.str2dbl(xml_anticipo.GetAttribute("n_importe").Value);
                    new_desc_anticipo.c_estado = "P";

                    NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO new_anticipo = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO();
                    new_anticipo.oi_tipo_anticipo = xml_anticipo.GetAttribute("oi_tipo_anticipo").Value;
                    new_anticipo.f_solicitud = Nomad.NSystem.Functions.StringUtil.str2date(xml_anticipo.GetAttribute("f_solicitud").Value);
                    new_anticipo.f_anticipo = Nomad.NSystem.Functions.StringUtil.str2date(xml_anticipo.GetAttribute("f_anticipo").Value);

                    new_anticipo.n_importe = Nomad.NSystem.Functions.StringUtil.str2dbl(xml_anticipo.GetAttribute("n_importe").Value);
                    new_anticipo.e_cant_cuotas = 1;
                    if (xml_anticipo.GetAttribute("e_periodo").Value != "")
                        new_anticipo.e_periodo = int.Parse(xml_anticipo.GetAttribute("e_periodo").Value);
                    if (xml_anticipo.GetAttribute("e_quincena").Value != "")
                        new_anticipo.e_quincena = int.Parse(xml_anticipo.GetAttribute("e_quincena").Value);
                    if ((xml_anticipo.GetAttribute("l_liquida").Value) == "1")
                        new_anticipo.l_liquida = true;
                    else
                        new_anticipo.l_liquida = false;
                    new_anticipo.c_estado = "O";
                    new_anticipo.o_anticipo = "Anticipo cargado en forma masiva";

                    new_anticipo.DESC_ANTI.Add(new_desc_anticipo);
                    personal_emp.ANTICIPOS.Add(new_anticipo);
                    transaction.SaveRefresh(personal_emp);
                    transaction.Commit();
                    NomadProxy.GetProxy().Batch().Trace.Add("ifo", "Se cargo un anticipo para la persona con legajo: " + personal_emp.e_numero_legajo, "PerLegajo");
                }
                catch
                {
                    NomadEnvironment.GetTrace().Error("Existe un anticipo en la fecha solicitada para la persona: " + personal_emp.e_numero_legajo);
                    NomadProxy.GetProxy().Batch().Trace.Add("err", "Existe un anticipo en la fecha solicitada para la persona con legajo: " + personal_emp.e_numero_legajo, "PerLegajo");
                    transaction.Rollback();

                }
                //

            }
        }

        public static bool Reingresar(string id)
        {
            NomadEnvironment.GetTrace().Error("Primer línea");
            try
            {
                PERSONAL_EMP legajo = PERSONAL_EMP.Get(id);
                NomadEnvironment.GetTrace().Error("Segunda línea - id:" + id);
                legajo.l_liq_final = false;
                NomadEnvironment.GetTrace().Error("Tercer línea");
                NomadEnvironment.GetCurrentTransaction().Save(legajo);
                NomadEnvironment.GetTrace().Error("Cuarta línea");
                return true;
            }
            catch (Exception e)
            {
                NomadEnvironment.GetTrace().Error("Error al reingresar:", e);
                return false;
            }
        }

        public static void CargarVariablesPeriodo(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargarVariablesPeriodo(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarNovedadesPeriodo(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargarNovedadesPeriodo(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargarConceptosManuales(Nomad.NSystem.Document.NmdXmlDocument pobjParametros)
        {
            try
            {
                //Crea el objeto cargador y carga... así nomás.
                NucleusRH.Base.Liquidacion.clsCargaVariables objCarVar = new NucleusRH.Base.Liquidacion.clsCargaVariables();
                objCarVar.CargaConceptosManual(pobjParametros.ToString());
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error al cargar las variables. " + ex.Message);
            }
        }

        public static void CargaMasivaManual(Nomad.NSystem.Proxy.NomadXML FILTRO, Nomad.NSystem.Proxy.NomadXML DATA)
        {
            string tipo = FILTRO.FirstChild().GetAttr("tipo");
            string oi_personal_liq = "";
            string oi_cargo = "";
            string oi_variable = "";
            if (tipo == "nc")
            {
                oi_cargo = FILTRO.FirstChild().GetAttr("oi_cargo");
                oi_personal_liq = FILTRO.FirstChild().GetAttr("oi_personal_liq");
            }
            else
            {
                oi_variable = FILTRO.FirstChild().GetAttr("oi_variable");
            }

            for (NomadXML row = DATA.FirstChild().FirstChild().FirstChild(); row != null; row = row.Next())
            {
                double val_nue = row.GetAttrDouble("val");
                double val_ori = row.GetAttrDouble("orig");
                if (val_nue == val_ori) continue;

                // FIJAS
                if (tipo == "fp")
                {
                    VAL_VAREF VAR;
                    PERSONAL_EMP leg;
                    if (row.GetAttr("idvar") != "")
                    {
                        VAR = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.VAL_VAREF.Get(row.GetAttr("idvar"), false);
                        VAR.n_valor = val_nue;
                        NomadEnvironment.GetCurrentTransaction().Save(VAR);
                    }
                    else
                    {
                        leg = PERSONAL_EMP.Get(row.GetAttr("id"), false);
                        VAR = (VAL_VAREF)leg.VAL_VAREF.GetByAttribute("oi_variable", oi_variable);
                        if (VAR == null)
                        {
                            VAR = new VAL_VAREF();
                            VAR.oi_variable = oi_variable;
                            leg.VAL_VAREF.Add(VAR);
                        }
                        VAR.n_valor = val_nue;
                        NomadEnvironment.GetCurrentTransaction().Save(leg);
                    }
                }

                // NOVEDADES
                if (tipo == "np")
                {
                    NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN VAR;
                    NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ leg;
                    if (row.GetAttr("idvar") != "")
                    {
                        VAR = NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN.Get(row.GetAttr("idvar"), false);
                        VAR.n_valor = val_nue;
                        NomadEnvironment.GetCurrentTransaction().Save(VAR);
                    }
                    else
                    {
                        leg = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(row.GetAttr("id"), false);
                        VAR = (NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN)leg.VAL_VAREN.GetByAttribute("oi_variable", oi_variable);
                        if (VAR == null)
                        {
                            VAR = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAL_VAREN();
                            VAR.oi_variable = oi_variable;
                            leg.VAL_VAREN.Add(VAR);
                        }
                        VAR.n_valor = val_nue;
                        NomadEnvironment.GetCurrentTransaction().Save(leg);
                    }

                }

                //NOVEDADES POR CARGO
                if (tipo == "nc")
                {
                    NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAREN_CARGO VAREN;
                    NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ PER;

                    if (row.GetAttr("idvar") != "")
                    {
                        VAREN = NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAREN_CARGO.Get(row.GetAttr("idvar"), false);
                        VAREN.n_valor = val_nue;
                        NomadEnvironment.GetCurrentTransaction().Save(VAREN);
                    }
                    else
                    {
                        PER = NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Get(oi_personal_liq, false);
                        /*VAREN = (NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAREN_CARGO)PER.VAREN_CARGO.GetByAttribute("oi_cargo", oi_cargo);
                        if (VAREN == null)
                        {*/
                            VAREN = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.VAREN_CARGO();
                            VAREN.oi_cargo = oi_cargo;
                            VAREN.oi_variable = row.GetAttr("id");
                            //PER.VAREN_CARGO.Add(VAREN);
                        //}
                        VAREN.n_valor = val_nue;
                        PER.VAREN_CARGO.Add(VAREN);
                        NomadEnvironment.GetCurrentTransaction().Save(PER);
                    }
                }
            }
        }

        public static void InterfazSidarig(string poi_empresa, string pfile_name)
        {
            NomadProxy proxy = NomadProxy.GetProxy();

            // Comienza la transaccion
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Interfaz SIRADIG");

            objBatch.SetMess("Descomprimiendo archivos...");

            string temppath = Path.Combine("NOMAD\\TEMP", proxy.Batch().ID);

            bool resultado =  Nomad.Base.Report.GeneralReport.REPORT.UnZip(Path.Combine("NOMAD\\TEMP", pfile_name), temppath);

            if (resultado)
            {
                NomadLog.Info("temppath:: " + temppath.ToString());

                NomadXML xmlfiles = proxy.FileSystem().List(temppath);

                //ARMO UNA HASH CON LOS TIPOS DE DOCUMENTOS
                Hashtable td = new Hashtable();
                td.Add("80", "CUIT");
                td.Add("86", "CUIL");
                td.Add("87", "CDI");
                td.Add("96", "DNI");
                td.Add("89", "LC");
                td.Add("90", "LE");
                td.Add("92", "En Trámite");

                if (xmlfiles.isDocument) xmlfiles = xmlfiles.FirstChild();

                int l = xmlfiles.ChildLength;
                int c = 0;
                string cuil_actual = "";
                string paso_actual = "";
                for (NomadXML file = xmlfiles.FirstChild(); file != null; file = file.Next())
                {

                    try
                    {
                        cuil_actual = "";
                        NomadEnvironment.GetCurrentTransaction().Begin();

                        c++;
                        objBatch.SetPro(20, 80, l, c);
                        objBatch.SetMess("Recorriendo archivos: " + c + " de " + l);
                        //NomadBatch objbatch = NomadBatch.GetBatch("Interfaz SIRADIG","Siradig");
                        //objbatch.SetMess("Recorriendo los legajos.");
                        //objbatch.Log("Recorriendo los legajos.");

                        if (file.Name == "FOLDER") continue;

                        if (Path.GetExtension(file.GetAttr("NAME")) == ".xml" || Path.GetExtension(file.GetAttr("NAME")) == ".XML")
                        {

                            NomadXML xmlDATOS = proxy.FileServiceIO().LoadFileXML(Path.Combine("TEMP", proxy.Batch().ID), file.GetAttr("NAME"));
                            int periodo = xmlDATOS.FindElement("periodo").GetAttrInt("");
                            cuil_actual = xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("");

                            NomadXML xmlqry = NomadEnvironment.QueryNomadXML(PERSONAL_EMP.Resources.QRY_LEGAJO, "<PARAM oi_empresa=\"" + poi_empresa + "\" cuit=\"" + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + "\"/>");
                            NomadLog.Info("Importando cuil:: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + " - oi_personal_emp::" + xmlqry.FirstChild().GetAttr("oi_personal_emp"));
                            if (string.IsNullOrEmpty(xmlqry.FirstChild().GetAttr("oi_personal_emp")))
                            {
                                objBatch.Err("CUIT no encontrado en la empresa: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr(""));
                                continue;
                            }

                            //Recorro las dreducciones de ganancias del legajo encontrado
                            PERSONAL_EMP ddoPER = PERSONAL_EMP.Get(xmlqry.FirstChild().GetAttr("oi_personal_emp"), false);

                            ArrayList dd = new ArrayList();
                            //Recorro el child de DEDUCCIONES del legajo
                            foreach (DEDUC_IG DEDUC in ddoPER.DEDUC_IG)
                            {
                                if (DEDUC.l_interfaz && ((DEDUC.e_periodo_des / 100) == periodo))
                                    dd.Add(DEDUC);
                            }
                            foreach (DEDUC_IG DEDU in dd)
                                ddoPER.DEDUC_IG.Remove(DEDU);

                            //Recorro el child de OTROS EMPLEADOS del legajo
                            foreach (OTROS_EMP OTROS in ddoPER.OTROS_EMP)
                            {
                                dd.Clear();
                                //Recorro el child de INGRESOS MENSUALES de OTROS EMPLEADOS
                                foreach (ING_MENSUAL INGRESO in OTROS.ING_MENSUAL)
                                {
                                    if (INGRESO.l_interfaz && ((INGRESO.e_periodo / 100) == periodo))
                                        dd.Add(INGRESO);
                                }
                                foreach (ING_MENSUAL ING in dd)
                                    OTROS.ING_MENSUAL.Remove(ING);
                            }

                            //DEDUCCIONES DE FAMILIA
                            if (xmlDATOS.FindElement("cargasFamilia") != null && xmlDATOS.FindElement("cargasFamilia").FindElement("cargaFamilia") != null)
                            {
                                paso_actual = "cargasFamilia";
                                for (NomadXML xmlFAM = xmlDATOS.FindElement("cargasFamilia").FindElement("cargaFamilia"); xmlFAM != null; xmlFAM = xmlFAM.Next())
                                {
                                    DEDUC_IG ddoDG = new DEDUC_IG();
                                    if (xmlFAM.FindElement("parentesco").GetAttr("") == "1")
                                        ddoDG.oi_item_ig = "1";
                                    if (xmlFAM.FindElement("parentesco").GetAttrInt("") > 2 && xmlFAM.FindElement("parentesco").GetAttrInt("") < 31)
                                        ddoDG.oi_item_ig = "2";
                                    if (xmlFAM.FindElement("parentesco").GetAttrInt("") > 32 && xmlFAM.FindElement("parentesco").GetAttrInt("") < 100)
                                        ddoDG.oi_item_ig = "3";
                                    if (xmlFAM.FindElement("parentesco").GetAttrInt("") > 30 && xmlFAM.FindElement("parentesco").GetAttrInt("") < 33)
                                        ddoDG.oi_item_ig = "1000000";

                                    ddoDG.e_periodo_des = periodo * 100 + xmlFAM.FindElement("mesDesde").GetAttrInt("");
                                    ddoDG.e_periodo_has = periodo * 100 + xmlFAM.FindElement("mesHasta").GetAttrInt("");
                                    string tipodoc = xmlFAM.FindElement("tipoDoc").GetAttr("");
                                    if (td.ContainsKey(tipodoc))
                                        tipodoc = (string)td[tipodoc];
                                    ddoDG.c_documento = tipodoc + " " + xmlFAM.FindElement("nroDoc").GetAttr("");
                                    if (xmlFAM.FindElement("nombre") != null)
                                        ddoDG.d_denominacion = xmlFAM.FindElement("apellido").GetAttr("") + ' ' + xmlFAM.FindElement("nombre").GetAttr("");
                                    ddoDG.d_denominacion = ddoDG.d_denominacion.Substring(0, Math.Min(ddoDG.d_denominacion.Length, 100));
                                    if (xmlFAM.FindElement("nombre") != null)
                                        ddoDG.d_descripcion = xmlFAM.FindElement("apellido").GetAttr("") + ' ' + xmlFAM.FindElement("nombre").GetAttr("");
                                    ddoDG.d_descripcion = ddoDG.d_descripcion.Substring(0, Math.Min(ddoDG.d_descripcion.Length, 100));
                                    ddoDG.n_importe = 0;
                                    ddoDG.l_interfaz = true;
                                    if (xmlFAM.FindElement("porcentajeDeduccion") != null)
                                        ddoDG.e_p_deduc = int.Parse(xmlFAM.FindElement("porcentajeDeduccion").GetAttr(""));

                                    ddoPER.DEDUC_IG.Add(ddoDG);
                                }
                            }

                            //RECUPERO TODOS LOS ITEMS IG
                            //Los items de Tipo 99, tienen un motivo - concatenado con el caracter ":" --> "99:x" (se usa este string como clave)
                            NomadXML ITEMS = new NomadXML();
                            Dictionary<string, string> ii = new Dictionary<string, string>();
                            ITEMS = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Items_Deducibles_de_Ganancias.ITEM_IG.Resources.QRY_ItemsIG, "");
                            if (ITEMS.FindElement("ROWS") != null && ITEMS.FindElement("ROWS").FindElement("ROW") != null)
                            {
                                for (NomadXML xmlItem = ITEMS.FindElement("ROWS").FindElement("ROW"); xmlItem != null; xmlItem = xmlItem.Next())
                                {
                                    if (string.IsNullOrEmpty(xmlItem.GetAttr("c_interfaz"))) continue;
                                    string[] claves = xmlItem.GetAttr("c_interfaz").Split(new char[] { ',', ';', '-' });
                                    for (int x = 0; x < claves.Length; x++)
                                    {
                                        if (!ii.ContainsKey(claves[x].Trim()))
                                            ii.Add(claves[x].Trim(), xmlItem.GetAttr("oi"));
                                    }
                                }
                            }

                            //DEDUCCIONES PERSONALES
                            if (xmlDATOS.FindElement("deducciones") != null && xmlDATOS.FindElement("deducciones").FindElement("deduccion") != null)
                            {
                                paso_actual = "deducciones";
                                NomadLog.Info("paso_actual: " + paso_actual);
                                for (NomadXML xmlPER = xmlDATOS.FindElement("deducciones").FindElement("deduccion"); xmlPER != null; xmlPER = xmlPER.Next())
                                {
                                    string oiITEM = "";

                                    //Recupero el tipo de deduccion del archivo de entrada
                                    string tipodeduc = xmlPER.GetAttr("tipo");
                                    NomadLog.Info("tipodeduc: " + tipodeduc);

                                    //mensaje de error para cuando la deduccion no tiene parentesco familiar
                                    if (xmlPER.GetAttr("tipo") == "32") {
                                        if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "parentescoFamiliar") != null)
                                        {
                                            tipodeduc += ":" + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "parentescoFamiliar").GetAttr("valor");
                                        }
                                        else
                                        {
                                            objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", parentesco de deducción no encontrado para el tipo:" + tipodeduc + ". Descargue el archivo de afip actualizado para volver a importar");
                                            continue;
                                        }

                                        if (ii.ContainsKey(tipodeduc)) {
                                            oiITEM = ii[tipodeduc];
                                        } else {
                                            tipodeduc = xmlPER.GetAttr("tipo");
                                            oiITEM = ii[tipodeduc];
                                        }

                                    } else {

                                    //Si la deduccion tiene un motivo - es de tipo 99 (otras deducciones)
                                    //Recupero el motivo del archivo de entrada, si es distinto de nulo, se lo concateno al "tipo" mediante el caracter ":"
                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "motivo") != null)
                                        tipodeduc += ":" + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "motivo").GetAttr("valor");

                                    if (ii.ContainsKey(tipodeduc))
                                    {
                                        oiITEM = ii[tipodeduc];
                                        NomadLog.Info("oiITEM: " + oiITEM);
                                    }
                                }

                                    if (string.IsNullOrEmpty(oiITEM))
                                    {
                                        objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", item de deducción no encontrado:" + tipodeduc);
                                        continue;
                                    }

                                    int mes = 0;

                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "mes") != null)
                                        mes = xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "mes").GetAttrInt("valor");

                                    NomadLog.Info("tipo: " + xmlPER.GetAttr("tipo"));

                                    if (xmlPER.GetAttr("tipo") != "10" && xmlPER.GetAttr("tipo") != "9")
                                    {
                                        string c_documento = "", d_denominacion = "", d_descripcion = "", observaciones = "", motivo="";
                                        int e_p_deduc = 0;
                                        //string d_adicional = "", nroNorma = "", anioNorma = "", tipoNorma = "";

                                        if (xmlPER.GetAttr("tipo") == "99")
                                        {
                                            string tipodoc;

                                            if (xmlPER.FindElement("tipoDoc") != null)
                                            {
                                                tipodoc = xmlPER.FindElement("tipoDoc").GetAttr("");

                                                if (xmlPER.FindElement("nroDoc") != null && xmlPER.FindElement("tipodoc") != null)
                                                    c_documento = tipodoc + " " + xmlPER.FindElement("nroDoc").GetAttr("");
                                            }
                                            if (xmlPER.FindElement("denominacion") != null)
                                                d_denominacion = xmlPER.FindElement("denominacion").GetAttr("");

                                            //el motivo no puede ser nulo
                                            int cant_tipodeduc = tipodeduc.Length;
                                            if (cant_tipodeduc == 4)
                                                motivo = tipodeduc.Substring(cant_tipodeduc - 1, 1);

                                            if (motivo != "")
                                            {
                                                //Si motivo de la deduccion de tipo 99 (otras deducciones) es igual a 4 o 5 o 7 u 8 o 9, los atributos de tipoNorma, nroNorma y anioNorma son obligatorios.
                                                if (motivo == "4" || motivo == "5" || motivo == "7" || motivo == "8" || motivo == "9")
                                                {
                                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "tipoNorma") != null)
                                                    {
                                                        observaciones = "tipoNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "tipoNorma").GetAttr("valor");
                                                    }
                                                    else
                                                    {
                                                        objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", tipoNorma no especificada en deducción de tipo:" + tipodeduc);
                                                        continue;
                                                    }

                                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "nroNorma") != null)
                                                    {
                                                        if (observaciones != "")
                                                            observaciones = observaciones + " ,nroNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "nroNorma").GetAttr("valor");
                                                        else
                                                            observaciones = "nroNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "nroNorma").GetAttr("valor");
                                                    }
                                                    else
                                                    {
                                                        objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", nroNorma no especificada en deducción de tipo:" + tipodeduc);
                                                        continue;
                                                    }

                                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "anioNorma") != null)
                                                    {
                                                        if (observaciones != "")
                                                            observaciones = observaciones + ", añoNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "anioNorma").GetAttr("valor");
                                                        else
                                                            observaciones = "añoNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "anioNorma").GetAttr("valor");
                                                    }
                                                    else
                                                    {
                                                        objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", añoNorma no especificado en deducción de tipo:" + tipodeduc);
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "tipoNorma") != null)
                                                    {
                                                        observaciones = "tipoNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "tipoNorma").GetAttr("valor");
                                                    }

                                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "nroNorma") != null)
                                                    {
                                                        if (observaciones != "")
                                                            observaciones = observaciones + ", nroNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "nroNorma").GetAttr("valor");
                                                        else
                                                            observaciones = "nroNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "nroNorma").GetAttr("valor");
                                                    }

                                                    if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "anioNorma") != null)
                                                    {
                                                        if (observaciones != "")
                                                            observaciones = observaciones + ", añoNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "anioNorma").GetAttr("valor");
                                                        else
                                                            observaciones = "añoNorma: " + xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "anioNorma").GetAttr("valor");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", motivo no especificado en deducción de tipo:" + tipodeduc);
                                                continue;
                                            }

                                            if (xmlPER.FindElement("descBasica").GetAttr("") != null)
                                                d_descripcion = d_descripcion + " " + xmlPER.FindElement("descBasica").GetAttr("");

                                            if (xmlPER.FindElement("descAdicional") != null)
                                            {
                                                if (d_descripcion != "")
                                                    d_descripcion = d_descripcion + ", " + xmlPER.FindElement("descAdicional").GetAttr("");
                                                else
                                                    d_descripcion = xmlPER.FindElement("descAdicional").GetAttr("");
                                            }

                                            d_denominacion = d_denominacion.Substring(0, Math.Min(d_denominacion.Length, 100));
                                            d_descripcion = d_descripcion.Substring(0, Math.Min(d_descripcion.Length, 100));
                                            observaciones = observaciones.Substring(0, Math.Min(observaciones.Length, 1000));
                                        }
                                        else
                                        {
                                            if (xmlPER.GetAttr("tipo") == "32")
                                            {
                                                if (xmlPER.FindElement("nroDoc") != null)
                                                    c_documento = xmlPER.FindElement("nroDoc").GetAttr("");
                                                if (xmlPER.FindElement("denominacion") != null)
                                                    d_denominacion = xmlPER.FindElement("denominacion").GetAttr("");
                                                if (xmlPER.FindElement("descAdicional").GetAttr("") != null)
                                                    d_descripcion = xmlPER.FindElement("descAdicional").GetAttr("");
                                                d_denominacion = d_denominacion.Substring(0, Math.Min(d_denominacion.Length, 100));
                                                d_descripcion = d_descripcion.Substring(0, Math.Min(d_descripcion.Length, 100));

                                                //detalles de deduccion tipo 32
                                                if (xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "porcentajeDedFamiliar") != null)
                                                    e_p_deduc = xmlPER.FindElement("detalles").FindElement2("detalle", "nombre", "porcentajeDedFamiliar").GetAttrInt("valor");
                                            }
                                            else
                                            {
                                                string tipodoc = xmlPER.FindElement("tipoDoc").GetAttr("");
                                                if (td.ContainsKey(tipodoc))
                                                    tipodoc = (string)td[tipodoc];
                                                if (xmlPER.FindElement("nroDoc") != null && xmlPER.FindElement("tipodoc") != null)
                                                    c_documento = tipodoc + " " + xmlPER.FindElement("nroDoc").GetAttr("");
                                                if (xmlPER.FindElement("denominacion") != null)
                                                    d_denominacion = xmlPER.FindElement("denominacion").GetAttr("");
                                                if (xmlPER.FindElement("descBasica").GetAttr("") != null)
                                                    d_descripcion = xmlPER.FindElement("descBasica").GetAttr("");
                                                d_denominacion = d_denominacion.Substring(0, Math.Min(d_denominacion.Length, 100));
                                                d_descripcion = d_descripcion.Substring(0, Math.Min(d_descripcion.Length, 100));
                                            }

                                        }

                                        if (xmlPER.FindElement("periodos") != null && xmlPER.FindElement("periodos").FindElement("periodo") != null)
                                        {
                                            for (NomadXML xmlPERI = xmlPER.FindElement("periodos").FindElement("periodo"); xmlPERI != null; xmlPERI = xmlPERI.Next())
                                            {
                                                DEDUC_IG ddoDG = new DEDUC_IG();
                                                ddoDG.oi_item_ig = oiITEM;

                                                if (!string.IsNullOrEmpty(xmlPERI.GetAttr("mesDesde")))
                                                    ddoDG.e_periodo_des = periodo * 100 + xmlPERI.GetAttrInt("mesDesde");
                                                if (!string.IsNullOrEmpty(xmlPERI.GetAttr("mesHasta")))
                                                    ddoDG.e_periodo_has = periodo * 100 + xmlPERI.GetAttrInt("mesHasta");

                                                ddoDG.c_documento = c_documento;
                                                ddoDG.d_denominacion = d_denominacion;
                                                ddoDG.d_descripcion = d_descripcion;

                                                if (xmlPER.GetAttr("tipo") == "32")
                                                    ddoDG.e_p_deduc = e_p_deduc;

                                                ddoDG.n_importe = xmlPERI.GetAttrDouble("montoMensual");
                                                ddoDG.l_interfaz = true;

                                                ddoPER.DEDUC_IG.Add(ddoDG);
                                            }
                                        }
                                        else
                                        {
                                            if (mes == 0)
                                            {
                                                objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", mes no especificado en deducción de tipo:" + tipodeduc);
                                                continue;
                                            }
                                            if (xmlPER.FindElement("montoTotal") == null)
                                            {
                                                objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", importe no especificado en deducción de tipo:" + tipodeduc);
                                                continue;
                                            }

                                            DEDUC_IG ddoDG = new DEDUC_IG();
                                            ddoDG.oi_item_ig = oiITEM;

                                            ddoDG.e_periodo_des = periodo * 100 + mes;
                                            ddoDG.e_periodo_has = ddoDG.e_periodo_des;

                                            ddoDG.c_documento = c_documento;
                                            ddoDG.d_denominacion = d_denominacion;
                                            ddoDG.d_descripcion = d_descripcion;

                                            ddoDG.o_deduc_ig = observaciones;

                                            ddoDG.n_importe = xmlPER.FindElement("montoTotal").GetAttrDouble("");
                                            ddoDG.l_interfaz = true;

                                            ddoPER.DEDUC_IG.Add(ddoDG);
                                        }

                                    }
                                    else
                                    {//Tipo de Deducción 10 - Vehículos y tipo 9

                                        double importe;
                                        string c_documento = "", d_denominacion = "", d_descripcion = "", observaciones = "";
                                        DEDUC_IG ddoDG = new DEDUC_IG();

                                        if (xmlPER.GetAttr("tipo") == "9")
                                        {
                                            NomadLog.Info("busca tipoDoc solo para 9");
                                            if( xmlPER.FindElement("tipoDoc") != null)
                                            {
                                                string tipodoc = xmlPER.FindElement("tipoDoc").GetAttr("");
                                                if (td.ContainsKey(tipodoc))
                                                    tipodoc = (string)td[tipodoc];
                                                if (xmlPER.FindElement("nroDoc") != null && xmlPER.FindElement("tipodoc") != null)
                                                    c_documento = tipodoc + " " + xmlPER.FindElement("nroDoc").GetAttr("");

                                                ddoDG.c_documento = c_documento;
                                            }

                                        }

                                        NomadLog.Info("busca descAdic + descBasica");

                                        if (xmlPER.FindElement("descAdicional") != null)
                                            d_descripcion = xmlPER.FindElement("descAdicional").GetAttr("");
                                        if (xmlPER.FindElement("descBasica").GetAttr("") != null)
                                            d_denominacion = xmlPER.FindElement("descBasica").GetAttr("");

                                        d_denominacion = d_denominacion.Substring(0, Math.Min(d_denominacion.Length, 100));
                                        d_descripcion = d_descripcion.Substring(0, Math.Min(d_descripcion.Length, 100));
                                        importe = xmlPER.FindElement("montoTotal").GetAttrDouble("");
                                        importe = importe / 12;

                                        if (xmlPER.FindElement("detalles") != null && xmlPER.FindElement("detalles").FindElement("detalle") != null)
                                        {
                                            for (NomadXML xmlPERI = xmlPER.FindElement("detalles").FindElement("detalle"); xmlPERI != null; xmlPERI = xmlPERI.Next())
                                            {
                                                if (observaciones != "")
                                                    observaciones = observaciones + ", " + xmlPERI.GetAttr("nombre") + ": " + xmlPERI.GetAttr("valor");
                                                else
                                                    observaciones = xmlPERI.GetAttr("nombre") + ": " + xmlPERI.GetAttr("valor");
                                            }
                                        }

                                        ddoDG.oi_item_ig = oiITEM;
                                        ddoDG.n_importe = importe;
                                        ddoDG.d_descripcion = d_descripcion;
                                        ddoDG.d_denominacion = d_denominacion;
                                        ddoDG.o_deduc_ig = observaciones;
                                        ddoDG.l_interfaz = true;

                                        ddoDG.e_periodo_des = periodo * 100 + 1;
                                        ddoDG.e_periodo_has = periodo * 100 + 12;

                                        NomadLog.Info("agrega a col deduc");
                                        ddoPER.DEDUC_IG.Add(ddoDG);
                                    }
                                }
                            }

                            //PAGOS A CUENTA
                            if (xmlDATOS.FindElement("retPerPagos") != null && xmlDATOS.FindElement("retPerPagos").FindElement("retPerPago") != null)
                            {
                                paso_actual = "retPerPagos";
                                for (NomadXML xmlPAGO = xmlDATOS.FindElement("retPerPagos").FindElement("retPerPago"); xmlPAGO != null; xmlPAGO = xmlPAGO.Next())
                                {
                                    if (xmlPAGO.FindElement("periodos") != null && xmlPAGO.FindElement("periodos").FindElement("periodo") != null)
                                    {
                                        for (NomadXML xmlPERI = xmlPAGO.FindElement("periodos").FindElement("periodo"); xmlPERI != null; xmlPERI = xmlPERI.Next())
                                        {
                                            DEDUC_IG ddoDG = new DEDUC_IG();
                                            string oiITEM = "";
                                            if (ii.ContainsKey(xmlPAGO.GetAttr("tipo")))
                                            {
                                                oiITEM = ii[xmlPAGO.GetAttr("tipo")];
                                            }

                                            if (string.IsNullOrEmpty(oiITEM))
                                            {
                                                objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", item de deducción no encontrado:" + xmlPAGO.GetAttr("tipo"));
                                                continue;
                                            }

                                            ddoDG.oi_item_ig = oiITEM;
                                            if (!string.IsNullOrEmpty(xmlPERI.GetAttr("mesDesde")))
                                                ddoDG.e_periodo_des = periodo * 100 + xmlPERI.GetAttrInt("mesDesde");
                                            if (!string.IsNullOrEmpty(xmlPERI.GetAttr("mesHasta")))
                                                ddoDG.e_periodo_has = periodo * 100 + xmlPERI.GetAttrInt("mesHasta");
                                            if (xmlPAGO.GetAttr("tipo") != "6")
                                            {
                                                string tipodoc = xmlPAGO.FindElement("tipoDoc").GetAttr("");
                                                if (td.ContainsKey(tipodoc))
                                                    tipodoc = (string)td[tipodoc];
                                                if (xmlPAGO.FindElement("nroDoc") != null && xmlPAGO.FindElement("tipodoc") != null)
                                                    ddoDG.c_documento = tipodoc + " " + xmlPAGO.FindElement("nroDoc").GetAttr("");
                                            }

                                            if (xmlPAGO.FindElement("denominacion") != null && xmlPAGO.FindElement("denominacion").GetAttr("") != null)
                                            {
                                                ddoDG.d_denominacion = xmlPAGO.FindElement("denominacion").GetAttr("");
                                                ddoDG.d_denominacion = ddoDG.d_denominacion.Substring(0, Math.Min(ddoDG.d_denominacion.Length, 100));
                                            }

                                            ddoDG.d_descripcion = xmlPAGO.FindElement("descBasica").GetAttr("");
                                            ddoDG.d_descripcion = ddoDG.d_descripcion.Substring(0, Math.Min(ddoDG.d_descripcion.Length, 100));
                                            ddoDG.n_importe = xmlPERI.GetAttrDouble("montoMensual");
                                            ddoDG.l_interfaz = true;

                                            ddoPER.DEDUC_IG.Add(ddoDG);
                                        }
                                    }
                                }
                            }

                            //GANANCIAS OTROS EMPLEADORES(GOE)
                            if (xmlDATOS.FindElement("ganLiqOtrosEmpEnt") != null && xmlDATOS.FindElement("ganLiqOtrosEmpEnt").FindElement("empEnt") != null)
                            {
                                paso_actual = "ganLiqOtrosEmpEnt";
                                for (NomadXML xmlOTROS = xmlDATOS.FindElement("ganLiqOtrosEmpEnt").FindElement("empEnt"); xmlOTROS != null; xmlOTROS = xmlOTROS.Next())
                                {
                                    //Busco el empleador
                                    OTROS_EMP otros_emp = (OTROS_EMP)ddoPER.OTROS_EMP.GetByAttribute("c_cuit", xmlOTROS.FindElement("cuit").GetAttr(""));
                                    if (otros_emp == null)
                                    {
                                        otros_emp = new OTROS_EMP();
                                        otros_emp.c_cuit = xmlOTROS.FindElement("cuit").GetAttr("");
                                        otros_emp.d_denominacion = xmlOTROS.FindElement("denominacion").GetAttr("");
                                        otros_emp.d_denominacion = otros_emp.d_denominacion.Substring(0, Math.Min(otros_emp.d_denominacion.Length, 100));

                                        ddoPER.OTROS_EMP.Add(otros_emp);
                                    }
                                    if (xmlOTROS.FindElement("ingresosAportes") != null && xmlOTROS.FindElement("ingresosAportes").FindElement("ingAp") != null)
                                    {
                                        //Recorre los ingAp dentro de la colección
										for (NomadXML xmlAPORTES = xmlOTROS.FindElement("ingresosAportes").FindElement("ingAp"); xmlAPORTES != null; xmlAPORTES = xmlAPORTES.Next())
                                        {
                                            ING_MENSUAL INGRESO = new ING_MENSUAL();

                                            INGRESO.e_periodo = periodo * 100 + xmlAPORTES.GetAttrInt("mes");

                                            if (xmlAPORTES.FindElement("ganBrut") != null) 					INGRESO.n_ganbruta = xmlAPORTES.FindElement("ganBrut").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("obraSoc") != null) 					INGRESO.n_obrasocial = xmlAPORTES.FindElement("obraSoc").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("segSoc") != null) 					INGRESO.n_segsocial = xmlAPORTES.FindElement("segSoc").GetAttrDouble("");

                                            if (xmlAPORTES.FindElement("sind") != null) 					INGRESO.n_sindicato = xmlAPORTES.FindElement("sind").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("retGan") != null) 					INGRESO.n_retganancias = xmlAPORTES.FindElement("retGan").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("retribNoHab") != null) 				INGRESO.n_retribnohab = xmlAPORTES.FindElement("retribNoHab").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("ajuste") != null) 					INGRESO.n_ajuste = xmlAPORTES.FindElement("ajuste").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("exeNoAlc") != null) 				INGRESO.n_exenoalc = xmlAPORTES.FindElement("exeNoAlc").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("sac") != null) 						INGRESO.n_sac = xmlAPORTES.FindElement("sac").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("horasExtGr") != null) 				INGRESO.n_horasextgr = xmlAPORTES.FindElement("horasExtGr").GetAttrDouble("");
                                            if (xmlAPORTES.FindElement("horasExtEx") != null)				INGRESO.n_horasextex = xmlAPORTES.FindElement("horasExtEx").GetAttrDouble("");
											if (xmlAPORTES.FindElement("matDid") != null) 					INGRESO.n_matdid = xmlAPORTES.FindElement("matDid").GetAttrDouble("");
											
											//Campos versión 19
											if (xmlAPORTES.FindElement("movilidad") != null) 				INGRESO.n_movilidad = xmlAPORTES.FindElement("movilidad").GetAttrDouble("");
											if (xmlAPORTES.FindElement("viaticos") != null) 				INGRESO.n_viaticos = xmlAPORTES.FindElement("viaticos").GetAttrDouble("");
											if (xmlAPORTES.FindElement("otrosConAn") != null) 				INGRESO.n_otrosConAn = xmlAPORTES.FindElement("otrosConAn").GetAttrDouble("");
											if (xmlAPORTES.FindElement("bonosProd") != null) 				INGRESO.n_bonosProd = xmlAPORTES.FindElement("bonosProd").GetAttrDouble("");
											if (xmlAPORTES.FindElement("fallosCaja") != null) 				INGRESO.n_fallosCaja = xmlAPORTES.FindElement("fallosCaja").GetAttrDouble("");
											if (xmlAPORTES.FindElement("conSimNat") != null) 				INGRESO.n_conSimNat = xmlAPORTES.FindElement("conSimNat").GetAttrDouble("");
											if (xmlAPORTES.FindElement("remunExentaLey27549") != null) 		INGRESO.n_remunExLey27549 = xmlAPORTES.FindElement("remunExentaLey27549").GetAttrDouble("");
											if (xmlAPORTES.FindElement("suplemParticLey19101") != null) 	INGRESO.n_supPartLey19101 = xmlAPORTES.FindElement("suplemParticLey19101").GetAttrDouble("");
											if (xmlAPORTES.FindElement("teletrabajoExento") != null) 		INGRESO.n_teletrabExento = xmlAPORTES.FindElement("teletrabajoExento").GetAttrDouble("");
											
											//campos Actualización Legal Interfaz SIRADIG V 1-22-1 - año 2025											
											if (xmlAPORTES.FindElement("segSocANSES") != null) 				INGRESO.n_SegSocANSES = xmlAPORTES.FindElement("segSocANSES").GetAttrDouble("");
											if (xmlAPORTES.FindElement("segSocCajas") != null) 				INGRESO.n_segSocCajas = xmlAPORTES.FindElement("segSocCajas").GetAttrDouble("");
											if (xmlAPORTES.FindElement("ajusteRemGravadas") != null) 		INGRESO.n_ajRemGravadas = xmlAPORTES.FindElement("ajusteRemGravadas").GetAttrDouble("");
											if (xmlAPORTES.FindElement("ajusteRemExeNoAlcanzadas") != null) INGRESO.n_ajRemExeNoAlc = xmlAPORTES.FindElement("ajusteRemExeNoAlcanzadas").GetAttrDouble("");
											if (xmlAPORTES.FindElement("asignFam") != null) 				INGRESO.n_asignFam = xmlAPORTES.FindElement("asignFam").GetAttrDouble("");
											if (xmlAPORTES.FindElement("intPrestEmp") != null) 				INGRESO.n_intPrestEmp = xmlAPORTES.FindElement("intPrestEmp").GetAttrDouble("");
											if (xmlAPORTES.FindElement("indemLey4003") != null) 			INGRESO.n_indemLey4003 = xmlAPORTES.FindElement("indemLey4003").GetAttrDouble("");
											if (xmlAPORTES.FindElement("remunLey19640") != null) 			INGRESO.n_remunLey19640 = xmlAPORTES.FindElement("remunLey19640").GetAttrDouble("");
											if (xmlAPORTES.FindElement("remunCctPetro ") != null) 			INGRESO.n_remunCctPetro = xmlAPORTES.FindElement("remunCctPetro").GetAttrDouble("");
											if (xmlAPORTES.FindElement("cursosSemin") != null) 				INGRESO.n_cursosSemin = xmlAPORTES.FindElement("cursosSemin").GetAttrDouble("");											
											if (xmlAPORTES.FindElement("indumEquipEmp ") != null) 			INGRESO.n_indumEquipEmp = xmlAPORTES.FindElement("indumEquipEmp").GetAttrDouble("");											
											
											//nuevo campo agregado
											if (xmlAPORTES.FindElement("remunJudiciales ") != null) 		INGRESO.n_remunJudiciales = xmlAPORTES.FindElement("remunJudiciales").GetAttrDouble("");																						
											
                                            INGRESO.l_interfaz = true;

                                            otros_emp.ING_MENSUAL.Add(INGRESO);
                                        }
                                    }
                                }
                            }

                            //DATOS ADICIONALES
                            if (xmlDATOS.FindElement("datosAdicionales") != null && xmlDATOS.FindElement("datosAdicionales").FindElement("datoAdicional") != null)
                            {
                                string oiITEM = "";
                                if (ii.ContainsKey("999"))
                                {
                                    oiITEM = ii["999"];
                                }

                                if (string.IsNullOrEmpty(oiITEM))
                                {
                                    objBatch.Err("Error en empleado: " + xmlDATOS.FindElement("empleado").FindElement("cuit").GetAttr("") + ", item de deducción no encontrado:" + " 999");
                                    continue;
                                }

                                string fechaPresentacion = xmlDATOS.FindElement("fechaPresentacion").GetAttr("");
                                string per = fechaPresentacion.Substring(0, 4) + fechaPresentacion.Substring(5, 2);

                                NomadXML datosAdic = xmlDATOS.FindElement("datosAdicionales").FindElement("datoAdicional");
                                string nombre = datosAdic.GetAttr("nombre");
                                string valor = datosAdic.GetAttr("valor");

                                DEDUC_IG ddoDG = new DEDUC_IG();
                                ddoDG.oi_item_ig = oiITEM;
                                ddoDG.d_denominacion = nombre;
                                ddoDG.d_descripcion = valor;
                                ddoDG.n_importe = 1;
                                ddoDG.e_periodo_des = Convert.ToInt32(per);
                                ddoDG.e_periodo_has = Convert.ToInt32(per);
                                ddoDG.l_interfaz = true;

                                ddoPER.DEDUC_IG.Add(ddoDG);
                            }

                            //Agente Retencion
                            if (xmlDATOS.FindElement("agenteRetencion") != null)
                            {

                                string periodoAgent = xmlDATOS.FindElement("periodo").GetAttr("");
                                string cuit = xmlDATOS.FindElement("agenteRetencion").FindElement("cuit").GetAttr("");
                                string denominacion = xmlDATOS.FindElement("agenteRetencion").FindElement("denominacion").GetAttr("");

                                //Busco el empleador
                                OTROS_EMP otros_emp = (OTROS_EMP)ddoPER.OTROS_EMP.GetByAttribute("c_cuit", cuit);
                                if (otros_emp == null)
                                {
                                    otros_emp = new OTROS_EMP();
                                    otros_emp.c_cuit = cuit;
                                    otros_emp.d_denominacion = denominacion;
                                    otros_emp.d_denominacion = otros_emp.d_denominacion.Substring(0, Math.Min(otros_emp.d_denominacion.Length, 100));

                                    ddoPER.OTROS_EMP.Add(otros_emp);

                                }

                                ING_MENSUAL INGRESO = null;
                                foreach (ING_MENSUAL ING in otros_emp.ING_MENSUAL)
                                {
                                    if (ING.l_interfaz && ((ING.e_periodo / 100) == int.Parse(periodoAgent) ))
                                    {
                                        INGRESO = ING;
                                        break;
                                    }
                                }
                                if(INGRESO == null) INGRESO = new ING_MENSUAL();

                                INGRESO.e_periodo = int.Parse(periodoAgent) * 100 + 01;
                                INGRESO.l_interfaz = true;
                                otros_emp.ING_MENSUAL.Add(INGRESO);

                                foreach (OTROS_EMP e in ddoPER.OTROS_EMP)
                                {
                                    e.l_agente = false;
                                }
                                otros_emp.e_periodo = int.Parse(periodoAgent);
                                otros_emp.l_agente = true;

                            }

                            //GUARDO EL ddoPER
                            //NomadLog.Info("ddoPER::" + ddoPER.SerializeAll());
                            objBatch.SetMess("Guardando datos...");
                            objBatch.SetPro(80);
                            NomadEnvironment.GetCurrentTransaction().Save(ddoPER);
                            NomadEnvironment.GetCurrentTransaction().Commit();
                        }
                        else
                        {
                            objBatch.Err("Los archivos dentro del zip deben ser de formato xml");
                        }
                    }
                    catch (Exception e)
                    {
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                        objBatch.Err("Error importando CUIL: " + cuil_actual + ". Paso: " + paso_actual + " Descripción de error: " + e.Message);
                        NomadEnvironment.GetTrace().Error("Error importando CUIL: " + cuil_actual + ". Paso: " + paso_actual + " Descripción de error: " + e.Message + "\n" + e.StackTrace);
                    }
                }

            }
            else
            {
                objBatch.Err("Fallo al descomprimir el archivo .zip. Verifique que el archivo no tenga contraseña o este dañado");
            }

            objBatch.SetPro(100);
        }

        public static NucleusRH.Base.Personal.LegajoEmpresa.CARGO chgCargo(int id)
        {
            NomadEnvironment.GetTrace().Info("****** Generando nuevo cargo ******");
            NomadEnvironment.GetTrace().Info("id: " + id.ToString());

            NucleusRH.Base.Personal.LegajoEmpresa.CARGO NEWDDO;
            NucleusRH.Base.Personal.LegajoEmpresa.CARGO CARGO = NucleusRH.Base.Personal.LegajoEmpresa.CARGO.Get(id, true);

            NEWDDO = (NucleusRH.Base.Personal.LegajoEmpresa.CARGO)CARGO.Duplicate();
            NomadEnvironment.GetTrace().Info("NEWDDO: " + NEWDDO.ToString());
            NEWDDO.LICEN_CARGO.Clear();

            NEWDDO.f_egresoNull = true;
            NEWDDO.f_ingreso = DateTime.Now;
            NEWDDO.e_cargo = 0;

            return NEWDDO;

        }

        public static void setCargo(NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP DDO, NucleusRH.Base.Personal.LegajoEmpresa.CARGO NEWDDO, int id_ant)
        {
            NomadEnvironment.GetTrace().Info("****** Seteando Datos de Cambio de Cargo ******");
            NomadEnvironment.GetTrace().Info("DDO: " + DDO.ToString());
            NomadEnvironment.GetTrace().Info("NEWDDO: " + NEWDDO.ToString());
            NomadEnvironment.GetTrace().Info("id_ant: " + id_ant.ToString());

            string strStep = "";
            NomadTransaction objTran1 = new NomadTransaction();
            NomadTransaction objTran2 = new NomadTransaction();

           try
            {
                objTran1.Begin();
                strStep = "Agregando Cargo nuevo";
                DDO.CARGOS.Add(NEWDDO);
                strStep = "Persistiendo Legajo para cargo nuevo";
                objTran1.SaveRefresh(DDO);
                objTran1.Commit();
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.setCargo()", ex);
                nmdEx.SetValue("Step", strStep);

                if (objTran1 != null)
                {
                    objTran1.Rollback();

                }
                throw nmdEx;
            }

            try
            {
                objTran2.Begin();

                NucleusRH.Base.Personal.LegajoEmpresa.CARGO CARGO_NEW = (NucleusRH.Base.Personal.LegajoEmpresa.CARGO)DDO.CARGOS.GetByAttribute("e_cargo", NEWDDO.e_cargo);
                NomadEnvironment.GetTrace().Info("CARGO_NEW: " + CARGO_NEW.ToString());

                strStep = "Obteniendo Cargo anterior";
                NucleusRH.Base.Personal.LegajoEmpresa.CARGO CARGO = (NucleusRH.Base.Personal.LegajoEmpresa.CARGO)DDO.CARGOS.GetById(id_ant.ToString());
                if (CARGO.f_egresoNull)
                {
                    CARGO.f_egreso = NEWDDO.f_ingreso.AddDays(-1);
                }

                strStep = "Generando datos fijos";
                NomadObjectList VAREFS = DDO.GetChild_VAREF_CARGO(false, true);
                foreach (VAREF_CARGO VAREF in VAREFS)
                {
                    if (VAREF.oi_cargo != CARGO.Id)
                    {
                        continue;
                    }
                    else
                    {
                        VAREF_CARGO VRF = (VAREF_CARGO)VAREF.Duplicate();
                        VRF.oi_cargo = CARGO_NEW.Id;
                        DDO.VAREF_CARGO.Add(VRF);
                    }
                }

                strStep = "Generando Acumuladores de periodo";

                NomadObjectList VARPAS = DDO.GetChild_VARPA_CARGO(false, true);
                foreach (VARPA_CARGO VARPA in VARPAS)
                {
                    if (VARPA.oi_cargo != CARGO.Id)
                    {
                        continue;
                    }
                    else
                    {
                        VARPA_CARGO VRP = (VARPA_CARGO)VARPA.Duplicate();
                        VRP.oi_cargo = CARGO_NEW.Id;
                        DDO.VARPA_CARGO.Add(VRP);
                    }
                }

                strStep = "Persistiendo Legajo";
                objTran2.SaveRefresh(DDO);
                objTran2.Commit();
            }
            catch (Exception ex)
            {
                NomadException nmdEx = new NomadException("NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.setCargo()", ex);
                nmdEx.SetValue("Step", strStep);

                if (objTran2 != null)
                {
                    objTran2.Rollback();

                }
                throw nmdEx;
            }

        }

        public static List<SortedList<string, object>> GetAnticiposLeg(int PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------GET ANTICIPOS-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetAnticiposLeg.PersonalEMP: " + PAR);

            List<SortedList<string, object>> retorno = new List<SortedList<string, object>>();
            SortedList<string, object> row;
            SortedList<string, string> types = new SortedList<string,string>();
            string type = "";

            int linea;
            NomadXML myRow;

            NomadXML param = new NomadXML("PARAM");

            //Parametros para ejecutar el query
            param.SetAttr("oi_personal_emp", PAR);

            //Ejecuto el query
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.QRY_GetAnticiposLeg",param);
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontraron anticipos para el legajo con ID: " + PAR + "." : "Anticipos encontrados: " + resultado.ChildLength + "."));

            if (resultado.FirstChild() != null)
            {
                //Armo un sorted list con los tipos de datos de los parametros que me trajo el resultado
                for (int i = 0; i < resultado.Attrs.Count; i++)
                {
                    types.Add(resultado.Attrs[i].ToString(), resultado.GetAttr(resultado.Attrs[i].ToString()));
                }

                //Agrego cada uno de los anticipos al retorno
                for (linea = 1, myRow = resultado.FirstChild(); myRow != null; linea++, myRow = myRow.Next())
                {
                    row = new SortedList<string, object>();

                    for (int r = 0; r < myRow.Attrs.Count; r++)
                    {
                        //Busco de que tipo es el atributo
                        foreach (KeyValuePair<string, string> kvp in types)
                        {
                            if (kvp.Key == myRow.Attrs[r].ToString())
                            {
                                type = kvp.Value;
                                break;
                            }
                        }

                        //Agrego el atributo en base a su tipo
                        switch (type)
                        {
                            case "string":
                                row.Add(myRow.Attrs[r].ToString(), myRow.GetAttr(myRow.Attrs[r].ToString()));
                                break;
                            case "int":
                                row.Add(myRow.Attrs[r].ToString(), myRow.GetAttrInt(myRow.Attrs[r].ToString()));
                                break;
                            case "datetime":
                                row.Add(myRow.Attrs[r].ToString(), myRow.GetAttrDateTime(myRow.Attrs[r].ToString()));
                                break;
                            case "double":
                                row.Add(myRow.Attrs[r].ToString(), myRow.GetAttrDouble(myRow.Attrs[r].ToString()));
                                break;
                            case "bool":
                                row.Add(myRow.Attrs[r].ToString(), myRow.GetAttrBool(myRow.Attrs[r].ToString()));
                                break;
                            default:
                                row.Add(myRow.Attrs[r].ToString(), myRow.GetAttr(myRow.Attrs[r].ToString()));
                                break;
                        }
                        type = "";
                    }

                    //Agrego el anticipo a la lista de resultados
                    retorno.Add(row);
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Distribucion de Costos
    public partial class DISTRIB_COS
    {
        public static bool ValidarCC(string id)
        {
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("oi_centro_costo", id);
            NomadXML resultado = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Legajo_Liquidacion.DISTRIB_COS.Resources.QRY_VALIDAR, param.ToString());
            NomadEnvironment.GetTrace().Info("RESULADO VALIDACIÓN: " + resultado.ToString());

            if (resultado.FirstChild().ChildLength == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Anticipos
    public partial class ANTICIPO
    {
        public void AgregarCuotas()
        {
            int i;
            DESC_ANTICIPO descuento;
            for (i = 1; i <= this.e_cant_cuotas; i++)
            {
                descuento = new DESC_ANTICIPO();
                descuento.e_cuota = i;
                descuento.c_estado = "P";
                this.DESC_ANTI.Add(descuento);
            }
        }

        public static System.Collections.Generic.SortedList<string, object> GetAnticipoLeg(int PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("----------GET ANTICIPO LEGAJO----------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("GetAnticipoleg.Id: " + PAR);

            SortedList<string, object> retorno = new SortedList<string, object>();
            SortedList<string, string> types = new SortedList<string, string>();
            string type = "";

            NomadXML param = new NomadXML("PARAM");

            //Agrego los parametros
            param.SetAttr("oi_anticipo", PAR);

            //Ejecuto el recurso
            NomadXML resultado = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO.QRY_GetAnticipo", param.ToString());
            NomadLog.Debug((resultado.FirstChild() == null ? "No se encontró el anticipo con id: " + PAR + "." : "Anticipo encontrado."));

            if (resultado.FirstChild() != null)
            {
                //Armo una sorted list con los atributos del resultado y sus tipos
                for (int x = 0; x < resultado.Attrs.Count; x++)
                {
                    types.Add(resultado.Attrs[x].ToString(), resultado.GetAttr(resultado.Attrs[x].ToString()));
                }

                resultado = resultado.FirstChild();

                for (int r = 0; r < resultado.Attrs.Count; r++)
                {
                    //Busco de que tipo es el atributo
                    foreach (KeyValuePair<string, string> kvp in types)
                    {
                        if (kvp.Key == resultado.Attrs[r].ToString())
                        {
                            type = kvp.Value;
                            break;
                        }
                    }

                    //Agrego el atributo en base a su tipo
                    switch (type)
                    {
                        case "string":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                        case "int":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrInt(resultado.Attrs[r].ToString()));
                            break;
                        case "datetime":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDateTime(resultado.Attrs[r].ToString()));
                            break;
                        case "double":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrDouble(resultado.Attrs[r].ToString()));
                            break;
                        case "bool":
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttrBool(resultado.Attrs[r].ToString()));
                            break;
                        default:
                            retorno.Add(resultado.Attrs[r].ToString(), resultado.GetAttr(resultado.Attrs[r].ToString()));
                            break;
                    }
                    type = "";
                }
            }
            NomadLog.Debug("Retorno: " + retorno.ToString());
            return retorno;
        }

        public static System.Collections.Generic.SortedList<string,string> GuardarAnticipo( System.Collections.Generic.SortedList<string,object> ANTICIPO, string PER)
        {
            #region DEBUG
            NomadLog.Debug("-----------------------------------------");
            NomadLog.Debug("-----------GUARDAR ANTICIPO--------------");
            NomadLog.Debug("-----------------------------------------");
            foreach (KeyValuePair<string, object> kvp in ANTICIPO)
            {
                if (kvp.Value != null) { NomadLog.Debug(kvp.Key.ToString() + ":" + kvp.Value.ToString()); }
                else { NomadLog.Debug(kvp.Key.ToString() + ":Null"); }
            }

            NomadLog.Debug("AgregarAnticipo.oi_personal_emp: " + PER);
            #endregion

            SortedList<string, string> retorno = new SortedList<string, string>();
            Int32 tipo_descuento = -1, d_quincena1 = 1, d_quincena2 = 16, anio_descuento = -1, mes_descuento = -1;
            string periodo;
            try
            {
                //Get PERSONAL_EMP
                NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO ANT = new NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO();
                NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP PERSONAL_EMP = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(PER);

                if (PERSONAL_EMP == null) throw new Exception("El legajo con oi_personal_emp " + PER + " no fue encontrado");

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
                            if (kvp.Value != null) { ANT.e_cant_cuotas = Convert.ToInt32(kvp.Value); break; }
                            else { ANT.e_cant_cuotasNull = true; break; }
                        case "OBS":
                            if (kvp.Value != null) { ANT.o_anticipo = kvp.Value.ToString(); break; }
                            else { ANT.o_anticipoNull = true; break; }
                        case "PERIODO":
                            if (kvp.Value != null)
                            {
                                ANT.e_periodo = Convert.ToInt32(kvp.Value);
                                periodo = kvp.Value.ToString();
                                anio_descuento = Convert.ToInt32(periodo.Substring(0, 4));
                                mes_descuento = Convert.ToInt32(periodo.Substring(4, 2));
                            }
                            break;
                        case "QUIN":
                            if (kvp.Value != null) { ANT.e_quincena = Convert.ToInt32(kvp.Value); break; }
                            else { ANT.e_quincenaNull = true; break; }
                        /*case "LIQ":
                            ANT.l_liquida = true;
                            break;*/
                        case "TIPO_DESC":
                            if (kvp.Value != null) { tipo_descuento = Convert.ToInt32(kvp.Value); break; }
                            break;
                        case "D_QUINCENA1":
                            if (kvp.Value != null) { d_quincena1 = Convert.ToInt32(kvp.Value); break; }
                            else { }
                            break;
                        case "D_QUINCENA2":
                            if (kvp.Value != null) { d_quincena2 = Convert.ToInt32(kvp.Value); break; }
                            break;
                    }
                }

                ANT.l_liquida = true;
                ANT.f_solicitud = DateTime.Now;
                ANT.c_estado = "O";

                int i;
                //Armo el plan de descuento
                //Tipo Descuento: 1- Periodo; 2-Quincena

                double importe = ANT.n_importe / ANT.e_cant_cuotas;

                NucleusRH.Base.Liquidacion.Legajo_Liquidacion.DESC_ANTICIPO descuento;
                if(tipo_descuento != -1)
                {
                    switch (tipo_descuento)
                    {
                        case 1:
                            for (i = 1; i <= ANT.e_cant_cuotas; i++ )
                            {
                                descuento = new DESC_ANTICIPO();
                                descuento.e_cuota = i;
                                descuento.c_estado = "P";
                                if (mes_descuento == 13)
                                {
                                    mes_descuento = 1;
                                    anio_descuento += 1;
                                }
                                descuento.f_descuento = new DateTime(anio_descuento, mes_descuento, d_quincena1);
                                descuento.n_importe = importe;
                                ANT.DESC_ANTI.Add(descuento);
                                mes_descuento += 1;
                            }
                                break;
                        case 2:
                                int flag;
                                descuento = new DESC_ANTICIPO();
                                descuento.e_cuota = 1;
                                descuento.c_estado = "P";
                                descuento.n_importe = importe;
                                if(ANT.e_quincena == 1)
                                {
                                    descuento.f_descuento = new DateTime(anio_descuento, mes_descuento, d_quincena1);
                                    flag = 0;//Proxima: segunda quincena
                                }
                                else
                                {
                                    descuento.f_descuento = new DateTime(anio_descuento, mes_descuento, d_quincena2);
                                    flag = 1;//Proxima: primera quincena
                                    mes_descuento += 1;
                                }
                                ANT.DESC_ANTI.Add(descuento);

                                for (i = 2; i <= ANT.e_cant_cuotas; i++)
                                {
                                    descuento = new DESC_ANTICIPO();
                                    descuento.e_cuota = i;
                                    descuento.c_estado = "P";
                                    descuento.n_importe = importe;
                                    if (mes_descuento == 13)
                                    {
                                        mes_descuento = 1;
                                        anio_descuento += 1;
                                    }
                                    if(flag == 1)
                                    {
                                        descuento.f_descuento = new DateTime(anio_descuento, mes_descuento, d_quincena1);
                                        flag = 0;
                                    }
                                    else
                                    {
                                        descuento.f_descuento = new DateTime(anio_descuento, mes_descuento, d_quincena2);
                                        mes_descuento += 1;
                                        flag = 1;
                                    }
                                    ANT.DESC_ANTI.Add(descuento);
                                }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    throw new Exception("Error: no se pudo otorgar el anticipo ya que no se pudo calcular el plan de descuento.");
                }

                PERSONAL_EMP.ANTICIPOS.Add(ANT);

                NomadEnvironment.GetCurrentTransaction().Save(PERSONAL_EMP);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("OK","El anticipo se guardo exitosamente");
                return retorno;
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error guardando ANTICIPO: " + ex);
                retorno = NucleusRH.Base.Configuracion.Herramientas.QUERIES.CreateRTA("ERR", ex.Message);
                return retorno;
            }
        }

        public static string DelAnticipo(string PAR)
        {
            NomadLog.Debug("--------------------------------------");
            NomadLog.Debug("-----------DELETE ANTICIPO-------------");
            NomadLog.Debug("--------------------------------------");

            NomadLog.Debug("DelAnticipo.oi_anticipo: " + PAR);

            //Get PERSONAL
            //NucleusRH.Base.Personal.Legajo.FAMILIAR_PER FAM;
            NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO anticipo;
            anticipo = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.ANTICIPO.Get(PAR, true);

            if (anticipo == null) return "0";

            try
            {
                NomadEnvironment.GetCurrentTransaction().Delete(anticipo);
                return "1";
            }
            catch (Exception ex)
            {
                NomadLog.Debug("Error al eliminar el anticipo con id " + anticipo.id + ": " + ex);
                return "0";
            }
        }
    }
}


