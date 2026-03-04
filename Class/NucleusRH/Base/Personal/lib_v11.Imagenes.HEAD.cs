using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.Imagenes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Imagenes Personal
    public partial class HEAD
    {
        public static void CopyImg(Nomad.NSystem.Document.NmdXmlDocument PARAM)
        {
            //DEFINO UN OBJETO DEL TIPO ARCHIVO
            Nomad.NSystem.Proxy.BINFile objBin;
            NomadProxy pProxy = Nomad.NSystem.Base.NomadEnvironment.GetProxy();

            //RECORRO CADA UNO DE LOS IDs DE IMAGENES RECUPERADOS
            Nomad.NSystem.Document.NmdXmlDocument imagen;

            for (imagen = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetFirstChildDocument(); imagen != null; imagen = (Nomad.NSystem.Document.NmdXmlDocument)PARAM.GetNextChildDocument())
            {
                //SI LA FILA TIENE CARGADA UNA IMAGEN ENTONCES...
                if (imagen.GetAttribute("oi_imagen") != null)
                {
                    //PIDO EL OBJETO ARCHIVO DE LA IMAGEN Y LA GUARDO CON EL NOMBRE GENERADO				
                    objBin = pProxy.BINService().GetFile("NucleusRH.Base.Personal.Imagenes.HEAD", imagen.GetAttribute("oi_imagen").Value);
                    objBin.SaveFile(pProxy.RunPath + "\\WEB\\TEMPFILES");
                    objBin.SaveFile(pProxy.RunPath + "\\TEMP");
                }
            }
        }
    }
}
