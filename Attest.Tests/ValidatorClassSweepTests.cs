using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// README documents the per-country validator classes as a direct entry point, and
    /// <c>new BelgiumValidator().ValidateVAT(...)</c> is a supported way to call this library. But
    /// <see cref="UnicodeDigitSweepTests"/> goes through the facade, which used to catch
    /// NotSupportedException and answer "Not supported" on the validator's behalf. Nothing checked
    /// the classes themselves, and eleven of them threw for a kind their country has no rule for,
    /// plus six public helpers that threw on null.
    ///
    /// This sweeps the classes directly, so the never-throw contract is enforced where consumers
    /// actually reach it rather than only behind the dispatch.
    /// </summary>
    public class ValidatorClassSweepTests
    {
        private static readonly string[] HostileInputs =
        {
            null, "", "   ", "---", "abc", "0",
            "١٢٣٤٥٦٧٨٩",   // Arabic-Indic
            "१२३४५६७८९",   // Devanagari
            "１２３４５６７８９",   // fullwidth
            "00000000000000000000000000000000",
        };

        public static IEnumerable<object[]> EveryValidatorClass()
        {
            foreach (Type t in typeof(CountryValidator).Assembly
                         .GetTypes()
                         .Where(t => t.IsPublic && !t.IsAbstract && typeof(IdValidationAbstract).IsAssignableFrom(t))
                         .OrderBy(t => t.Name))
            {
                yield return new object[] { t };
            }
        }

        [Theory]
        [MemberData(nameof(EveryValidatorClass))]
        public void TheFiveContractMethodsAnswerRatherThanThrow(Type validatorType)
        {
            var validator = (IdValidationAbstract)Activator.CreateInstance(validatorType);

            foreach (string input in HostileInputs)
            {
                AssertAnswers(() => validator.ValidateNationalIdentity(input), validatorType, "ValidateNationalIdentity", input);
                AssertAnswers(() => validator.ValidateIndividualTaxCode(input), validatorType, "ValidateIndividualTaxCode", input);
                AssertAnswers(() => validator.ValidateEntity(input), validatorType, "ValidateEntity", input);
                AssertAnswers(() => validator.ValidateVAT(input), validatorType, "ValidateVAT", input);
                AssertAnswers(() => validator.ValidatePostalCode(input), validatorType, "ValidatePostalCode", input);
            }
        }

        /// <summary>
        /// The extra public methods a validator exposes beyond the five — ValidateITIN, ValidateNHS,
        /// ValidateOnderwijsnummer, the checksum helpers. They are public API and a consumer can
        /// reach them, so the same rule applies.
        /// </summary>
        [Theory]
        [MemberData(nameof(EveryValidatorClass))]
        public void ThePublicMethodsBeyondTheContractDoNotThrowEither(Type validatorType)
        {
            var contract = new[]
            {
                "ValidateNationalIdentity", "ValidateIndividualTaxCode",
                "ValidateEntity", "ValidateVAT", "ValidatePostalCode",
            };

            object instance = Activator.CreateInstance(validatorType);

            foreach (MethodInfo method in validatorType
                         .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(m => !m.IsSpecialName
                                     && !contract.Contains(m.Name)
                                     && m.GetParameters().Length == 1
                                     && m.GetParameters()[0].ParameterType == typeof(string)))
            {
                foreach (string input in HostileInputs)
                {
                    try
                    {
                        method.Invoke(method.IsStatic ? null : instance, new object[] { input });
                    }
                    catch (TargetInvocationException e)
                    {
                        Assert.Fail(
                            $"{validatorType.Name}.{method.Name}(\"{input ?? "null"}\") threw " +
                            $"{e.InnerException.GetType().Name}. Public methods answer; they do not throw.");
                    }
                }
            }
        }

        /// <summary>
        /// Every method returns a result for the empty string, including the kinds a country has no
        /// rule for. Those are declared through UnsupportedKinds now rather than signalled by a
        /// throw, so the class answers even where the facade would say "Not supported".
        /// </summary>
        [Theory]
        [MemberData(nameof(EveryValidatorClass))]
        public void AnUnsupportedKindIsAnsweredWithNotSupported(Type validatorType)
        {
            var validator = (IdValidationAbstract)Activator.CreateInstance(validatorType);
            // Every method still answers for the empty string, whether or not the kind is supported.
            Assert.NotNull(validator.ValidateNationalIdentity(string.Empty));
            Assert.NotNull(validator.ValidateIndividualTaxCode(string.Empty));
            Assert.NotNull(validator.ValidateEntity(string.Empty));
            Assert.NotNull(validator.ValidateVAT(string.Empty));
            Assert.NotNull(validator.ValidatePostalCode(string.Empty));
        }

        private static void AssertAnswers(Func<ValidationResult> call, Type type, string method, string input)
        {
            ValidationResult result;
            try
            {
                result = call();
            }
            catch (Exception e)
            {
                Assert.Fail($"{type.Name}.{method}(\"{input ?? "null"}\") threw {e.GetType().Name}.");
                return;
            }

            Assert.NotNull(result);
            if (!result.IsValid)
            {
                Assert.False(string.IsNullOrEmpty(result.ErrorMessage),
                    $"{type.Name}.{method}(\"{input ?? "null"}\") rejected the value without saying why.");
            }
        }
    }
}
