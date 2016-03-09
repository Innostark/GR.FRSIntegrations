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
    
    public partial class OracleGLLoad
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OracleGLLoad()
        {
            this.Loads = new HashSet<Load>();
            this.OracleGLEntries = new HashSet<OracleGLEntry>();
        }
    
        public long OracleGLLoadId { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public int OracleGLEntryCount { get; set; }
        public long FileContentId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedOn { get; set; }
        public byte StatusId { get; set; }
    
        public virtual FileContent FileContent { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Load> Loads { get; set; }
        public virtual Status Status { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OracleGLEntry> OracleGLEntries { get; set; }
    }
}
