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

namespace NucleusRH.Base.Configuracion.Mails
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Mensajes de Correo
    public partial class MAIL : Nomad.NSystem.Base.NomadObject
    {
        public static void Save(NucleusRH.Base.Configuracion.Mails.MAIL DDO)
        {
            //Cargo la lista de imagenes existentes
            Hashtable imgList = new Hashtable();
            foreach (IMAGEN curIMG in DDO.IMAGENES)
            {
                NomadLog.Debug("HASH-Add-Image:" + curIMG.oi_file);
                imgList[curIMG.oi_file] = curIMG;
            }

            //Texto
            string WIKITxt = DDO.o_text1 + DDO.o_text2 + DDO.o_text3 + DDO.o_text4;

            //Genera los atributos TXT y HTML desde el campo WIKI
            if (WIKITxt != "")
            {
                NomadXML xmlResult = new NomadXML("HELP");
                NomadXML xmlResultWIKI;
                NomadXML xmlImages = new NomadXML("IMAGES");

                xmlResultWIKI = xmlResult.AddTailElement("PAGES").AddTailElement("WIKI");

                //Analizo el WIKI
                Nomad.Base.Report.GeneralReport.REPORT.WIKIEncode("Mail.WIKI", WIKITxt, xmlResultWIKI, xmlImages);

                //Actualizar las IMAGENES
                NomadLog.Debug("xmlImages: " + xmlImages.ToString());
                for (NomadXML cur = xmlImages.FirstChild(); cur != null; cur = cur.Next())
                {
                    cur.SetAttr("id-find", "ID:" + cur.GetAttr("id"));
                    switch (cur.GetAttr("type").ToUpper())
                    {
                        case "DB":
                            //Obtengo el ID                                         
                            string ID = cur.GetAttr("src").Split(':')[1];

                            //Obtengo el Adjunto				
                            IMAGEN MyIMG = (IMAGEN)DDO.IMAGENES.GetByAttribute("oi_file", cur.GetAttr("src").Split(':')[1]);
                            if (MyIMG == null)
                            {
                                //Agrego el Adjunto
                                BINFile MyBIN = NomadProxy.GetProxy().BINService().GetFile("NucleusRH.Base.Configuracion.Archivos.HEAD", ID);
                                MyIMG = new IMAGEN();
                                MyIMG.c_imagen = MyBIN.Name;
                                MyIMG.d_imagen = MyBIN.Name;
                                MyIMG.oi_file = ID;
                                DDO.IMAGENES.Add(MyIMG);

                                NomadLog.Debug("ADD-Image:" + ID);
                            }
                            else
                                if (imgList.ContainsKey(ID))
                                {
                                    NomadLog.Debug("HASH-del-Image:" + ID);
                                    imgList.Remove(ID);
                                }
                                else
                                {
                                    NomadLog.Debug("HASH-not-found-Image:" + ID);
                                }
                            break;

                        default:
                            cur.SetAttr("src-html", cur.GetAttr("src"));
                            break;
                    }
                }
            }

            //Elimino las imagenes que no se usan MAS....
            ArrayList toDelete = new ArrayList();
            foreach (IMAGEN curIMG in imgList.Values)
            {
                NomadLog.Debug("HASH-USE-Image:" + curIMG.oi_file);
                toDelete.Add(NucleusRH.Base.Configuracion.Archivos.HEAD.Get(curIMG.oi_file));
                DDO.IMAGENES.Remove(curIMG);
            }

            //GUARDO los DDOs
            NomadEnvironment.GetCurrentTransaction().Begin();
            NomadEnvironment.GetCurrentTransaction().SaveRefresh(DDO);
            foreach (NucleusRH.Base.Configuracion.Archivos.HEAD curFILE in toDelete)
            {
                NomadLog.Debug("DEL-Image:" + curFILE.id);
                NomadEnvironment.GetCurrentTransaction().Delete(curFILE);
            }
            NomadEnvironment.GetCurrentTransaction().Commit();
        }
    }
}
