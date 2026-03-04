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

namespace NucleusRH.Base.Capacitacion.MigInscriptos
{
    public partial class INSCRIPTO : Nomad.NSystem.Base.NomadObject
    {
        public static void ImportarInscriptos()
        {
            //Instancio el DICTADO
            NucleusRH.Base.Capacitacion.Dictados.DICTADO DDODIC = null;

            int Linea = 0, Errores = 0;
            string oiDICANT = "";

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Importación de Inscriptos a Dictados");

            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Capacitacion.MigInscriptos.INSCRIPTO objRead;
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
                    objRead = NucleusRH.Base.Capacitacion.MigInscriptos.INSCRIPTO.Get(row.GetAttr("id"));

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
                    if (objRead.c_empresa == "")
                    {
                        objBatch.Err("No se especificó la Empresa, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_personal == "")
                    {
                        objBatch.Err("No se especificó el Código de la Persona, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_estado == "")
                    {
                        objBatch.Err("No se especificó el Estado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.c_estado_aprob == "")
                    {
                        objBatch.Err("No se especificó el Estado de Aprobación, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }
                    if (objRead.f_estadoNull)
                    {
                        objBatch.Err("No se especificó la Fecha de Estado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Recupero los OI de los códigos ingresados
                    string oiDIC = "", oiEMP = "", oiCUR = "", oiPER_EMP = "";

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
                    //Recuperar la empresa
                    if (objRead.c_empresa != "")
                    {
                        oiEMP = NomadEnvironment.QueryValue("ORG03_EMPRESAS", "oi_empresa", "c_empresa", objRead.c_empresa, "", true);
                        if (oiEMP == null)
                        {
                            objBatch.Err("La Empresa no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                            Errores++;
                            continue;
                        }

                        //Recuperar el legajo
                        if (objRead.c_personal != "")
                        {
                            oiPER_EMP = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "e_numero_legajo", objRead.c_personal, "PER02_PERSONAL_EMP.oi_empresa= " + oiEMP, true);
                            if (oiPER_EMP == null)
                            {
                                objBatch.Err("El legajo no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }

                            /*//Recuperar el legajo
                            oiPER_EMP = NomadEnvironment.QueryValue("PER02_PERSONAL_EMP", "oi_personal_emp", "oi_personal", oiPER, "PER02_PERSONAL_EMP.oi_empresa="+oiEMP, true);
                            if (oiPER_EMP==null)
                            {
                                objBatch.Err("El legajo no existe, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                                Errores++;
                                continue;
                            }
                            */
                        }
                    }

                    if (oiDICANT != oiDIC)
                    {
                        if (DDODIC == null || DDODIC.Id != oiDIC)
                        {
                            if (DDODIC != null)
                            {
                                //Grabo
                                try
                                {
                                    NomadEnvironment.GetCurrentTransaction().Save(DDODIC);
                                }
                                catch (Exception e)
                                {
                                    objBatch.Err("Error al grabar registro " + DDODIC.c_dictado + "' - Linea: " + Linea.ToString() + " - " + e.Message);
                                    Errores++;
                                }
                            }
                        }
                        if (DDODIC != null) oiDICANT = DDODIC.Id; else oiDICANT = oiDIC;
                        DDODIC = NucleusRH.Base.Capacitacion.Dictados.DICTADO.Get(oiDIC);
                    }

                    //Me fijo si ya existe el Inscripto en el DICTADO
                    if (DDODIC.INSCRIPTOS.GetByAttribute("oi_personal_emp", oiPER_EMP) != null)
                    {
                        objBatch.Err("Ya existe un registro para el Inscripto en el Dictado, se rechaza el registro '" + objRead.c_curso + " - " + objRead.c_dictado + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Inscripto
                    NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO DDOINSDIC;
                    DDOINSDIC = new NucleusRH.Base.Capacitacion.Dictados.INSCRIPTO();

                    DDOINSDIC.c_estado = objRead.c_estado;
                    DDOINSDIC.c_estado_aprob = objRead.c_estado_aprob;
                    DDOINSDIC.f_estado = objRead.f_estado;
                    DDOINSDIC.n_asistencia = objRead.n_asistencia;
                    DDOINSDIC.o_inscripto = objRead.o_inscripto;
                    DDOINSDIC.oi_personal_emp = oiPER_EMP;
                    DDOINSDIC.oi_dictado = int.Parse(oiDIC);

                    //Agrego el Inscripto
                    DDODIC.INSCRIPTOS.Add(DDOINSDIC);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error desconocido en registro, por favor verifique el formato del archivo y la correcta asignación de los campos - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            if (DDODIC != null)
            {
                //Grabo
                try
                {
                    NomadEnvironment.GetCurrentTransaction().Save(DDODIC);
                }
                catch (Exception e)
                {
                    objBatch.Err("Error al grabar registro - Linea: " + Linea.ToString() + " - " + e.Message);
                    Errores++;
                }
            }

            objBatch.Log("Registros Procesados: " + Linea.ToString() + " - Importados: " + (Linea - Errores).ToString());
            objBatch.Log("Finalizado...");
        }
    }
}

