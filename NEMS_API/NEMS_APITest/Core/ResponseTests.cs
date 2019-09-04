using NEMS_API.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NEMS_APITest.Models.Core
{
    public class ResponseTests
    {
        [Fact]
        public void ResponseNoOverload_ValidIsFalse()
        {
            var res = new Response();

            Assert.False(res.Success);
        }

        [Fact]
        public void ResponseBoolOverload_ValidIsTrue()
        {
            var res = new Response(true);

            Assert.True(res.Success);
        }

        [Fact]
        public void ResponseStringOverload_ValidIsFalseAndMessage()
        {
            var res = new Response("success!");

            Assert.Equal("success!", res.Message);
            Assert.False(res.Success);
        }

        [Fact]
        public void ResponseBoolStringOverload_ValidIsTrueAndMessage()
        {
            var res = new Response(true, "is success!");

            Assert.Equal("is success!", res.Message);
            Assert.True(res.Success);
        }

        [Fact]
        public void ResponseSetError_ValidIsTError()
        {
            var res = new Response(true, "is success!");

            res.SetError("it failed.");

            Assert.Equal("it failed.", res.Message);
            Assert.False(res.Success);
        }

    }
}
