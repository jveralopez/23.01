using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Organizacion.Convenios
{
    
    //////////////////////////////////////////////////////////////////////////////////
    //Clase Categorías de Convenios
    public partial class CATEGORIA
    {
        public void Actualizar_Costos()
        {
            // CREO DOCUMENTO DE PARAMETROS PARA EL QUERY
            string auxID = this.Id;
            string PARAM = "<DATOS oi_categoria=\"" + auxID + "\"/>";
            NmdXmlDocument paramID = new NmdXmlDocument(PARAM);

            // EJECUTO EL QUERY, GUARDO EL RESULTADO EN "PUESTOSCOL"
            Nomad.NSystem.Document.NmdXmlDocument puestosCol = null;
            puestosCol = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Organizacion.Convenios.CATEGORIA.Resources.QRY_PUESTOS, paramID.DocumentToString()));

            // PARA CADA PUESTO OBTENIDO,	
            Nomad.NSystem.Document.NmdXmlDocument puestos = (Nomad.NSystem.Document.NmdXmlDocument)puestosCol.GetDocumentByName("PUESTOS");
            Nomad.NSystem.Document.NmdXmlDocument puesto;
            for (puesto = (Nomad.NSystem.Document.NmdXmlDocument)puestos.GetFirstChildDocument(); puesto != null; puesto = (Nomad.NSystem.Document.NmdXmlDocument)puestos.GetNextChildDocument())
            {
                // OBTENGO EL OI_PUESTO
                string oi_puesto = puesto.GetAttribute("oi_puesto").Value;
                // OBTENGO EL DDO PUESTO
                NucleusRH.Base.Organizacion.Puestos.PUESTO ddoPuesto = NucleusRH.Base.Organizacion.Puestos.PUESTO.Get(oi_puesto);
                // EJECUTO EL METODO COSTO_PUESTO		
                try
                {
                    ddoPuesto.Costo_Puesto(this, null);
                }
                catch (Exception) { }
            }
        }
    }
}
