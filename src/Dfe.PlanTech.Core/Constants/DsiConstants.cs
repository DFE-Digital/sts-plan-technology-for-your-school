using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class DsiConstants
{
    // Category IDs, supplied from DSI 17-Oct-2025
    public const string EstablishmentCategoryId = "001";
    public const string LocalAuthorityCategoryId = "002";
    public const string OtherLegacyOrganisationCategoryId = "003";
    public const string EarlyYearsSettingCategoryId = "004";
    public const string OtherStakeholderCategoryId = "008";
    public const string TrainingProviderCategoryId = "009";
    public const string MatOrganisationCategoryId = "010";
    public const string GovernmentCategoryId = "011";
    public const string OtherGiasStakeholderCategoryId = "012";
    public const string SatOrganisationCategoryId = "013";
    public const string SSatOrganisationCategoryId = "014";
    public const string SoftwareSupplierCategoryId = "050";
    public const string PimsTrainingProviderCategoryId = "051";
    public const string BillingAuthorityCategoryId = "052";
    public const string YouthCustodyServiceCategoryId = "053";

    /// <summary>
    /// Organisation "groups" are those who have schools within them,
    /// and we should show the "select a school" page after login etc.
    /// </summary>
    public static HashSet<string> OrganisationGroupCategories { get; } =
        new()
        {
            MatOrganisationCategoryId,
            //SatOrganisationCategoryId,
            //SSatOrganisationCategoryId,
        };

    // Establishment Type IDs, supplied from DSI 17-Oct-2025
    public const string CommunitySchoolEstablishmentTypeId = "01";
    public const string VoluntaryAidedSchoolEstablishmentTypeId = "02";
    public const string VoluntaryControlledSchoolEstablishmentTypeId = "03";
    public const string FoundationSchoolEstablishmentTypeId = "05";
    public const string CityTechnologyCollegeEstablishmentTypeId = "06";
    public const string CommunitySpecialSchoolEstablishmentTypeId = "07";
    public const string NonMaintainedSpecialSchoolEstablishmentTypeId = "08";
    public const string OtherIndependentSpecialSchoolEstablishmentTypeId = "10";
    public const string OtherIndependentSchoolEstablishmentTypeId = "11";
    public const string FoundationSpecialSchoolEstablishmentTypeId = "12";
    public const string PupilReferralUnitEstablishmentTypeId = "14";
    public const string LANurserySchoolEstablishmentTypeId = "15";
    public const string FurtherEducationEstablishmentTypeId = "18";
    public const string SecureUnitsEstablishmentTypeId = "24";
    public const string OffshoreSchoolsEstablishmentTypeId = "25";
    public const string ServiceChildrensEducationEstablishmentTypeId = "26";
    public const string MiscellaneousEstablishmentTypeId = "27";
    public const string AcademySponsorLedEstablishmentTypeId = "28";
    public const string HigherEducationInstitutionEstablishmentTypeId = "29";
    public const string WelshEstablishmentTypeId = "30";
    public const string SixthFormCentresEstablishmentTypeId = "31";
    public const string SpecialPost16InstitutionEstablishmentTypeId = "32";
    public const string AcademySpecialSponsorLedEstablishmentTypeId = "33";
    public const string AcademyConverterEstablishmentTypeId = "34";
    public const string FreeSchoolsEstablishmentTypeId = "35";
    public const string FreeSchoolsSpecialEstablishmentTypeId = "36";
    public const string BritishOverseasSchoolsEstablishmentTypeId = "37";
    public const string FreeSchoolsAlternativeProvisionEstablishmentTypeId = "38";
    public const string FreeSchools1619EstablishmentTypeId = "39";
    public const string UniversityTechnicalCollegeEstablishmentTypeId = "40";
    public const string StudioSchoolsEstablishmentTypeId = "41";
    public const string AcademyAlternativeProvisionConverterEstablishmentTypeId = "42";
    public const string AcademyAlternativeProvisionSponsorLedEstablishmentTypeId = "43";
    public const string AcademySpecialConverterEstablishmentTypeId = "44";
    public const string Academy1619ConverterEstablishmentTypeId = "45";
    public const string Academy1619SponsorLedEstablishmentTypeId = "46";
    public const string ChildrensCentreEstablishmentTypeId = "47";
    public const string ChildrensCentreLinkedSiteEstablishmentTypeId = "48";
    public const string InstitutionFundedByOtherGovernmentDepartmentEstablishmentTypeId = "56";
    public const string AcademySecure1619EstablishmentTypeId = "57";
}
