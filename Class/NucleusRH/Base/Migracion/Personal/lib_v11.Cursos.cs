using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Migracion.Personal.Cursos
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Cursos Personal
    public partial class CURSO_PER 
    {
        public static void ImportarCursos()
        {

            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Cursos");

            Hashtable htPARENTS = new Hashtable();            
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Migracion.Personal.Cursos.CURSO_PER objRead;            

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
                    objRead = NucleusRH.Base.Migracion.Personal.Cursos.CURSO_PER.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios  	                                             
                    if (objRead.c_persona == "")
                    {
                        objBatch.Err("No se especificó la Persona, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_area_curso == "")
                    {
                        objBatch.Err("No se especificó el Area de Curso, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.d_titulo == "")
                    {
                        objBatch.Err("No se especificó el Titulo del Curso, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiAREACUR = "", oiUTPO = ""; ;

                    if (objRead.c_area_curso != "")
                    {
                        oiAREACUR = NomadEnvironment.QueryValue("ORG06_CURSOS", "oi_curso", "c_curso", objRead.c_area_curso, "", true);
                        if (oiAREACUR == null)
                        {
                            objBatch.Err("El Area de Curso no existe, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_unidad_tiempo != "")
                    {
                        oiUTPO = NomadEnvironment.QueryValue("ORG25_UNIDADES_TPO", "oi_unidad_tiempo", "c_unidad_tiempo", objRead.c_unidad_tiempo, "", true);
                        if (oiUTPO == null)
                        {
                            objBatch.Err("La Unidad de Tiempo no existe, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    //Cargo el Padre  	                  
                    string oiPER = NomadEnvironment.QueryValue("PER01_PERSONAL", "oi_personal", "c_personal", objRead.c_persona, "", true);
                    NucleusRH.Base.Personal.Legajo.PERSONAL DDOPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(oiPER);
                    if (DDOPER == null)
                    {
                        objBatch.Err("La Persona no existe, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Busco cursos duplicados
                    NomadXML myXml = new NomadXML();

                    NomadProxy proxy = NomadProxy.GetProxy();
                    string fechaFin = objRead.f_fin.ToString("yyyyMMdd");

                    myXml.SetText(proxy.SQLService().Get(NucleusRH.Base.Migracion.Personal.Cursos.CURSO_PER.Resources.QRY_DUPLICADAS, "<DATA oi_personal='" + DDOPER.id + "' oi_curso='" + oiAREACUR + "' f_fin_curso='" + fechaFin+ "' d_titulo = '"+objRead.d_titulo+"' />"));
                    if (myXml.FirstChild().FirstChild() != null)
                    {
                        objBatch.Err("El curso esta duplicado, se rechaza el registro '" + objRead.c_persona + " - " + objRead.d_titulo + "' - Linea: " + Linea.ToString());
                        Errores++;
                    }
                    else
                    {
                        //Creo el curso
                        NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER DDOCURPER;
                        DDOCURPER = new NucleusRH.Base.Personal.Legajo.CURSO_EXT_PER();

                        DDOCURPER.d_curso_ext_per = objRead.d_titulo;
                        DDOCURPER.d_lugar = objRead.d_lugar;
                        DDOCURPER.n_duracion = objRead.n_duracion;
                        if (objRead.f_finNull) DDOCURPER.f_fin_cursoNull = true; else DDOCURPER.f_fin_curso = objRead.f_fin;
                        DDOCURPER.l_certificado = objRead.l_certificado;
                        DDOCURPER.l_externo = objRead.l_externo;
                        DDOCURPER.o_curso = objRead.o_curso_per;
                        if (oiAREACUR != "") DDOCURPER.oi_curso = oiAREACUR;
                        if (oiUTPO != "") DDOCURPER.oi_unidad_tiempo = oiUTPO;

                        //Agrego el curso
                        DDOPER.CURSOS_PER.Add(DDOCURPER);
                        NomadEnvironment.GetCurrentTransaction().Save(DDOPER);
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
