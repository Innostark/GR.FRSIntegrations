using System;

namespace Gf.Frs.OracleGLLoader.DataModel
{
    public sealed class AccountingEntry
    {
        private string _uniqueReferenceKey;
        private string _journalEntryHeaderNumber;
        private string _journalEntryDescription;
        private int? _lineNumber;
        private string _lineDescription;
        private string _accountNumber;
        private string _accountDescription;
        private string _subAccountDescription;
        private DateTime? _effectiveDate;
        private string _entrySource;
        private decimal? _enteredDr;
        private decimal? _enteredCr;
        private decimal? _accountedDr;
        private decimal? _accountedCr;
        private string _currency;
        private decimal? _exchangeRate;
        private string _period;
        private AccountingFiscalYear? _fiscalYear;
        private DateTime? _jECreationDate;
        private DateTime? _jELastUpdateDate;

        public string UniqueReferenceKey
        {
            get
            {
                return _uniqueReferenceKey;
            }

            set
            {
                _uniqueReferenceKey = value;
            }
        }

        public string JournalEntryHeaderNumber
        {
            get
            {
                return _journalEntryHeaderNumber;
            }

            set
            {
                _journalEntryHeaderNumber = value;
            }
        }

        public string JournalEntryDescription
        {
            get
            {
                return _journalEntryDescription;
            }

            set
            {
                _journalEntryDescription = value;
            }
        }

        public int? LineNumber
        {
            get
            {
                return _lineNumber;
            }

            set
            {
                _lineNumber = value;
            }
        }

        public string LineDescription
        {
            get
            {
                return _lineDescription;
            }

            set
            {
                _lineDescription = value;
            }
        }

        public string AccountNumber
        {
            get
            {
                return _accountNumber;
            }

            set
            {
                _accountNumber = value;
            }
        }

        public string AccountDescription
        {
            get
            {
                return _accountDescription;
            }

            set
            {
                _accountDescription = value;
            }
        }

        public string SubAccountDescription
        {
            get
            {
                return _subAccountDescription;
            }

            set
            {
                _subAccountDescription = value;
            }
        }

        public DateTime? EffectiveDate
        {
            get
            {
                return _effectiveDate;
            }

            set
            {
                _effectiveDate = value;
            }
        }

        public string EntrySource
        {
            get
            {
                return _entrySource;
            }

            set
            {
                _entrySource = value;
            }
        }

        public decimal? EnteredDr
        {
            get
            {
                return _enteredDr;
            }

            set
            {
                _enteredDr = value;
            }
        }

        public decimal? EnteredCr
        {
            get
            {
                return _enteredCr;
            }

            set
            {
                _enteredCr = value;
            }
        }

        public decimal? AccountedDr
        {
            get
            {
                return _accountedDr;
            }

            set
            {
                _accountedDr = value;
            }
        }

        public decimal? AccountedCr
        {
            get
            {
                return _accountedCr;
            }

            set
            {
                _accountedCr = value;
            }
        }

        public string Currency
        {
            get
            {
                return _currency;
            }

            set
            {
                _currency = value;
            }
        }

        public decimal? ExchangeRate
        {
            get
            {
                return _exchangeRate;
            }

            set
            {
                _exchangeRate = value;
            }
        }

        public string Period
        {
            get
            {
                return _period;
            }

            set
            {
                _period = value;
            }
        }

        public AccountingFiscalYear? FiscalYear
        {
            get
            {
                return _fiscalYear;
            }

            set
            {
                _fiscalYear = value;
            }
        }

        public DateTime? JECreationDate
        {
            get
            {
                return _jECreationDate;
            }

            set
            {
                _jECreationDate = value;
            }
        }

        public DateTime? JELastUpdateDate
        {
            get
            {
                return _jELastUpdateDate;
            }

            set
            {
                _jELastUpdateDate = value;
            }
        }
    }
}
