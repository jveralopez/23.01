using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Organizacion.Departamentos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Ubicaciones Org
    public partial class DEPARTAMENTO
    {
        public static void ImportarDepartamentos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Departamentos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Organizacion.Departamentos.DEPARTAMENTO objRead;            

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
                    //Cargo el registro
                    objRead = NucleusRH.Base.Migracion.Organizacion.Departamentos.DEPARTAMENTO.Get(row.GetAttr("id"));
                 
                    //Me fijo si llegan los atributos obligatorios
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No especificó la empresa, se rechaza el registro '" + objRead.c_departamento + " - " + objRead.d_departamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_departamento == "")
                    {
                        objBatch.Err("No especificó el codigo de departamento, se rechaza el registro '" + objRead.c_departamento + " - " + objRead.d_departamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_departamento == "")
                    {
                        objBatch.Err("No especificó la descripcion del departamento, se rechaza el registro '" + objRead.c_departamento + " - " + objRead.d_departamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Cargo el Padre  	
                    string oiEmp = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                    if (oiEmp == null)
                    {
                        objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_departamento + " - " + objRead.d_departamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el departamento	
                    string oiVal = NomadEnvironment.QueryValue("ORG51_DEPARTAMENTOS", "oi_departamento", "c_departamento", objRead.c_departamento, "ORG51_DEPARTAMENTOS.oi_empresa = " + oiEmp, true);
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Departamento '" + objRead.c_departamento + " - " + objRead.d_departamento + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                   
                        //Creo el departamento
                     NucleusRH.Base.Organizacion.Departamentos.DEPARTAMENTO departamento = new NucleusRH.Base.Organizacion.Departamentos.DEPARTAMENTO();

                    departamento.oi_empresa = oiEmp.ToString();
                    departamento.c_departamento = objRead.c_departamento;
                    departamento.d_departamento = objRead.d_departamento;


                    NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG DDOTUO = NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get(6);
                    NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG DDOUNIORG;
                    DDOUNIORG = new NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG();

                    DDOUNIORG.c_unidad_org = departamento.Getoi_empresa().c_empresa + "-" + objRead.c_departamento;            
                    DDOUNIORG.d_unidad_org = objRead.d_departamento;
                   
                    //Guardo la Unidad Organizativa
                    DDOTUO.UNI_ORG.Add(DDOUNIORG);
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDOTUO);

                    //Asigno la unidad al centro de costos
                    departamento.oi_unidad_org = DDOTUO.UNI_ORG.GetByAttribute("c_unidad_org", departamento.Getoi_empresa().c_empresa + "-" + objRead.c_departamento).Id;

                    //Grabo
                    try
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(departamento);
                        NomadEnvironment.QueryValueChange("ORG51_DEPARTAMENTOS", "oi_departamento", "c_departamento", objRead.c_departamento, "ORG51_DEPARTAMENTOS.oi_empresa = " + oiEmp,"1", true);

                    }
                    catch (Exception e)
                    {
                        objBatch.Err("Error al grabar registro " + objRead.c_departamento + " - " + objRead.d_departamento + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
