using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Control_de_Visitas.Visitas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Visitas
    public partial class VISITA
    {

        public static void ModificarVisitantes(Nomad.NSystem.Document.NmdXmlDocument PARAM, Nomad.NSystem.Document.NmdXmlDocument DATA, string Par, string Cant)
        {

            //RECUPERO TODOS LOS VISITA_VIS A PARTIR DE LOS IDS QUE INGRESAN COMO PARAMETROS
            Nomad.NSystem.Document.NmdXmlDocument vis;
            int C = 0;

            //CUENTO LA CANTIDAD DE VISITANTES QUE VOY A MODIFICAR
            for (vis = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetFirstChildDocument(); vis != null; vis = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetNextChildDocument())
            {
                C++;
            }

            //VALIDO QUE, SI A LOS VISITANTES SE LE ESTA INGRESANDO LA FECHA DE ENTRADA, NO SE SUPERE LA CANTIDAD DE VISITANTES
            //SIMULTANEOS EN PLANTA
            int cantvis = 0;
            cantvis = C + int.Parse(Cant);
            if (cantvis > int.Parse(Par) && DATA.GetAttribute("f_fechahora_ent").Value != "")
            {
                throw new NomadAppException("No se puede guardar la Visita porque se ha superado la cantidad maxima de Visitantes simultaneos en planta");
            }

            //DESPUES DE VALIDAR, RECORRO LOS VISITANTES SELCCIONADOS
            for (vis = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetFirstChildDocument(); vis != null; vis = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetNextChildDocument())
            {
                NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisitavis;
                ddoVisitavis = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS.Get(vis.GetAttribute("id").Value);
                NomadEnvironment.GetTrace().Info("VISITA_VIS1 -- " + ddoVisitavis.SerializeAll());

                //PEGO TODOS LOS DATOS INGRESADOS EN EL VISITANTE

                if (DATA.GetAttribute("f_fechahora_sal_es").Value == "")
                {
                    //ddoVisitavis.f_fechahora_sal_esNull = true;
                }
                else
                {
                    ddoVisitavis.f_fechahora_sal_es = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_es").Value);
                }

                if (DATA.GetAttribute("f_fechahora_ent").Value == "")
                {
                    //ddoVisitavis.f_fechahora_entNull = true;
                }
                else
                {
                    ddoVisitavis.f_fechahora_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_ent").Value);
                }

                if (DATA.GetAttribute("f_fechahora_sal").Value == "")
                {
                    //ddoVisitavis.f_fechahora_salNull = true;
                }
                else
                {
                    ddoVisitavis.f_fechahora_sal = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal").Value);
                }

                if (DATA.GetAttribute("f_fechahora_en_ent").Value == "")
                {
                    //ddoVisitavis.f_fechahora_en_entNull =true;
                }
                else
                {
                    ddoVisitavis.f_fechahora_en_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_en_ent").Value);
                }

                if (DATA.GetAttribute("f_fechahora_sal_en").Value == "")
                {
                    //ddoVisitavis.f_fechahora_sal_enNull = true;
                }
                else
                {
                    ddoVisitavis.f_fechahora_sal_en = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_en").Value);
                }

                if (DATA.GetAttribute("d_credencial").Value == "")
                {
                    //ddoVisitavis.d_credencialNull = true;
                }
                else
                {
                    ddoVisitavis.d_credencial = DATA.GetAttribute("d_credencial").Value;
                }

                if (DATA.GetAttribute("d_credencial").Value == "")
                {
                    //ddoVisitavis.d_credencialNull = true;
                }
                else
                {
                    ddoVisitavis.d_credencial = DATA.GetAttribute("d_credencial").Value;
                }

                if (/*DATA.GetAttribute("l_anteojos").Value == "" || */DATA.GetAttribute("l_anteojos").Value == "0")
                {
                    ddoVisitavis.l_anteojos = false;
                }
                if (DATA.GetAttribute("l_anteojos").Value == "1")
                {
                    ddoVisitavis.l_anteojos = true;
                }

                if (/*DATA.GetAttribute("l_casco").Value == "" || */DATA.GetAttribute("l_casco").Value == "0")
                {
                    ddoVisitavis.l_casco = false;
                }
                if (DATA.GetAttribute("l_casco").Value == "1")
                {
                    ddoVisitavis.l_casco = true;
                }

                //GUARDO LA VISITAVIS
                NomadEnvironment.GetCurrentTransaction().Save(ddoVisitavis);
            }

        }
        public void Validar(int Cont, string Param, string Cant)
        {

            if (Param == "")
                throw new NomadAppException("No se encuentra el parámetro 'Maxima Cantidad de Visitantes en Planta'");

            //TENGO LA VISITA
            NomadEnvironment.GetTrace().Info("VISITA -- " + this.SerializeAll());

            //VALIDAR CANTIDAD
            int C = 0;
            int D = 0;
            //RECORRO LOS VISITANTES EN LA VISITA
            foreach (NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisitaVis in this.VISITAS_VIS)
            {

                //CUENTO TODOS LOS VISITANTES QUE SE VAN A GUARDAR
                if (!ddoVisitaVis.IsForDelete)
                {
                    C++;
                }

                //CUENTO TODOS LOS VISITANTES QUE SE VAN A GUARDAR CON FECHA DE ENTRADA Y SIN FECHA DE SALIDA
                if (ddoVisitaVis.IsForInsert || ddoVisitaVis.IsForUpdate)
                {
                    if (!ddoVisitaVis.f_fechahora_entNull && ddoVisitaVis.f_fechahora_ent < DateTime.Now)
                    {
                        if (ddoVisitaVis.f_fechahora_salNull)
                        {
                            D++;
                        }
                    }
                }
            }
            NomadEnvironment.GetTrace().Info("C -- " + C.ToString());
            NomadEnvironment.GetTrace().Info("D cant visitante que van a entrar-- " + D.ToString());

            //COMPARO EL ATRIBUTO CANTIDAD DE LA VISITA CON LOS VISITANTES INGRESADOS
            if (C != Cont)
            {
                throw new NomadAppException("La cantidad de Visitantes en la Visita es incorrecta");
            }

            //VALIDAR CANTIDAD MAXIMA DE VISITANTES
            //COMPARO LA CANTIDAD DE VISITANTES QUE INGRESARON A LA PLANTA MAS LO QUE YA ESTAN CONTRA EL PARAMETRO
            int cantvis = 0;
            cantvis = D + int.Parse(Cant);
            if (cantvis > int.Parse(Param))
            {
                throw new NomadAppException("No se puede guardar la Visita porque se ha superado la cantidad maxima de Visitantes simultaneos en planta");
            }

            //VALIDAR CREDENCIALES
            //VALIDAR QUE EL VISITANTE A INGRESAR NO ESTE YA CARGADO EN OTRA VISITA SIN FECHA DE SALIDA
            //CON UN RECURSO RECUPERO, LAS CREDENCIALES QUE SE LES ASIGNARON A LOS VISITANTES QUE ESTAN DENTRO DE LA PLANTA
            Nomad.NSystem.Document.NmdXmlDocument creds = null;
            creds = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Control_de_Visitas.Visitas.VISITA.Resources.QRY_CREDS, ""));
            NomadEnvironment.GetTrace().Info("CREDS -- " + creds.ToString());

            foreach (NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisVis in this.VISITAS_VIS)
            {
                Nomad.NSystem.Document.NmdXmlDocument credencial;
                for (credencial = (Nomad.NSystem.Document.NmdXmlDocument)creds.GetFirstChildDocument(); credencial != null; credencial = (Nomad.NSystem.Document.NmdXmlDocument)creds.GetNextChildDocument())
                {
                    if (ddoVisVis.f_fechahora_sal > DateTime.Now || ddoVisVis.f_fechahora_salNull)
                    {
                        if (ddoVisVis.d_credencial == credencial.GetAttribute("d_credencial").Value && credencial.GetAttribute("oi_visita_vis").Value != ddoVisVis.Id)
                        {
                            throw new NomadAppException("No se puede guardar la Visita porque se ha asignado a algun Visitante una credencial actualmente en uso");
                        }
                    }
                }
                string param = "<DATO oi_visita=\"" + ddoVisVis.oi_visita + "\" oi_visitante=\"" + ddoVisVis.oi_visitante + "\"/>";
                NomadEnvironment.GetTrace().Info("PARAM -- " + param);
                Nomad.NSystem.Document.NmdXmlDocument flag = null;
                flag = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Control_de_Visitas.Visitas.VISITA.Resources.QRY_VISITANTE, param));
                NomadEnvironment.GetTrace().Info("FLAG -- " + flag.ToString());
                if (flag.GetAttribute("flag").Value == "1" && !ddoVisVis.f_fechahora_entNull)
                {
                    throw new NomadAppException("No se puede guardar la Visita porque existe algun Visitante en la misma, ya registrado con Fecha-Hora Entrada y sin Fecha-Hora de Salida");
                }

            }
            NomadEnvironment.GetTrace().Info("FIN METODO -- " + this.SerializeAll());

        }
        public static void DelVisitas()
        {

            //TIRO UN RECURSO QUE ME TRAIGA DE ACUERDO AL PARAMETRO CORRRESPONDIENTE QUE RECUPERO EN EL RECURSO
            //LAS VISITAS A ELIMINAR

            NomadEnvironment.GetTrace().Info("SCHD EJECUTANDO");

            Nomad.NSystem.Document.NmdXmlDocument xml_doc = null;
            xml_doc = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Control_de_Visitas.Visitas.VISITA.Resources.QRY_DEL_VISITAS, ""));
            NomadEnvironment.GetTrace().Info(xml_doc.ToString());

            //PREGUNTO SI EL RECURSO DEVUELVE VALORES
            if (xml_doc.GetFirstChildDocument() != null)
            {
                Nomad.NSystem.Document.NmdXmlDocument doc;

                //PARA CADA OI RECUPERADO
                for (doc = (Nomad.NSystem.Document.NmdXmlDocument)xml_doc.GetFirstChildDocument(); doc != null; doc = (Nomad.NSystem.Document.NmdXmlDocument)xml_doc.GetNextChildDocument())
                {
                    //CARGO LA VISITA Y LA ELIMINO
                    NucleusRH.Base.Control_de_Visitas.Visitas.VISITA ddoVisita;
                    ddoVisita = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA.Get(doc.GetAttribute("oi_visita").Value);

                    //LA ELIMINO
                    NomadEnvironment.GetCurrentTransaction().Delete(ddoVisita);
                }
            }

        }

        public static void IngresoMasivo(Nomad.NSystem.Document.NmdXmlDocument VISITA_VIS, Nomad.NSystem.Document.NmdXmlDocument DATA,Nomad.NSystem.Proxy.NomadXML VISITANTES, string Par, string Cant)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("----------------------------------IngresoMasivo-------------------------------------");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            NomadEnvironment.GetTrace().Info("RECUPERANDO-VISITA");
            //RECUPERO ID DE LA VISITA
            //Nomad.NSystem.Document.NmdXmlDocument ID_VISITA;
            string ID = VISITA_VIS.GetAttribute("id").Value;
            //convierto el ID de string a entero
            int ID_VISITA = Int16.Parse(ID);
            NomadEnvironment.GetTrace().Info("VISITA --" + ID_VISITA);
            //NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisitavis;
            //ddoVisitavis = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS.Get(VISITA_VIS.GetAttribute("id").Value);

            //ROCORRO "VISITANTES", RECUPERO LOS IDs y CREO UN REGISTRO EN VISITA_VIS POR CADA VISITANTE QUE INGRESA
            ArrayList ROWS;
            ROWS = VISITANTES.FirstChild().FirstChild().GetChilds();
            //Nomad.NSystem.Proxy.NomadXML visitante;
            NucleusRH.Base.Control_de_Visitas.Visitas.VISITA ddoVisita;

            foreach (NomadXML row in ROWS)
            {
                string oi_visitante = row.GetAttr("id");
                string param = "<DATO oi_visita='" + ID + "' oi_visitante='" + oi_visitante + "'/>";
                NomadXML OI_VisitaVis = NomadEnvironment.QueryNomadXML(Control_de_Visitas.Visitas.VISITA.Resources.QRY_VISITA_VIS, param);
                string id_visitavis = OI_VisitaVis.FirstChild().GetAttr("oi_visita_vis");

                ddoVisita = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA.Get(ID_VISITA);
                int cant = ddoVisita.e_cantidad;

                if (id_visitavis == "")
                {
                    cant++;
                    NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisita_Vis = new NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS();
                    string id_visitante=row.GetAttr("id");
                    NomadEnvironment.GetTrace().Info("SETEO ATRIBUTOS AL VISITANTE --" + id_visitante);
                    ddoVisita_Vis.oi_visitante = id_visitante;
                    ddoVisita_Vis.oi_visita = ID_VISITA;

                    if (DATA.GetAttribute("f_fechahora_sal_es").Value == ""){/*ddoVisitavis.f_fechahora_sal_esNull = true;*/}
                    else{ ddoVisita_Vis.f_fechahora_sal_es = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_es").Value);}

                    if (DATA.GetAttribute("f_fechahora_ent").Value == ""){/*ddoVisitavis.f_fechahora_entNull = true;*/}
                    else { ddoVisita_Vis.f_fechahora_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_ent").Value);}

                    //entrada entrevista
                    if (DATA.GetAttribute("f_fechahora_en_ent").Value == "") { /*ddoVisitavis.f_fechahora_en_entNull =true;*/}
                    else { ddoVisita_Vis.f_fechahora_en_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_en_ent").Value); }
                    //salida entrevista
                    if (DATA.GetAttribute("f_fechahora_sal_en").Value == "") { /*ddoVisitavis.f_fechahora_sal_enNull = true;*/}
                    else { ddoVisita_Vis.f_fechahora_sal_en = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_en").Value); }

                    //if (DATA.GetAttribute("f_fechahora_sal").Value == ""){/*ddoVisitavis.f_fechahora_salNull = true;*/}
                    //else { ddoVisita_Vis.f_fechahora_sal = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal").Value); }

                    if (DATA.GetAttribute("l_anteojos").Value == "0"){ ddoVisita_Vis.l_anteojos = false; }
                    if (DATA.GetAttribute("l_anteojos").Value == "1"){ ddoVisita_Vis.l_anteojos = true; }

                    if (DATA.GetAttribute("l_casco").Value == "0"){ ddoVisita_Vis.l_casco = false; }
                    if (DATA.GetAttribute("l_casco").Value == "1") { ddoVisita_Vis.l_casco = true; }

                    //ACTUALIZO LA CANTIDAD DE VISITANTES EN LA VISITA
                    ddoVisita.e_cantidad = cant;

                    ddoVisita.VISITAS_VIS.Add(ddoVisita_Vis);
                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoVisita);
                }
                else
                {
                    NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisita_Vis;
                    int ID_VISITA_VIS = Int16.Parse(id_visitavis);
                    ddoVisita_Vis = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS.Get(ID_VISITA_VIS);

                    NomadEnvironment.GetTrace().Info("SETEO ATRIBUTOS AL VISITANTE --" + row.GetAttr("id") + "Y VISITA_VIS --" + ID_VISITA_VIS);

                    if (DATA.GetAttribute("f_fechahora_sal_es").Value == "") { /*ddoVisitavis.f_fechahora_sal_esNull = true;*/}
                    else { ddoVisita_Vis.f_fechahora_sal_es = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_es").Value); }

                    if (DATA.GetAttribute("f_fechahora_ent").Value == "") { /*ddoVisitavis.f_fechahora_entNull = true;*/}
                    else { ddoVisita_Vis.f_fechahora_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_ent").Value); }

                    if (DATA.GetAttribute("f_fechahora_en_ent").Value == "") { /*ddoVisitavis.f_fechahora_en_entNull =true;*/}
                    else { ddoVisita_Vis.f_fechahora_en_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_en_ent").Value); }

                    if (DATA.GetAttribute("f_fechahora_sal_en").Value == "") { /*ddoVisitavis.f_fechahora_sal_enNull = true;*/}
                    else { ddoVisita_Vis.f_fechahora_sal_en = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_en").Value); }

                    if (DATA.GetAttribute("l_anteojos").Value == "0") { ddoVisita_Vis.l_anteojos = false; }
                    if (DATA.GetAttribute("l_anteojos").Value == "1") { ddoVisita_Vis.l_anteojos = true; }

                    if (DATA.GetAttribute("l_casco").Value == "0") { ddoVisita_Vis.l_casco = false; }
                    if (DATA.GetAttribute("l_casco").Value == "1") { ddoVisita_Vis.l_casco = true; }

                    NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoVisita_Vis);
                }
            }

            /*foreach (NomadXML row in ROWS)
            {
                cant++;
                NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisita_Vis = new NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS();
                string id_visitante=row.GetAttr("id");
                NomadEnvironment.GetTrace().Info("SETEO ATRIBUTOS AL VISITANTE --" + id_visitante);
                ddoVisita_Vis.oi_visitante = id_visitante;
                ddoVisita_Vis.oi_visita = ID_VISITA;
                */
                //if (DATA.GetAttribute("f_fechahora_sal_es").Value == ""){/*ddoVisitavis.f_fechahora_sal_esNull = true;*/}
                //else{ ddoVisita_Vis.f_fechahora_sal_es = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_es").Value);}

                //if (DATA.GetAttribute("f_fechahora_ent").Value == ""){/*ddoVisitavis.f_fechahora_entNull = true;*/}
                //else { ddoVisita_Vis.f_fechahora_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_ent").Value);}

                //if (DATA.GetAttribute("f_fechahora_en_ent").Value == "") { /*ddoVisitavis.f_fechahora_en_entNull =true;*/}
                //else { ddoVisita_Vis.f_fechahora_en_ent = StringUtil.str2date(DATA.GetAttribute("f_fechahora_en_ent").Value); }

                //if (DATA.GetAttribute("f_fechahora_sal_en").Value == "") { /*ddoVisitavis.f_fechahora_sal_enNull = true;*/}
                //else { ddoVisita_Vis.f_fechahora_sal_en = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal_en").Value); }

                //if (DATA.GetAttribute("f_fechahora_sal").Value == ""){/*ddoVisitavis.f_fechahora_salNull = true;*/}
                //else { ddoVisita_Vis.f_fechahora_sal = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal").Value); }

                //if (DATA.GetAttribute("l_anteojos").Value == "0"){ ddoVisita_Vis.l_anteojos = false; }
                //if (DATA.GetAttribute("l_anteojos").Value == "1"){ ddoVisita_Vis.l_anteojos = true; }

                //if (DATA.GetAttribute("l_casco").Value == "0"){ ddoVisita_Vis.l_casco = false; }
                //if (DATA.GetAttribute("l_casco").Value == "1") { ddoVisita_Vis.l_casco = true; }

                ////ACTUALIZO LA CANTIDAD DE VISITANTES EN LA VISITA
                //ddoVisita.e_cantidad = cant;

                //ddoVisita.VISITAS_VIS.Add(ddoVisita_Vis);
                //NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoVisita);
            //}

        }

        public static void EgresoMasivo(Nomad.NSystem.Document.NmdXmlDocument VISITA_VIS, Nomad.NSystem.Document.NmdXmlDocument DATA, Nomad.NSystem.Proxy.NomadXML VISITANTES, Nomad.NSystem.Proxy.NomadXML VISITA_VISITANTES, string Par, string Cant)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("----------------------------------EgresoMasivo-------------------------------------");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            NomadEnvironment.GetTrace().Info("RECUPERANDO-VISITA");
            //RECUPERO ID DE LA VISITA
            string ID = VISITA_VIS.GetAttribute("id").Value;
            int ID_VISITA = Int16.Parse(ID);
            NomadEnvironment.GetTrace().Info("VISITA --" + ID_VISITA);

            //ROCORRO "VISITANTES", RECUPERO LOS IDs y CREO UN REGISTRO EN VISITA_VIS POR CADA VISITANTE QUE INGRESA
            ArrayList ROWS;
            ROWS = VISITANTES.FirstChild().FirstChild().GetChilds();

            //NucleusRH.Base.Control_de_Visitas.Visitas.VISITA ddoVisita;
            //ddoVisita = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA.Get(ID_VISITA);

            foreach (NomadXML row in ROWS)
            {
                //RECUPERO EL OI_VISITA_VIS SEGUN LOS VISITANTES QUE TENGO QUE UPDATEAR
                string oi_visitante = row.GetAttr("id");
                string param = "<DATO oi_visita='" + ID + "' oi_visitante='" + oi_visitante + "'/>";
                //string XML = "<DATA oi_pedido_lf='" + strOiPLF + "'/>";
                //strOiPLF = r.GetAttr("oi_pedido_lf");

                NomadEnvironment.GetTrace().Info("oi_visita_vis -- " + param);
                NomadXML OI_VisitaVis = NomadEnvironment.QueryNomadXML(Control_de_Visitas.Visitas.VISITA.Resources.QRY_VISITA_VIS, param);
                //NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisita_Vis = new NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS();

                NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS ddoVisita_Vis;
                string id_visitavis = OI_VisitaVis.FirstChild().GetAttr("oi_visita_vis");
                int ID_VISITA_VIS = Int16.Parse(id_visitavis);
                ddoVisita_Vis = NucleusRH.Base.Control_de_Visitas.Visitas.VISITA_VIS.Get(ID_VISITA_VIS);

                NomadEnvironment.GetTrace().Info("SETEO ATRIBUTOS AL VISITANTE --" + row.GetAttr("id") + "Y VISITA_VIS --" + ID_VISITA_VIS);

                if (DATA.GetAttribute("f_fechahora_sal").Value == "") { /*ddoVisitavis.f_fechahora_salNull = true;*/}
                else { ddoVisita_Vis.f_fechahora_sal = StringUtil.str2date(DATA.GetAttribute("f_fechahora_sal").Value); }

                if (DATA.GetAttribute("l_anteojos").Value == "0") { ddoVisita_Vis.l_anteojos = false; }
                if (DATA.GetAttribute("l_anteojos").Value == "1") { ddoVisita_Vis.l_anteojos = true; }

                if (DATA.GetAttribute("l_casco").Value == "0") { ddoVisita_Vis.l_casco = false; }
                if (DATA.GetAttribute("l_casco").Value == "1") { ddoVisita_Vis.l_casco = true; }

                NomadEnvironment.GetCurrentTransaction().Save(ddoVisita_Vis);
                //ddoVisita.VISITAS_VIS.Add(ddoVisita_Vis);
               // NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoVisita);

            }
        }

        public static void Validar_fechas(NomadXML PARAM)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("------------------------------------Valido_fechas-----------------------------------------");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            //RECUPERO ID DE LA VISITA
            //string ID = PARAM.FirstChild().GetAttr("id");
            //int ID_VISITA = Int16.Parse(ID);
            //NomadEnvironment.GetTrace().Info("VISITA --" + ID_VISITA);

            DateTime ENTRADA_ESTIPULADA = PARAM.FirstChild().GetAttrDateTime("f_fechahora_ent_es");
            string date_ent_es = ENTRADA_ESTIPULADA.ToString("dd/MM/yyyy HH:mm");
            DateTime fecha_entrada_estipulada = DateTime.Parse(date_ent_es);

            NomadXML VISITAS_VIS;
            VISITAS_VIS = PARAM.FirstChild().FindElement("VISITAS_VIS");
            //RECORRO LOS VISITANTES Y VALIDO SI TIENEN FECHA-HORA DE ENTRADA REAL MAYOR QUE LA FECHA-HORA DE ENTRADA ESTIPULADA
            //LA FECHA-HORA ESTIPULADA DEBE SER MAYOR O IGUAL QUE LA FECHA-HORA DE ENTRADA REAL
            /*nuevo*/
            string fecha_entrada;
            string fecha_salida_estipulada;
            string fecha_salida_entrevista;
            string fecha_entrada_entrevista;
            string status;
            /*fin nuevo*/
            foreach (NomadXML row in VISITAS_VIS.GetChilds())
            {
                status = row.GetAttr("nmd-status");
                if (status != "~d,")
                {
                    fecha_entrada = row.GetAttr("f_fechahora_ent");
                    fecha_salida_estipulada = row.GetAttr("f_fechahora_sal_es");
                    fecha_salida_entrevista = row.GetAttr("f_fechahora_sal_en");
                    fecha_entrada_entrevista = row.GetAttr("f_fechahora_en_ent");
                    if (fecha_entrada != "")
                    {
                        //DateTime fecha_entrada_real = DateTime.Parse(row.GetAttr("f_fechahora_ent"));
                        DateTime ENTRADA_REAL = row.GetAttrDateTime("f_fechahora_ent");
                        string date_ent = ENTRADA_REAL.ToString("dd/MM/yyyy HH:mm");
                        DateTime fecha_entrada_real = DateTime.Parse(date_ent);

                        if (fecha_entrada_real > fecha_entrada_estipulada)
                        {
                            throw new NomadAppException("No se puede guardar la Visita porque existe algun Visitante en la misma, con Fecha-Hora Entrada Real mayor que Fecha-Hora Entrada Estipulada");
                        }
                    }
                    if (fecha_salida_estipulada != "")
                    {
                        DateTime SALIDA_ESTIPULADA = row.GetAttrDateTime("f_fechahora_sal_es");
                        string date_sal_est = SALIDA_ESTIPULADA.ToString("dd/MM/yyyy HH:mm");
                        DateTime fecha_salida_est = DateTime.Parse(date_sal_est);

                        if (fecha_salida_est < fecha_entrada_estipulada)
                        {
                            throw new NomadAppException("No se puede guardar la Visita porque existe algun Visitante en la misma, con Fecha-Hora Salida Estipulada menor que Fecha-Hora Entrada Estipulada");
                        }
                    }
                    if (fecha_salida_entrevista != "")
                    {
                        DateTime SALIDA_ENTREVISTA = row.GetAttrDateTime("f_fechahora_sal_en");
                        string date_sal_ent = SALIDA_ENTREVISTA.ToString("dd/MM/yyyy HH:mm");
                        DateTime fecha_salida_entrev = DateTime.Parse(date_sal_ent);

                        if (fecha_salida_entrev < fecha_entrada_estipulada)
                        {
                            throw new NomadAppException("No se puede guardar la Visita porque existe algun Visitante en la misma, con Fecha-Hora Salida Entrevista menor que Fecha-Hora Entrada Estipulada");
                        }
                    }
                    if (fecha_entrada_entrevista != "")
                    {
                        DateTime ENTRADA_ENTREVISTA = row.GetAttrDateTime("f_fechahora_en_ent");
                        string date_ent_ent = ENTRADA_ENTREVISTA.ToString("dd/MM/yyyy HH:mm");
                        DateTime fecha_entrada_entrev = DateTime.Parse(date_ent_ent);

                        if (fecha_entrada_entrev < fecha_entrada_estipulada)
                        {
                            throw new NomadAppException("No se puede guardar la Visita porque existe algun Visitante en la misma, con Fecha-Hora Entrada Entrevista menor que Fecha-Hora Entrada Estipulada");
                        }
                    }
                }
            }
        }
        public static void ValidarVisita(NomadXML VISITA_VIS, string Param, string Cant)
        {
            NomadLog.Debug("------------------------------------------------------------------------------------------");
            NomadLog.Debug("------------------------------------ValidoVisita-----------------------------------------");
            NomadLog.Debug("------------------------------------------------------------------------------------------");

            NomadXML VISITAS_VIS;
            VISITAS_VIS = VISITA_VIS.FirstChild().FindElement("VISITAS_VIS");
            string status_visita = VISITA_VIS.FirstChild().GetAttr("nmd-status");
            if (status_visita != "") { status_visita = status_visita.Substring(0, 3); }
            foreach (NomadXML row in VISITAS_VIS.GetChilds())
            {
                string status_cadena;
                string status = "1";
                status_cadena = row.GetAttr("nmd-status");
                if (status_cadena != "") { status = status_cadena.Substring(0, 3); }
                string oi_visitante = row.FirstChild().GetAttr("value");
                string param = "<DATO oi_visitante='" + oi_visitante + "'/>";

                if ((status == "~i,")) //&& f_entrada !=""
                {
                    //VALIDAR QUE EL VISITANTE NO SE ENCUENTRE EN CURSO
                    /*llamo al recurso que me devuelve si el visitante que quiero ingresar
                    ya se encuentra "En curso" en otra visita*/
                    NomadEnvironment.GetTrace().Info("oi_visitante -- " + param);
                    NomadXML Cant_VisitaVis = NomadEnvironment.QueryNomadXML(Control_de_Visitas.Visitas.VISITA.Resources.QRY_VISITANTE_EN_CURSO, param);


                    string cant = Cant_VisitaVis.FirstChild().GetAttr("COUNT");
                    if (cant != "0")
                    {
                        if (status_visita == "~u,")
                            throw new NomadAppException("No se puede guardar la Visita porque existe algun Visitante en la misma, con Fecha-Hora Entrada y sin Fecha-Hora de Salida");
                        else
                            throw new NomadAppException("No se puede guardar la Visita porque el Visitante ya se encuentra registrado con Fecha-Hora Entrada y sin Fecha-Hora de Salida");
                    }
                }
                //valido si no es un update O si es update y ademas se updatea la credencial
                if ((status == "~i,") || (status == "~u," && status_cadena.Contains("d_credencial")))
                {
                    //VALIDAR QUE LA CREDENCIAL NO EXISTA
                    NomadXML Cred_VisitaVis = NomadEnvironment.QueryNomadXML(Control_de_Visitas.Visitas.VISITA.Resources.QRY_CREDS, "");
                    string credencial = VISITAS_VIS.FirstChild().GetAttr("d_credencial");//crendencial que ingreso
                    NomadXML cred = Cred_VisitaVis.FirstChild().FirstChild();
                    if (cred != null)
                    {
                        foreach (NomadXML creds in Cred_VisitaVis.FirstChild().GetChilds())
                        {
                            //string credencial2 = Cred_VisitaVis.FirstChild().FirstChild().GetAttr("d_credencial");//credencial existente
                            string credencial2 = creds.GetAttr("d_credencial");    
                            if (credencial == credencial2)
                            {
                                throw new NomadAppException("No se puede guardar la Visita porque la credencial ya se ha asignado a algun Visitante en estado En curso o Egresado");
                            }
                        }
                    }
                }

                //VALIDAR CANTIDAD MAXIMA DE VISITANTES
                //COMPARO LA CANTIDAD DE VISITANTES MÁX CONTRA LOS QUE INGRESAN + LO QUE YA ESTÁN A LA PLANTA
                int cantvis = 0;
                cantvis = 1 + int.Parse(Cant);
                if (cantvis > int.Parse(Param))
                {
                    throw new NomadAppException("No se puede guardar la Visita porque se ha superado la cantidad maxima de Visitantes simultaneos en planta");
                }
                
                
            }

        }
    }
}


