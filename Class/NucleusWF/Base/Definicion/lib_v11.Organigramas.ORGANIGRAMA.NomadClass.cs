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

namespace NucleusWF.Base.Definicion.Organigramas
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Organigramas
    public partial class ORGANIGRAMA : Nomad.NSystem.Base.NomadObject
    {

    public static void Exportar(string ID, NomadXML result)
    {
      //Obtengo el OBJETO
      ORGANIGRAMA objExp=ORGANIGRAMA.Get(ID,false);

      //Agrego el ITEM a la SELECCION ACTUAL
      NomadXML newElement=result.AddTailElement("ITEM");
      newElement.SetAttr("TYPE"   , "ORG");
      newElement.SetAttr("ID"     , ID);
      newElement.SetAttr("CODE"   , objExp.c_organigrama);
      newElement.SetAttr("LABEL"  , "["+objExp.c_organigrama+"]"+objExp.d_organigrama);
      newElement.SetAttr("VERSION", objExp.l_automatica?"N/A":objExp.e_version.ToString());

      //Elemento RAIZ
      NomadXML ORG=newElement.AddTailElement("ORGANIGRAMA");
      ORG.SetAttr("CODE"   , objExp.c_organigrama);
      ORG.SetAttr("DESC"   , objExp.d_organigrama);

      //Roles
      NomadXML ROLES=ORG.AddTailElement("ROLES");
      foreach(ROLE MyROL in objExp.ROLES)
      {
        NomadXML ROLE=ROLES.AddTailElement("ROLE");

        ROLE.SetAttr("CODE"  , MyROL.c_role);
        ROLE.SetAttr("DESC"  , MyROL.d_role);
        ROLE.SetAttr("UNIQUE", MyROL.l_unique);
        ROLE.SetAttr("COLOR" , MyROL.d_color);
      }

      //Estructuras
      NomadXML ESTRS=ORG.AddTailElement("ESTRUCTURAS");
      foreach(NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA MyESTR in objExp.ESTRUCTURA)
      {
        NomadXML ESTR=ESTRS.AddTailElement("ESTRUCTURA");

        ESTR.SetAttr("CODE"  , MyESTR.c_estructura);
        ESTR.SetAttr("DESC"  , MyESTR.d_estructura);
        ESTR.SetAttr("ROLE"  , MyESTR.c_role);

        ESTR.SetAttr("INI"   , MyESTR.l_inicia);
        ESTR.SetAttr("ORDER" , MyESTR.e_orden);
        ESTR.SetAttr("PARENT", MyESTR.c_estr_padre);

        foreach(NucleusWF.Base.Definicion.Organigramas.ENTIDAD_ESTR MyETTY in MyESTR.ENTIDAD_ESTR)
        {
           NomadXML ETTY=ESTR.AddTailElement("ENTIDAD");
          ETTY.SetAttr("ID"  , MyETTY.oi_entidad);
          ETTY.SetAttr("DESC", MyETTY.d_entidad_estr);
        }

      }

    }

    public static void Importar(string itemID, string c_modo, string oi_organigrama, string c_code, string c_desc, bool refreshETTYs)
    {
      NomadBatch MyBATCH=NomadBatch.GetBatch("Importar", "Importar");
      NomadXML objXML,objITEM,curs,cur,cur2;
      ArrayList arrAUX=new ArrayList();

      ORGANIGRAMA orgTrunk=null;
      ORGANIGRAMA orgNew  =null;

      //Creo el Organigrama RAIZ
      switch(c_modo)
      {
        case "N":
          orgNew=CREATE_DDO(null);
          orgNew.c_organigrama=c_code;
          orgNew.d_organigrama=c_desc;
          orgNew.e_version    =1;
          orgNew.e_version_pub=0;
          orgNew.l_automatica =false;
          break;

        case "O":
          orgNew=ORGANIGRAMA.Get(oi_organigrama,false);
          orgNew.e_version=orgNew.e_version+1;
          break;

        case "A":
          //Incremento la Version del Organigrama Principal
          orgTrunk=ORGANIGRAMA.Get(oi_organigrama,false);
          orgTrunk.e_version=orgTrunk.e_version+1;

          //Creo el Publicado
          orgNew=new ORGANIGRAMA();
          orgNew.c_organigrama=orgTrunk.c_organigrama;
          orgNew.d_organigrama=orgTrunk.d_organigrama;
          orgNew.e_version    =orgTrunk.e_version-1;
          orgNew.e_version_pub=orgTrunk.e_version-1;
          orgNew.l_automatica =true;
          break;
      }

      //Obtengo el XML
      objXML=NomadProxy.GetProxy().FileServiceIO().LoadFileXML("TEMP","wfimportfile.txt");
      if (objXML.isDocument) objXML=objXML.FirstChild();

      //Busco el ITEM
      objITEM=null;
      for(cur=objXML.FirstChild(); cur!=null && objITEM==null; cur=cur.Next())
        if (cur.GetAttr("TYPE")+":"+cur.GetAttr("ID")==itemID)
          objITEM=cur;
      if (objITEM==null)
        throw new Exception("ITEM 'ID="+itemID+"' no encontrado.");

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizo la lista de ROLES
      Hashtable RoleCount=new Hashtable();
      MyBATCH.SetMess("Copiando ACTORES/ROLES....");
      MyBATCH.Log("Copiando ACTORES/ROLES....");
      MyBATCH.SetPro(20);

      curs=objITEM.FirstChild().FindElement("ROLES");

      // Eliminar
      arrAUX.Clear();
      foreach(ROLE role in orgNew.ROLES)
      {
        if (curs.FindElement2("ROLE","CODE",role.c_role)==null)
        {
          NomadLog.Info("Elimino el Role '"+role.c_role+"' a la Lista....");
          arrAUX.Add(role);
        }
      }
      foreach(ROLE role in arrAUX)
        orgNew.ROLES.Remove(role);

      // Agregar/Modificar
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        ROLE role=(ROLE)orgNew.ROLES.GetByAttribute("c_role",cur.GetAttr("CODE").ToUpper());
        if (role==null)
        { //No existe crear
          role=new ROLE();
          role.c_role  =cur.GetAttr("CODE").ToUpper();
          role.d_role  =cur.GetAttr("DESC");
          role.d_color =cur.GetAttr("COLOR").ToUpper();
          role.l_unique=cur.GetAttrBool("UNIQUE");
          orgNew.ROLES.Add(role);
        } else
        { //Existe modificar
          role.d_role  =cur.GetAttr("DESC");
          role.d_color =cur.GetAttr("COLOR").ToUpper();
          role.l_unique=cur.GetAttrBool("UNIQUE");
        }

        //Para la Validacion (Caso Publicada...)
        NomadLog.Info("Agregando el Role '"+role.c_role+"' a la Lista....");
        RoleCount[role.c_role.ToUpper()]=0;
      }
      if (c_modo=="A")
      {
        if (orgNew.ROLES.Count==0)
        {
            NomadLog.Error("No hay ACTORES/ROLES....");
            MyBATCH.Err("No hay ACTORES/ROLES definidos....");
            return;
        }
        if (!RoleCount.ContainsKey("OWNER"))
        {
            NomadLog.Error("El Actor/Rol 'OWNER' No Encontrado....");
            MyBATCH.Err("El Actor/Rol 'OWNER' No Encontrado....");
            return;
        }
        RoleCount["*"]=0;
       }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Actualizo la lista de ESTRUCTURAS
      MyBATCH.SetMess("Copiando las ESTRUCTURAS....");
      MyBATCH.SetPro(30);
      MyBATCH.Log("Copiando las ESTRUCTURAS....");

      curs=objITEM.FirstChild().FindElement("ESTRUCTURAS");

      // Eliminar
      arrAUX.Clear();
      foreach(ESTRUCTURA estr in orgNew.ESTRUCTURA)
      {
        if (curs.FindElement2("ESTRUCTURA","CODE",estr.c_estructura)==null)
        {
          NomadLog.Info("Elimino el Estructura '"+estr.c_estructura+"' a la Lista....");
          arrAUX.Add(estr);
        }
      }
      foreach(ESTRUCTURA estr in arrAUX)
        orgNew.ESTRUCTURA.Remove(estr);

      //Agregar/Modificar
      for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
      {
        ESTRUCTURA estr=(ESTRUCTURA)orgNew.ESTRUCTURA.GetByAttribute("c_estructura",cur.GetAttr("CODE").ToUpper());
        if (estr==null)
        { //No existe crear
          estr=new ESTRUCTURA();
          estr.c_estructura=cur.GetAttr("CODE");
          estr.d_estructura=cur.GetAttr("DESC");
          estr.c_role      =cur.GetAttr("ROLE");
          estr.c_estr_padre=cur.GetAttr("PARENT");
          estr.e_orden     =cur.GetAttrInt("ORDER");
          estr.l_inicia    =cur.GetAttrBool("INI");
          orgNew.ESTRUCTURA.Add(estr);
        } else
        { //Existe modificar
          estr.d_estructura=cur.GetAttr("DESC");
          estr.c_role      =cur.GetAttr("ROLE");
          estr.c_estr_padre=cur.GetAttr("PARENT");
          estr.e_orden     =cur.GetAttrInt("ORDER");
          estr.l_inicia    =cur.GetAttrBool("INI");
        }

        if (c_modo=="A")
        {
          if (!RoleCount.ContainsKey(estr.c_role.ToUpper()))
          {
            NomadLog.Error("Role '"+estr.c_role+"' No Encontrado....");
            MyBATCH.Err("Existen Nodos con ACTORES/ROLES mal configurados....");
            return;
          }

          NomadLog.Info("Incrementando el Role '"+estr.c_role+"'....");
          RoleCount[estr.c_role.ToUpper()]=((int)RoleCount[estr.c_role.ToUpper()])+1;
        }
      }
      if (c_modo=="A")
      {
        if (orgNew.ESTRUCTURA.Count==0)
        {
            NomadLog.Error("No hay Nodos definidos....");
            MyBATCH.Err("No hay NODOS definidos....");
            return;
        }

        //Validando los ACTORES
        foreach(ROLE role in orgNew.ROLES)
        {
          if (role.c_role.ToUpper()=="OWNER" && ((int)RoleCount[role.c_role.ToUpper()])>0)
          {
            MyBATCH.Err("El Actor ["+role.c_role+"] "+role.d_role+" no puede ser asignado a un NODO....");
            return;
          } else
          if (role.l_unique && ((int)RoleCount[role.c_role.ToUpper()])>1)
          {
            MyBATCH.Err("El Actor ["+role.c_role+"] "+role.d_role+" no puede estar en mas de un NODO....");
            return;
          }
        }

      }

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Guardo el DDO
      MyBATCH.SetMess("Guardando las ESTRUCTURAS Y ACTORES....");
      MyBATCH.Log("Guardando las ESTRUCTURAS Y ACTORES....");
      MyBATCH.SetPro(40);
      NomadEnvironment.GetCurrentTransaction().Begin();
      if (orgTrunk!=null) NomadEnvironment.GetCurrentTransaction().SaveRefresh(orgTrunk);
      NomadEnvironment.GetCurrentTransaction().SaveRefresh(orgNew);
      NomadEnvironment.GetCurrentTransaction().Commit();
      orgNew=ORGANIGRAMA.Get(orgNew.Id,false);

      //////////////////////////////////////////////////////////////////////////////////////////////////
      //Cargando las Personas....
      MyBATCH.SetMess("Copiando las PERSONAS....");
      MyBATCH.Log("Copiando las PERSONAS....");
      MyBATCH.SetPro(50);
      int tot=curs.ChildLength, cnt=0;

      if (refreshETTYs)
      {
        for (cur=curs.FirstChild(); cur!=null; cur=cur.Next())
        {
          cnt++; MyBATCH.SetPro(50,70,tot,cnt);
          if (cur.ChildLength==0) continue;

          //Obtengo la estructura
          ESTRUCTURA estrP=NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(orgNew.ESTRUCTURA.GetByAttribute("c_estructura",cur.GetAttr("CODE")).Id,true);

          //Eliminar
          arrAUX.Clear();
          foreach(ENTIDAD_ESTR etty in estrP.ENTIDAD_ESTR)
          {
            if (cur.FindElement2("ENTIDAD","ID",etty.oi_entidad)==null)
            {
              NomadLog.Info("Elimino la Entidad '"+etty.d_entidad_estr+"' a la Estructura '"+estrP.c_estructura+"' ....");
              arrAUX.Add(etty);
            }
          }
          foreach(ENTIDAD_ESTR etty in arrAUX)
            estrP.ENTIDAD_ESTR.Remove(etty);

          //Agregar/Modificar
          for (cur2=cur.FirstChild(); cur2!=null; cur2=cur2.Next())
          {
            ENTIDAD_ESTR MyETTY=(ENTIDAD_ESTR)estrP.ENTIDAD_ESTR.GetByAttribute("oi_entidad",cur.GetAttr("ID").ToUpper());
            if (MyETTY==null)
            {
              MyETTY=new ENTIDAD_ESTR();
              MyETTY.oi_entidad    =cur2.GetAttr("ID");
              MyETTY.d_entidad_estr=cur2.GetAttr("DESC");
              estrP.ENTIDAD_ESTR.Add(MyETTY);
            }
          }

          //Guardo la Estructura
          NomadEnvironment.GetCurrentTransaction().Save(estrP);
        }
      }

      if (c_modo=="A")
      {
        //Calculando las Estructuras Relacionadas....
        MyBATCH.SetSubBatch(70,90);
        CalcularRelaciones(orgNew);
      }
    }

    public static void CalcularRelaciones(ORGANIGRAMA orgNew)
    {
      NomadBatch MyBATCH=NomadBatch.GetBatch("CalcularRelaciones", "CalcularRelaciones");

      //Calculando las Estructuras Relacionadas....
      MyBATCH.SetMess("Calculando la RELACION de las ESTRUCTURAS....");
      MyBATCH.Log("Calculando la RELACION de las ESTRUCTURAS....");
      MyBATCH.SetPro(10);

      int cnt=0, deep;
      int tot=orgNew.ESTRUCTURA.Count;
      ESTR_RAMA newRAMA;
      ROLE roleAux;
      Hashtable ESTR=new Hashtable();
      bool isUnique;

      foreach(ESTRUCTURA estrO in orgNew.ESTRUCTURA)
      {
        ESTR.Clear();
        cnt++; MyBATCH.SetPro(10,90,tot,cnt);
        NomadLog.Info("Analizando la Estructura '"+estrO.c_estructura+"-"+estrO.c_role+"'....");

        ESTRUCTURA estrP=NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(orgNew.ESTRUCTURA.GetByAttribute("c_estructura",estrO.c_estructura).Id,true);
        ESTRUCTURA estrParent;

        //Agrego la RAMA ACTUAL
        isUnique=true;
        roleAux=(ROLE)orgNew.ROLES.GetByAttribute("c_role",estrO.c_role);
        if (roleAux!=null)
        {
          NomadLog.Info("--Agregando la Estructura ACTUAL '"+estrO.c_estructura+"-"+estrO.c_role+"'....");
          newRAMA=new ESTR_RAMA();
          newRAMA.c_estructura=estrO.c_estructura;
          newRAMA.c_role      =estrO.c_role;
          newRAMA.e_deep      =0;
          estrP.ESTR_ESTR.Add(newRAMA);

          //la agrego a la coleccion de ramas agregadas
          ESTR[newRAMA.c_estructura]=1;
          isUnique=roleAux.l_unique;
        }

        //Agrego las RAMAS PADRES
        estrParent=estrO; deep=0;
        do
        {
          deep--;

          estrParent=(ESTRUCTURA)orgNew.ESTRUCTURA.GetByAttribute("c_estructura", estrParent.c_estr_padre);
          if (estrParent!=null)
          {
            roleAux=(ROLE)orgNew.ROLES.GetByAttribute("c_role", estrParent.c_role);
            if (roleAux!=null)
            {
              NomadLog.Info("--Agregando la Estructura PADRE '"+estrParent.c_estructura+"-"+estrParent.c_role+"'....");

              //Agrego la RAMA PADRE
              newRAMA=new ESTR_RAMA();
              newRAMA.c_estructura=estrParent.c_estructura;
              newRAMA.c_role      =estrParent.c_role;
              newRAMA.e_deep      =deep;
              estrP.ESTR_ESTR.Add(newRAMA);

              //la agrego a la coleccion de ramas agregadas
              ESTR[newRAMA.c_estructura]=1;
            }
          }
        } while (estrParent!=null);

        //Agrego las ESTRUCTURAS HIJAS
        ArrayList arrParent=new ArrayList();
        arrParent.Add(estrO);

        ArrayList arrParentDeep=new ArrayList();
        arrParentDeep.Add(0);

        for (int t=0; t<arrParent.Count; t++)
        {
          estrParent=(ESTRUCTURA)arrParent[t];
          deep      =(int)arrParentDeep[t];
          deep++;

          //Busco las estructuras
          foreach (ESTRUCTURA estrCHILD in orgNew.ESTRUCTURA)
          if (estrCHILD.c_estr_padre==estrParent.c_estructura)
          {
            arrParent.Add(estrCHILD);
            arrParentDeep.Add(deep);

            roleAux=(ROLE)orgNew.ROLES.GetByAttribute("c_role", estrCHILD.c_role);
            if (roleAux!=null)
            {
              NomadLog.Info("--Agregando la Estructura HIJA '"+estrCHILD.c_estructura+"-"+estrCHILD.c_role+"'....");

              //Agrego la HIJA
              newRAMA=new ESTR_RAMA();
              newRAMA.c_estructura=estrCHILD.c_estructura;
              newRAMA.c_role      =estrCHILD.c_role;
              newRAMA.e_deep      =deep;
              estrP.ESTR_ESTR.Add(newRAMA);

              //la agrego a la coleccion de ramas agregadas
              ESTR[newRAMA.c_estructura]=1;
            }
          }
        }

        //Agrego las RAMAS UNIQUE
        if (!isUnique)
        foreach (ROLE roleO in orgNew.ROLES)
        {
          if (!roleO.l_unique) continue;

          estrParent=(ESTRUCTURA)orgNew.ESTRUCTURA.GetByAttribute("c_role", roleO.c_role);
          if (estrParent!=null)
          {
            if (ESTR.ContainsKey(estrParent.c_estructura)) continue;
            NomadLog.Info("--Agregando la Estructura UNICA '"+estrParent.c_estructura+"-"+estrParent.c_role+"'....");

            //Agrego la RAMA PADRE
            newRAMA=new ESTR_RAMA();
            newRAMA.c_estructura=estrParent.c_estructura;
            newRAMA.c_role      =estrParent.c_role;
            estrP.ESTR_ESTR.Add(newRAMA);
          }
        }

        //Guardo la Estructura
        NomadEnvironment.GetCurrentTransaction().Save(estrP);
      }
    }

    public static void EstructuraDEL(int oi_organigrama, int oi_estructura)
    {
        //Cargo el Organigrama
        ORGANIGRAMA org = ORGANIGRAMA.Get(oi_organigrama, false);

        //Borro la Estructura
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA ESTR = (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA)org.ESTRUCTURA.GetById(oi_estructura.ToString());
        ESTR.ENTIDAD_ESTR.Clear();
        org.ESTRUCTURA.Remove(ESTR);

        //Limpio el Organigrama
        EstructuraPURGE(ref org);

        //Guardo el Organigrama
        NomadEnvironment.GetCurrentTransaction().Save(org);
    }

    public static void EstructuraPASTE(int oi_organigrama, int oi_estructura, string c_estr_padre, int e_order)
    {
		NomadXML xmlParam = new NomadXML("PARAM");
		xmlParam.SetAttr("oi_estructura", oi_estructura);
		xmlParam.SetAttr("oi_organigrama", oi_organigrama);
		xmlParam.SetAttr("c_estr_padre", c_estr_padre);
		NomadXML xmlValid = NomadProxy.GetProxy().SQLService().GetXML(NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Resources.ValidCutPaste,xmlParam.ToString());

		if (Nomad.NSystem.Functions.StringUtil.str2int(xmlValid.GetAttr("valid")) == 0)
		{
			return;
		}
		
		//Cargo el Organigrama
        ORGANIGRAMA org = ORGANIGRAMA.Get(oi_organigrama, false);        
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA est = (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA)org.ESTRUCTURA.GetById(oi_estructura.ToString());
		
        //Cambio los datos
        est.c_estr_padre = c_estr_padre;
        est.e_orden = e_order;

        //Guardo el Organigrama
		NomadEnvironment.GetCurrentTransaction().Save(org);
    }

    public static void EstructuraCOPY(int oi_organigrama, int oi_estructura, string c_estr_padre, int e_order)
    {
        //Cargo el Organigrama
        ORGANIGRAMA org = ORGANIGRAMA.Get(oi_organigrama, false);
        NomadObject est = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(oi_estructura).Duplicate();

        int max = 0;
        foreach (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA cur in org.ESTRUCTURA)
        {
            if (Nomad.NSystem.Functions.StringUtil.str2int(cur.c_estructura) > max)
                max = Nomad.NSystem.Functions.StringUtil.str2int(cur.c_estructura);
        }

        //Cambio los datos
        est.SetAttr("c_estr_padre", c_estr_padre);
        est.SetAttr("e_orden", e_order);
        est.SetAttr("c_estructura", (max + 1).ToString());
        NomadLog.Debug(est.ToString());

        //Agrego la Estructura
        org.ESTRUCTURA.Add(est);

        //Limpio el Organigrama
        EstructuraPURGE(ref org);

        //Guardo el Organigrama
        NomadEnvironment.GetCurrentTransaction().Save(org);
    }

    public static void EstructuraLEFT(int oi_organigrama, int oi_estructura)
    {

    NomadXML RS = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.POS", "<PARAMS oi_organigrama=\"" + oi_organigrama + "\" oi_estructura=\"" + oi_estructura + "\" />");
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA ESTR;

        int prevID, prevOR, actID, actOR;

        prevID = 0;
        prevOR = 0;
        oi_estructura = RS.GetAttrInt("oi_estructura");

        for (NomadXML cur = RS.FirstChild(); cur != null; cur = cur.Next())
        {
            actID = cur.GetAttrInt("oi_estructura");
            actOR = cur.GetAttrInt("e_orden");

            if (actID == oi_estructura)
            {
                if (prevID == 0)
                    break;

                //Actualizo el Orden Actual
                ESTR = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(actID.ToString());
                ESTR.e_orden = prevOR;
                NomadEnvironment.GetCurrentTransaction().Save(ESTR);

                //Actualizo del Previo
                ESTR = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(prevID.ToString());
                ESTR.e_orden = actOR;
                NomadEnvironment.GetCurrentTransaction().Save(ESTR);
            }

            prevID = actID;
            prevOR = actOR;
        }
    }

    public static void EstructuraRIGHT(int oi_organigrama, int oi_estructura)
    {

    NomadXML RS = NomadProxy.GetProxy().SQLService().GetXML("CLASS.NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.POS", "<PARAMS oi_organigrama=\"" + oi_organigrama + "\" oi_estructura=\"" + oi_estructura + "\" />");
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA ESTR;

        int nextID, nextOR, actID, actOR;

        oi_estructura = RS.GetAttrInt("oi_estructura");
        for (NomadXML cur = RS.FirstChild(); cur != null && cur.Next() != null; cur = cur.Next())
        {
            actID = cur.GetAttrInt("oi_estructura");
            actOR = cur.GetAttrInt("e_orden");

            nextID = cur.Next().GetAttrInt("oi_estructura");
            nextOR = cur.Next().GetAttrInt("e_orden");

            if (actID == oi_estructura)
            {
                //Actualizo el Orden Actual
                ESTR = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(actID.ToString());
                ESTR.e_orden = nextOR;
                NomadEnvironment.GetCurrentTransaction().Save(ESTR);

                //Actualizo del Proximo
                ESTR = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(nextID.ToString());
                ESTR.e_orden = actOR;
                NomadEnvironment.GetCurrentTransaction().Save(ESTR);
            }
        }
    }

    public static void EstructuraPURGE(ref NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA ORG)
    {
        ArrayList MyDELETEList = new ArrayList(); ;

        //////////////////////////////////////////////////////////////////////////////////////
        //Busco Estrucuturas sin Padres
        do
        {
            MyDELETEList.Clear();

            //recorro la lista de estructuras y verifico cuales no tienen mas padre
            foreach (ESTRUCTURA est in ORG.ESTRUCTURA)
            {
                if (est.c_estr_padre == "") continue;

                if (ORG.ESTRUCTURA.GetByAttribute("c_estructura", est.c_estr_padre) == null)
                    MyDELETEList.Add(est);
            }

            //Elimino las estructuras listadas
            foreach (ESTRUCTURA estdel in MyDELETEList)
            {
                estdel.ENTIDAD_ESTR.Clear();
                ORG.ESTRUCTURA.Remove(estdel);
            }

        } while (MyDELETEList.Count > 0);

        //////////////////////////////////////////////////////////////////////////////////////
        //Busco Nodos Ficticios sin CHILDs
        do
        {
            MyDELETEList.Clear();

            //recorro la lista de estructuras y verifico cuales no tienen mas padre
            foreach (ESTRUCTURA est in ORG.ESTRUCTURA)
            {
                if (est.c_role != "*") continue;

                if (ORG.ESTRUCTURA.GetByAttribute("c_estr_padre", est.c_estructura) == null)
                    MyDELETEList.Add(est);
            }

            //Elimino las estructuras listadas
            foreach (ESTRUCTURA estdel in MyDELETEList)
            {
                estdel.ENTIDAD_ESTR.Clear();
                ORG.ESTRUCTURA.Remove(estdel);
            }

        } while (MyDELETEList.Count > 0);
    }

    public static void EstructuraDOWN(int oi_organigrama, int oi_estructura)
    {
        //Cargo el Organigrama
        ORGANIGRAMA org = ORGANIGRAMA.Get(oi_organigrama, false);

        //Busco el Maximo ID
        int max = 0;
        foreach (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA cur in org.ESTRUCTURA)
        {
            if (Nomad.NSystem.Functions.StringUtil.str2int(cur.c_estructura) > max)
                max = Nomad.NSystem.Functions.StringUtil.str2int(cur.c_estructura);
        }

        //Estructuras
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA ESTR = (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA)org.ESTRUCTURA.GetById(oi_estructura.ToString());
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA FICT = new NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA();

        //Agrego la Ficticia
        FICT.c_estructura = (max + 1).ToString();
        FICT.d_estructura = "**ficticia**";
        FICT.c_role = "*";
        FICT.l_inicia = false;
        FICT.e_orden = ESTR.e_orden;
        FICT.c_estr_padre = ESTR.c_estr_padre;
        org.ESTRUCTURA.Add(FICT);

        //Modifico la Actual
        ESTR.c_estr_padre = (max + 1).ToString();
        ESTR.e_orden = 65536;

        //Limpio el Organigrama
        EstructuraPURGE(ref org);

        //Guardo el Organigrama
        NomadEnvironment.GetCurrentTransaction().Save(org);
    }

    public static void EstructuraUP(int oi_organigrama, int oi_estructura)
    {
        //Cargo el Organigrama
        ORGANIGRAMA org = ORGANIGRAMA.Get(oi_organigrama, false);

        //Estructuras
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA ESTR = (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA)org.ESTRUCTURA.GetById(oi_estructura.ToString());
        if (ESTR.c_estr_padre == "")
        {
            NomadLog.Debug("ESTR.c_estr_padre is NULL");
            return;
        }

        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA FICT = (NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA)org.ESTRUCTURA.GetByAttribute("c_estructura", ESTR.c_estr_padre);
        if (FICT == null)
        {
            NomadLog.Debug("org.ESTRUCTURA.GetByAttribute(c_estr_padre," + ESTR.c_estr_padre + ")  is NULL");
            return;
        }
        if (FICT.c_role != "*")
        {
            NomadLog.Debug("FICT.c_role = " + FICT.c_role);
            return;
        }		
		
		NomadXML xmlParam = new NomadXML("PARAM");
		xmlParam.SetAttr("c_estructura", FICT.c_estructura);
		xmlParam.SetAttr("oi_organigrama", oi_organigrama);
		NomadXML xmlCount = NomadProxy.GetProxy().SQLService().GetXML(NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Resources.CountChilds,xmlParam.ToString());

		if (Nomad.NSystem.Functions.StringUtil.str2int(xmlCount.GetAttr("count")) > 1)
		{
			return;
		}		
		
        //Modifico la Actual
        ESTR.c_estr_padre = FICT.c_estr_padre;
        ESTR.e_orden = FICT.e_orden;

        //Elimino la Ficticia
        org.ESTRUCTURA.Remove(FICT);

        //Limpio el Organigrama
        EstructuraPURGE(ref org);

        //Guardo el Organigrama
        NomadEnvironment.GetCurrentTransaction().Save(org);
    }

    public static void EstructuraADDDEL(int oi_estructura, string oi_entidades)
    {
        //Cargo la Estructura
        NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA ESTR = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(oi_estructura.ToString());
        NucleusWF.Base.Definicion.Organigramas.ENTIDAD_ESTR PERS;
        NomadXML MyXML;

        //Cargo la Hash con las personas
        Hashtable toDelete = new Hashtable();
        foreach (NucleusWF.Base.Definicion.Organigramas.ENTIDAD_ESTR CUR in ESTR.ENTIDAD_ESTR)
            toDelete[CUR.oi_entidad] = CUR;

        //Recorro las entidades y agrego o las quito de la hash
        if (oi_entidades != "")
        {
            string[] lst = oi_entidades.Split(',');
            for (int t = 0; t < lst.Length; t++)
            {
                if (toDelete.ContainsKey(lst[t]))
                    toDelete.Remove(lst[t]);
                else
                {
                    MyXML = NomadProxy.GetProxy().SQLService().GetXML(
                        @"<qry:include name=""Nomad.Base.Login.Entidades.ENTIDAD.INFO_IDS_LIST""/>" +
                        @"<qry:main maxdeep=""2"" doc=""PARAMIN"">" +
                        @"  <ROWS>" +
                        @"   <qry:insert-select name=""sql1"" doc-path=""#PARAMIN:/PARAM""/>" +
                        @"  </ROWS>" +
                        @"</qry:main>",
                        @"<PARAM IDS=""" + lst[t] + @""" />"
                    );

                    //Agrego la entidad
                    PERS = new NucleusWF.Base.Definicion.Organigramas.ENTIDAD_ESTR();
                    PERS.oi_entidad = lst[t];

                    if (MyXML != null) MyXML = MyXML.FirstChild();
                    if (MyXML != null && MyXML.GetAttr("COD") != "") PERS.d_entidad_estr = "[" + MyXML.GetAttr("COD") + "] " + MyXML.GetAttr("DES");
                    else PERS.d_entidad_estr = "Desconocido";

                    ESTR.ENTIDAD_ESTR.Add(PERS);
                }
            }
        }

        //Elimino los ToDelete
        foreach (string key in toDelete.Keys)
            ESTR.ENTIDAD_ESTR.Remove((NomadObject)toDelete[key]);

        //Guardo el Organigrama
        NomadEnvironment.GetCurrentTransaction().Save(ESTR);
    }

    public static void Publicar(int oi_organigrama)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Publicar", "Publicar");

        //Cargo el Organigrama
        MyBATCH.SetMess("Copiando NODO Principal....");
        MyBATCH.Log("Copiando NODO Principal....");
        MyBATCH.SetPro(10);
        ORGANIGRAMA orgO = ORGANIGRAMA.Get(oi_organigrama, false);
        orgO.e_version = orgO.e_version + 1;

        //Creo el Organigrama Original
        ORGANIGRAMA orgP = new ORGANIGRAMA();
        orgP.c_organigrama = orgO.c_organigrama;
        orgP.d_organigrama = orgO.d_organigrama;
        orgP.e_version = orgO.e_version - 1;
        orgP.e_version_pub = orgO.e_version - 1;
        orgP.l_automatica = true;

        //Agrego los Roles
        Hashtable RoleCount = new Hashtable();
        MyBATCH.SetMess("Copiando ACTORES/ROLES....");
        MyBATCH.Log("Copiando ACTORES/ROLES....");
        MyBATCH.SetPro(15);
        foreach (ROLE roleO in orgO.ROLES)
        {
            NomadLog.Info("Agregando el Role '" + roleO.c_role + "' a la Lista....");
            RoleCount[roleO.c_role.ToUpper()] = 0;
            orgP.ROLES.Add(roleO.Duplicate());
        }
        if (orgO.ROLES.Count == 0)
        {
            NomadLog.Error("No hay ACTORES/ROLES....");
            MyBATCH.Err("No hay ACTORES/ROLES definidos....");
            return;
        }
        if (!RoleCount.ContainsKey("OWNER"))
        {
            NomadLog.Error("El Actor/Rol 'OWNER' No Encontrado....");
            MyBATCH.Err("El Actor/Rol 'OWNER' No Encontrado....");
            return;
        }
        RoleCount["*"] = 0;

        //Agrego las Estructuras
        MyBATCH.SetMess("Copiando las ESTRUCTURAS....");
        MyBATCH.SetPro(20);
        MyBATCH.Log("Copiando las ESTRUCTURAS....");
        foreach (ESTRUCTURA estrO in orgO.ESTRUCTURA)
        {
            if (!RoleCount.ContainsKey(estrO.c_role.ToUpper()))
            {
                NomadLog.Error("Role '" + estrO.c_role + "' No Encontrado....");
                MyBATCH.Err("Existen Nodos con ACTORES/ROLES mal configurados....");
                return;
            }

            NomadLog.Info("Incrementando el Role '" + estrO.c_role + "'....");
            RoleCount[estrO.c_role.ToUpper()] = ((int)RoleCount[estrO.c_role.ToUpper()]) + 1;
            orgP.ESTRUCTURA.Add(estrO.Duplicate());
        }
        if (orgO.ESTRUCTURA.Count == 0)
        {
            NomadLog.Error("No hay Nodos definidos....");
            MyBATCH.Err("No hay NODOS definidos....");
            return;
        }

        //Validando los ACTORES
        foreach (ROLE roleO in orgO.ROLES)
        {
            if (roleO.c_role.ToUpper() == "OWNER" && ((int)RoleCount[roleO.c_role.ToUpper()]) > 0)
            {
                MyBATCH.Err("El Actor [" + roleO.c_role + "] " + roleO.d_role + " no puede ser asignado a un NODO....");
                return;
            }
            else
                if (roleO.l_unique && ((int)RoleCount[roleO.c_role.ToUpper()]) > 1)
                {
                    MyBATCH.Err("El Actor [" + roleO.c_role + "] " + roleO.d_role + " no puede estar en mas de un NODO....");
                    return;
                }
        }

        //Guardando la Estructura Principal
        MyBATCH.SetMess("Guardando las ESTRUCTURAS Y ACTORES....");
        MyBATCH.Log("Guardando las ESTRUCTURAS Y ACTORES....");
        MyBATCH.SetPro(25);
        NomadEnvironment.GetCurrentTransaction().Begin();
        NomadEnvironment.GetCurrentTransaction().SaveRefresh(orgO);
        NomadEnvironment.GetCurrentTransaction().SaveRefresh(orgP);
        NomadEnvironment.GetCurrentTransaction().Commit();

        //Cargando las Personas....
        MyBATCH.SetMess("Copiando las PERSONAS....");
        MyBATCH.Log("Copiando las PERSONAS....");
        MyBATCH.SetPro(30);
        int tot = orgO.ESTRUCTURA.Count, cnt = 0;
        foreach (ESTRUCTURA estrO in orgO.ESTRUCTURA)
        {
            cnt++; MyBATCH.SetPro(30, 50, tot, cnt);
            if (estrO.ENTIDAD_ESTR.Count == 0) continue;

            //Obtengo la estructura
            ESTRUCTURA estrP = NucleusWF.Base.Definicion.Organigramas.ESTRUCTURA.Get(orgP.ESTRUCTURA.GetByAttribute("c_estructura", estrO.c_estructura).Id, true);
            foreach (ENTIDAD_ESTR perO in estrO.ENTIDAD_ESTR)
                estrP.ENTIDAD_ESTR.Add(perO.Duplicate());

            //Guardo la Estructura
            NomadEnvironment.GetCurrentTransaction().Save(estrP);
        }

        //Calculando las Estructuras Relacionadas....
        MyBATCH.SetSubBatch(50, 70);
        CalcularRelaciones(orgP);

        //Publicando
        MyBATCH.Log("Publicando....");
        orgO.e_version_pub = orgP.e_version;
        NomadEnvironment.GetCurrentTransaction().SaveRefresh(orgO);

    //Actualizando Instancias....
    MyBATCH.SetMess("Actualizando Instancias....");
    MyBATCH.Log("Actualizando Instancias....");
    MyBATCH.SetPro(75);

    //Consulta de Instancias a Republicar
    NomadXML PARAM = new NomadXML("PARAM");
    PARAM.SetAttr("c_organigrama",orgO.c_organigrama);
    PARAM.SetAttr("e_version",orgO.e_version_pub);
    NomadXML INSTs = NomadEnvironment.QueryNomadXML("CLASS.NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA.QRY_INSTANCIAS", PARAM.ToString(), false);
    if (INSTs.isDocument) INSTs = INSTs.FirstChild();

    //Recorro las instancias
    tot = INSTs.ChildLength; cnt = 0;
    for (NomadXML INST=INSTs.FirstChild(); INST!=null; INST=INST.Next())
    {
      cnt++; MyBATCH.SetPro(75, 90, tot, cnt);
      try {
        NucleusWF.Base.Ejecucion.Instancias.INSTANCE.RefreshThreadVersion(INST.GetAttrInt("oi_instance"), INST.GetAttrInt("oi_thread"));
      } catch(Exception) { }
    }
  }

    public static bool Verificar(string oi_organigrama)
    {
        NomadBatch MyBATCH = NomadBatch.GetBatch("Verificar", "Verificar");

        //Cargo el Organigrama
        MyBATCH.SetMess("Verificando el NODO Principal....");
        MyBATCH.SetPro(10);
        MyBATCH.Log("Verificando NODO Principal....");

        string oi_org_pub;
        ORGANIGRAMA orgO;
        ORGANIGRAMA orgP;
        bool retval = true;
        int tot, cnt;

        //Organigrama Principal
        orgO = ORGANIGRAMA.Get(oi_organigrama, false);
        oi_org_pub = NomadEnvironment.QueryValue("WRK05_ORGANIGRAMAS", "oi_organigrama", "c_organigrama", orgO.c_organigrama, "WRK05_ORGANIGRAMAS.l_automatica=1 and WRK05_ORGANIGRAMAS.e_version=" + orgO.e_version_pub, false);
        if (oi_org_pub == null)
        {
            MyBATCH.Wrn("No existe ninguna version publicada del Organigrama.");
            return false;
        }

        //Organigrama Publicado
        orgP = ORGANIGRAMA.Get(oi_org_pub, false);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Verificando los ROLES
        Hashtable Roles = new Hashtable();
        MyBATCH.SetMess("Verificando ACTORES....");
        MyBATCH.Log("Verificando ACTORES....");

        //Cargo los Roles Originales
        MyBATCH.SetPro(20);
        foreach (ROLE roleO in orgO.ROLES)
            Roles[roleO.c_role] = roleO;

        //Verifico con los Publicados
        MyBATCH.SetPro(25);
        foreach (ROLE roleP in orgP.ROLES)
        {
            if (Roles.ContainsKey(roleP.c_role))
            {
                ROLE roleO = (ROLE)Roles[roleP.c_role];

                if (roleO.d_role != roleP.d_role) { MyBATCH.Wrn("El Rol/Actor '" + roleP.c_role + "' es distinto al Publicado."); retval = false; continue; }
                if (roleO.l_unique != roleP.l_unique) { MyBATCH.Wrn("El Rol/Actor '" + roleP.c_role + "' es distinto al Publicado."); retval = false; continue; }
                if (roleO.d_color != roleP.d_color) { MyBATCH.Wrn("El Rol/Actor '" + roleP.c_role + "' es distinto al Publicado."); retval = false; continue; }
                Roles.Remove(roleP.c_role);
            }
            else
            {
                MyBATCH.Wrn("El Rol/Actor '" + roleP.c_role + "' no existe en el Organigrama Principal.");
                retval = false;
            }
        }
        foreach (ROLE roleO in Roles.Values)
        {
            MyBATCH.Wrn("El Rol/Actor '" + roleO.c_role + "' no existe en el Organigrama Publicado.");
            retval = false;
        }
        if (!retval)
        {
            MyBATCH.Wrn("Los Roles/Actores actuales no estan Sincronizado con el Publicado.");
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Verificando los ESTRUCTURAS
        Hashtable Estructuras = new Hashtable();
        MyBATCH.SetMess("Verificando ESTRUCTURAS....");
        MyBATCH.Log("Verificando ESTRUCTURAS....");

        //Cargo las Estructuras Originales
        MyBATCH.SetPro(30);
        foreach (ESTRUCTURA estrO in orgO.ESTRUCTURA)
            Estructuras[estrO.c_estructura] = estrO;

        //Verifico con los Publicados
        MyBATCH.SetPro(40);
        foreach (ESTRUCTURA estrP in orgP.ESTRUCTURA)
        {
            if (Estructuras.ContainsKey(estrP.c_estructura))
            {
                ESTRUCTURA estrO = (ESTRUCTURA)Estructuras[estrP.c_estructura];

                if (estrO.d_estructura != estrP.d_estructura) { MyBATCH.Wrn("La Estructura '" + estrP.c_estructura + "-" + estrP.d_estructura + "' es distinta a la Publicada."); retval = false; continue; }
                if (estrO.l_inicia != estrP.l_inicia) { MyBATCH.Wrn("La Estructura '" + estrP.c_estructura + "-" + estrP.d_estructura + "' es distinta a la Publicada."); retval = false; continue; }
                if (estrO.c_role != estrP.c_role) { MyBATCH.Wrn("La Estructura '" + estrP.c_estructura + "-" + estrP.d_estructura + "' es distinta a la Publicada."); retval = false; continue; }
                if (estrO.c_estr_padre != estrP.c_estr_padre) { MyBATCH.Wrn("La Estructura '" + estrP.c_estructura + "-" + estrP.d_estructura + "' es distinta a la Publicada."); retval = false; continue; }

                Estructuras.Remove(estrP.c_estructura);
            }
            else
            {
                MyBATCH.Wrn("La Estructura '" + estrP.c_estructura + "-" + estrP.d_estructura + "' no existe en el Organigrama Principal.");
                retval = false;
            }
        }
        foreach (ESTRUCTURA estrO in Estructuras.Values)
        {
            MyBATCH.Wrn("La Estructura '" + estrO.c_estructura + "-" + estrO.d_estructura + "' no existe en el Organigrama Publicado.");
            retval = false;
        }
        if (!retval)
        {
            MyBATCH.Wrn("Las Estructura actual no estan Sincronizada con el Publicada.");
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Verificando las PERSONAS
        Hashtable Entidades = new Hashtable();
        MyBATCH.SetMess("Verificando PERSONAS por ESTRUCTURAS....");
        MyBATCH.SetPro(50);
        MyBATCH.Log("Verificando PERSONAS por ESTRUCTURAS....");

        tot = orgO.ESTRUCTURA.Count; cnt = 0;
        foreach (ESTRUCTURA estrO in orgO.ESTRUCTURA)
        {
            cnt++; MyBATCH.SetPro(50, 90, tot, cnt);
            ESTRUCTURA estrP = (ESTRUCTURA)orgP.ESTRUCTURA.GetByAttribute("c_estructura", estrO.c_estructura);

            Entidades.Clear();

            //Cargo las Entidades Originales
            foreach (ENTIDAD_ESTR ettyO in estrO.ENTIDAD_ESTR)
                Entidades[ettyO.oi_entidad] = ettyO;

            //Verifico con los Publicados
            foreach (ENTIDAD_ESTR ettyP in estrP.ENTIDAD_ESTR)
            {
                if (Entidades.ContainsKey(ettyP.oi_entidad))
                {
                    Entidades.Remove(ettyP.oi_entidad);
                }
                else
                {
                    MyBATCH.Wrn("La Entidad '" + ettyP.d_entidad_estr + "' no existe en la Estructura '" + estrP.c_estructura + "-" + estrP.d_estructura + "' del Organigrama Principal.");
                    retval = false;
                }
            }

            foreach (ENTIDAD_ESTR ettyO in Entidades.Values)
            {
                MyBATCH.Wrn("La Entidad '" + ettyO.d_entidad_estr + "' no existe en la Estructura '" + estrO.c_estructura + "-" + estrO.d_estructura + "' del Organigrama Publicado.");
                retval = false;
            }
        }

        if (!retval)
        {
            MyBATCH.Wrn("Las Estructura actual no estan Sincronizada con el Publicada.");
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Fin
        MyBATCH.SetMess("Verificacion Completa.");
        MyBATCH.SetPro(90);
        MyBATCH.Log("Verificacion Completa.");
        return true;
    }

    public static NucleusWF.Base.Definicion.Organigramas.ORGANIGRAMA CREATE_DDO(string parent)
    {

        //Cargo el Organigrama
        ORGANIGRAMA org = new ORGANIGRAMA();

        ROLE rol = new ROLE();
        rol.c_role = "OWNER";
        rol.d_role = "Dueño/Creador";
        rol.l_unique = false;
        rol.d_color = "00FF00";
        org.ROLES.Add(rol);

        return org;
    }
  }
}


