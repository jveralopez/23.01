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
    //Clase Clases involucradas en las Acciones
    public partial class CLASE_ACC 
    {
        public static string getIDbyClass(string oi_accion, string d_clase)
        {

            try
            {
                string xqry = NucleusRH.Base.RecordCard.Acciones.CLASE_ACC.Resources.QRY_getIDbyClass;
                System.Xml.XmlDocument x_oi_clase_acc = NomadEnvironment.QueryXML(xqry, "<param oi_accion=\"" + oi_accion + "\" d_clase=\"" + d_clase + "\"/>");
                return ((System.Xml.XmlAttribute)x_oi_clase_acc.SelectSingleNode("/rows/row/@oi_clase_acc")).Value;
            }
            catch (Exception ex)
            {
                throw new NomadAppException("Error en método getIDbyClass. " + ex.Message, ex);
            }

        }

    }
}
