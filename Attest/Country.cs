namespace Attest
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country codes, with an explicit value on every member.
    /// </summary>
    /// <remarks>
    /// The values are stated rather than implied because the members are in alphabetical order by
    /// code, and this library grows by adding countries. An implied value means inserting one
    /// mid-list renumbers every member after it, which silently changes what a stored integer means
    /// for any consumer that persisted one. A new country takes the next free number, wherever its
    /// code sorts.
    /// </remarks>
    public enum Country
    {
        /// <summary>
        /// Unknown
        /// </summary>
        XX = 0,

        /// <summary>
        /// Andorra
        /// </summary>
        AD = 1,
        /// <summary>
        /// United Arab Erimates
        /// </summary>
        AE = 2,
        /// <summary>
        /// Albania
        /// </summary>
        AL = 3,
        /// <summary>
        /// Armenia
        /// </summary>
        AM = 4,
        /// <summary>
        /// Argentina
        /// </summary>
        AR = 5,
        /// <summary>
        /// Austria
        /// </summary>
        AT = 6,
        /// <summary>
        /// Australia
        /// </summary>
        AU = 7,
        /// <summary>
        /// Azerbaijan
        /// </summary>
        AZ = 8,
        /// <summary>
        /// Bosnia and Herzegovina
        /// </summary>
        BA = 9,
        /// <summary>
        /// Belgium
        /// </summary>
        BE = 10,
        /// <summary>
        /// Bulgaria
        /// </summary>
        BG = 11,
        /// <summary>
        /// Bahrain
        /// </summary>
        BH = 12,
        /// <summary>
        /// Bolivia
        /// </summary>
        BO = 13,
        /// <summary>
        /// Brazil
        /// </summary>
        BR = 14,
        /// <summary>
        /// Belarus
        /// </summary>
        BY = 15,
        /// <summary>
        /// Canada
        /// </summary>
        CA = 16,
        /// <summary>
        /// Switzerland
        /// </summary>
        CH = 17,
        /// <summary>
        /// Chile
        /// </summary>
        CL = 18,
        /// <summary>
        /// China
        /// </summary>
        CN = 19,
        /// <summary>
        /// Columbia
        /// </summary>
        CO = 20,
        /// <summary>
        /// Costa Rica
        /// </summary>
        CR = 21,
        /// <summary>
        /// Cuba
        /// </summary>
        CU = 22,
        /// <summary>
        /// Cyprus
        /// </summary>
        CY = 23,
        /// <summary>
        /// Czech Republic
        /// </summary>
        CZ = 24,
        /// <summary>
        /// Germany
        /// </summary>
        DE = 25,
        /// <summary>
        /// Denmark
        /// </summary>
        DK = 26,
        /// <summary>
        /// Dominica Republic
        /// </summary>
        DO = 27,
        /// <summary>
        /// Ecuador
        /// </summary>
        EC = 28,
        /// <summary>
        /// Estonia
        /// </summary>
        EE = 29,
        /// <summary>
        /// Spain
        /// </summary>
        ES = 30,
        /// <summary>
        /// Finland
        /// </summary>
        FI = 31,
        /// <summary>
        /// Faroe Islands
        /// </summary>
        FO = 32,
        /// <summary>
        /// France
        /// </summary>
        FR = 33,
        /// <summary>
        /// Great Britain
        /// </summary>
        GB = 34,
        /// <summary>
        /// Georgia
        /// </summary>
        GE = 35,
        /// <summary>
        /// Greece
        /// </summary>
        GR = 36,
        /// <summary>
        /// Guatemala
        /// </summary>
        GT = 37,
        /// <summary>
        /// Hong Kong
        /// </summary>
        HK = 38,
        /// <summary>
        /// Croatia
        /// </summary>
        HR = 39,
        /// <summary>
        /// Hungary
        /// </summary>
        HU = 40,
        /// <summary>
        /// Indonesia
        /// </summary>
        ID = 41,
        /// <summary>
        /// Ireland
        /// </summary>
        IE = 42,
        /// <summary>
        /// Israel
        /// </summary>
        IL = 43,
        /// <summary>
        /// India
        /// </summary>
        IN = 44,
        /// <summary>
        /// Iceland
        /// </summary>
        IS = 45,
        /// <summary>
        /// Italy
        /// </summary>
        IT = 46,
        /// <summary>
        /// Japan
        /// </summary>
        JP = 47,
        /// <summary>
        /// Korea
        /// </summary>
        KR = 48,
        /// <summary>
        /// Kazahstan
        /// </summary>
        KZ = 49,
        /// <summary>
        /// Lithuania
        /// </summary>
        LT = 50,
        /// <summary>
        /// Luxembourg
        /// </summary>
        LU = 51,
        /// <summary>
        /// Latvia
        /// </summary>
        LV = 52,
        /// <summary>
        /// Monaco
        /// </summary>
        MC = 53,
        /// <summary>
        /// Moldova
        /// </summary>
        MD = 54,
        /// <summary>
        /// Montenegro
        /// </summary>
        ME = 55,
        /// <summary>
        /// North Macedonia
        /// </summary>
        MK = 56,
        /// <summary>
        /// Malta
        /// </summary>
        MT = 57,
        /// <summary>
        /// Mauritius
        /// </summary>
        MU = 58,
        /// <summary>
        /// summary
        /// </summary>
        MX = 59,
        /// <summary>
        /// Malaysia
        /// </summary>
        MY = 60,
        /// <summary>
        /// Nigeria
        /// </summary>
        NG = 61,
        /// <summary>
        /// Netherlands
        /// </summary>
        NL = 62,
        /// <summary>
        /// Norway
        /// </summary>
        NO = 63,
        /// <summary>
        /// New Zealand
        /// </summary>
        NZ = 64,
        /// <summary>
        /// Peru
        /// </summary>
        PE = 65,
        /// <summary>
        /// Philippines
        /// </summary>
        PH = 66,
        /// <summary>
        /// Pakistan
        /// </summary>
        PK = 67,
        /// <summary>
        /// Poland
        /// </summary>
        PL = 68,
        /// <summary>
        /// Portugal
        /// </summary>
        PT = 69,
        /// <summary>
        /// Paraguay
        /// </summary>
        PY = 70,
        /// <summary>
        /// Romania
        /// </summary>
        RO = 71,
        /// <summary>
        /// Serbia
        /// </summary>
        RS = 72,
        /// <summary>
        /// Russia
        /// </summary>
        RU = 73,
        /// <summary>
        /// Sweden
        /// </summary>
        SE = 74,
        /// <summary>
        /// Slovenia
        /// </summary>
        SI = 75,
        /// <summary>
        /// Slovakia
        /// </summary>
        SK = 76,
        /// <summary>
        /// San Marino
        /// </summary>
        SM = 77,
        /// <summary>
        /// El Salvador
        /// </summary>
        SV = 78,
        /// <summary>
        /// Thailand
        /// </summary>
        TH = 79,
        /// <summary>
        /// Turkey
        /// </summary>
        TR = 80,
        /// <summary>
        /// Taiwan
        /// </summary>
        TW = 81,
        /// <summary>
        /// Ukraine
        /// </summary>
        UA = 82,
        /// <summary>
        /// United States
        /// </summary>
        US = 83,
        /// <summary>
        /// Uruguay
        /// </summary>
        UY = 84,
        /// <summary>
        /// Uzbekistan
        /// </summary>
        UZ = 85,
        /// <summary>
        /// Venezuela
        /// </summary>
        VE = 86,
        /// <summary>
        /// South Africa
        /// </summary>
        ZA = 87,
    }
}
