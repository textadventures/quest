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
                await File.WriteAllTextAsync(tempFile, initialFileText);
                var controller = new EditorController();
                var errorsRaised = string.Empty;

                controller.ShowMessage += OnControllerOnShowMessage;
                var result = await controller.Initialise(tempFile);

                Assert.IsTrue(result,
                    $"Initialisation failed for template '{template.ResourceName}': {errorsRaised}");
                Assert.AreEqual(0, errorsRaised.Length,
                    $"Error loading game with template '{template.ResourceName}': {errorsRaised}");
                
                controller.ShowMessage -= OnControllerOnShowMessage;
                controller.Uninitialise();
                
                tempFiles.Add(tempFile);
                continue;

                void OnControllerOnShowMessage(object _, EditorController.ShowMessageEventArgs e)
                {
                    errorsRaised += e.Message;
                }
            }

            try
            {
                foreach (var tempFile in tempFiles)
                {
                    File.Delete(tempFile);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
