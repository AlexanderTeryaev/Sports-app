using AppModel;
using CmsApp.Models;
using DataService;
using Resources;
using System.ComponentModel.DataAnnotations;


namespace CmsApp.Helpers.DataNotations
{
    public class UnAssignedTeamPlayerToClub : ValidationAttribute
    {
        private string v;

        public UnAssignedTeamPlayerToClub(string v)
        {
            this.v = v;
        }

        protected override ValidationResult IsValid(object firstValue, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is TeamPlayerForm == false) {
                return ValidationResult.Success;
            }

            var model = (TeamPlayerForm)validationContext.ObjectInstance;

            if (model.AlternativeId)
            {
                return ValidationResult.Success;
            }

            if (firstValue == null && model.PassportNum == null)
            {
                return new ValidationResult(Messages.FieldIsRequired);
            }

            if (firstValue == null) {
                return ValidationResult.Success;
            }

            if (firstValue.ToString().Length != 9) {
                return new ValidationResult(Messages.IdentNumMissingDigits);
            }

            using (var playersrepo = new PlayersRepo(new DataEntities()))
            {
                int? unionId;
                using (var seasonsRepo = new SeasonsRepo(new DataEntities()))
                {
                    unionId = seasonsRepo.GetById(model.SeasonId).UnionId;
                }

                var playerInClubs = playersrepo.GetTeamPlayersByIdentNum(firstValue.ToString());
                foreach (var player in playerInClubs)
                {
                    if (player.Club != null &&
                        player.Club.Union != null &&
                        player.Club.Union.Section.Alias == GamesAlias.Athletics &&
                        unionId.HasValue &&
                        player.SeasonId == model.SeasonId &&
                        player.Club.Union.UnionId == unionId.Value &&
                        (model.TeamId == 0 || player.TeamId > 0))
                    {
                        return new ValidationResult(Messages.PlayerisAlreadyParticipatinginthisUnion);
                    }
                }

                return ValidationResult.Success;
            }
        }
    }
}