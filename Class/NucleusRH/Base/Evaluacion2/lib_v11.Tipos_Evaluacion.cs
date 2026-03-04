using System;
using System.Xml;
using System.IO;
using System.Collections;

using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;
using Nomad.NSystem.Functions;

namespace NucleusRH.Base.Evaluacion2.Tipos_Evaluacion
{

  //////////////////////////////////////////////////////////////////////////////////
  //Clase Tipos de Evaluaciones
  public partial class TIPO_EVAL
  {
    public static void RefreshItems(string plantilla, NomadXML Items)
    {

      if (Items.isDocument) Items=Items.FirstChild();

      NomadProxy proxy = NomadProxy.GetProxy();

      //Obtengo la Plantilla
      PLANTILLA MyPLA = PLANTILLA.Get(plantilla, false);

      //Obtengo el QUERY
      NomadXML MyQUERY = proxy.FileServiceIO().LoadFileXML("EVALUACIONES", MyPLA.c_tipo_plantilla + ".TYPE_PLA.xml");

      //Obtengo el Recordset
      NomadXML MyRS=proxy.SQLService().GetXML(MyQUERY, (NomadXML)null);

      //Elimino los Elementos Eliminados
      ArrayList toDelete=new ArrayList();
      foreach(OBJETIVO MyOBJ in MyPLA.OBJETIVOS)
      {

        if (Items.FindElement2("ITEM", "c_objetivo", MyOBJ.c_objetivo)==null)
          toDelete.Add(MyOBJ);
      }
      foreach(OBJETIVO MyOBJ in toDelete)
      {
        //obligo a que se cargen los subobjetivos
        if (MyOBJ.SUBOBJETIVOS.Count>=0)
          MyPLA.OBJETIVOS.Remove(MyOBJ);
      }

      //Agrego Todos los elementos Nuevos
      for (NomadXML MyCUR=MyRS.FirstChild(); MyCUR!=null; MyCUR=MyCUR.Next())
      {
        OBJETIVO MyOBJ;
        if (Items.FindElement2("ITEM", "c_objetivo", MyCUR.GetAttr("c_objetivo"))==null) continue;

        //Busco el Objetivo
        MyOBJ=(OBJETIVO)MyPLA.OBJETIVOS.GetByAttribute("c_objetivo", MyCUR.GetAttr("c_objetivo"));

        if (MyOBJ==null)
        { //No existe lo Agrego
          MyOBJ=new OBJETIVO();
          MyPLA.OBJETIVOS.Add(MyOBJ);
        }

        //Actualizo los Campos
        MyOBJ.oi_competencia = MyCUR.GetAttr("oi_competencia");
        MyOBJ.c_objetivo = MyCUR.GetAttr("c_objetivo");
        MyOBJ.d_objetivo = MyCUR.GetAttr("d_objetivo");
        MyOBJ.oi_escala = MyCUR.GetAttr("oi_escala");
        MyOBJ.n_ponderacion = MyCUR.GetAttrDouble("n_ponderacion");
        MyOBJ.d_cus1 = MyCUR.GetAttr("d_cus1");
        MyOBJ.d_cus2 = MyCUR.GetAttr("d_cus2");
        MyOBJ.d_cus3 = MyCUR.GetAttr("d_cus3");
        MyOBJ.d_cus4 = MyCUR.GetAttr("d_cus4");
        MyOBJ.d_cus5 = MyCUR.GetAttr("d_cus5");

        //Actualiza el NULL
        MyOBJ.oi_competenciaNull = MyCUR.GetAttr("oi_competencia") == "" ? true : false;
        MyOBJ.oi_escalaNull = MyCUR.GetAttr("oi_escala") == "" ? true : false;
        MyOBJ.n_ponderacionNull = MyCUR.GetAttr("n_ponderacion") == "" ? true : false;

        //Actualiza SubItems
        for (NomadXML MyCUR2=MyCUR.FirstChild(); MyCUR2!=null; MyCUR2=MyCUR2.Next())
        {
          SUBOBJETIVO MySUBOBJ;

          MySUBOBJ=(SUBOBJETIVO)MyOBJ.SUBOBJETIVOS.GetByAttribute("c_subobjetivo", MyCUR2.GetAttr("c_subobjetivo"));
          if (MySUBOBJ==null)
          {
            MySUBOBJ = new SUBOBJETIVO();
            MyOBJ.SUBOBJETIVOS.Add(MySUBOBJ);
          }

          MySUBOBJ.oi_aptit_comp = MyCUR2.GetAttr("oi_aptit_comp");
          MySUBOBJ.c_subobjetivo = MyCUR2.GetAttr("c_subobjetivo");
          MySUBOBJ.d_subobjetivo = MyCUR2.GetAttr("d_subobjetivo");
          MySUBOBJ.oi_escala = MyCUR2.GetAttr("oi_escala");
          MySUBOBJ.n_ponderacion = MyCUR2.GetAttrDouble("n_ponderacion");
          MySUBOBJ.d_cus1 = MyCUR2.GetAttr("d_cus1");
          MySUBOBJ.d_cus2 = MyCUR2.GetAttr("d_cus2");
          MySUBOBJ.d_cus3 = MyCUR2.GetAttr("d_cus3");
          MySUBOBJ.d_cus4 = MyCUR2.GetAttr("d_cus4");
          MySUBOBJ.d_cus5 = MyCUR2.GetAttr("d_cus5");

          //Actualiza el NULL
          MySUBOBJ.oi_aptit_compNull = MyCUR2.GetAttr("oi_aptit_comp") == "" ? true : false;
          MySUBOBJ.oi_escalaNull = MyCUR2.GetAttr("oi_escala") == "" ? true : false;
          MySUBOBJ.n_ponderacionNull = MyCUR2.GetAttr("n_ponderacion") == "" ? true : false;
        }
      }

      //Commit
      NomadEnvironment.GetCurrentTransaction().Save(MyPLA);
    }

    public static NomadXML LoadItems(string plantilla)
    {
      NomadProxy proxy = NomadProxy.GetProxy();

      //Obtengo la Plantilla
      PLANTILLA MyPLA = PLANTILLA.Get(plantilla, false);

      //Obtengo el QUERY
      NomadXML MyQUERY = proxy.FileServiceIO().LoadFileXML("EVALUACIONES", MyPLA.c_tipo_plantilla + ".TYPE_PLA.xml");

      //Obtengo el Recordset
      NomadXML MyRS=proxy.SQLService().GetXML(MyQUERY, (NomadXML)null);

      //Resultado
      NomadXML retval=new NomadXML("ROWS");
      NomadXML row;

      //Recorro la lista y armo el resultado
      for (NomadXML cur=MyRS.FirstChild(); cur!=null; cur=cur.Next())
      {
        row=retval.AddTailElement("ROW");
        row.SetAttr("id", cur.GetAttr("c_objetivo"));
        row.SetAttr("c_objetivo", cur.GetAttr("c_objetivo"));
        row.SetAttr("d_objetivo", cur.GetAttr("d_objetivo"));
        row.SetAttr("n_ponderacion", cur.GetAttr("n_ponderacion"));
      }

       //Resultado
       return retval;
    }

  }
}

