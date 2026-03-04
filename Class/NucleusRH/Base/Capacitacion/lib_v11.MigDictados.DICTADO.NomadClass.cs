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

namespace NucleusRH.Base.Capacitacion.MigDictados
{
    public partial class DICTADO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarDictados()
        {
            int Linea = 0, Errores = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Dictados");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigDictados.DICTADO objRead;

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
                    objRead = NucleusRH.Base.Capacitacion.MigDictados.DICTADO.Get(row.GetAttr("id"));

                    //Valido atributos obligatorios
                    if (objRead.c_curso == "")
                    {
                        objBatch.Err("No se especificó el Curso, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_dictado == "" || objRead.d_dictado == "")
                    {
                        objBatch.Err("No se especificó el código o la descripción del Dictado, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_inicioNull)
                    {
                        objBatch.Err("No se especificó la Fecha de Inicio, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_finNull)
                    {
                        objBatch.Err("No se especificó Fecha de Fin, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_estado == "")
                    {
                        objBatch.Err("No se especificó Estado del Dictado, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_estadoNull)
                    {
                        objBatch.Err("No se especificó Fecha de Estado, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    if (objRead.l_cursoNull)
                    {
                        objBatch.Err("No se especificó si pasa el curso a personal, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.l_competenciaNull)
                    {
                        objBatch.Err("No se especificó si pasa la competencia al Personal, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.l_externoNull)
                    {
                        objBatch.Err("No se especificó si es Externo, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiDIC = "", oiPRO = "", oiCUR = "";

                    if (objRead.c_curso != "")
                    {
                        oiCUR = NomadEnvironment.QueryValue("CYD01_CURSOS", "oi_curso", "c_curso", objRead.c_curso, "", true);
                        if (oiCUR == null)
                        {
                            objBatch.Err("El Curso no existe, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_proveedor != "")
                    {
                        oiPRO = NomadEnvironment.QueryValue("CYD07_PROVEEDORES", "oi_proveedor", "c_proveedor", objRead.c_proveedor, "", true);
                        if (oiPRO == null)
                        {
                            objBatch.Err("El proveedor no existe, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }
                    }

                    if (objRead.c_prioridad != "" && objRead.c_prioridad != "A" && objRead.c_prioridad != "M" && objRead.c_prioridad != "B")
                    {
                        objBatch.Err("La prioridad tiene que tomar el valor A, M o B , se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Me fijo si ya existe el Dictado
                    oiDIC = NomadEnvironment.QueryValue("CYD02_DICTADOS", "oi_dictado", "c_dictado", objRead.c_dictado, "", true);
                    if (oiDIC != null)
                    {
                        objBatch.Err("El código del Dictado ya existe, se rechaza el registro '" + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    else
                    {
                        //Creo el DICTADO
                        NucleusRH.Base.Capacitacion.Dictados.DICTADO DDODIC;
                        DDODIC = new NucleusRH.Base.Capacitacion.Dictados.DICTADO();

                        DDODIC.oi_curso = oiCUR;
                        DDODIC.c_dictado = objRead.c_dictado;
                        DDODIC.d_dictado = objRead.d_dictado;
                        DDODIC.f_inicio = objRead.f_inicio;
                        DDODIC.f_fin = objRead.f_fin;
                        DDODIC.n_duracion_hs = objRead.n_duracion_hs;
                        DDODIC.c_estado = objRead.c_estado;
                        DDODIC.f_estado = objRead.f_estado;
                        DDODIC.l_competencia = objRead.l_competencia;
                        DDODIC.l_curso = objRead.l_curso;
                        DDODIC.l_externo = objRead.l_externo;
                        DDODIC.d_organizador = objRead.d_organizador;
                        DDODIC.e_cupo_conv = objRead.e_cupo_conv;
                        DDODIC.e_cupo_max = objRead.e_cupo_max;
                        DDODIC.e_cupo_min = objRead.e_cupo_min;
                        DDODIC.e_cupo_prev = objRead.e_cupo_prev;
                        DDODIC.e_cupo_real = objRead.e_cupo_real;
                        DDODIC.o_objetivos = objRead.o_objetivos;
                        DDODIC.c_tipo_asistencia = objRead.c_tipo_asistencia;
                        DDODIC.o_dictado = objRead.o_dictado;
                        DDODIC.oi_proveedor = oiPRO;
                        DDODIC.l_evaluable = objRead.l_evaluable;
                        DDODIC.c_prioridad = objRead.c_prioridad;
                        DDODIC.o_forma_eval = objRead.o_forma_eval;

                        //Grabo
                        try
                        {
                            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDODIC);
                            NomadEnvironment.QueryValueChange("CYD02_DICTADOS", "oi_dictado", "c_dictado", objRead.c_dictado, "","1", true);

                        }
                        catch (Exception e)
                        {
                            objBatch.Err("Error al grabar registro " + objRead.c_dictado + " - " + objRead.d_dictado + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                            Errores++;
                        }
                    }

                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignaci&#243;n de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

