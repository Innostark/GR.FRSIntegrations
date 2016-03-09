using CsvHelper;
using Gf.Frs.OracleGLLoader.Csv.Map;
using Gf.Frs.OracleGLLoader.DataModel;
using System.IO;
using System.Linq;

namespace Gf.Frs.ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {

            //MT940Loader.MT940Loader l = new MT940Loader.MT940Loader(@"C:\ISTWORK\CODE\GF.FRS\GF.FRS.MT940Loader\Samples\KSA\SCB Vostro - 031001548008 -940d.txt",
            //                                          "{1:F01AAALSARIAXXX.SN...ISN.}{2:I940SCBLGB20XWEBN}{3:{108:xxxxx}}{4:",
            //                                          "-}");
            //l.ValidateFile();

            var csv = new CsvReader(new StreamReader(@"C:\ISTWORK\CODE\git1\GR.FRSIntegrations\Gf.Frs.OracleGLLoader\Samples\KSA\GLExtract From GFC_KSA PROD 2.CSV"));

            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.RegisterClassMap<AccountingEntryMap>();
            //int i = 0;
            //while (csv.Read())
            //{
            //    i++;
            //    var row = csv.GetRecord<AccountingEntry>();
            //}


            var records = csv.GetRecords<AccountingEntry>().ToList();

            var q = 123;
        }
    }
}
