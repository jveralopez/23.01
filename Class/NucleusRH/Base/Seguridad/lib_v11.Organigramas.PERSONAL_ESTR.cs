using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Seguridad.Organigramas
{
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Personas Asociadas
    public partial class PERSONAL_ESTR 
    {
        public static void Agregar(Nomad.NSystem.Proxy.NomadXML xmlParam, string id_estructura, bool aplicaseg)
        {
            int C = 0;
            int P = 0;
            int I = 0;
            int R = 0;

            Hashtable MyPER = new Hashtable();
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Agregar", "Agregar Personas a Estructura de Seguridad");
            if (xmlParam.isDocument) xmlParam = xmlParam.FirstChild();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //CARGO LA ESTRUCTURA
            objBatch.SetMess("Obtengo la estructura...");
            ESTRUCTURA ddoESTR = (ESTRUCTURA)ESTRUCTURA.Get(id_estructura, false);
            ORGANIGRAMA ddoORG = (ORGANIGRAMA)ORGANIGRAMA.Get(ddoESTR.oi_organigrama, false);
            ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(id_estructura);

            /////////////////////////////////////////////////////////////////////////////////////
            //LEGAJO + PERSONAL
            NomadXML xmlPAR = new NomadXML("ROWS");
            for (NomadXML xmlCUR2 = xmlParam.FirstChild(); xmlCUR2 != null; xmlCUR2 = xmlCUR2.Next())
                xmlPAR.AddTailElement("VALUES").SetAttr("IDS", xmlCUR2.GetAttr("VALUES"));
            NomadXML xmlLEG = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_LEGAJOS, xmlPAR.ToString());
            NomadLog.Debug("xmlLEG: " + xmlLEG.ToString());
            objBatch.SetPro(10);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //RECORRO REGISTROS
            for (NomadXML xmlCUR = xmlParam.FirstChild(); xmlCUR != null; xmlCUR = xmlCUR.Next())
            {
                C++;
                objBatch.SetPro(10, 30, xmlParam.ChildLength, C);
                string[] aIds = xmlCUR.GetAttr("VALUES").Split(',');

                for (int i = 0; i < aIds.Length; i++)
                {
                    P++; objBatch.SetMess("Analizando registro: " + P);

                    NomadLog.Debug("PERSONAL_ESTR.Resources.QRY_VALIDAR - oi_personal:" + aIds[i] + " oi_estructura: " + id_estructura);
                    //Valido que no este en la misma estructura ya agregado
                    NomadXML xmldoc = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_VALIDAR, "<DATO oi_personal=\"" + aIds[i] + "\" oi_estructura=\"" + id_estructura + "\" />");
                    if (xmldoc.GetAttr("oi_estructura") != "" && !xmldoc.GetAttrBool("l_borrado"))
                    {
                        objBatch.Err("Se rechaza el registro " + xmldoc.GetAttr("label") + " - ya se encuentra asociado a esta estructura de Seguridad");
                        R++; continue;
                    }

                    if (xmldoc.GetAttr("oi_personal") != "" && xmldoc.GetAttr("oi_usuario_sistema") == "" && (ROLE.isJefe(ddoESTR.c_role) || ROLE.isAdmin(ddoESTR.c_role) || ROLE.isPar(ddoESTR.c_role)))
                    {
                        objBatch.Err("Se rechaza el registro " + xmldoc.GetAttr("label") + " - no tiene asignado una Cuenta de Usuario de Sistema");
                        R++; continue;
                    }

                    //Valido que no este en otra estructura ya agregado
                    NomadXML xmldoc2 = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_VALIDAR_DUP, "<DATO oi_personal=\"" + aIds[i] + "\"/>");

                    if (xmldoc2.GetAttr("oi_estructura") != "")
                    {
                        objBatch.Err("Se rechaza el registro " + xmldoc2.GetAttr("label") + " - ya se encuentra asociado a una estructura de Seguridad: " + xmldoc2.GetAttr("d_estructura"));
                        R++; continue;
                    }

                    //Valido que la persona tenga entidad si es necesario
                    NomadXML xmldoc3 = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_VALIDAR, "<DATO oi_personal=\"" + aIds[i] + "\"/>");
                    if (xmldoc3.GetAttr("oi_personal") != "" && xmldoc3.GetAttr("oi_usuario_sistema") == "" && (ROLE.isJefe(ddoESTR.c_role) || ROLE.isAdmin(ddoESTR.c_role) || ROLE.isPar(ddoESTR.c_role)))
                    {
                        objBatch.Err("Se rechaza el registro " + xmldoc3.GetAttr("label") + " - no tiene asignado una Cuenta de Usuario de Sistema");
                        R++; continue;
                    }

                    PERSONAL_ESTR ddoESTRPER = (PERSONAL_ESTR)ddoESTR.PER_ESTR.GetByAttribute("oi_personal", aIds[i]);
                    if (ddoESTRPER != null)
                    {
                        if (!ddoESTRPER.l_borrado)
                        {
                            objBatch.Err("Se rechaza del registro - ya se encuentra asociado a esta estructura de Seguridad");
                            R++; continue;
                        }
                        else
                        {
                            //Ya se existe el registro, pero esta borrado sin seguridad
                            NomadLog.Debug("Actualizo el registro...");                            
                            ddoESTRPER.l_borrado = false;
                            I++;
                            continue;
                        }
                    }

                    //Lo Agrego a la lista de Personas a actualizar
                    MyPER[aIds[i]] = 1;
                        
                    //Agrego el Registro		
                    NomadLog.Debug("Agrego el registro...");
                    ddoESTRPER = new PERSONAL_ESTR();
                    ddoESTRPER.oi_personal = aIds[i];
                    ddoESTR.PER_ESTR.Add(ddoESTRPER);
                    I++;
                }
            }
            objBatch.SetPro(30);

            //////////////////////////
            //GUARDO SOLO LAS ESTRUCTURAS CON LAS PERSONAS (SIN POLITICAS)
            objBatch.SetMess("Guardo los Cambios en las estructuras...");
            NomadEnvironment.GetCurrentTransaction().Save(ddoORG);

            NomadLog.Debug("aplicaseguridad: " + aplicaseg.ToString());
            if (aplicaseg)
            {
                    //Aplica la seguridad

                    /////////////////////////////////////////////////////////////////////////////////////
                    //POLICY                                                                        
                    Nomad.Base.Security.Policies.POLICY_BASE ddoPB = Nomad.Base.Security.Policies.POLICY_BASE.Get(NomadEnvironment.GetProxy().AppName, false);
                    NomadLog.Debug("ddoPB: " + ddoPB.ToString());
                    ESTRUCTURA.CheckPolicyTree(ddoPB, ddoORG);
                    NomadLog.Debug("ddoPB-RESULT: " + ddoPB.ToString());

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // PROPAGO LOS PERMISOS                      
                    objBatch.SetMess("Propago los permisos...");
                    Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName);
                    ESTRUCTURA.CheckPolicyValues(ddoPVS, ddoORG);
                    ESTRUCTURA.SetValues(ddoPVS, ddoPB, ddoORG, ddoESTR.c_estructura);
                    objBatch.SetPro(40);


                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // GUARDO LOS REGISTROS
                    objBatch.SetMess("Guardo los Cambios en las políticas...");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
                    objBatch.SetPro(50);

                    //Actualizo las Personas
                    string path = null;
                    ESTRUCTURA.FindNodeSec(ddoPB, ddoESTR.c_estructura, ref path);

                    objBatch.SetSubBatch(50, 90);
                    ESTRUCTURA.ChangePolicy(MyPER, xmlLEG, path);
                    objBatch.SetPro(90);
            }
            
            objBatch.Log("Proceso finalizado. Total: " + P + " Personas. Incorporadas: " + I + " - Rechazadas: " + R);

        }
        public static void Eliminar(Nomad.NSystem.Proxy.NomadXML xmlParam, string id_estructura)
        {

            int C = 0;
            int I = 0;
            int R = 0;
            int P = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Eliminar", "Eliminar Personas de Estructura de Seguridad");
            Hashtable MyPER = new Hashtable();
            if (xmlParam.isDocument) xmlParam = xmlParam.FirstChild();


            //CARGO LA ESTRUCTURA
            objBatch.SetMess("Obtengo la Estructura...");
            ESTRUCTURA ddoESTR = (ESTRUCTURA)ESTRUCTURA.Get(id_estructura, false);
            ORGANIGRAMA ddoORG = (ORGANIGRAMA)ORGANIGRAMA.Get(ddoESTR.oi_organigrama, false);
            ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(id_estructura);


            /////////////////////////////////////////////////////////////////////////////////////
            //POLICY                                                                        
            Nomad.Base.Security.Policies.POLICY_BASE ddoPB = Nomad.Base.Security.Policies.POLICY_BASE.Get(NomadEnvironment.GetProxy().AppName, false);
            NomadLog.Debug("ddoPB: " + ddoPB.ToString());
            ESTRUCTURA.CheckPolicyTree(ddoPB, ddoORG);
            NomadLog.Debug("ddoPB-RESULT: " + ddoPB.ToString());

            /////////////////////////////////////////////////////////////////////////////////////
            //LEGAJO + PERSONAL
            NomadXML xmlPAR = new NomadXML("ROWS");
            for (NomadXML xmlCUR2 = xmlParam.FirstChild(); xmlCUR2 != null; xmlCUR2 = xmlCUR2.Next())
                xmlPAR.AddTailElement("VALUES").SetAttr("IDS", xmlCUR2.GetAttr("IDS"));
            NomadXML xmlLEG = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_LEGAJOS_ESTR, xmlPAR.ToString());
            NomadLog.Debug("xmlLEG: " + xmlLEG.ToString());
            objBatch.SetPro(10);


            //RECORRO REGISTROS
            for (NomadXML xmlCUR = xmlParam.FirstChild(); xmlCUR != null; xmlCUR = xmlCUR.Next())
            {
                C++;
                objBatch.SetPro(10, 40, xmlParam.ChildLength, C);
                string[] aIds = xmlCUR.GetAttr("IDS").Split(',');

                for (int i = 0; i < aIds.Length; i++)
                {
                    P++; objBatch.SetMess("Analizando registro: " + P);
                    //NomadLog.Debug("Eliminando oi_personal:" + aIds[i]);
                    NomadLog.Debug("Eliminando oi_personal_estr:" + aIds[i]);

                    //PERSONAL_ESTR ddoESTRPER = (PERSONAL_ESTR)ddoESTR.PER_ESTR.GetByAttribute("oi_personal", aIds[i]);
                    PERSONAL_ESTR ddoESTRPER = (PERSONAL_ESTR)ddoESTR.PER_ESTR.GetById(aIds[i]);
                    if (ddoESTRPER == null)
                    {
                        objBatch.Err("Se rechaza el registro - No se encuentra asociada a la estructura");
                        R++; continue;
                    }

                    //Lo Agrego a la lista de Personas a actualizar
                    //MyPER[aIds[i]] = 1;
                    MyPER[ddoESTRPER.oi_personal] = 1;
                    objBatch.SetMess("Elimino la persona...");
                    ddoESTR.PER_ESTR.Remove(ddoESTRPER);
                    I++;
                }

            }
            objBatch.SetPro(50);


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // PROPAGO LOS PERMISOS
            objBatch.SetMess("Propago los permisos...");
            Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName);
            ESTRUCTURA.CheckPolicyValues(ddoPVS, ddoORG);
            ESTRUCTURA.SetValues(ddoPVS, ddoPB, ddoORG, ddoESTR.c_estructura);
            objBatch.SetPro(60);


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // GUARDO LOS REGISTROS
            objBatch.SetMess("Guardando los Registros...");
            NomadEnvironment.GetCurrentTransaction().Save(ddoORG);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
            objBatch.SetPro(70);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Actualizo las Personas
            objBatch.SetSubBatch(70, 90);
            ESTRUCTURA.ChangePolicy(MyPER, xmlLEG, "");
            objBatch.SetPro(90);


            objBatch.Log("Proceso finalizado. Total: " + P + " Personas. Eliminadas: " + I + " - Rechazadas: " + R);

        }

        /// <summary>
        /// Elimina las personas de la estructura, sin aplicar seguridad.
        /// </summary>
        /// <param name="xmlParam"></param>
        /// <param name="id_estructura"></param>
        public static void EliminarSS(Nomad.NSystem.Proxy.NomadXML xmlParam, string id_estructura)
        {
            int C = 0;
            int P = 0;
            int R = 0;
            int I = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Eliminar", "Eliminar Personas sin seguridad activada");
            Hashtable MyPER = new Hashtable();
            if (xmlParam.isDocument) xmlParam = xmlParam.FirstChild();


            //CARGO LA ESTRUCTURA
            objBatch.SetMess("Obtengo la Estructura...");
            ESTRUCTURA ddoESTR = (ESTRUCTURA)ESTRUCTURA.Get(id_estructura, false);
            ORGANIGRAMA ddoORG = (ORGANIGRAMA)ORGANIGRAMA.Get(ddoESTR.oi_organigrama, false);
            ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(id_estructura);

            //RECORRO REGISTROS
            for (NomadXML xmlCUR = xmlParam.FirstChild(); xmlCUR != null; xmlCUR = xmlCUR.Next())
            {
                C++;
                objBatch.SetPro(10, 40, xmlParam.ChildLength, C);
                string[] aIds = xmlCUR.GetAttr("IDS").Split(',');

                for (int i = 0; i < aIds.Length; i++)
                {
                    P++; objBatch.SetMess("Analizando registro: " + P);
                    NomadLog.Debug("Eliminando oi_personal_estr:" + aIds[i]);

                    //PERSONAL_ESTR ddoESTRPER = (PERSONAL_ESTR)ddoESTR.PER_ESTR.GetByAttribute("oi_personal", aIds[i]);
                    PERSONAL_ESTR ddoESTRPER = (PERSONAL_ESTR)ddoESTR.PER_ESTR.GetById(aIds[i]);
                    if (ddoESTRPER == null)
                    {
                        objBatch.Err("Se rechaza el registro - No se encuentra asociada a la estructura");
                        R++; continue;
                    }

                    ddoESTRPER.l_borrado = true;
                    NomadLog.Debug("borrado: " + ddoESTRPER.l_borrado);
                    I++;
                }

            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // GUARDO LOS REGISTROS
            objBatch.SetMess("Guardando los Registros...");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoORG);
            objBatch.SetPro(70);

            objBatch.SetPro(50);
            objBatch.Log("Proceso finalizado. Total: " + P + " Personas. Eliminadas: " + I + " - Rechazadas: " + R);

        }
    }
}
