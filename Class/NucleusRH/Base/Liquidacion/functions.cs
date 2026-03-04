public static bool isDigit( char c )
{
	return (c >= '0') && ('9' >= c);
}

public static double str2dbl( string s )
{
	char[] a = s.ToCharArray();
	int signo = 1;
	double divisor = 1, res = 0;
	bool dec = false;

	for ( int i = 0; i < a.Length; i++ )
		if ( a[i] == '-' )
			signo = -1;
		else if ( a[i] == '.' )
			dec = true;
		else if ( isDigit(a[i]) )
		{
			res *= 10;
			res += (a[i] - '0');
			if ( dec ) divisor *= 10;
		}

	return signo * (res / divisor);
}

public static DateTime str2date( string s )
{
	s = s.Trim();

	if ( s.Length == 8 )
		return new DateTime( Convert.ToInt32(s.Substring(0,4),10), Convert.ToInt32(s.Substring(4,2),10), Convert.ToInt32(s.Substring(6,2),10), 0, 0, 0, 0 );
	else if ( s.Length == 6 )
		return new DateTime( 0, 0, 0, Convert.ToInt32(s.Substring(0,2),10), Convert.ToInt32(s.Substring(2,2),10), Convert.ToInt32(s.Substring(4,2),10), 0 );
	else if ( s.Length == 14 )
		return new DateTime( Convert.ToInt32(s.Substring(0,4),10), Convert.ToInt32(s.Substring(4,2),10), Convert.ToInt32(s.Substring(6,2),10), Convert.ToInt32(s.Substring(8,2),10), Convert.ToInt32(s.Substring(10,2),10), Convert.ToInt32(s.Substring(12,2),10), 0 );

	return new DateTime();
}

public static string dbl2str( double s )
{
	return s.ToString().Replace(",",".");
}

public static string date2str( DateTime d )
{
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

public static string rtrim( string s )
{
	while ( s.Substring( s.Length - 1, 1 ) == " " )
		s = s.Substring( 0, s.Length - 1);

	return s;
}

public static string ltrim( string s )
{
	while ( s.Substring( 0, 1 ) == " " )
		s = s.Substring( 1, s.Length - 1);

	return s;
}

public static string trim( string s )
{
	return s.Trim();
}


public static int parseInt( string s )
{
	return Convert.ToInt32( s );
}

public static int parseInt( double d )
{
	return Convert.ToInt32( d );
}

public static double round( double n, int d )
{
	return System.Math.Round( n, d );
}

public static int round( double n )
{
	return Convert.ToInt32(System.Math.Round( n, 0 ));
}

public static int day( System.DateTime d )
{
	return d.Day;
}
		
public static int month( System.DateTime d )
{
	return d.Month;
}

public static int year( System.DateTime d )
{
	return d.Year;
}

public static DateTime date()
{
	return DateTime.Now;
}

public static DateTime date( int day, int month, int year )
{
	return new DateTime( year, month, day );
}


public static int diffYears( DateTime d1, DateTime d2 )
{
	int r = d2.Year - d1.Year;
	if ( d2.Month > d1.Month )	return r;
	if ( d2.Month < d1.Month )	return r - 1;
	if ( d2.Day > d1.Day )		return r;
	return r - 1;
}

public static int diffMonths( DateTime d1, DateTime d2 )
{
	int m = d2.Year * 12 + d2.Month - (d1.Year * 12 + d1.Month);
	if ( d2.Day < d1.Day ) return m - 1;
	return m;
}

public static int diffDays( DateTime d1, DateTime d2 )
{
	double d = d2.ToOADate() - d1.ToOADate();
	return Convert.ToInt32( Math.Floor( Math.Abs( d ) ) * Math.Sign( d ) );
}

public static DateTime addDays( DateTime d1, int n )
{
	return new DateTime( d1.Year, d1.Month, d1.Day + n );
}

public static DateTime addMonths( DateTime d1, int n )
{
	return new DateTime( d1.Year, d1.Month + n, d1.Day );
}

public static DateTime addYears( DateTime d1, int n )
{
	return new DateTime( d1.Year + n, d1.Month, d1.Day );
}

