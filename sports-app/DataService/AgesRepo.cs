using AppModel;
using AutoMapper;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService
{
    public class AgesRepo : BaseRepo
    {
        public AgesRepo() : base()
        {

        }

        public AgesRepo(DataEntities db) : base(db)
        {
        }

        public IEnumerable<AgeDto> GetAll()
        {
            var ages = db.Ages.OrderBy(t => t.AgeId);
            Mapper.Initialize(cfg => cfg.CreateMap<Age, AgeDto>());
            return Mapper.Map<IEnumerable<Age>, IEnumerable<AgeDto>>(ages);
        }

        public void Create(AgeDto ageDto)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<AgeDto, Age>());
            var age = Mapper.Map<AgeDto, Age>(ageDto);
            db.Ages.Add(age);
        }

        public void Update(AgeDto age)
        {
            var model = db.Ages.Find(age.AgeId);

            if (model == null) return;

            db.Entry(model).CurrentValues.SetValues(age);

        }

        public void Delete(int id)
        {
            var age = db.Ages.FirstOrDefault(a => a.AgeId == id);

            if (age != null)
            {
                db.Ages.Remove(age);
                db.Leagues.RemoveRange(age.Leagues);
            }

        }

        public CompetitionAge GetCompetitionAge(int? unionId, DateTime? birthDay, int? genderId)
        {
            return db.CompetitionAges.FirstOrDefault(a => a.UnionId == unionId && a.gender == genderId && a.from_birth < birthDay && birthDay < a.to_birth);
        }
    }
}
