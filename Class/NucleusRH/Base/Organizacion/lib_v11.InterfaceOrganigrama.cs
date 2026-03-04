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

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace NucleusRH.Base.Organizacion.InterfaceOrganigrama
{

    //////////////////////////////////////////////////////////////////////////////////
    //Clase Interface Organigrama
    public partial class ORGANIGRAMA
    {
        public static void ExportOrg(string oi_estructura, bool l_legajos, bool l_ingreso)
        {
            string bruto = "";
            string neto = "";
            int totRegs;
            NomadXML param_s = new NomadXML("PARAM");
            NomadXML param = new NomadXML("PARAM");
            param.SetAttr("oi_estructura", oi_estructura);
            param.SetAttr("legajos", l_legajos);

            NomadBatch objBatch;
            objBatch = NomadBatch.GetBatch("Iniciando...", "Exportando Organigrama... ");

            //NOMBRE DEL EXCEL
            string outFileName = NomadProxy.GetProxy().RunPath + "Nomad\\TEMP\\" + NomadProxy.GetProxy().Batch().ID;

            NomadXML xml_organigrama = NomadEnvironment.QueryNomadXML(NucleusRH.Base.Organizacion.InterfaceOrganigrama.ORGANIGRAMA.Resources.QRY_ORGANIGRAMA, param.ToString());
            xml_organigrama = xml_organigrama.FirstChild();

            objBatch.Log("Diseñando reporte XLS");
            NomadLog.Info("Diseñando reporte XLS");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("Sheet A1");

            //ESTILO TITULOS
            ICellStyle titStyle = workbook.CreateCellStyle();
            IFont font = workbook.CreateFont();
            font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            titStyle.WrapText = true;
            titStyle.Alignment = HorizontalAlignment.Center;
            titStyle.VerticalAlignment = VerticalAlignment.Top;
            titStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            titStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            titStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            titStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            titStyle.SetFont(font);

            //ESTILO RESULTADOS SIMPLES
            ICellStyle cs = workbook.CreateCellStyle();
            cs.WrapText = true;
            cs.Alignment = HorizontalAlignment.Center;
            cs.VerticalAlignment = VerticalAlignment.Top;
            cs.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cs.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cs.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cs.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cs.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");

            //ESTILO DATOS
            ICellStyle datos = workbook.CreateCellStyle();
            datos.WrapText = true;
            datos.Alignment = HorizontalAlignment.Left;
            datos.VerticalAlignment = VerticalAlignment.Top;
            datos.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            datos.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            datos.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            datos.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;

            //ANCHO DE COLUMNAS
            sheet1.SetColumnWidth(0, 10000);
            sheet1.SetColumnWidth(1, 10000);
            sheet1.SetColumnWidth(2, 10000);
            sheet1.SetColumnWidth(3, 20000);

            //TITULOS FIJOS DEL REPORTE
            sheet1.CreateRow(0).CreateCell(0).SetCellValue("OI");
            sheet1.GetRow(0).GetCell(0).CellStyle.SetFont(font);

            sheet1.GetRow(0).CreateCell(1).SetCellValue("OI PADRE");
            sheet1.GetRow(0).GetCell(1).CellStyle.SetFont(font);

            sheet1.GetRow(0).CreateCell(2).SetCellValue("PUESTO");
            sheet1.GetRow(0).GetCell(2).CellStyle.SetFont(font);

            sheet1.GetRow(0).CreateCell(3).SetCellValue("LEGAJOS");
            sheet1.GetRow(0).GetCell(3).CellStyle.SetFont(font);

            int rows = 1;
            string datos_aux = "";
            totRegs = xml_organigrama.ChildLength;
            objBatch.SetMess("Recorriendo estructuras...");
            objBatch.Log("Recorriendo estructuras...");
            int l = 1;
            for (NomadXML xml_org = xml_organigrama.FirstChild(); xml_org != null; l++, xml_org = xml_org.Next())
            {
                IRow linea = sheet1.CreateRow(rows);

                objBatch.SetPro(0, 90, totRegs, l);

                NomadEnvironment.GetTrace().Info("ESTRUCTURA: " + xml_org.GetAttr("d_unidad_org"));
                linea.CreateCell(0).SetCellValue(xml_org.GetAttr("oi_estructura"));
                linea.GetCell(0).CellStyle = datos;
                linea.CreateCell(1).SetCellValue(xml_org.GetAttr("oi_estr_padre"));
                linea.GetCell(1).CellStyle = datos;
                linea.CreateCell(2).SetCellValue(xml_org.GetAttr("d_unidad_org"));
                linea.GetCell(2).CellStyle = datos;
                if (l_legajos)
                {

                    if (xml_org.GetAttr("legajos") != "")
                    {
                        string[] legs = xml_org.GetAttr("legajos").Split(new Char[] { ',', ' ' });
                        for (int x = 0; x < legs.Length; x++)
                        {
                            if (legs[x].ToString() != "")
                            {

                                NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP LEG = NucleusRH.Base.Personal.LegajoEmpresa.PERSONAL_EMP.Get(legs[x]);
                                NomadEnvironment.GetTrace().Info("LEGAJO ID: " + LEG.id + " - PERSONA ID: " + LEG.oi_personal);

                                if (datos_aux != "")
                                {
                                    datos_aux = datos_aux + '\n' + LEG.e_numero_legajo.ToString();
                                }
                                else
                                {
                                    datos_aux = LEG.e_numero_legajo.ToString();
                                }

                                NucleusRH.Base.Personal.Legajo.PERSONAL PER = NucleusRH.Base.Personal.Legajo.PERSONAL.Get(LEG.oi_personal);
                                datos_aux = datos_aux + '-' + PER.d_ape_y_nom;

                               /* if (l_neto || l_bruto)
                                {
                                    param_s.SetAttr("oi_personal_emp", LEG.id);
                                    param_s.SetAttr("periodo", periodo);
                                    NomadXML xml_sueldos = NomadEnvironment.QueryNomadXML(NucleusRH.WillinerRH.Organizacion.InterfaceOrganigrama.ORGANIGRAMA.Resources.QRY_SUELDOS, param_s.ToString());
                                    xml_sueldos = xml_sueldos.FirstChild();
                                    for (NomadXML xml_su = xml_sueldos.FirstChild(); xml_su != null; xml_su = xml_su.Next())
                                    {

                                        if (xml_su.GetAttr("cod") == "aa_suebru")
                                        {
                                            bruto = xml_su.GetAttr("text");
                                        }
                                        {
                                            neto = xml_su.GetAttr("text");
                                        }
                                    }

                                    if (l_bruto)
                                    {
                                        datos_aux = datos_aux + ' ' + bruto;
                                    }

                                    if (l_neto)
                                    {
                                        datos_aux = datos_aux + ' ' + neto;
                                    }

                                } */

                                if (l_ingreso)
                                {
                                    datos_aux = datos_aux + " Ingreso: " + LEG.f_ingreso.ToShortDateString();
                                }
                            }
                        }
                    }
                }

                linea.CreateCell(3).SetCellValue(datos_aux);
                linea.GetCell(3).CellStyle = datos;

                rows++;
                datos_aux = "";
            }

            objBatch.SetMess("Finalizó correctamente...");
            objBatch.Log("Finalizó correctamente...");
            FileStream sw = File.Create(outFileName + ".xls");
            workbook.Write(sw);
            sw.Close();

            objBatch.SetPro(100);
        }
    }
}


