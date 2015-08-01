using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void TestTemplates()
        {
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6).Replace("/", @"\");
            string templateFolder = System.IO.Path.Combine(folder, @"..\..\..\..\WorldModel\WorldModel\Core");
            Dictionary<string, TemplateData> templates = EditorController.GetAvailableTemplates(templateFolder);
            List<string> tempFiles = new List<string>();

            foreach (TemplateData template in templates.Values)
            {
                string tempFile = System.IO.Path.GetTempFileName();

                var initialFileText = EditorController.CreateNewGameFile(tempFile, template.Filename, "Test");
                System.IO.File.WriteAllText(tempFile, initialFileText);
                EditorController controller = new EditorController();
                string errorsRaised = string.Empty;
                
                controller.ShowMessage += (object sender, TextAdventures.Quest.EditorController.ShowMessageEventArgs e) =>
                {
                    errorsRaised += e.Message;
                };

                bool result = controller.Initialise(tempFile, templateFolder);

                Assert.IsTrue(result, string.Format("Initialisation failed for template '{0}': {1}", System.IO.Path.GetFileName(template.Filename), errorsRaised));
                Assert.AreEqual(0, errorsRaised.Length, string.Format("Error loading game with template '{0}': {1}", System.IO.Path.GetFileName(template.Filename), errorsRaised));

                tempFiles.Add(tempFile);
            }

            try
            {
                foreach (string tempFile in tempFiles)
                {
                    System.IO.File.Delete(tempFile);
                }
            }
            catch { }
        }
    }
}
