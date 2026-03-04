using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.Personal_Liquidacion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Liquidaciones por Personal
    public partial class PERSONAL_LIQ
    {
        public static Nomad.NSystem.Document.NmdXmlDocument Guardar(Nomad.NSystem.Document.NmdXmlDocument param, string estado, string tipo)
        {
            NomadEnvironment.GetTrace().Info("Comienza el método de Inicializar");

            if (estado != "C")
            {
                /* inicializa personas a la liquidacion */
                bool bolConPersonas = false;
                NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ DDOAux = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ();

                Nomad.NSystem.Document.NmdXmlDocument dokum;
                dokum = new Nomad.NSystem.Document.NmdXmlDocument(Nomad.NSystem.Proxy.NomadProxy.GetProxy().SQLService().Get(NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Resources.QRY_Inicializar, param.DocumentToString()));

                NomadEnvironment.GetTrace().Info("Formato esperado: <DATOS><ROWS><ROW>");
                NomadEnvironment.GetTrace().Info("Resultado dokum: " + dokum.ToString());

                string liqui = param.GetAttribute("oi_Liquidacion").Value;
                Nomad.NSystem.Document.NmdDocument nxdPersonas = dokum.GetFirstChildDocument();

                NomadEnvironment.GetTrace().Info("hijo dokum: " + nxdPersonas.ToString());

                Nomad.NSystem.Document.NmdDocument ndoPersona;

                //Inicia la transacción
                NomadEnvironment.GetCurrentTransaction().Begin();
                bool bandera = true;
                string sError = "";
                string sOK = "";
                string strResult = "";
                int Contador = 1;
                int ContadorOK = 1;

                for (ndoPersona = nxdPersonas.GetFirstChildDocument(); ndoPersona != null; ndoPersona = nxdPersonas.GetNextChildDocument())
                {
                    NomadEnvironment.GetTrace().Info("por cada persona");
                    if (ndoPersona.GetAttribute("oi_afjp") == null)
                    {
                        sError = sError + "<ERROR id=\"" + Contador.ToString() + "\" desc=\"La persona " + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + " no tiene cargada la AFJP.\" />";
                        Contador = Contador + 1;
                        bandera = false;
                    }
                    if (ndoPersona.GetAttribute("oi_sindicato") == null)
                    {
                        sError = sError + "<ERROR id=\"" + Contador.ToString() + "\" desc=\"La persona " + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + " no tiene cargado el Sindicato.\" />";
                        Contador = Contador + 1;
                        bandera = false;
                    }
                    if (ndoPersona.GetAttribute("oi_obra_social") == null)
                    {
                        sError = sError + "<ERROR id=\"" + Contador.ToString() + "\" desc=\"La persona " + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + " no tiene cargada la Obra Social.\" />";
                        Contador = Contador + 1;
                        bandera = false;
                    }
                    NomadEnvironment.GetTrace().Info("Termino las validaciones");
                    if (bandera)
                    {
                        sOK = sOK + "<OK id=\"" + ContadorOK.ToString() + "\" desc=\"" + ndoPersona.GetAttribute("D_APE_Y_NOM").Value + "\" />";
                        NomadEnvironment.GetTrace().Info(sOK);
                        ContadorOK = ContadorOK + 1;
                        string pers = ndoPersona.GetAttribute("oi_personal_emp").Value;
                        NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ nuevoDDO = new NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ();
                        nuevoDDO.oi_liquidacion = liqui;
                        nuevoDDO.oi_personal_emp = pers;
                        NomadEnvironment.GetCurrentTransaction().Save(nuevoDDO);
                        bolConPersonas = true;
                        NomadEnvironment.GetTrace().Info("Guardo la persona");
                    }
                }

                if (bolConPersonas)
                {
                    NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION ddoLiq = NucleusRH.Base.Liquidacion.Liquidacion.LIQUIDACION.Get(liqui);
                    if (ddoLiq.c_estado == "A")
                    {
                        ddoLiq.c_estado = "I";
                        NomadEnvironment.GetCurrentTransaction().Save(ddoLiq);
                        NomadEnvironment.GetTrace().Info("Cambia el estado de la liquidacion");
                    }

                    try
                    {
                        //Hace persistente los cambios
                        NomadEnvironment.GetTrace().Info("Antes Commit");
                        NomadEnvironment.GetCurrentTransaction().Commit();
                        NomadEnvironment.GetTrace().Info("Despues Commit");
                    }
                    catch (Exception ex)
                    {
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                        throw new NomadAppException("Se produjo un error al intentar grabar los cambios. " + ex.Message, ex);
                    }
                }
                else
                {
                    //Hace un rollback para limpiar la transacción
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                }
                NomadEnvironment.GetTrace().Info("Va a Imprimier los errores");
                if (sError == "")
                {
                    strResult = "<DATOS cant_errores=\"" + (Contador - 1).ToString() + "\"><ERRORES></ERRORES><OKS>" + sOK + "</OKS></DATOS>";
                }
                else
                {
                    strResult = "<DATOS cant_errores=\"" + (Contador - 1).ToString() + "\"><ERRORES>" + sError + "</ERRORES><OKS>" + sOK + "</OKS></DATOS>";
                }
                NomadEnvironment.GetTrace().Info("Armo los errores: " + strResult);
                Nomad.NSystem.Document.NmdXmlDocument nxdResult;
                nxdResult = new Nomad.NSystem.Document.NmdXmlDocument(strResult);
                NomadEnvironment.GetTrace().Info("Termino");
                return nxdResult;
            }
            else
                throw new NomadAppException("La liquidacion se encuentra cerrada, por lo tanto no puede inicializar personas");
        }

        /// <summary>
        /// Carga de novedades de cargo por variable
        /// </summary>
        /// <param name="xmlFil"></param>
        ///     Filtros
        /// <param name="xmlDoc"></param>
        ///     Grilla
        public static void NovCarPorVariables(Nomad.NSystem.Proxy.NomadXML xmlFil, Nomad.NSystem.Proxy.NomadXML xmlDoc)
        {
            NomadEnvironment.GetTrace().Info("****** Carga de Novedades de cargo por variable ******");
            NomadEnvironment.GetTrace().Info("FILTROS: " + xmlFil.ToString());
            NomadEnvironment.GetTrace().Info("GRILLA: " + xmlDoc.ToString());

            string strStep = "Seteando Parámetros";
            string oi_empresa = xmlFil.FirstChild().GetAttr("oi_empresa");
            string oi_liquidacion = xmlFil.FirstChild().GetAttr("oi_liquidacion");
            string oi_variable = xmlFil.FirstChild().GetAttr("oi_variable");
            bool existe = false;
            bool existe2 = false;

            //OBTENGO LOS VALORES DE LA BD
            strStep = "Obteniendo valores BD";
            NomadXML Param = new NomadXML("PARAM");
            Param.SetAttr("oi_empresa", oi_empresa);
            Param.SetAttr("oi_liquidacion", oi_liquidacion);
            Param.SetAttr("oi_variable", oi_variable);
            NomadEnvironment.GetTrace().Info("Param: " + Param.ToString());
            NomadXML VAREN_DB = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Resources.QRY_VAREN, Param.ToString());
            NomadEnvironment.GetTrace().Info("VAREN_DB: " + VAREN_DB.ToString());

            try
            {
                strStep = "RECORRIENDO DESDE BD A GRILLA";
                if (VAREN_DB.FirstChild().FirstChild() != null)
                {
                    for (NomadXML row2 = VAREN_DB.FirstChild().FirstChild(); row2 != null; row2 = row2.Next())
                    {
                        if (xmlDoc.FirstChild().FirstChild() != null)
                        {
                        
                            for (NomadXML row = xmlDoc.FirstChild().FirstChild(); row != null; row = row.Next())
                            {
                           
                                double val_nue = row.GetAttrDouble("n_valor");
                                string oi_cargo = row.GetAttr("oi_cargo");

                                if (row2.GetAttrInt("id") == row.GetAttrInt("id"))
                                {
                                    strStep = "ACTUALIZANDO REGISTRO";
                                    existe = true;
                                    PERSONAL_LIQ PER = PERSONAL_LIQ.Get(row.GetAttr("oi_personal_liq"), false);
                                    VAREN_CARGO VAREN = (VAREN_CARGO)PER.VAREN_CARGO.GetByAttribute("id", row.GetAttrInt("id"));
                                    if (VAREN.n_valor != val_nue || VAREN.oi_cargo != row.GetAttr("oi_cargo"))
                                    {
                                        VAREN.n_valor = val_nue;
                                        VAREN.oi_cargo = row.GetAttr("oi_cargo");
                                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(PER);
                                    }
                                }

                            }
                        } 

                        if (!existe)
                        {
                            strStep = "ELIMINANDO REGISTRO";
                            PERSONAL_LIQ PER_LIQ = PERSONAL_LIQ.Get(row2.GetAttr("oi_personal_liq"), false);
                            VAREN_CARGO VAREN = (VAREN_CARGO)PER_LIQ.VAREN_CARGO.GetByAttribute("id", row2.GetAttrInt("id"));
                            PER_LIQ.VAREN_CARGO.Remove(VAREN);
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(PER_LIQ);
                            NomadEnvironment.GetTrace().Info("ELIMINANDO: " + strStep + " " + row2.GetAttrInt("id"));
                        }

                        existe = false;

                    }
                }


                strStep = "RECORRIENDO DESDE GRILLA A BD";
                if (xmlDoc.FirstChild().FirstChild() != null)
                {
                    for (NomadXML row = xmlDoc.FirstChild().FirstChild(); row != null; row = row.Next())
                    {
                        if (VAREN_DB.FirstChild().FirstChild() != null)
                        {
                            for (NomadXML row2 = VAREN_DB.FirstChild().FirstChild(); row2 != null; row2 = row2.Next())
                            {
                                if (row2.GetAttr("id") == row.GetAttr("id"))
                                {
                                    existe2 = true;
                                }
                                
                            }
                         
                        }
                        //SI EL REGISTRO ES NUEVO, LO CREO
                        if (!existe2) 
                        {
                        strStep = "CREANDO REGISTRO";
                        double val_nue = row.GetAttrDouble("n_valor");
                        string oi_cargo = row.GetAttr("oi_cargo");

                        NomadXML Par = new NomadXML("PARAM");
                        Param.SetAttr("oi_cargo", row.GetAttr("oi_cargo"));
                        Param.SetAttr("oi_liquidacion", oi_liquidacion);
                        NomadXML PerLiq = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Liquidacion.Personal_Liquidacion.PERSONAL_LIQ.Resources.QRY_PerLiq, Param.ToString());
                        NomadEnvironment.GetTrace().Info("PerLiq: " + PerLiq.ToString());

                        PERSONAL_LIQ PER = PERSONAL_LIQ.Get(PerLiq.FirstChild().FirstChild().GetAttrInt("oi_personal_liq"), false);
                        VAREN_CARGO VAREN_CAR = new VAREN_CARGO();
                        VAREN_CAR.oi_cargo = row.GetAttr("oi_cargo");
                        VAREN_CAR.oi_variable = oi_variable;
                        VAREN_CAR.n_valor = row.GetAttrDouble("n_valor");
                        PER.VAREN_CARGO.Add(VAREN_CAR);
                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(PER);
                        }

                        existe2 = false;
                     }
                 }

            }

            catch (Exception ex)
            {
                NomadException nmdEx = NomadException.NewInternalException("CARGA DE NOVEDADES DE CARGO POR VARIABLE", ex);
                nmdEx.SetValue("step", strStep);
                throw nmdEx;
            }
        }
            
            
        }
    }

       

