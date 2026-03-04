using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Control_de_Visitas.Visitantes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Visitantes
    public partial class VISITANTE 
    {
        public void GuardarVisitante()
        {

            //TENGO EL VISITANTE QUE ESTOY CREANDO
            NomadEnvironment.GetTrace().Info("VISITANTE -- " + this.SerializeAll());

            //TENGO QUE RECUPERAR LA DESCRIPCION DEL TIPO DE DOCUMENTO. LEVANTO EL DDO DE TIPO DE DOCUMENTO
            NucleusRH.Base.Personal.Tipos_Documento.TIPO_DOCUMENTO ddoTipoDoc;
            ddoTipoDoc = this.Getoi_tipo_documento();
            NomadEnvironment.GetTrace().Info("TIPO DOC -- " + ddoTipoDoc.SerializeAll());

            //ARMO EL CODIGO DEL VISTANTE EN BASE A LA DESCRIPICION DEL TIPO DE DOCUMENTO Y EL NRO DE DOCUMENTO
            this.c_visitante = ddoTipoDoc.c_tipo_documento.ToString() + this.c_nro_documento.ToString();

            //GUARDO EL VISITANTE
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(this);

        }
        public static void CopyImg(string visitaID)
        {

            //TIRO UN QRY PARA TRAER LOS ID DE LAS IMAGENES DE TODOS LOS VISITANTES EN LA VISITA, LAS OBSERVACIONES DE LAS MISMAS
            //Y LOS IDS DE LOS VISITANTES
            string param = "<DATO oi_visita=\"" + visitaID + "\"/>";
            Nomad.NSystem.Document.NmdXmlDocument docIDs = null;
            docIDs = new Nomad.NSystem.Document.NmdXmlDocument(NomadEnvironment.QueryString(NucleusRH.Base.Control_de_Visitas.Visitantes.VISITANTE.Resources.QRY_IMGS, param));

            NomadEnvironment.GetTrace().Info("docIDS -- " + docIDs.ToString());

            //DEFINO UN OBJETO DEL TIPO ARCHIVO
            Nomad.NSystem.Proxy.BINFile objBin;
            NomadProxy pProxy = Nomad.NSystem.Base.NomadEnvironment.GetProxy();

            //RECORRO CADA UNO DE LOS IDs DE IMAGENES RECUPERADOS
            Nomad.NSystem.Document.NmdXmlDocument id;
            for (id = (Nomad.NSystem.Document.NmdXmlDocument)docIDs.GetFirstChildDocument(); id != null; id = (Nomad.NSystem.Document.NmdXmlDocument)docIDs.GetNextChildDocument())
            {
                //SI EL VISITANTE TIENE CARGADA UNA IMAGEN ENTONCES...
                if (id.GetAttribute("oi_foto") != null)
                {
                    //GENERO UN STRING PARA FORMAR LA PRIMER PARTE DEL NOMBRE DEL ARCHIVO
                    string name_img = "imgVisitante_";

                    name_img = name_img + id.GetAttribute("oi_visitante").Value + "_" + id.GetAttribute("o_imagen").Value;
                    NomadEnvironment.GetTrace().Info("NombreImg -- " + name_img);

                    //PIDO EL OBJETO ARCHIVO DE LA IMAGEN Y LA GUARDO CON EL NOMBRE GENERADO				
                    objBin = pProxy.BINService().GetFile("NucleusRH.Base.Personal.Imagenes.HEAD", id.GetAttribute("oi_foto").Value);
                    NomadEnvironment.GetTrace().Info("1 -- " + name_img);
                    objBin.SaveFile(pProxy.RunPath + "\\TEMP");
                }
            }

        }
        public void LoadPhoto()
        {

            //BUSCO AL ARCHIVO QUE GENERA EL USUARIO CUANDO UTILIZA LA CAMARA DE FOTOS
            string fPath = Nomad.NSystem.Proxy.NomadProxy.GetProxy().RunPath + "TEMP\\";
            string uName = Nomad.NSystem.Proxy.NomadProxy.GetProxy().UserName;
            string fName = fPath + "Foto_" + uName + ".jpg";

            NomadEnvironment.GetTrace().Info("DATOS -- " + fPath + " - " + uName + " - " + fName);

            //PIDO EL STREAM DEL ARCHIVO PARA PASARSELO AL PUTFILE   
            try
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(fName);
                //GUARDO EL OBJETO IMAGEN EN LA BASE DE DATOS Y LE PEGO SU ID AL CAMPO OI_FOTO DEL VISITANTE
                this.oi_foto = Nomad.NSystem.Proxy.NomadProxy.GetProxy().BINService().PutFile("NucleusRH.Base.Personal.Imagenes.HEAD", "Foto_user.jpg", sr.BaseStream).Id;
                sr.Close();
                System.IO.File.Delete(fName);
            }
            catch
            {
                throw new NomadAppException("No hay ninguna imagen disponible.");
            }

        }

    }
}
