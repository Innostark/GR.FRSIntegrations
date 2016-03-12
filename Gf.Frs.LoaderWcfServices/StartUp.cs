using Gf.Frs.LoaderWcfServices.Bank.MT940;
using System;
using System.Linq;
using System.ServiceModel;

namespace Gf.Frs.LoaderWcfServices
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var svcHost = new ServiceHost(typeof(BankMT940Loader));
            Console.WriteLine("Available Endpoints :\n");
            svcHost.Description.Endpoints.ToList().ForEach(endpoints => Console.WriteLine(endpoints.Address.ToString()));
            svcHost.Open();
            Console.ReadLine();
        }
    }
}