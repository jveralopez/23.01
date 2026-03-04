using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.TiposPersonalLegajo
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Tipos Personal por Legajo
    public partial class TPER_PER 
    {
        public static void ImportarTiposPersonalLegajos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Tipos Personal por Legajos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.TiposPersonalLegajo.TPER_PER objRead;
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
                    objRead = NucleusRH.Base.Migracion.Personal.TiposPersonalLegajo.TPER_PER.Get(row.GetAttr("id"));

                    string oiEMP = "", oiTIPOPER = "", oiMC = "";
                    //Valido atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó el Código de la Empresa, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.e_nro_legajo == "")
                    {
                        objBatch.Err("No se especificó el Número de Legajo, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_tipo_personal == "")
                    {
                        objBatch.Err("No se especificó el Código de Tipo de Personal, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_inicioNull || objRead.f_inicio < fCompare)
                    {
                        objBatch.Err("No se especificó la Fecha de Ingreso, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero la Empresa
                    oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEMP == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Tipo Personal
                    oiTIPOPER = NomadEnvironment.QueryValue("PER11_TIPOS_PERS", "oi_tipo_personal", "c_tipo_personal", objRead.c_tipo_personal, "", true);
                    if (oiTIPOPER == null)
                    {
                        objBatch.Err("El Tipo Personal no existe, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero el Motivo de Cambio
                    if (objRead.c_motivo_cambio != "")
                    {
                        oiMC = NomadEnvironment.QueryValue("PER05_MOT_CAMBIO", "oi_motivo_cambio", "c_motivo_cambio", objRead.c_motivo_cambio, "", true);
                        if (oiMC == null)
                        {
                            objBatch.Err("El Motivo de Cambio no existe, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Valido que la fecha de egreso NO sea anterior a la de inicio
                    if(!(objRead.f_finNull) && objRead.f_fin<objRead.f_inicio)
                    {
                        objBatch.Err("La Fecha de Egreso no puede ser anterior a la Fecha de Ingreso, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Padre  	
                    NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP DDOLEG = NucleusRH.Base.Migracion.Interfaces.INTERFACE.GetLegajo(objRead.e_nro_legajo, oiEMP, htPARENTS);
                    if (DDOLEG == null)
                    {
                        objBatch.Err("El Legajo no existe en la Empresa, se rechaza el registro '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe un registro de tipo de personal en la fecha especificada para la persona  	
                    if (DDOLEG.TIPOSP_PER.GetByAttribute("f_ingreso", objRead.f_inicio) != null)
                    {
                        objBatch.Err("Ya existe un Ingreso de Tipo Personal en el Legajo para la Fecha, '" + objRead.e_nro_legajo + " - " + objRead.f_inicio.ToString("dd/MM/yyyy") + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Tipo Personal en el Legajo			
                    NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER DDOTIPOPER;
                    DDOTIPOPER = new NucleusRH.Base.Personal.LegajoEmpresa.TIPOP_PER();

                    DDOTIPOPER.f_ingreso = objRead.f_inicio;
                    if(!objRead.f_finNull)
                        DDOTIPOPER.f_egreso = objRead.f_fin;
                    DDOTIPOPER.f_egresoNull = objRead.f_finNull;
                    DDOTIPOPER.o_cambio_tper = objRead.observaciones;
                    DDOTIPOPER.oi_tipo_personal = oiTIPOPER;                    
                    if (oiMC != "") DDOTIPOPER.oi_motivo_cambio = oiMC;

                    //Agrego el tipo de personal
                    DDOLEG.TIPOSP_PER.Add(DDOTIPOPER);
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
