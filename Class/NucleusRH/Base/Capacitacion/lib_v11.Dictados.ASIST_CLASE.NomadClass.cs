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

namespace NucleusRH.Base.Capacitacion.Dictados
{
    public partial class ASIST_CLASE : Nomad.NSystem.Base.NomadObject
    {
        public static void AsistenciaClase(string claseId, Nomad.NSystem.Document.NmdXmlDocument InscriptosCol)
        {
            NomadTransaction objTrans = null;
            
            try
            {
                DICTADO ddoDictado;
                ASIST_CLASE objAsistClase;
                CLASE_DICTADO objCLASE;
                INSCRIPTO objInscripto;


                objTrans = NomadEnvironment.GetCurrentTransaction();
                Nomad.NSystem.Document.NmdXmlDocument doc;

                objCLASE = CLASE_DICTADO.Get(claseId, false);

                //Se recupera el DDO de Dictado
                ddoDictado = DICTADO.Get(objCLASE.oi_dictado);
                objCLASE = (CLASE_DICTADO)ddoDictado.CLASES_DICT.GetById(claseId);
                

                //Calculo la duración en horas de la clase
                TimeSpan ts = objCLASE.f_fecha_hora_fin - objCLASE.f_fecha_hora_ini;
                int diferencia = ts.Hours;
         
                objTrans.Begin();

                //Recorro el xml de entrada            
                for (doc = (Nomad.NSystem.Document.NmdXmlDocument)InscriptosCol.GetFirstChildDocument(); doc != null; doc = (Nomad.NSystem.Document.NmdXmlDocument)InscriptosCol.GetNextChildDocument())
                {
                    bool encontroBD = false;
  
                    //Recorro la coleccion ASIT_CLASE de la clase CLASE_DICTADO, verifico si el oi_inscripto esta o no en la coleccion
                    for (int i = 0; i < objCLASE.ASIST_CLASE.Count; i++)
                    {
                        if (doc.GetAttribute("oi_inscripto").Value == objCLASE.ASIST_CLASE[i].GetAttribute("oi_inscripto").ToString())
                        {
                            //Si encontre el incripto en coleccion ASIST_CLASE de la clase CLASE_DICTADO
                            encontroBD = true;
                            
                            if (doc.GetAttribute("sel").Value == "1") //Si esta en la BD y se lo selecciono por pantalla
                            {
                                break;
                            }
                            else //Si esta en la BD y si NO se lo selecciono por pantalla
                            {
                                //Eliminarle las horas cargadas en el registro anterior y eliminarlo de la BD                                
                                objAsistClase = (ASIST_CLASE)objCLASE.ASIST_CLASE[i];

                                //Actualiza las horas asistidas de un inscripto
                                objInscripto = (INSCRIPTO) ddoDictado.INSCRIPTOS.GetById(objAsistClase.oi_inscripto);
                                objInscripto.n_hs_activas = objInscripto.n_hs_activas - diferencia;

                                //Elimina la asistencia a la clase, elimino el objeto de asistencia de la coleccion ASIST_CLASE de la clase
                                objCLASE.ASIST_CLASE.Remove(objAsistClase);

                                break;
                            }
                        }
                    }

                    //No encontre XML en BD?
                    if (!encontroBD)
                    {
                        if (doc.GetAttribute("sel").Value == "1")
                        {
                            objAsistClase = new NucleusRH.Base.Capacitacion.Dictados.ASIST_CLASE();
                            objAsistClase.oi_inscripto = doc.GetAttribute("oi_inscripto").Value;
                            objCLASE.ASIST_CLASE.Add(objAsistClase);

                            //Actualiza las horas asistidas de un inscripto
                            objInscripto = (INSCRIPTO)ddoDictado.INSCRIPTOS.GetById(objAsistClase.oi_inscripto);
                            objInscripto.n_hs_activas = objInscripto.n_hs_activas + diferencia;                                                        
                        }
                    }
                
                }
                objTrans.Save(ddoDictado); 
                objTrans.Commit();      

            }
            catch (Exception Ex)
            {
                if (objTrans != null)
                    objTrans.Rollback();

                NomadException nmdEx = new NomadException("ASIST_CLASE.AsistenciaClase.AsistenciaClase()", Ex);
                //nmdEx.SetValue("Step", strStep);
                throw nmdEx;

            }

        }
    }
}
