using System;
using System.Collections;
using System.Text;
using System.Xml;

using Nomad.NSystem.Base;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Functions;

using NucleusRH.Base.Liquidacion.Legajo_Liquidacion;
using NucleusRH.Base.Liquidacion.Empresa_Liquidacion;
using NucleusRH.Base.Liquidacion.Personal_Liquidacion;
using NucleusRH.Base.Liquidacion.Liquidacion;

namespace NucleusRH.Base.Liquidacion {
    public enum eCargas {FijasPersona=0, FijasEmpresa=1, AcumuladorasPersona=2, NovedadPersona=3, NovedadLiquidacion=4, NovedadPersonaXVar=5, FijasCargo=6 };

    /// <summary>
    /// Clase que contiene los métodos para la carga de variables de la liquidación
    /// </summary>
    public class clsCargaVariables {

        public clsCargaVariables() {
        }

        /// <summary>
        /// Carga las variables fijas por persona. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargaFijasPersona(string pstrParametros) {

            clsParametros objPrametros;
            PERSONAL_EMP objPerEmp;
            VAL_VAREF objVarFija;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaFijasPersona()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.FijasPersona);

            //Carga la persona
            objPerEmp = PERSONAL_EMP.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objPerEmp.VAL_VAREF.Count > 0)
                objPerEmp.VAL_VAREF.Remove(objPerEmp.VAL_VAREF[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para la persona
                objVarFija = new VAL_VAREF();
                objVarFija.oi_variable = xelElemento.GetAttribute("id");
                objVarFija.n_valor = ObtenerValor(xelElemento);
                if (xelElemento.GetAttribute("p")!="")
                    objVarFija.e_periodo = int.Parse(xelElemento.GetAttribute("p"));

                objPerEmp.VAL_VAREF.Add(objVarFija);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerEmp);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");
        }

        /// <summary>
        /// Carga las variables fijas por Cargo. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>

        public void CargaFijasCargo(string pstrParametros)
        {

            clsParametros objPrametros;
            PERSONAL_EMP objPerEmp;
            VAREF_CARGO objVarFija;
            //VAREF_CARGO objVarCar;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaFijasCargo()");

            //Carga los parametros 
            objPrametros = new clsParametros(pstrParametros, eCargas.FijasCargo);

            //Carga la persona
            objPerEmp = PERSONAL_EMP.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por Persona y Cargo elimina las anteriores
            ArrayList arrDeleteVaref = new ArrayList();
            foreach (VAREF_CARGO objVarefCargo in objPerEmp.VAREF_CARGO) {
                if (objVarefCargo.oi_cargo == objPrametros.OiCargo) 
                {
                    arrDeleteVaref.Add(objVarefCargo);
                }
            }

            for (int iv = 0; iv < arrDeleteVaref.Count; iv++) {
                objPerEmp.VAREF_CARGO.Remove((VAREF_CARGO)arrDeleteVaref[iv]);
            }

            arrDeleteVaref = null;

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para el Cargo
                objVarFija = new VAREF_CARGO();
                objVarFija.oi_variable = xelElemento.GetAttribute("id");
                objVarFija.oi_cargo = objPrametros.OiCargo;
                objVarFija.n_valor = ObtenerValor(xelElemento);
                //if (xelElemento.GetAttribute("p")!="")
                  //  objVarFija.e_periodo = int.Parse(xelElemento.GetAttribute("p"));

                objPerEmp.VAREF_CARGO.Add(objVarFija);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerEmp);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");
        }
        
        
        /// <summary>
        /// Carga las variables fijas por Cargo. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargarVarPeriodoCargos(string pstrParametros)
        {

            clsParametros objPrametros;
            PERSONAL_EMP objPerEmp;
            VARPA_CARGO objVarFija;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargarVarPeriodoCargos()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.FijasCargo);

            //Carga la persona
            objPerEmp = PERSONAL_EMP.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por Persona y Cargo y elimina las anteriores
            ArrayList arrDeleteVarpa = new ArrayList();
            foreach (VARPA_CARGO objVarpaCargo in objPerEmp.VARPA_CARGO)
            {
                if (objVarpaCargo.oi_cargo == objPrametros.OiCargo)
                {
                    arrDeleteVarpa.Add(objVarpaCargo);
                }
            }

            for (int iv = 0; iv < arrDeleteVarpa.Count; iv++)
            {
                objPerEmp.VARPA_CARGO.Remove((VARPA_CARGO)arrDeleteVarpa[iv]);
            }

            arrDeleteVarpa = null;

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("a") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable por Periodo para el Cargo
                objVarFija = new VARPA_CARGO();
                objVarFija.oi_cargo = objPrametros.OiCargo;
                objVarFija.oi_variable = xelElemento.GetAttribute("a");
                objVarFija.n_valor = StringUtil.str2dbl(xelElemento.GetAttribute("v"));
                objVarFija.e_periodo = int.Parse(xelElemento.GetAttribute("p"));

                objPerEmp.VARPA_CARGO.Add(objVarFija);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerEmp);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");
        }


        /// <summary>
        /// Carga las variables fijas por persona. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargarVariablesPeriodo(string pstrParametros) {

            clsParametros objPrametros;
            PERSONAL_EMP objPerEmp;
            VAL_VARPA objVarFija;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargarVariablesPeriodo()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.FijasPersona);

            //Carga la persona
            objPerEmp = PERSONAL_EMP.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objPerEmp.VAL_VARPA.Count > 0)
                objPerEmp.VAL_VARPA.Remove(objPerEmp.VAL_VARPA[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("a") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable por Periodo para la personas
                objVarFija = new VAL_VARPA();
                objVarFija.oi_variable = xelElemento.GetAttribute("a");
                objVarFija.n_valor = StringUtil.str2dbl(xelElemento.GetAttribute("v"));
                objVarFija.e_periodo = int.Parse(xelElemento.GetAttribute("p"));

                objPerEmp.VAL_VARPA.Add(objVarFija);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerEmp);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");
        }

        /// <summary>
        /// Carga las variables fijas por empresa. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargaFijasEmpresa(string pstrParametros) {

            clsParametros objPrametros;
            EMPRESA objEmpresa;
            VAL_VARGF objVarFija;
            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaFijasEmpresa()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.FijasEmpresa);

            //Carga la empresa
            objEmpresa = EMPRESA.Get(objPrametros.OiEmpresa);

            NomadEnvironment.GetTrace().Info("Procesando la empresa (" + objPrametros.OiEmpresa + ") " + objPrametros.Descripcion);

            //Recorre las variables por empresa y elimina las anteriores
            while (objEmpresa.VAL_VARGF.Count > 0)
                objEmpresa.VAL_VARGF.Remove(objEmpresa.VAL_VARGF[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para la empresa
                objVarFija = new VAL_VARGF();
                //objVarFija.e_periodo = int.Parse(xelElemento.GetAttribute("p"));
                objVarFija.oi_variable = xelElemento.GetAttribute("id");
                objVarFija.n_valor = ObtenerValor(xelElemento);

                objEmpresa.VAL_VARGF.Add(objVarFija);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la empresa.");
            NomadEnvironment.GetCurrentTransaction().Save(objEmpresa);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

        }

        /// <summary>
        /// Carga las variables acumuladoras por persona. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargaAcumuladorasPersona(string pstrParametros) {

            clsParametros objPrametros;
            PERSONAL_EMP objPerEmp;
            VAL_VAREA objVarAcu;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaAcumuladorasPersona()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.AcumuladorasPersona);

            //Carga la persona
            objPerEmp = PERSONAL_EMP.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objPerEmp.VAL_VAREA.Count > 0)
                objPerEmp.VAL_VAREA.Remove(objPerEmp.VAL_VAREA[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable acumuladora para la persona
                objVarAcu = new VAL_VAREA();
                objVarAcu.oi_variable = xelElemento.GetAttribute("id");
                objVarAcu.n_valor = ObtenerValor(xelElemento);

                objPerEmp.VAL_VAREA.Add(objVarAcu);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerEmp);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

        }

        /// <summary>
        /// Carga las variables de novedad por persona. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargaNovedadPersona(string pstrParametros) {

            //NucleusRH.Base.Liquidacion.Personal_Liquidacion.
            clsParametros objPrametros;
            PERSONAL_LIQ objPerLiq;
            VAL_VAREN objVarNovedad;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaNovedadPersona()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.NovedadPersona);

            //Carga la persona
            objPerLiq = PERSONAL_LIQ.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objPerLiq.VAL_VAREN.Count > 0)
                objPerLiq.VAL_VAREN.Remove(objPerLiq.VAL_VAREN[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para la persona
                objVarNovedad = new VAL_VAREN();
                objVarNovedad.oi_variable = xelElemento.GetAttribute("id");
                objVarNovedad.n_valor = ObtenerValor(xelElemento);

                objPerLiq.VAL_VAREN.Add(objVarNovedad);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerLiq);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

        }

        public void CargaConceptosManual(string pstrParametros) {

            //NucleusRH.Base.Liquidacion.Personal_Liquidacion.
            clsParametros objPrametros;
            PERSONAL_LIQ objPerLiq;
            CONC_MAN_PER objConcManual;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaConceptosManual()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.NovedadPersona);

            //Carga la persona
            objPerLiq = PERSONAL_LIQ.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objPerLiq.CONC_MAN_PER.Count > 0)
                objPerLiq.CONC_MAN_PER.Remove(objPerLiq.CONC_MAN_PER[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '' or @c!='']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando el concepto '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para la persona
                objConcManual = new CONC_MAN_PER();
                objConcManual.oi_concepto = xelElemento.GetAttribute("id");
                objConcManual.n_valor = StringUtil.str2dbl(xelElemento.GetAttribute("v"));
                objConcManual.n_cantidad = StringUtil.str2dbl(xelElemento.GetAttribute("c"));

                objPerLiq.CONC_MAN_PER.Add(objConcManual);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerLiq);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

        }
        //CargaVariablesManuales

        /// <summary>
        /// Carga las variables de novedad por persona. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargarNovedadesPeriodo(string pstrParametros) {

            //NucleusRH.Base.Liquidacion.Personal_Liquidacion.
            clsParametros objPrametros;
            PERSONAL_LIQ objPerLiq;
            VAL_VARPN objVarNovedad;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaNovedadPeriodo()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.AcumuladorasPersona);

            //Carga la persona
            objPerLiq = PERSONAL_LIQ.Get(objPrametros.OiPersona);

            NomadEnvironment.GetTrace().Info("Procesando la persona (" + objPrametros.OiPersona + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objPerLiq.VAL_VARPN.Count > 0)
                objPerLiq.VAL_VARPN.Remove(objPerLiq.VAL_VARPN[0]);

            NomadEnvironment.GetTrace().Info("Fin Limpiar.");

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("a") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para la persona
                objVarNovedad = new VAL_VARPN();
                objVarNovedad.oi_variable = xelElemento.GetAttribute("a");
                objVarNovedad.n_valor = StringUtil.str2dbl(xelElemento.GetAttribute("v"));
                objVarNovedad.e_periodo = int.Parse(xelElemento.GetAttribute("p"));

                objPerLiq.VAL_VARPN.Add(objVarNovedad);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objPerLiq);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

        }

        /// <summary>
        /// Carga las variables de novedad por Liquidación. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargaNovedadLiquidacion(string pstrParametros) {

            clsParametros objPrametros;
            LIQUIDACION objLiq;
            VAL_VARGN objVarNovedad;

            XmlElement xelElemento;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaNovedadLiquidacion()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.NovedadLiquidacion);

            //Carga la liquidación
            objLiq = LIQUIDACION.Get(objPrametros.OiLiquidacion);

            NomadEnvironment.GetTrace().Info("Procesando la liquidación (" + objPrametros.OiLiquidacion + ") " + objPrametros.Descripcion);

            //Recorre las variables por persona y elimina las anteriores
            while (objLiq.VAL_VARGN.Count > 0)
                objLiq.VAL_VARGN.Remove(objLiq.VAL_VARGN[0]);

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '']")) {
                xelElemento = (XmlElement) objNodo;

                NomadEnvironment.GetTrace().Info("Procesando la variable '" + xelElemento.GetAttribute("n") + "' con valor '" + xelElemento.GetAttribute("v") + "'.");

                //Crea la nueva variable fija para la persona
                objVarNovedad = new VAL_VARGN();
                objVarNovedad.oi_variable = xelElemento.GetAttribute("id");
                objVarNovedad.n_valor = ObtenerValor(xelElemento);

                objLiq.VAL_VARGN.Add(objVarNovedad);
            }

            //Graba los cambios
            NomadEnvironment.GetTrace().Info("Intenta grabar la persona.");
            NomadEnvironment.GetCurrentTransaction().Save(objLiq);
            NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

        }

        /// <summary>
        /// Carga las variables de novedad por persona. Saca los valores desde el string de parámetros.
        /// </summary>
        /// <param name="pstrParametros">Parámetros Xml en formato string.</param>
        public void CargaNovedadPersonaXVar(string pstrParametros) {

            clsParametros objPrametros;
            PERSONAL_LIQ objPerLiq = new PERSONAL_LIQ();
            VAL_VAREN objVarNovedad;

            XmlElement xelElemento;

            string strValor;
            string strValorAnt;
            string strOiV;
            string strOiPer;
            string strNombrePersona;
            bool bolTransaUsada = false;

            NomadEnvironment.GetTrace().Info("Comienza la ejecución del método CargaNovedadPersonaXVar()");

            //Carga los parametros
            objPrametros = new clsParametros(pstrParametros, eCargas.NovedadPersonaXVar);

            NomadEnvironment.GetTrace().Info("Procesando la variable (" + objPrametros.OiVariable + ")");
            NomadEnvironment.GetCurrentTransaction().Begin();

            //Recorre los rows y agrga los nuevos valores
            foreach (XmlNode objNodo in objPrametros.Rows.SelectNodes("ROW[@v != '' or @va != '']")) {
                xelElemento = (XmlElement) objNodo;

                strValor = xelElemento.GetAttribute("v");
                strValorAnt = xelElemento.GetAttribute("va");
                strOiPer = xelElemento.GetAttribute("id");
                strNombrePersona = xelElemento.GetAttribute("d");

                //Solo se realizarán actualizaciones a la DB si la variable cambió de valor.
                if (strValor != strValorAnt) {

                    //Carga la persona
                    objPerLiq = PERSONAL_LIQ.Get(strOiPer);
                    NomadEnvironment.GetTrace().Info("Procesando la persona_liquidacion '" + strNombrePersona + "'.");

                    strOiV = xelElemento.GetAttribute("oiv");

                    //Se ha modificado el valor de la variable
                    if (strOiV == null || strOiV == "" ) {
                        //La variable no existía. Se hace un INSERT
                        NomadEnvironment.GetTrace().Info("Inserta la variable.");
                        objVarNovedad = new VAL_VAREN();
                        objVarNovedad.oi_variable = objPrametros.OiVariable;
                        objVarNovedad.n_valor = ObtenerValor(xelElemento);
                        objPerLiq.VAL_VAREN.Add(objVarNovedad);

                    } else {
                        //La variable ya existía.

                        if (strValor == null || strValor == "" ) {
                            NomadEnvironment.GetTrace().Info("Deletea la variable.");
                            //Se hace un DELETE
                            objPerLiq.VAL_VAREN.RemoveById(strOiV);
                        } else {
                            //Se hace un UPDATE
                            NomadEnvironment.GetTrace().Info("Updatea la variable.");
                            objVarNovedad = (VAL_VAREN) objPerLiq.VAL_VAREN.GetById(strOiV);
                            objVarNovedad.n_valor = ObtenerValor(xelElemento);
                        }
                    }

                    NomadEnvironment.GetCurrentTransaction().Save(objPerLiq);
                    bolTransaUsada = true;
                }

            }

            if (bolTransaUsada) {
                //Graba los cambios
                try {
                    NomadEnvironment.GetTrace().Info("Intenta grabar las personas.");
                    NomadEnvironment.GetCurrentTransaction().Commit();
                    NomadEnvironment.GetTrace().Info("Se grabó correctamente.");

                } catch (Exception ex) {
                    NomadEnvironment.GetTrace().Info("Se produjo un error al commitear la transacción. " + ex.Message);
                    NomadEnvironment.GetCurrentTransaction().Rollback();
                    throw ex;
                }
            } else {
                NomadEnvironment.GetCurrentTransaction().Rollback();
            }

        }

        /***********************************************************************************/
        /** Funciones generales                                                           **/
        /***********************************************************************************/

        static double ObtenerValor(XmlElement pobjElemento) {
            string strValor;
            string strTipo;
            double dblResultado = 0d;

            strValor = pobjElemento.GetAttribute("v");
            strTipo = pobjElemento.GetAttribute("t");

            switch (strTipo.ToLower()) {
              case "datetime":
                    dblResultado = clsCargaVariables.strDate2double(strValor);
                break;

              case "double":
                    dblResultado = StringUtil.str2dbl(strValor);
                break;

              case "bool":
                  dblResultado = clsCargaVariables.str2bool(strValor) ? 1d : 0d;
                break;

              case "int":
                  dblResultado = Convert.ToDouble(strValor);
                    break;
            }

            return dblResultado;
        }

        /// <summary>
        /// Convierte un string con un booleano dentro en un double formateado 1|0.
        /// </summary>
        /// <param name="pstrBool">Boleano que puede ser S|1|T|V para los verdaderos.</param>
        /// <returns></returns>
        static bool str2bool (string pstrBool) {
            try {
                pstrBool = pstrBool.ToUpper();
                return pstrBool.Substring(0, 1) == "S" ||
                        pstrBool.Substring(0, 1) == "1" ||
                        pstrBool.Substring(0, 1) == "T" ||
                        pstrBool.Substring(0, 1) == "V";
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Convierte un string con una fecha dentro en un double formateado yyyymmdd.
        /// </summary>
        /// <param name="pstrDate">Fecha con formato dd/mm/yyyy.</param>
        /// <returns></returns>
    static double strDate2double(string pstrDate) {
      pstrDate = pstrDate.Trim();
            string strFormateado = pstrDate.Substring(6, 4) + pstrDate.Substring(3, 2) + pstrDate.Substring(0, 2);
      return double.Parse(strFormateado);
    }

    }

    class clsParametros {
        private string m_strOiCargo;
        private string m_strOi;
        private string m_strOiVar;
        private string m_strDescripcion;

        private XmlElement m_xelRows;

        public clsParametros(string pstrParametros, eCargas peCargas) {
            XmlDocument xmlParametros = new XmlDocument();
            XmlElement xelElemento;

            xmlParametros.LoadXml(pstrParametros);

            //Recorre los elementos hijos
            foreach (XmlNode objNodo in xmlParametros.DocumentElement.ChildNodes) {
                xelElemento = (XmlElement) objNodo;

                if (objNodo.Name == "TITLE") {

                    switch (peCargas) {

                        case eCargas.FijasCargo:
                            this.m_strOiCargo = xelElemento.GetAttribute("oi");
                            this.m_strOi = xelElemento.GetAttribute("oi_personal_emp"); 
                            break;

                        case eCargas.AcumuladorasPersona:
                            this.m_strOi = xelElemento.GetAttribute("oi");
                            break;

                        case eCargas.FijasEmpresa:
                            this.m_strOi = xelElemento.GetAttribute("oi");
                            break;

                        case eCargas.FijasPersona:
                            this.m_strOi = xelElemento.GetAttribute("oi");
                            break;

                        case eCargas.NovedadLiquidacion:
                            this.m_strOi = xelElemento.GetAttribute("oi");
                            break;

                        case eCargas.NovedadPersona:
                            this.m_strOi = xelElemento.GetAttribute("oi");
                            break;

                        case eCargas.NovedadPersonaXVar:
                            this.m_strOi = xelElemento.GetAttribute("oi_liquidacion");
                            this.m_strOiVar = xelElemento.GetAttribute("oi_variable");
                            break;

                    }

                    this.m_strDescripcion = xelElemento.GetAttribute("value");

                } else {
                    if (objNodo.Name == "ROWS") {
                        this.m_xelRows = xelElemento;
                    }
                }
            }

        }

        /*****************************************************************************/
        /** Atributos                                                               **/
        /*****************************************************************************/

        public string OiPersona {
            get {return this.m_strOi;}
        }

        public string OiEmpresa {
            get {return this.m_strOi;}
        }

        public string OiLiquidacion {
            get {return this.m_strOi;}
        }

        public string OiVariable {
            get {return this.m_strOiVar;}
        }

        public string Descripcion {
            get {return this.m_strDescripcion;}
        }

        public string OiCargo {
            get { return this.m_strOiCargo;}
        }

        public XmlElement Rows {
            get {return this.m_xelRows;}
        }

    }
}


