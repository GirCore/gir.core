﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtk.Tests
{
    [TestClass, TestCategory("SystemTest")]
    public class ConstructorTest
    {
        [TestMethod]
        public void WindowConstructorShouldSetTitle()
        {
            var title = "MyTitle";
            var window = new Window(title);

            window.Title.Should().Be(title);
        }
    }
}
