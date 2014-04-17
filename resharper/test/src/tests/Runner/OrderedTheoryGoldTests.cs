using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    [TestFixture("xunit1")]
    [TestFixture("xunit2")]
    public class OrderedTheoryGoldTests : XunitTaskRunnerTestBase
    {
        public OrderedTheoryGoldTests(string environmentId)
            : base(environmentId)
        {
        }

        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [Test]
        public void TestPassingTheory()
        {
            DoOneTestWithStrictOrdering("PassingTheory");
        }

        [Test]
        public void TestFailingTheory()
        {
            DoOneTestWithStrictOrdering("FailingTheory");
        }

        [Test]
        public void TestTheoryWithInvalidParameters()
        {
            DoOneTestWithStrictOrdering("TheoryWithInvalidParameters");
        }

        [Test]
        public void TestCustomTheoryAttributeSkips()
        {
            DoOneTestWithStrictOrdering("CustomTheoryAttributeSkips");
        }
    }
}