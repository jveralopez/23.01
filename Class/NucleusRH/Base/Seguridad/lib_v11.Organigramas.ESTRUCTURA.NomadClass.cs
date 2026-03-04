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

    public partial class ESTRUCTURA
    {

        public static Nomad.Base.Security.Policies.POLICY InternalFindNodeSec(Nomad.Base.Security.Policies.POLICY ddo, string cret, ref string path)
        {
            //Esta pidiendo el NODO RAIZ?
            if (cret == "" || cret == "JER")
                return ddo;

            //Busco el NODO
            string npath;
            Nomad.Base.Security.Policies.POLICY ddoPRET = (Nomad.Base.Security.Policies.POLICY)ddo.POLICIES.GetByAttribute("COD", cret);
            if (ddoPRET != null)
            {
                path = path + "/" + cret;
                return ddoPRET;
            }

            //Busco entre los NODOS HIJOS
            foreach (Nomad.Base.Security.Policies.POLICY ddoP in ddo.POLICIES)
            {
                npath = path + "/" + ddoP.COD;
                ddoPRET = ESTRUCTURA.InternalFindNodeSec(ddoP, cret, ref npath);

                if (ddoPRET != null)
                {
                    path = npath;
                    return ddoPRET;
                }
            }

            path = "";
            return null;
        }

        public static Nomad.Base.Security.Policies.POLICY FindNodeSec(Nomad.Base.Security.Policies.POLICY_BASE ddoPB, string cret, ref string path)
        {
            Nomad.Base.Security.Policies.POLICY ddoPC = (Nomad.Base.Security.Policies.POLICY)ddoPB.POLICIES.GetByAttribute("COD", "JER");

            //BUSCO EL NODO RAIZ
            if (ddoPC == null)
            {
                ddoPC = new Nomad.Base.Security.Policies.POLICY();
                ddoPC.COD = "JER";
                ddoPC.DES = "Permisos Jerárquicos";

                ddoPB.POLICIES.Add(ddoPC);
            }

            //inicializo el PATH
            path = "./JER";
            return InternalFindNodeSec(ddoPC, cret, ref path);
        }

        public static void InternalCheckPolicyValues(Nomad.Base.Security.Values.POLICY_VALUES ddoPVS, string path, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, string code_parent, Hashtable toDelete)
        {
            string childpath;
            Nomad.Base.Security.Values.POLICY_VALUE MyVAL;

            //Cargo la lista de VALUES
            foreach (NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA MyEstr in ddoORG.ESTRUCTURA)
            {
                if (MyEstr.c_estr_padre != code_parent) continue;
                childpath = path + "/" + MyEstr.c_estructura;

                //Busco el Elemento
                if (toDelete.ContainsKey(childpath.ToUpper()))
                {
                    //Obtengo el Elemento y Actualizo el XPATH
                    MyVAL = (Nomad.Base.Security.Values.POLICY_VALUE)toDelete[childpath.ToUpper()];
                    //Lo elimino de la lista
                    toDelete.Remove(childpath.ToUpper());
                }
                else
                {
                    //Creo el Elemento
                    MyVAL = new Nomad.Base.Security.Values.POLICY_VALUE();
                    //Agrego el Elemento
                    ddoPVS.VALUE.Add(MyVAL);
                    NomadLog.Debug("ADD POLICY_VALUE XPATH:" + MyVAL.XPATH);
                }
                MyVAL.ACCESS = false;
                MyVAL.XPATH = childpath;
                //Checkeo los CHILD
                InternalCheckPolicyValues(ddoPVS, childpath, ddoORG, MyEstr.c_estructura, toDelete);
            }
        }

        public static void CheckPolicyValues(Nomad.Base.Security.Values.POLICY_VALUES ddoPVS, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG)
        {
            int del1Count = 0;
            Hashtable toDelete = new Hashtable();

            //Cargo la lista de VALUES
            foreach (Nomad.Base.Security.Values.POLICY_VALUE MyVAL in ddoPVS.VALUE)
            {
                if (MyVAL.XPATH.ToUpper() != "./JER" && !MyVAL.XPATH.ToUpper().StartsWith("./JER/")) continue;

                if (toDelete.ContainsKey(MyVAL.XPATH.ToUpper()))
                {
                    //Duplicados....
                    del1Count++;
                    toDelete["__DEL__:" + del1Count.ToString()] = MyVAL;
                }
                else
                    toDelete[MyVAL.XPATH.ToUpper()] = MyVAL;
            }

            //No eliminar el ROOT
            if (toDelete.ContainsKey("./JER"))
                toDelete.Remove("./JER");

            //Actualizo ese NODO
            InternalCheckPolicyValues(ddoPVS, "./JER", ddoORG, "", toDelete);

            //Elimino las Politicas sin USO
            foreach (Nomad.Base.Security.Values.POLICY_VALUE toDEL in toDelete.Values)
            {
                NomadLog.Debug("REMOVE POLICY_VALUE XPATH:" + toDEL.XPATH);
                ddoPVS.VALUE.Remove(toDEL);
            }
        }

        public static void CheckPolicyTree(Nomad.Base.Security.Policies.POLICY_BASE ddoPB, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG)
        {
            Nomad.Base.Security.Policies.POLICY ddoPC = (Nomad.Base.Security.Policies.POLICY)ddoPB.POLICIES.GetByAttribute("COD", "JER");

            //BUSCO EL NODO RAIZ
            if (ddoPC == null)
            {
                ddoPC = new Nomad.Base.Security.Policies.POLICY();
                ddoPC.COD = "JER";
                ddoPC.DES = "Permisos Jerárquicos";

                ddoPB.POLICIES.Add(ddoPC);

                NomadLog.Debug("ADD POLICY XPATH: ./JER");
            }

            //Actualizo ese NODO
            InternalCheckPolicyTree(ddoPC, "./JER", ddoORG, "");
        }

        public static void InternalCheckPolicyTree(Nomad.Base.Security.Policies.POLICY ddoPC, string path, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, string code_parent)
        {
            Nomad.Base.Security.Policies.POLICY ddoPN;

            int del1Count = 0;
            Hashtable toDelete = new Hashtable();

            //Cargo la lista de POLITICAS
            foreach (Nomad.Base.Security.Policies.POLICY MyPOL in ddoPC.POLICIES)
            {
                if (toDelete.ContainsKey(MyPOL.COD.ToUpper()))
                {
                    //Duplicados....
                    del1Count++;
                    toDelete["__DEL__:" + del1Count.ToString()] = MyPOL;
                }
                else
                    toDelete[MyPOL.COD.ToUpper()] = MyPOL;
            }

            //Agrego las Politicas Faltantes
            foreach (NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA MyEstr in ddoORG.ESTRUCTURA)
            {
                
                if (MyEstr.c_estr_padre != code_parent) continue;

                //Lo elimino de la lista
                if (toDelete.ContainsKey(MyEstr.c_estructura.ToUpper()))
                    toDelete.Remove(MyEstr.c_estructura.ToUpper());

                //Busco el Elemento
                ddoPN = (Nomad.Base.Security.Policies.POLICY)ddoPC.POLICIES.GetByAttribute("COD", MyEstr.c_estructura);

                if (ddoPN == null)
                { //Creo el Item si no EXISTE.
                    ddoPN = new Nomad.Base.Security.Policies.POLICY();
                    ddoPC.POLICIES.Add(ddoPN);
                    NomadLog.Debug("ADD POLICY XPATH: " + path + "/" + MyEstr.c_estructura);
                }
 
                ddoPN.COD = MyEstr.c_estructura;
                ddoPN.DES = MyEstr.d_estructura;

                //Checkeo los CHILD
                InternalCheckPolicyTree(ddoPN, path + "/" + MyEstr.c_estructura, ddoORG, MyEstr.c_estructura);
            }

            //Elimino las Politicas sin USO
            foreach (Nomad.Base.Security.Policies.POLICY toDEL in toDelete.Values)
            {
                NomadLog.Debug("REMOVE POLICY XPATH: " + path + "/" + toDEL.COD);
                ddoPC.POLICIES.Remove(toDEL);
            }
        }

        public static void SetValues(Nomad.Base.Security.Values.POLICY_VALUES ddoPVS, Nomad.Base.Security.Policies.POLICY_BASE ddoPB, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, string code)
        {
            NomadXML xmlACC = NomadEnvironment.QueryNomadXML(ORGANIGRAMA.Resources.QRY_CUENTAS, "");
            NomadLog.Debug("NomadEnvironment.QueryNomadXML(ORGANIGRAMA.Resources.QRY_CUENTAS)\n" + xmlACC.ToString());

            InternalSetValues(ddoPVS, ddoPB, ddoORG, code, xmlACC);
        }

        public static void InternalSetValues(Nomad.Base.Security.Values.POLICY_VALUES ddoPVS, Nomad.Base.Security.Policies.POLICY_BASE ddoPB, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, string code, Nomad.NSystem.Proxy.NomadXML xmlACC)
        {
            Hashtable htPers = new Hashtable();
            Hashtable htGroups = new Hashtable();

            string path = null;
            ESTRUCTURA.FindNodeSec(ddoPB, code, ref path);
            NomadLog.Debug("ESTRUCTURA.FindNodeSec(ddoPB," + code + ",ref " + path + ")");

            string[] pathParts = path.Split('/');
            string code_parent = pathParts[pathParts.Length - 2];

            string path_parent = null;
            ESTRUCTURA.FindNodeSec(ddoPB, code_parent, ref path_parent);
            NomadLog.Debug("ESTRUCTURA.FindNodeSec(ddoPB," + code_parent + ",ref " + path_parent + ")");

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Busco el NODO PADRE
            Nomad.Base.Security.Values.POLICY_VALUE ddoPP = (Nomad.Base.Security.Values.POLICY_VALUE)ddoPVS.VALUE.GetByAttribute("XPATH", path_parent);
            if (ddoPP == null)
            {
                ddoPP = new Nomad.Base.Security.Values.POLICY_VALUE();
                ddoPVS.VALUE.Add(ddoPP);
            }
            ddoPP.ACCESS = false;
            ddoPP.XPATH = path_parent;
                   
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Agrego el NODO
            Nomad.Base.Security.Values.POLICY_VALUE ddoPV = (Nomad.Base.Security.Values.POLICY_VALUE)ddoPVS.VALUE.GetByAttribute("XPATH", path);
            if (ddoPV == null)
            {
                ddoPV = new Nomad.Base.Security.Values.POLICY_VALUE();
                ddoPVS.VALUE.Add(ddoPV);
            }
 
            ddoPV.ACCESS = false;
            ddoPV.XPATH = path;
            if (ddoPV.XPATH != "./JER")
                ddoPV.GROUPS.Clear();
            //Obtengo los accessos padres
            foreach (Nomad.Base.Security.Values.POLICY_USER PU in ddoPP.USERS)
                htPers[PU.ACCOUNT] = 1;

            NomadLog.Debug("POLICY-GROUP-PADRE(" + ddoPP.GROUPS.ToString() + ");");
            foreach (Nomad.Base.Security.Values.POLICY_GROUP PolG in ddoPP.GROUPS)
            {
                NomadLog.Debug("POLICY-GROUP-PADRE-IN(" + PolG.Code.ToString() + ");");
                htGroups[PolG.GROUP] = 1;
            }
            NomadLog.Debug("SET-VALUE(" + path + ");");

            //Verifico si tiene estructura padre.
            if (code_parent != "JER")
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Obtengo la Estructura Padre
                ESTRUCTURA ddoESTRPARENT = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetByAttribute("c_estructura", code_parent);
                NomadLog.Debug("ddoESTRPARENT(" + path_parent + ");\n" + ddoESTRPARENT.ToString());

                //Obtengo el ROLE del Padre
                ROLE ddoROLEPARENT = (ROLE)ddoORG.ROLES.GetByAttribute("c_role", ddoESTRPARENT.c_role);
                 
                foreach (GRUPO_ESTR ddoGRUPOESTR in ddoESTRPARENT.GRUPO_ESTR)
                {
                    NucleusRH.Base.Seguridad.Grupos.GRUPO ddoGRUPO = NucleusRH.Base.Seguridad.Grupos.GRUPO.Get(ddoGRUPOESTR.oi_grupo);

                    if (ddoROLEPARENT.l_inferioresg)
                    {
                        NomadLog.Debug("Agrega inferiores Grupo: " + ddoGRUPO.oi_grupo_seguridad);
                        htGroups[ddoGRUPO.oi_grupo_seguridad] = 1;
                    }
                    else
                    {
                        NomadLog.Debug("Borra Padres que no ven inferiores Grupos: " + ddoGRUPO.oi_grupo_seguridad);
                        if (htGroups.Contains(ddoGRUPO.oi_grupo_seguridad))
                            htGroups.Remove(ddoGRUPO.oi_grupo_seguridad);
                    }                        
                }
 
                foreach (PERSONAL_ESTR ddoPERESTR in ddoESTRPARENT.PER_ESTR)
                {
                    //Recupero las cuentas asociadas a la entidad de la persona
                    NomadXML xmlC = xmlACC.FirstChild().FindElement2("ETTY", "oip", ddoPERESTR.oi_personal);
                    if (xmlC != null)
                    {
                        foreach (NomadXML xmlcuenta in xmlC.GetChilds())
                        {
                            if (ddoROLEPARENT.l_inferiores)
                            {
                                NomadLog.Debug("Agrega inferiores: " + xmlcuenta.GetAttr("id"));
                                htPers[xmlcuenta.GetAttr("id")] = 1;
                            }
                            else
                            {
                                NomadLog.Debug("Borra Padres que no ven inferiores: " + xmlcuenta.GetAttr("id"));
                                if (htPers.Contains(xmlcuenta.GetAttr("id")))
                                    htPers.Remove(xmlcuenta.GetAttr("id"));
                            }
                        }
                    }
                } 
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Obtengo la Estructura
            ESTRUCTURA ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetByAttribute("c_estructura", code);
            NomadLog.Debug("ddoESTR(" + code + ");\n" + ddoESTR.ToString());

            //Obtengo el ROLE
            NomadLog.Debug("ROL");
            ROLE ddoROLE = (ROLE)ddoORG.ROLES.GetByAttribute("c_role", ddoESTR.c_role);
            NomadLog.Debug("NO ROL: " + ddoROLE.c_role);
            if (ddoROLE.l_igualesg)
            {
                NomadLog.Debug("Opera iguales Grupos");
                foreach (GRUPO_ESTR ddoGRUPOESTR in ddoESTR.GRUPO_ESTR)
                {
                    NucleusRH.Base.Seguridad.Grupos.GRUPO ddoGRUPO = NucleusRH.Base.Seguridad.Grupos.GRUPO.Get(ddoGRUPOESTR.oi_grupo);
                    NomadLog.Debug("Agrega iguales Grupo: " + ddoGRUPO.oi_grupo_seguridad);
                    htGroups[ddoGRUPO.oi_grupo_seguridad] = 1;
                }
            }

            if (ddoROLE.l_iguales)
            {
                NomadLog.Debug("Opera iguales");
                foreach (PERSONAL_ESTR ddoPERESTR in ddoESTR.PER_ESTR)
                {
                    //Recupero las cuentas asociadas a la entidad de la persona
                    NomadXML xmlC = xmlACC.FirstChild().FindElement2("ETTY", "oip", ddoPERESTR.oi_personal);
                    if (xmlC != null)
                    {
                        foreach (NomadXML xmlcuenta in xmlC.GetChilds())
                        {
                            NomadLog.Debug("Agrega iguales: " + xmlcuenta.GetAttr("id"));
                            htPers[xmlcuenta.GetAttr("id")] = 1;

                        }
                    }
                }
            }

            //Limpio los Usuarios y los Grupos
            ddoPV.USERS.Clear();
            ddoPV.GROUPS.Clear();

            //Agrego los Accesos por USUARIO
            foreach (string cuenta in htPers.Keys)
            {
                Nomad.Base.Security.Values.POLICY_USER ddoPU = new Nomad.Base.Security.Values.POLICY_USER();
                ddoPU.ACCESS = true;
                ddoPU.ACCOUNT = cuenta;

                ddoPV.USERS.Add(ddoPU);
            }

            //Agrego los Accesos por GRUPO
            foreach (string grupo in htGroups.Keys)
            {
                NomadLog.Debug("ENTRO A RECORRER GRUPOS");
                Nomad.Base.Security.Values.POLICY_GROUP ddoPG = new Nomad.Base.Security.Values.POLICY_GROUP();
                ddoPG.ACCESS = true;
                ddoPG.GROUP = grupo;

                ddoPV.GROUPS.Add(ddoPG);
                NomadLog.Debug("LO AGREGO");
            }

            //Copio los Accesos por Grupo
            /*foreach (Nomad.Base.Security.Values.POLICY_GROUP PG in ddoPV.GROUPS)
                ddoPV.GROUPS.Add(PG.Duplicate());*/

            //Recorro las esctructuras y Actualiza las Estructuras HIJAS....
            foreach (ESTRUCTURA curESTR in ddoORG.ESTRUCTURA)
            {
                if (curESTR.c_estr_padre != code) continue;
                InternalSetValues(ddoPVS, ddoPB, ddoORG, curESTR.c_estructura, xmlACC);
            }
        }

        public static void ChangePolicy(Hashtable ids, NomadXML data, string path)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Propagando la seguridad", "Propagando la seguridad");

            string oi_personal = "";
            NucleusRH.Base.Personal.Legajo.PERSONAL MyPER;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP MyLEG;

            int t, l = data.ChildLength;

            t = 1;
            for (NomadXML cur = data.FirstChild(); cur != null; cur = cur.Next(), t++)
            {
                objBatch.SetPro(0, 100, l, t);
                objBatch.SetMess("Actualizando Legajo " + t + " de " + l);

                if (ids != null)
                    if (!ids.ContainsKey(cur.GetAttr("oi_personal")))
                        continue;

                if (cur.GetAttr("oi_personal") != oi_personal)
                {
                    NomadLog.Debug("Actualizar oi_personal: " + cur.GetAttr("oi_personal") + " - " + path);
                    MyPER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(cur.GetAttr("oi_personal"), false);

                    //Actualizo la Seguridad
                    if (MyPER.Security.Policy != path)
                    {
                        MyPER.Security.Policy = path;
                        NomadEnvironment.GetCurrentTransaction().Save(MyPER);
                    }

                    oi_personal = cur.GetAttr("oi_personal");
                }

                if (cur.GetAttr("oi_personal_emp") != "")
                {
                    NomadLog.Debug("Actualizar oi_personal_emp: " + cur.GetAttr("oi_personal_emp") + " - " + path);
                    MyLEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(cur.GetAttr("oi_personal_emp"), false);

                    //Actualizo la Seguridad
                    if (MyLEG.Security.Policy != path)
                    {
                        MyLEG.Security.Policy = path;
                        NomadEnvironment.GetCurrentTransaction().Save(MyLEG);
                    }
                }
            }

        }

        public static void RefreshPolicyTree(Nomad.Base.Security.Policies.POLICY_BASE ddoPB, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, string c_node)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Propagando la seguridad", "Propagando la seguridad");

            //Log
            NomadLog.Debug("RefreshPolicyTree(" + c_node + ")");

            //Estructura
            int t = 0;
            NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ddoESTR = (NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA)ddoORG.ESTRUCTURA.GetByAttribute("c_estructura", c_node);

            //Lista de Objetos
            string path = null;
            System.Collections.ArrayList lst = new System.Collections.ArrayList();
            EstructuraChild(ddoORG, ddoESTR, lst);

            //Recorro las ESTRUCTUTRAS
            foreach (NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA MyEstr in lst)
            {
                NomadXML xmlPAR = new NomadXML("ROWS");
                xmlPAR.SetAttr("ESTR", MyEstr.Id);
                NomadXML xmlLEG = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_LEGAJOS, xmlPAR.ToString());

                //Obtengo el PATH
                path = null;
                FindNodeSec(ddoPB, MyEstr.c_estructura, ref path);
                NomadLog.Debug("FindNodeSec(" + MyEstr.c_estructura + ",ref " + path + ")");

                //Cambio las politicas
                objBatch.SetSubBatch(t * 90 / lst.Count, (t + 1) * 90 / lst.Count);
                ChangePolicy(null, xmlLEG, path);
                t++; objBatch.SetPro(t * 90 / lst.Count);
            }
        }

        public static void EstructuraChild(NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ddoESTR, System.Collections.ArrayList lst)
        {
            lst.Add(ddoESTR);

            //Recorro todos los nodos hijos y los Verifico
            foreach (NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA MyEstr in ddoORG.ESTRUCTURA)
                if (ddoESTR.c_estructura == MyEstr.c_estr_padre)
                    EstructuraChild(ddoORG, MyEstr, lst);
        }

        public static void CrearNodo(string id_parent, NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ESTRUCTURA)
        {
            /////////////////////////////////////////////////////////////////////////////////////
            //ESTRUCTURA
            ORGANIGRAMA ddoORG = ORGANIGRAMA.Get(id_parent, false);
            if (ddoORG.ESTRUCTURA.GetByAttribute("c_estructura", ESTRUCTURA.c_estructura) == null)
            {
                ddoORG.ESTRUCTURA.Add(ESTRUCTURA);

                ESTRUCTURA.AgregarAArbolDePoliticas(ESTRUCTURA);

                ESTRUCTURA ddoESTRPARENT = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetByAttribute("c_estructura", ESTRUCTURA.c_estr_padre);
                if (ddoESTRPARENT != null)
                {
                    ROLE ddoROLEPARENT = (ROLE)ddoORG.ROLES.GetByAttribute("c_role", ddoESTRPARENT.c_role);
                    ESTRUCTURA.AgregarAPolicyValue(ESTRUCTURA, ddoROLEPARENT);
                }
              
                NomadEnvironment.GetCurrentTransaction().Save(ddoORG);
            }
        }

        private static void AgregarAPolicyValue(ESTRUCTURA estructura,ROLE rolPadre)
        {
            Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName);
            
            string pathPadre = "";
  
            foreach (Nomad.Base.Security.Values.POLICY_VALUE MyVAL in ddoPVS.VALUE)
            {
                if (MyVAL.XPATH.ToUpper().EndsWith(estructura.c_estr_padre.ToUpper()))
                {
                    pathPadre = MyVAL.XPATH;
                    break;
                }
            }

            if (pathPadre != "" && ddoPVS.VALUE.GetByAttribute("XPATH", pathPadre + "/" + estructura.c_estructura) == null)
            {
                Nomad.Base.Security.Values.POLICY_VALUE policyPadre = (Nomad.Base.Security.Values.POLICY_VALUE)ddoPVS.VALUE.GetByAttribute("XPATH", pathPadre);
               
                Nomad.Base.Security.Values.POLICY_VALUE nuevoPolicyValue = new Nomad.Base.Security.Values.POLICY_VALUE();
                nuevoPolicyValue.ACCESS = false;
                nuevoPolicyValue.XPATH = pathPadre + "/" + estructura.c_estructura;

                CopiarAccesosDeUsuarios(nuevoPolicyValue, policyPadre, rolPadre);
                CopiarAccesosDeGrupos(nuevoPolicyValue, policyPadre, rolPadre);
                ddoPVS.VALUE.Add(nuevoPolicyValue);

            }
            
            NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);
        }

        private static void CopiarAccesosDeUsuarios(Nomad.Base.Security.Values.POLICY_VALUE nuevoPolicyValue, Nomad.Base.Security.Values.POLICY_VALUE policiValuePadre, ROLE rolPadre)
        {
            if (rolPadre.l_inferiores)
            {
                foreach (Nomad.Base.Security.Values.POLICY_USER PolUser in policiValuePadre.USERS)
                {
                    Nomad.Base.Security.Values.POLICY_USER ddoPU = new Nomad.Base.Security.Values.POLICY_USER();
                    ddoPU.ACCESS = true;
                    ddoPU.ACCOUNT = PolUser.ACCOUNT;

                    nuevoPolicyValue.USERS.Add(ddoPU);
                } 
            }

        }
        private static void CopiarAccesosDeGrupos(Nomad.Base.Security.Values.POLICY_VALUE nuevoPolicyValue, Nomad.Base.Security.Values.POLICY_VALUE policiValuePadre, ROLE rolPadre)
        {
            if (rolPadre.l_inferioresg)
            {
                foreach (Nomad.Base.Security.Values.POLICY_GROUP policyGroup in policiValuePadre.GROUPS)
                {
                    Nomad.Base.Security.Values.POLICY_GROUP ddoPG = new Nomad.Base.Security.Values.POLICY_GROUP();
                    ddoPG.ACCESS = true;
                    ddoPG.GROUP = policyGroup.GROUP;

                    nuevoPolicyValue.GROUPS.Add(ddoPG);
                }
            }
        }
        private static void AgregarAArbolDePoliticas(ESTRUCTURA estructura)
        {
            Nomad.Base.Security.Policies.POLICY_BASE ddoPB = Nomad.Base.Security.Policies.POLICY_BASE.Get(NomadEnvironment.GetProxy().AppName, false);
            Nomad.Base.Security.Policies.POLICY ddoPC = (Nomad.Base.Security.Policies.POLICY)ddoPB.POLICIES.GetByAttribute("COD", "JER");
            ArrayList arrRaiz = new ArrayList();
            
            if (ddoPC == null)
            {
                ddoPC = new Nomad.Base.Security.Policies.POLICY();
                ddoPC.COD = "JER";
                ddoPC.DES = "Permisos Jerárquicos";

                ddoPB.POLICIES.Add(ddoPC);

                NomadLog.Debug("ADD POLICY XPATH: ./JER");
            }
            arrRaiz.Add(ddoPC);
            Nomad.Base.Security.Policies.POLICY politicaPadre = BuscarPolitica(arrRaiz, estructura.c_estr_padre);
            if (politicaPadre != null && politicaPadre.POLICIES.GetByAttribute("COD", estructura.c_estructura) == null)
            {  
                Nomad.Base.Security.Policies.POLICY policyNueva = new Nomad.Base.Security.Policies.POLICY();
                policyNueva.COD = estructura.c_estructura;
                policyNueva.DES = estructura.d_estructura;
                politicaPadre.POLICIES.Add(policyNueva);
            }

            NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
        }
        
        private static Nomad.Base.Security.Policies.POLICY BuscarPolitica(ArrayList arrPoliticas,string c_est_padre)
	    {
            ArrayList siguienteNivel = new ArrayList();
            foreach (Nomad.Base.Security.Policies.POLICY ddoPol in arrPoliticas)
            {
                foreach (Nomad.Base.Security.Policies.POLICY MyPOL in ddoPol.POLICIES)
                {
                    if (MyPOL.COD == c_est_padre)
                        return MyPOL;

                    if (MyPOL.POLICIES.Count > 0)
                        siguienteNivel.Add(MyPOL);
                }
              
	        }
            return siguienteNivel.Count > 0 ? BuscarPolitica(siguienteNivel, c_est_padre) : null;
	    }

        public static void Crear(string id_parent, NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ESTRUCTURA)
        {

            /////////////////////////////////////////////////////////////////////////////////////
            //ESTRUCTURA
            ORGANIGRAMA ddoORG = ORGANIGRAMA.Get(id_parent, false);
            ddoORG.ESTRUCTURA.Add(ESTRUCTURA);

            /////////////////////////////////////////////////////////////////////////////////////
            //POLICY
            Nomad.Base.Security.Policies.POLICY_BASE ddoPB = Nomad.Base.Security.Policies.POLICY_BASE.Get(NomadEnvironment.GetProxy().AppName, false);
            NomadLog.Debug("ddoPB: " + ddoPB.ToString());
            ESTRUCTURA.CheckPolicyTree(ddoPB, ddoORG);
            NomadLog.Debug("ddoPB-RESULT: " + ddoPB.ToString());

            /////////////////////////////////////////////////////////////////////////////////////
            // PROPAGACION DE PERMISOS
            Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName);
            NomadLog.Debug("ddoPVS: " + ddoPVS.ToString());
            ESTRUCTURA.CheckPolicyValues(ddoPVS, ddoORG);
            ESTRUCTURA.SetValues(ddoPVS, ddoPB, ddoORG, ESTRUCTURA.c_estructura);
            NomadLog.Debug("ddoPVS-RESULT: " + ddoPVS.ToString());

            /////////////////////////////////////////////////////////////////////////////////////
            // GUARDO TODO
            NomadLog.Debug("Guardo ddoORG, ddoPB, ddoPVS...");
            NomadEnvironment.GetCurrentTransaction().Save(ddoORG);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);

        }
        public static void Eliminar(string id)
        {

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Eliminando la Estructura de Seguridad");

            /////////////////////////////////////////////////////////////////////////////////////
            // ESTRUCTURA
            objBatch.SetMess("Obtengo la Estructura...");
            ESTRUCTURA ddoESTR = ESTRUCTURA.Get(id, true);
            objBatch.SetPro(10);

            //Legajos
            NomadXML xmlPAR = new NomadXML("ROWS");
            xmlPAR.SetAttr("ESTR", id);
            NomadXML xmlLEG = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_LEGAJOS, xmlPAR.ToString());

            //Codigo Actual y Codigo Padre
            string code, code_parent;
            code = ddoESTR.c_estructura;
            code_parent = ddoESTR.c_estr_padre;

            /////////////////////////////////////////////////////////////////////////////////////
            //POLICY
            objBatch.SetMess("Actualizo el Arbol de Politicas...");
            Nomad.Base.Security.Policies.POLICY_BASE ddoPB = Nomad.Base.Security.Policies.POLICY_BASE.Get(NomadEnvironment.GetProxy().AppName, false);
            NomadLog.Debug("ddoPB: " + ddoPB.ToString());

            //BUSCO LA ESTRUCTURA PADRE
            string path_parent = null;
            Nomad.Base.Security.Policies.POLICY ddoPPARENT = ESTRUCTURA.FindNodeSec(ddoPB, code_parent, ref path_parent);
            string path = path_parent + "/" + code;

            //ELIMINO LA ESTRUCTURA
            if (ddoPPARENT != null)
            {
                Nomad.Base.Security.Policies.POLICY ddoPDEL = (Nomad.Base.Security.Policies.POLICY)ddoPPARENT.POLICIES.GetByAttribute("COD", code);
                if (ddoPDEL != null)
                    ddoPPARENT.POLICIES.Remove(ddoPDEL);
            }
            NomadLog.Debug("ddoPB-RESULT: " + ddoPB.ToString());
            objBatch.SetPro(20);

            /////////////////////////////////////////////////////////////////////////////////////
            //POLICY VALUE
            objBatch.SetMess("Actualizo los Valores de Politicas...");
            Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName, false);
            NomadLog.Debug("ddoPVS: " + ddoPVS.ToString());

            ArrayList toDelete = new ArrayList();
            foreach (Nomad.Base.Security.Values.POLICY_VALUE ddoPV in ddoPVS.VALUE)
            {
                if ((ddoPV.XPATH == path) || (ddoPV.XPATH.StartsWith(path + "/")))
                {
                    NomadLog.Debug("REMOVE POLICY_VALUE XPATH:" + ddoPV.XPATH);
                    toDelete.Add(ddoPV);
                }
            }
            foreach (Nomad.Base.Security.Values.POLICY_VALUE ddoPVD in toDelete)
                ddoPVS.VALUE.Remove(ddoPVD);

            NomadLog.Debug("ddoPVS-RESULT: " + ddoPVS.ToString());
            objBatch.SetPro(30);

            /////////////////////////////////////////////////////////////////////////////////////
            // GUARDO TODO
            objBatch.SetMess("Guardar Cambios...");
            NomadEnvironment.GetCurrentTransaction().Delete(ddoESTR);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
            NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);
            objBatch.SetPro(40);

            /////////////////////////////////////////////////////////////////////////////////////
            // Eliminar las Politicas
            objBatch.SetSubBatch(50, 90);
            ESTRUCTURA.ChangePolicy(null, xmlLEG, "");
            objBatch.SetPro(90);

        }
        public static void Pegar(string id, string code_topaste, bool aplicaseg)
        {

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Moviendo la Estructura de Seguridad");

            /////////////////////////////////////////////////////////////////////////////////////
            // ESTRUCTURA
            objBatch.SetMess("Obtengo la Estructura...");
            ESTRUCTURA ddoESTR = ESTRUCTURA.Get(id, false);
            ORGANIGRAMA ddoORG = ORGANIGRAMA.Get(ddoESTR.oi_organigrama, false);
            ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(id);
            objBatch.SetPro(10);
            ddoESTR.c_estr_padre = code_topaste;

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

                    /////////////////////////////////////////////////////////////////////////////////////
                    //POLICY VALUE
                    objBatch.SetMess("Actualizo los Valores de Politicas...");
                    Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName, false);
                    NomadLog.Debug("ddoPVS: " + ddoPVS.ToString());
                    ESTRUCTURA.CheckPolicyValues(ddoPVS, ddoORG);
                    ESTRUCTURA.SetValues(ddoPVS, ddoPB, ddoORG, ddoESTR.c_estructura);
                    objBatch.SetPro(40);

                    /////////////////////////////////////////////////////////////////////////////////////
                    // GUARDO LOS REGISTROS
                    objBatch.SetMess("Guardo los cambios...");
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPB);
                    NomadEnvironment.GetCurrentTransaction().Save(ddoPVS);
                    objBatch.SetPro(50);

                    /////////////////////////////////////////////////////////////////////////////////////
                    // ACTUALIZO LOS PERMISOS
                    objBatch.SetSubBatch(50, 90);
                    ESTRUCTURA.RefreshPolicyTree(ddoPB, ddoORG, code_topaste);
                    objBatch.SetPro(90);
            }
        }

        /// <summary>
        /// Aplica la seguridad en todo el organigrama
        /// </summary>
        /// <param name="id_parent"></param>
        /// <param name="ESTRUCTURA"></param>
        public static void AplicarSeg(string id)
        {
            NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ddoESTR;
            NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ddoESTR_DEL;
            NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ddoESTR_DEL_GRU;
            NucleusRH.Base.Seguridad.Organigramas.PERSONAL_ESTR ddoPER_ESTR_DEL;
            NucleusRH.Base.Seguridad.Organigramas.GRUPO_ESTR ddoGRU_ESTR_DEL;
            NomadXML estructuras;
            NomadXML personas_estr;
            NomadXML grupos_estr;
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("oi_organigrama", id);

            ORGANIGRAMA ddoORG = ORGANIGRAMA.Get(id, false);

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Aplicar Seguridad", "Aplicar seguridad a todo el diagrama");
            Hashtable MyPER = new Hashtable();

            //Obtengo todos los grupos que fueron borrados sin seguridad
            objBatch.SetMess("Obtengo los grupos eliminados sin seguridad...");
            objBatch.SetPro(10);
            grupos_estr = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA.Resources.QRY_GRUPOS_ESTR, "");
            NomadLog.Debug("Grupos a Eliminar: " + grupos_estr.ToString());
            for (NomadXML gru_del = grupos_estr.FirstChild().FirstChild(); gru_del != null; gru_del = gru_del.Next())
            {
                ddoESTR_DEL_GRU = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(gru_del.GetAttr("oi_estructura"));
                ddoGRU_ESTR_DEL = (GRUPO_ESTR)ddoESTR_DEL_GRU.GRUPO_ESTR.GetById(gru_del.GetAttr("id"));
                objBatch.SetMess("Elimino el grupo...");
                ddoESTR_DEL_GRU.GRUPO_ESTR.Remove(ddoGRU_ESTR_DEL);
            }

            //Obtengo todas las personas que fueron borradas sin seguridad
            objBatch.SetMess("Obtengo las personas eliminadas sin seguridad...");
            objBatch.SetPro(20);
            personas_estr = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA.Resources.QRY_PERSONAS_ESTR, "");
            NomadLog.Debug("Personas a Eliminar: " + personas_estr.ToString());
            for (NomadXML per_del = personas_estr.FirstChild().FirstChild(); per_del != null; per_del = per_del.Next())
            {
                ddoESTR_DEL = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(per_del.GetAttr("oi_estructura"));
                ddoPER_ESTR_DEL = (PERSONAL_ESTR)ddoESTR_DEL.PER_ESTR.GetById(per_del.GetAttr("id"));
                //Lo agrego a la lista de personas a actualizar
                MyPER[ddoPER_ESTR_DEL.oi_personal] = 1;
                objBatch.SetMess("Elimino la persona...");
                ddoESTR_DEL.PER_ESTR.Remove(ddoPER_ESTR_DEL);
            }

            /////////////////////////////////////////////////////////////////////////////////////
            //LEGAJO + PERSONAL
            NomadXML xmlLEG = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_LEGAJOS_ESTR_DEL, personas_estr.ToString());
            NomadLog.Debug("xmlLEG: " + xmlLEG.ToString());

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Actualizo las Personas eliminadas
            NomadLog.Debug("Actualizo las Personas eliminadas");
            objBatch.SetMess("Actualizo las Personas eliminadas...");
            objBatch.SetSubBatch(20, 50);
            ESTRUCTURA.ChangePolicy(MyPER, xmlLEG, "");

            /////////////////////////////////////////////////////////////////////////////////////
            //POLICY
            Nomad.Base.Security.Policies.POLICY_BASE ddoPB = Nomad.Base.Security.Policies.POLICY_BASE.Get(NomadEnvironment.GetProxy().AppName, false);
            NomadLog.Debug("ddoPB: " + ddoPB.ToString());
            ESTRUCTURA.CheckPolicyTree(ddoPB, ddoORG);
            NomadLog.Debug("ddoPB-RESULT: " + ddoPB.ToString());

            Nomad.Base.Security.Values.POLICY_VALUES ddoPVS = Nomad.Base.Security.Values.POLICY_VALUES.Get(NomadEnvironment.GetProxy().AppName);
            NomadLog.Debug("ddoPVS: " + ddoPVS.ToString());
            ESTRUCTURA.CheckPolicyValues(ddoPVS, ddoORG);
            NomadLog.Debug("ddoPVS-RESULT: " + ddoPVS.ToString());

            //Obtengo todas las estructuras que que no tienen padre
            estructuras = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA.Resources.QRY_ESTRUC_PARENT, param.ToString());
            //Recorro las estructuras y les aplico la seguridad
            for (NomadXML estr = estructuras.FirstChild().FirstChild(); estr != null; estr = estr.Next())
            {
                ddoESTR = (ESTRUCTURA)ddoORG.ESTRUCTURA.GetById(estr.GetAttr("id"));

                ESTRUCTURA.SetValues(ddoPVS, ddoPB, ddoORG, ddoESTR.c_estructura);
 
                objBatch.SetPro(90);
                ESTRUCTURA.RefreshPolicyTreeEst(ddoPB, ddoORG, ddoESTR);

            }
            /////////////////////////////////////////////////////////////////////////////////////
            // GUARDO TODO
            NomadLog.Debug("Guardo ddoORG, ddoPB, ddoPVS...");
            ddoORG.l_seguridad = true;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoORG);
            File.Delete(NomadProxy.GetProxy().RunPath + "NOMAD\\DB\\Nomad\\Base\\Security\\Policies\\" + NomadProxy.GetProxy().AppName + ".POLICY_BASE.xml");
           
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPB);
            
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPVS);

        }

        public static void NoAplicarSeg(string id)
        {
            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Desactivar Seguridad", "Desactivar la seguridad del diagrama");
            ORGANIGRAMA ddoORG = ORGANIGRAMA.Get(id, false);
            ddoORG.l_seguridad = false;
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoORG);
        }

        public static void RefreshPolicyTreeEst(Nomad.Base.Security.Policies.POLICY_BASE ddoPB, NucleusRH.Base.Seguridad.Organigramas.ORGANIGRAMA ddoORG, NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA ddoESTR)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Propagando la seguridad", "Propagando la seguridad");

            //Log
            NomadLog.Debug("RefreshPolicyTreeEst(" + ddoESTR.c_estructura + ")");

            //Estructura
            int t = 0;

            //Lista de Objetos
            string path = null;
            System.Collections.ArrayList lst = new System.Collections.ArrayList();
            EstructuraChild(ddoORG, ddoESTR, lst);

            //Recorro las ESTRUCTUTRAS
            foreach (NucleusRH.Base.Seguridad.Organigramas.ESTRUCTURA MyEstr in lst)
            {
                NomadXML xmlPAR = new NomadXML("ROWS");
                xmlPAR.SetAttr("ESTR", MyEstr.Id);
                NomadXML xmlLEG = NomadProxy.GetProxy().SQLService().GetXML(PERSONAL_ESTR.Resources.QRY_LEGAJOS, xmlPAR.ToString());

                //Obtengo el PATH
                path = null;
                FindNodeSec(ddoPB, MyEstr.c_estructura, ref path);
                NomadLog.Debug("FindNodeSec(" + MyEstr.c_estructura + ",ref " + path + ")");

                //Cambio las politicas
                objBatch.SetSubBatch(t * 90 / lst.Count, (t + 1) * 90 / lst.Count);
                ChangePolicy(null, xmlLEG, path);
                t++; objBatch.SetPro(t * 90 / lst.Count);
            }
        }
    }
}


