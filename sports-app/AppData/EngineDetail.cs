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
    
    public partial class EngineDetail
    {
        public EngineDetail()
        {
            this.Vehicles = new HashSet<Vehicle>();
        }
    
        public int Id { get; set; }
        public string EngineNo { get; set; }
        public string EngineVolume { get; set; }
        public string EngineProduct { get; set; }
        public double MaxPowerHp { get; set; }
        public string TermsAndConditions { get; set; }
        public int NumberOfImportEnrty { get; set; }
    
        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }
}
