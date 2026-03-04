using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.RecordCard.Acciones
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Acciones
    public partial class ACCION 
    {
        public static string getIDbyName(string c_accion)
        {

            try
            {
                string xqry = NucleusRH.Base.RecordCard.Acciones.ACCION.Resources.QRY_getIDbyName;
                System.Xml.XmlDocument x_oi_accion = NomadEnvironment.QueryXML(xqry, "<param c_accion=\"" + c_accion + "\"/>");
                return ((System.Xml.XmlAttribute)x_oi_accion.SelectSingleNode("/rows/row/@oi_accion")).Value;
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Error en método getIDbyName. " + ex.Message, ex);
            }

        }

    }
}
