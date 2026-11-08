using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameStudioSite.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
    {
        public MustBeTrueAttribute() => ErrorMessage = "This box must be checked.";

        // Server side: the posted value must be true
        public override bool IsValid(object? value) => value is true;

        // Client side: jQuery Validation's "required" rule on a checkbox means "checked"
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-mustbetrue"] =
                FormatErrorMessage(context.ModelMetadata.GetDisplayName());
        }
    }
}