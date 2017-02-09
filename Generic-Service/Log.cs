using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Generic_Service
{
  class Log
  {
    private static string _path = "";

    static Log()
    {
      try
      {
        _path = System.Reflection.Assembly.GetExecutingAssembly().Location;
        _path = _path.Replace( System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name, "" );
        _path += @"log.txt";

        if( File.Exists( _path ) )
          File.Delete( _path );
      }
      catch { }
    }

    public static void WriteLine( string data )
    {
      try
      {
        StreamWriter sw = new StreamWriter( _path, true );
        sw.WriteLine( data );
        sw.Close();
      }
      catch { }
      Console.WriteLine( data );
    }
    public static void WriteLine(string data, params object[] parameters)
    {
      try
      {
        string d = data;
        int i = parameters.Length;
        string s = "";
        for( int x = 0 ; x<i ; x++ )
        {
          if (parameters[x] is string)
            s = (string)parameters[x];
          else
            s = parameters[x].ToString();
          d = d.Replace("{" + x.ToString() +"}", s);
        }

        StreamWriter sw = new StreamWriter(_path, true);
        sw.WriteLine(d);
        sw.Close();
      }
      catch { }
      Console.WriteLine(data, parameters);
    }


    public static void WriteError( string data )
    {
      Console.WriteLine( data );
    }

    public static void WriteError( string data, params object[] parameters )
    {
      WriteLine( data, parameters );
    }
  }
}
