using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Presupuesto.InterfaceNovedadesPorLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase efinicion de interfaz de entrada de tipo uno
    public partial class ENTRADA
    {
        public static void ImportarNovedades(string poi_escenario)
        {
            //NucleusRH.Base.Liquidacion.clsImportarVariables objImportador = new NucleusRH.Base.Liquidacion.clsImportarVariables();
            //objImportador.ImportarVariablesPersona(poi_liquidacion);

            int totRegs, linea = 0, errores = 0, procesados = 0;
            //string str_habiles, d_habiles = "";
            //bool l_habiles = false;

            NomadXML MyROW;
            NomadXML MyXML = new NomadXML();
            ENTRADA objRead;
                        
            //DateTime fCompare = new DateTime(1900, 1, 1);

            //Instancio el Objeto Batch
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Importación Masiva de Novedades por Legajo", "Importación Masiva de Novedades por Legajo");

            //Ejecuto el query que trae el archivo de solicitud.dat
            NomadBatch.Trace("Cargando el Query...");
            MyXML = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Presupuesto.InterfaceNovedadesPorLegajo.ENTRADA.Resources.qry_OiVarArvchivo, "");
            
            totRegs = MyXML.FirstChild().ChildLength;

            string oi_anio_fiscal = null;
            //Recupero oi_presupuesto
            string oi_presupuesto = NomadEnvironment.QueryValue("PRE03_ESCENARIOS", "oi_presupuesto", "oi_escenario", poi_escenario, "", true);
            if (oi_presupuesto == null)
            {
                objBatch.Err("No existe un Presupuesto vinculado al Código de Escenario " + poi_escenario);
                NomadLog.Info("poi_escenario = " + poi_escenario);
                errores++;
            }
            else
            {
                //Recupero oi_anio_fiscal del presupuesto
                oi_anio_fiscal = NomadEnvironment.QueryValue("PRE12_PRESUPUESTO", "oi_anio_fiscal", "oi_presupuesto", oi_presupuesto, "", true);
                if (oi_anio_fiscal == null)
                {
                    objBatch.Err("No existe Año Fiscal vinculado al Código de Presupuesto " + oi_presupuesto);
                    NomadLog.Info("oi_presupuesto = " + oi_presupuesto);
                    errores++;
                }
            }

            if(errores==0)
            {
                for (linea = 1, MyROW = MyXML.FirstChild().FirstChild(); MyROW != null; linea++, MyROW = MyROW.Next())
                {
                    //Creando interface
                    objBatch.SetPro(0, 100, MyXML.FirstChild().ChildLength, linea);
                    objBatch.SetMess("Importando la Linea " + linea + " de " + totRegs);
                    NomadLog.Info("0-- " + MyROW.GetAttr("id"));
                    objRead = NucleusRH.Base.Presupuesto.InterfaceNovedadesPorLegajo.ENTRADA.Get(MyROW.GetAttr("id"));

                    try
                    {
                        //Valido que TODOS los campos obligatorios TENGAN VALOR

                        if (objRead.e_numero_legajoNull || objRead.e_numero_legajo <= 0)
                        {
                            objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        if (objRead.c_novedadNull || objRead.c_novedad == "")
                        {
                            objBatch.Err("No se especificó el Código de Novedad, se rechaza el registro - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        if (objRead.c_periodoNull || objRead.c_periodo == "")
                        {
                            objBatch.Err("No se especificó el Código Periodo Fiscal, se rechaza el registro - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        if (objRead.n_valorNull || objRead.n_valor == "")
                        {
                            objBatch.Err("No se especificó el Valor de la Novedad, se rechaza el registro - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        //Valido que el legajo exista y pertenezca al escenario elegido                   
                        string oi_legajo = NomadEnvironment.QueryValue("PRE05_LEGAJOS", "oi_legajo", "e_numero_legajo", objRead.e_numero_legajo.ToString(), "PRE05_LEGAJOS.oi_escenario = " + poi_escenario, true);
                        if (oi_legajo == null)
                        {
                            objBatch.Err("No existe el Legajo " + objRead.e_numero_legajo + " en el Escenario indicado - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        //El tipo de novedad exista
                        string oi_novedad = NomadEnvironment.QueryValue("PRE17_NOVEDADES", "oi_novedad", "c_novedad", objRead.c_novedad, "", true);
                        if (oi_novedad == null)
                        {
                            objBatch.Err("No existe el Código de Novedad " + objRead.c_novedad + " - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        string oi_detalle_pres;
                        //El periodo fiscal exista y pertenezca al escenario elegido. 
                        string oi_periodo_fiscal = NomadEnvironment.QueryValue("PRE01_PER_FISCAL", "oi_periodo_fiscal", "e_periodo_fiscal", objRead.c_periodo, "PRE01_PER_FISCAL.oi_anio_fiscal = " + oi_anio_fiscal, true);
                        if (oi_periodo_fiscal == null)
                        {
                            objBatch.Err("No existe el Código de Periodo Fiscal " + objRead.c_periodo + " - Linea: " + linea.ToString());
                            NomadLog.Info("oi_anio_fiscal = " + oi_anio_fiscal);
                            NomadLog.Info("c_periodo = " + objRead.c_periodo);
                            errores++;
                            continue;
                        }
                        else
                        {
                            oi_detalle_pres = NomadEnvironment.QueryValue("PRE03_DETALLES", "oi_detalle_pres", "oi_periodo_fiscal", oi_periodo_fiscal, "PRE03_DETALLES.oi_escenario = " + poi_escenario, true);
                            if (oi_detalle_pres == null)
                            {
                                objBatch.Err("No existe el Período Fiscal " + objRead.c_periodo + " en el Escenario elegido - Linea: " + linea.ToString());
                                NomadLog.Info("oi_periodo_fiscal = " + oi_periodo_fiscal);
                                NomadLog.Info("poi_escenario = " + poi_escenario);
                                errores++;
                                continue;
                            }
                        }

                        //Valor sea el esperado (decimal)
                        double monto;
                        if (!double.TryParse(objRead.n_valor, out monto))
                        {
                            objBatch.Err("Formato del Valor de la Novedad " + objRead.c_periodo + " no es el esperado (decimal separado por punto) - Linea: " + linea.ToString());
                            errores++;
                            continue;
                        }

                        NomadEnvironment.GetCurrentTransaction().Begin();

                        NucleusRH.Base.Presupuesto.Legajos.LEGAJO legajo = NucleusRH.Base.Presupuesto.Legajos.LEGAJO.Get(oi_legajo);

                        NucleusRH.Base.Presupuesto.Legajos.NOV_LEGAJO novedad_legajo = null;
                        //Valido si ya existe la novedad                    
                        string oi_nov_legajo = NomadEnvironment.QueryValue("PRE05_NOV_LEGAJO", "oi_nov_legajo", "oi_legajo", oi_legajo, "PRE05_NOV_LEGAJO.oi_novedad = " + oi_novedad + " AND PRE05_NOV_LEGAJO.oi_detalle_pres = " + oi_detalle_pres, true);
                        //Si existe, la actualizo
                        if (oi_nov_legajo != null)
                        {
                            novedad_legajo = NucleusRH.Base.Presupuesto.Legajos.NOV_LEGAJO.Get(oi_nov_legajo);
                            novedad_legajo.o_det_concepto = objRead.observaciones;
                            novedad_legajo.n_valor = double.Parse(objRead.n_valor);
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(novedad_legajo);
                        }
                        //Si no existe, la creo
                        else
                        {
                            novedad_legajo = NucleusRH.Base.Presupuesto.Legajos.NOV_LEGAJO.New();
                            novedad_legajo.oi_legajo = int.Parse(oi_legajo);
                            novedad_legajo.oi_novedad = oi_novedad;
                            novedad_legajo.oi_detalle_pres = oi_detalle_pres;
                            novedad_legajo.n_valor = double.Parse(objRead.n_valor);
                            novedad_legajo.o_det_concepto = objRead.observaciones;
                            legajo.NOV_LEGAJO.Add(novedad_legajo);
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(legajo);
                        }

                        NomadEnvironment.GetCurrentTransaction().Commit();
                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro - Linea: " + linea.ToString() + " - " + e.Message);
                        errores++;
                        NomadEnvironment.GetCurrentTransaction().Rollback();
                    }

                }
            }            

            procesados = linea - 1;
            objBatch.Log("Registros Procesados: " + procesados.ToString() + " - Importados: " + (procesados - errores).ToString());
            objBatch.Log("Finalizado...");

        }
    }
}


