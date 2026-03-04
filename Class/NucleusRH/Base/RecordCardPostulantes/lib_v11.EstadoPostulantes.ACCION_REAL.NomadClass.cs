using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.RecordCardPostulantes.EstadoPostulantes
{

    public partial class ACCION_REAL
    {


        public override System.Xml.XmlDocument ObtieneDDORC(string parametrosQry)
        {
            try
            {
                // obtiene los datos para el recordcard
                string xqry = Resources.QRY;
                System.Xml.XmlDocument xmlDDO = NomadEnvironment.QueryXML(xqry, parametrosQry);
                //Nomad.NSystem.Document.NmdXmlDocument nmdXmlDoc = new Nomad.NSystem.Document.NmdXmlDocument(xmlDDO.OuterXml);

                return xmlDDO;
            }
            catch (System.Exception ex)
            {
                NomadEnvironment.GetTrace().Debug("ObtieneDDORC() ERROR: " + ex.Message);
                new NomadAppException("Error en método ObtieneDDORC: " + ex.Message, ex);
                return null;
            }
        }
    }
}












