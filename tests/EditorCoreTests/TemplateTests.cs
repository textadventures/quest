using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public async Task TestTemplates()
        {
            var templates = EditorController.GetAvailableTemplates();
            var tempFiles = new List<string>();

            foreach (var template in templates.Values)
            {
                var tempFile = Path.GetTempFileName();

                var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
                File.WriteAllText(tempFile, initialFileText);
                var controller = new EditorController();
                var errorsRaised = string.Empty;
                
                controller.ShowMessage += (_, e) =>
                {
                    errorsRaised += e.Message;
                };

                var result = await controller.Initialise(tempFile);

                Assert.IsTrue(result,
                    $"Initialisation failed for template '{template.ResourceName}': {errorsRaised}");
                Assert.AreEqual(0, errorsRaised.Length,
                    $"Error loading game with template '{template.ResourceName}': {errorsRaised}");

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
