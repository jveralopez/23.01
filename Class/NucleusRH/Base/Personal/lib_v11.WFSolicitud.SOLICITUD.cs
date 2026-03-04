using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.WFSolicitud
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Solicitudes
    public partial class SOLICITUD
    {
        public static void IniciarValores(ref Nomad.NSystem.Proxy.NomadXML XMLData)
        {

            NomadXML MySOLICITUD = XMLData.FindElement("WFI").FindElement("DATOS").FindElement("SOLICITUD");


            NomadXML XMLResult = NomadEnvironment.QueryNomadXML(SOLICITUD.Resources.Datos, "<DATOS id=\"" + MySOLICITUD.GetAttr("oi_personal_emp") + "\"/>").FirstChild();
            NomadEnvironment.GetTrace().Info(XMLResult.ToString());


            VALOR newVAL;

            for (NomadXML myCUR = XMLResult.FirstChild(); myCUR != null; myCUR = myCUR.Next())
            {
                //Agrego los Valores
                newVAL = new VALOR();
                newVAL.c_code = myCUR.GetAttr("cod");
                newVAL.d_value = myCUR.GetAttr("val");
                MySOLICITUD.FindElement("VALORES").AddText(newVAL.SerializeAll());
            }

            return;
        }

        public static void EnviarSolicitud(Nomad.NSystem.Proxy.NomadXML XMLData)
        {

            NomadXML MySOLICITUD = XMLData.FindElement("WFI").FindElement("DATOS").FindElement("SOLICITUD");


            NomadXML XMLResult = NomadEnvironment.QueryNomadXML(SOLICITUD.Resources.Datos, "<DATOS id=\"" + MySOLICITUD.GetAttr("oi_personal_emp") + "\"/>").FirstChild();
            NomadEnvironment.GetTrace().Info(XMLResult.ToString());

            //Recorro los Campos y Elimino los que no Cambiaron......
            for (NomadXML myCUR = XMLResult.FirstChild(); myCUR != null; myCUR = myCUR.Next())
            {
                NomadXML findVAL = MySOLICITUD.FindElement("VALORES").FindElement2("VALOR", "c_code", myCUR.GetAttr("cod"));
                if (findVAL != null)
                {
                    if (findVAL.GetAttr("d_value") == myCUR.GetAttr("val"))
                        MySOLICITUD.FindElement("VALORES").DeleteChild(findVAL);
                }
            }

            Nomad.Base.Workflow.WorkflowInstancias.WFI.PassTo(ref XMLData, "PENDAPROB");
            return;
        }

        public static void AprobarSolicitud(Nomad.NSystem.Proxy.NomadXML XMLData, Nomad.NSystem.Proxy.NomadXML XMLList)
        {

            NomadXML MySOLICITUD = XMLData.FindElement("WFI").FindElement("DATOS").FindElement("SOLICITUD");
            NomadXML MyLIST = XMLList.FindElement("SELECT");


            NomadXML XMLResult = NomadEnvironment.QueryNomadXML(SOLICITUD.Resources.Datos, "<DATOS id=\"" + MySOLICITUD.GetAttr("oi_personal_emp") + "\"/>").FirstChild();
            NomadEnvironment.GetTrace().Info(XMLResult.ToString());

            //Obtengo la Persona
            NucleusRH.Base.Personal.Legajo.PERSONAL MyLEG = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(XMLResult.GetAttr("oi_personal"));
            NucleusRH.Base.Personal.Legajo.DOMIC_PER MyDOM = (NucleusRH.Base.Personal.Legajo.DOMIC_PER)MyLEG.DOMIC_PER.GetByAttribute("l_domic_fiscal", true);
            if (MyDOM == null)
            {
                //No Existe Ningun Domicilio FISCAL....
                MyDOM = new NucleusRH.Base.Personal.Legajo.DOMIC_PER();
                MyDOM.l_domic_fiscal = true;
                MyLEG.DOMIC_PER.Add(MyDOM);

                //Busco el Domicilio Recien Agregado
                MyDOM = (NucleusRH.Base.Personal.Legajo.DOMIC_PER)MyLEG.DOMIC_PER.GetByAttribute("l_domic_fiscal", true);
            }


            //Recorro los Campos y Elimino los que no Cambiaron......
            for (NomadXML myIT = MyLIST.FirstChild(); myIT != null; myIT = myIT.Next())
            {
                NomadXML findVAL = MySOLICITUD.FindElement("VALORES").FindElement2("VALOR", "c_code", myIT.GetAttr("id"));

                NomadEnvironment.GetTrace().Info(myIT.GetAttr("id") + "=" + findVAL.GetAttr("d_value"));

                switch (myIT.GetAttr("id"))
                {
                    case "d_nombres": MyLEG.d_nombres = findVAL.GetAttr("d_value"); MyLEG.d_ape_y_nom = MyLEG.d_apellido + ", " + MyLEG.d_nombres; break;
                    case "d_apellido": MyLEG.d_apellido = findVAL.GetAttr("d_value"); MyLEG.d_ape_y_nom = MyLEG.d_apellido + ", " + MyLEG.d_nombres; break;
                    case "d_apellido_materno": MyLEG.d_apellido_materno = findVAL.GetAttr("d_value"); break;
                    case "c_nro_documento": MyLEG.c_nro_documento = findVAL.GetAttr("d_value"); break;
                    case "c_nro_cuil": MyLEG.c_nro_cuil = findVAL.GetAttr("d_value"); break;
                    case "c_sexo": MyLEG.c_sexo = findVAL.GetAttr("d_value"); break;
                    case "oi_grupo_sanguineo": MyLEG.oi_grupo_sanguineo = findVAL.GetAttr("d_value"); break;
                    case "d_email": MyLEG.d_email = findVAL.GetAttr("d_value"); break;
                    case "te_celular": MyLEG.te_celular = findVAL.GetAttr("d_value"); break;
                    case "f_nacim": MyLEG.f_nacim = findVAL.GetAttrDateTime("d_value"); break;
                    case "oi_local_nacim": MyLEG.oi_local_nacim = findVAL.GetAttr("d_value"); break;
                    case "oi_nacionalidad": MyLEG.oi_nacionalidad = findVAL.GetAttr("d_value"); break;

                    case "oi_tipo_domicilio": MyDOM.oi_tipo_domicilio = findVAL.GetAttr("d_value"); break;
                    case "c_postal": MyDOM.c_postal = findVAL.GetAttr("d_value"); break;
                    case "d_calle": MyDOM.d_calle = findVAL.GetAttr("d_value"); break;
                    case "e_numero": MyDOM.e_numero = findVAL.GetAttrInt("d_value"); break;
                    case "d_piso": MyDOM.d_piso = findVAL.GetAttr("d_value"); break;
                    case "d_departamento": MyDOM.d_departamento = findVAL.GetAttr("d_value"); break;
                    case "d_entre_calle_1": MyDOM.d_entre_calle_1 = findVAL.GetAttr("d_value"); break;
                    case "d_entre_calle_2": MyDOM.d_entre_calle_2 = findVAL.GetAttr("d_value"); break;
                    case "te_1": MyDOM.te_1 = findVAL.GetAttr("d_value"); break;
                    case "d_partido": MyDOM.d_partido = findVAL.GetAttr("d_value"); break;
                    case "oi_localidad": MyDOM.oi_localidad = findVAL.GetAttr("d_value"); break;
                }

            }
            NomadEnvironment.GetCurrentTransaction().Save(MyLEG);

            Nomad.Base.Workflow.WorkflowInstancias.WFI.PassTo(ref XMLData, "APROBADA");
            return;
        }
    }
}
