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
    public partial class FORM : Nomad.NSystem.Base.NomadObject
    {

  /// <summary>
  /// Genera una copia de un formulario.
  /// Realiza las validaciones correspondientes.
  /// Genera el HTML del formulario.
  /// </summary>
  /// <param name="pobjFormO">Formulario original del cual se hace la copia.</param>
  /// <param name="pxmlParam">Hashtable con los formularios ya creados.</param>
  /// <returns></returns>
  public static FORM CopyFrom(FORM pobjFormO, Hashtable phtaForms, Hashtable phtaGroupVars) {
    Hashtable htaGridChilds;

    //Realiza las validaciones iniciales
    //if (!WF.CODE_TEST(pobjFormO.c_form))
    //  throw new NomadException("El nombre del Formulario '" + pobjFormO.c_form + "' contiene caracteres inválidos...");

    if (phtaForms.ContainsKey(pobjFormO.c_form.ToUpper()))
      throw new NomadException("El Formulario '" + pobjFormO.c_form + "' está duplicado...");

    //Recorre los grupos del formulario
    htaGridChilds = new Hashtable();
    foreach (FORM_GROUP objFG in pobjFormO.FORM_GROUPS) {

      //Valida que el codigo del grupo este bien formado
      if (!WF.CODE_TEST(objFG.c_form_group))
        throw new NomadException("El nombre del grupo '" + objFG.c_form_group + "' del formulario '" + pobjFormO.c_form + "' contiene caracteres inválidos...");

      //Valida que el tipo de grupo sea el correcto
      if (!("|MAIN|CHILD|GRID|BUT|REPORT|").Contains("|" + objFG.c_mode + "|"))
        throw new NomadException("El MODO '" + objFG.c_mode + "' del grupo '" + objFG.c_form_group + "' no se correspondo con los modos válidos.");

      //Valida que el grupo de variable asociado sea valido
      if (phtaGroupVars.ContainsKey(objFG.c_var_group.ToUpper()))
        throw new NomadException("El grupo '" + objFG.c_form_group + "' está asociado a un grupo de variables inexistente.");

      //Valida que no existan dos grillas apuntando al mismo CHILD
      if (objFG.c_mode == "CHILD" || objFG.c_mode == "GRID") {
        if (htaGridChilds.ContainsKey(objFG.c_var_group)) {
          FORM_GROUP objDup = (FORM_GROUP) htaGridChilds[objFG.c_var_group];
          throw new NomadException("Los grupos '" + objFG.c_form_group + "' y '" + objDup.c_form_group + "' están asociados al mismo grupo de variables. No pueden existir dos o más grupos asociados al mismo grupo de variables.");
        }

        //Agrega el grupo de variable a la lista de grupos utilizados
        htaGridChilds[objFG.c_var_group] = objFG;
      }

      //Recorre los campos/columnas del grupo
      foreach(FIELD objField in objFG.FORM_FIELDS) {

        //Valida que el codigo del grupo este bien formado
        if (!WF.CODE_VAR_TEST(objField.c_parent_field))
          throw new NomadException("El nombre del campo '" + objField.c_parent_field + "' del grupo '" + objFG.c_form_group + "' del formulario '" + pobjFormO.c_form + "' contiene caracteres inválidos...");

        //Valida que los campos/columnas de los grupos apunten a variables existentes
        if (phtaGroupVars.ContainsKey(objField.c_var.ToUpper()))
          throw new NomadException("El campo/columna '" + objField.c_field + "' está asociado a una variable inexistente.");

        if (phtaGroupVars.ContainsKey(objField.c_parent_field.ToUpper()))
          throw new NomadException("El campo/columna '" + objField.c_field + "' contiene como variable padre una variable inexistente.");

        if ((objField.c_var.ToUpper() == objField.c_parent_field.ToUpper()) && (objField.c_var != "" || objField.c_parent_field != ""))
          throw new NomadException("Error en el campo/columna '" + objField.c_field + "'. La variable y la variable padre no pueden apuntar a la misma variable.");
      }
    }

    //Crea el nuevo formulario
    FORM objFormP = new FORM();
    objFormP.c_form      = pobjFormO.c_form;
    objFormP.d_form      = pobjFormO.d_form;
    objFormP.c_file_mode = pobjFormO.c_file_mode;

    phtaForms[objFormP.c_form.ToUpper()] = objFormP;

    //Genera el archivo HTML del formulario
    objFormP.d_file_html = NucleusWF.Base.Ejecucion.Instancias.INSTANCE.GenerateHTMLForm(pobjFormO.Id);

    return objFormP;

  }

      public static NucleusWF.Base.Definicion.Workflows.FORM CREAR_DDO(string oi_WF)
      {
          int max = 0;
          WF objWF = WF.Get(oi_WF, false);
          foreach (NucleusWF.Base.Definicion.Workflows.FORM form in objWF.FORMS)
          {
              if (Nomad.NSystem.Functions.StringUtil.str2int(form.c_form) > max)
                  max = Nomad.NSystem.Functions.StringUtil.str2int(form.c_form);
          }

          FORM retval = new FORM();
          retval.c_form = (max + 1).ToString();
          return retval;
      }
  }

}


