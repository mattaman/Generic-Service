using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;

namespace Generic_Service
{


  [RunInstallerAttribute(true)]
  public class SvrInstall: Installer
  {
    private ServiceInstaller serviceInstaller;
    private ServiceProcessInstaller processInstaller;
    private String instanceName = null;


    public override void Uninstall(IDictionary savedState)
    {
      try
      {
        if(Context.Parameters.ContainsKey("n"))
        {
          instanceName = "." + Context.Parameters["n"];
          instanceName = instanceName.Replace(" ","");
          serviceInstaller.ServiceName = SvrBase.Name + instanceName;
          serviceInstaller.DisplayName = SvrBase.Name + instanceName;
				
        }
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.WriteLine(e.Message);
      }
      base.Uninstall (savedState);
    }

    public override void Install(IDictionary stateSaver)
    {
      try
      {
        if(Context.Parameters.ContainsKey("n"))
        {
          instanceName = "." + Context.Parameters["n"];
          instanceName = instanceName.Replace(" ","");
          serviceInstaller.ServiceName = SvrBase.Name + instanceName;
          serviceInstaller.DisplayName = SvrBase.Name + instanceName;
				
        }
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.WriteLine(e.Message);
      }

      base.Install (stateSaver);
    }

	
    public SvrInstall()
    {


      // Instantiate installers for process and services.
      processInstaller = new ServiceProcessInstaller();
      serviceInstaller = new ServiceInstaller();

      // The services run under the system account.
      processInstaller.Account = ServiceAccount.LocalSystem;

      // The services are started manually.
      serviceInstaller.StartType = ServiceStartMode.Automatic;

      serviceInstaller.ServiceName = SvrBase.Name;
      serviceInstaller.DisplayName = SvrBase.Name;

      // Add installers to collection. Order is not important.
      Installers.Add(serviceInstaller);
      Installers.Add(processInstaller);
    }
  }
}
