using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Generic_Service
{
	class SampleServer : IServiceObject
	{
		#region properties

		//used to contorl aborting the processing loop. Can also be refactored to use Tasks (this was written a long time ago...)
		public ManualResetEvent MRE { get; private set; }

		//lazy approach to providing a name for the service. Name can be hard-coded or made into an attribute if so desired...
		public string Name { get; private set; }

		#endregion

		#region Constructor

		public SampleServer()
		{
			this.MRE = new ManualResetEvent( false );
			this.Name = this.GetType().Name;
		}

		#endregion

		#region Start

		public void Start()
		{
			try
			{
				Log.WriteLine( "Starting up service: {0}", this.Name );
				WaitHandle[] handles = { this.MRE };
				Boolean bContinue = true;

				//we want to cycle right off the bat...
				do
				{
					if ( WaitHandle.WaitAny( handles, 500, false ) == 0 )
					{
						bContinue = false;
						break;
					}

					try
					{
						//get the next available record from sql
						//try to print
						//if fails, update the attempts count on the record
						//if succeeds delete the print item

						while ( true )
						{
							if ( WaitHandle.WaitAny( handles, 2, false ) == 0 )
							{
								bContinue = false;
								break;
							}

							//do some work
							Console.WriteLine( "Looping!" );

							//call worker function 
							if ( !ProcessWork() )
							{
								bContinue = false;
								break;
							}
						}

						//loop


					}
					catch ( System.InvalidOperationException )
					{
						//ignore this message - we're good. let other messages propagate up
					}

				}
				while ( bContinue );
				Log.WriteLine( "Shutting down service: {0}", this.Name );
			}
			catch ( Exception ex )
			{
				Log.WriteLine( ex.Message );
			}

		}

		#endregion

		#region ProcessWork

		public bool ProcessWork()
		{
			//sample of doing something.... In this case, just sleep for a minute and print the time..

			try
			{
				System.Threading.Thread.Sleep( 2000 );
				Console.WriteLine( "Pretended to do something for 2 seconds. Current time: {0}", DateTime.Now.ToString() );

				return true;
			}
			catch ( Exception ex )
			{
				Log.WriteError( "Error processing work on service '{0}', Error: ", this.Name, ex.ToString() );
				return false;
			}
		}

		#endregion
	}
}