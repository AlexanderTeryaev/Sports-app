using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CmsApp.Helpers.DataNotations
{
    public class NumberIsValid : ValidationAttribute
    {
        private readonly int[] _validNumbers;

        public NumberIsValid(int[] validNumbers)
        {
            _validNumbers = validNumbers;
        }

        public override bool IsValid(object value)
        {
            var number = (int)value;
            return _validNumbers.Any(x => x == number);
        }
    }
}
