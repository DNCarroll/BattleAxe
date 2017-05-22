using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;

namespace BattleAxe.Class {
    public class Wizard : IWizard {
        public void BeforeOpeningFile(ProjectItem projectItem) {

        }

        public void ProjectFinishedGenerating(Project project) {

        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {

        }

        public void RunFinished() {

        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            var form = new Form2();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                
                var cc = new Creator(new CommandDefinition(form.CommandText.Text, form.GetConnectionString()));
                replacementsDictionary.Add("$NameSpace$", form.NameSpace.Text);
                replacementsDictionary.Add("$ClassName$", form.ClassName.Text);
                replacementsDictionary.Add("$CommandText$", form.CommandText.Text);
                replacementsDictionary.Add("$GetCaseStatements$", cc.GetCaseStatements());
                replacementsDictionary.Add("$SetCaseStatements$", cc.SetCaseStatemetns());
                replacementsDictionary.Add("$Properties$", cc.Properties());
            }
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }
    }
}