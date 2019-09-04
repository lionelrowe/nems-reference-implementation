using Hl7.Fhir.Model;
using NEMS_API.Core.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace NEMS_APITest.Core.Helpers
{
    public class FhirHelperTests : IDisposable
    {
        IDictionary<string, Bundle> _bundles;

        public FhirHelperTests()
        {
            var organization = new Organization();
            var organizationMeta = new Meta();
            organizationMeta.Profile = new List<string> { "organization-profile" };
            organization.Meta = organizationMeta;
            var organizationComp = new Bundle.EntryComponent();
            organizationComp.Resource = organization;

            var messageHeader = new MessageHeader();
            var messageHeaderMeta = new Meta();
            messageHeaderMeta.Profile = new List<string> { "messageHeader-profile" };
            messageHeader.Meta = messageHeaderMeta;
            var messageHeaderComp = new Bundle.EntryComponent();
            messageHeaderComp.Resource = messageHeader;

            var bundleComplete = new Bundle();
            bundleComplete.Entry = new List<Bundle.EntryComponent> { messageHeaderComp, organizationComp };


            var messageHeaderEmptyComp = new Bundle.EntryComponent();
            messageHeaderEmptyComp.Resource = new MessageHeader();

            var bundleEmptyResource = new Bundle();
            bundleEmptyResource.Entry = new List<Bundle.EntryComponent> { messageHeaderEmptyComp };

            var bundleEmptyEntry = new Bundle { Entry = new List<Bundle.EntryComponent>() };

            var bundleEmpty = new Bundle();

            _bundles = new Dictionary<string, Bundle>
            {
                { "bundleComplete", bundleComplete },
                { "bundleEmptyResource", bundleEmptyResource },
                { "bundleEmptyEntry", bundleEmptyEntry },
                { "bundleEmpty", bundleEmpty }
            };
        }

        public void Dispose()
        {
            _bundles = null;
        }

        [Fact]
        public void GetFirstEntryOfTypeProfile_Valid_MessageHeaderReturned()
        {
            var helper = FhirHelper.GetFirstEntryOfTypeProfile<MessageHeader>(_bundles["bundleComplete"], "messageHeader-profile");

            Assert.IsType<MessageHeader>(helper);

            Assert.NotNull(helper.Meta);

            Assert.Contains("messageHeader-profile", helper.Meta.Profile);
        }

        [Theory]
        [InlineData(1, "bundleComplete", "immunization-profile")]
        [InlineData(2, "bundleComplete", null)]
        [InlineData(3, "bundleEmptyResource", "organization-profile")]
        [InlineData(4, "bundleEmptyEntry", "organization-profile")]
        [InlineData(5, "bundleEmpty", "organization-profile")]
        [InlineData(6, null, "organization-profile")]
        public void GetFirstEntryOfTypeProfile_Invalid_NullReturned(int id, string bundleKey, string profile)
        {
            var bundle = bundleKey == null ? null :_bundles[bundleKey];

            var helper = FhirHelper.GetFirstEntryOfTypeProfile<Resource>(bundle, profile);

            Assert.Null(helper);

        }

        [Fact]
        public void GetFirstEntryOfTypeProfile_IncorrectResource_NullReturned()
        {
            var helper = FhirHelper.GetFirstEntryOfTypeProfile<Organization>(_bundles["bundleComplete"], "organization-profile");

            Assert.Null(helper);
        }

        [Fact]
        public void GetFirstEntryOfTypeProfile_InvalidType_NullReturned()
        {
            var helper = FhirHelper.GetFirstEntryOfTypeProfile<Immunization>(_bundles["bundleComplete"], "messageHeader-profile");

            Assert.Null(helper);

        }

    }
}
