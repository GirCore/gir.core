using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GObject.Tests
{
    [TestClass, TestCategory("IntegrationTest")]
    public class ValueTest
    {
        [DataTestMethod]
        [DataRow(5)]
        [DataRow(true)]
        [DataRow("TestString")]
        public void ValueFromDataShouldContainGivenData(object data)
        {
            var v = Value.From(data);
            v.Extract().Should().Be(data);
        }
        
        [TestMethod]
        public void ValueReturnsExpectedType()
        {
            // Check that we have a value
            var v = Value.From("Hello");
            Native.Functions.TypeCheckValue(v.Handle).Should().Be(true);
            
            // Check we can marshal as a struct
            Native.Value.Struct str = Marshal.PtrToStructure<Native.Value.Struct>(v.Handle.DangerousGetHandle());
            Native.Functions.TypeCheckValueHolds(v.Handle, str.GType);
            Native.Functions.TypeCheckValueHolds(v.Handle, (nuint)Native.BasicType.String);
            str.GType.Should().Be((nuint) Native.BasicType.String);
        }
    }
}
