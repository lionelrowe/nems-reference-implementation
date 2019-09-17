using Hl7.Fhir.Model;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Models.Core;
using NEMS_APITestHelper.Data;
using NEMS_APITestHelper.StubClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace NEMS_APITest.Core.Helpers
{
    public class StaticCacheHelperTests : IDisposable
    {
        private IMemoryCache _sdsCacheMock;
        //private IFileHelper _fileHelper;

        public StaticCacheHelperTests()
        {
            var sdsCache = new List<SdsViewModel>
            {
                SdsViewModels.SdsAsid000
            };

            var patientsCache = new Bundle
            {
                Type = Bundle.BundleType.Collection,
                Entry = new List<Bundle.EntryComponent>
                {
                    new Bundle.EntryComponent
                    {
                        Resource = Patients.Patient_2686033207
                    }
                }
            };

            var cache = new Dictionary<string, Tuple<object, bool>>
            {
                { "SdsViewModel", new Tuple<object, bool>(sdsCache, true) },
                { "SdsViewModelers", new Tuple<object, bool>(sdsCache, false) },
                { "PatientsList", new Tuple<object, bool>(patientsCache, true) },
                { "PatientsListed", new Tuple<object, bool>(null, true) },
                { "PatientsListeder", new Tuple<object, bool>(null, false) }
            };

            _sdsCacheMock = MemoryCacheStub.GetMemoryCache(cache);

            //var fileHelperMock = new Mock<IFileHelper>();
            //fileHelperMock.Setup(op => op.GetStringFromFile(It.IsAny<string>())).Throws(new FileNotFoundException());
            //fileHelperMock.Setup(op => op.GetStringFromFile(It.Is<string>(x => x == "Data/sds.json"))).Returns("[{\"odsCode\":\"2XR\"}]");
            //fileHelperMock.Setup(op => op.GetStringFromFile(It.Is<string>(x => x == "Data/patients.json"))).Returns("{\"resourceType\":\"Bundle\", \"entry\":[]}");

            //_fileHelper = fileHelperMock.Object;
        }

        public void Dispose()
        {
            _sdsCacheMock = null;
            //_fileHelper = null;
        }

        [Fact]
        public void GetCacheData_Valid()
        {

            var sut = new StaticCacheHelper(_sdsCacheMock);

            var expected = sut.GetEntry<List<SdsViewModel>>("SdsViewModel");

            Assert.IsType<List<SdsViewModel>>(expected);

            Assert.True(expected.Count == 1);
        }

        [Fact]
        public void GetCacheData_ValidFhir()
        {

            var sut = new StaticCacheHelper(_sdsCacheMock);

            var expected = sut.GetEntry<Bundle>("PatientsList");

            Assert.NotNull(expected);

            Assert.IsType<Bundle>(expected);

            Assert.NotNull(expected.Entry);

            Assert.NotEmpty(expected.Entry);

            Assert.True(expected.Entry.Count == 1);

            Assert.Equal(ResourceType.Patient, expected.Entry.First().Resource.ResourceType);
        }

        [Fact]
        public void GetCacheData_Valid_NewKey()
        {

            var sut = new StaticCacheHelper(_sdsCacheMock);

            var expected = sut.GetEntry<List<SdsViewModel>>("SdsViewModels");

            Assert.IsType<List<SdsViewModel>>(expected);

            Assert.True(expected.Count == 1);
        }

        [Fact]
        public void GetCacheData_Valid_NewKeySet()
        {

            var sut = new StaticCacheHelper(_sdsCacheMock);

            var expected = sut.GetEntry<List<SdsViewModel>>("SdsViewModelers");

            Assert.IsType<List<SdsViewModel>>(expected);

            Assert.True(expected.Count == 1);
        }

        [Fact]
        public void GetCacheData_Valid_EmptyCache()
        {

            var sut = new StaticCacheHelper(_sdsCacheMock);

            var expected = sut.GetEntry<Bundle>("PatientsListed");

            Assert.Null(expected);
        }

        [Fact]
        public void GetCacheData_Invalid_ErrorDataStore()
        {

            var sut = new StaticCacheHelper(_sdsCacheMock);


            Assert.ThrowsAny<Exception>(() => {
                var expected = sut.GetEntry<Bundle>("PatientsListeder");
            });
        }
    }
}
