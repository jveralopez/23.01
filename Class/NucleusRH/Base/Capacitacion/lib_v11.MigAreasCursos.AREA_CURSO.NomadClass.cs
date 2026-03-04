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

namespace NucleusRH.Base.Capacitacion.MigAreasCursos
{
    public partial class AREA_CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarAreasCursos()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Áreas de Cursos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigAreasCursos.AREA_CURSO objRead;
            
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
                    objRead = NucleusRH.Base.Capacitacion.MigAreasCursos.AREA_CURSO.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Área de Curso
                    string oiVal = NomadEnvironment.QueryValue("ORG06_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para el Área de Curso '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Área de Curso
                        NucleusRH.Base.Organizacion.Cursos.CURSO DDOCUR;
                        DDOCUR = new NucleusRH.Base.Organizacion.Cursos.CURSO();

                        if (objRead.c_curso == "" || objRead.d_curso == "")
                        {
                            objBatch.Err("El Código o la Descripción del Área de Curso no es válido, se rechaza el registro '" + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOCUR.c_curso = objRead.c_curso;
                        DDOCUR.d_curso = objRead.d_curso;
                        DDOCUR.o_curso = objRead.o_curso;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCUR);
                            NomadEnvironment.QueryValueChange("ORG06_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "","1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_curso + " - " + objRead.d_curso + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
