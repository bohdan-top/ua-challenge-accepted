using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SitemapGenerator.Models {
	public class GenerateContext : IValidatableObject {
		[Required(ErrorMessage="Обов'язкове поле")]

		[Display(Name = "URL сайту")]
		public string Path { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			Uri result;
			if (!Uri.TryCreate(Path, UriKind.Absolute, out result))
			{
				yield return new ValidationResult("Невірний формат посилання", new[] { "Path" });
			}
		}
	}
}