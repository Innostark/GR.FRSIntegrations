//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gf.Frs.OracleGLLoader.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class FiscalYear
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FiscalYear()
        {
            this.OracleGLEntries = new HashSet<OracleGLEntry>();
        }
    
        public short Value { get; set; }
        public string Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OracleGLEntry> OracleGLEntries { get; set; }
    }
}
