using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Centros_de_Costos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Centros de Costo
    public partial class CENTRO_COSTO 
    {
        public static void ImportarCentrosCostos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Centros de Costos");
            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Centros_de_Costos.CENTRO_COSTO objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Organizacion.Centros_de_Costos.CENTRO_COSTO.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Centro de Costo
                    string oiVal = NomadEnvironment.QueryValue("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", objRead.c_centro_costo, "", true);
                    string oiGRCC = "";

                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Centro de Costo '" + objRead.c_centro_costo + " - " + objRead.d_centro_costo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Trabajo el Grupo de Centro de Costos
                        if (objRead.c_grupo_ccosto != "")
                        {
                            oiGRCC = NomadEnvironment.QueryValue("ORG09_GR_CCOSTO", "oi_grupo_ccosto", "c_grupo_ccosto", objRead.c_grupo_ccosto, "", true);
                            if (oiGRCC == null)
                            {
                                NucleusRH.Base.Organizacion.Grupos_Centros_Costo.GRUPO_CCOSTO DDOGRCC;
                                DDOGRCC = new NucleusRH.Base.Organizacion.Grupos_Centros_Costo.GRUPO_CCOSTO();

                                if (objRead.d_grupo_ccosto == "")
                                {
                                    objBatch.Err("La Descripción del Grupo de Centro de Costo no es válida, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.d_centro_costo + "' - Linea: " + Linea.ToString());
                                    Errores++;
                                    continue;
                                }
                                DDOGRCC.c_grupo_ccosto = objRead.c_grupo_ccosto;
                                DDOGRCC.d_grupo_ccosto = objRead.d_grupo_ccosto;

                                //Grabo
                                NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOGRCC);
                                oiGRCC = DDOGRCC.Id;
                                NomadEnvironment.QueryValueChange("ORG09_GR_CCOSTO", "oi_grupo_ccosto", "c_grupo_ccosto", objRead.c_grupo_ccosto, "", oiGRCC, true);
                                
                            }
                        }

                        //Creo el Centro de Costos
                        NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO DDOCC;
                        DDOCC = new NucleusRH.Base.Organizacion.Centros_de_Costos.CENTRO_COSTO();

                        if (objRead.c_centro_costo == "" || objRead.d_centro_costo == "")
                        {
                            objBatch.Err("El Código o la Descripción del Centro de Costo no es válido, se rechaza el registro '" + objRead.c_centro_costo + " - " + objRead.d_centro_costo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOCC.c_centro_costo = objRead.c_centro_costo;
                        DDOCC.d_centro_costo = objRead.d_centro_costo;
                        if (oiGRCC != "") DDOCC.oi_grupo_ccosto = oiGRCC;
                        DDOCC.o_centro_costo = objRead.o_centro_costo;

                        //Creo la Unidad Organizativa
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG DDOTUO = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get(4);
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG DDOUNIORG;
                        DDOUNIORG = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();

                        DDOUNIORG.c_unidad_org = objRead.c_centro_costo;
                        DDOUNIORG.d_unidad_org = objRead.d_centro_costo;
                        DDOUNIORG.o_unidad_org = objRead.o_centro_costo;

                        //Guardo la Unidad Organizativa
                        DDOTUO.UNI_ORG.Add(DDOUNIORG);
                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOTUO);

                        //Asigno la unidad al centro de costos
                        DDOCC.oi_unidad_org = DDOTUO.UNI_ORG.GetByAttribute("c_unidad_org", objRead.c_centro_costo).Id;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCC);
                            NomadEnvironment.QueryValueChange("ORG08_CS_COSTO", "oi_centro_costo", "c_centro_costo", objRead.c_centro_costo, "","1", true);

                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_centro_costo + " - " + objRead.d_centro_costo + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
