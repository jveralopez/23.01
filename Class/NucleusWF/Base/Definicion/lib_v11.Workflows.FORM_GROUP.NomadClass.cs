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

namespace NucleusWF.Base.Definicion.Workflows
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase WorkFlows
    public partial class FORM_GROUP : Nomad.NSystem.Base.NomadObject
    {

	/// <summary>
	/// Graba los datos para la registración
	/// </summary>
	/// <param name="pxmlParam">Xml de parametros</param>
	/// <returns></returns>
	public static string UpDown(NomadXML pxmlParam) {
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("Comienza el metodo FORM_GROUP.UpDown V1.1");
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("pxmlParam: " + pxmlParam.ToString());
		NomadLog.Debug("--------------------------------------------------------------------------");
		
		FORM objForm;
		FORM_GROUP objFromFGroup;
		FORM_GROUP objToFGroup;
		
		NomadXML xmlParam;
		NomadXML xmlFGroups;
		int OIFGroup;
		string strDirection;
		int intOITo;
		string strResult = "";
		
		//Recupera los parametros
		xmlParam     = pxmlParam.FirstChild();
		OIFGroup     = xmlParam.GetAttrInt("oi_form_group");
		strDirection = xmlParam.GetAttr("direction"); //Direccion que se quiere mover el campo "UL"(Up/Left) o "DR"(Down/Right)
		
		//Ejecuta un query para obtener los grupos pertenecientes al Form del grupo
		//Segun la direccion el resultado del query es ASC o DESC, de manera que el intOITo siempre sera el anterior al buscado
		xmlFGroups = NomadEnvironment.QueryNomadXML(NucleusWF.Base.Definicion.Workflows.FORM_GROUP.Resources.QRY_Groups, xmlParam.ToString());
		xmlFGroups = xmlFGroups.FirstChild();
		NomadLog.Debug("Resultado query: " + xmlFGroups.ToString());
		
		
		//Recorre los campos y busca el campo a mover y el afectado por el cambio
		intOITo = 0;
		for (NomadXML xmlFGroup = xmlFGroups.FirstChild(); xmlFGroup != null; xmlFGroup = xmlFGroup.Next()) {
			
			//Pregunta por el campo buscado
			if (xmlFGroup.GetAttrInt("oi_form_group") == OIFGroup) break;
			
			//Guarda el oi del ultimo campo leido
			intOITo = xmlFGroup.GetAttrInt("oi_form_group");
		}
		
		
		//Si no existe el grupo TO significa que no se puede mover mas y deberia finaliza sin realiza cambios
		if (intOITo == 0)  {
			
			NomadLog.Debug("No se realiza cambio de orden");
			
		} else {
			
			NomadLog.Debug("Recupera el form con OI " + xmlFGroups.GetAttr("oi_form"));
			
			objForm = FORM.Get(xmlFGroups.GetAttrInt("oi_form"));
			
			if (objForm != null) {
				string strTempOrder;
				
				objFromFGroup = (FORM_GROUP) objForm.FORM_GROUPS.GetById(OIFGroup.ToString());
				objToFGroup = (FORM_GROUP) objForm.FORM_GROUPS.GetById(intOITo.ToString());

				NomadLog.Debug("Realiza cambio de orden");
				NomadLog.Debug("OI from: " + objFromFGroup.Id);
				NomadLog.Debug("Order from: " + objFromFGroup.c_order);
				NomadLog.Debug("OI to: " + objToFGroup.Id);
				NomadLog.Debug("Order to: " + objToFGroup.c_order);
								
				strTempOrder = objToFGroup.c_order;
				objToFGroup.c_order = objFromFGroup.c_order;
				objFromFGroup.c_order = strTempOrder;
				
				NomadEnvironment.GetCurrentTransaction().Save(objForm);				
			}

		}
		
		return strResult;
	
	}

	/// <summary>
	/// Elimina un control
	/// </summary>
	/// <param name="pxmlParam">Xml de parametros</param>
	/// <returns></returns>
	public static string Delete(NomadXML pxmlParam) {
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("Comienza el metodo FORM_GROUP.Delete V1.1");
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("pxmlParam: " + pxmlParam.ToString());
		NomadLog.Debug("--------------------------------------------------------------------------");
		
		FORM_GROUP objFGroup;
		NomadXML xmlParam;
		string strResult = "";

		//Recupera los parametros
		xmlParam = pxmlParam.FirstChild();
		
		objFGroup = (FORM_GROUP) FORM_GROUP.Get(xmlParam.GetAttrInt("oi_form_group"));
		
		try {
			NomadEnvironment.GetCurrentTransaction().Delete(objFGroup);
		
		} catch (Exception ex) {
			strResult = "Se produjo un error al eliminar el control.";
			NomadLog.Error("Se produjo un error al eliminar el control: " + ex.Message);
		}
		
		NomadLog.Debug("EL proceso finalizo correctamente. " + strResult);
		return strResult;
	
	}
   }
}
  

