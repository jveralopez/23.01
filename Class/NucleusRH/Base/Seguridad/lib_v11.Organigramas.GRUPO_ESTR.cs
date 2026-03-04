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
    //Clase Grupos Asociados
    public partial class GRUPO_ESTR 
    {
        public static void Agregar(Nomad.NSystem.Proxy.NomadXML xmlParam, string id_estructura, bool aplicaseg)
        {
            int C = 0;
            int P = 0;
            int I = 0;
            int R = 0;

            Hashtable MyGRU = new Hashtable();
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Agregar", "Agregar Grupos a Estructura de Seguridad");

            if (xmlParam.isDocument) xmlParam = xmlParam.FirstChild();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //CARGO LA ESTRUCTURA
            objBatch.SetMess("Obtengo la estructura...");
            ESTRUCTURA ddoESTR = (ESTRUCTURA)ESTRUCTURA.Get(id_estructura, false);
            ORGANIGRAMA ddoORG = (ORGANIGRAMA)ORGANIGRAMA.Get(ddoESTR.oi_organigrama, false);
            ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(id_estructura);

			////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //RECORRO REGISTROS
            for (NomadXML xmlCUR = xmlParam.FirstChild(); xmlCUR != null; xmlCUR = xmlCUR.Next())
            {
                C++;
                objBatch.SetPro(10, 30, xmlParam.ChildLength, C);

                P++; objBatch.SetMess("Analizando registro: " + P);

                NomadLog.Debug("GRUPO_ESTR.Resources.QRY_VALIDAR - oi_grupo:" + xmlCUR.GetAttr("oi_grupo") + " oi_estructura: " + id_estructura);
                
                //Valido que no este en la misma estructura ya agregado
                NomadXML xmldoc = NomadProxy.GetProxy().SQLService().GetXML(GRUPO_ESTR.Resources.QRY_VALIDAR, "<DATO oi_grupo=\"" + xmlCUR.GetAttr("oi_grupo") + "\" oi_estructura=\"" + id_estructura + "\" />");
                if (xmldoc.GetAttr("oi_estructura") != "" && !xmldoc.GetAttrBool("l_borrado"))
                {
                    objBatch.Err("Se rechaza el registro " + xmldoc.GetAttr("label") + " - ya se encuentra asociado a esta estructura de Seguridad");
                    R++; continue;
                }

                //Valido que no este en otra estructura ya agregado
                NomadXML xmldoc2 = NomadProxy.GetProxy().SQLService().GetXML(GRUPO_ESTR.Resources.QRY_VALIDAR_DUP, "<DATO oi_grupo=\"" + xmlCUR.GetAttr("oi_grupo") + "\"/>");
                if (xmldoc2.GetAttr("oi_estructura") != "")
                {
                    objBatch.Err("Se rechaza el registro " + xmldoc2.GetAttr("label") + " - ya se encuentra asociado a una estructura de Seguridad: " + xmldoc2.GetAttr("d_estructura"));
                    R++; continue;
                }

                GRUPO_ESTR ddoESTRGRU = (GRUPO_ESTR)ddoESTR.GRUPO_ESTR.GetByAttribute("oi_grupo", xmlCUR.GetAttr("oi_grupo"));
                if (ddoESTRGRU != null)
                {
                    if (!ddoESTRGRU.l_borrado)
                    {
                        objBatch.Err("Se rechaza del registro - ya se encuentra asociado a esta estructura de Seguridad");
                        R++; continue;
                    }
                    else
                    {
                        //Ya se existe el registro, pero esta borrado sin seguridad
                        NomadLog.Debug("Actualizo el registro...");
                        ddoESTRGRU.l_borrado = false;
                        I++;
                        continue;
                    }
                }

                //Lo Agrego a la lista de Personas a actualizar
                MyGRU[xmlCUR.GetAttr("oi_grupo")] = 1;

                //Agrego el Registro		
                NomadLog.Debug("Agrego el registro...");
                ddoESTRGRU = new GRUPO_ESTR();
                ddoESTRGRU.oi_grupo = xmlCUR.GetAttr("oi_grupo");
                ddoESTR.GRUPO_ESTR.Add(ddoESTRGRU);
                I++;
            }
                
            objBatch.SetPro(30);

              
            //////////////////////////
            //GUARDO SOLO LAS ESTRUCTURAS CON LOS GRUPOS    
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
                objBatch.SetPro(60);


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // GUARDO LOS REGISTROS
                objBatch.SetMess("Guardo los Cambios en las políticas...");
                NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);
                NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
                objBatch.SetPro(90);
            }

            objBatch.Log("Proceso finalizado. Total: " + P + " Grupos. Incorporados: " + I + " - Rechazados: " + R);
        }

        public static void Eliminar(Nomad.NSystem.Proxy.NomadXML xmlParam, string id_estructura)
        {

            int C = 0;
            int I = 0;
            int R = 0;
            int P = 0;

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Eliminar", "Eliminar Personas de Estructura de Seguridad");
            Hashtable MyGRU = new Hashtable();
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
            objBatch.SetPro(20);


            //RECORRO REGISTROS
            for (NomadXML xmlCUR = xmlParam.FirstChild(); xmlCUR != null; xmlCUR = xmlCUR.Next())
            {
                C++;
                objBatch.SetPro(10, 40, xmlParam.ChildLength, C);
                string[] aIds = xmlCUR.GetAttr("IDS").Split(',');

                for (int i = 0; i < aIds.Length; i++)
                {
                    P++; objBatch.SetMess("Analizando registro: " + P);
                    NomadLog.Debug("Eliminando oi_grupo_estr:" + aIds[i]);

                    GRUPO_ESTR ddoGRUPOESTR = (GRUPO_ESTR)ddoESTR.GRUPO_ESTR.GetById(aIds[i]);
                    if (ddoGRUPOESTR == null)
                    {
                        objBatch.Err("Se rechaza el registro - No se encuentra asociada a la estructura");
                        R++; continue;
                    }

                    objBatch.SetMess("Elimino el grupo...");
                    ddoESTR.GRUPO_ESTR.Remove(ddoGRUPOESTR);
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
            objBatch.SetPro(80);

            objBatch.Log("Proceso finalizado. Total: " + P + " Grupos. Eliminados: " + I + " - Rechazados: " + R);

        }

        /// <summary>
        /// Elimina los grupos de la estructura, sin aplicar seguridad.
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
            objBatch = NomadBatch.GetBatch("Eliminar", "Eliminar Grupos sin seguridad activada");
            Hashtable MyGRU = new Hashtable();
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
                    NomadLog.Debug("Eliminando oi_grupo_estr:" + aIds[i]);

                    GRUPO_ESTR ddoESTRGRU = (GRUPO_ESTR)ddoESTR.GRUPO_ESTR.GetById(aIds[i]);
                    if (ddoESTRGRU == null)
                    {
                        objBatch.Err("Se rechaza el registro - No se encuentra asociada a la estructura");
                        R++; continue;
                    }

                    ddoESTRGRU.l_borrado = true;
                    NomadLog.Debug("borrado: " + ddoESTRGRU.l_borrado);
                    I++;
                }

            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // GUARDO LOS REGISTROS
            objBatch.SetMess("Guardando los Registros...");
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoORG);
            objBatch.SetPro(70);

            objBatch.SetPro(50);
            objBatch.Log("Proceso finalizado. Total: " + P + " Grupos. Eliminados: " + I + " - Rechazados: " + R);

        }
    }
}
