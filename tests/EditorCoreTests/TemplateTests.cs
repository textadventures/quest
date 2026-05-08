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

            foreach (var template in templates.Values)
            {
                var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
                var bytes = System.Text.Encoding.UTF8.GetBytes(initialFileText);
                var controller = new EditorController();
                var errorsRaised = string.Empty;

                controller.ShowMessage += OnControllerOnShowMessage;
                var result = await controller.Initialise(new Config(), new QuestViva.Common.ByteArrayGameDataProvider(bytes, "test.aslx"), partialInit: true);

                Assert.IsTrue(result,
                    $"Initialisation failed for template '{template.ResourceName}': {errorsRaised}");
                Assert.AreEqual(0, errorsRaised.Length,
                    $"Error loading game with template '{template.ResourceName}': {errorsRaised}");

                controller.ShowMessage -= OnControllerOnShowMessage;
                controller.Uninitialise();
                continue;

                void OnControllerOnShowMessage(object _, EditorController.ShowMessageEventArgs e)
                {
                    errorsRaised += e.Message;
                }
            }
        }
    }
}
