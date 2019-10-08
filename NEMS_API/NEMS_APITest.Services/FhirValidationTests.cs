using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using Moq;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Models.FhirResources;
using NEMS_API.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace NEMS_APITest.Services
{
    public class FhirValidationTests : IDisposable
    {
        IValidationHelper _validationHelper;
        IOptions<NemsApiSettings> _nemsApiOptions;
        NemsSubscription _validSubscription;
        ISdsService _sdsService;

        public FhirValidationTests()
        {

            var nemsApiSettings = new NemsApiSettings
            {
                ValidationOptions = new ValidationOptions
                {
                    SkipSubscriptionCriteria = false
                },
                SubscriptionCriteriaRules = new List<SubscriptionCriteriaRule>
                {
                    new SubscriptionCriteriaRule
                    {
                        Parameter = "prefix",
                        Min = 1,
                        Max = 1,
                        ValueType = "single",
                        Values = "/Bundle?type=message",
                        IsPrefix = true,
                        Ignore = false
                      },
                        new SubscriptionCriteriaRule
                      {
                        Parameter = "type",
                        Min = 1,
                        Max = 1,
                        ValueType = "single",
                        Values = "message",
                        IsPrefix = false,
                        Ignore = true
                      },new SubscriptionCriteriaRule
                      {
                        Parameter = "servicetype",
                        Min = 0,
                        Max = 1,
                        ValueType = "regExp",
                        Values = "^(GP|CHO|UHV|EPCHR)$",
                        IsPrefix = false,
                        Ignore = false
                      },
                    new SubscriptionCriteriaRule
                      {
                        Parameter = "patient.identifier",
                        Min = 1,
                        Max = 1,
                        ValueType = "regExp",
                        Values = "^http://fhir.nhs.net/Id/nhs-number\\|(\\d){10}$",
                        IsPrefix = false,
                        Ignore = false
                      },
                    new SubscriptionCriteriaRule
                      {
                        Parameter = "messageheader.event",
                        Min = 1,
                        Max = 0,
                        ValueType = "codeSystem",
                        Values = "https://fhir.nhs.uk/STU3/CodeSystem/EventType-1",
                        IsPrefix = false,
                        Ignore = false
                      },
                    new SubscriptionCriteriaRule
                      {
                        Parameter = "patient.age",
                        Min = 0,
                        Max = 2,
                        ValueType = "searchParameter",
                        Values = "https://fhir.nhs.uk/STU3/SearchParameter/EMS-PatientAge-1",
                        IsPrefix = false,
                        Ignore = false
                      }
                }
            };

            var settingsMock = new Mock<IOptions<NemsApiSettings>>();
            settingsMock.Setup(op => op.Value).Returns(nemsApiSettings);
            _nemsApiOptions = settingsMock.Object;

            var nemsPatientSearchParam = new SearchParameter
            {
                Id = "EMS-PatientAge-1",
                Url = "https://fhir.nhs.uk/STU3/SearchParameter/EMS-PatientAge-1",
                Version = "1.0.0",
                Name = "EMS Patient Age",
                Status = PublicationStatus.Draft,
                Date = "2018-10-10",
                Publisher = "NHS Digital",
                Contact = new List<ContactDetail> { new ContactDetail { Name = "Interoperability Team", Telecom = new List<ContactPoint> { new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "interoperabilityteam@nhs.net", Use = ContactPoint.ContactPointUse.Work } } } },
                Purpose = new Markdown("This search parameter has been defined to enable the ability to form a search criteria based on the Patient age at the time of the event."),
                Code = "Patient.age",
                Base = new List<ResourceType?> { ResourceType.Bundle },
                Type = SearchParamType.Number,
                Description = new Markdown("A search parameter to form the search criteria expression for a Patient age range."),
                Expression = "Bundle('https://fhir.nhs.uk/STU3/StructureDefinition/EMS-Bundle-1')",
                Xpath = "f:Bundle/[@url='https://fhir.nhs.uk/STU3/StructureDefinition/EMS-Bundle-1']",
                XpathUsage = SearchParameter.XPathUsageType.Normal,
                Comparator = new List<SearchParameter.SearchComparator?> { SearchParameter.SearchComparator.Lt, SearchParameter.SearchComparator.Gt },
                Modifier = new List<SearchParameter.SearchModifierCode?> { SearchParameter.SearchModifierCode.Contains }
            };

            var eventTypeCodeSystem = new CodeSystem
            {
                Concept = new List<CodeSystem.ConceptDefinitionComponent>
                {
                    new CodeSystem.ConceptDefinitionComponent { Display = "​Vaccinations" , Code = "vaccinations-1" },
                    new CodeSystem.ConceptDefinitionComponent { Display = "​PDS Change of Address" , Code = "pds-change-of-address-1" }
                }
            };


            var validationHelper = new Mock<IValidationHelper>();
            validationHelper.Setup(m => m.GetSearchParameter(It.IsAny<string>())).Returns(nemsPatientSearchParam);
            validationHelper.Setup(m => m.GetCodeSystem(It.IsAny<string>())).Returns(eventTypeCodeSystem);
            _validationHelper = validationHelper.Object;

            var sub = new NemsSubscription();
            sub.Meta = new Meta();
            sub.Meta.Profile = new string[] { "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-Subscription-1" };
            sub.Status = Subscription.SubscriptionStatus.Requested;
            sub.Contact = new List<ContactPoint>();
            sub.Contact.Add(new ContactPoint(ContactPoint.ContactPointSystem.Url,
                                             ContactPoint.ContactPointUse.Work,
                                             "https://directory.spineservices.nhs.uk/STU3/Organization/RR8"));
            sub.Reason = "Health visiting service responsible for Leeds";
            sub.Criteria = "/Bundle?type=message&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1";
            sub.Channel = new Subscription.ChannelComponent
            {
                Type = Subscription.SubscriptionChannelType.Message,
                Endpoint = "Mailbox1234"
            };

            _validSubscription = sub;

            var sdsMock = new Mock<ISdsService>();
            sdsMock.Setup(op => op.GetFor(It.IsAny<string>())).Returns((SdsViewModel)null);

            _sdsService = sdsMock.Object;

        }

        public void Dispose()
        {
            _validationHelper = null;
            _nemsApiOptions = null;
            _validSubscription = null;
            _sdsService = null;
        }

        [Theory]
        [InlineData("/Bundle?type=message&serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14")]
        [InlineData("/Bundle?type=message&serviceType=CHO&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765922&MessageHeader.event=pds-change-of-address-1")]
        [InlineData("/Bundle?type=message&serviceType=UHV&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14&Patient.age=gt4")]
        [InlineData("/Bundle?type=message&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1")]
        [InlineData("/Bundle?type=message&serviceType=EPCHR&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&MessageHeader.event=vaccinations-1&Patient.age=200")]
        public void ValidateSubscriptionCriteria_Valid(string criteria)
        {
             var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);

            var result = ts.ValidateSubscriptionCriteria(criteria);

            Assert.IsType<OperationOutcome>(result);

            var operationOutcome = result as OperationOutcome;

            Assert.True(operationOutcome.Success);
        }

        [Theory]
        [InlineData(1, "Bundle?type=message&serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14")]
        [InlineData(2, "/Bundle?type=tree&serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14")]
        [InlineData(3, "/Bundle?serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14")]
        [InlineData(4, "/Bundle?type=message&serviceType=MHS&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14")]
        [InlineData(6, "/Bundle?type=message&Patient.identifier=http://fhir.nhs.net/Id/nhs-numbers|9434765919&MessageHeader.event=pds-change-of-address-1")]
        [InlineData(7, "/Bundle?type=message&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|94347&MessageHeader.event=pds-change-of-address-1")]
        [InlineData(8, "/Bundle?type=message&MessageHeader.event=pds-change-of-address-1")]
        [InlineData(9, "/Bundle?type=message&Patient.identifier=9434765919&MessageHeader.event=pds-change-of-address-1")]
        [InlineData(10, "/Bundle?type=message&serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&Patient.age=lt14")]
        [InlineData(11, "/Bundle?type=message&serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-134&Patient.age=lt14")]
        [InlineData(12, "/Bundle?type=message&serviceType=GP&Patient.identifier=http://fhir.nhs.net/Id/nhs-number|9434765919&MessageHeader.event=pds-change-of-address-1&Patient.age=lt14&Patient.age=gt5&Patient.age=7")]
        public void ValidateSubscriptionCriteria_Invalid(int id, string criteria)
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);

            var result = ts.ValidateSubscriptionCriteria(criteria);

            Assert.IsType<OperationOutcome>(result);

            var operationOutcome = result as OperationOutcome;

            Assert.False(operationOutcome.Success);
        }

        [Fact]
        public void ValidSubscription_Valid()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);

            var result = ts.ValidSubscription(_validSubscription);

            Assert.NotNull(result);

            Assert.IsType<OperationOutcome>(result);

            Assert.True(result.Success);
        }

        [Fact]
        public void ValidSubscription_ValidExtraContact()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);

            var sub = _validSubscription;
            sub.Contact.Add(new ContactPoint(ContactPoint.ContactPointSystem.Other,
                                 ContactPoint.ContactPointUse.Work,
                                 "https://directory.spineservices.nhs.uk/STU3/Organization/RR8"));

            var result = ts.ValidSubscription(sub);

            Assert.NotNull(result);

            Assert.IsType<OperationOutcome>(result);

            Assert.True(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidContactSystem()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Contact = new List<ContactPoint>();
            sub.Contact.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email,
                                             ContactPoint.ContactPointUse.Work,
                                             "https://directory.spineservices.nhs.uk/STU3/Organization/RR8"));

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidContactUse()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Contact = new List<ContactPoint>();
            sub.Contact.Add(new ContactPoint(ContactPoint.ContactPointSystem.Url,
                                             ContactPoint.ContactPointUse.Home,
                                             "https://directory.spineservices.nhs.uk/STU3/Organization/RR8"));

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidContactValue()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Contact = new List<ContactPoint>();
            sub.Contact.Add(new ContactPoint(ContactPoint.ContactPointSystem.Url,
                                             ContactPoint.ContactPointUse.Work,
                                             "https://directory.spineservices.nhs.uk/STU3/Patient/RR8"));

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidContactValueCode()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Contact = new List<ContactPoint>();
            sub.Contact.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email,
                                             ContactPoint.ContactPointUse.Work,
                                             "https://directory.spineservices.nhs.uk/STU3/Organization/"));

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidStatus()
        {
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Status = Subscription.SubscriptionStatus.Off;

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidCriteria()
        {
            //Not testing the criteria validation, but the catching of the error within it
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Criteria = "anything.com";

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidChannelType()
        {
            //Not testing the criteria validation, but the catching of the error within it
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Channel.Type = Subscription.SubscriptionChannelType.RestHook;

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidChannelEndpoint()
        {
            //Not testing the criteria validation, but the catching of the error within it
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Channel.Endpoint = null;

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidHasId()
        {
            //Not testing the criteria validation, but the catching of the error within it
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Id = "SHoud-not-be-here";

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidHasVersion()
        {
            //Not testing the criteria validation, but the catching of the error within it
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Meta.VersionId = "Shoud-not-be-here";

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidSubscription_InvalidHasLastUpdated()
        {
            //Not testing the criteria validation, but the catching of the error within it
            var ts = new FhirValidation(_nemsApiOptions, _validationHelper, _sdsService);
            var sub = _validSubscription;
            sub.Meta.LastUpdated = DateTimeOffset.Now;

            var result = ts.ValidSubscription(sub);
            Assert.NotNull(result);
            Assert.IsType<OperationOutcome>(result);
            Assert.False(result.Success);
        }
    }
}
