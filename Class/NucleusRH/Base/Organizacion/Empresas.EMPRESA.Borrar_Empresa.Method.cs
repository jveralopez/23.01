try
{
	NucleusRH.Base.Organizacion.Unidades_Organizativas.UNIDAD_ORG unidad;
	NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG tipo_unidad;
	tipo_unidad= NucleusRH.Base.Organizacion.Unidades_Organizativas.TIPO_UNI_ORG.Get("1", Proxy);
	unidad = tipo_unidad.UNI_ORG.GetByAttr("c_unidad_org", this.c_empresa.TypedValue);
	unidad.Delete();
	tipo_unidad.Save();
}
catch(Exception e)
{
	throw new NomadAppException("Error Eliminando Unidad Organizativa: " + e.Message);
}