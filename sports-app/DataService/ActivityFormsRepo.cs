using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService
{
    public class ActivityFormsRepo : BaseRepo
    {
        public ActivityFormsRepo()
        {
        }

        public ActivityFormsRepo(DataEntities db) : base(db)
        {
        }

        public List<ActivityFormsSubmittedData> GetRegistrationByCardComLpc(Guid lpc)
        {
            return db.ActivityFormsSubmittedDatas.Where(x => x.CardComLpc == lpc).ToList();
        }
        public List<ActivityFormsSubmittedData> GetRegistrationByLiqPayOrderId(Guid orderId)
        {
            return db.ActivityFormsSubmittedDatas.Where(x => x.LiqPayOrderId == orderId).ToList();
        }

        public ActivityForm GetByActivityId(int activityId)
        {
            return db.ActivityForms.FirstOrDefault(x => x.ActivityId == activityId);
        }

        public void RemoveFormDetails(int formId)
        {
            var details = db.ActivityFormsDetails.Where(x => x.FormId == formId);

            db.ActivityFormsDetails.RemoveRange(details);

            db.SaveChanges();
        }
    }
}
