#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace Attest.DataAnnotations
{
    /// <summary>
    /// When applied to a <see cref="string" /> property or parameter, validates it against the
    /// identifier type with the given id, for example <c>[Identifier("be.vat")]</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class IdentifierAttribute : ValidationAttribute
    {
        /// <summary>Validates against the type with the given id or alias.</summary>
        /// <param name="typeId">An id from <see cref="Identifiers.All"/>, such as "be.vat", or an alias.</param>
        /// <exception cref="ArgumentException">No type has this id or alias.</exception>
        public IdentifierAttribute(string typeId)
        {
            if (Identifiers.Find(typeId) == null)
            {
                throw new ArgumentException("Unknown identifier type: " + typeId, nameof(typeId));
            }

            TypeId = typeId;
        }

        /// <summary>The id of the type the annotated member is validated against.</summary>
        public string TypeId { get; }

        /// <summary>
        /// Null passes, so that whether the member is optional stays <c>RequiredAttribute</c>'s
        /// decision; a value that is not a string fails rather than throwing. A type Attest has no
        /// rule for (<see cref="Verdict.NotCovered"/>) fails too, because a member that cannot be
        /// checked must not look checked. The reason is recorded under
        /// <c>validationContext.Items["Error"]</c>, overwriting whatever was there.
        /// </summary>
        protected override System.ComponentModel.DataAnnotations.ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return System.ComponentModel.DataAnnotations.ValidationResult.Success;
            }

            IdentifierResult result = value is string text
                ? Identifiers.Validate(text, TypeId)
                : default;

            if (result.IsValid)
            {
                return System.ComponentModel.DataAnnotations.ValidationResult.Success;
            }

            validationContext.Items["Error"] = result.Detail;

            return new System.ComponentModel.DataAnnotations.ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                validationContext.MemberName != null ? new[] { validationContext.MemberName } : null);
        }
    }
}
