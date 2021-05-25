using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;

namespace OpenSilver.TemplateWizards
{
    // This wizard applies a solution-wide build dependency from OpenSilverApplication.Browser to OpenSilverApplication
    class SetSlnDependencies : IWizard
    {
        Solution _solution;
        string _solutionName;

        public void BeforeOpeningFile(global::EnvDTE.ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(global::EnvDTE.Project project)
        {

        }

        public void ProjectItemFinishedGenerating(global::EnvDTE.ProjectItem projectItem)
        {

        }

        public void RunFinished()
        {
            Project projectMain = null;
            Project projectSimulator = null;
            
            foreach (Project project in _solution.Projects)
            {
                if (project.Name == $"{_solutionName}")
                {
                    projectMain = project;
                }
                else if (project.Name == $"{_solutionName}.Simulator")
                {
                    projectSimulator = project;
                }
            }

            if (projectMain == null || projectSimulator == null)
            {
                throw new WizardCancelledException("Some projects could not be resolved");
            }

            BuildDependency dependency = _solution.SolutionBuild.BuildDependencies.Item(projectSimulator.UniqueName);
            dependency.AddProject(projectMain.UniqueName);
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (automationObject is DTE dte)
            {
                _solution = dte.Solution;
                _solutionName = replacementsDictionary["$safeprojectname$"];
            }
            else
            {
                throw new WizardCancelledException("'automationObject' is not of type DTE");
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
