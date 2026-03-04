using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text;

using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using NCompiler;
using Nomad.NSystem.Document;
using Nomad.NSystem.Proxy;
using Nomad.NSystem.Base;

// <refresh srv="nucleusnet" port="1501" service="compile"/>

namespace NucleusRH.Base.Personal {

	/* Funciones utiles */
	
	public class UtilBase {

    //!methods
		//!method double Max(double a, double b)
		//!description Maximo de dos numeros
		//! Retorna el maximo de los numeros 'a' y 'b'.
		//!return Retorna Numero maximo entre 'a' y 'b'
		public static double Max(double a, double b) { return a > b ? a : b; }

		//!method double Min(double a, double b)
		//!description Minimo de dos numeros
		//! Retorna el minimo de los numeros 'a' y 'b'
		//!return Retorna Numero minimo entre 'a' y 'b'
		public static double Min(double a, double b) { return a < b ? a : b; }


		//!method bool IsDigit(char c)
		//!description Indica si un caractedr es un digito
		//!return Retorna true si 'c' es digito'
		public static bool IsDigit(char c) {
			return (c >= '0') && ('9' >= c);
		}

		//!method double Str2Dbl( string s )
		//!description Convierte un string a double, el formato adminito es usando el caracter '.'
		//!return Retorna valor de 's' con tipo double
		public static double Str2Dbl(string s) {
			char[] a = s.ToCharArray();
			int signo = 1;
			double divisor = 1, res = 0;
			bool dec = false;
			for ( int i = 0; i < a.Length; i++ )
				if ( a[i] == '-' )
					signo = -1;
				else if ( a[i] == '.' )
					dec = true;
				else if ( IsDigit(a[i]) )
				{
					res *= 10;
					res += (a[i] - '0');
					if ( dec ) divisor *= 10;
				}
			return signo * (res / divisor);
		}

		//!method DateTime Str2Date(string s)
		//!description Convierte un string de fecha u hora al tipo DateTime\\ 
		//! Los formatos permitidos de 's' son:
		//!  * 'YYYYMMDD', 'YYYYMMDDHHmmSS', 'HHmmSS'
		//!  * YYYY indica anio en cuatro digitos
		//!  * MM mes en dos dijitos
		//!  * DD dia en dos dijitos
		//!  * HH hora en dos digitos
		//!  * mm minutos en dos digitos
		//!  * SS segundos dos digitos
		//! Por ejemplo:
		//!  * 20040310 es el dia: 10 de Marzo del 2004
		//!  * 160210 es la hora: 4 de la tarde con 2 minutos y 10 segundos
		//!  * 20040310160210 es el dia anterior a la hora anterior
		//!return
		//!   * Si 's' es de formato 'YYYYMMDD' retorna DateTime con el dia indicado y hora 00:00:00.0000000
		//!  * Si 's' es de formato 'YYYYMMDDHHmmSS' retorna DateTime con dia y hora indica
		//!  * Si 's' es de formato 'HHmmSS' retorna DateTime con dia 1/1/0001 y hora indicada
		public static DateTime Str2Date(string s) {
			s = s.Trim();
 			if ( s.Length == 8 )
				return new DateTime( Convert.ToInt32(s.Substring(0,4),10), Convert.ToInt32(s.Substring(4,2),10), Convert.ToInt32(s.Substring(6,2),10), 0, 0, 0, 0 );
			else if ( s.Length == 6 )
				return new DateTime( 1, 1, 1, Convert.ToInt32(s.Substring(0,2),10), Convert.ToInt32(s.Substring(2,2),10), Convert.ToInt32(s.Substring(4,2),10), 0 );
			else if ( s.Length == 14 )
				return new DateTime( Convert.ToInt32(s.Substring(0,4),10), Convert.ToInt32(s.Substring(4,2),10), Convert.ToInt32(s.Substring(6,2),10), Convert.ToInt32(s.Substring(8,2),10), Convert.ToInt32(s.Substring(10,2),10), Convert.ToInt32(s.Substring(12,2),10), 0 );

			return DateTime.MinValue;
		}

		//!method string Dbl2Str(double s)
		//!description Convierte un double a string
		//!return Retorna string con el caracter '.' como separador de miles.
		public static string Dbl2Str(double s) {
			return s.ToString().Replace(",",".");
		}

		//!method string Date2Str(DateTime d)
		//!description Convierte un DateTime a string
		//!return Retorna string con formato YYYYMMDDHHmmDD
		public static string Date2Str(DateTime d)	{
			string year = d.Year.ToString().Trim();
			string month = d.Month.ToString().Trim();
			if (month.Length == 1) month = "0" + month;
			string day = d.Day.ToString().Trim();
			if (day.Length == 1) day = "0" + day;
			string hour = d.Hour.ToString().Trim();
			if (hour.Length == 1) hour = "0" + hour;
			string minute = d.Minute.ToString().Trim();
			if (minute.Length == 1) minute = "0" + minute;
			string second = d.Second.ToString().Trim();
			if (second.Length == 1) second = "0" + second;
			return year + month + day + hour + minute + second;
		}

		//!method string RTrim(string s)
		//!description Limpia los espacios en blanco a la derecha
		//!return Retorna la cadena sin los caracteres en blanco de la derecha
		public static string RTrim(string s) {
			while ( s.Substring( s.Length - 1, 1 ) == " " )
				s = s.Substring( 0, s.Length - 1);
			return s;
		}

		//!method string LTrim(string s)
		//!description Limpia los espacios en blanco a la izquierda
		//!return Retorna la cadena sin los caracteres en blanco de la Izquierda
		public static string LTrim(string s) {
			while ( s.Substring( 0, 1 ) == " " )
				s = s.Substring( 1, s.Length - 1);
			return s;
		}

		//!method string Trim(string s)
		//!description Limpia los espacios en blanco de la cadena.
		//!return Retorna la cadena sin los caracteres en blanco de la derecha e izquierda.
		public static string Trim( string s )	{
			return s.Trim();
		}


		//!method int ParseInt(string s)
		//!description Convierte un string en entero
		//!return Retorna el entero de la cadena
		public static int ParseInt(string s) {
			return Convert.ToInt32( s );
		}
		public static int parseInt(string s) {
			return Convert.ToInt32( s );
		}


		//!method int ParseInt(double d)
		//!description Convierte un double a entero, truncando los decimales.
		//!return Retorna la parte entera de 'd'
		public static int ParseInt(double d) {
			return Convert.ToInt32( d );
		}
		public static int parseInt(double d) {
			return Convert.ToInt32( d );
		}

		//!method double Round(double n, int d)
		//!description Redondea un double
		//!return Retorna 'n' redondeado a 'd' digitos.
		public static double Round(double n, int d)	{
			return System.Math.Round( n, d );
		}
		public static double round(double n, int d)	{
			return System.Math.Round( n, d );
		}


		//!method int Round(double n)
		//!description Redondea un double
		//!return Retorna 'n' redondeado a 0 digitos.
		public static int Round(double n)	{
			return Convert.ToInt32(System.Math.Round( n, 0 ));
		}
		public static int round(double n)	{
			return Convert.ToInt32(System.Math.Round( n, 0 ));
		}


		//!method int Day(System.DateTime fecha)
		//!description Dia de una fecha
		//!return Retorna el numero de dia de la fecha del parametro 'fecha'
		public static int Day(System.DateTime fecha) {
			return fecha.Day;
		}

		public static int day(System.DateTime fecha) {
			return Day(fecha);
		}

		
		//!method int Month(System.DateTime fecha)
		//!description Mes de una fecha
		//!return Retorna el numero de mes de la fecha del parametro 'fecha'
		public static int Month(System.DateTime fecha) {
			return fecha.Month;
		}
		public static int month(System.DateTime fecha) {
			return Month(fecha);
		}


		//!method int Year(System.DateTime fecha)
		//!description Anio de una fecha
		//!return Retorna el numero de anio de la fecha del parametro 'fecha'
		public static int Year(System.DateTime fecha)	{
			return fecha.Year;
		}
		public static int year(System.DateTime fecha)	{
			return Year(fecha);
		}


		//!method DateTime Date()
		//!description Fecha Actual
		//!return Retorna la fecha actual.
		public static DateTime Date()	{
			return DateTime.Now;
		}
		public static DateTime date()	{
			return Date();
		}


		//!method DateTime Date(int dia, int mes, int anio)
		//!description Fecha a partir del numero de dias, mes y anio.
		//!return Retorna la fecha formado por los parametros 'dia', 'mes', 'anio', dicha fecha tendra la
		//! hora 00:00:00.00000000
		public static DateTime Date( int dia, int mes, int anio )	{
			return new DateTime(anio, mes, dia);
		}
		public static DateTime date( int dia, int mes, int anio )	{
			return new DateTime(anio, mes, dia);
		}


		//!method DateTime LastDateMonth(DateTime fecha)
		//!description Fecha con del ultimo dia del mes de 'fecha'
		//!return Fecha que corresponde al ultimo dia del mes y anio del parametro 'fecha'
		public static DateTime LastDateMonth(DateTime fecha)	{
			return new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
		}

		//!method DateTime FirstDateMonth(DateTime fecha)
		//!description Fecha con el primer dia del mes de 'fecha'
		//!return Fecha que corresponde al primer dia del mes y anio del parametro 'fecha'
		public static DateTime FirstDateMonth(DateTime fecha)	{
			return new DateTime(fecha.Year, fecha.Month, 1);
		}

		//!method int DiffYears(DateTime d1, DateTime d2)
		//!description Cantidad de anios entre dos fechas.
		//!return Retorna la cantidad de anios entre las fechas 'desde' y 'hasta'.
		public static int DiffYears( DateTime desde, DateTime hasta )	{
			int r = hasta.Year - desde.Year;
			if ( hasta.Month > desde.Month ) return r;
			if ( hasta.Month < desde.Month ) return r - 1;
			if ( hasta.Day >= desde.Day ) return r;
			return r - 1;
		}

		//!method int DiffMonths(DateTime d1, DateTime d2)
		//!description Cantidad de meses entre dos fechas.
		//!return Retorna la cantidad de meses dos fechas.
		public static int DiffMonths(DateTime d1, DateTime d2) {
			int m = d2.Year * 12 + d2.Month - (d1.Year * 12 + d1.Month);
			if ( d2.Day < d1.Day ) return m - 1;
			return m;
		}

		//!method int DiffDays(DateTime d1, DateTime d2)
		//!description Cantidad de dias entre dos fechas.
		//!return Retorna la cantidad de dias dos fechas.
		public static int DiffDays(DateTime d1, DateTime d2) {
			double d = d2.ToOADate() - d1.ToOADate();
			return Convert.ToInt32( Math.Floor( Math.Abs( d ) ) * Math.Sign( d ) );
		}

		//!method DateTime AddDays(DateTime fecha, int n)
		//!description Agrega dias a una fecha.
		//!return Retorna una nueva fecha a la que se le han agregado 'n' dias a 'fecha'
		public static DateTime AddDays(DateTime fecha, int n) {
			return fecha.AddDays(n);
		}

		//!method DateTime AddMonths(DateTime fecha, int n)
		//!description Agrega meses a una fecha.
		//!return Retorna una nueva fecha a la que se le han agregado 'n' meses a 'fecha'
		public static DateTime AddMonths(DateTime fecha, int n) {
			return fecha.AddMonths(n);
		}

		//!method DateTime AddYears(DateTime d1, int n)
		//!description Agrega anios a una fecha.
		//!return Retorna una nueva fecha a la que se le han agregado 'n' anios a 'fecha'
		public static DateTime AddYears(DateTime fecha, int n) {
			return fecha.AddYears(n);
		}

		//!field DateTime nulldate
		//!description Fecha Nula
		//!return Retorna valor que representa una fecha nula (01/01/0001 00:00:00.0000000)
		public static DateTime nulldate = DateTime.MinValue;
		
		//!method DateTime NullDate()
		//!description Fecha Nula
		//!return Retorna valor que representa una fecha nula (01/01/0001 00:00:00.0000000)
		public static DateTime NullDate() {
			return DateTime.MinValue;
		}

		//!method bool IsNull(DateTime fecha)
		//!description Es Nula una fecha
		//!return Retorna verdadero si 'fecha' fecha es null o representa el valor de fecha null
		//!  (01/01/0001 00:00:00.0000000) sino retorna false.
		public static bool IsNull(DateTime fecha) {
			return (fecha == DateTime.MinValue);
		}

		//!method bool IsNull(string cadena)
		//!description Es Nula una cadena
		//!return Retorna verdadero si 'cadena' es null o representa el valor de cadena nula (cadena
		//! vacia: "")
		public static bool IsNull(string cadena) {
			return (cadena == null || cadena == "");
		}

		//!method bool IsNull(int valor)
		//!description Es Nula una cadena
		//!return Retorna verdadero si 'valor' es null o representa el valor de cadena nula (0)
		public static bool IsNull(int valor) {
			return (valor == 0);
		}

		//!method bool IsNull(double valor)
		//!description Es Nula una cadena
		//!return Retorna verdadero si 'valor' es null o representa el valor de cadena nula (0)
		public static bool IsNull(double valor) {
			return (valor == 0D);
		}

	}	
}