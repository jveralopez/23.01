using System;
using System.Xml;
using System.IO;
using System.Collections;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Capacitacion.MigClases
{
    public partial class CLASE : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarClases()
        {

            int Linea = 0, Errores = 0, Importados= 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Clases de Dictados");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigClases.CLASE objRead;
            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(Resources.QRY_REGISTROS, ""));
            ArrayList lista = (ArrayList)IDList.FirstChild().GetElements("ROW");

            Hashtable DDODicHas = new Hashtable();

            //RECORRO LOS IDS QUE ENTRAN Y PREGUNTO SI ALGUN ID NO ESTA EN LA HASH, PORQ DE SER ASI HAY QUE AGREGARLO
            for (int xml = 0; xml < lista.Count; xml++)
            {
                Linea++;
                NomadXML row = (NomadXML)lista[xml];
                objBatch.SetPro(0, 100, lista.Count, xml);
                objBatch.SetMess("Incorporando registro " + (xml + 1) + " de " + lista.Count);

                //Instancio el DICTADO
                NucleusRH.Base.Capacitacion.Dictados.DICTADO DDODIC = null;

                //Inicio la Transaccion
                try
                {
                    objRead = NucleusRH.Base.Capacitacion.MigClases.CLASE.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_curso == "")
                    {
                        objBatch.Err("No se especificó el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_dictado == "")
                    {
                        objBatch.Err("No se especificó el código del Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_fecha_hora_iniNull)
                    {
                        objBatch.Err("No se especificó la Fecha y Hora de Inicio del Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_fecha_hora_finNull)
                    {
                        objBatch.Err("No se especificó la Fecha y Hora de Fin del Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiDIC = "", oiCUR = "";

                    if (objRead.c_curso != "")
                    {
                        oiCUR = NomadEnvironment.QueryValue("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                        if (oiCUR == null)
                        {
                            objBatch.Err("El Curso no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_dictado != "")
                    {
                        oiDIC = NomadEnvironment.QueryValue("CYD02_DICTADOS", "oi_dictado", "c_dictado", objRead.c_dictado, "CYD02_DICTADOS.oi_curso = " + oiCUR, true);
                        if (oiDIC == null)
                        {
                            objBatch.Err("El Dictado no existe en el Curso, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }


                    if (oiDIC != null)
                    {                   
                        if (!DDODicHas.ContainsKey(oiDIC)) //Si no contiene instancio
                        {
                            DDODIC = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(oiDIC);
                        }
                        else //Si contiene recupero la instancia
                        {
                            DDODIC = (NucleusRH.Base.Capacitacion.Dictados.DICTADO)DDODicHas[oiDIC];
                        }                       

                        //Creo la clase
                        NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO DDOCLADIC;
                        DDOCLADIC = new NucleusRH.Base.Capacitacion.Dictados.CLASE_DICTADO();

                        DDOCLADIC.f_fecha_hora_fin = objRead.f_fecha_hora_fin;
                        DDOCLADIC.f_fecha_hora_ini = objRead.f_fecha_hora_ini;
                        DDOCLADIC.oi_dictado = Nomad.NSystem.Functions.StringUtil.str2int(oiDIC);

                        //Agrego la Clase
                        DDODIC.CLASES_DICT.Add(DDOCLADIC);

                        //Guardo la instancia del dictado con las clases en la hashtable
                        DDODicHas[oiDIC] = DDODIC;
                    }

                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            //Guardo el dictado con sus clases
            foreach (NucleusRH.Base.Capacitacion.Dictados.DICTADO DIC in DDODicHas.Values)
            {
                //Grabo
                try
                {
                    //Validar que la duracion total de las clases no supere la duracion del dictado
                    NomadEnvironment.GetCurrentTransaction().Save(DIC);
                    Importados++;
                }
                catch (Exception e)
                {
                    objBatch.Err("Error al grabar registro del dictado: " + DIC.c_dictado + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Se importaron las clases de: " + Importados.ToString() + " Dictados - Errores: " + Errores.ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

