using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;


namespace Generico.Classes {
    class clsFunctions {
        
        /// <summary>
        /// Aplica un template de tipo interface y retorna el string resultante.
        /// </summary>
        /// <param name="pstrTemplate"></param>
        /// <param name="pxtrRow"></param>
        /// <returns>Registro resuelto.</returns>
        public static string ApplyTemplate(string pstrTemplate, XmlTextReader pxtrRow, string pstrRowType) {
            Hashtable htaResult = new Hashtable();
            string strResult;
            
            for (int x = 0; x < pxtrRow.AttributeCount; x++) {
              pxtrRow.MoveToAttribute(x);
              //if (pxtrRow.Value == "") continue;
              
              htaResult.Add(pxtrRow.Name, pxtrRow.Value);
            }
            
            strResult = ApplyTemplate(pstrTemplate, htaResult, pstrRowType);
            
            return ApplyReplace(strResult);
            
        }    

        /// <summary>
        /// Aplica un template de tipo interface y retorna el string resultante.
        /// </summary>
        /// <param name="pstrTemplate"></param>
        /// <param name="phtaValues"></param>
        /// <returns>Registro resuelto.</returns>
        public static string ApplyTemplate(string pstrTemplate, Hashtable phtaValues, string pstrRowType) {
	        string strResult = "";
	        int l = 0;
	        string strKey;
	        int intLength;
	        string strLength;
	        string[] strField;
	        string strValue;
	        bool bolComplete = false;
	        
	        for (int i = pstrTemplate.IndexOf("{"); i >= 0; i = pstrTemplate.IndexOf("{", l), l++) {
		        
		        strResult = strResult + pstrTemplate.Substring(l, i - l);
	            l = pstrTemplate.IndexOf("}", i);
	            
	            strField = pstrTemplate.Substring(i + 1, (l - i) - 1).Split(':');
	            strKey = strField[0];
	            
	            if (strField.Length == 1) {
	                intLength = 0;
	            } else {
	                strLength = strField[1];
	                
	                //Pregunta se deben completarse los espacios
	                bolComplete = (strLength.IndexOf('+') >= 0);
	                intLength = int.Parse(strLength.Replace("+", ""));
	            }
	            
	            if (strKey.IndexOf("(") > 0) {
	                //Es una función
	                strResult = strResult + clsFunctions.ApplyFunction(strKey, phtaValues);
	            } else {
	                //Es un campo normal
	                
	                strValue = phtaValues.ContainsKey(strKey) ? (string) phtaValues[strKey] : "";

    	            if (bolComplete) {
                        strValue = new string(' ', intLength);
    	            } else {
    	                if (intLength > 0)
    	                    if (strValue.Length != intLength)
    	                        if (phtaValues.ContainsKey("rowdesc"))
    	                            Console.WriteLine(string.Format("3:El largo del campo '{0}' es de '{1}' y no corresponde con el esperado '{2}' para el registro {3}.", strKey, strValue.Length, intLength, phtaValues["rowdesc"]));
    	                        else
    	                            Console.WriteLine(string.Format("3:El largo del campo '{0}' es de '{1}' y no corresponde con el esperado '{2}' para el registro de tipo {3}.", strKey, strValue.Length, intLength, pstrRowType));
    	            }        

    	            strResult = strResult + strValue;
	            }
	                        
	        }

	        strResult = strResult + pstrTemplate.Substring(l);
        	
	        return strResult;
        }    
        
        public static string ApplyFunction(string pstrFunction, Hashtable phtaValues) {
            string strRestult = "";
            
            string strFunctionName = pstrFunction.Substring(0, pstrFunction.IndexOf("("));
            string strArguments = pstrFunction.Substring(strFunctionName.Length, pstrFunction.Length - strFunctionName.Length);
            strArguments = strArguments.Substring(1, strArguments.Length - 2);
            ArrayList arrArguments = new ArrayList(strArguments.Split(','));
            
            
            switch (strFunctionName) {
                case "space":
                    strRestult = clsFunctions.ApplySpace(arrArguments);
                    break;
            }
            
            return strRestult;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parrArguments"></param>
        /// <returns></returns>
        public static string ApplySpace(ArrayList parrArguments) {
            string strResult = "";
            
            try {
                strResult = new string(' ', int.Parse((string) parrArguments[0]));
            } catch {
                throw new Exception("El parámetro de la función space no es un número válido.");
            }
            
            return strResult;
        }

        
        /// <summary>
        /// Aplica el reemplazo de los caracteres no ASCII
        /// </summary>
        /// <param name="pstrSource">String a aplicarle los reemplazos.</param>
        /// <returns></returns>
        public static string ApplyReplace(string pstrSource) {
            string strRestult = pstrSource;
            Hashtable htaAscii = new Hashtable();
            
            //Completa el hash con los caracteres a reemplazar
            htaAscii.Add("Á", "A");
            htaAscii.Add("É", "E");
            htaAscii.Add("Í", "I");
            htaAscii.Add("Ó", "O");
            htaAscii.Add("Ú", "U");
            htaAscii.Add("á", "a");
            htaAscii.Add("é", "e");
            htaAscii.Add("í", "i");
            htaAscii.Add("ó", "o");
            htaAscii.Add("ú", "u");

            htaAscii.Add("Ñ", "N"); 
            htaAscii.Add("ñ", "n"); 

            htaAscii.Add("Ü", "U");
            htaAscii.Add("ü", "u");
            
            //Realiza los reemplazos de los caracteres
            foreach (string strKey in htaAscii.Keys) {
                strRestult = strRestult.Replace(strKey, (string) htaAscii[strKey]);
            }

            return strRestult;
        }
        
    }
}
