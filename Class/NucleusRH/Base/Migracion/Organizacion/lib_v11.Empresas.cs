using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Empresas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Empresas
    public partial class EMPRESA 
    {
        public static void ImportarEmpresas()
        {
#line 1 "Clase.NucleusRH.Base.Migracion.Organizacion.Empresas.EMPRESA.Metodo.ImportarEmpresas"
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Empresas");
            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Empresas.EMPRESA objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Organizacion.Empresas.EMPRESA.Get(row.GetAttr("id"));
                    //Me fijo si ya existe la EMPRESA  	
                    string oiVal = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    string oiAct, oiTEmp = "", oiLoc = "";

                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Empresa '" + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Busco las FK correspondientes  		
                        //Actividades
                        if (objRead.c_actividadNull)
                        {
                            objBatch.Err("No se especifico la Actividad, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                        else
                        {
                            oiAct = NomadEnvironment.QueryValue("ORG34_ACTIVIDADES", "oi_actividad", "c_actividad", objRead.c_actividad, "", true);
                            if (oiAct == null)
                            {
                                objBatch.Err("No existe la Actividad especificada, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        //Tipos de Empresa
                        if (!objRead.c_tipo_empresaNull)
                        {
                            oiTEmp = NomadEnvironment.QueryValue("ORG05_TIPOS_EMPR", "oi_tipo_empresa", "c_tipo_empresa", objRead.c_tipo_empresa, "", true);
                            if (oiTEmp == null)
                            {
                                objBatch.Err("No existe el Tipo de Empresa especificado, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }

                        //Localidades
                        if (!objRead.c_localidadNull)
                        {
                            oiLoc = NomadEnvironment.QueryValue("ORG19_LOCALIDADES", "oi_localidad", "c_localidad", objRead.c_localidad, "", true);
                            if (oiLoc == null)
                            {
                                objBatch.Err("No existe la Localidad especificada, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                        }
                        //Creo la EMPRESA
                        NucleusRH.Base.Organizacion.Empresas.EMPRESA DDOEMPRESA;
                        DDOEMPRESA = new NucleusRH.Base.Organizacion.Empresas.EMPRESA();

                        if (objRead.c_empresa == "" || objRead.d_empresa == "")
                        {
                            objBatch.Err("El Código o la Descripción de la Empresa no es válido, se rechaza el registro '" + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOEMPRESA.c_empresa = objRead.c_empresa;
                        DDOEMPRESA.d_empresa = objRead.d_empresa;
                        DDOEMPRESA.c_cuit = objRead.c_cuit;
                        DDOEMPRESA.d_calle = objRead.d_calle;
                        DDOEMPRESA.d_departamento = objRead.d_departamento;
                        DDOEMPRESA.d_domic_radic = objRead.d_domic_radic;
                        DDOEMPRESA.d_domicilio = objRead.d_calle + " " + objRead.e_numero + " " + objRead.e_piso + " " + objRead.d_departamento;
                        DDOEMPRESA.dc_empresa = objRead.dc_empresa;
                        DDOEMPRESA.e_numero = objRead.e_numero;
                        DDOEMPRESA.e_numeroNull = objRead.e_numeroNull;
                        DDOEMPRESA.e_piso = objRead.e_piso;
                        DDOEMPRESA.e_pisoNull = objRead.e_pisoNull;
                        DDOEMPRESA.f_cese_act = objRead.f_cese_act;
                        DDOEMPRESA.f_cese_actNull = objRead.f_cese_actNull;
                        DDOEMPRESA.f_inicio_act = objRead.f_inicio_act;
                        DDOEMPRESA.f_inicio_actNull = objRead.f_inicio_actNull;
                        DDOEMPRESA.o_empresa = objRead.o_empresa;
                        if (oiAct != "") DDOEMPRESA.oi_actividad = oiAct;
                        if (oiLoc != "") DDOEMPRESA.oi_localidad = oiLoc;
                        if (oiTEmp != "") DDOEMPRESA.oi_tipo_empresa = oiTEmp;
                        DDOEMPRESA.te_telefono = objRead.te_telefono;

                        //Creo el calendario ESTANDAR para la empresa			
                        NucleusRH.Base.Organizacion.Empresas.FERIADO_EMP DDOCAL;
                        DDOCAL = new NucleusRH.Base.Organizacion.Empresas.FERIADO_EMP();
                        DDOCAL.oi_calendario = NucleusRH.Base.Migracion.Interfaces.INTERFACE.FindOI("ORG27_CAL_FERIADOS", "oi_calendario", "c_calendario", "STD");
                        DDOEMPRESA.FERIADOS_EMP.Add(DDOCAL);

                        //Creo la Unidad Organizativa
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG DDOTUO = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get(1);
                        NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG DDOUNIORG;
                        DDOUNIORG = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();

                        DDOUNIORG.c_unidad_org = objRead.c_empresa;
                        DDOUNIORG.d_unidad_org = objRead.d_empresa;
                        DDOUNIORG.o_unidad_org = objRead.o_empresa;

                        //Guardo la Unidad Organizativa
                        DDOTUO.UNI_ORG.Add(DDOUNIORG);
                        NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOTUO);

                        DDOEMPRESA.oi_unidad_org = DDOTUO.UNI_ORG.GetByAttribute("c_unidad_org", objRead.c_empresa).Id;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOEMPRESA);
                            NomadEnvironment.QueryValueChange("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_empresa + " - " + objRead.d_empresa + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
#line default
        }
    }
}
