using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Liquidacion.MIGLegajo_Liquidacion
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Legajo Liquidacion
    public partial class LEGAJO_LIQUIDACION
    {
        public static void ImportarLegajosLiquidacion()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Imp.Legajos", "Importación de Legajos");

            NomadXML IDList = new NomadXML();
            LEGAJO_LIQUIDACION objRead;

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");

            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {
                    objRead = Get(row.GetAttr("id"));

                    //Valido que el legajo exista en la tabla Per02_Personal_Emp
                    string oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (string.IsNullOrEmpty(oiEMP))
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    string oiPEPER = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP, true);
                    if (string.IsNullOrEmpty(oiPEPER))
                    {
                        objBatch.Err("El Legajo no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el DDO segun el OI recuperado
                    NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP DDOPERLIQ;
                    DDOPERLIQ = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(oiPEPER);

                    // Actualiza los datos
                    if (!string.IsNullOrEmpty(objRead.c_cta_bancaria))
                        DDOPERLIQ.c_cta_bancaria = objRead.c_cta_bancaria;
                    else
                        DDOPERLIQ.c_cta_bancariaNull = true;

                    if (!string.IsNullOrEmpty(objRead.c_tarjeta))
                        DDOPERLIQ.c_tarjeta = objRead.c_tarjeta;
                    else
                        DDOPERLIQ.c_tarjetaNull = true;

                    if (!string.IsNullOrEmpty(objRead.c_cbu))
                        DDOPERLIQ.c_cbu = objRead.c_cbu;
                    else
                        DDOPERLIQ.c_cbuNull = true;

                    if (!objRead.n_bruto_antNull)
                        DDOPERLIQ.n_bruto_ant = objRead.n_bruto_ant;
                    else
                        DDOPERLIQ.n_bruto_antNull = true;

                    if (!objRead.n_deduc_antNull)
                        DDOPERLIQ.n_deduc_ant = objRead.n_deduc_ant;
                    else
                        DDOPERLIQ.n_deduc_antNull = true;

                    if (!objRead.n_reten_antNull)
                        DDOPERLIQ.n_reten_ant = objRead.n_reten_ant;
                    else
                        DDOPERLIQ.n_reten_antNull = true;

                    if (!objRead.n_bruto_nr_antNull)
                        DDOPERLIQ.n_bruto_nr_ant = objRead.n_bruto_nr_ant;
                    else
                        DDOPERLIQ.n_bruto_nr_antNull = true;

                    if (!objRead.n_ap_jub_antNull)
                        DDOPERLIQ.n_ap_jub_ant = objRead.n_ap_jub_ant;
                    else
                        DDOPERLIQ.n_ap_jub_antNull = true;

                    if (!objRead.n_ap_os_antNull)
                        DDOPERLIQ.n_ap_os_ant = objRead.n_ap_os_ant;
                    else
                        DDOPERLIQ.n_ap_os_antNull = true;

                    if (!objRead.n_ap_ley_antNull)
                        DDOPERLIQ.n_ap_ley_ant = objRead.n_ap_ley_ant;
                    else
                        DDOPERLIQ.n_ap_ley_antNull = true;

                    if (!objRead.l_agente_retNull)
                        DDOPERLIQ.l_agente_ret = objRead.l_agente_ret;
                    else
                        DDOPERLIQ.l_agente_ret = false;

                    if (!objRead.l_liq_finalNull)
                        DDOPERLIQ.l_liq_final = objRead.l_liq_final;
                    else
                        DDOPERLIQ.l_liq_final = false;

                    if (!string.IsNullOrEmpty(objRead.o_liq_per))
                        DDOPERLIQ.o_liq_per = objRead.o_liq_per;
                    else
                        DDOPERLIQ.o_liq_perNull = true;

                    //Busca BANCO - SUCURSAL
                    string oiBAN = NomadEnvironment.QueryValue("LIQ22_BANCOS", "oi_banco", "c_banco", objRead.c_banco, "", true);
                    if (string.IsNullOrEmpty(oiBAN))
                    {
                        objBatch.Err("El banco no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    string oiSUC = NomadEnvironment.QueryValue("LIQ22_BANCOS_SUCUR", "oi_sucursal", "c_sucursal", objRead.c_sucursal, "LIQ22_BANCOS_SUCUR.oi_banco = " + oiBAN, true);
                    if (string.IsNullOrEmpty(oiSUC))
                    {
                        objBatch.Err("La Sucursal no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    DDOPERLIQ.oi_sucursal = oiSUC;

                    //Busca FORMA DE PAGO ALTERNATIVA
                    if (!string.IsNullOrEmpty(objRead.c_forma_pago))
                    {
                        string oiFPA = NomadEnvironment.QueryValue("PER31_FORMAS_PAGO", "oi_forma_pago", "c_forma_pago", objRead.c_forma_pago, "", true);
                        if (string.IsNullOrEmpty(oiFPA))
                        {
                            objBatch.Err("La forma de pago no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        DDOPERLIQ.oi_forma_pago = oiFPA;
                    }
                    else
                    {
                        DDOPERLIQ.oi_forma_pagoNull = true;
                    }

                    // BORRAR LOG !!!!
                    //NomadEnvironment.GetTrace().Info("DDOPERLIQ " + DDOPERLIQ.SerializeAll());
                    //Graba
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(DDOPERLIQ);
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro " + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                        Errores++;
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

