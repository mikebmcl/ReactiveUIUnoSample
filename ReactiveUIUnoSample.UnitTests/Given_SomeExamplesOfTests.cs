//#define ENABLE_FAILING_TESTS

using System;
using System.Collections.Generic;

using FluentAssertions;
using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIUnoSample;
using ReactiveUIUnoSample.ViewModels;
using ReactiveUIUnoSample.ViewModels.Testing;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class Given_SomeExamplesOfTests : ReactiveTestBase
    {
        [SetUp]
        public void SetUp()
        {
            // Perform any pre-test actions here. This runs before each test in this class. This is a where you should set up the
            // preconditions that are assumed to exist for this particular Given: https://en.wikipedia.org/wiki/Behavior-driven_development
            // An exception here will prevent the test from running and the [TearDown] method in this class will not run (tear down
            // methods in base classes will still run though).
        }

        [TearDown]
        public void TearDown()
        {
            // Perform any post-test actions here. For example if there are any class members that need to be disposed, this is a good
            // place to do that. This will run even if the test failed so that that into account. Also, an exception here will not stop
            // a base class [TearDown] method from being called and other tests in this class, if any, will be run.
        }

        [Test]
        public void SuccessfulTest()
        {

        }

        // Just as an upfront note, below we separate the classical NUnit framework testing from the newer Constraint-based NUnit testing.
        // This is just for learning purposes. They can be mixed together within a single test, including within a single Assert.Multiple(...).
        // For example, if you're comparing floating point numbers and want to allow for a slight difference between the expect and actual,
        // it's much simpler to write Assert.AreEqual(actual, expected, acceptableDifference); than to write
        // Assert.That(actual, Is.InRange(expected - acceptableDifference, expected + acceptableDifference));. However, if you're comparing
        // other types of values (especially if you want to use a custom IComparer<T>), then Assert.That(actual, Is.InRange(...)); or
        // Assert.That(actual, Is.InRange(...).Using(...)); is likely to be a better choice than trying to do write it using the classical style.

        // Also, do not assume that the methods will run in the order you put them in your test class or that the results will be in that order.
        // ReactiveTestBase.SetUpTest will run before each test and is meant to reset everything to the state the application would be in when it
        // first runs. You can and should add to it if there are additional things that you want the test execution engine to do before running
        // every test. If you just want all of the tests in a particular test class to start from some specific state, write a method in that
        // class that will put things into that desired state. As a matter of good practice it should be written as a test method so that if it
        // fails you'll know that it was the soure of failure. Once you've written that method, have your other test methods call it before they
        // do anything else. There's nothing that forbids a test method from calling another test method; the test execution engine won't treat
        // that method as being special or different when it's called in some other test method.

        // The point above about the methods not running in the order they appear in your class is especially important to remember when your
        // test methods have similar names, especially if they have similar code and error messages. You don't want to be trying to track down
        // why a certain test method failed when it actually succeeded and it was another, similar test method that failed.

        // Some test engines show the method name in their results while others show the Description from the TestAttribute so make sure to set
        // both, have them be description of the test, and be essentially identical so that there is no confusion if you run them using different
        // test engines. The When...Then... naming convention is good practice. Having method names and description strings this long should be
        // avoided because having a wall of text in the output isn't useful. Short names and descriptions make it easier to find the failed test
        // and you can add comments in the test itself if the code and failure messages don't make it clear what the test is checking, how it's
        // checking it, ahd why it's checking it (though the why should usually be obvious and normally doesn't need to be explicitly documented).
        [Test(Description = "When a lot of tests using NUnit.Framework-based testing that should be in separate test methods are run, then some will intentionally fail as examples of failure")]
        public void WhenLotsOfTestsUsingNUnitFrameworkAssertsInOneMethodThatShouldBeInSeparateTestMethodsRun_ThenSomeIntentionallyFail()
        {
            // If you want NUnit to keep going despite an Assert failing, you put the Asserts inside a method or lambda that you pass to Assert.Multiple like this.
            Assert.Multiple(() =>
            {
                double sut = 1.1;
                double allowableDifference = 0.2;
                double expected = 1.1;
                double approximateExpected = 1.0;
                Assert.AreEqual(sut, expected); // Assert checking for exact equality using classical testing (specific methods for each type of test).
                Assert.AreEqual(sut, approximateExpected, allowableDifference); // Assert checking for approximate equality using classical testing (specific methods for each type of test).
#if ENABLE_FAILING_TESTS
                Assert.AreEqual(sut, approximateExpected + allowableDifference * 10, allowableDifference); // Assert checking for approximate equality using classical testing (specific methods for each type of test). This is meant to fail.
#endif
                void ThrowArgumentException() => throw new ArgumentException();
                void DoNotThrow() { };
                Assert.Throws(typeof(ArgumentException), ThrowArgumentException); // Assert checking for an expected exception type using classical testing.
#if ENABLE_FAILING_TESTS
                Assert.Throws(typeof(InvalidOperationException), ThrowArgumentException); // Assert checking for an expected exception type using classical testing. This is meant to fail.
#endif
                Assert.DoesNotThrow(DoNotThrow); // Assert checking that no exception is thrown using classical testing.
#if ENABLE_FAILING_TESTS
                Assert.DoesNotThrow(ThrowArgumentException); // Assert checking that no exception is thrown using classical testing. This is meant to fail.
#endif
            });

#if ENABLE_THROWING_TESTS
            Assert.IsTrue(false, "I will never run because I'm outside of the Assert.Multiple and it had failures.");
#endif
        }

        [Test(Description = "When a lot of tests using NUnit Constraint-based testing that should be in separate test methods are run, then some will intentionally fail as examples of failure")]
        public void WhenLotsOfTestsUsingNUnitConstraintBasedTestingInOneMethodThatShouldBeInSeparateTestMethodsRun_ThenSomeIntentionallyFail()
        {
            // If you want NUnit to keep going despite an Assert failing, you put the Asserts inside a method or lambda that you pass to Assert.Multiple like this.
            Assert.Multiple(() =>
            {
                double sut = 1.1;
                double allowableDifference = 0.2;
                double expected = 1.1;
                Assert.That(sut, Is.EqualTo(expected)); // Assert checking for exact equality using constraint-based testing.

                Assert.That(sut, Is.InRange(expected - allowableDifference, expected + allowableDifference)); // Assert checking for approximate equality using constraint-based testing. If you care about the specific value along with the range then this is the right way to check it. Is.InRange will terminate your tests regardless of Assert.Multiple if the first argument (from) is greater than or equal to the second argument (to) using the default IComparer<T> for the type. You can supply an explicit comparer via the Using method, i.e. Is.InRange(...).Using(...). There is also a generic Using method.
                Assert.That(Math.Abs(sut - expected), Is.LessThanOrEqualTo(allowableDifference)); // Assert checking for approximate equality using constraint-base testing. Using Math.Abs to get the absolute difference between actual and expected then checking it against our allowable difference is somewhat simpler to write than the Is.InRange version above. However the only information we get from this is how far outside of our acceptable difference the actual value was. We don't get the value itself.

#if ENABLE_FAILING_TESTS
                Assert.That(sut, Is.InRange(sut - allowableDifference * 3, sut - allowableDifference * 2)); // Assert checking for approximate equality using constraint-based testing. We are using sut inside the Is.InRange rather than expected because we want this to fail and using the actual value to create a range that it cannot be in ensures that it will fail as intended. Also, Is.InRange will terminate your tests regardless of Assert.Multiple if the first argument (from) is greater than or equal to the second argument (to) using the default IComparer<T> for the type. You can supply an explicit comparer via the Using method, i.e. Is.InRange(...).Using(...). There is also a generic Using method.
                Assert.That(Math.Abs(sut - expected) * 2, Is.LessThanOrEqualTo(allowableDifference)); // Assert checking for approximate equality using constraint-base testing. Using Math.Abs to get the absolute difference between actual and expected then checking it against our allowable difference is somewhat simpler to write than the Is.InRange version above. However the only information we get from this is how far outside of our acceptable difference the actual value was. We don't get the value itself.
#endif

                void ThrowArgumentException() => throw new ArgumentException();
                void DoNotThrow() { };
                Assert.That(ThrowArgumentException, Throws.ArgumentException); // Assert checking for an expected exception type using Contraint-based testing.
#if ENABLE_FAILING_TESTS
                Assert.That(ThrowArgumentException, Throws.InvalidOperationException); // Assert checking for an expected exception type using Contraint-based testing. This is meant to fail.
#endif
                Assert.That(DoNotThrow, Throws.Nothing); // Assert checking that no exception is thrown using Constraint-based testing.
#if ENABLE_FAILING_TESTS
                Assert.That(ThrowArgumentException, Throws.Nothing); // Assert checking that no exception is thrown. This is meant to fail.
#endif
            });

#if ENABLE_THROWING_TESTS
            Assert.That(false, Is.True, "I will never run because I'm outside of the Assert.Multiple and it had failures.");
#endif
        }

        [Test(Description = "When a lot of tests using FluentAssertions-baset checking that should be in separate test methods are run, then some will intentionally fail as examples of failure")]
        public void WhenLotsOfTestsUsingFluentAssertionsInOneMethodThatShouldBeInSeparateTestMethodsRun_ThenSomeIntentionallyFail()
        {
            // AssertionScope allows testing to continue if a test fails. This unnamed scope is a catch all that will exist to the end of this
            // method. Named scopes provide more context in situations where the test has a lot of components because it requires a complex
            // series of steps in order to get to the starting state for what we actually want to test. If you do have a complex setup and
            // want to exit if anything has failed when you reach a certain point, you can call the scope's HasFailures method and if it
            // returns true you can just use return; to exit the test method instead of continuing.
            using var unnamedAssertionScope = new AssertionScope(); // If it wasn't for this then the test would exit when AssertionScope we created for the floating point tests was disposed at the end of its using statement. 

            double sut = 1.1;
            double allowableDifference = 0.2;
            double expected = 1.1;
            double approximateExpected = 1.0;
            unnamedAssertionScope.HasFailures().Should().BeFalse("because we aren't expecting any failures yet.");

            using (var defaultValuesAssertionScope = new AssertionScope("Floating point testing"))
            {
                sut.Should().Be(expected, "because we are requiring exact equality");
                sut.Should().BeApproximately(approximateExpected, allowableDifference, "because floating point values aren't always exact");
#if ENABLE_FAILING_TESTS
                sut.Should().BeApproximately(approximateExpected + allowableDifference * 10, allowableDifference, "because this demonstrates a failed BeApproximately check");
#endif
            }

            //// Note: this should not fail.
            //unnamedAssertionScope.HasFailures().Should().BeTrue("because some of the floating point tests were meant to fail and we are expecting that this AssertionScope will keep things going even though the AssertionScope we created for the floating point tests has been disposed.");

            using (var exceptionTestingAssertionScope = new AssertionScope("Exception testing"))
            {
                Action throwArgumentException = () => throw new ArgumentException();
                Action doNotThrow = () => { };
                throwArgumentException.Should().Throw<ArgumentException>("because it is meant to throw an ArgumentException");
#if ENABLE_FAILING_TESTS
                throwArgumentException.Should().Throw<InvalidOperationException>("because this is an example of a failed test");
#endif
                doNotThrow.Should().NotThrow("because this is an example of checking that an exception is not thrown");
#if ENABLE_FAILING_TESTS
                throwArgumentException.Should().NotThrow("because this is an example of a failed test");
#endif
            }
        }
    }
}
