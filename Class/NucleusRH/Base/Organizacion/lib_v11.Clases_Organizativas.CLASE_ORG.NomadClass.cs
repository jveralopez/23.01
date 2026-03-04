using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Clases_Organizativas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Organigramas
    public partial class CLASE_ORG
    {

        public static int CountPer(NomadXML Estr, bool AllUsers)
        {
            int retval = 0;

            //Recorro los child.
            for (NomadXML MyCUR = Estr.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                if (MyCUR.Name.ToUpper() == "ST")
                {
                    retval += CountPer(MyCUR, AllUsers);
                    NomadXML PE = MyCUR.FindElement("PE");
                    if (PE != null)
                        retval += 1 + PE.ChildLength;
                }

            //Recorro los PE
            NomadXML PERS = Estr.FindElement("PERS");
            if (PERS != null)
                for (NomadXML PE = PERS.FirstChild(); PE != null; PE = PE.Next())
                    retval += 1 + PE.ChildLength;

            return retval;
        }

        public static void AsignClass(string PRENAME, NomadXML Estr, bool AllUsers)
        {
            NomadBatch LOG = NomadBatch.GetBatch("Asignar Permisos", "Asignar Permisos");
            int Total = CountPer(Estr, AllUsers);

            AsignClass(LOG, 0, Total, PRENAME, Estr, AllUsers);
        }

        public static void AsignClass(NomadBatch LOG, int Count, int Tot, string PRENAME, NomadXML Estr, bool AllUsers)
        {
            NucleusRH.Base.Personal.Legajo.PERSONAL objPer;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objLeg;

            if (Estr == null) return;

            //Permisos sobre el Responsable
            NomadXML PE = Estr.FindElement("PE");
            if (PE != null)
            {
                Count++;
                LOG.SetPro(0, 100, Tot, Count);
                LOG.SetMess("Asignar permisos (" + Count.ToString() + " de " + Tot.ToString() + ")....");

                //Pego los Permisos sobre la persona
                objPer = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(PE.GetAttr("oi_personal"));
                objPer.Security.Policy = PRENAME;
                NomadEnvironment.GetCurrentTransaction().Save(objPer);

                for (NomadXML LE = PE.FirstChild(); LE != null; LE = LE.Next())
                {
                    Count++;
                    LOG.SetPro(0, 100, Tot, Count);
                    LOG.SetMess("Asignar permisos (" + Count.ToString() + " de " + Tot.ToString() + ")....");

                    //Pego los Permisos sobre el legajo
                    objLeg = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LE.GetAttr("oi_personal_emp"));
                    objLeg.Security.Policy = PRENAME;
                    NomadEnvironment.GetCurrentTransaction().Save(objLeg);
                }
            }

            NomadXML PERS = Estr.FindElement("PERS");
            if (PERS != null)
            {
                //Aplico la SEGURIDAD
                for (PE = PERS.FirstChild(); PE != null; PE = PE.Next())
                {
                    Count++;
                    LOG.SetPro(0, 100, Tot, Count);
                    LOG.SetMess("Asignar permisos (" + Count.ToString() + " de " + Tot.ToString() + ")....");

                    //Pego los Permisos sobre la persona
                    objPer = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(PE.GetAttr("oi_personal"));
                    objPer.Security.Policy = PRENAME + "/" + Estr.GetAttr("code");
                    NomadEnvironment.GetCurrentTransaction().Save(objPer);

                    for (NomadXML LE = PE.FirstChild(); LE != null; LE = LE.Next())
                    {
                        Count++;
                        LOG.SetPro(0, 100, Tot, Count);
                        LOG.SetMess("Asignar permisos (" + Count.ToString() + " de " + Tot.ToString() + ")....");

                        //Pego los Permisos sobre el legajo
                        objLeg = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(LE.GetAttr("oi_personal_emp"));
                        objLeg.Security.Policy = PRENAME + "/" + Estr.GetAttr("code");
                        NomadEnvironment.GetCurrentTransaction().Save(objLeg);
                    }
                }
            }

            //Recorro los child.
            for (NomadXML MyCUR = Estr.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                if (MyCUR.Name.ToUpper() == "ST")
                    AsignClass(LOG, Count, Tot, PRENAME + "/" + Estr.GetAttr("code"), MyCUR, AllUsers);

            return;
        }

        public static void LimpiarPermisos(string PolicyValue)
        {
            NomadBatch LOG = NomadBatch.GetBatch("Limpiar Permisos", "Limpiar Permisos");

            int t;
            bool startTrans = false;
            NomadXML MySQLPer = new NomadXML();
            NomadXML MySQLCur;
            NucleusRH.Base.Personal.Legajo.PERSONAL objPer;
            NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP objLeg;

            MySQLPer.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_PERSONAS_SEC, "<DATA CLASS=\"NucleusRH.Base.Personal.Legajo.PERSONAL\" POLICY=\"" + PolicyValue + "\"/>"));
            for (t = 1, MySQLCur = MySQLPer.FirstChild().FirstChild(); MySQLCur != null; MySQLCur = MySQLCur.Next(), t++)
            {
                LOG.SetPro(0, 45, MySQLPer.FirstChild().ChildLength, t);
                LOG.SetMess("Limpiar permisos en las Personas (" + t.ToString() + " de " + MySQLPer.FirstChild().ChildLength.ToString() + ")....");

                objPer = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(MySQLCur.GetAttr("ID"));
                objPer.Security.Policy = PolicyValue;

                if (!startTrans) NomadEnvironment.GetCurrentTransaction().Begin();
                startTrans = true;
                NomadEnvironment.GetCurrentTransaction().Save(objPer);
            }

            MySQLPer.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_PERSONAS_SEC, "<DATA CLASS=\"NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP\" POLICY=\"" + PolicyValue + "\"/>"));
            for (t = 1, MySQLCur = MySQLPer.FirstChild().FirstChild(); MySQLCur != null; MySQLCur = MySQLCur.Next(), t++)
            {
                LOG.SetPro(45, 90, MySQLPer.FirstChild().ChildLength, t);
                LOG.SetMess("Limpiar permisos en los Legajos (" + t.ToString() + " de " + MySQLPer.FirstChild().ChildLength.ToString() + ")....");

                objLeg = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(MySQLCur.GetAttr("ID"));
                objLeg.Security.Policy = PolicyValue;

                if (!startTrans) NomadEnvironment.GetCurrentTransaction().Begin();
                startTrans = true;
                NomadEnvironment.GetCurrentTransaction().Save(objLeg);
            }

            if (startTrans) NomadEnvironment.GetCurrentTransaction().Commit();

        }

        private static void CreatePOLICYTree(Nomad.Base.Security.Policies.POLICY POL, NomadXML Estr)
        {
            if (Estr == null) return;

            //Agrego el NODO.
            Nomad.Base.Security.Policies.POLICY ORG = new Nomad.Base.Security.Policies.POLICY();
            ORG.COD = Estr.GetAttr("code");
            ORG.DES = Estr.GetAttr("nombre");
            POL.POLICIES.Add(ORG);

            //Recorro los child.
            for (NomadXML MyCUR = Estr.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                if (MyCUR.Name.ToUpper() == "ST")
                    CreatePOLICYTree(ORG, MyCUR);
        }

        private static void RefreshPOLICYValues(string PRENAME, Nomad.Base.Security.Values.POLICY_VALUES VAL, NomadXML Estr)
        {
            if (Estr == null) return;

            //Agrego el NODO.
            Nomad.Base.Security.Values.POLICY_VALUE parentVAL = (Nomad.Base.Security.Values.POLICY_VALUE)VAL.VALUE.GetByAttribute("XPATH", PRENAME);
            Nomad.Base.Security.Values.POLICY_VALUE newVAL = (Nomad.Base.Security.Values.POLICY_VALUE)VAL.VALUE.GetByAttribute("XPATH", PRENAME + "/" + Estr.GetAttr("code"));

            if (parentVAL == null)
            {
                parentVAL = new Nomad.Base.Security.Values.POLICY_VALUE();
                parentVAL.ACCESS = false;
                parentVAL.XPATH = PRENAME;
                VAL.VALUE.Add(parentVAL);
            }

            if (newVAL == null)
            {
                newVAL = new Nomad.Base.Security.Values.POLICY_VALUE();
                newVAL.ACCESS = false;
                newVAL.XPATH = PRENAME + "/" + Estr.GetAttr("code");
                VAL.VALUE.Add(newVAL);
            }

            //Propago los Permisos del PADRE
            newVAL.GROUPS.Clear();
            foreach (Nomad.Base.Security.Values.POLICY_GROUP pGroup in parentVAL.GROUPS)
            {
                Nomad.Base.Security.Values.POLICY_GROUP nGroup = new Nomad.Base.Security.Values.POLICY_GROUP();
                nGroup.ACCESS = pGroup.ACCESS;
                nGroup.GROUP = pGroup.GROUP;

                newVAL.GROUPS.Add(nGroup);
            }
            newVAL.USERS.Clear();
            foreach (Nomad.Base.Security.Values.POLICY_USER pUser in parentVAL.USERS)
            {
                Nomad.Base.Security.Values.POLICY_USER nUser = new Nomad.Base.Security.Values.POLICY_USER();
                nUser.ACCESS = pUser.ACCESS;
                nUser.ACCOUNT = pUser.ACCOUNT;

                newVAL.USERS.Add(nUser);
            }
            //Agrego Usuarios Adicionales
            NomadXML PE = Estr.FindElement("PE");
            if (PE != null && (PE.GetAttr("oi_usuario_sistema") != ""))
            {
                NomadXML MySQLAccount = new NomadXML();
                MySQLAccount.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_ACCOUNT, "<DATA oi_usuario_sistema=\"" + PE.GetAttr("oi_usuario_sistema") + "\" />"));
                NomadXML MySQLPer = MySQLAccount.FindElement("ROWS");
                for (NomadXML MySQLAcc = MySQLPer.FirstChild(); MySQLAcc != null; MySQLAcc = MySQLAcc.Next())
                {
                    Nomad.Base.Security.Values.POLICY_USER nUser = (Nomad.Base.Security.Values.POLICY_USER)newVAL.USERS.GetByAttribute("ACCOUNT", MySQLAcc.GetAttr("ID"));
                    if (nUser == null)
                    {
                        nUser = new Nomad.Base.Security.Values.POLICY_USER();
                        nUser.ACCESS = true;
                        nUser.ACCOUNT = MySQLAcc.GetAttr("ID");
                        newVAL.USERS.Add(nUser);
                    }
                    nUser.ACCESS = true;
                }
            }

            //Recorro los child.
            for (NomadXML MyCUR = Estr.FirstChild(); MyCUR != null; MyCUR = MyCUR.Next())
                if (MyCUR.Name.ToUpper() == "ST")
                    RefreshPOLICYValues(newVAL.XPATH, VAL, MyCUR);
        }

        public static void UpdateTreePolicy(int oi_clase_org)
        {
            InternalGenerarPolitica(oi_clase_org, false, false);
        }

        public static void GenerarPolitica(int oi_clase_org)
        {
            InternalGenerarPolitica(oi_clase_org, false, true);
        }

        public static void GenerarPoliticaAll(int oi_clase_org, bool AllUsers)
        {
            InternalGenerarPolitica(oi_clase_org, AllUsers, true);
        }

        public static void InternalGenerarPolitica(int oi_clase_org, bool AllUsers, bool ChangeLegs)
        {
            NomadBatch LOG = NomadBatch.GetBatch("Propagar Permisos", "Propagar Permisos");
            LOG.SetMess("Comienza generar politica...");
            LOG.Log("Comienza generar politica...");

            //Basicos
            NomadProxy Proxy = NomadProxy.GetProxy();

            //Obtengo la lista de Estructuras
            LOG.SetMess("Obtengo las ESTRUCTURAS existentes...");
            LOG.Log("Obtengo las ESTRUCTURAS existentes...");
            NomadXML EstDoc = new NomadXML(10, 1024, 1024);
            EstDoc.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_POLICY, "<DATA oi_clase_org=\"" + oi_clase_org.ToString() + "\" />"));
            NomadXML EstrRoot = EstDoc.FirstChild().FirstChild();
            LOG.SetPro(10);

            //Obtengo el DDO de Politica
            LOG.SetMess("Obtengo el DDO de SEGURIDAD...");
            LOG.Log("Obtengo el DDO de SEGURIDAD...");
            Nomad.Base.Security.Policies.POLICY_BASE POLCLS = Nomad.Base.Security.Policies.POLICY_BASE.Get(Proxy.AppName);
            Nomad.Base.Security.Values.POLICY_VALUES POLVAL = Nomad.Base.Security.Values.POLICY_VALUES.Get(Proxy.AppName);

            //Busco la POLITICA DE ORGANIGRAMA RAIZ
            Nomad.Base.Security.Policies.POLICY POL = (Nomad.Base.Security.Policies.POLICY)POLCLS.POLICIES.GetByAttribute("COD", "ORG");
            if (POL == null)
            {
                POL = new Nomad.Base.Security.Policies.POLICY();
                POL.COD = "ORG";
                POL.DES = "Organigramas";
                POLCLS.POLICIES.Add(POL);
            }

            //Busco la POLITICA DE ORGANIGRAMA
            Nomad.Base.Security.Policies.POLICY ORG = (Nomad.Base.Security.Policies.POLICY)POL.POLICIES.GetByAttribute("COD", EstrRoot.GetAttr("code"));
            if (ORG == null)
            {
                ORG = new Nomad.Base.Security.Policies.POLICY();
                ORG.COD = EstrRoot.GetAttr("code");
                ORG.DES = EstrRoot.GetAttr("nombre");
                POL.POLICIES.Add(ORG);
            }
            ORG.POLICIES.Clear();
            LOG.SetPro(20);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            //Paso 1) Actualizo el ARBOL de ESTRUCTURAS
            LOG.SetMess("Actualizo las Estructuras Estructuras...");
            LOG.Log("Actualizo las Estructuras Estructuras...");
            CreatePOLICYTree(ORG, EstrRoot.FindElement("ST"));
            NomadEnvironment.GetCurrentTransaction().Save(POLCLS);
            LOG.SetPro(30);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            //Paso 2) Actualizo el ARBOL de PERMISOS
            LOG.SetMess("Permisos en la ESTRUCTURA...");
            LOG.Log("Permisos en la ESTRUCTURA...");
            RefreshPOLICYValues("./ORG/" + EstrRoot.GetAttr("code"), POLVAL, EstrRoot.FindElement("ST"));
            NomadEnvironment.GetCurrentTransaction().Save(POLVAL);
            LOG.SetPro(40);

            if (ChangeLegs)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                //Paso 3) Elimino el Acceso a todos los Usuarios....
                LOG.SetMess("Limpiar permisos en los Legajos/Personas....");
                LOG.Log("Limpiar permisos en los Legajos/Personas....");
                LOG.SetSubBatch(40, 70);
                LimpiarPermisos("./ORG/" + EstrRoot.GetAttr("code"));
                LOG.SetPro(70);

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                //Paso 6) Asigno Permisos en PERSONAS
                LOG.SetMess("Asigno Permisos en PERSONAS...");
                LOG.Log("Asigno Permisos en PERSONAS...");
                LOG.SetSubBatch(70, 100);
                AsignClass("./ORG/" + EstrRoot.GetAttr("code"), EstrRoot.FindElement("ST"), AllUsers);
            }

            LOG.SetPro(100);

            LOG.Log("Fin generar politica...");
        }



        public static NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG Duplicar(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG ddoORG)
        {
            CLASE_ORG ddoDUP = new CLASE_ORG();

            ddoDUP.c_clase_org = ddoORG.c_clase_org;
            ddoDUP.d_clase_org = ddoORG.d_clase_org;
            ddoDUP.f_desde_vigencia = ddoORG.f_desde_vigencia;
            ddoDUP.f_hasta_vigencia = ddoORG.f_hasta_vigencia;
            ddoDUP.c_estado = ddoORG.c_estado;
            ddoDUP.e_version = GetLastVersion(ddoORG.Id) + 1;
            ddoDUP.l_automatica = ddoORG.l_automatica;
            ddoDUP.o_clase_org = ddoORG.o_clase_org;

            //CARGO LA PRIMER ESTRUCTURA
            ESTRUCTURA estr = ESTRUCTURA.Get(ddoORG.oi_estructura_org.Id, false);

            ddoDUP.oi_estructura_org.oi_unidad_org = estr.oi_unidad_org;
            ddoDUP.oi_estructura_org.l_staff = estr.l_staff;
            ddoDUP.oi_estructura_org.oi_claseNull = true;
            ddoDUP.oi_estructura_org.oi_estr_padreNull = true;

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoDUP);

            ESTRUCTURA estrDUP = ESTRUCTURA.Get(ddoDUP.oi_estructura_org.Id, false);

            CloneEstr(estr, estrDUP, ddoDUP.Id.ToString());

            return ddoDUP;
        }
        public static void CloneEstr(NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA EstrOrigen, NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUCTURA EstrDestino, string idClase)
        {
            foreach (ESTRUC_PERS ddoEP in EstrOrigen.ESTRUC_PERS)
            {
                ESTRUC_PERS ddoEPNEW = new ESTRUC_PERS();
                ddoEPNEW.oi_personal_emp = ddoEP.oi_personal_emp;
                ddoEPNEW.l_responsable = ddoEP.l_responsable;
                ddoEPNEW.l_staff = ddoEP.l_staff;
                ddoEPNEW.oi_clase_org = idClase;
                EstrDestino.ESTRUC_PERS.Add(ddoEPNEW);
            }

            foreach (CLASE_HIJA ddoCH in EstrOrigen.CLASES_HIJAS)
            {
                CLASE_HIJA ddoCHNEW = new CLASE_HIJA();
                //ddoCHNEW.oi_clase_org = idClase;
                ddoCHNEW.oi_clase_org = ddoCH.oi_clase_org;

                EstrDestino.CLASES_HIJAS.Add(ddoCHNEW);
            }

            foreach (ESTRUCTURA ddoES in EstrOrigen.ESTRUCTURAS)
            {
                ESTRUCTURA ddoESNEW = new ESTRUCTURA();
                ddoESNEW.oi_unidad_org = ddoES.oi_unidad_org;
                ddoESNEW.l_staff = ddoES.l_staff;

                //Vistas				
                string param = "<DATO oi_estructura=\"" + ddoES.Id.ToString() + "\"/>";
                NomadXML xmlvistas = new NomadXML();
                xmlvistas.SetText(NomadEnvironment.QueryString(CLASE_ORG.Resources.QRY_VISTAS, param));
                if (xmlvistas.FirstChild().GetAttr("oi_vista") != "")
                    ddoESNEW.oi_vista = xmlvistas.FirstChild().GetAttrInt("oi_vista");

                EstrDestino.ESTRUCTURAS.Add(ddoESNEW);
            }

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(EstrDestino);


            int c = 0;
            foreach (ESTRUCTURA ddoES in EstrOrigen.ESTRUCTURAS)
            {
                ESTRUCTURA estr = ESTRUCTURA.Get(EstrDestino.ESTRUCTURAS[c].Id, false);
                CloneEstr(ddoES, estr, idClase);
                c++;
            }
        }
        public void TranslateVistas()
        {
            //Paso las vistas
            NomadXML xmlvistas;
            string param = "<DATO oi_clase_org=\"" + this.Id + "\"/>";
            xmlvistas = new NomadXML();
            xmlvistas.SetText(NomadEnvironment.QueryString(CLASE_ORG.Resources.QRY_VISTAS_EST, param));
            for (NomadXML xmlv = xmlvistas.FirstChild().FirstChild(); xmlv != null; xmlv = xmlv.Next())
            {
                NucleusRH.Base.Organizacion.Vistas.VISTA ddoVISTA = NucleusRH.Base.Organizacion.Vistas.VISTA.Get(xmlv.GetAttr("oi_vista"), false);
                ddoVISTA.oi_clase_org = this.Id;
                ddoVISTA.oi_estructura = xmlv.GetAttr("oi_estructura");
                NomadEnvironment.GetCurrentTransaction().Save(ddoVISTA);
            }
        }
        // Codigo fuente en LIB
        //public void GenerarPolitica( int oi_clase_org);

        public static void GenerarPolitica2(int oi_clase_org, bool ChangeLegs)
        {
            InternalGenerarPolitica(oi_clase_org, false, ChangeLegs);
        }
        // Codigo fuente en LIB
        //public void GenerarPoliticaAll( int oi_clase_org, bool AllUsers);

        // Codigo fuente en LIB
        //public void UpdateTreePolicy( int oi_clase_org);

        public static void QuitarLegajo(string estructura_id, string personal_emp_id)
        {
            //CARGO LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);

            //CARGO LA ESTRUC_PERS DE LA PERSONA QUE HAY QUE ELIMINAR
            ESTRUC_PERS ddoEstPer;
            ddoEstPer = (ESTRUC_PERS)ddoEstr.ESTRUC_PERS.GetByAttribute("oi_personal_emp", personal_emp_id);
            NomadEnvironment.GetTrace().Info("ESTRPER -- " + ddoEstPer.SerializeAll());

            //ELIMINO A LA PERSONA DE LA COLECCION DE PERSONAS EN LA ESTRUCTURA
            ddoEstr.ESTRUC_PERS.Remove(ddoEstPer);

            //GUARDO LA ELIMINACION
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void SetResponsable(string estructura_id, string personal_emp_id)
        {
            //CARGO LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);

            //CARGO LA ESTRUC_PERS DE LA PERSONA A SETEAR
            ESTRUC_PERS ddoEstPer;
            ddoEstPer = (ESTRUC_PERS)ddoEstr.ESTRUC_PERS.GetByAttribute("oi_personal_emp", personal_emp_id);

            ddoEstPer.l_responsable = true;
            //SETEO TODOS LAS DEMAS PERSONAS EN FALSE
            foreach (ESTRUC_PERS ddoPer in ddoEstr.ESTRUC_PERS)
            {
                if (ddoPer.Id != ddoEstPer.Id)
                    ddoPer.l_responsable = false;
            }

            //GUARDO LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void UnSetResponsable(string estructura_id, string personal_emp_id)
        {
            //CARGO LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);

            //CARGO LA ESTRUC_PERS DE LA PERSONA A SETEAR
            ESTRUC_PERS ddoEstPer;
            ddoEstPer = (ESTRUC_PERS)ddoEstr.ESTRUC_PERS.GetByAttribute("oi_personal_emp", personal_emp_id);

            ddoEstPer.l_responsable = false;

            //GUARDO LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void AddOrganigrama(string estructura_id, string organigrama_id)
        {
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);

            //CREO UN NUEVO CLASE HIJA
            NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA ddoClaseHija;
            ddoClaseHija = new NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA();
            ddoClaseHija.oi_clase_org = organigrama_id;

            //AGREGO EL ORGANIGRAMA A LA ESTRUCTURA
            ddoEstr.CLASES_HIJAS.Add(ddoClaseHija);

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public void SplitOrganigrama(string estructura_id)
        {
            ESTRUCTURA ddoEstrSplit = ESTRUCTURA.Get(estructura_id);

            //PREGUNTO SI LA ESTRUCUTRA NO ES LA CABECERA Y VUELVO A RECORRER LA CLASE PARA PARARME EN EL PADRE DE LA ESTRUCTURA A ELIMINAR
            if (ddoEstrSplit.oi_estr_padreNull)
                throw new NomadAppException("No puede separar el Organigrama en otro a partir de la Estructura Cabecera");

            //EN EL THIS TENGO LA CLASE, TENGO QUE AGREGARLE LA DEFINCION QUE ME DA LA ESTRUCTURA A SEPARAR
            NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.CopiarEstructura(this.Id, this.oi_estructura_org, ddoEstrSplit);

            //CREO UN NUEVO CLASE HIJA
            NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA ddoClaseHija;
            ddoClaseHija = new NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA();
            ddoClaseHija.oi_clase_org = this.Id;

            //AGREGO EL ORGANIGRAMA A LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(ddoEstrSplit.oi_estr_padre.ToString(), false);
            ddoEstr.CLASES_HIJAS.Add(ddoClaseHija);

            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                //GUARDO
                NomadLog.Info("this I -- " + this.SerializeAll());
                NomadEnvironment.GetCurrentTransaction().Save(this);
                NomadLog.Info("this F -- " + this.SerializeAll());
                //ELIMINO LA ESTRUCTURA DEL ORGANIGRAMA
                NomadEnvironment.GetCurrentTransaction().Delete(ddoEstrSplit);
                //GUARDO
                NomadLog.Info("ddoEstr I -- " + ddoEstr.SerializeAll());
                NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
                NomadLog.Info("ddoEstr F -- " + ddoEstr.SerializeAll());

                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new Exception("Se ha producido un error al intentar guardar la transaccion " + e.ToString());

            }
        }
        public static void DelOrganigrama(string clase_org_id, string estr_padre_id)
        {
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estr_padre_id, false);

            //CARGO LA CLASE HIJA
            NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA ddoClaseHija;
            ddoClaseHija = (NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_HIJA)ddoEstr.CLASES_HIJAS.GetByAttribute("oi_clase_org", clase_org_id);

            //QUITO DE LA ESTRUCTURA A LA CLASE
            ddoEstr.CLASES_HIJAS.Remove(ddoClaseHija);

            //GUARDO LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void AddLegajo(string clase_id, string estructura_id, Nomad.NSystem.Proxy.NomadXML legajos_ids, bool l_validate)
        {
            //CARGO LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);

            //SI SE REQUIERE SE VALIDA LA DUPLICIDAD DEL LEGAJO
            if (l_validate)
            {
                // Para cada persona del XML	

                for (NomadXML legajo = legajos_ids.FirstChild().FirstChild(); legajo != null; legajo = legajo.Next())
                {
                    NomadEnvironment.GetTrace().Info("ids -- " + legajo.GetAttr("id"));

                    //CON UN QRY VERIFICO SI EL LEGAJO YA EXISTE EN LA CLASE
                    NomadXML xmlflag;
                    string param = "<DATO oi_personal_emp=\"" + legajo.GetAttr("id") + "\" oi_clase_org=\"" + clase_id + "\"/>";
                    NomadEnvironment.GetTrace().Info(param.ToString());
                    xmlflag = new NomadXML();
                    xmlflag.SetText(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Clases_Organizativas.CLASE_ORG.Resources.QRY_FLAG_LEGAJO, param));
                    NomadEnvironment.GetTrace().Info(xmlflag.ToString());

                    //SI VUELVE UNO YA EXISTE EN EL ORGANIGRAMA
                    if (xmlflag.FirstChild().GetAttr("flag") == "1")
                        throw new NomadAppException("No se agregaron los legajos seleccionados a la Estructura debido a que el Legajo " + xmlflag.FirstChild().GetAttr("legajo") + " ya existe en otro nodo del Organigrama");
                    else
                    {
                        //CREO UN NUEVO ESTRUCPERS
                        NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoEstPer;
                        ddoEstPer = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                        ddoEstPer.oi_personal_emp = legajo.GetAttr("id");
                        ddoEstPer.l_responsable = false;
                        ddoEstPer.oi_clase_org = clase_id;

                        //AGREGO LA PERSONA A LA ESTRUCTURA
                        ddoEstr.ESTRUC_PERS.Add(ddoEstPer);
                    }
                }
            }
            else
            {
                // Para cada persona del XML
                for (NomadXML legajo = legajos_ids.FirstChild().FirstChild(); legajo != null; legajo = legajo.Next())
                {
                    NomadEnvironment.GetTrace().Info("ids -- " + legajo.GetAttr("id"));

                    //CREO UN NUEVO ESTRUCPERS
                    NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS ddoEstPer;
                    ddoEstPer = new NucleusRH.Base.Organizacion.Clases_Organizativas.ESTRUC_PERS();
                    ddoEstPer.oi_personal_emp = legajo.GetAttr("id");
                    ddoEstPer.l_responsable = false;
                    ddoEstPer.oi_clase_org = clase_id;

                    //AGREGO LA PERSONA A LA ESTRUCTURA
                    ddoEstr.ESTRUC_PERS.Add(ddoEstPer);
                }
            }

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void AddHijo(string estructura_id, string unidad_org_id, bool staff)
        {
            //CARGO LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);

            //CREO UN NUEVO ESTRUCTURA
            ESTRUCTURA ddoEstNew;
            ddoEstNew = new ESTRUCTURA();
            ddoEstNew.oi_unidad_org = unidad_org_id;
            ddoEstNew.l_staff = false;

            //AGREGO LA ESTRUCTURA
            ddoEstr.ESTRUCTURAS.Add(ddoEstNew);

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void Mover_Estructura(string estructura_change_id, string estructura_to_id)
        {
            //CARGO EL DDO DE LA ESTRUCTURA A MOVER
            Estructuras.ESTRUCAUX ddoESTRMOVE = Estructuras.ESTRUCAUX.Get(estructura_change_id);

            //CARGO LOS DATOS
            ddoESTRMOVE.oi_estr_padre = int.Parse(estructura_to_id);

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Save(ddoESTRMOVE);
        }
        public static void EliminarEstructura(string estructura_id, string unidad_org_id)
        {
            //CARGO LA ESTRUCTURA
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id);

            //GUARDO
            NomadEnvironment.GetCurrentTransaction().Delete(ddoEstr);
        }
        public static void SetStaff(string estructura_id)
        {
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);
            //Marco com STAFF
            ddoEstr.l_staff = true;

            //GUARDO LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static void DelStaff(string estructura_id)
        {
            ESTRUCTURA ddoEstr = ESTRUCTURA.Get(estructura_id, false);
            //Marco com STAFF
            ddoEstr.l_staff = false;

            //GUARDO LOS CAMBIOS
            NomadEnvironment.GetCurrentTransaction().Save(ddoEstr);
        }
        public static string GetIDVigente(string orgId)
        {
            NomadXML xmlversion;
            string param = "<DATO oi_clase_org=\"" + orgId + "\"/>";
            xmlversion = new NomadXML();
            xmlversion.SetText(NomadEnvironment.QueryString(CLASE_ORG.Resources.QRY_VIGENTE, param));
            return xmlversion.FirstChild().GetAttr("oi_clase_org");
        }
        public static int GetLastVersion(string orgId)
        {
            NomadXML xmlversion;
            string param = "<DATO oi_clase_org=\"" + orgId + "\"/>";
            xmlversion = new NomadXML();
            xmlversion.SetText(NomadEnvironment.QueryString(CLASE_ORG.Resources.QRY_VERSION, param));
            return xmlversion.FirstChild().GetAttrInt("e_version");
        }
        public static void Discontinuar(string orgId)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Crear Traza", "");

            objBatch.SetPro(0);
            objBatch.SetMess("Creando traza del Organigrama.");
            objBatch.Log("Comienza el proceso de cambio de estado a Discontinuado del Organigrama.");

            CLASE_ORG ddoORG = CLASE_ORG.Get(orgId, false);
            CLASE_ORG ddoORGVIG = new CLASE_ORG();

            objBatch.SetMess("Realizando copia del Organigrama.");
            objBatch.Log("Realizando copia del Organigrama.");

            objBatch.SetPro(30);
            ddoORGVIG = Duplicar(ddoORG);
            objBatch.SetPro(80);

            ddoORG.f_hasta_vigencia = DateTime.Now;
            ddoORG.c_estado = "D";

            //Graba nuevamente los datos del planificado
            string strIDCO = ddoORGVIG.Id.ToString();
            ddoORGVIG = null;
            ddoORGVIG = CLASE_ORG.Get(strIDCO, false);
            
            ddoORGVIG.f_desde_vigencia = DateTime.Now;
            ddoORGVIG.f_hasta_vigenciaNull = true;

            objBatch.SetMess("Guardando datos.");
            objBatch.Log("Guardando datos.");
            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(ddoORGVIG);
                NomadEnvironment.GetCurrentTransaction().Save(ddoORG);
                NomadEnvironment.GetCurrentTransaction().Commit();
                objBatch.SetPro(95);
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new Exception("Se ha producido un error al intentar guardar la transaccion " + e.ToString());
            }

            ddoORGVIG.TranslateVistas();
            objBatch.SetPro(100);
            objBatch.SetMess("El proceso finalizo correctamente.");
            objBatch.Log("El proceso finalizo correctamente.");
        }
        public static void Planificar(string orgId)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Crear Traza", "");

            objBatch.SetPro(0);
            objBatch.SetMess("Creando copia del Organigrama.");
            objBatch.Log("Comienza el proceso de cambio de estado a Planificado del Organigrama.");

            CLASE_ORG ddoORG = CLASE_ORG.Get(orgId, false);
            CLASE_ORG ddoORGPL = new CLASE_ORG();

            objBatch.SetPro(30);
            ddoORGPL = Duplicar(ddoORG);
            objBatch.SetPro(80);


            //Graba nuevamente los datos del planificado
            string strIDCO = ddoORGPL.Id.ToString();
            ddoORGPL = null;
            ddoORGPL = CLASE_ORG.Get(strIDCO, false);
            

            ddoORGPL.c_estado = "P";
            ddoORGPL.f_desde_vigenciaNull = true;
            ddoORGPL.f_hasta_vigenciaNull = true;
            
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoORGPL);
            //return ddoORGPL.Id.ToString();
        }
        public static void Rechazar(string orgId)
        {
            CLASE_ORG ddoORG = CLASE_ORG.Get(orgId, false);
            ddoORG.c_estado = "R";
            NomadEnvironment.GetCurrentTransaction().Save(ddoORG);
        }
        public static void Vigente(string orgId)
        {
            CLASE_ORG ddoORGPL = CLASE_ORG.Get(orgId, false);
            ddoORGPL.c_estado = "V";
            ddoORGPL.f_desde_vigencia = DateTime.Now;

            //Obtengo el ID del Organigrama Vigente
            string vigId = GetIDVigente(orgId);
            CLASE_ORG ddoORGVIG = CLASE_ORG.Get(vigId, false);
            ddoORGVIG.f_hasta_vigencia = DateTime.Now;
            ddoORGVIG.c_estado = "D";

            //Obtengo todos los Organigramas que estan Planificados 
            NomadXML xmlplanificados;
            string param = "<DATO oi_clase_org=\"" + orgId + "\"/>";
            xmlplanificados = new NomadXML();
            xmlplanificados.SetText(NomadEnvironment.QueryString(CLASE_ORG.Resources.QRY_PLANIFICADOS, param));
            for (NomadXML xmlorg = xmlplanificados.FirstChild().FirstChild(); xmlorg != null; xmlorg = xmlorg.Next())
            {
                CLASE_ORG ddoORGREC = CLASE_ORG.Get(xmlorg.GetAttr("oi_clase_org"), false);
                ddoORGREC.c_estado = "R";
                NomadEnvironment.GetCurrentTransaction().Save(ddoORGREC);
            }

            NomadEnvironment.GetCurrentTransaction().Begin();
            try
            {
                NomadEnvironment.GetCurrentTransaction().Save(ddoORGVIG);
                NomadEnvironment.GetCurrentTransaction().Save(ddoORGPL);
                NomadEnvironment.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                NomadEnvironment.GetCurrentTransaction().Rollback();
                throw new Exception("Se ha producido un error al intentar guardar la transaccion " + e.ToString());
            }
        }
        public static void PasarAVigente(int pOiClaseOrgPla)
        {
            //Crea el objeto que realiza el paso a Vigente de un organigrama planificado
            NucleusRH.Base.Organizacion.Puestos.clsOrganigrama objOrganigrama;

            objOrganigrama = new NucleusRH.Base.Organizacion.Puestos.clsOrganigrama(pOiClaseOrgPla);

            objOrganigrama.PasarAVigente();
        }

    }
}


