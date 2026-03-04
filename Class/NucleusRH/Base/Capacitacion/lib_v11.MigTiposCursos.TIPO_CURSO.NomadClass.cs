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

namespace NucleusRH.Base.Capacitacion.MigTiposCursos
{
    public partial class TIPO_CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarTiposCursos()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Tipos de Cursos");
                        
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigTiposCursos.TIPO_CURSO objRead;
            
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
                    objRead = NucleusRH.Base.Capacitacion.MigTiposCursos.TIPO_CURSO.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Tipo de Curso
                    string oiVal = NomadEnvironment.QueryValue("CYD05_TIPOS_CURSO", "oi_tipo_curso", "c_tipo_curso", objRead.c_tipo_curso, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Tipo de Curso '" + objRead.c_tipo_curso + " - " + objRead.d_tipo_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Tipo de Curso   
                        NucleusRH.Base.Capacitacion.TiposCurso.TIPO_CURSO DDOTIPCUR;
                        DDOTIPCUR = new NucleusRH.Base.Capacitacion.TiposCurso.TIPO_CURSO();

                        if (objRead.c_tipo_curso == "" || objRead.d_tipo_curso == "")
                        {
                            objBatch.Err("El Código o la Descripción del Tipo de Curso no es válido, se rechaza el registro '" + objRead.c_tipo_curso + " - " + objRead.d_tipo_curso + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOTIPCUR.c_tipo_curso = objRead.c_tipo_curso;
                        DDOTIPCUR.d_tipo_curso = objRead.d_tipo_curso;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOTIPCUR);
                            NomadEnvironment.QueryValueChange("CYD05_TIPOS_CURSO", "oi_tipo_curso", "c_tipo_curso", objRead.c_tipo_curso, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_tipo_curso + " - " + objRead.d_tipo_curso + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
