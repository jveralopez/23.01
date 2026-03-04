using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;
using NucleusRH.Base.Liquidacion.Legajo_Liquidacion;
using System.Globalization;

namespace NucleusRH.Base.Liquidacion.MigConsumos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Anticipos
    public partial class CONSUMOS
    {
        public static void ImportarConsumos()
        {
            int Linea = 0, Errores, Importados = 0, Errores2 = 0, ErroresTotal = 0;
            Hashtable EmpresasHash = new Hashtable();
            Hashtable LegajosHash = new Hashtable();
            Hashtable ConsumosHash = new Hashtable();
            Hashtable ConsumosPerHash = new Hashtable();
            Hashtable TipoConsumoHash = new Hashtable();
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Consumos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Liquidacion.MigConsumos.CONSUMOS objRead;
 
            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");

            for (int xml = 0; xml < lista.Count; xml++)
            {
                Errores = 0; 
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion - Crear primero los consumos en los legajos de liquidacion
                try
                {
                    objRead = NucleusRH.Base.Liquidacion.MigConsumos.CONSUMOS.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios del consumo
                    if (objRead.c_consumo_per =="")
                    {
                        objBatch.Err("No se especifico el Numero de Comprobante, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especifico la Empresa, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.e_numero_legajo == null)
                    {
                        objBatch.Err("No se especifico el Numero de Legajo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.c_consumo == "")
                    {
                        objBatch.Err("No se especifico el Consumo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.f_consumoNull)
                    {
                        objBatch.Err("No se especifico la Fecha de Consumo, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                    if (objRead.e_importeNull)
                    {
                        objBatch.Err("No se especifico el Importe, se rechaza el registro de la Linea:'" + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }
                     
                    //Recupero los OI de los c&#243;digos ingresados
                    string oiCon = "", oiEMP = "", oiPER = "", oiConPer,oiTipoCon;
                  
                    //Recupero la empresa
                    if (EmpresasHash.ContainsKey(objRead.c_empresa))
                    {
                        oiEMP = EmpresasHash[objRead.c_empresa].ToString();
                    }
                    else
                    {
                        oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                        EmpresasHash.Add(objRead.c_empresa, oiEMP);
                    }
                    
                    if (oiEMP == null)
                    {
                        objBatch.Err("La empresa no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }


                    if (LegajosHash.ContainsKey(oiEMP + "_" + objRead.e_numero_legajo))
                    {
                        oiPER = LegajosHash[oiEMP + "_" + objRead.e_numero_legajo].ToString();
                    }
                    else
                    {
                        oiPER = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PER02_PERSONAL_EMP.oi_empresa = " + oiEMP, true);
                        LegajosHash.Add(oiEMP + "_" + objRead.e_numero_legajo, oiPER);
                    }
                    //Recupero el Legajo en la Empresa 
                    if (oiPER == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    if (ConsumosHash.ContainsKey(objRead.c_consumo))
                    {
                        oiCon = ConsumosHash[objRead.c_consumo].ToString();
                        oiTipoCon = TipoConsumoHash[objRead.c_consumo].ToString();
                    }
                    else
                    {
                        oiCon = NomadEnvironment.QueryValue("LIQ08_CONSUMOS", "oi_consumo", "c_consumo", objRead.c_consumo.ToString(), "", true);
                        oiTipoCon = NucleusRH.Base.Liquidacion.Consumos.CONSUMO.Get(oiCon).oi_tipo_consumo;
                        ConsumosHash.Add(objRead.c_consumo, oiCon);
                        TipoConsumoHash.Add(objRead.c_consumo, oiTipoCon);
                    }
                    //Recupero el Consumo
                    if (oiCon == null)
                    {
                        objBatch.Err("El Consumo no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        ErroresTotal++;
                        continue;
                    }

                    if (ConsumosPerHash.ContainsKey(objRead.c_consumo_per))
                    {
                        oiConPer = ConsumosPerHash[objRead.c_consumo_per].ToString();
                    }
                    else
                    {
                        oiConPer = NomadEnvironment.QueryValue("LIQ07_CONSUMO_PER", "oi_consumo_per", "c_consumo_per", objRead.c_consumo_per.ToString(), "LIQ07_CONSUMO_PER.oi_personal_emp = " + oiPER, true);
                        ConsumosPerHash.Add(objRead.c_consumo_per, oiConPer);
                    }
                    //verifico si esta duplicado el registro
                    NucleusRH.Base.Liquidacion.Legajo_Liquidacion.CONSUMO consumo;
                    PERSONAL_EMP peremp = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.PERSONAL_EMP.Get(oiPER,false);

                    if (oiConPer != null )
                    {           
                        consumo = (CONSUMO)peremp.CONSUMOS.GetById(oiConPer);
                     
                        if (consumo.c_estado == "P")
                        {
                            objBatch.Err("El consumo existe y se encuentra en estado Procesado, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_numero_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            ErroresTotal++;
                            continue;
                        }
                    }
                    else
                    {
                        consumo = NucleusRH.Base.Liquidacion.Legajo_Liquidacion.CONSUMO.New();
                    }

                    if (Errores == 0)
                    {
                        consumo.oi_personal_emp = StringUtil.str2int(oiPER);
                        consumo.c_estado = "A"; //Estado = Otorgado
                        consumo.f_consumo = objRead.f_consumo;
                        string importe = objRead.e_importe.Replace(",", ".");

                        consumo.e_importe = Convert.ToDouble(importe, CultureInfo.InvariantCulture);
                        consumo.oi_consumo = oiCon;        
                        consumo.oi_tipo_consumo = oiTipoCon;
                        consumo.c_consumo_per = objRead.c_consumo_per;
                        consumo.d_observaciones = objRead.d_observaciones;
                    
                        CONSUMO consumoPer = (CONSUMO)peremp.CONSUMOS.GetByAttribute("c_consumo_per", consumo.c_consumo_per);
                        if (consumoPer == null)
                            peremp.CONSUMOS.Add(consumo);

                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Begin();
                            NomadEnvironment.GetCurrentTransaction().Save(peremp);
                            NomadEnvironment.GetCurrentTransaction().Commit();
                            Importados++;
                        }
                        catch (Exception e)
                        {
                            NomadEnvironment.GetCurrentTransaction().Rollback();
                            Errores2++;
                            ErroresTotal++;

                            if (e.Message == "DB.SQLSERVER.2627")
                            {
                                //Violation of primary key. Handle Exception
                                objBatch.Err("Ya existe un consumo para el Legajo '" + peremp.e_numero_legajo + " con fecha de consumo " + consumo.f_consumo);
                            }
                            else
                            {
                                objBatch.Err("Error al grabar registro del Consumo - Legajo: " + peremp.e_numero_legajo + " - Fecha de Consumo" + consumo.f_consumo);

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                    ErroresTotal++;
                }
            }
            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Consumos Importados: " + Importados.ToString() + " - Registros con Errores: " + ErroresTotal.ToString() + " - Error al guardar Consumo: " + Errores2.ToString());
            objBatch.Log("Finalizado...");

        }
    }
}


