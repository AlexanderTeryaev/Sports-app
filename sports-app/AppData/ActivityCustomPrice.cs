//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class ActivityCustomPrice
    {
        public ActivityCustomPrice()
        {
            this.ActivityFormsDetails = new HashSet<ActivityFormsDetail>();
        }
    
        public int Id { get; set; }
        public Nullable<int> ActivityId { get; set; }
        public string TitleEng { get; set; }
        public string TitleHeb { get; set; }
        public decimal Price { get; set; }
        public int MaxQuantity { get; set; }
        public string CardComProductId { get; set; }
        public string TitleUk { get; set; }
        public int DefaultQuantity { get; set; }
    
        public virtual Activity Activity { get; set; }
        public virtual ICollection<ActivityFormsDetail> ActivityFormsDetails { get; set; }
    }
}
