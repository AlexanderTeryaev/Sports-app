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
    
    public partial class VehicleProduct
    {
        public VehicleProduct()
        {
            this.VehicleDetails = new HashSet<VehicleDetail>();
            this.VehicleModels = new HashSet<VehicleModel>();
        }
    
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<VehicleDetail> VehicleDetails { get; set; }
        public virtual ICollection<VehicleModel> VehicleModels { get; set; }
    }
}
