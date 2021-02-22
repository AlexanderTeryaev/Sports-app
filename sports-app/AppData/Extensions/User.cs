using System;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace AppModel
{
	public partial class User
	{
		/// <summary>
		/// OMFG, why is this not a method? It's state-dependant, it's non-serializable, it's even non deterministic. 
		/// Should be a method at least
		/// </summary>
		public string FullName
		{
			get => GetFullName(Thread.CurrentThread.CurrentUICulture, FirstName, LastName, MiddleName);
			set =>
				throw new InvalidOperationException("Set of User.FullName is not allowed. Use FirstName, LastName and MiddleName fields");
		}

		public static string GetFullName(System.Globalization.CultureInfo culture, string firstName, string lastName, string middleName) =>
			"uk".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)
				? $"{lastName} {firstName} {middleName}"
				: $"{firstName} {(!string.IsNullOrWhiteSpace(middleName) ? $"{middleName} " : string.Empty)}{lastName}";
	}
}
