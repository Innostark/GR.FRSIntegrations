using CsvHelper.TypeConversion;
using Gf.Frs.OracleGLLoader.DataModel;

namespace Gf.Frs.OracleGLLoader.Csv.Converter
{
    /// <summary>
    /// Converts a Accounting Fiscal Year to and from a string.
    /// </summary>
    public class AccountingFiscalYearConverter : DefaultTypeConverter
    {
        /// <summary>
        /// Converts the string to an object.
        /// </summary>
        /// <param name="options">The options to use when converting.</param>
        /// <param name="text">The string to convert to an object.</param>
        /// <returns>The object created from the string.</returns>
        public override object ConvertFromString(TypeConverterOptions options, string text)
        {
            uint ui;
            if(uint.TryParse(text, out ui))
            {
                return (AccountingFiscalYear)ui;
            }

            return base.ConvertFromString(options, text);
        }

        /// <summary>
        /// Determines whether this instance [can convert from] the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can convert from] the specified type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvertFrom(System.Type type)
        {
            // We only care about strings.
            return type == typeof(string);
        }
    }
}
