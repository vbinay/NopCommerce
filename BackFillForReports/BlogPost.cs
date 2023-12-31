//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BackFillForReports
{
    using System;
    using System.Collections.Generic;
    
    public partial class BlogPost
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BlogPost()
        {
            this.BlogComments = new HashSet<BlogComment>();
        }
    
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string BodyOverview { get; set; }
        public bool AllowComments { get; set; }
        public int CommentCount { get; set; }
        public string Tags { get; set; }
        public Nullable<System.DateTime> StartDateUtc { get; set; }
        public Nullable<System.DateTime> EndDateUtc { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool LimitedToStores { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public Nullable<bool> IsMaster { get; set; }
        public Nullable<int> MasterId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BlogComment> BlogComments { get; set; }
        public virtual Language Language { get; set; }
    }
}
