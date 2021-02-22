using AppModel;
using CmsApp.Models;
using DataService;
using Resources;
using System.ComponentModel.DataAnnotations;


namespace CmsApp.Helpers.DataNotations
{
    public class UnAssignedTeamPlayerToClubByPass : ValidationAttribute
    {
        private string v;

        public UnAssignedTeamPlayerToClubByPass(string v)
        {
            this.v = v;
        }

        protected override ValidationResult IsValid(object firstValue, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is TeamPlayerForm == false) {
                return ValidationResult.Success;
            }
            TeamPlayerForm tpf = (TeamPlayerForm)validationContext.ObjectInstance;
            if (firstValue == null && tpf.IdentNum == null)
            {
                return new ValidationResult(Messages.FieldIsRequired);
            }
            if (firstValue == null) {
                return ValidationResult.Success;
            }

            if (!tpf.IsAthletics) {
                return ValidationResult.Success;
            }
            using (var playersrepo = new PlayersRepo(new DataEntities())) {
                int? unionId;
                using (var seasonsRepo = new SeasonsRepo(new DataEntities()))
                {
                    unionId = seasonsRepo.GetById(tpf.SeasonId).UnionId;
                }
                var playerInClubs = playersrepo.GetTeamPlayersByPassport(firstValue.ToString());
                foreach (var player in playerInClubs)
                {
                    if (player.Club != null && player.Club.Union != null && player.Club.Union.UnionId == unionId.Value && player.SeasonId == tpf.SeasonId && player.Club.Union.Section.Alias == GamesAlias.Athletics && (tpf.TeamId == 0 || player.TeamId > 0)) {
                        return new ValidationResult(Messages.PlayerisAlreadyParticipatinginthisUnion);
                    }
                }
                return ValidationResult.Success;
            }  
        }
    }
}