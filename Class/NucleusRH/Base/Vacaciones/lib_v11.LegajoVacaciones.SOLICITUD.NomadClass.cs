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
using System.Collections.Generic;

namespace NucleusRH.Base.Vacaciones.LegajoVacaciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Solicitud Vacaciones
    public partial class SOLICITUD : Nomad.NSystem.Base.NomadObject
    {
        public static void AprobarSolic_Selec(Nomad.NSystem.Proxy.NomadXML xmlListaDoc, bool force)
        {
            
            NomadLog.Debug("SOLICITUD.AprobarSolic_Selec force: " + force);
            NomadLog.Debug("SOLICITUD.AprobarSolic_Selec xmlListaDoc: " + xmlListaDoc.ToString());

            NomadBatch objBatch = NomadBatch.GetBatch("Aprobar Solicitudes", "Aprobar Solicitudes");
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per;
            NucleusRH.Base.Vacaciones.LegajoVacaciones.SOLICITUD obj_sol;
            List<SOLICITUD> lista_soli_can = new List<SOLICITUD>();
            int max, cnt=0;
            NomadXML Sel, Cur;

                 
            //Elemento RAIZ
            xmlListaDoc = xmlListaDoc.FirstChild();

            NomadXML SELECT = xmlListaDoc.FindElement("SELECT");
            NomadXML ROWS = xmlListaDoc.FindElement("ROWS");

            //La cantidad maxima de elementos seleccionados desde la pantalla que llegan como parametro en el elemento Select del XML xmlListaDoc 
            max = SELECT.ChildLength;

            //Recorro la Lista de Legajos/Solicitud a Aprobar para crear una nueva lista ordenada por la fecha desde de la solicitud
            for (Sel = SELECT.FirstChild(); Sel != null; Sel = Sel.Next())
            {
                Cur = ROWS.FindElement2("ROW", "id", Sel.GetAttr("id"));
                obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(Cur.GetAttr("oi_personal_emp"));

                //Encuentro el objeto hijo de solicitud, dentro de la coleccion de solicitudes del personal empresa por id de solicitud
                obj_sol = (SOLICITUD)obj_Per.SOLICITUDES.GetById(Cur.GetAttr("id"));
                
                //agrego a la lista generica el objeto de solicitud
                lista_soli_can.Add(obj_sol);
            }

            
            //Ordeno por fecha desde las solicitudes canceladas
            lista_soli_can.Sort(new Comparison<SOLICITUD>(delegate(SOLICITUD a, SOLICITUD b) { return DateTime.Compare((DateTime)a.f_desde_solicitud, (DateTime)b.f_desde_solicitud); }));


            //Recorro la Lista de Legajos/Solicitud a Aprobar de la lista ordenada
            foreach (SOLICITUD item in lista_soli_can)
            {
                cnt++;
                Sel = SELECT.FindElement2("ROW", "id",item.Id);
                Cur = ROWS.FindElement2("ROW", "id", Sel.GetAttr("id"));

                objBatch.SetPro(0, 90, max, cnt);
                objBatch.SetMess("Aprobando legajos (" + cnt.ToString() + "/" + max.ToString() + ")");
                obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(Cur.GetAttr("oi_personal_emp"));

                try
                {
                    obj_Per.Aprobar_Solicitud(Cur.GetAttr("id"), force);
                    ROWS.DeleteChild(Cur);
                    SELECT.DeleteChild(Sel);
                    
                }
                catch (Exception E)
                {
                    objBatch.Err(Cur.GetAttr("label") + "-" + E.ToString());
                    Cur.SetAttr("MSG", E.Message);
                }          
            }

            //Guardo el Resultado.	
            NomadEnvironment.GetProxy().StoreService().SetUserStore("RESULT", xmlListaDoc.ToString());
            objBatch.SetPro(100);
        }
        
        public static void Liquidar_Selec(Nomad.NSystem.Proxy.NomadXML xmlListaDoc)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Generando Licencias", "Generando Licencias");
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per;
            int max, cnt;
            NomadXML Sel, Cur;
            ArrayList MyDelete = new ArrayList();

            //Elemento RAIZ
            xmlListaDoc = xmlListaDoc.FirstChild();

            NomadXML SELECT = xmlListaDoc.FindElement("SELECT");
            NomadXML ROWS = xmlListaDoc.FindElement("ROWS");


            //Recorro la Lista de Legajos/Solicitud a Aprobar
            max = SELECT.ChildLength;
            for (Sel = SELECT.FirstChild(), cnt = 1; Sel != null; Sel = Sel.Next(), cnt++)
            {
                Cur = ROWS.FindElement2("ROW", "id", Sel.GetAttr("id"));

                objBatch.SetPro(0, 90, max, cnt);
                objBatch.SetMess("Generando Licencias (" + cnt.ToString() + "/" + max.ToString() + ")");
                obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(Cur.GetAttr("oi_personal_emp"));

                try
                {
                    obj_Per.Liquidar_Solicitud(Cur.GetAttr("id"));
                    ROWS.DeleteChild(Cur);
                    MyDelete.Add(Sel);
                }
                catch (Exception E)
                {
                    objBatch.Err(Cur.GetAttr("label") + "-" + E.ToString());
                    Cur.SetAttr("MSG", E.Message);
                }
            }

            //Eliminar los Elementos del XML
            foreach (NomadXML toDel in MyDelete)
                SELECT.DeleteChild(toDel);

            //Guardo el Resultado.	
            NomadEnvironment.GetProxy().StoreService().SetUserStore("RESULT", xmlListaDoc.ToString());
            objBatch.SetPro(100);
        }
        
        public static void Rechazar_Solic(Nomad.NSystem.Proxy.NomadXML xmlListaDoc)
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Rechazar Solicitudes", "Rechazar Solicitudes");
            NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP obj_Per;
            int max, cnt;
            NomadXML Sel, Cur;
            ArrayList MyDelete = new ArrayList();

            //Elemento RAIZ
            xmlListaDoc = xmlListaDoc.FirstChild();

            NomadXML SELECT = xmlListaDoc.FindElement("SELECT");
            NomadXML ROWS = xmlListaDoc.FindElement("ROWS");


            //Recorro la Lista de Legajos/Solicitud a Aprobar
            max = SELECT.ChildLength;
            for (Sel = SELECT.FirstChild(), cnt = 1; Sel != null; Sel = Sel.Next(), cnt++)
            {
                Cur = ROWS.FindElement2("ROW", "id", Sel.GetAttr("id"));

                objBatch.SetPro(0, 90, max, cnt);
                objBatch.SetMess("Rechazando legajos (" + cnt.ToString() + "/" + max.ToString() + ")");
                obj_Per = NucleusRH.Base.Vacaciones.LegajoVacaciones.PERSONAL_EMP.Get(Cur.GetAttr("oi_personal_emp"));

                try
                {
                    obj_Per.Rechazar_Solicitud(Cur.GetAttr("id"));
                    ROWS.DeleteChild(Cur);
                    MyDelete.Add(Sel);
                }
                catch (Exception E)
                {
                    objBatch.Err(Cur.GetAttr("label") + "-" + E.ToString());
                    Cur.SetAttr("MSG", E.Message);
                }
            }

            //Eliminar los Elementos del XML
            foreach (NomadXML toDel in MyDelete)
                SELECT.DeleteChild(toDel);

            //Guardo el Resultado.	
            NomadEnvironment.GetProxy().StoreService().SetUserStore("RESULT", xmlListaDoc.ToString());
            objBatch.SetPro(100);
        }
    }
}
