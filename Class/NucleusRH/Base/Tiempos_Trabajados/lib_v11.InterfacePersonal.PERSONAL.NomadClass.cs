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

namespace NucleusRH.Base.Tiempos_Trabajados.InterfacePersonal
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase FichadasIngresadas
    public partial class PERSONAL : NucleusRH.Base.Personal.InterfacePersonal.PERSONAL
    {

        public static string AddLegajoTTA(string oi_personal_emp, string NUMERO_LEGAJO, string NUMERO_TARGETA)
        {
            string legajoFound;
            NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP objLegajo;

            legajoFound = FindOI("TTA04_PERSONAL", "oi_personal_emp", "d_nro_tarjeta", NUMERO_TARGETA);
            if (legajoFound != "")
            {
                TraceWrn("La tarjeta '" + NUMERO_TARGETA + "' ya esta asignada....");
                return legajoFound;
            }

            legajoFound = FindOI("TTA04_PERSONAL", "oi_personal_emp", "e_nro_legajo_reloj", NUMERO_LEGAJO);
            if (legajoFound != "")
            {
                TraceWrn("El legajo reloj '" + NUMERO_LEGAJO + "' ya esta asignado....");
                return legajoFound;
            }

            objLegajo = NucleusRH.Base.Tiempos_Trabajados.Personal.PERSONAL_EMP.Get(oi_personal_emp);
            objLegajo.e_nro_legajo_reloj = int.Parse(NUMERO_LEGAJO);
            objLegajo.d_nro_tarjeta = NUMERO_TARGETA;

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(objLegajo);
            return objLegajo.Id;
        }

        public static void ImportarLegajosTTA(int oi_empresa)
        {
            int Linea, Errores = 0;
            NomadXML IDCur;
            NomadXML IDList = new NomadXML();
            NucleusRH.Base.Tiempos_Trabajados.InterfacePersonal.PERSONAL objRead;
            string PersonalOI, LegajoOI, LegajoTTAOI;

            TraceInfo("Comienza la Importacion de Legajos...");
            IDList.SetText(NomadProxy.GetProxy().SQLService().Get(PERSONAL.Resources.qry_legajos, ""));

            for (Linea = 1, IDCur = IDList.FindElement("ROWS").FirstChild(); IDCur != null; IDCur = IDCur.Next(), Linea++)
            {
                objRead = NucleusRH.Base.Tiempos_Trabajados.InterfacePersonal.PERSONAL.Get(IDCur.GetAttr("id"));

                //Inicio la Transaccion
                try
                {
                    //Creo la Persona
                    PersonalOI = AddPersona(objRead.APELLIDO, objRead.NOMBRES, objRead.SEXO, objRead.DOC_TIPO, objRead.DOC_NUMERO);
                    if (PersonalOI == "")
                    {
                        TraceError("No se pudo agregar la Persona '" + objRead.APELLIDO + ", " + objRead.NOMBRES + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Legajo
                    LegajoOI = AddLegajo(oi_empresa, PersonalOI, objRead.LEGAJO, objRead.F_INGRESO, objRead.TP_COD, objRead.CAL_COD, objRead.CC_COD);
                    if (LegajoOI == "")
                    {
                        TraceError("No se pudo agregar el Legajo para la Persona '" + objRead.APELLIDO + ", " + objRead.NOMBRES + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                    //Creo el Legajo TTA
                    LegajoTTAOI = AddLegajoTTA(LegajoOI, objRead.LEGAJO_RELOJ, objRead.TARGETA);
                    if (LegajoTTAOI == "")
                    {
                        TraceError("No se pudo agregar el Legajo para la Persona '" + objRead.APELLIDO + ", " + objRead.NOMBRES + "' - Linea: " + Linea.ToString());
                        Errores++;
                        continue;
                    }

                }
                catch (Exception e)
                {
                    TraceError("Error desconocido. " + e.Message + " - Linea " + Linea.ToString());
                    Errores++;
                }
                if (Linea % 10 == 0) TraceInfo("Registros Procesados:" + Linea.ToString() + " - Importados:" + (Linea - Errores).ToString());

            }
            Linea--;
            TraceInfo("Registros Procesados:" + Linea.ToString() + " - Importados:" + (Linea - Errores).ToString());
            TraceInfo("Finalizado...");

        }
    }
}


