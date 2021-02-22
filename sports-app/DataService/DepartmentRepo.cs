using DataService.DTO;
using System.Collections.Generic;
using System.Linq;
using System;
using AppModel;

namespace DataService
{
    public class DepartmentRepo : BaseRepo
    {
        public ICollection<DepartmentDTO> GetByParentClubId(int parentClubId, int? seasonId)
        {
            var clubs = db.Clubs.Where(c => c.ParentClubId.HasValue && c.ParentClubId == parentClubId && c.IsArchive == false);

            return clubs.Select(c => new DepartmentDTO { Id = c.ClubId, Title = c.Name, SportId = c.SportSectionId }).ToList();
        }

        public Club Create(DepartmentDTO departmentDto)
        {
            var section = db.Sections.FirstOrDefault(s => s.Alias == SectionAliases.MultiSport);
            if (section == null) throw new Exception("MultiSport section not found");

            var department = new Club
            {
                ClubId = departmentDto.Id,
                Name = departmentDto.Title,
                ParentClubId = departmentDto.ParentClubId,
                SportSectionId = departmentDto.SportId,
                SectionId = section.SectionId
            };
            db.Clubs.Add(department);
            db.SaveChanges();

            return department;
        }

        public int? UpdateDepartment(DepartmentDTO departmentDto)
        {
            try
            {
                var department = db.Clubs.Find(departmentDto.Id);
                if (department != null)
                {
                    department.Name = departmentDto.Title;
                    department.SportSectionId = departmentDto.SportId;
                    db.SaveChanges();
                }
                return department?.SectionId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int? DeleteDepartment(int id)
        {
            try
            {
                var department = db.Clubs.Find(id);
                if (department != null)
                {
                    department.IsArchive = true;
                    db.SaveChanges();
                }
                return department.SectionId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<int> GetByManagerId(int adminId)
        {
            return db.UsersJobs
                .Where(j => j.UserId == adminId)
                .Select(j => j.ClubId)
                .Where(u => u != null)
                .Cast<int>()
                .Distinct()
                .ToList();
        }
    }
}
