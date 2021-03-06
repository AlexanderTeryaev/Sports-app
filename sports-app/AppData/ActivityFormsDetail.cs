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
    
    public partial class ActivityFormsDetail
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string PropertyName { get; set; }
        public string Type { get; set; }
        public string LabelTextEn { get; set; }
        public string LabelTextHeb { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanBeRequired { get; set; }
        public bool CanBeDisabled { get; set; }
        public string FieldNote { get; set; }
        public string CustomDropdownValues { get; set; }
        public bool CanBeRemoved { get; set; }
        public bool HasOptions { get; set; }
        public Nullable<int> CustomPriceId { get; set; }
        public string LabelTextUk { get; set; }
    
        public virtual ActivityCustomPrice ActivityCustomPrice { get; set; }
        public virtual ActivityForm ActivityForm { get; set; }
    }
}
