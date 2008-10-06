﻿using System;
using System.Collections.Generic;
using System.Linq;

using IoC.Framework.Abstraction;
using IoC.Framework.Tests.Adapters;
using IoC.Framework.Tests.Classes;
using MbUnit.Framework;

namespace IoC.Framework.Tests {
    // ashmind: CommonServiceLocator expects you to be able to resolve services by-key.
    // However I do not really like this practice and faciities like Contextual binding in Ninject
    // seem much more interesting. 
    // I will probably find a way to do keyed resolution for all frameworks, however for now it is 
    // in MayHave.
    public class MayHaveTest : FrameworkTestBase {
        [Test]
        public void SupportsKeysForTransients(IFrameworkAdapter framework) {
            AssertSupportsKeysFor(framework, framework.AddTransient);
        }

        [Test]
        public void SupportsKeysForSingletons(IFrameworkAdapter framework) {
            AssertSupportsKeysFor(framework, framework.AddSingleton);
        }

        [Test]
        public void SupportsKeysForInstances(IFrameworkAdapter framework) {
            var first = new IndependentTestComponent();
            var second = new IndependentTestComponent();

            framework.AddInstance<ITestService>(first, "FirstKey");
            framework.AddInstance<ITestService>(second, "SecondKey");

            var resolved = framework.GetLocator().GetInstance<ITestService>("FirstKey");

            Assert.AreSame(first, resolved);
        }

        public void AssertSupportsKeysFor(IFrameworkAdapter framework, Action<Type, Type, string> add) {
            add(typeof(ITestService), typeof(IndependentTestComponent), "FirstKey");
            add(typeof(ITestService), typeof(AnotherIndependentTestComponent), "SecondKey");

            var firstComponent = framework.GetLocator().GetInstance<ITestService>("FirstKey");
            var secondComponent = framework.GetLocator().GetInstance<ITestService>("SecondKey");

            Assert.IsNotNull(firstComponent);
            Assert.IsNotNull(secondComponent);

            Assert.IsInstanceOfType(typeof(IndependentTestComponent), firstComponent);
            Assert.IsInstanceOfType(typeof(AnotherIndependentTestComponent), secondComponent);
        }
    }
}