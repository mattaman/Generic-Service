using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace Generic_Service
{
	public interface IServiceObject
	{
		void Start();
		string Name { get; }
		ManualResetEvent MRE { get; }
	}

	public class SvrBase : ServiceBase
	{
		private static readonly List<IServiceObject> _serviceObjects = new List<IServiceObject>();
		private static readonly List<Thread> _serviceThreads = new List<Thread>();

		//will be set on startup
		private static string _executableName = null;

		//this can be whatever you'd like. It's the base name (in the event you need to create multiple services grouped together
		//additionally you can move this to the app.config
		public  const string Name = "Generic-Tester";


		static SvrBase()
		{
			//highly optimistic approach to getting the name of the executable...
			_executableName = System.Reflection.Assembly.GetExecutingAssembly().Location.Split("\\".ToCharArray()).Last();

			//look for every object that implements IServiceObject. This code could be a little cleaner, but it's only called once...
			foreach ( Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where( c => c.GetInterfaces().Contains( typeof( IServiceObject ) ) ) )
			{
				IServiceObject service = Activator.CreateInstance(t) as IServiceObject;
				_serviceObjects.Add( service );
				_serviceThreads.Add( new Thread( new ThreadStart( service.Start ) ) );
			}
		}

		internal static void ConsoleModeStart()
		{
			_serviceThreads.ForEach( c=> c.Start() );
		}


		internal static void ConsoleModeStop()
		{
			for ( int i = 0; i <= _serviceObjects.Count; i++ )
			{
				try
				{
					_serviceObjects[i].MRE.Set();
					_serviceThreads[i].Join();
				}
				catch { }
			}
		}

		protected override void OnStart( string[] args )
		{
			ConsoleModeStart();
			base.OnStart( args );
		}

		protected override void OnStop()
		{
			ConsoleModeStop();
			base.OnStop();
		}

		static void Main( String[] args )
		{
			try
			{

				if ( args.Length == 0 )
				{
					//no args, just run the service
					ServiceBase.Run( new SvrBase() );
				}
				else                                                                
				{
					//we're installing or uninstalling
					ArrayList options = new ArrayList();
					Boolean toConsole = false;
					Boolean toUnInstall = false;
					Boolean toInstall = false;
					Boolean toPrintHelp = false;
					TransactedInstaller myTransactedInstaller = new TransactedInstaller();

					String myOption;
					AssemblyInstaller myAssemblyInstaller;
					InstallContext myInstallContext;

					try
					{
						for ( int i = 0; i < args.Length; i++ )
						{
							String argument = args[i].Trim();
							// Process the arguments.
							if ( argument.StartsWith( "/" ) || argument.StartsWith( "-" ) )
							{
								myOption = argument.Substring( 1 );

								//if they want to run in console mode, pass in the /c
								if ( String.Compare( myOption, "c", true ) == 0 )
								{
									toConsole = true;
									break;
								}


								//check for instance name
								if ( myOption.StartsWith( "n=" ) && myOption.Trim().Length > 2 )
								{
									options.Add( myOption );
									continue;
								}

								// Determine whether the option is to 'install' an assembly.
								if ( ( String.Compare( myOption, "i", true ) == 0 ) || ( String.Compare( myOption, "install", true ) == 0 ) )
								{
									toInstall = true;
									options.Add( myOption );
									continue;
								}
								// Determine whether the option is to 'uninstall' an assembly.
								if ( ( String.Compare( myOption, "u", true ) == 0 ) || ( String.Compare( myOption, "uninstall", true ) == 0 ) )
								{
									toUnInstall = true;
									options.Add( myOption );
									continue;
								}
								// Determine whether the option is for printing help information.
								if ( ( String.Compare( myOption, "?", true ) == 0 ) || ( String.Compare( myOption, "help", true ) == 0 ) )
								{
									toPrintHelp = true;
									break;
								}
								// Add the option encountered to the list of all options encountered for the current assembly.
								// if instance=val or i=val was included in the cmdline, it'll go through in this
							}
						}

						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//****************************************************************************
						//GO CONSOLE MODE BABY!!! - call the 2 methods defined at the top
						if ( toConsole )
						{
							ConsoleModeStart();
							Console.WriteLine( "Press enter key to quit" );
							Console.ReadLine();
							ConsoleModeStop();
							Console.WriteLine( "Application Terminated" );
							return;
						}

						// IF THE above statement is true, the rest of this will never happen


						// If user requested help or didn't provide any assemblies to install then print help message.
						if ( toPrintHelp )
						{
							PrintHelpMessage();
							return;
						}

						// Create an instance of 'AssemblyInstaller' that installs the given assembly.
						String[] parameters = (String[])options.ToArray(typeof(string));
						myAssemblyInstaller = new AssemblyInstaller( _executableName, parameters );
						// Add the instance of 'AssemblyInstaller' to the 'TransactedInstaller'.
						myTransactedInstaller.Installers.Add( myAssemblyInstaller );

						// Create an instance of 'InstallContext' with the options specified.
						myInstallContext = new InstallContext( "Install.log", parameters );
						myTransactedInstaller.Context = myInstallContext;

						// Install or Uninstall an assembly depending on the option provided.
						if ( ( toInstall && toUnInstall ) )
							Console.WriteLine( " CANNOT DO BOTH INSTALL AND UNINSTALL." );
						else
						{
							if ( toInstall )
								myTransactedInstaller.Install( new Hashtable() );
							if ( toUnInstall )
								myTransactedInstaller.Uninstall( null );
						}
					}
					catch ( Exception e )
					{
						Log.WriteLine( " Exception raised : {0}", e.Message );
					}
				}
			}
			catch ( Exception e )
			{
				Log.WriteLine( " Exception raised : {0}", e.Message );
			}

		}

		public static void PrintHelpMessage()
		{
			try
			{
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine( "*************** SERVER EXECUTION HELP ***************" );
				Console.WriteLine( "Usage : " + _executableName + " [ [/i | /install] | [/u | /uninstall]] [/n=] " );
				Console.WriteLine( _executableName + " is what the name of this exe should be" );
				Console.WriteLine();
				Console.WriteLine( "  CONSOLE MODE:  use the /c switch" );
				Console.WriteLine();
				Console.WriteLine( " RULE #1: parameters are case-sensitive, and must be lowercase" );
				Console.WriteLine();
				Console.WriteLine( " NOTE: if the exe name doesn't match the name shown above," );
				Console.WriteLine( "       no action will be taken." );
				Console.WriteLine();
				Console.WriteLine( " If /i or /install option is given, create the service:" );
				Console.WriteLine( " If /u or /uninstall is uninstalls the service (if present)." );
				Console.WriteLine();
				Console.WriteLine( " NOTE: if you enter /u or /uninstall and the service doesn't" );
				Console.WriteLine( "       exist, no action will be taken." );
				Console.WriteLine();
				Console.WriteLine( " Use the /n= option to specify the name of a new instance." );
				Console.WriteLine();
				Console.WriteLine( "  Examples:" );
				Console.WriteLine();
				Console.WriteLine( "   no /n  would create a new service called: " + SvrBase.Name + ".Net" );
				Console.WriteLine( "   /n=1   would create a new service called: " + SvrBase.Name + ".Net.1" );
				Console.WriteLine( "   /n=Dev would create a new service called: " + SvrBase.Name + ".Dev" );
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine();
			}
			catch ( Exception e )
			{
				System.Diagnostics.Debug.Assert( false, e.Message );
			}
		}
	}
}