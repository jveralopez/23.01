using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.CategoriasLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Categorías Personal
    public partial class CATEGORIA_PER 
    {
        public static void ImportarCategoriasLegajos()
        {

            int Linea = 0, Errores = 0;
            string oiPERANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Categorías por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.CategoriasLegajo.CATEGORIA_PER objRead;
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
                    objRead = NucleusRH.Base.Migracion.Personal.CategoriasLegajo.CATEGORIA_PER.Get(row.GetAttr("id"));

                    string oiEMP = "", oiPEREMP = "", oiMC = "", oiCONV = "", oiCAT = "";
                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_convenio == "")
                    {
                        objBatch.Err("No se especificó el Convenio, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_categoria == "")
                    {
                        objBatch.Err("No se especificó la Categoría, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_ingresoNull || objRead.f_ingreso < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha de Ingreso, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero la Empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero el Convenio  	
                    oiCONV = NomadEnvironment.QueryValue("ORG18_CONVENIOS", "oi_convenio", "c_convenio", objRead.c_convenio, "", true);
                    if (oiCONV == null)
                    {
                        objBatch.Err("El Convenio no existe, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero la Categoria  	
                    oiCAT = NomadEnvironment.QueryValue("ORG18_CATEGORIAS", "oi_categoria", "c_categoria", objRead.c_categoria, "ORG18_CATEGORIAS.oi_convenio = " + oiCONV, true);
                    if (oiCAT == null)
                    {
                        objBatch.Err("La Categoría no existe en el Convenio, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    //Recupero el Motivo de Cambio
                    if (objRead.c_motivo_cambio != "")
                    {
                        oiMC = NomadEnvironment.QueryValue("PER05_MOT_CAMBIO", "oi_motivo_cambio", "c_motivo_cambio", objRead.c_motivo_cambio, "", true);
                        if (oiMC == null)
                        {
                            objBatch.Err("El Motivo de Cambio no existe, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }
                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe, se rechaza el registro '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de categoria en la fecha especificada para la persona  	
                    if (DDOLEG.CATEG_PER.GetByAttribute("f_ingreso", objRead.f_ingreso) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Categoría en el Legajo para la Fecha, '" + objRead.c_categoria + " - " + objRead.f_ingreso.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo la Categoria en el Legajo			
                    NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER DDOCATEGPER;
                    DDOCATEGPER = new NucleusRH.Base.Personal.LegajoEmpresa.CATEGORIA_PER();
                    DDOCATEGPER.f_egreso = objRead.f_egreso;
                    DDOCATEGPER.f_egresoNull = objRead.f_egresoNull;
                    DDOCATEGPER.f_ingreso = objRead.f_ingreso;
                    DDOCATEGPER.oi_categoria = oiCAT;
                    DDOCATEGPER.o_cambio_categoria = objRead.o_cambio_categoria;

                    if (oiMC != "") DDOCATEGPER.oi_motivo_cambio = oiMC;

                    //Agrego la categoria
                    DDOLEG.CATEG_PER.Add(DDOCATEGPER);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message + " - " + e.StackTrace);
                    objBatch.Err(e.StackTrace);
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
