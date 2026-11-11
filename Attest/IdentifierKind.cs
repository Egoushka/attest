using System;

namespace Attest
{
    /// <summary>
    /// The kinds of identifier a country can issue, so that a caller can ask for a whole category
    /// instead of naming one method. <see cref="PostalCode"/> is deliberately outside
    /// <see cref="Any"/>: it identifies a place rather than a person or a company.
    /// </summary>
    [Flags]
    public enum IdentifierKind
    {
        /// <summary>National identity number of a natural person.</summary>
        PersonalId = 1 << 0,

        /// <summary>Tax identification number of a natural person.</summary>
        PersonalTaxCode = 1 << 1,

        /// <summary>Registration number of a company or other legal entity.</summary>
        CompanyNumber = 1 << 2,

        /// <summary>VAT registration number.</summary>
        Vat = 1 << 3,

        /// <summary>Postal code. Not part of <see cref="Any"/>.</summary>
        PostalCode = 1 << 4,

        /// <summary>Bank account number, such as an IBAN. Not part of <see cref="Any"/>.</summary>
        BankAccount = 1 << 5,

        /// <summary>Health insurance or patient number (NHS, AMKA, AIC). Not part of <see cref="Any"/>.</summary>
        Healthcare = 1 << 6,

        /// <summary>Financial instrument identifier (ISIN, CUSIP, FIGI, LEI). Not part of <see cref="Any"/>.</summary>
        Security = 1 << 7,

        /// <summary>Trade item identifier (EAN, GTIN, ISBN, ISSN). Not part of <see cref="Any"/>.</summary>
        TradeItem = 1 << 8,

        /// <summary>Telecom equipment or subscriber identifier (IMEI, IMSI, MAC). Not part of <see cref="Any"/>.</summary>
        Telecom = 1 << 9,

        /// <summary>Vehicle identifier. Not part of <see cref="Any"/>.</summary>
        Vehicle = 1 << 10,

        /// <summary>Any identifier that fits none of the other kinds. Not part of <see cref="Any"/>.</summary>
        Other = 1 << 11,

        /// <summary>Every identifier belonging to a natural person.</summary>
        Person = PersonalId | PersonalTaxCode,

        /// <summary>Every identifier belonging to a company or other legal entity.</summary>
        Business = CompanyNumber | Vat,

        /// <summary>Every identifier belonging to a person or a company.</summary>
        Any = Person | Business,
    }
}
