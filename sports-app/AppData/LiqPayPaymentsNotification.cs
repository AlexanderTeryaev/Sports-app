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
    
    public partial class LiqPayPaymentsNotification
    {
        public int Id { get; set; }
        public System.Guid OrderId { get; set; }
        public string Status { get; set; }
        public string JsonData { get; set; }
        public System.DateTime DateCreated { get; set; }
    }
}