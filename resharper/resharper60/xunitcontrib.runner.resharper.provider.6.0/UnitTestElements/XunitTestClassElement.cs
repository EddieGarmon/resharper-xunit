using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class XunitTestClassElement : IUnitTestElement, ISerializableUnitTestElement
    {
        private readonly ProjectModelElementEnvoy projectModelElementEnvoy;
        private readonly CacheManager cacheManager;
        private readonly PsiModuleManager psiModuleManager;

        public XunitTestClassElement(IUnitTestProvider provider, ProjectModelElementEnvoy projectModelElementEnvoy, CacheManager cacheManager, PsiModuleManager psiModuleManager, IClrTypeName typeName, string assemblyLocation)
        {
            Provider = provider;
            this.projectModelElementEnvoy = projectModelElementEnvoy;
            this.cacheManager = cacheManager;
            this.psiModuleManager = psiModuleManager;
            TypeName = typeName;
            AssemblyLocation = assemblyLocation;
            Children = new List<IUnitTestElement>();
        }

        public IUnitTestProvider Provider { get; private set; }
        public IUnitTestElement Parent { get; set; }

        public ICollection<IUnitTestElement> Children { get; private set; }

        public string ShortName
        {
            get { return TypeName.ShortName; }
        }

        public bool Explicit
        {
            get { return !string.IsNullOrEmpty(ExplicitReason); }
        }

        public UnitTestElementState State { get; set; }

        public IProject GetProject()
        {
            return projectModelElementEnvoy.GetValidProjectElement() as IProject;
        }

        public string GetPresentation()
        {
            return ShortName;
        }

        public UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(TypeName.GetNamespaceName());
        }

        public UnitTestElementDisposition GetDisposition()
        {
            var element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                               declaration.GetNameDocumentRange().TextRange,
                                                               declaration.GetDocumentRange().TextRange);
            return new UnitTestElementDisposition(locations, this);
        }

        public IDeclaredElement GetDeclaredElement()
        {
            var p = GetProject();
            if (p == null)
                return null;

            var psiModule = psiModuleManager.GetPrimaryPsiModule(p);
            if (psiModule == null)
                return null;

            return cacheManager.GetDeclarationsCache(psiModule, false, true).GetTypeElementByCLRName(TypeName);
        }

        public IEnumerable<IProjectFile> GetProjectFiles()
        {
            var declaredElement = GetDeclaredElement();
            if (declaredElement == null)
                return EmptyArray<IProjectFile>.Instance;

            return from sourceFile in declaredElement.GetSourceFiles()
                   select sourceFile.ToProjectFile();
        }

        // ReSharper 6.1
        public IList<UnitTestTask> GetTaskSequence(IList<IUnitTestElement> explicitElements)
        {
            return GetTaskSequence((IEnumerable<IUnitTestElement>)explicitElements);
        }

        // ReSharper 6.0
        public IList<UnitTestTask> GetTaskSequence(IEnumerable<IUnitTestElement> explicitElements)
        {
            return new List<UnitTestTask>
                       {
                           new UnitTestTask(null, new XunitTestAssemblyTask(AssemblyLocation)),
                           new UnitTestTask(this, new XunitTestClassTask(AssemblyLocation, TypeName.FullName, explicitElements.Contains(this)))
                       };
        }

        public string Kind
        {
            get { return "xUnit.net Test Class"; }
        }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return UnitTestElementCategory.Uncategorized; }
        }

        public string ExplicitReason
        {
            // xunit doesn't support class level skip
            get { return string.Empty; }
        }

        public string Id { get { return TypeName.FullName; } }

        public string AssemblyLocation { get; private set; }
        public IClrTypeName TypeName { get; private set; }

        public bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestClassElement);
        }

        private bool Equals(XunitTestClassElement other)
        {
            if (other == null)
                return false;

            return Equals(TypeName, other.TypeName) &&
                   Equals(AssemblyLocation, other.AssemblyLocation);
        }

        public void WriteToXml(XmlElement element)
        {
            element.SetAttribute("projectId", GetProject().GetPersistentID());
        }

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, ISolution solution, UnitTestElementFactory unitTestElementFactory)
        {
            var id = parent.GetAttribute("Id");
            var projectId = parent.GetAttribute("projectId");

            var project = (IProject)ProjectUtil.FindProjectElementByPersistentID(solution, projectId);
            if (project == null)
                return null;
            var assemblyLocation = UnitTestManager.GetOutputAssemblyPath(project).FullPath;

            return unitTestElementFactory.GetOrCreateTestClass(project, new ClrTypeName(id), assemblyLocation);
        }

        public void AddChild(XunitTestMethodElement xunitTestMethodElement)
        {
            Children.Add(xunitTestMethodElement);
        }

        public void RemoveChild(XunitTestMethodElement xunitTestMethodElement)
        {
            Children.Remove(xunitTestMethodElement);
        }
    }
}