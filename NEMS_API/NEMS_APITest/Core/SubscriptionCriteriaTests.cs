using NEMS_API.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NEMS_APITest.Models.Core
{
    public class SubscriptionCriteriaTests : IDisposable
    {
        IDictionary<string, List<(string, string)>> _criterias;

        public SubscriptionCriteriaTests()
        {
            _criterias = new Dictionary<string, List<(string, string)>>
            {
                { "valid1", new List<(string, string)> { ("prefix", "/Bundle?type=message"), ("type", "message"), ("servicetype", "GP") } },
                { "valid2", new List<(string, string)> { ("prefix", "/Patient?age=10"), ("age", "10"), ("gender", "male") } },
                { "valid3", new List<(string, string)> { ("prefix", "/Organization?identifier=system%7C1XR"), ("identifier", "system|1XR"), ("active", "true") } },
                { "valid4", new List<(string, string)> { ("prefix", "/Patient?age=10"), ("age", "10") } },
                { "valid5", new List<(string, string)> { ("prefix", "/?id=123"), ("id", "123") } }
            };
        }

        public void Dispose()
        {
            _criterias = null;
        }

        [Theory]
        [InlineData("/Bundle?type=message&serviceType=GP", "valid1")]
        [InlineData("/Patient?age=10&gender=male", "valid2")]
        [InlineData("/Organization?identifier=system|1XR&active=true", "valid3")]
        [InlineData("/Patient?age=10&", "valid4")]
        [InlineData("/?id=123", "valid5")]
        public void SubscriptionCriteria_Valid(string criteria, string criteriaExpKey)
        {
            var sc = new SubscriptionCriteria(criteria);

            Assert.NotNull(sc.Parameters);

            Assert.NotEmpty(sc.Parameters);

            var validCriteria = _criterias[criteriaExpKey];

            foreach((string key, string value) vc in validCriteria)
            {
                var pm = sc.Parameters.FirstOrDefault(x => x.Key == vc.key);

                Assert.NotNull(pm.Key);

                Assert.Equal(vc.key, pm.Key);
                Assert.Equal(vc.value, pm.Value);
            }

        }

        [Theory]
        [InlineData("Bundle?type=message&serviceType=GP")]
        [InlineData("/Organization?")]
        [InlineData("/Immuization")]
        [InlineData("?active=true")]
        [InlineData("&blue=false&add=10")]
        public void SubscriptionCriteria_Invalid(string criteria)
        {
            var sc = new SubscriptionCriteria(criteria);

            Assert.NotNull(sc.Parameters);

            Assert.Empty(sc.Parameters);

        }

    }
}
