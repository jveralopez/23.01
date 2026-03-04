using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Puestos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Puestos
    public partial class PUESTO
    {
        public static void ImportarPuestos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Puestos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Puestos.PUESTO objRead;

            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));

            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");
            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.Migracion.Organizacion.Puestos.PUESTO.Get(row.GetAttr("id"));
                    string oiEMPRESA;
                    string oiBS = "", oiCONV = "", oiCAT = "";

                    //Trabajo la empresa
                    oiEMPRESA = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMPRESA == null)
                    {
                        objBatch.Err("El Código de la Empresa no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Puesto en la Empresa
                    string oiVal = NomadEnvironment.QueryValue("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto, "ORG04_PUESTOS.oi_empresa = " + oiEMPRESA, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Puesto '" + objRead.c_puesto + " - " + objRead.d_puesto + "' en la Empresa especificada - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {

                        #region validacion_anterior
                        //En caso que el Puesto sea de Convenio trabajo con la Categoria, de lo contrario con la Banda Salarial
                        /*if (objRead.l_convenio)
                        {
                            //Si el puesto es de Covennio si o si tiene que tener una Categoría
                            if (objRead.c_convenio == "" && objRead.c_categoria == "")
                            {
                                objBatch.Err("Se especificó que el Puesto a ingresar es de Convenio y no se especificó el Convenio y la Categoría al que pertence, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            else
                            {
                                //Recupero el Convenio para recuperar la Categoría
                                oiCONV = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);
                                if (oiCONV == null)
                                {
                                    objBatch.Err("El Código del Convenio no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                                //Recupero la Categoría
                                oiCAT = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + oiCONV, true);
                                if (oiCAT == null)
                                {
                                    objBatch.Err("El Código de la Categoria no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            //Si el puesto NO es de Covennio si o si tiene que tener una Banda Salarial
                            if (objRead.c_banda_sal == "")
                            {
                                objBatch.Err("Se especifico que el Puesto a ingresar no es de Convenio y no se especifico la Banda Salarial al que pertence, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            else
                            {
                                //Recupero la Banda Salarial
                                oiBS = NomadEnvironment.QueryValue("ORG17_BANDAS_SAL", "oi_banda_sal", "c_banda_sal", objRead.c_banda_sal, "", true);
                                if (oiBS == null)
                                {
                                    objBatch.Err("El Código de la Banda Salarial no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                            }
                        }*/
                        #endregion

                        //Ni la categoria-convenio ni la banda salarial son obligatorios
                        //En caso de que algun valor tenga valor deberá validar que ese exista o sea correcto
                        if (objRead.c_convenio != "" || objRead.c_categoria != "")
                        {
                            if (objRead.c_convenio == "")
                            {
                                objBatch.Err("Se especificó el Código de la Categoría pero no el del Convenio, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            if (objRead.c_categoria == "")
                            {
                                objBatch.Err("Se especificó el Código del Convenio pero no el de la Categoría, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            //Recupero el Convenio para recuperar la Categoría
                            oiCONV = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);
                            if (oiCONV == null)
                            {
                                objBatch.Err("El Código del Convenio no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            //Recupero la Categoría
                            oiCAT = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + oiCONV, true);
                            if (oiCAT == null)
                            {
                                objBatch.Err("El Código de la Categoria no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }                                                   
                        }

                        //En caso de que especifique Banda Salarial
                        if (objRead.c_banda_sal != "")
                        {
                            //Recupero la Banda Salarial
                            oiBS = NomadEnvironment.QueryValue("ORG17_BANDAS_SAL", "oi_banda_sal", "c_banda_sal", objRead.c_banda_sal, "", true);
                            if (oiBS == null)
                            {
                                objBatch.Err("El Código de la Banda Salarial no existe, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }                        

                        //Creo el Puesto
                        NucleusRH.Base.Organizacion.Puestos.PUESTO DDOPuesto;
                        DDOPuesto = new NucleusRH.Base.Organizacion.Puestos.PUESTO();

                        if (objRead.c_puesto == "" || objRead.d_puesto == "")
                        {
                            objBatch.Err("El Código o la Descripción del Puesto no es válido, se rechaza el registro '" + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOPuesto.c_puesto = objRead.c_puesto;
                        DDOPuesto.d_puesto = objRead.d_puesto;
                        DDOPuesto.c_confidencialidad = objRead.c_confidencialidad;
                        DDOPuesto.e_personas_cargo = objRead.e_personas_cargo;
                        DDOPuesto.e_personas_cargoNull = objRead.e_personas_cargoNull;
                        DDOPuesto.l_convenio = objRead.l_convenio;
                        DDOPuesto.n_costo = objRead.n_costo;
                        DDOPuesto.n_costoNull = objRead.n_costoNull;
                        /*if (objRead.l_convenio)
                            DDOPuesto.oi_categoria = oiCAT;
                        else
                            DDOPuesto.oi_banda_sal = oiBS;*/
                        DDOPuesto.oi_categoria = oiCAT;
                        DDOPuesto.oi_banda_sal = oiBS;
                        DDOPuesto.oi_empresa = oiEMPRESA;
                        DDOPuesto.o_puesto = objRead.o_puesto;

                        //Creo la Unidad Organizativa
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG DDOTUO = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get(2);
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG DDOUNIORG;
                        DDOUNIORG = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();
                        DDOUNIORG.c_unidad_org = objRead.c_empresa + "-" + objRead.c_puesto;
                        DDOUNIORG.d_unidad_org = objRead.d_puesto;
                        DDOUNIORG.o_unidad_org = objRead.o_puesto;

                        //Guardo la Unidad Organizativa
                        DDOTUO.UNI_ORG.Add(DDOUNIORG);
                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOTUO);
                        DDOPuesto.oi_unidad_org = DDOTUO.UNI_ORG.GetByAttribute("c_unidad_org", objRead.c_empresa + "-" + objRead.c_puesto).Id;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOPuesto);
                            NomadEnvironment.QueryValueChange("ORG04_PUESTOS", "oi_puesto", "c_puesto", objRead.c_puesto, "ORG04_PUESTOS.oi_empresa = " + oiEMPRESA,"1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_puesto + " - " + objRead.d_puesto + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                            Errores++;
                        }
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

