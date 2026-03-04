using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;
using System.IO;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Personal.Legajo;
using NucleusRH.Base.Personal.Imagenes;

namespace NucleusRH.Base.Personal_Fotos {

    class clsImportarFotos {

    	private Hashtable htaOIs = null;
    	private string strQuery;
    	private string strPath;
    	private long lngSize = 0;
    	private Hashtable htaExtensions;
    	private string strClass;

        /// <summary>
        /// Constructor
        /// </summary>
        public clsImportarFotos(string pstrQuery, string pstrPath, long plngSize, ArrayList parrExtensions, string pstrClass) {
			this.strQuery      = pstrQuery;
			this.strPath       = pstrPath;
			this.lngSize       = plngSize;
			this.strClass      = pstrClass;
			
			this.htaExtensions = new Hashtable();
			
			//Incorpora las extensiones
			for (int x = 0; x < parrExtensions.Count; x++)
				this.htaExtensions.Add(parrExtensions[x], parrExtensions[x]);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public clsImportarFotos(string pstrQuery, string pstrPath, ArrayList parrExtensions, string pstrClass) : this(pstrQuery, pstrPath, 0, parrExtensions, pstrClass) {
        }

        /// <summary>
        /// Importa las imagenes a la Persona
        /// </summary>
        public void Importar(bool pbolReplace) {
			
			DirectoryInfo dirPath;
			FileInfo[] arrFileInfo;
			FileData fdFile;
            NomadBatch objBatch = NomadBatch.GetBatch("Importación", "Importación");
            NomadXML xmlPersonal;
            int intFilesOk = 0;
            
            PERSONAL objPersonal = null;
			HEAD objImagen = null;

			NomadBatch.Trace("--------------------------------------------------------------------------");
			NomadBatch.Trace(" Comienza Importar Fotos del Personal ------------------------------------");
			NomadBatch.Trace("--------------------------------------------------------------------------");
			
			objBatch.SetPro(0);
			objBatch.SetMess("Importando los Fotos del Personal.");
			objBatch.Log("Comienza la importación");

			NomadBatch.Trace("Importando imagenes desde '" + this.strPath + "'");

			//Valida que el directorio pasado existe
			try {
				dirPath = new DirectoryInfo(this.strPath);
				arrFileInfo = dirPath.GetFiles();
			} catch (Exception) {
				objBatch.Err("El path '"+ this.strPath + "' no existe o no se tiene permiso de lectura");
				return;
			}


			//Recorre los archivos del directorio
			objBatch.SetPro(5);
			int x = 0;
			foreach (FileInfo fiFile in arrFileInfo) {

				x++;
				objBatch.SetMess("Importando el archivo  '" + fiFile.Name + "'.");
				objBatch.SetPro(5, 80, arrFileInfo.Length, x);
				
				//Obtiene la información del archivo
				try {
					fdFile = GetParts(fiFile.Name);
					
					//Verifica que tenga una extension correcta
					if (this.htaExtensions.Count > 0) {
						if (!this.htaExtensions.ContainsKey(fdFile.Extension)) {
							objBatch.Err("El archivo '" + fiFile.Name + "' no tiene una extensión permitida.");
							continue;
						}
					}
					
					//Verifica que tenga un tamaño permitido
					if (this.lngSize > 0) {
						if (fiFile.Length > this.lngSize) {
							objBatch.Err("El archivo '" + fiFile.Name + "' supera el tamaño permitido (" + (this.lngSize / 1024) + "KB).");
							continue;
						}
					}

				} catch (Exception) {
					objBatch.Err("Se produjo un error al intentar interpretar el archivo '" + fiFile.Name + "'.");
					continue;
				}


				try {
					//Busca el OI Personal desde la clave
					xmlPersonal = this.GetPersonal(fdFile.Name);
					
					//Valida que el legajo exista en la liquidación
					if (xmlPersonal == null) {
						objBatch.Err("No se encontró una persona que coincida con los datos del archivo '" + fiFile.Name + "'.");
						continue;
					} else
						objPersonal = PERSONAL.Get(xmlPersonal.GetAttr("oi_personal"));
					
					//Valida si ya tiene una imagen cargada --------------------------------------------------
					objImagen = null;
					switch (this.strClass.ToUpper()) {
						case "FOTO":
							if (!objPersonal.oi_fotoNull && !pbolReplace) {
								objBatch.Wrn("No se incorporará la foto para '" + objPersonal.d_ape_y_nom + " (" + fiFile.Name + ")' porque ya tiene cargada una.");
								continue;
							}
							
							//Se obtiene el DDO de la imagen anterior
							if (!objPersonal.oi_fotoNull)
								objImagen = objPersonal.Getoi_foto();
								
							break;

						case "FIRMA":
							if (!objPersonal.oi_fotoNull && !pbolReplace) {
								objBatch.Wrn("No se incorporará la firma para '" + objPersonal.d_ape_y_nom + " (" + fiFile.Name + ")' porque ya tiene cargada una.");
								continue;
							}

							//Se obtiene el DDO de la imagen anterior
							if (!objPersonal.oi_fotoNull)
								objImagen = objPersonal.Getoi_firma();

							break;
							
					}
					
					//Se da de alta la nueva imagen ----------------------------------------------------------
					BINFile objBin;
					Stream strFile;
					
					strFile = fiFile.OpenRead();
					objBin = NomadEnvironment.GetProxy().BINService().PutFile("NucleusRH.Base.Personal.Imagenes.HEAD", fiFile.Name, strFile);
					strFile.Close();
					
					//Se enlaza al personal con la nueva imagen ----------------------------------------------
					switch (this.strClass.ToUpper()) {
						case "FOTO":
							objPersonal.oi_foto = objBin.Id;
							break;

						case "FIRMA":
							objPersonal.oi_firma = objBin.Id;
							break;
					}
					
					try {
						NomadEnvironment.GetCurrentTransaction().Begin();
						
						//Graba el personal con la nueva imagen
						NomadEnvironment.GetCurrentTransaction().Save(objPersonal);

						//Se elimina la imagen anterior
						if (objImagen != null)
							NomadEnvironment.GetCurrentTransaction().Delete(objImagen);

						NomadEnvironment.GetCurrentTransaction().Commit();
						intFilesOk++;

					} catch (Exception ex) {
						objBatch.Err("Se produjo un error al actualizar el la persona '" + objPersonal.d_ape_y_nom + "' con el archivo '" + fiFile.Name + "'. " + ex.Message + ".");
						NomadEnvironment.GetCurrentTransaction().Rollback();
						break;
					}

				} catch (Exception) {
					objBatch.Err("Se produjo un error al intentar interpretar el archivo '" + fiFile.Name + "'.");
					continue;
				}

			}

			objBatch.Log("La importación terminó correctamente.");
			objBatch.Log("Se analizaron '" + arrFileInfo.Length.ToString() + "' archivos. \\\\ Se importaron '" + intFilesOk.ToString() + "' archivos.");
			
			objBatch.SetPro(100);

		}

        /// <summary>
        /// Obtiene un hashtable accesible por codigo de varible y retorna el OI.
        /// </summary>
        /// <param name="pstrClave">Clave de búsqueda.</param>
        /// <returns></returns>
        private NomadXML GetPersonal(string pstrClave) {
            if (this.htaOIs == null) {
        		this.htaOIs = NomadEnvironment.QueryHashtable(this.strQuery, "", "clave");
            }

            return this.htaOIs.ContainsKey(pstrClave) ? (NomadXML)this.htaOIs[pstrClave] : null;
        }

		
		private FileData GetParts(string pstrFileName) {
			FileData fResult = new FileData();
			
			char[] arrLetters;
			int intPoint = 0;
			
			arrLetters = pstrFileName.ToCharArray();
			intPoint = pstrFileName.LastIndexOf('.');
/*			
			for (int x = 0; x < arrLetters.Length; x++) {
				
				chrLetter = arrLetters[x];
				if (chrLetter >= 48 && chrLetter <= 57) {
					intStartNumber = x;
					break;
				}

			}
*/			
//			fResult.DocType   = pstrFileName.Substring(0, intStartNumber);
//			fResult.Number    = pstrFileName.Substring(intStartNumber, (intPoint - intStartNumber));
			fResult.Name      = pstrFileName.Substring(0, intPoint);
			fResult.Extension = pstrFileName.Substring((intPoint + 1), (pstrFileName.Length - intPoint - 1));
			
//			if (fResult.DocType.Length < 2 || fResult.Number == "" || fResult.Extension.Length < 3)
//				throw new Exception("El archivo '" + pstrFileName + "' no tiene un formato válido.");
			
			return fResult;
		}

		private struct FileData {
//			public string DocType;
//			public string Number;
			public string Name;
			public string Extension;
		}

    }
    
}


