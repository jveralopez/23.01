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

namespace NucleusRH.Base.Capacitacion.MigCatCursos
{
    public partial class CAT_CURSO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarCatCursos()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Categorías de Cursos");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigCatCursos.CAT_CURSO objRead;
            
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
                    objRead = NucleusRH.Base.Capacitacion.MigCatCursos.CAT_CURSO.Get(row.GetAttr("id"));
                    //Me fijo si ya existe el Catagoría de Curso
                    string oiVal = NomadEnvironment.QueryValue("CYD06_CATEG_CURSO", "oi_categ_curso", "c_categ_curso", objRead.c_categ_curso, "", true);
                    
                    if (oiVal != null)
                    {
                        objBatch.Err("Ya existe un registro para la Catagoría de Curso '" + objRead.c_categ_curso + " - " + objRead.d_categ_curso + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el Catagoría de Curso   
                        NucleusRH.Base.Capacitacion.CategoriasCurso.CATEG_CURSO DDOCATCUR;
                        DDOCATCUR = new NucleusRH.Base.Capacitacion.CategoriasCurso.CATEG_CURSO();

                        if (objRead.c_categ_curso == "" || objRead.d_categ_curso == "")
                        {
                            objBatch.Err("El Código o la Descripción de la Catagoría de Curso no es válido, se rechaza el registro '" + objRead.c_categ_curso + " - " + objRead.d_categ_curso + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        DDOCATCUR.c_categ_curso = objRead.c_categ_curso;
                        DDOCATCUR.d_categ_curso = objRead.d_categ_curso;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().Save(DDOCATCUR);
                            NomadEnvironment.QueryValueChange("CYD06_CATEG_CURSO", "oi_categ_curso", "c_categ_curso", objRead.c_categ_curso, "", "1", true);
                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_categ_curso + " - " + objRead.d_categ_curso + "' - Linea: " + Linea.ToString() + " - " + e.Message);
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
