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
    public partial class FIELD : Nomad.NSystem.Base.NomadObject
    {

	/// <summary>
	/// Cambiar el numero de orden del control
	/// </summary>
	/// <param name="pxmlParam">Xml de parametros</param>
	/// <returns></returns>
	public static string UpDown(NomadXML pxmlParam) {
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("Comienza el metodo FIELD.UpDown V1.1");
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("pxmlParam: " + pxmlParam.ToString());
		NomadLog.Debug("--------------------------------------------------------------------------");
		
		FORM_GROUP objGroup;
		FIELD objFromField;
		FIELD objToField;
		
		NomadXML xmlParam;
		NomadXML xmlFields;
		int OIField;
		string strDirection;
		int intOITo;
		string strResult = "";
		
		//Recupera los parametros
		xmlParam     = pxmlParam.FirstChild();
		OIField      = xmlParam.GetAttrInt("oi_field");
		strDirection = xmlParam.GetAttr("direction"); //Direccion que se quiere mover el campo "UL"(Up/Left) o "DR"(Down/Right)
		
		//Ejecuta un query para obtener los campos pertenecientes al Form_Group del campo
		//Segun la direccion el resultado del query es ASC o DESC, de manera que el fieldTo siempre sera el anterior al buscado
		xmlFields = NomadEnvironment.QueryNomadXML(NucleusWF.Base.Definicion.Workflows.FIELD.Resources.QRY_GroupFields, xmlParam.ToString());
		xmlFields = xmlFields.FirstChild();
		NomadLog.Debug("Resultado query: " + xmlFields.ToString());
		
		
		//Recorre los campos y busca el campo a mover y el afectado por el cambio
		intOITo = 0;
		for (NomadXML xmlField = xmlFields.FirstChild(); xmlField != null; xmlField = xmlField.Next()) {
			
			//Pregunta por el campo buscado
			if (xmlField.GetAttrInt("oi_field") == OIField) break;
			
			//Guarda el oi del ultimo campo leido
			intOITo = xmlField.GetAttrInt("oi_field");
		}
		
		
		//Si no existe el campo TO significa que no se puede mover mas y deberia finaliza sin realiza cambios
		if (intOITo == 0)  {
			
			NomadLog.Debug("No se realiza cambio de orden");
			
		} else {
			
			NomadLog.Debug("Recupera el form_group con OI " + xmlFields.GetAttr("oi_form_group"));
			
			objGroup = FORM_GROUP.Get(xmlFields.GetAttrInt("oi_form_group"));
			
			if (objGroup != null) {
				int intTempOrder;
				
				objFromField = (FIELD) objGroup.FORM_FIELDS.GetById(OIField.ToString());
				objToField = (FIELD) objGroup.FORM_FIELDS.GetById(intOITo.ToString());

				NomadLog.Debug("Realiza cambio de orden");
				NomadLog.Debug("OI from: " + objFromField.Id);
				NomadLog.Debug("Order from: " + objFromField.e_order);
				NomadLog.Debug("OI to: " + objToField.Id);
				NomadLog.Debug("Order to: " + objToField.e_order);
								
				intTempOrder = objToField.e_order;
				objToField.e_order = objFromField.e_order;
				objFromField.e_order = intTempOrder;
				
				NomadEnvironment.GetCurrentTransaction().Save(objGroup);				
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
		NomadLog.Debug("Comienza el metodo FIELD.Delete V1.1");
		NomadLog.Debug("--------------------------------------------------------------------------");
		NomadLog.Debug("pxmlParam: " + pxmlParam.ToString());
		NomadLog.Debug("--------------------------------------------------------------------------");
		
		FIELD objField;
		NomadXML xmlParam;
		string strResult = "";

		//Recupera los parametros
		xmlParam = pxmlParam.FirstChild();
		
		objField = (FIELD) FIELD.Get(xmlParam.GetAttrInt("oi_field"));
		
		try {
			NomadEnvironment.GetCurrentTransaction().Delete(objField);
		
		} catch (Exception ex) {
			strResult = "Se produjo un error al eliminar el control.";
			NomadLog.Error("Se produjo un error al eliminar el control: " + ex.Message);
		}
		
		NomadLog.Debug("EL proceso finalizo correctamente. " + strResult);
		return strResult;
	
	}

   }
}
    
