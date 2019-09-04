namespace NEMS_API.Core.Resources
{
    public class FhirConstants
    {
        public const string SystemNemsBundleProfile = "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-Bundle-1";

        public const string SystemNemsSubscriptionProfile = "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-Subscription-1";

        public const string SystemNemsMessageHeaderProfile = "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-MessageHeader-1";

        public const string SDSpineOpOutcome = "https://fhir.nhs.uk/STU3/StructureDefinition/Spine-OperationOutcome-1";

        public const string SDSpineOpOutcome1 = "https://fhir.nhs.uk/StructureDefinition/spine-operationoutcome-1-0";

        public const string SystemOpOutcome = "https://fhir.nhs.uk/STU3/CodeSystem/Spine-ErrorOrWarningCode-1";

        public const string SystemOpOutcome1 = "https://fhir.nhs.uk/STU3/ValueSet/spine-response-code-1-0";

        public const string SystemASID = "https://fhir.nhs.uk/Id/accredited-system";

        public const string SystemSdsRole = "https://fhir.nhs.uk/Id/sds-role-profile-id";

        public const string SystemOrgCode = "https://fhir.nhs.uk/Id/ods-organization-code";

        public const string SystemNhsNumber = "https://fhir.nhs.uk/Id/nhs-number";

        //HTTP Headers
        public const string HeaderFromAsid = "fromASID";

        public const string HeaderToAsid = "toASID";

        public const string HeaderInteractionID = "InteractionID";

        //JWT
        public const string JwtClientSysIssuer = "iss";

        public const string JwtIndOrSysIdentifier = "sub";

        public const string JwtEndpointUrl = "aud";

        public const string JwtExpiration = "exp";

        public const string JwtIssued = "iat";

        public const string JwtReasonForRequest = "reason_for_request";

        public const string JwtScope = "scope";

        public const string JwtRequestingSystem = "requesting_system";

        public const string JwtRequestingOrganization = "requesting_organization";

        public const string JwtRequestingUser = "requesting_user";

        public const string JwtRequestingPatient = "requesting_patient";


        //InteractionIds

        public const string IISubscriptionCreate = "urn:nhs:names:services:clinicals-sync:SubscriptionsApiPost";

        public const string IISubscriptionDelete = "urn:nhs:names:services:clinicals-sync:SubscriptionsApiDelete";

        public const string IISubscriptionRead = "urn:nhs:names:services:clinicals-sync:SubscriptionsApiGet";

        public string IIPublishEvent(string code) => $"urn:nhs:names:services:events:{code}.Write";
    }
}
