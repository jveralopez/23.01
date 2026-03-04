using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Personal.ExpImagenes
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Imagenes Personal
    public partial class EXPORTAR
    {
        public static void ExportarImagenes(Nomad.NSystem.Proxy.NomadXML xmlIds)
        {
            NomadBatch B = NomadBatch.GetBatch("Exportacion Imagenes del personal", "Exportacion de imagenes del personal");

            NomadXML xmlFotos = NomadEnvironment.QueryNomadXML(Resources.QRY_FOTOS, xmlIds.ToString());

            xmlFotos = xmlFotos.FirstChild();
            if (xmlFotos.FirstChild() != null)
            {
                NomadProxy pProxy = Nomad.NSystem.Base.NomadEnvironment.GetProxy();

                if (!Directory.Exists(pProxy.RunPath + "ImagenesPersonal"))
                    Directory.CreateDirectory(pProxy.RunPath + "ImagenesPersonal");

                int cantidad = 0;

                foreach (NomadXML foto in xmlFotos.GetChilds())
                {
                    B.SetPro(10, 90, xmlFotos.ChildLength, (cantidad + 1));
                    B.SetMess("Exportando imagen " + (cantidad + 1).ToString() + " de " + xmlFotos.ChildLength + " ...");
                    cantidad++;

                    NucleusRH.Base.Personal.Imagenes.HEAD Img = NucleusRH.Base.Personal.Imagenes.HEAD.Get(foto.GetAttr("oi_foto"));
                    SaveFile(Img, foto.GetAttr("c_tipo_documento"), foto.GetAttr("c_nro_documento"));
                }

                B.Log("Se exportaron '" + cantidad + "' imagenes");
                B.Log("Los archivos se exportaron a la carpeta '" + Path.GetFullPath(NomadProxy.GetProxy().RunPath + "ImagenesPersonal") + "' en el servidor");

            }
            else
            {
                B.Log("no se encontraron imagenes a exportar");
            }

            B.SetPro(100);
            B.SetMess("Fin...");
        }


        private static string SaveFile(NucleusRH.Base.Personal.Imagenes.HEAD imagen,string tipoDNI,string dni)
        {
            System.IO.StreamWriter MyDATA;
            string FileName;
            byte[] myBuffer;
            int pos;

            FileName = NomadProxy.GetProxy().RunPath + @"ImagenesPersonal" + "\\" + tipoDNI + dni +".jpg";

            MyDATA = new System.IO.StreamWriter(FileName);

            pos = 0;
            do
            {
                NucleusRH.Base.Personal.Imagenes.BIN bin = (NucleusRH.Base.Personal.Imagenes.BIN)imagen.BINS[pos];
                myBuffer = Convert.FromBase64String(bin.DATA);
                MyDATA.BaseStream.Write(myBuffer, 0, myBuffer.Length);
                pos++;
            } while (myBuffer.Length == 3000);

            MyDATA.Close();

            return FileName;
        }
    }
}
