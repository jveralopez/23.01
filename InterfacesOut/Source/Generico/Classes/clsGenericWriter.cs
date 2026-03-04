using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using Nomad.NSystem.Proxy;

namespace Generico.Classes {

    class clsGenericWriter {
        
        private clsFileTemplate _objTemplate;
        private clsFileParams _objParams;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pstrFileTemplate">Archivo de template para la interfaz.</param>
        /// <param name="pstrFileParams">Archivo de parámetros para ejecutar junto con el template.</param>
        public clsGenericWriter(string pstrFileTemplate, string pstrFileParams) {
            this._objTemplate = new clsFileTemplate(pstrFileTemplate);
            this._objParams = new clsFileParams(pstrFileParams);
        }
        
        /// <summary>
        /// Interpreta el template y lo envia al server con el proxy
        /// </summary>
        public void Save() {
            string strQResult;
            string strRow = "";
            string strFileName;
            string strBatchID = NomadProxy.GetProxy().Batch().ID == "" ? "SinBatchID" : NomadProxy.GetProxy().Batch().ID;

            XmlTextReader xtrResult;
            StreamWriter swrWriter;
            FileStream objFile;
            StringBuilder stbHead = new StringBuilder();
            StringBuilder stbDetail = new StringBuilder();
            StringBuilder stbFoot = new StringBuilder();
            
            strFileName = System.Reflection.Assembly.GetExecutingAssembly().GetFiles()[0].Name;
            strFileName = strFileName.Substring(0, strFileName.LastIndexOf('\\'));
            strFileName = strFileName.Substring(0, strFileName.LastIndexOf('\\'));
            strFileName = strFileName + "\\TEMP\\" + "file-" + strBatchID + ".txt";
            objFile = new FileStream(strFileName, FileMode.Create);
            
            //--------------------------------------------------------------------------------
            //Ejecuta el query
            strQResult = this.ExecuteQuery(this._objParams.ToString());
            
            //--------------------------------------------------------------------------------
            //Recorre el resultado del query
            xtrResult = new XmlTextReader(strQResult, System.Xml.XmlNodeType.Document, null);
            xtrResult.XmlResolver = null; // ignore the DTD
            xtrResult.WhitespaceHandling = WhitespaceHandling.None;

            xtrResult.Read();
			if (xtrResult.Name != "DATA")
			    throw new Exception ("El resultado del XQuery no concuerda con el formato. Se esperaba <DATA>.");
            
            xtrResult.Read();

            //Recorre los registros y los va eliminando
            while (xtrResult.Name != "DATA") {
                if (xtrResult.IsStartElement()) {
                    switch (xtrResult.Name.ToUpper()) {
                        case "HEAD":
                            strRow = clsFunctions.ApplyTemplate(this._objTemplate.GetHead(), xtrResult, "CABECERA");
                            if (stbHead.Length > 0) stbHead.Append(this._objTemplate.GetEnter());
                            stbHead.Append(strRow);
                            break;

                        case "DETAIL":
                            strRow = clsFunctions.ApplyTemplate(this._objTemplate.GetDetail(), xtrResult, "DETALLE");
                            if (stbDetail.Length > 0) stbDetail.Append(this._objTemplate.GetEnter());
                            stbDetail.Append(strRow);
                            break;


                        case "FOOT":
                            strRow = clsFunctions.ApplyTemplate(this._objTemplate.GetFoot(), xtrResult, "PIE");
                            if (stbFoot.Length > 0) stbFoot.Append(this._objTemplate.GetEnter());
                            stbFoot.Append(strRow);
                            break;
                    }

                }
                xtrResult.Read();
            }
            
            xtrResult.Close();
            
            if (stbHead.Length == 0 && stbDetail.Length == 0 && stbFoot.Length == 0) {
                Console.WriteLine("3:" + this._objTemplate.GetMess_Empty());
            } else {

                //Graba los datos en el archivo temporal
                string strEnter = ""; 
                swrWriter = new System.IO.StreamWriter(objFile, Encoding.ASCII);

                //HEAD
                if (stbHead.Length != 0) {
                    swrWriter.Write(strEnter);
                    swrWriter.Write(stbHead.ToString());
                    strEnter = this._objTemplate.GetEnter();
                }
                
                //DETAIL
                if (stbDetail.Length != 0) {
                    swrWriter.Write(strEnter);
                    swrWriter.Write(stbDetail.ToString());
                    strEnter = this._objTemplate.GetEnter();
                }

                //FOOT
                if (stbFoot.Length != 0) {
                    swrWriter.Write(strEnter);
                    swrWriter.Write(stbFoot.ToString());
                    strEnter = this._objTemplate.GetEnter();
                }
                
                //FINAL ENTER
                if (this._objTemplate.FinalEnter())
                    swrWriter.Write(this._objTemplate.GetEnter());
                
                swrWriter.Close();
                
                //--------------------------------------------------------------------------------
                //Envia el resultado al server
                NomadProxy.GetProxy().FileServiceIO().SaveBinFile("INTERFACES", strBatchID + ".txt", objFile.Name);
            }
                
            
        }
        
        /*-----------------------------------------------------------------------------------*/
        /*-- FUNCIONES PRIVADAS                                                            --*/
        /*-----------------------------------------------------------------------------------*/
        
        /// <summary>
        /// Ejecuta el query de la interface y le pasa como parámetros el documento obtenido desde el Store
        /// </summary>
        /// <param name="pstrFileParams"></param>
        /// <returns></returns>
        private string ExecuteQuery(string pstrFileParams) {
            string strResult = "";
            
            try {
                //Ejecuta el query
                strResult = NomadProxy.GetProxy().SQLService().Get(this._objTemplate.GetQuery(), pstrFileParams);

            } catch (Exception ex) {
                throw new Exception ("Se produjo un error al intentar ejecutar el query principal. " + ex.Message);
            }
            
            return strResult;
        }
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    class clsFileParams {
        private string _strFileParams;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pstrFileParams"></param>
        public clsFileParams (string pstrFileParams) {
            this._strFileParams = pstrFileParams;
        }
        
        /// <summary>
        /// Retorna el string completo
        /// </summary>
        public override string ToString() {
            return this._strFileParams;
        }
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    class clsFileTemplate {
        
        private XmlDocument _xmlFile;
        private string _strEnter = "";
        private string _strEmpty = "";
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pstrFileTemplate">Archivo a interpretar</param>
        public clsFileTemplate(string pstrFileTemplate) {
            this._xmlFile = new XmlDocument();
            
            try {
                this._xmlFile.LoadXml(pstrFileTemplate);
            } catch {
                throw new Exception("El formato del archivo template no tiene formato XML.");
            }
        }
        
        
        /// <summary>
        /// Retorna el modo del enter
        /// </summary>
        /// <returns></returns>
        public bool FinalEnter() {
            return this._xmlFile.DocumentElement.GetAttribute("finalenter") == "1";
        }

        /// <summary>
        /// Retorna el modo del enter
        /// </summary>
        /// <returns></returns>
        public string GetEnterMode() {
            return this._xmlFile.DocumentElement.GetAttribute("entermode");
        }

        /// <summary>
        /// Retorna el Enter para el tipo de enter indicado
        /// </summary>
        /// <returns></returns>
        public string GetEnter() {
            if (this._strEnter == "") {
                switch (this.GetEnterMode().ToUpper()) {
                    case "UNIX":
                        this._strEnter = "\n";
                        break;
                        
                    case "DOS":
                    default:
                        this._strEnter = "\r\n";
                        break;
                    
                }
            }
                
            return this._strEnter;
        }

        /// <summary>
        /// Retorna el string del query que está dentro del archivo de template.
        /// </summary>
        /// <returns></returns>
        public string GetQuery() {
            return this.GetElementContent("QUERY", false);
        }

        /// <summary>
        /// Retorna el string del HEAD que está dentro del archivo de template.
        /// </summary>
        /// <returns></returns>
        public string GetHead() {
            return this.GetElementContent("HEAD");
        }

        /// <summary>
        /// Retorna el string del DETAIL que está dentro del archivo de template.
        /// </summary>
        /// <returns></returns>
        public string GetDetail() {
            return this.GetElementContent("DETAIL");
        }

        /// <summary>
        /// Retorna el string del FOOT que está dentro del archivo de template.
        /// </summary>
        /// <returns></returns>
        public string GetFoot() {
            return this.GetElementContent("FOOT");
        }

        /// <summary>
        /// Retorna el string del de un elemento que esté dentro del archivo de template.
        /// </summary>
        /// <returns></returns>
        public string GetElementContent(string pstrElementName) {
            return this.GetElementContent(pstrElementName, true);
        }

        /// <summary>
        /// Retorna el string del de un elemento que esté dentro del archivo de template.
        /// </summary>
        /// <returns></returns>
        public string GetElementContent(string pstrElementName, bool pIsText) {
            try {
                if (pIsText)
                    return this._xmlFile.DocumentElement.GetElementsByTagName(pstrElementName)[0].InnerText;
                else
                    return this._xmlFile.DocumentElement.GetElementsByTagName(pstrElementName)[0].InnerXml;
            } catch {
                throw new Exception("El formato del archivo template con concuerda con el esperado. Se esperaba el tag <" + pstrElementName + ">.");
            }
        }

        /// <summary>
        /// Retorna el mensaje EMPTY
        /// </summary>
        /// <returns></returns>
        public string GetMess_Empty() {
            if (this._strEmpty == "") {
                this._strEmpty = GetMessValue("EMPTY");
                
                if (this._strEmpty == "") 
                    this._strEmpty = "No se encontraron registros para el filtro seleccionado.";
            }
            
            return this._strEmpty;
            
        } 
        
        /// <summary>
        /// Retorna el contenido del mensaje para un tag en particular
        /// </summary>
        /// <param name="pstrMessName">Nombre del tag de mensaje</param>
        /// <returns></returns>
        private string GetMessValue(string pstrMessName) {
            string strResult = "";
            
            XmlNodeList xnlTemp = this._xmlFile.DocumentElement.GetElementsByTagName("MESS");
            
            if (xnlTemp.Count > 0) {
                XmlElement xelMess = (XmlElement) xnlTemp[0];
                
                xnlTemp = xelMess.GetElementsByTagName(pstrMessName);
                
                if (xnlTemp.Count > 0) {
                    xelMess = (XmlElement) xnlTemp[0];
                    strResult = xelMess.GetAttribute("mess");
                }
            }
            
            return strResult;
        }
    }
    
}
