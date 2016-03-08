using CsvHelper.Configuration;
using Gf.Frs.OracleGLLoader.DataModel;
using Gf.Frs.OracleGLLoader.Csv.Converter;

namespace Gf.Frs.OracleGLLoader.Csv.Map
{
    public sealed class AccountingEntryMap: CsvClassMap<AccountingEntry>
    {
        public AccountingEntryMap()
        {
            Map(m => m.UniqueReferenceKey).Index(0);
            Map(m => m.JournalEntryHeaderNumber).Index(1);
            Map(m => m.JournalEntryDescription).Index(2);
            Map(m => m.LineNumber).Index(3);
            Map(m => m.LineDescription).Index(4);
            Map(m => m.AccountNumber).Index(5);
            Map(m => m.AccountDescription).Index(6);
            Map(m => m.SubAccountDescription).Index(7);
            Map(m => m.EffectiveDate).Index(8).TypeConverterOption("dd-MMM-yyyy HH:mm:fffff"); ;
            Map(m => m.EntrySource).Index(9);
            Map(m => m.EnteredDr).Index(10);
            Map(m => m.EnteredCr).Index(11);
            Map(m => m.AccountedDr).Index(12);
            Map(m => m.AccountedCr).Index(13);
            Map(m => m.Currency).Index(14);
            Map(m => m.ExchangeRate).Index(15);
            Map(m => m.Period).Index(16);
            Map(m => m.FiscalYear).Index(17).TypeConverter<AccountingFiscalYearConverter>();
            Map(m => m.JECreationDate).Index(18).TypeConverterOption("dd-MMM-yyyy HH:mm:fffff"); ;
            Map(m => m.JELastUpdateDate).Index(19).TypeConverterOption("dd-MMM-yyyy HH:mm:fffff"); ;
        }
    }
}
