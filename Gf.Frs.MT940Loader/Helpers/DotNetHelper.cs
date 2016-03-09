using Gf.Frs.MT940Loader;
using Raptorious.SharpMt940Lib;

namespace Gf.Frs.IntegrationCommon.Helpers
{
    public static partial class DotNetLoaderHelper
    {
        public static MT940Balance ConvertTransactionBalanceToMT940Balance(this TransactionBalance source, byte currencyValue, string userId)
        {
            return new MT940Balance
            {
                CurrencyId = currencyValue,
                DebitOrCredit = source.DebitCredit.ConvertToDebitOrCredit(),
                EntryDate = source.EntryDate,
                Value = source.Balance.Value,
                CreatedBy = userId,
                ModifiedBy = userId
            };
        }

        public static string ConvertToDebitOrCredit(this DebitCredit source)
        {
           return (source == DebitCredit.RC || source == DebitCredit.Credit) ? "C" : "D";
        }
    }
}
