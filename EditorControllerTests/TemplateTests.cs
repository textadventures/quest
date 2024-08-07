using TextAdventures.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void TestTemplates()
        {
            var templateFolder = Path.Combine(["..", "..", "..", "..", "WorldModel", "WorldModel", "Core"]);

            var templates = EditorController.GetAvailableTemplates(templateFolder);
            var tempFiles = new List<string>();

            foreach (var template in templates.Values)
            {
                var tempFile = Path.GetTempFileName();

                var initialFileText = EditorController.CreateNewGameFile(tempFile, template.Filename, "Test");
                File.WriteAllText(tempFile, initialFileText);
                var controller = new EditorController();
                var errorsRaised = string.Empty;
                
                controller.ShowMessage += (_, e) =>
                {
                    errorsRaised += e.Message;
                };

                var result = controller.Initialise(tempFile, templateFolder);

                Assert.IsTrue(result,
                    $"Initialisation failed for template '{Path.GetFileName(template.Filename)}': {errorsRaised}");
                Assert.AreEqual(0, errorsRaised.Length,
                    $"Error loading game with template '{System.IO.Path.GetFileName(template.Filename)}': {errorsRaised}");

                tempFiles.Add(tempFile);
            }

            try
            {
                foreach (var tempFile in tempFiles)
                {
                    File.Delete(tempFile);
                }
            }
            catch { }
        }
    }
}
