using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Gestion_de_Postulantes.Examenes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Examenes
    public partial class EXAMEN 
    {
        public void Guardar_Examen()
        {

            string param = "";
            if (this.IsForInsert)
            {
                param = "<DATOS oi_examen=\"\" e_orden=\"" + this.e_orden + "\"/>";

                // Ejecuto el recurso

                Nomad.NSystem.Document.NmdXmlDocument band = null;
                band = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(EXAMEN.Resources.QRY_ORDEN, param));

                if (band.GetAttribute("cont").Value != "0")
                {
                    throw new NomadAppException("El numero de orden especificado ya ha sido asigando a otro examen");
                }
                if (band.GetAttribute("cont").Value == "0")
                {
                    NomadEnvironment.GetCurrentTransaction().Save(this);
                }

            }
            else
            {
                if (this.IsForUpdate)
                {
                    param = "<DATOS oi_examen=\"" + this.Id + "\" e_orden=\"" + this.e_orden + "\"/>";

                    // Ejecuto el recurso

                    Nomad.NSystem.Document.NmdXmlDocument band = null;
                    band = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(EXAMEN.Resources.QRY_ORDEN, param));

                    if (band.GetAttribute("cont").Value != "0")
                    {
                        throw new NomadAppException("El numero de orden especificado ya ha sido asigando a otro examen");
                    }
                    if (band.GetAttribute("cont").Value == "0")
                    {
                        NomadEnvironment.GetCurrentTransaction().Save(this);
                    }
                }
                else
                {
                    NomadEnvironment.GetCurrentTransaction().Save(this);
                }
            }

        }

    }
}
