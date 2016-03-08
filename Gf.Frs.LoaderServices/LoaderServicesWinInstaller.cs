using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Gf.Frs.LoaderServices
{
    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class LoaderServicesWinInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public LoaderServicesWinInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "IST FRS.MT940Loader WindowService V1.0.0.0"; //This name will appear in the services.msc console.
            service.Description = "This is WCF Service host in a Managed Windows Service to run and handle the MT940 file loading, developed by InnoStark Technologies Limited, Pakistan for Gulf Finance, Saudi Arabia.";
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
