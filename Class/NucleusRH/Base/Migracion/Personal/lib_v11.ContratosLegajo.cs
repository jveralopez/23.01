using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.ContratosLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Contratos por Legajo
    public partial class CONTRATO_PER 
    {
        public static void ImportarContratosLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Contratos por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.ContratosLegajo.CONTRATO_PER objRead;
            DateTime fCompare = new DateTime(1900, 1, 1);            

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
                    objRead = NucleusRH.Base.Migracion.Personal.ContratosLegajo.CONTRATO_PER.Get(row.GetAttr("id"));

                    string oiEMP = "", oiTC = "", oiUTPO = "", oiCON = "";
                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }                    
                    if (objRead.c_tipo_contrato == "")
                    {
                        objBatch.Err("No se especificó el Tipo de Contrato, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_contrato == "")
                    {
                        objBatch.Err("No se especificó el Contrato, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_inicioNull || objRead.f_inicio < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha de Inicio, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    
                    //Recupero el Tipo de Contrato
                    oiTC = NomadEnvironment.QueryValue("PER28_TIPOS_CONTR", "oi_tipo_contrato", "c_tipo_contrato", objRead.c_tipo_contrato, "", true);
                    if (oiTC == null)
                    {
                        objBatch.Err("El Tipo de Contrato no existe, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Contrato
                    oiCON = NomadEnvironment.QueryValue("PER28_CONTRATOS", "oi_contrato", "c_contrato", objRead.c_contrato, "PER28_CONTRATOS.oi_tipo_contrato = " + oiTC, true);                    
                    if (oiCON == null)
                    {
                        objBatch.Err("El Contrato no existe, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.c_unidad_tiempo != "")
                    {
                        //Recupero la Unidad de Tiempo  	
                        oiUTPO = NomadEnvironment.QueryValue("ORG25_UNIDADES_TPO", "oi_unidad_tiempo", "c_unidad_tiempo", objRead.c_unidad_tiempo, "", true);
                        if (oiUTPO == null)
                        {
                            objBatch.Err("La Unidad de Tiempo no existe, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.e_nro_legajo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de contrato en la fecha especificada para la persona  	
                    if (DDOLEG.CONTR_PER.GetByAttribute("f_inicio", objRead.f_inicio) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Contrato en el Legajo para la Fecha, '" + objRead.c_contrato + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo Contrato en el Legajo			
                    NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER DDOCONPER;
                    DDOCONPER = new NucleusRH.Base.Personal.LegajoEmpresa.CONTRATO_PER();

                    DDOCONPER.e_duracion = objRead.e_duracion;
                    DDOCONPER.f_fin = objRead.f_fin;
                    DDOCONPER.f_finNull = objRead.f_finNull;
                    DDOCONPER.f_inicio = objRead.f_inicio;
                    DDOCONPER.o_contrato_per = objRead.o_contrato_per;                    
                    if (oiCON != "") DDOCONPER.oi_contrato = oiCON;
                    if (oiUTPO != "") DDOCONPER.oi_unidad_tiempo = oiUTPO;

                    //Agrego el contrato
                    DDOLEG.CONTR_PER.Add(DDOCONPER);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            try
            {
                NucleusRH.Base.Migracion.Interfaces.INTERFACE.Grabar(htPARENTS);
            }
            catch (Exception e)
            {
                objBatch.Err("Error al grabar - " + e.Message);
                Errores = Linea;
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");

        }

    }
}
