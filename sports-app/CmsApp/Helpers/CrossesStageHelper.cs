using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AppModel;
using Resources;

namespace CmsApp.Helpers
{
    public static class CrossesStageHelper
    {
        //The reason behind this method is that stages, groups, etc are created via DataService project, which has no access to Messages resource
        public static void SetNameForCrossesStages(List<Stage> stages)
        {
            var crossesStagesWithoutName = stages?.Where(x => x.IsCrossesStage && string.IsNullOrWhiteSpace(x.Name)).ToList();

            if (crossesStagesWithoutName?.Any() == true)
            {
                var stagesIds = crossesStagesWithoutName.Select(x => x.StageId).ToArray();

                using (var db = new DataEntities())
                {
                    var dbStages = db.Stages
                        .Include(x => x.Groups)
                        .Where(x => stagesIds.Contains(x.StageId))
                        .ToList();

                    foreach (var crossesStage in crossesStagesWithoutName)
                    {
                        var dbStage = dbStages.FirstOrDefault(x => x.StageId == crossesStage.StageId);
                        var parentStage = crossesStage.ParentStage;

                        if (parentStage != null)
                        {
                            var newName = string.Format(Messages.CrossesStageOfStage,
                                parentStage.Name ?? $"{Messages.Stage} {parentStage.Number}");

                            crossesStage.Name = newName;

                            if (dbStage != null)
                            {
                                dbStage.Name = newName;
                            }
                        }

                        foreach (var crossesGroup in crossesStage.Groups.Where(x => string.IsNullOrWhiteSpace(x.Name)).ToList())
                        {
                            crossesGroup.Name = Messages.CrossesGroup;

                            var dbGroup = dbStage?.Groups?.FirstOrDefault(x => x.GroupId == crossesGroup.GroupId);
                            if (dbGroup != null)
                            {
                                dbGroup.Name = Messages.CrossesGroup;
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}