using AppModel;
using CmsApp.Models;
using DataService;
using System.Collections.Generic;

namespace CmsApp.Helpers
{
    public class KarateRefereeRanksHelper
    {
        public void SetRefereesRanks(CreateWorkerForm frm, IEnumerable<KarateRefereesRank> refereesRanks)
        {
            foreach (var refereeRank in refereesRanks)
            {
                if (string.Equals(nameof(frm.KumiteJudgeBCompleteIsrael), refereeRank.Type))
                    frm.KumiteJudgeBCompleteIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeBValidityIsrael), refereeRank.Type))
                    frm.KumiteJudgeBValidityIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeACompleteIsrael), refereeRank.Type))
                    frm.KumiteJudgeACompleteIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeAValidityIsrael), refereeRank.Type))
                    frm.KumiteJudgeAValidityIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeBCompleteIsrael), refereeRank.Type))
                    frm.RefereeBCompleteIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeBValidityIsrael), refereeRank.Type))
                    frm.RefereeBValidityIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeACompleteIsrael), refereeRank.Type))
                    frm.RefereeACompleteIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeAValidityIsrael), refereeRank.Type))
                    frm.RefereeAValidityIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeBCompleteIsrael), refereeRank.Type))
                    frm.KataJudgeBCompleteIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeBValidityIsrael), refereeRank.Type))
                    frm.KataJudgeBValidityIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeACompleteIsrael), refereeRank.Type))
                    frm.KataJudgeACompleteIsrael = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeAValidityIsrael), refereeRank.Type))
                    frm.KataJudgeAValidityIsrael = refereeRank.Date;

                if (string.Equals(nameof(frm.KumiteJudgeBCompleteEKF), refereeRank.Type))
                    frm.KumiteJudgeBCompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeBValidityEKF), refereeRank.Type))
                    frm.KumiteJudgeBValidityEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeACompleteEKF), refereeRank.Type))
                    frm.KumiteJudgeACompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeAValidityEKF), refereeRank.Type))
                    frm.KumiteJudgeAValidityEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeBCompleteEKF), refereeRank.Type))
                    frm.RefereeBCompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeBValidityEKF), refereeRank.Type))
                    frm.RefereeBValidityEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeACompleteEKF), refereeRank.Type))
                    frm.RefereeACompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeAValidityEKF), refereeRank.Type))
                    frm.RefereeAValidityEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeBCompleteEKF), refereeRank.Type))
                    frm.KataJudgeBCompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeBValidityEKF), refereeRank.Type))
                    frm.KataJudgeBValidityEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeACompleteEKF), refereeRank.Type))
                    frm.KataJudgeACompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeAValidityEKF), refereeRank.Type))
                    frm.KataJudgeAValidityEKF = refereeRank.Date;


                if (string.Equals(nameof(frm.KumiteJudgeBCompleteWKF), refereeRank.Type))
                    frm.KumiteJudgeBCompleteWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeBValidityWKF), refereeRank.Type))
                    frm.KumiteJudgeBValidityWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeACompleteWKF), refereeRank.Type))
                    frm.KumiteJudgeACompleteWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KumiteJudgeAValidityWKF), refereeRank.Type))
                    frm.KumiteJudgeAValidityWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeBCompleteWKF), refereeRank.Type))
                    frm.RefereeBCompleteWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeBValidityWKF), refereeRank.Type))
                    frm.RefereeBValidityWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeACompleteWKF), refereeRank.Type))
                    frm.RefereeACompleteEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.RefereeAValidityWKF), refereeRank.Type))
                    frm.RefereeAValidityWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeBCompleteWKF), refereeRank.Type))
                    frm.KataJudgeBCompleteWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeBValidityWKF), refereeRank.Type))
                    frm.KataJudgeBValidityEKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeACompleteWKF), refereeRank.Type))
                    frm.KataJudgeACompleteWKF = refereeRank.Date;
                if (string.Equals(nameof(frm.KataJudgeAValidityWKF), refereeRank.Type))
                    frm.KataJudgeAValidityWKF = refereeRank.Date;

            }
        }
        public void AlterRefereesRanks(JobsRepo jobsRepo, CreateWorkerForm frm, UsersJob userJob)
        {
            if (frm.KumiteJudgeBCompleteIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeBCompleteIsrael), frm.KumiteJudgeBCompleteIsrael.Value);
            if (frm.KumiteJudgeBValidityIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeBValidityIsrael), frm.KumiteJudgeBValidityIsrael.Value);
            if (frm.KumiteJudgeACompleteIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeACompleteIsrael), frm.KumiteJudgeACompleteIsrael.Value);
            if (frm.KumiteJudgeAValidityIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeAValidityIsrael), frm.KumiteJudgeAValidityIsrael.Value);
            if (frm.RefereeBCompleteIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeBCompleteIsrael), frm.RefereeBCompleteIsrael.Value);
            if (frm.RefereeBValidityIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeBValidityIsrael), frm.RefereeBValidityIsrael.Value);
            if (frm.RefereeACompleteIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeACompleteIsrael), frm.RefereeACompleteIsrael.Value);
            if (frm.RefereeAValidityIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeAValidityIsrael), frm.RefereeAValidityIsrael.Value);
            if (frm.KataJudgeBCompleteIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeBCompleteIsrael), frm.KataJudgeBCompleteIsrael.Value);
            if (frm.KataJudgeBValidityIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeBValidityIsrael), frm.KataJudgeBValidityIsrael.Value);
            if (frm.KataJudgeACompleteIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeACompleteIsrael), frm.KataJudgeACompleteIsrael.Value);
            if (frm.KataJudgeAValidityIsrael.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeAValidityIsrael), frm.KataJudgeAValidityIsrael.Value);
            if (frm.KumiteJudgeBCompleteEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeBCompleteEKF), frm.KumiteJudgeBCompleteEKF.Value);
            if (frm.KumiteJudgeBValidityEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeBValidityEKF), frm.KumiteJudgeBValidityEKF.Value);
            if (frm.KumiteJudgeACompleteEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeACompleteEKF), frm.KumiteJudgeACompleteEKF.Value);
            if (frm.KumiteJudgeAValidityEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeAValidityEKF), frm.KumiteJudgeAValidityEKF.Value);
            if (frm.RefereeBCompleteEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeBCompleteEKF), frm.RefereeBCompleteEKF.Value);
            if (frm.RefereeBValidityEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeBValidityEKF), frm.RefereeBValidityEKF.Value);
            if (frm.RefereeACompleteEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeACompleteEKF), frm.RefereeACompleteEKF.Value);
            if (frm.RefereeAValidityEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeAValidityEKF), frm.RefereeAValidityEKF.Value);
            if (frm.KataJudgeBCompleteEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeBCompleteEKF), frm.KataJudgeBCompleteEKF.Value);
            if (frm.KataJudgeBValidityEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeBValidityEKF), frm.KataJudgeBValidityEKF.Value);
            if (frm.KataJudgeACompleteEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeACompleteEKF), frm.KataJudgeACompleteEKF.Value);
            if (frm.KataJudgeAValidityEKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeAValidityEKF), frm.KataJudgeAValidityEKF.Value);
            if (frm.KumiteJudgeBCompleteWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeBCompleteWKF), frm.KumiteJudgeBCompleteWKF.Value);
            if (frm.KumiteJudgeBValidityWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeBValidityWKF), frm.KumiteJudgeBValidityWKF.Value);
            if (frm.KumiteJudgeACompleteWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeACompleteWKF), frm.KumiteJudgeACompleteWKF.Value);
            if (frm.KumiteJudgeAValidityWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KumiteJudgeAValidityWKF), frm.KumiteJudgeAValidityWKF.Value);
            if (frm.RefereeBCompleteWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeBCompleteWKF), frm.RefereeBCompleteWKF.Value);
            if (frm.RefereeBValidityWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeBValidityWKF), frm.RefereeBValidityWKF.Value);
            if (frm.RefereeACompleteWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeACompleteWKF), frm.RefereeACompleteWKF.Value);
            if (frm.RefereeAValidityWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.RefereeAValidityWKF), frm.RefereeAValidityWKF.Value);
            if (frm.KataJudgeBCompleteWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeBCompleteWKF), frm.KataJudgeBCompleteWKF.Value);
            if (frm.KataJudgeBValidityWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeBValidityWKF), frm.KataJudgeBValidityWKF.Value);
            if (frm.KataJudgeACompleteWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeACompleteWKF), frm.KataJudgeACompleteWKF.Value);
            if (frm.KataJudgeAValidityWKF.HasValue)
                jobsRepo.CreteRankForReferee(userJob.Id, nameof(frm.KataJudgeAValidityWKF), frm.KataJudgeAValidityWKF.Value);
        }
    }
}