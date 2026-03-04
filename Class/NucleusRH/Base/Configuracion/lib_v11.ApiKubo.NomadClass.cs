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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using NucleusRH.Base.Personal.LegajoEmpresa;

namespace NucleusRH.Base.Configuracion.Kubo
{

    public partial class ApiKubo
    {
        private static Login login;
        private static Hashtable hashUrls = null;
        private static Dictionary<string,object> parametros = null;

        public static TipoDevolucion Enviar<TipoDevolucion>(Dictionary<string, object> datos, string codigoUrl)
        {
            if(parametros == null)
                parametros = ObtenerParametros();

            Login loginInterno = Login(parametros);

            if (loginInterno.Error == "")
            {
                string url = BuscarUrl(parametros["KUBO-VERSION"].ToString(), codigoUrl);

                datos["token"] = loginInterno.Token;
                if (loginInterno.Scheme != null && loginInterno.Scheme != "")
                    datos["scheme"] = loginInterno.Scheme;

                try
                {
                    TipoDevolucion resultado = Post<TipoDevolucion>(datos, loginInterno.Url + (loginInterno.Url.EndsWith("/") ? "" : "/") + url);
                    return resultado;
                }
                catch (Exception e)
                {

                    throw e;
                }

            }
            else
            {
              throw new Exception("Error al intentar loguearse con API_KUBO: " + loginInterno.Error);
            }

            return default(TipoDevolucion);
        }

        private static string BuscarUrl(string version, string url)
        {
            if(!url.Contains("/"))
            {
                if (hashUrls == null)
                {
                    hashUrls = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Configuracion.Kubo.ApiKubo.Resources.QRY_HASH, "<DATA urlBase='" + version + "' />", "cod","url",true);
                }
                url = hashUrls[url].ToString();
            }
            return url;
        }

        public static TipoDevolucion Enviar<TipoDevolucion>(NomadXML datos, string codigoUrl)
        {
            Dictionary<string, object> datosConvetidos = ConvertirADiccionario(datos);
            return Enviar<TipoDevolucion>(datosConvetidos, codigoUrl);
        }

        public static TipoDevolucion Enviar<TipoDevolucion>(string query, string parametros, string codigoUrl)
        {
            NomadXML resQuery = NomadEnvironment.QueryNomadXML(query, parametros, false);
            return Enviar<TipoDevolucion>(resQuery, codigoUrl);
        }

        private static Dictionary<string, object> ConvertirADiccionario(NomadXML datos)
        {
            ICollection atributos = datos.GetAttrs();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (string key in atributos)
            {
                if (datos.GetAttr(key) != "" && key != "TYPE")
                {
                    string keyConvertida = ToCamelCase(key);
                    if (datos.GetAttr("type") == "int")
                        dic.Add(keyConvertida, datos.GetAttrInt(key));
                    else
                        dic.Add(keyConvertida, datos.GetAttr(key));
                }
            }

            for (NomadXML xmlElemento = datos.FirstChild(); xmlElemento != null; xmlElemento = xmlElemento.Next())
            {
                dic.Add(xmlElemento.Name, ConvertirADiccionario(xmlElemento));
            }
            return dic;
        }

        private static string ToCamelCase(string key)
        {
            return Regex.Replace(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key.ToLower()), "_[aA-zZ]", delegate(Match m)
            {
                return m.ToString().TrimStart('_').ToUpper();
            });
        }

        public static NomadXML LoginUsuario(NomadXML xmlParam)
        {
            xmlParam = xmlParam.FirstChild();
            string email = xmlParam.GetAttr("email");
            string cuil = xmlParam.GetAttr("cuil");

            if(parametros == null)
                parametros = ObtenerParametros();

            string tiempoAhora = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            List<string> listaParametros = new List<string>();
            listaParametros.Add(parametros["KUBO-CLAVE-PUBLICA"].ToString());
            listaParametros.Add(tiempoAhora);
            listaParametros.Add(parametros["KUBO-CLAVE-PRIVADA"].ToString());
            listaParametros.Add(cuil);
            listaParametros.Add(email);

            Dictionary<string, object> dicLogin = new Dictionary<string, object>();
            dicLogin.Add("Public", parametros["KUBO-CLAVE-PUBLICA"].ToString());
            dicLogin.Add("Time", tiempoAhora);
            dicLogin.Add("User", cuil);
            dicLogin.Add("Mail", email);

            Login login = HacerLogueoApi(dicLogin, new HashParametros(listaParametros), parametros["KUBO-URL"].ToString());
            NomadLog.Debug("URL:" + login.Url);

            NomadXML xmlRespuesta = new NomadXML("DATA");

            xmlRespuesta.SetAttr("url",login.Url);
            xmlRespuesta.SetAttr("error", login.Error != "" ? "NDOC: " + login.Error + ". Por favor, comunicarse con el área de RRHH." : "");

            return xmlRespuesta;
        }

        public static Login Login(Dictionary<string,object> parametros)
        {
            if (login != null && LoginSinExpirar(login.Expired))
                return login;

            LoginRegistro loginRegistro = BuscarLoginMemoria();

            if (loginRegistro != null)
            {
                login = new Login();
                login.Token = loginRegistro.Token;
                login.Expired = loginRegistro.Expired;
                login.Url = loginRegistro.Url;
                login.Scheme = loginRegistro.Scheme;
            }
            else
            {
                string tiempoAhora = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                List<string> listaParametros = new List<string>();
                listaParametros.Add(parametros["KUBO-CLAVE-PUBLICA"].ToString());
                listaParametros.Add(tiempoAhora);
                listaParametros.Add(parametros["KUBO-CLAVE-PRIVADA"].ToString());

                Dictionary<string, object> dicLogin = new Dictionary<string, object>();
                dicLogin.Add("Public", parametros["KUBO-CLAVE-PUBLICA"].ToString());
                dicLogin.Add("Time", tiempoAhora);
                login = HacerLogueoApi(dicLogin, new HashParametros(listaParametros), parametros["KUBO-LOGIN-URL"].ToString());

                if (login.Error == "")
                {
                    if (loginRegistro == null)
                        loginRegistro = new LoginRegistro();
                    loginRegistro.COD = "1";
                    loginRegistro.Token = login.Token;
                    loginRegistro.Expired = login.Expired;
                    loginRegistro.Url = login.Url;
                    loginRegistro.Scheme = login.Scheme;

                    NomadEnvironment.GetCurrentTransaction().Save(loginRegistro);
                }

            }

            return login;
        }

        private static Login HacerLogueoApi(Dictionary<string, object> dicLogin, HashParametros hash,string url)
        {
            dicLogin.Add("Hash", hash.GenerarHash());

            try
            {
                login = Post<Login>(dicLogin, url);
            }
            catch (Exception e)
            {
                throw e;
            }
            return login;
        }

        private static bool LoginSinExpirar(string fechaExpiracion)
        {
            return Convert.ToDateTime(fechaExpiracion).ToUniversalTime().AddMinutes(-10) > DateTime.UtcNow;
        }

        private static LoginRegistro BuscarLoginMemoria()
        {
            LoginRegistro loginRegistro = null;

            try
            {
                loginRegistro = LoginRegistro.Get("1");
            }
            catch (Exception e)
            {
            }

            return loginRegistro != null && LoginSinExpirar(loginRegistro.Expired) ? loginRegistro : null;
        }

        private static TipoDevolucion Post<TipoDevolucion>(Dictionary<string, object> diccionario, string url)
        {
            try
            {
                string json;
                //0x30=SecurityProtocolType.ssl3 | SecurityProtocolType.Tls | 0x300= SecurityProtocolType.Tls11 |0xc00= SecurityProtocolType.Tls12
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0x30 | 0xc0 | 0x300 | 0xc00);

        NomadLog.Info("<API> URL POST: " + url);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Method = "POST";

                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    json = StringUtil.Object2JSON(diccionario);
                    streamWriter.Write(json);
                }
        NomadLog.Info("<API> JSON POST: " + HideToken(json));

                string result = "";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
          NomadLog.Info("<API> JSON RESULT: " + result);
                }

                return StringUtil.JSON2Object<TipoDevolucion>(result);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static string HideToken(string json) {return HideValueOfProp(json, "\"token\":\""); }

        public static string HideValueOfProp(string json, string property)
        {
            string tmp = @json.ToLower();
            string result = @json;

      try
            {
        int ini = tmp.IndexOf(property);

        if  (ini > -1)
        {
          int end = tmp.IndexOf("\"",ini + property.Length);
          if (end >-1)
          {
            result = result.Substring(0,ini + property.Length) + "*****" +  result.Substring(end);
          }
        }
      }
            catch(Exception e) {}

            return result;
        }

        public static Dictionary<string,object> ObtenerParametros()
        {
            List<string> ParametrosABuscar = new List<string>();
            ParametrosABuscar.Add("KUBO-LOGIN-URL");
            ParametrosABuscar.Add("KUBO-CLAVE-PUBLICA");
            ParametrosABuscar.Add("KUBO-CLAVE-PRIVADA");
            ParametrosABuscar.Add("KUBO-URL");
            ParametrosABuscar.Add("KUBO-VERSION");
            //Pide el context para obtener los parametros
            NomadXML Ctx = NomadProxy.GetProxy().ReadContext();
            NomadXML Apps = Ctx.FindElement("apps");

            NomadXML instance = null;
            for (NomadXML ap = Apps.FirstChild(); ap != null; ap = ap.Next())
            {
                for (NomadXML ins = ap.FirstChild(); ins != null; ins = ins.Next())
                {
                    if (ins.GetAttr("id").ToUpper() == NomadProxy.GetProxy().AppName.ToUpper())
                    {
                        instance = ins;
                        break;
                    }
                }
                if (instance != null)
                    break;
            }

            Dictionary<string,object> parametros = new Dictionary<string,object>();

            if (instance != null)
            {
                ICollection atributos = instance.GetAttrs();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                string error = "";
                foreach (string key in atributos)
                {
                    if (ParametrosABuscar.Contains(key) && instance.GetAttr(key) != "")
                    {
                        //NomadLog.Debug("PARAMETER:" + "Name=" + key + " - Value=" + instance.GetAttr(key));
                        parametros.Add(key, instance.GetAttr(key));
                        ParametrosABuscar.Remove(key);
                    }
                }

                foreach(string faltante in ParametrosABuscar)
                    error += "Falta configurar el parametro " + faltante + " en el NDK \r\n";

                if (error != "") throw NomadException.NewMessage("Kubo.Kubo.ERR-PARAM", error);
            }
            return parametros;
        }

        public class HashParametros
        {
            List<string> parametros;
            public HashParametros(List<string> parametros)
            {
                this.parametros = parametros;
            }

            public string GenerarHash()
            {
                string cadenaIntermedia ="";
                foreach(string p in parametros)
                    cadenaIntermedia += p + ":";
                cadenaIntermedia = cadenaIntermedia.Substring(0,cadenaIntermedia.Length - 1);

                byte[] bytes = sha256(cadenaIntermedia);
                return System.Convert.ToBase64String(bytes);
            }

            private byte[] sha256(string cadena)
            {
                return new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(cadena));
            }
        }

        public static void EnviarEmpleados()
        {
            NomadBatch MyBatch = NomadBatch.GetBatch("Envio de Empleados a NDOC", "Envio de Empleados a NDOC");

            if(ProcesoBloquado(MyBatch)) return;

            MyBatch.Log("El dato de mail se enviara hasta que el usuario ingrese por primera vez a Nucleus Doc");

            int nuevos = 0, actualizados = 0, errores = 0;
            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            dic2.Add("Ignorar", 0);
            string param = "";
            ListaResultado<Empresa> listaEmpresas =  Enviar<ListaResultado<Empresa>>(dic2, "listarEmpresa");
            foreach (Empresa e in listaEmpresas.Registros)
            {
                param += "<ROW codigo=\"\\\'" + e.Codigo + "\\\'\" />";
            }

            NomadXML legajosAEnviar = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Configuracion.Kubo.ApiKubo.Resources.QRY_LEGAJOS, "<DATA>" + param + "</DATA>").FirstChild();
            Personal.LegajoEmpresa.PERSONAL_EMP legajo;
            int i = 0;
            for (NomadXML xmLeg = legajosAEnviar.FirstChild(); xmLeg != null; xmLeg = xmLeg.Next())
            {
                i++;
                MyBatch.SetPro(10, 90, legajosAEnviar.ChildLength, i);
                string hash = GenerarHashLegajo(xmLeg);

                if (hash != xmLeg.GetAttr("c_hashdoc"))
                {
                    //CrearImagenBase64(xmLeg);
                    Resultado resultadoLegajo;
                    try
                    {
                        resultadoLegajo = Enviar<Resultado>(xmLeg, "crearEmpleado");
                    }
                    catch (Exception e)
                    {
                        MyBatch.Err("Error al enviar empleado CUIL: " + xmLeg.GetAttr("Cuil") + " - " + e.Message);
                        continue;
                    }

                    if ((Acciones)int.Parse(resultadoLegajo.Action) == Acciones.Error)
                    {
                        MyBatch.Err(resultadoLegajo.Description.ES);
                        errores++;
                        continue;
                    }
                    if ((Acciones)int.Parse(resultadoLegajo.Action) == Acciones.Insert)
                    {
                        MyBatch.Log("Se Creo el Empleado de CUIL " + xmLeg.GetAttr("Cuil"));
                        nuevos++;
                    }
                    if ((Acciones)int.Parse(resultadoLegajo.Action) == Acciones.Update)
                    {
                        MyBatch.Log("Se Actualizo el Empleado de CUIL " + xmLeg.GetAttr("Cuil"));
                        actualizados++;
                    }

                    try
                    {
                        legajo = Personal.LegajoEmpresa.PERSONAL_EMP.Get(xmLeg.GetAttr("oi_personal_emp"), false);
                        legajo.c_hashdoc = hash;
                        NomadEnvironment.GetCurrentTransaction().Save(legajo);
                    }
                    catch(Exception e)
                    {
                        MyBatch.Err("Error al guardar el Empleado de CUIL: " + xmLeg.GetAttr("Cuil") + " " + e.Message);
                    }
                }

            }

            MyBatch.Log("Empleados creados: " + nuevos);
            MyBatch.Log("Empleados actualizados: " + actualizados);
            MyBatch.Log("Empleados con error: " + errores);
            MyBatch.SetPro(100);
            MyBatch.Log("Proceso Finalizado");
        }

        public static void ObtenerLicencias_AUTO()
        {
            NomadBatch objBatch = NomadBatch.GetBatch("Obtener licencias de vacaciones", "Obtener licencias de vacaciones");

            objBatch.SetPro(0);

            string d_valor = ObtenerParametro("f_ult_cambio_lic");
            DateTime f_desde = d_valor != "" ? DateTime.ParseExact(d_valor, "ddMMyyyy", CultureInfo.InvariantCulture) : new DateTime(1899, 01, 01);

            NomadXML xmlParam = new NomadXML("<DATOS fecha='" + f_desde.ToString("yyyyMMdd")+"'  />");

            ObtenerLicencias(xmlParam);
            GuardarParametro("PER","FEC","F","f_ult_cambio_lic", DateTime.Now.ToString("ddMMyyyy"));
            objBatch.SetPro(100);
        }
        //cargar en parametro codRH=codDOC;docRH=codDOC;

        //busca un patron adentro de una cadena de texto sin importar si está en mayúscula o minúscula
        public static bool ICContains(string cadena, string patron)
        {
           return (cadena.IndexOf(patron, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static bool ModuleExist(string Module)
        {
            NomadXML xmlConfig = NomadProxy.GetProxy().ReadCfgApplication();

            if (xmlConfig.isDocument) xmlConfig = xmlConfig.FirstChild();

            NomadXML xmlModules = xmlConfig.FindElement("build-modules");

            for (NomadXML xmlModule = xmlModules.FirstChild(); xmlModule != null; xmlModule = xmlModule.Next())
            {
              string ModuleName = xmlModule.GetAttr("name");

              if (ICContains(ModuleName, Module))
        {
          return true;
        }
            }
            return false;
        }

        public static void ObtenerLicencias(NomadXML xmlParam)
        {
            xmlParam = xmlParam.FirstChild();
            DateTime fecha = xmlParam.GetAttrDateTime("fecha");
            Fecha fechaCambio = new Fecha();
            fechaCambio.Day = fecha.Day;
            fechaCambio.Month = fecha.Month;
            fechaCambio.Year = fecha.Year;

            NomadBatch objBatch = NomadBatch.GetBatch("Obtener licencias de vacaciones", "Obtener licencias de vacaciones");

            Hashtable hashLicencia = ObtenerHashLicencias(objBatch);
           if (hashLicencia.Count == 0)
            {
                objBatch.Wrn("No se logró cargar el [hashLicencia] que debe contener los códigos de licencias para poder validar las Licencias a importar.");
                return;
            }

            Hashtable hashEmpresas = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Organizacion.Empresas.EMPRESA.Resources.INFO, "", "ID", "COD", true);
            Hashtable hashLegajos = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Configuracion.Kubo.ApiKubo.Resources.QRY_PERSONAL_TODO, "", "c_personal_emp", "oi_personal_emp", true);
            Hashtable hashLegajosEncontrados = new Hashtable();

            Dictionary<string, object> dic2 = new Dictionary<string, object>();
            dic2.Add("Ignorar", 0);

            ListaResultado<Empresa> listaEmpresas = NucleusRH.Base.Configuracion.Kubo.ApiKubo.Enviar<ListaResultado<Empresa>>(dic2, "listarEmpresa");
            ListaResultado<Licencia<ExtraVacaciones>> listaLicenciasTemp;
            ListaResultado<Licencia<ExtraVacaciones>> listaLicencias = new ListaResultado<Licencia<ExtraVacaciones>>();
            Dictionary<string, object> dicParam = new Dictionary<string, object>();
            foreach (Empresa e in listaEmpresas.Registros)
            {
                int ignorar = 0;
                do
                {
                    dicParam["Empresa"] = e.Codigo;
                    //dicParam["Legajo"] = null;
                    dicParam["Activos"] = true;
                    dicParam["Estados"] = new string[] { "A", "I", "B" };
                    dicParam["FechaCambio"] = fechaCambio;
                    dicParam["Ignorar"] = ignorar;
                    listaLicenciasTemp = Base.Configuracion.Kubo.ApiKubo.Enviar<ListaResultado<Licencia<ExtraVacaciones>>>(dicParam, "listarLicencias");
                    listaLicencias.Registros.AddRange(listaLicenciasTemp.Registros);
                    listaLicencias.Cantidad += listaLicenciasTemp.Registros.Count;
                    ignorar += 100;
                } while (listaLicenciasTemp.Registros.Count > 0);
            }

            PERSONAL_EMP peremp;
            foreach (Licencia<ExtraVacaciones> l in listaLicencias.Registros)
            {
                if (!hashLicencia.ContainsKey(l.Tipo.Codigo)) {
                    objBatch.Wrn("No se encuentra el código de licencia '" + l.Tipo.Codigo + "' dado de alta en NRH.");
                    continue;
                }
                string oi_licencia = hashLicencia[l.Tipo.Codigo].ToString();

                if (hashLegajosEncontrados.ContainsKey(l.Empleado.Empresa.Codigo + "_" + l.Empleado.Legajo))
                    peremp = (PERSONAL_EMP)hashLegajosEncontrados[l.Empleado.Empresa.Codigo + "_" + l.Empleado.Legajo];
                else
                {
                    object c_legajo = hashLegajos[l.Empleado.Empresa.Codigo + "_" + l.Empleado.Legajo];
                    if (c_legajo != null)
                        peremp = PERSONAL_EMP.Get(c_legajo.ToString());
                    else
                    {
                        objBatch.Wrn("Legajo: " + l.Empleado.Empresa.Codigo + "_" + l.Empleado.Legajo + ". No existe en NucleusRH");
                        continue;
                    }
                }

                List<int> listaDias = ArmarListaDias(l);

                if (l.Extras != null && CambiaTipo(l.Extras))
                {
                    DateTime f_inicio = new DateTime(l.Inicio.Year, l.Inicio.Month, l.Inicio.Day);
                    DateTime f_inicio_licencia = new DateTime(l.Inicio.Year, l.Inicio.Month, l.Inicio.Day);
                    bool nueva = false;
                    foreach (ExtraVacaciones ext in l.Extras)
                    {
                        DateTime f_fin = CalcularFechaFin(f_inicio, (f_inicio - f_inicio_licencia).Days, ext.CANT - 1, ext.HAB, listaDias);
                        NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER lic = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)peremp.LICEN_PER.GetByAttribute("f_inicio", f_inicio);
                        NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO lic_per = null;
                        if (ext.CANT == ext.INT)
                        {
                            if (lic != null)
                                peremp.LICEN_PER.RemoveById(lic.id.ToString());
                            continue;
                        }

                        if (lic == null)
                        {
                            if (l.Estado.Codigo == "B") continue;
                            lic = new Base.Personal.LegajoEmpresa.LICENCIA_PER();
                            lic_per = new Base.Personal.LegajoEmpresa.LIC_PERIODO();
                            lic.LIC_PERIODO.Add(lic_per);
                            nueva = true;
                        }
                        else
                        {
                            lic_per = (NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO)lic.LIC_PERIODO[0];

                            if (l.Estado.Codigo != "A")
                            {
                                if (LicenciaLiquidada(lic, peremp))
                                {
                                    objBatch.Wrn("Legajo: " + peremp.e_numero_legajo + ". La licencia del dia " + lic.f_inicio + " del tipo '" + l.Tipo.Codigo + "' ya se encuentra liquidada. No se puede " + (l.Estado.Codigo == "B" ? "anular" : "interrumpir"));
                                    continue;
                                }
                                else
                                {
                                    if (l.Estado.Codigo == "B")
                                    {
                                        lic.SetStatusDelete();
                                        continue;
                                    }
                                }
                            }
                        }

                        lic.f_inicio = f_inicio;
                        lic.f_fin = f_fin;
                        lic.f_carga = new DateTime(l.Inicio.Year, l.Inicio.Month, l.Inicio.Day);
                        lic.e_cant_dias = ext.CANT;
                        lic.d_habiles = ext.HAB ? GenerarHabiles((f_inicio - f_inicio_licencia).Days, (f_fin - f_inicio).Days + 1, listaDias) : "";
                        if (ext.INT > 0 && ext.CANT != ext.INT) lic.f_interrupcion = CalcularFechaFin(f_inicio, (f_inicio - f_inicio_licencia).Days, ext.CANT - 1 - ext.INT, ext.HAB, listaDias);
                        lic.e_anio_corresp = lic.f_inicio.Year;
                        lic.l_habiles = ext.HAB;
                        lic.oi_licencia = oi_licencia;

                        lic_per.e_anio = int.Parse(ext.PER);
                        lic_per.e_cant = ext.CANT;

                        try
                        {
                            if (nueva)
                            {
                                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia(peremp.id.ToString(), oi_licencia, lic.f_inicio, lic.f_fin, lic.e_cant_dias, lic.e_anio_corresp);
                                peremp.LICEN_PER.Add(lic);
                            }
                            else
                            {
                                List<string> lista = new List<string>();
                                lista.Add(lic.id.ToString());
                                NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia(peremp, oi_licencia, lic.f_inicio, lic.f_fin, lic.e_cant_dias, lic.e_anio_corresp,lista);

                            }
                         }
                        catch (Exception e)
                        {
                            //objBatch.Err("Error al Validar licencia: " + e.Message); 
                            objBatch.Wrn("Errores al Validar licencias: " + e.Message);
                            continue;
                        }

                        f_inicio = f_fin.AddDays(1);

                    }
                }
                else
                {
                    bool nueva = false;
                    DateTime f_inicio = new DateTime(l.Inicio.Year, l.Inicio.Month, l.Inicio.Day);
                    NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER lic = (NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER)peremp.LICEN_PER.GetByAttribute("f_inicio", f_inicio);

                    if (lic == null)
                    {
                        if (l.Estado.Codigo == "B") continue;
                        lic = new Base.Personal.LegajoEmpresa.LICENCIA_PER();
                        nueva = true;
                    }
                    else
                    {
                        if (l.Estado.Codigo != "A")
                        {
                            if (LicenciaLiquidada(lic, peremp))
                            {
                                objBatch.Wrn("Legajo: " + peremp.e_numero_legajo + ". La licencia del dia " + lic.f_inicio + " del tipo '" + l.Tipo.Codigo + "' ya se encuentra liquidada. No se puede " + (l.Estado.Codigo == "B" ? "anular" : "interrumpir"));
                                continue;
                            }
                            else
                            {
                                if (l.Estado.Codigo == "B")
                                {
                                    lic.SetStatusDelete();
                                    continue;
                                }
                            }
                        }
                    }

                    lic.f_inicio = f_inicio;
                    lic.f_fin = new DateTime(l.Fin.Year, l.Fin.Month, l.Fin.Day);
                    lic.f_carga = new DateTime(l.Inicio.Year, l.Inicio.Month, l.Inicio.Day);
                    lic.e_cant_dias = l.Dias.GetValueOrDefault();
                    lic.d_habiles = l.Habil ? GenerarHabiles(0, (lic.f_fin - lic.f_inicio).Days + 1, listaDias) : "";//d_habiles;// peremp.DiasTrabajablesString(lic.f_inicio, lic.f_fin);
                    if (l.Interrupcion != null) lic.f_interrupcion = new DateTime(l.Interrupcion.Year, l.Interrupcion.Month, l.Interrupcion.Day);

                    lic.e_anio_corresp = l.Inicio.Year;
                    lic.l_habiles = l.Habil;
                    lic.oi_licencia = oi_licencia;

                    if (l.Extras != null)
                    {
                        foreach (ExtraVacaciones ext in l.Extras)
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO lic_per = (NucleusRH.Base.Personal.LegajoEmpresa.LIC_PERIODO)lic.LIC_PERIODO.GetByAttribute("e_anio", int.Parse(ext.PER));
                            if (lic_per == null)
                            {
                                lic_per = new Base.Personal.LegajoEmpresa.LIC_PERIODO();
                                lic.LIC_PERIODO.Add(lic_per);
                            }
                            lic_per.e_anio = int.Parse(ext.PER);
                            lic_per.e_cant = ext.CANT;
                        }
                    }

                    try
                    {
                        if (nueva)
                        {
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarAltaLicencia(peremp.id.ToString(), oi_licencia, lic.f_inicio, lic.f_fin, lic.e_cant_dias, lic.e_anio_corresp);
                            peremp.LICEN_PER.Add(lic);
                        }
                        else
                        {
                            List<string> lista = new List<string>();
                            lista.Add(lic.id.ToString());
                            NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER.ValidarModLicencia(peremp, oi_licencia, lic.f_inicio, lic.f_fin, lic.e_cant_dias, lic.e_anio_corresp,lista);

                        }
                    }
                    catch (Exception e)
                    {
                        //objBatch.Err("Error al Validar licencia: " + e.Message);  
                        objBatch.Wrn("Errores al Validar licencias: " + e.Message);
                        continue;
                    }
                }

                hashLegajosEncontrados[l.Empleado.Empresa.Codigo + "_" + l.Empleado.Legajo] = peremp;
            }

            foreach (PERSONAL_EMP leg in hashLegajosEncontrados.Values)
            {
                NomadEnvironment.GetCurrentTransaction().Save(leg);
            }
        }

        private static Hashtable ObtenerHashLicencias(NomadBatch oBatch)
        {
            string d_valor_tipos = ObtenerParametroObservacion("d_tipos_lic");
            Hashtable hash = new Hashtable();

            if (d_valor_tipos == "")
            {
                      oBatch.Wrn("PARAMETRIZACION_INCOMPLETA: falta la definición de la variable (d_tipos_lic = Parámetro de licencias NDOC) que debe contener los valores en el atributo [Observaciones].");
              return hash;
            }

            string[] pares = d_valor_tipos.Split(';');

            Hashtable hashLic = NomadEnvironment.QueryHashtableValue(NucleusRH.Base.Personal.Licencias.LICENCIA.Resources.INFO, "", "COD", "ID", false);
            foreach(string par in pares)
            {
                string codigoRH = par.Substring(0, (par.IndexOf('=') >= 0 ? par.IndexOf('='): 0));
        if (codigoRH != "")
        {
          if(hashLic.ContainsKey(codigoRH))
          {
            string oi_licencia = hashLic[codigoRH].ToString();
            hash.Add(par.Substring(par.IndexOf('=') + 1,par.Length - par.IndexOf('=') -1),oi_licencia);
          }
        }
            }

            return hash;
        }

        private static bool CambiaTipo(List<ExtraVacaciones> extras)
        {
            bool cambio = extras[0].HAB;
            for (int i = 1; i < extras.Count; i++)
            {
                if (cambio != extras[i].HAB)
                    return true;
            }
            return false;
        }

        private static DateTime CalcularFechaFin(DateTime f_inicio, int inicio, int cantidadDias, bool habil, List<int> listaDias)
        {
            int dias = 0;
            if (habil)
            {
                for (int i = inicio; i <= inicio + cantidadDias; i++)
                {
                    if (listaDias[i] == 1)
                        dias++;
                }
            }

            dias += cantidadDias;

            return f_inicio.AddDays(dias);
        }

        private static bool LicenciaLiquidada(NucleusRH.Base.Personal.LegajoEmpresa.LICENCIA_PER lic, PERSONAL_EMP peremp)
        {
            if(!ModuleExist("Liquidacion")) return false;

            NucleusRH.Base.Organizacion.Empresas.EMPRESA emp = peremp.Getoi_empresa();
            NomadXML xmlLicencia = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Configuracion.Kubo.ApiKubo.Resources.QRY_LICENCIAS, "<DATA e_numero_legajo='" + peremp.e_numero_legajo + "' c_empresa='" + emp.c_empresa + "' f_inicio='" + lic.f_inicio + "' />").FirstChild();
            return xmlLicencia.GetAttr("existe") == "1";

        }

        private static List<int> ArmarListaDias(Licencia<ExtraVacaciones> licencia)
        {
            DateTime fechaInicio = new DateTime(licencia.Inicio.Year, licencia.Inicio.Month, licencia.Inicio.Day);
            DateTime fechaFin = new DateTime(licencia.Fin.Year, licencia.Fin.Month, licencia.Fin.Day);
            int cantidad = (fechaFin - fechaInicio).Days + 1;
            List<int> lista = new List<int>();
            for (int i = 0; i < cantidad; i++)
                lista.Add(0);

            foreach (Fecha fecha in licencia.NoHabiles)
            {
                DateTime feriado = new DateTime(fecha.Year, fecha.Month, fecha.Day);
                int cant = (feriado - fechaInicio).Days;
                lista[cant] = 1;
            }

            return lista;
        }

        private static string GenerarHabiles(int desde, int hasta, List<int> listaDias)
        {
            string d_habiles = "";
            for (int i = desde; i < desde + hasta; i++)
                d_habiles += listaDias[i];
            return d_habiles;
        }

        private static bool ProcesoBloquado(NomadBatch MyBatch)
        {
            bool Bloqueado = false;
            if (!NomadProxy.GetProxy().Lock().LockOBJ("NDOC:App=" + "NucleusRH.Base.Configuracion.Kubo." + NomadProxy.GetProxy().AppName))
            {
                MyBatch.Err("Existe un procesamiento hacia Nucleus Doc en este momento. Intente más tarde, si el problema persiste consulte con su administrador.");
                Bloqueado = true;
            }
            return Bloqueado ;
        }

        private static void CrearImagenBase64(NomadXML xmLeg)
        {
            if (xmLeg.GetAttr("oi_foto") != "")
            {
                Base.Personal.Imagenes.HEAD imagen = NucleusRH.Base.Personal.Imagenes.HEAD.Get(xmLeg.GetAttr("oi_foto"));
                string base64 = "";
                foreach (Personal.Imagenes.BIN bin in imagen.BINS)
                {
                    base64 += bin.DATA;
                }
                NomadXML images = xmLeg.AddTailElement("Imagen");
                images.SetAttr("Image", base64);
                images.SetAttr("Icon", "");
            }
        }

        private static string GenerarHashLegajo(NomadXML legajo)
        {
            List<string> parametros = new List<string>();
            parametros.Add(legajo.GetAttr("Nombre"));
            parametros.Add(legajo.GetAttr("Apellido"));
            parametros.Add(legajo.GetAttr("Notificar"));
            parametros.Add(legajo.GetAttr("Mail"));
            //parametros.Add(legajo.GetAttr("Celular"));
            parametros.Add(legajo.GetAttr("Cuil"));
            parametros.Add(legajo.GetAttr("Legajo"));
            parametros.Add(legajo.GetAttr("GrupoUsuario"));
            parametros.Add(legajo.GetAttr("f_egreso"));
            parametros.Add(legajo.GetAttr("CONVENIO"));			

            HashParametros hashParametros = new HashParametros(parametros);

            return hashParametros.GenerarHash();
        }

        public enum Acciones
        {
            None = 0, /// <summary>No se realizo ninguna accion </summary>
            Insert = 1,  /// <summary>Se agrego un registro</summary>
            Update = 2,   /// <summary>Se modifico un registro</summary>
            Delete = 4, /// <summary>Se elimino un registro</summary>
            Other = 8,  /// <summary>Se realizo otra operacion</summary>
            Error = 16 /// <summary>Se produjo un Error</summary>
        }

        public static string ObtenerParametro(string parametro)
        {
            try { return NomadEnvironment.QueryValue("ORG26_PARAMETROS", "d_valor", "c_parametro", parametro, "", false); }
            catch { return ""; }
        }
        public static string ObtenerParametroObservacion(string parametro)
        {
            try { return NomadEnvironment.QueryValue("ORG26_PARAMETROS", "o_parametro", "c_parametro", parametro, "", false); }
            catch { return ""; }
        }

        public static void GuardarParametro(string modulo, string clase,string tipo, string nombre_param, string f_hasta)
        {
            NomadLog.Info("~ Guardando parametro: " + nombre_param + " ~");

            string oiParam = NomadEnvironment.QueryValue("ORG26_PARAMETROS", "oi_parametro", "c_parametro", nombre_param, "ORG26_PARAMETROS.c_modulo=\\'" + modulo + "\\'", false);
            NucleusRH.Base.Organizacion.Parametros.PARAMETRO ddoPARAM = oiParam != "" ? ddoPARAM = NucleusRH.Base.Organizacion.Parametros.PARAMETRO.Get(oiParam) : new NucleusRH.Base.Organizacion.Parametros.PARAMETRO();

            ddoPARAM.c_modulo = modulo;
            ddoPARAM.c_parametro = nombre_param;
            ddoPARAM.d_parametro = "Parametro: " + nombre_param;
            ddoPARAM.d_clase = clase;
            ddoPARAM.d_tipo_parametro = tipo;
            ddoPARAM.l_bloqueado = false;
            ddoPARAM.d_valor = f_hasta;

            NomadEnvironment.GetCurrentTransaction().SaveRefresh(ddoPARAM);

            NomadLog.Info("~ Guardado el parametro: " + nombre_param + " con éxito ~");
        }
    }

    #region clases
    public class Empleado
    {
        private Empresa m_empresa;
        private int m_legajo;
        private bool m_activo;
        private string m_sector;
        private string m_identificador;
        private Extensiones m_extensiones;
        private string m_codigo;
        private string m_description;

        public Empresa Empresa {get{return m_empresa;}set{m_empresa = value;}}
        public int Legajo {get{return m_legajo;} set{m_legajo = value;}}
        public bool Activo{get{return m_activo;} set{m_activo = value;}}
        public string Sector{get{return m_sector;} set{m_sector = value;}}
        public string Identificador {get{return m_identificador;} set{m_identificador = value;}}
        public Extensiones Extensiones  {get{return m_extensiones;} set{m_extensiones = value;}}
        public string Codigo {get{return m_codigo;} set{m_codigo = value;}}
        public string Descripcion {get{return m_description;} set{m_description = value;}}
    }

    public class EmpleadoLic
    {
        private Empresa m_empresa;
        private int m_legajo;
        private bool m_activo;
        private CodificadoraSimple m_sector;
        private string m_identificador;
        private Extensiones m_extensiones;
        private string m_codigo;
        private string m_description;

        public Empresa Empresa {get{return m_empresa;}set{m_empresa = value;}}
        public int Legajo {get{return m_legajo;} set{m_legajo = value;}}
        public bool Activo{get{return m_activo;} set{m_activo = value;}}
        public CodificadoraSimple Sector{get{return m_sector;} set{m_sector = value;}}
        public string Identificador {get{return m_identificador;} set{m_identificador = value;}}
        public Extensiones Extensiones  {get{return m_extensiones;} set{m_extensiones = value;}}
        public string Codigo {get{return m_codigo;} set{m_codigo = value;}}
        public string Descripcion {get{return m_description;} set{m_description = value;}}
    }

    public class Extensiones
    {
       private EXTRA_VACA m_extra_vaca;

       public EXTRA_VACA EXTRA_VACA {get {return m_extra_vaca;} set{m_extra_vaca = value;}}
    }

    public class EXTRA_VACA
    {
        private List<Periodo> m_periodos;

        public List<Periodo> PERIODOS {get{return m_periodos;} set{m_periodos = value;}}
    }

    public class Periodo
    {
        private int m_anio;
        private HABIL m_habil;
        private string m_dias;
        private string m_pago;

        public int ANNO {get{return m_anio;} set{m_anio = value;}}
        public HABIL HABIL {get{return m_habil;} set{m_habil = value;}}
        public string DIAS {get{return m_dias;} set{m_dias = value;}}
        public string PAGO {get{return m_pago;} set{m_pago = value;}}
    }

    public class HABIL
    {
        private string m_codigo;

        public string Codigo {get{return m_codigo;} set{m_codigo = value;}}
    }

    public class Fecha
    {
        private int m_dia;
        private int m_mes;
        private int m_anio;

        public int Year { get { return m_anio; } set { m_anio = value; } }
        public int Month { get { return m_mes; } set { m_mes = value; } }
        public int Day { get { return m_dia; } set { m_dia = value; } }
    }

    public class Licencia<T>
    {
        private EmpleadoLic m_empleado;
        private Tipo m_tipo;
        private Fecha m_fechaInicio;
        private Fecha m_fechaFin;
        private Fecha m_interrupcion;
        private Estado m_estado;
        private int? m_dias;
        private bool m_habil;
        private List<T> m_extras;
        private List<Fecha> m_nohabiles;

        public EmpleadoLic Empleado { get { return m_empleado; } set { m_empleado = value; } }
        public Tipo Tipo { get { return m_tipo; } set { m_tipo = value; } }
        public Fecha Inicio { get { return m_fechaInicio; } set { m_fechaInicio = value; } }
        public Fecha Fin { get { return m_fechaFin; } set { m_fechaFin = value; } }
        public int? Dias { get { return m_dias; } set { m_dias = value; } }
        public bool Habil { get { return m_habil; } set { m_habil = value; } }
        public Fecha Interrupcion { get { return m_interrupcion; } set { m_interrupcion = value; } }
        public List<T> Extras { get { return m_extras; } set { m_extras = value; } }
        public List<Fecha> NoHabiles { get { return m_nohabiles; } set { m_nohabiles = value; } }
        public Estado Estado { get { return m_estado; } set { m_estado = value; } }
    }

    public class Estado
    {
        private string m_codigo;
        private string m_descripcion;

        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
    }

    public class ExtraVacaciones
    {
        private int m_cant;
        private string m_per;
        private bool m_hab;
        private int m_int;

        public int CANT { get { return m_cant; } set { m_cant = value; } }
        public string PER { get { return m_per; } set { m_per = value; } }
        public bool HAB { get { return m_hab; } set { m_hab = value; } }
        public int INT { get { return m_int; } set { m_int = value; } }
    }

    public class Tipo
    {
        private string m_codigo;
        private string m_descripcion;

        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
    }

    public class CodificadoraSimple
    {
        private string m_codigo;
        private string m_descripcion;

        public string Codigo { get { return m_codigo; } set { m_codigo = value; } }
        public string Descripcion { get { return m_descripcion; } set { m_descripcion = value; } }
    }

    #endregion
}


