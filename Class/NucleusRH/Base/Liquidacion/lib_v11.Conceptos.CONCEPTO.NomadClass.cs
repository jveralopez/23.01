using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Conceptos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Conceptos
    public partial class CONCEPTO
    {
    public NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO DUPLICATE(NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO paramDDO)
        {
            //Tomar un ddo existente y duplicarlo
            NomadEnvironment.GetTrace().Info("****** Duplicar un Concepto ******");
            NomadEnvironment.GetTrace().Info("****** paramDDO ******" + paramDDO.SerializeAll());

            //Creo una transaction de base de datos
            Nomad.NSystem.Base.NomadTransaction transaction = NomadEnvironment.GetCurrentTransaction();

            this.oi_grupo_tliq = paramDDO.oi_grupo_tliq;
            this.oi_clase_concepto = paramDDO.oi_clase_concepto;
            this.oi_grupo_empresa = paramDDO.oi_grupo_empresa;
            this.oi_tipo_concepto = paramDDO.oi_tipo_concepto;
            this.d_concepto = paramDDO.d_concepto;
            this.ca_acum_ganancia = paramDDO.ca_acum_ganancia;
            this.n_coef_ganancia = paramDDO.n_coef_ganancia;
            this.ca_fig_recibo = paramDDO.ca_fig_recibo;
            this.ca_tipo_asiento = paramDDO.ca_tipo_asiento;
            this.descr = paramDDO.descr;
            this.c_concepto = paramDDO.c_concepto;
            this.l_activo = paramDDO.l_activo;
            this.e_secuencia = paramDDO.e_secuencia;
            this.l_interno = paramDDO.l_interno;
            this.t_formula = paramDDO.t_formula;
            this.l_ej_ajuste = paramDDO.l_ej_ajuste;
            this.e_etapa = paramDDO.e_etapa;
            this.l_retroactivo = paramDDO.l_retroactivo;
            this.o_concepto = paramDDO.o_concepto;

            foreach (CONC_VARIABLE con_variable in paramDDO.CONC_VAR)
            {
                NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE new_CONC_VARIABLE = new NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE();
                new_CONC_VARIABLE.descr = con_variable.descr;
                new_CONC_VARIABLE.c_tipo_parametro = con_variable.c_tipo_parametro;
                new_CONC_VARIABLE.oi_concepto = con_variable.oi_concepto;
                new_CONC_VARIABLE.oi_variable = con_variable.oi_variable;
                new_CONC_VARIABLE.c_alias = con_variable.c_alias;

                this.CONC_VAR.Add(new_CONC_VARIABLE);
            }
            return this;
        }

        public static void CargarVariables(Nomad.NSystem.Proxy.NomadXML pobjParametros)
        {
            try
            {
                //Nomad.NSystem.Proxy.NomadXML pobjParametros;

                string oi_variable = pobjParametros.FirstChild().GetAttr("oi_variable").ToString();
                string tipo = pobjParametros.FirstChild().GetAttr("tipo").ToString();

                Nomad.NSystem.Proxy.NomadXML SI;
                Nomad.NSystem.Proxy.NomadXML NO;
                SI = pobjParametros.FirstChild().FindElement("SI");
                NO = pobjParametros.FirstChild().FindElement("NO");

                foreach (Nomad.NSystem.Proxy.NomadXML doc in NO.GetChilds())
                {
                    if (doc.GetAttr("selected").ToString() == "1")
                    {
                        string oi = doc.GetAttr("oi_concepto").ToString();
                        NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE concVar = new NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE();
                        concVar.c_tipo_parametro = tipo;
                        concVar.oi_variable = oi_variable;
                        NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO concepto = NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO.Get(oi);
                        concepto.CONC_VAR.Add(concVar);
                        NomadEnvironment.GetCurrentTransaction().Save(concepto);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Se produjo un error. " + ex.Message);
            }
        }

        static public void ExtraerVariables(string Path, NomadXML VARS, ArrayList lVARS)
        {
            for (NomadXML CUR = VARS.FirstChild(); CUR != null; CUR = CUR.Next())
            {
                switch (CUR.Name.ToUpper())
                {
                    case "GVAR":
                        ExtraerVariables(Path + "/" + CUR.GetAttr("des"), CUR, lVARS);
                        break;

                    case "VAR":
                        CUR.SetAttr("PATH", Path);
                        lVARS.Add(CUR);
                        break;

                    default:
                        NomadLog.Warning("ExtraerVariables - Tag " + CUR.Name + " no reconocido.");
                        break;
                }
            }
        }

        static public void ExtraerConceptos(string Path, NomadXML CONS, ArrayList lCONS)
        {
            for (NomadXML CUR = CONS.FirstChild(); CUR != null; CUR = CUR.Next())
            {
                switch (CUR.Name.ToUpper())
                {
                    case "GCON":
                        ExtraerConceptos(Path + "/" + CUR.GetAttr("des"), CUR, lCONS);
                        break;

                    case "CON1":
                    case "CON":
                        CUR.SetAttr("PATH", Path);
                        lCONS.Add(CUR);
                        break;

                    default:
                        NomadLog.Warning("ExtraerConceptos - Tag " + CUR.Name + " no reconocido.");
                        break;
                }
            }
        }

        static public void ImportarConceptos()
        {
            NomadBatch bth = NomadBatch.GetBatch("ImportarConceptos", "ImportarConceptos");

            string strFileName = NomadProxy.GetProxy().RunPath + "TEMP\\" + NomadProxy.GetProxy().Batch().ID + "-conceptos_upload.txt";

            //Obteniendo el Archivo desde el NOMAD
            bth.SetMess("Obteniendo el Archivo....");
            NomadBatch.Trace("Copiando el Archivo: NOMAD/INTERFACES/concepto_upload.txt -> " + strFileName);
            NomadProxy.GetProxy().FileServiceIO().LoadBinFile("INTERFACES", "concepto_upload.txt", strFileName);
            bth.SetPro(5);

            //ARCHIVO -> XML
            NomadBatch.Trace("Cargando el Archivo XML: " + strFileName);
            System.IO.StreamReader swrRead = new System.IO.StreamReader(strFileName);
            NomadXML intXML = new NomadXML(swrRead.ReadToEnd());
            swrRead.Close();
            bth.SetPro(10);

            //ANALIZO EL XML
            NomadBatch.Trace("Analizando el TAG Raiz de " + strFileName);
            intXML = intXML.FirstChild();
            switch (intXML.Name)
            {
                case "DATA2":
                    bth.SetSubBatch(10, 100);
                    ImportarConceptos(intXML);
                    break;

                default:
                    bth.Err("Formato de Archivo no Reconocido....");
                    break;
            }

        }

        static public void ImportarConceptos(NomadXML intXML)
        {
            NomadBatch bth = NomadBatch.GetBatch("ImportarConceptos", "ImportarConceptos");

            ////////////////////////////////////////////////////////////////////////////////
            // EXTRAER VARIABLES
            ArrayList varList = new ArrayList();
            bth.SetMess("Extraer Variables...");
            ExtraerVariables(".", intXML.FindElement("VARS"), varList);
            NomadEnvironment.GetCurrentTransaction().Begin();
            bth.SetPro(10);

            ////////////////////////////////////////////////////////////////////////////////
            // IMPORTAR VARIABLES
            string VARID;
            NucleusRH.Base.Liquidacion.Variables.VARIABLE myVAR;
            int x = 0, t = varList.Count;

            bth.SetMess("Importando Variables...");
            foreach (NomadXML VAR in varList)
            {
                //Actualizo el Progreso
                x++; bth.SetPro(10, 50, t, x);
                bth.SetMess("Importando Variables (" + x.ToString() + "/" + t.ToString() + ") ");

                NomadBatch.Trace("Buscar en LIQ09_VARIABLES c_variable=" + VAR.GetAttr("c_variable"));
                VARID = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", VAR.GetAttr("c_variable"), "", true);
                if (VARID == null)
                {
                    NomadBatch.Trace("Variable NO ENCONTRADA, Crearla....");
                    myVAR = new NucleusRH.Base.Liquidacion.Variables.VARIABLE();
                }
                else
                {
                    NomadBatch.Trace("Variable ENCONTRADA, Cargando id:" + VARID);
                    myVAR = NucleusRH.Base.Liquidacion.Variables.VARIABLE.Get(VARID);
                }

                NomadBatch.Trace("Seteando Valores...");
                myVAR.c_tipo_variable = VAR.GetAttr("c_tipo_variable");
                myVAR.d_path = VAR.GetAttr("PATH");
                myVAR.c_tipo_dato = VAR.GetAttr("c_tipo_dato");
                myVAR.c_variable = VAR.GetAttr("c_variable");
                myVAR.d_variable = VAR.GetAttr("d_variable");
                myVAR.o_variable = VAR.GetAttr("o_variable");
                if (VAR.GetAttr("n_valor") == "") myVAR.n_valorNull = true;
                else myVAR.n_valor = VAR.GetAttrDouble("n_valor");
                myVAR.l_apertura = VAR.GetAttrBool("l_apertura");
                myVAR.l_reservada = VAR.GetAttrBool("l_reservada");

                //Actualizo la Variable
                NomadBatch.Trace("Guardando...");
                NomadEnvironment.GetCurrentTransaction().Save(myVAR);
                NomadEnvironment.QueryValueChange("LIQ09_VARIABLES", "oi_variable", "c_variable", VAR.GetAttr("c_variable"), "", "1",true);
            }

            bth.SetMess("Guardando las Variables...");
            NomadEnvironment.GetCurrentTransaction().Commit();
            NomadProxy.GetProxy().CacheClear();

            ////////////////////////////////////////////////////////////////////////////////
            // EXTRAER CONCEPTOS
            ArrayList conList = new ArrayList();
            bth.SetMess("Extraer Conceptos...");
            ExtraerConceptos(".", intXML.FindElement("CONS"), conList);
            NomadEnvironment.GetCurrentTransaction().Begin();
            bth.SetPro(60);

            ////////////////////////////////////////////////////////////////////////////////
            // IMPORTAR CONCEPTOS
            NomadXML CONVAR;
            string CONID, GROUPID, TIPID;
            NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO myCON;
            NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE myCONVAR;
            x = 0; t = conList.Count;

            bth.SetMess("Importando Conceptos...");
            foreach (NomadXML CON in conList)
            {
                //Actualizo el Progreso
                x++; bth.SetPro(60, 90, t, x);
                bth.SetMess("Importando Conceptos (" + x.ToString() + "/" + t.ToString() + ") ");

                NomadBatch.Trace("Buscar en LIQ14_CONCEPTOS c_concepto=" + CON.GetAttr("c_concepto"));
                CONID = NomadEnvironment.QueryValue("LIQ14_CONCEPTOS", "oi_concepto", "c_concepto", CON.GetAttr("c_concepto"), "", true);
                if (CONID == null)
                {
                    NomadBatch.Trace("Concepto NO ENCONTRADO, Crearlo....");
                    myCON = new NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO();
                }
                else
                {
                    NomadBatch.Trace("Concepto ENCONTRADO, Cargando id:" + CONID);
                    myCON = NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO.Get(CONID);
                }

                if (CON.GetAttr("c_grupo_tliq") == "")
                {
                    GROUPID = "";
                }
                else
                {
                    NomadBatch.Trace("Buscar en LIQ02_GRUPOS_TLIQ c_grupo_tliq=" + CON.GetAttr("c_grupo_tliq"));
                    GROUPID = NomadEnvironment.QueryValue("LIQ02_GRUPOS_TLIQ", "oi_grupo_tliq", "c_grupo_tliq", CON.GetAttr("c_grupo_tliq"), "", true);
                    if (GROUPID == null)
                    {
                        bth.Err("Concepto " + CON.GetAttr("c_concepto") + " - Grupo de Liquidacion '" + CON.GetAttr("c_grupo_tliq") + "' no encontrado.");
                        return;
                    }
                }

                if (CON.GetAttr("c_tipo_concepto") == "")
                {
                    bth.Err("Concepto " + CON.GetAttr("c_concepto") + " - El Tipo de Concepto es Obligatorio.");
                    return;
                }
                else
                {
                    NomadBatch.Trace("Buscar en LIQ12_TIPOS_CONC c_tipo_concepto=" + CON.GetAttr("c_tipo_concepto"));
                    TIPID = NomadEnvironment.QueryValue("LIQ12_TIPOS_CONC", "oi_tipo_concepto", "c_tipo_concepto", CON.GetAttr("c_tipo_concepto"), "", true);
                    if (TIPID == null)
                    {
                        bth.Err("Concepto " + CON.GetAttr("c_concepto") + " - Tipo de Concepto '" + CON.GetAttr("c_tipo_concepto") + "' no encontrado.");
                        return;
                    }
                    else
                    {
                        if (myCON.oi_tipo_concepto != "")
                        {
                            Tipos_Concepto.TIPO_CONCEPTO tipoConcepto = Tipos_Concepto.TIPO_CONCEPTO.Get(myCON.oi_tipo_concepto);
                            if (tipoConcepto.c_tipo_concepto != CON.GetAttr("c_tipo_concepto"))
                            {
                                bth.Err("Error en Concepto " + CON.GetAttr("c_concepto") + " - El Tipo de Concepto no se puede modificar");
                                return;
                            }
                        }
                    }
                }

                //////////////////////////////////////////////////////////////
                // Basicos
                NomadBatch.Trace("Seteando Valores...");
                myCON.c_concepto = CON.GetAttr("c_concepto");
                myCON.d_concepto = CON.GetAttr("d_concepto");
                myCON.ca_fig_recibo = CON.GetAttr("ca_fig_recibo");
                myCON.ca_acum_ganancia = CON.GetAttr("ca_acum_ganancia");
                myCON.n_coef_ganancia = CON.GetAttrDouble("n_coef_ganancia");
                myCON.ca_tipo_asiento = CON.GetAttr("ca_tipo_asiento");
                myCON.l_activo = CON.GetAttrBool("l_activo");
                myCON.l_ej_ajuste = CON.GetAttrBool("l_ej_ajuste");
                myCON.l_retroactivo = CON.GetAttrBool("l_retroactivo");
                myCON.e_secuencia = CON.GetAttrInt("e_secuencia");
                myCON.e_etapa = CON.GetAttrInt("e_etapa");
                myCON.o_concepto = CON.GetAttr("o_concepto");

                //////////////////////////////////////////////////////////////
                // FKs
                NomadBatch.Trace("Seteando FKs...");
                if (GROUPID == "") myCON.oi_grupo_tliqNull = true;
                else myCON.oi_grupo_tliq = GROUPID;

                myCON.oi_tipo_concepto = TIPID;

                //////////////////////////////////////////////////////////////
                // Formaula y Childs
                NomadBatch.Trace("Eliminando Parametros...");
                myCON.CONC_VAR.Clear();
                string formulaFinal = "";

                switch (CON.Name.ToUpper())
                {
                    case "CON":
                        NomadBatch.Trace("Seteando Version 1.0 y Formula...");
                        formulaFinal = LimpiarFormula("Concepto "+CON.GetAttr("c_concepto"), CON.GetAttr("").Trim(),4000);

                        if (formulaFinal.Length > 4000)
                        {
                            bth.Err("La fórmula del Concepto " + CON.GetAttr("c_concepto") + " supera el límite de largo permitido (4000 caracteres). El largo de la fórmula a importar es: " + formulaFinal.Length + ".");
                            return;
                        }

                        myCON.t_formula = formulaFinal;
                        myCON.c_version = "1";

                        //Variables
                        for (CONVAR = CON.FirstChild(); CONVAR != null; CONVAR = CONVAR.Next())
                        {
                            NomadBatch.Trace("Buscar en LIQ09_VARIABLES c_variable=" + CONVAR.GetAttr("c_variable"));
                            VARID = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", CONVAR.GetAttr("c_variable"), "", true);
                            if (VARID == null)
                            {
                                bth.Err("Concepto " + CON.GetAttr("c_concepto") + " - Variable '" + CONVAR.GetAttr("c_variable") + "' no encontrada.");
                                return;
                            }

                            NomadBatch.Trace("Agregando el Parametro...");
                            myCONVAR = new NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE();
                            myCONVAR.c_tipo_parametro = CONVAR.GetAttr("c_tipo_parametro");
                            myCONVAR.oi_variable = VARID;
                            myCONVAR.c_alias = CONVAR.GetAttr("c_variable");
                            myCON.CONC_VAR.Add(myCONVAR);
                        }
                        break;

                    case "CON1":
                        NomadBatch.Trace("Seteando Version 2.0 y Formula...");
                        formulaFinal = LimpiarFormula("Concepto "+CON.GetAttr("c_concepto"), CON.GetAttr("").Trim(),4000);

                        if (formulaFinal.Length > 4000)
                        {
                            bth.Err("La fórmula del Concepto " + CON.GetAttr("c_concepto") + " supera el límite de largo permitido (4000 caracteres). El largo de la fórmula a importar es: " + formulaFinal.Length + ".");
                            return;
                        }

                        myCON.t_formula = formulaFinal;
                        myCON.c_version = "2";

                        //Variables de LOCALES
                        for (CONVAR = CON.FindElement("LOC").FirstChild(); CONVAR != null; CONVAR = CONVAR.Next())
                        {
                            NomadBatch.Trace("Agregando el Parametro LOCAL " + CONVAR.GetAttr("c_variable") + "...");
                            myCONVAR = new NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE();
                            myCONVAR.c_tipo_parametro = "LOC";
                            myCONVAR.oi_variableNull = true;
                            myCONVAR.c_alias = CONVAR.GetAttr("c_variable");
                            myCONVAR.c_tipo_dato = CONVAR.GetAttr("c_tipo_dato");
                            myCONVAR.t_formula = LimpiarFormula("Concepto "+ CONVAR.GetAttr("c_variable") + " - Variable '" + CONVAR.GetAttr("c_variable")+"'", CONVAR.GetAttr("c_formula").Trim(), 1000);

                            if (myCONVAR.t_formula.Length > 1000)
                            {
                                bth.Err("La fórmula del Concepto " + CON.GetAttr("c_concepto") + " - Variable '" + CONVAR.GetAttr("c_variable") + "' supera el límite de largo permitido (1000 caracteres). El largo de la fórmula a importar es: " + formulaFinal.Length + ".");
                                return;
                            }

                            myCON.CONC_VAR.Add(myCONVAR);
                        }

                        //Variables de ENTRADA
                        for (CONVAR = CON.FindElement("IN").FirstChild(); CONVAR != null; CONVAR = CONVAR.Next())
                        {
                            NomadBatch.Trace("Buscar en LIQ09_VARIABLES c_variable=" + CONVAR.GetAttr("c_variable"));
                            VARID = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", CONVAR.GetAttr("c_variable"), "", true);
                            if (VARID == null)
                            {
                                bth.Err("Concepto " + CON.GetAttr("c_concepto") + " - Variable '" + CONVAR.GetAttr("c_variable") + "' no encontrada.");
                                return;
                            }

                            NomadBatch.Trace("Agregando el Parametro de ENTRADA " + CONVAR.GetAttr("c_variable") + "...");
                            myCONVAR = new NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE();
                            myCONVAR.c_tipo_parametro = "IN";
                            myCONVAR.oi_variable = VARID;
                            myCONVAR.c_alias = (CONVAR.GetAttr("c_alias") == "" ? CONVAR.GetAttr("c_variable") : CONVAR.GetAttr("c_alias"));
                            myCON.CONC_VAR.Add(myCONVAR);
                        }

                        //Variables de SALIDA
                        for (CONVAR = CON.FindElement("OUT").FirstChild(); CONVAR != null; CONVAR = CONVAR.Next())
                        {
                            NomadBatch.Trace("Buscar en LIQ09_VARIABLES c_variable=" + CONVAR.GetAttr("c_variable"));
                            VARID = NomadEnvironment.QueryValue("LIQ09_VARIABLES", "oi_variable", "c_variable", CONVAR.GetAttr("c_variable"), "", true);
                            if (VARID == null)
                            {
                                bth.Err("Concepto " + CON.GetAttr("c_concepto") + " - Variable '" + CONVAR.GetAttr("c_variable") + "' no encontrada.");
                                return;
                            }

                            NomadBatch.Trace("Agregando el Parametro de SALIDA " + CONVAR.GetAttr("c_variable") + "...");
                            myCONVAR = new NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE();
                            myCONVAR.c_tipo_parametro = CONVAR.GetAttr("c_tipo_parametro");
                            myCONVAR.oi_variable = VARID;
                            myCONVAR.c_alias = (CONVAR.GetAttr("c_alias") == "" ? CONVAR.GetAttr("c_variable") : CONVAR.GetAttr("c_alias"));
                            myCON.CONC_VAR.Add(myCONVAR);
                        }
                        break;
                }

                //Actualizo el Concepto
                NomadEnvironment.GetCurrentTransaction().Save(myCON);
                NomadEnvironment.QueryValueChange("LIQ14_CONCEPTOS", "oi_concepto", "c_concepto", CON.GetAttr("c_concepto"), "", "1", true);
            }

            bth.SetPro(90);
            bth.SetMess("Guardando los Conceptos...");
            NomadEnvironment.GetCurrentTransaction().Commit();
        }

        private static string LimpiarFormula(string codVar, string formulaIN, int largo)
        {
            string formulaOUT = formulaIN;
            if (formulaOUT.Length > largo)
            {
                NomadBatch.Trace("El " + codVar + " tiene una Formula mas larga de " + largo + "... (Largo: " + formulaOUT.Length + ") ");

                //ENTER DOS->UNIX
                if (formulaOUT.Contains("\r\n"))
                {
                    NomadBatch.Trace("Largo: " + formulaOUT.Length + " - Replazo 'ENTER DOS' por 'ENTER UNIX'...");
                    formulaOUT = formulaOUT.Replace("\r\n", "\n");
                    NomadBatch.Trace("Largo: " + formulaOUT.Length);
                }

                //ESPACIOS AL FINAL DE LA LINEA
                if (formulaOUT.Contains(" \n") || formulaOUT.Contains("\t\n"))
                {
                    NomadBatch.Trace("Largo: " + formulaOUT.Length + " - Elimino los Espacios al Final de las LINEAS...");
                    string org;
                    do
                    {
                        org = formulaOUT;
                        formulaOUT = formulaOUT.Replace(" \n", "\n");
                        formulaOUT = formulaOUT.Replace("\t\n", "\n");
                    } while (org != formulaOUT);
                    NomadBatch.Trace("Largo: " + formulaOUT.Length);
                }

                //REEMPLAZO ESPACIOS DOBLES POR TAB
                if (formulaOUT.Length > largo)
                    if (formulaOUT.Contains("  ") || formulaOUT.Contains(" \t") || formulaOUT.Contains("\t "))
                    {
                        NomadBatch.Trace("Largo: " + formulaOUT.Length + " - Reemplazo los 'Espacios Dobles' por 'Tabs'...");
                        string org;
                        do
                        {
                            org = formulaOUT;
                            formulaOUT = formulaOUT.Replace("  ", "\t");
                            formulaOUT = formulaOUT.Replace(" \t", "\t");
                            formulaOUT = formulaOUT.Replace("\t ", "\t");
                        } while (org != formulaOUT);
                        NomadBatch.Trace("Largo: " + formulaOUT.Length);
                    }

                //REEMPLAZO ENTER DOBLES
                if (formulaOUT.Length > largo)
                    if (formulaOUT.Contains("\n\n"))
                    {
                        NomadBatch.Trace("Largo: " + formulaOUT.Length + " - Reemplazo los 'Espacios Dobles' por 'Tabs'...");
                        string org;
                        do
                        {
                            org = formulaOUT;
                            formulaOUT = formulaOUT.Replace("\n\n", "\n");
                        } while (org != formulaOUT);
                        NomadBatch.Trace("Largo: " + formulaOUT.Length);
                    }

                if (formulaOUT.Length > largo)
                    NomadBatch.Trace("El " + codVar + " se modifico la Formula y sigue siendo larga. (Largo: " + formulaOUT.Length + ") ");
                else
                    NomadBatch.Trace("El " + codVar + " se modifico la Formula. (Largo: " + formulaOUT.Length + ") ");
            }
            return formulaOUT;
        }

        static public NomadXML FindConcGroup(NomadXML GCONS, string path)
        {
            NomadXML AUX;
            int t;
            string[] pathPart = path.Split('/');

            for (t = 1; t < pathPart.Length; t++)
            {
                AUX = GCONS.FindElement2("GCON", "des", pathPart[t]);
                if (AUX == null)
                {
                    AUX = GCONS.AddTailElement("GCON");
                    AUX.SetAttr("des", pathPart[t]);
                }
                GCONS = AUX;
            }

            return GCONS;
        }

        static public NomadXML FindVarGroup(NomadXML GVARS, string path)
        {
            NomadXML AUX;
            int t;
            string[] pathPart = path.Split('/');

            for (t = 1; t < pathPart.Length; t++)
            {
                AUX = GVARS.FindElement2("GVAR", "des", pathPart[t]);
                if (AUX == null)
                {
                    AUX = GVARS.AddTailElement("GVAR");
                    AUX.SetAttr("des", pathPart[t]);
                }
                GVARS = AUX;
            }

            return GVARS;
        }

        static string FindVariable(NomadXML VARS, string oi_variable)
        {
            NucleusRH.Base.Liquidacion.Variables.VARIABLE MyVAR;

            //Obtengo la Variable
            MyVAR = (NucleusRH.Base.Liquidacion.Variables.VARIABLE)NomadProxy.GetProxy().CacheGetObj("VAROBJ" + oi_variable);
            if (MyVAR == null)
            {
                MyVAR = NucleusRH.Base.Liquidacion.Variables.VARIABLE.Get(oi_variable);
                NomadProxy.GetProxy().CacheAdd("VAROBJ" + oi_variable, MyVAR);
            }

            //Busco el PATH
            NomadXML GVARS = FindVarGroup(VARS, MyVAR.d_path);

            NomadXML VAR = GVARS.FindElement2("VAR", "c_variable", MyVAR.c_variable);
            if (VAR == null)
            {
                VAR = GVARS.AddTailElement("VAR");
                VAR.SetAttr("c_variable", MyVAR.c_variable);
                VAR.SetAttr("d_variable", MyVAR.d_variable);
                VAR.SetAttr("o_variable", MyVAR.o_variable);
                if (!MyVAR.n_valorNull) VAR.SetAttr("n_valor", MyVAR.n_valor);
                VAR.SetAttr("l_apertura", MyVAR.l_apertura);
                VAR.SetAttr("c_tipo_dato", MyVAR.c_tipo_dato);
                VAR.SetAttr("l_reservada", MyVAR.l_reservada);
                VAR.SetAttr("c_tipo_variable", MyVAR.c_tipo_variable);
            }

            return VAR.GetAttr("c_variable");
        }

        static public void ExportarConceptos(Nomad.NSystem.Document.NmdXmlDocument parametros)
        {
            NomadBatch bth = NomadBatch.GetBatch("ExportarConceptos", "ExportarConceptos");

            //Genero el XML de parametros
            NomadXML param = new NomadXML();
            param.SetText(parametros.DocumentToString());

            //Genero el XML
            NomadXML retval = ExportarConceptos(param.FirstChild());

            //Grabo el Archivo
            bth.SetMess("Guardando el Archivo Exportado....");

            string strFileName = NomadProxy.GetProxy().RunPath + "TEMP\\" + NomadProxy.GetProxy().Batch().ID + "-conceptos.txt";

            retval.ToFile(strFileName);

            //Publicando el Archivo en el NOMAD
            bth.SetMess("Publicando el Archivo Exportado....");
            NomadProxy.GetProxy().FileServiceIO().SaveBinFile("INTERFACES", NomadProxy.GetProxy().Batch().ID + ".txt", strFileName);
        }

        static public NomadXML ExportarConceptos(NomadXML param)
        {
            int x;
            NomadBatch bth = NomadBatch.GetBatch("ExportarConceptos", "ExportarConceptos");
            NomadXML retval = new NomadXML("DATA2");
            NomadXML VARS = retval.AddTailElement("VARS");
            NomadXML CONS = retval.AddTailElement("CONS");
            NomadXML GCONS, CON, IN, OUT, LOC, CONVAR;

            NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO myCONCDDO;

            bth.SetMess("Exportando Conceptos...");
            x = 0;
            for (NomadXML cur = param.FirstChild(); cur != null; cur = cur.Next())
            {
                x++;
                bth.SetMess("Exportando Conceptos - " + x.ToString() + " de " + param.ChildLength.ToString());
                bth.SetPro(10, 70, param.ChildLength, x);
                myCONCDDO = NucleusRH.Base.Liquidacion.Conceptos.CONCEPTO.Get(cur.GetAttr("id"));

                GCONS = FindConcGroup(CONS, myCONCDDO.d_path);
                switch (myCONCDDO.c_version)
                {
                    case "":
                    case "1":
                        CON = GCONS.AddTailElement("CON");

                        //Basicos
                        CON.SetAttr("c_concepto", myCONCDDO.c_concepto);
                        CON.SetAttr("d_concepto", myCONCDDO.d_concepto);
                        CON.SetAttr("ca_fig_recibo", myCONCDDO.ca_fig_recibo);
                        CON.SetAttr("ca_acum_ganancia", myCONCDDO.ca_acum_ganancia);
                        CON.SetAttr("n_coef_ganancia", myCONCDDO.n_coef_ganancia);
                        CON.SetAttr("ca_tipo_asiento", myCONCDDO.ca_tipo_asiento);
                        CON.SetAttr("l_activo", myCONCDDO.l_activo);
                        CON.SetAttr("l_retroactivo", myCONCDDO.l_retroactivo);
                        CON.SetAttr("l_ej_ajuste", myCONCDDO.l_ej_ajuste);
                        if (!myCONCDDO.e_secuenciaNull) CON.SetAttr("e_secuencia", myCONCDDO.e_secuencia);
            if (!myCONCDDO.e_etapaNull) CON.SetAttr("e_etapa", myCONCDDO.e_etapa);
                        CON.SetAttr("o_concepto", myCONCDDO.o_concepto);

                        //FK
                        CON.SetAttr("c_grupo_tliq", (myCONCDDO.oi_grupo_tliqNull ? "" : myCONCDDO.Getoi_grupo_tliq().c_grupo_tliq));
                        CON.SetAttr("c_tipo_concepto", (myCONCDDO.oi_tipo_conceptoNull ? "" : myCONCDDO.Getoi_tipo_concepto().c_tipo_concepto));

                        //Parametros
                        foreach (NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE myVARCONCDDO in myCONCDDO.CONC_VAR)
                        {
                            CONVAR = CON.AddTailElement("VAR");
                            CONVAR.SetAttr("c_tipo_parametro", myVARCONCDDO.c_tipo_parametro);
                            CONVAR.SetAttr("c_variable", FindVariable(VARS, myVARCONCDDO.oi_variable));
                        }

                        //Formula
                        CON.SetAttr("", myCONCDDO.t_formula.Trim());
                        break;

                    case "2":
                        CON = GCONS.AddTailElement("CON1");

                        //Basicos
                        CON.SetAttr("c_concepto", myCONCDDO.c_concepto);
                        CON.SetAttr("d_concepto", myCONCDDO.d_concepto);
                        CON.SetAttr("ca_fig_recibo", myCONCDDO.ca_fig_recibo);
                        CON.SetAttr("ca_acum_ganancia", myCONCDDO.ca_acum_ganancia);
                        CON.SetAttr("n_coef_ganancia", myCONCDDO.n_coef_ganancia);
                        CON.SetAttr("ca_tipo_asiento", myCONCDDO.ca_tipo_asiento);
                        CON.SetAttr("l_activo", myCONCDDO.l_activo);
                        CON.SetAttr("l_retroactivo", myCONCDDO.l_retroactivo);
                        CON.SetAttr("l_ej_ajuste", myCONCDDO.l_ej_ajuste);
                        if (!myCONCDDO.e_secuenciaNull) CON.SetAttr("e_secuencia", myCONCDDO.e_secuencia);
            if (!myCONCDDO.e_etapaNull) CON.SetAttr("e_etapa", myCONCDDO.e_etapa);
                        CON.SetAttr("o_concepto", myCONCDDO.o_concepto);

                        //FK
                        CON.SetAttr("c_grupo_tliq", (myCONCDDO.oi_grupo_tliqNull ? "" : myCONCDDO.Getoi_grupo_tliq().c_grupo_tliq));
                        CON.SetAttr("c_tipo_concepto", (myCONCDDO.oi_tipo_conceptoNull ? "" : myCONCDDO.Getoi_tipo_concepto().c_tipo_concepto));

                        //Parametros
                        IN = CON.AddTailElement("IN");
                        OUT = CON.AddTailElement("OUT");
                        LOC = CON.AddTailElement("LOC");

                        foreach (NucleusRH.Base.Liquidacion.Conceptos.CONC_VARIABLE myVARCONCDDO in myCONCDDO.CONC_VAR)
                        {
                            switch (myVARCONCDDO.c_tipo_parametro)
                            {
                                case "IN":
                                    CONVAR = IN.AddTailElement("VAR");
                                    CONVAR.SetAttr("c_variable", FindVariable(VARS, myVARCONCDDO.oi_variable));
                                    CONVAR.SetAttr("c_alias", myVARCONCDDO.c_alias == "" ? CONVAR.GetAttr("c_variable") : myVARCONCDDO.c_alias);
                                    break;

                                case "OUT":
                                case "ACV":
                                case "ACC":
                                    CONVAR = OUT.AddTailElement("VAR");
                                    CONVAR.SetAttr("c_tipo_parametro", myVARCONCDDO.c_tipo_parametro);
                                    CONVAR.SetAttr("c_variable", FindVariable(VARS, myVARCONCDDO.oi_variable));
                                    CONVAR.SetAttr("c_alias", myVARCONCDDO.c_alias == "" ? CONVAR.GetAttr("c_variable") : myVARCONCDDO.c_alias);
                                    break;

                                case "LOC":
                                    CONVAR = LOC.AddTailElement("VAR");
                                    CONVAR.SetAttr("c_variable", myVARCONCDDO.c_alias);
                                    CONVAR.SetAttr("c_tipo_dato", myVARCONCDDO.c_tipo_dato);
                                    CONVAR.SetAttr("c_formula", myVARCONCDDO.t_formula.Trim());
                                    break;
                            }
                        }

                        //Formula
                        CON.AddTailElement("FORMULA").SetAttr("", myCONCDDO.t_formula.Trim());
                        break;
                }

            }

            return retval;
        }
    }
}


