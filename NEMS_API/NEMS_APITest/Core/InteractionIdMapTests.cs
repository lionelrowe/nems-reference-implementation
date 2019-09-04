using NEMS_API.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NEMS_APITest.Models.Core
{
    public class InteractionIdMapTests
    {
        [Fact]
        public void ClinicalScopes_Valid()
        {
            var mapModel = new InteractionIdMap
            {
                ResourceType = "MessageHeader",
                Scopes = new List<string> { "read", "write" }
            };

            var clinicalScopes = mapModel.ClinicalScopes("patient");

            Assert.NotNull(clinicalScopes);

            Assert.NotEmpty(clinicalScopes);

            Assert.Collection<string>(clinicalScopes, scope => Assert.Equal("patient/MessageHeader.read", scope),
                                                      scope => Assert.Equal("patient/MessageHeader.write", scope));
        }


        [Theory]
        [InlineData("", new string[] { "read" }, "patient")]
        [InlineData(null, new string[] { "read" }, "patient")]
        [InlineData("MessageHeader", new string[0], "patient")]
        [InlineData("MessageHeader", null, "patient")]
        [InlineData("MessageHeader", new string[] { "read" }, " ")]
        [InlineData("MessageHeader", new string[] { "read" }, null)]
        public void ClinicalScopes_Empty(string resourceType, string[] scopes, string context)
        {
            var mapModel = new InteractionIdMap
            {
                ResourceType = resourceType,
                Scopes = (scopes ?? new string[0]).ToList()
            };

            var clinicalScopes = mapModel.ClinicalScopes(context);

            Assert.NotNull(clinicalScopes);

            Assert.Empty(clinicalScopes);
        }
    }
}
