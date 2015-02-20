using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitSourceTestBase
    {
        protected override IUnitTestElementsSource FileExplorer
        {
            get
            {
                return new XunitTestElementsSource(new XunitTestProvider(),
                    Solution.GetComponent<UnitTestElementFactory>(),
                    Solution.GetComponent<SearchDomainFactory>(),
                    Solution.GetComponent<IShellLocks>());
            }
        }

        protected override string GetIdString(IUnitTestElement element)
        {
            return string.Format("{0}::{1}::{2}", element.Id.Provider.ID, element.Id.PersistentProjectId.Id, element.Id.Id);
        }
    }
}