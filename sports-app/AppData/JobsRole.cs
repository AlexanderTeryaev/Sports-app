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
    
    public partial class JobsRole
    {
        public JobsRole()
        {
            this.Jobs = new HashSet<Job>();
            this.LeagueOfficialsSettings = new HashSet<LeagueOfficialsSetting>();
            this.UnionOfficialSettings = new HashSet<UnionOfficialSetting>();
        }
    
        public int RoleId { get; set; }
        public string Title { get; set; }
        public string RoleName { get; set; }
        public int Priority { get; set; }
    
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<LeagueOfficialsSetting> LeagueOfficialsSettings { get; set; }
        public virtual ICollection<UnionOfficialSetting> UnionOfficialSettings { get; set; }
    }
}
