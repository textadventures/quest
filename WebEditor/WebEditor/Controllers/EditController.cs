using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Ionic.Zip;
using TextAdventures.Quest;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using WebEditor.Models;

namespace WebEditor.Controllers
{
    public class EditController : Controller
    {
        //
        // GET: /Edit/Game/1

        public ActionResult Game(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound();
            }
            Models.Editor model = new Models.Editor();
            model.GameId = id.Value;
            ViewBag.Title = "Quest";
            model.SimpleMode = GetSettingBool("simplemode", false);
            model.ErrorRedirect = ConfigurationManager.AppSettings["WebsiteHome"] ?? "http://textadventures.co.uk/";
            model.CacheBuster = Convert.ToInt32((DateTime.Now - (new DateTime(2012, 1, 1))).TotalSeconds);
            return View(model);
        }

        public JsonResult Load(int id, bool simpleMode)
        {
            Services.EditorService editor = new Services.EditorService();
            EditorDictionary[id] = editor;
            string libFolder = Server.MapPath("~/bin/Core/");
            string filename = Services.FileManagerLoader.GetFileManager().GetFile(id);
            if (filename == null)
            {
                Logging.Log.InfoFormat("Invalid game {0}", id);
                return Json(new { error = "Invalid ID" }, JsonRequestBehavior.AllowGet);
            }
            var result = editor.Initialise(id, filename, libFolder, simpleMode);
            if (!result.Success)
            {
                Logging.Log.InfoFormat("Failed to load game {0} - {1}", id, result.Error);
                return Json(new { error = result.Error.Replace(Environment.NewLine, "<br/>") }, JsonRequestBehavior.AllowGet);
            }

            string playFilename = Services.FileManagerLoader.GetFileManager().GetPlayFilename(id);

            return Json(new {
                tree = editor.GetElementTreeForJson(),
                editorstyle = editor.Style,
                playurl = ConfigurationManager.AppSettings["PlayURL"] + "?id=editor/" + playFilename,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Scripts(int id)
        {
            Logging.Log.DebugFormat("{0}: Get scripts JSON", id);
            return Json(EditorDictionary[id].GetScriptAdderJson(), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditAttribute(int id, string key, string attributeName)
        {
            var element = EditorDictionary[id].GetElementModelForView(id, key, "", null, null, ModelState);
            var data = (IEditorDataExtendedAttributeInfo)element.EditorData;

            var model = new EditAttributeModel
            {
                Element = element,
                Control = new AttributeSubEditorControlData(attributeName),
                Value = data.GetAttribute(attributeName)
            };

            return PartialView("EditAttribute", model);
        }

        public PartialViewResult EditElement(int id, string key, string tab, string error, string refreshTreeSelectElement)
        {
            Logging.Log.DebugFormat("{0}: EditElement {1}", id, key);
            if (Session["EditorDictionary"] == null)
            {
                return Timeout();
            }
            Models.Element model = EditorDictionary[id].GetElementModelForView(id, key, tab, error, refreshTreeSelectElement, ModelState);
            return PartialView("ElementEditor", model);
        }

        [HttpPost]
        public PartialViewResult SaveElement(Models.ElementSaveData element)
        {
            Logging.Log.DebugFormat("{0}: SaveElement {1}", element.GameId, element.Key);
            if (!element.Success)
            {
                Logging.Log.ErrorFormat("Element save failed");
                return Timeout();
            }
            var result = EditorDictionary[element.GameId].SaveElement(element.Key, element);
            string tab = !string.IsNullOrEmpty(element.AdditionalAction) ? element.AdditionalActionTab : null;
            return EditElement(element.GameId, element.RedirectToElement, tab, result.Error, result.RefreshTreeSelectElement);
        }

        [HttpPost]
        public PartialViewResult ProcessAction(int id, string key, string tab, string actionCmd)
        {
            var result = EditorDictionary[id].ProcessAdditionalAction(key, actionCmd);
            return EditElement(id, key, tab, null, result.RefreshTreeSelectElement);
        }

        private PartialViewResult Timeout()
        {
            return PartialView("ElementEditor", new Models.Element
            {
                PopupError = "Sorry, your session has timed out.",
                Reload = "1"
            });
        }

        public PartialViewResult EditStringList(int id, string key, IEditorControl control)
        {
            return PartialView("StringListEditor", EditorDictionary[id].GetStringListModel(key, control));
        }

        public PartialViewResult EditScriptStringList(int id, string key, string path, IEditorControl control)
        {
            return PartialView("StringListEditor", EditorDictionary[id].GetScriptStringListModel(key, path, control));
        }

        public PartialViewResult EditScript(int id, string key, IEditorControl control)
        {
            return PartialView("ScriptEditor", EditorDictionary[id].GetScriptModel(id, key, control, ModelState));
        }

        public PartialViewResult EditScriptValue(int id, string key, IEditableScripts script, string attribute)
        {
            return PartialView("ScriptEditor", EditorDictionary[id].GetScriptModel(id, key, script, attribute, ModelState));
        }

        public PartialViewResult ElementsList(int id, string key, IEditorControl control)
        {
            return PartialView("ElementsList", EditorDictionary[id].GetElementsListModel(id, key, control));
        }

        public PartialViewResult EditExits(int id, string key, IEditorControl control)
        {
            return PartialView("ExitsEditor", EditorDictionary[id].GetExitsModel(id, key, control));
        }

        public PartialViewResult EditVerbs(int id, string key, IEditorControl control)
        {
            return PartialView("VerbsEditor", EditorDictionary[id].GetVerbsModel(id, key, control));
        }

        public PartialViewResult EditScriptDictionary(int id, string key, IEditorControl control)
        {
            return PartialView("ScriptDictionaryEditor", EditorDictionary[id].GetScriptDictionaryModel(id, key, control, ModelState));
        }

        public PartialViewResult EditScriptScriptDictionary(int id, string key, string path, IEditorControl control)
        {
            return PartialView("ScriptDictionaryEditor", EditorDictionary[id].GetScriptScriptDictionaryModel(id, key, path, control, ModelState));
        }

        public PartialViewResult EditScriptDictionaryValue(int id, string key, IEditableDictionary<IEditableScripts> value, string keyPrompt, string source, string attribute)
        {
            return PartialView("ScriptDictionaryEditor", EditorDictionary[id].GetScriptDictionaryModel(id, key, value, keyPrompt, source, attribute, ModelState));
        }

        public PartialViewResult EditStringDictionary(int id, string key, IEditorControl control)
        {
            return PartialView("StringDictionaryEditor", EditorDictionary[id].GetStringDictionaryModel(id, key, control, ModelState));
        }

        public PartialViewResult EditGameBookOptions(int id, string key, IEditorControl control)
        {
            return PartialView("StringDictionaryEditor", EditorDictionary[id].GetStringDictionaryModel(id, key, control, ModelState, true));
        }

        private Dictionary<int, Services.EditorService> EditorDictionary
        {
            get
            {
                Dictionary<int, Services.EditorService> result = Session["EditorDictionary"] as Dictionary<int, Services.EditorService>;
                if (result == null)
                {
                    result = new Dictionary<int, Services.EditorService>();
                    Session["EditorDictionary"] = result;
                }
                return result;
            }
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult RefreshTree(int id)
        {
            return Json(EditorDictionary[id].GetElementTreeForJson(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public RedirectToRouteResult SaveSettings(Models.Editor settings)
        {
            SaveSettingBool("simplemode", settings.SimpleMode);
            return RedirectToAction("Game", new { id = settings.GameId });
        }

        private void SaveSetting(string name, string value)
        {
            HttpCookie cookie = new HttpCookie("simplemode", value);
            cookie.Expires = DateTime.Now + new TimeSpan(30, 0, 0, 0);
            Response.Cookies.Add(cookie);
        }

        private string GetSetting(string name)
        {
            HttpCookie cookie = Request.Cookies.Get(name);
            if (cookie == null) return string.Empty;
            return cookie.Value;
        }

        private void SaveSettingBool(string name, bool value)
        {
            SaveSetting(name, value ? "1" : "0");
        }

        private bool GetSettingBool(string name, bool defaultValue)
        {
            string result = GetSetting(name);
            if (result == "1") return true;
            if (result == "0") return false;
            return defaultValue;
        }

        public ActionResult FileUpload(int id)
        {
            return View(new WebEditor.Models.FileUpload
            {
                GameId = id,
                AllFiles = GetAllFilesList(id)
            });
        }

        private static List<string> s_serverPermittedExtensions = new List<string>
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".wav",
            ".mp3"
        };

        [HttpPost]
        public ActionResult FileUpload(FileUpload fileModel)
        {
            if (!ModelState.IsValid)
            {
                return View(fileModel);
            }

            if (!EditorDictionary.ContainsKey(fileModel.GameId))
            {
                Logging.Log.ErrorFormat("FileUpload - game id {0} not in EditorDictionary", fileModel.GameId);
                return new HttpStatusCodeResult(500);
            }

            var ext = Path.GetExtension(fileModel.File.FileName).ToLower();
            var controlPermittedExtensions = EditorDictionary[fileModel.GameId].GetPermittedExtensions(fileModel.Key, fileModel.Attribute);

            if (fileModel.File == null
                || fileModel.File.ContentLength == 0
                || !s_serverPermittedExtensions.Contains(ext)
                || !controlPermittedExtensions.Contains(ext))
            {
                ModelState.AddModelError("File", "Invalid file type");
                return View(fileModel);
            }

            var filename = Path.GetFileName(fileModel.File.FileName);
            Logging.Log.DebugFormat("{0}: Upload file {1}", fileModel.GameId, filename);
            var uploadPath = Services.FileManagerLoader.GetFileManager().UploadPath(fileModel.GameId);

            if (Config.AzureFiles)
            {
                var container = GetAzureBlobContainer("editorgames");
                var blob = container.GetBlockBlobReference(uploadPath + "/" + filename);

                var continueSave = true;

                if (blob.Exists())
                {
                    using (var ms = new MemoryStream())
                    {
                        blob.DownloadToStream(ms);
                        ms.Position = 0;
                        if (!FilesAreIdentical(fileModel.File.InputStream, ms))
                        {
                            filename = Path.GetFileNameWithoutExtension(fileModel.File.FileName) + " " + DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss") + Path.GetExtension(fileModel.File.FileName);
                            blob = container.GetBlockBlobReference(uploadPath + "/" + filename);
                        }
                        else
                        {
                            // skip saving if files are identical
                            continueSave = false;
                        }
                    }
                }

                if (continueSave)
                {
                    blob.Properties.ContentType = "application/octet-stream";
                    blob.UploadFromStream(fileModel.File.InputStream);
                }
            }
            else
            {
                var continueSave = true;

                // Check to see if file with same name exists
                if (System.IO.File.Exists(Path.Combine(uploadPath, filename)))
                {
                    FileStream existingFile = new FileStream(Path.Combine(uploadPath, filename), FileMode.Open);

                    if (!FilesAreIdentical(fileModel.File.InputStream, existingFile))
                    {
                        // rename the file by adding a number [count] at the end of filename
                        filename = EditorUtility.GetUniqueFilename(fileModel.File.FileName);
                    }
                    else
                    {
                        // skip saving if files are identical
                        continueSave = false;
                    }

                    existingFile.Close();
                }

                if (continueSave)
                {
                    var saveFile = Path.Combine(uploadPath, filename);
                    fileModel.File.SaveAs(saveFile);
                    UploadOutputToAzure(saveFile);
                }
            }

            ModelState.Remove("AllFiles");
            fileModel.AllFiles = GetAllFilesList(fileModel.GameId);
            ModelState.Remove("PostedFile");
            fileModel.PostedFile = filename;

            return View(fileModel);
        }

        private string GetAllFilesList(int id)
        {
            var path = Services.FileManagerLoader.GetFileManager().UploadPath(id);
            if (path == null) return null;  // this will be the case if there was no logged-in user

            if (Config.AzureFiles)
            {
                var uploadPath = Services.FileManagerLoader.GetFileManager().UploadPath(id);
                var container = GetAzureBlobContainer("editorgames");
                var blobs = container.ListBlobs(uploadPath + "/");
                return string.Join(":", blobs.Select(b => Path.GetFileName(b.Uri.ToString())));
            }

            var files = Directory.GetFiles(path).Select(f => Path.GetFileName(f)).OrderBy(f => f);
            return string.Join(":", files);
        }

        public ActionResult Publish(int id)
        {
            Logging.Log.InfoFormat("Publishing game {0}", id);
            Services.EditorService editor = new Services.EditorService();
            string libFolder = Server.MapPath("~/bin/Core/");
            string filename = Services.FileManagerLoader.GetFileManager().GetFile(id);
            if (filename == null)
            {
                Logging.Log.InfoFormat("Publish failed for {0} - couldn't get file", id);
                return View("Error");
            }
            var result = editor.Initialise(id, filename, libFolder, false);
            if (!result.Success)
            {
                Logging.Log.InfoFormat("Publish failed for {0} - failed to initialise editor", id);
                return View("Error");
            }
            
            if (Config.AzureFiles)
            {
                var uploadPath = Services.FileManagerLoader.GetFileManager().UploadPath(id);
                var container = GetAzureBlobContainer("editorgames");
                var blobs = container.ListBlobs(uploadPath + "/");
                var includeFiles = new List<EditorController.PackageIncludeFile>();
                foreach (var blob in blobs.OfType<CloudBlockBlob>())
                {
                    if (blob.Name.EndsWith(".aslx")) continue;
                    var blobReference = container.GetBlockBlobReference(blob.Name);
                    var ms = new MemoryStream();
                    blobReference.DownloadToStream(ms);
                    ms.Position = 0;
                    includeFiles.Add(new EditorController.PackageIncludeFile
                    {
                        Filename = Path.GetFileName(blob.Uri.ToString()),
                        Content = ms
                    });
                }

                using (var outputStream = new MemoryStream())
                {
                    editor.Publish(null, includeFiles, outputStream);
                    outputStream.Position = 0;
                    var blob = container.GetBlockBlobReference(uploadPath + "/Output/" + Path.GetFileNameWithoutExtension(filename) + ".quest");
                    blob.UploadFromStream(outputStream);
                }
                
                foreach (var stream in includeFiles.Select(i => i.Content))
                {
                    stream.Dispose();
                }
            }
            else
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(filename), "Output");

                Directory.CreateDirectory(outputFolder);

                string outputFilename = Path.Combine(
                    outputFolder,
                    Path.GetFileNameWithoutExtension(filename) + ".quest");

                if (System.IO.File.Exists(outputFilename))
                {
                    System.IO.File.Delete(outputFilename);
                }

                Logging.Log.InfoFormat("Publishing {0} as {1}", id, outputFilename);

                editor.Publish(outputFilename, null, null);

                UploadOutputToAzure(outputFilename);

                Logging.Log.InfoFormat("Publish succeeded for {0}", id, outputFilename);
            }
            

            string url = ConfigurationManager.AppSettings["PublishURL"] + id;

            return Redirect(url);
        }

        private void UploadOutputToAzure(string filename)
        {
            // filename will be like "D:\Editor Games\guid\Output\file.quest"
            var container = GetAzureBlobContainer("editorgames");
            var blob = container.GetBlockBlobReference(filename.Substring(16).Replace(@"\", "/"));
            blob.Properties.ContentType = "application/octet-stream";
            
            var stream = System.IO.File.OpenRead(filename);
            blob.UploadFromStream(stream);
        }

        private static CloudBlobContainer GetAzureBlobContainer(string containerName)
        {
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var account = CloudStorageAccount.Parse(connectionString);

            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            return container;
        }

        private bool FilesAreIdentical(Stream file1, Stream file2)
        {
            int file1byte;
            int file2byte;

            // check the file-sizes; if they are not same then not identical
            if (file1.Length != file2.Length)
            {
                return false;
            }

            // do Byte by Byte Comparison
            do
            {
                file1byte = file1.ReadByte();
                file2byte = file2.ReadByte();
            } while ((file1byte == file2byte) && (file1byte != -1));
            
            // return result of comparison
            return ((file1byte - file2byte) == 0);
        }

        public ActionResult Download(int id)
        {
            ZipFile zip = new ZipFile();

            var uploadPath = Services.FileManagerLoader.GetFileManager().UploadPath(id);
            var container = GetAzureBlobContainer("editorgames");
            var blobs = container.ListBlobs(uploadPath + "/");
            foreach (var blob in blobs.OfType<CloudBlockBlob>())
            {
                var blobReference = container.GetBlockBlobReference(blob.Name);
                var ms = new MemoryStream();
                blobReference.DownloadToStream(ms);
                ms.Position = 0;

                zip.AddEntry(Path.GetFileName(blob.Uri.ToString()), ms);
            }

            return new FileGeneratingResult("game.zip", "application/zip", zip.Save);
        }
    }

    // From http://stackoverflow.com/questions/943122/writing-to-output-stream-from-action:
    /// <summary>
    /// MVC action result that generates the file content using a delegate that writes the content directly to the output stream.
    /// </summary>
    public class FileGeneratingResult : FileResult
    {
        /// <summary>
        /// The delegate that will generate the file content.
        /// </summary>
        private Action<System.IO.Stream> Content;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileGeneratingResult" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="content">The content.</param>
        public FileGeneratingResult(string fileName, string contentType, Action<System.IO.Stream> content)
            : base(contentType)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            this.Content = content;
            this.FileDownloadName = fileName;
        }

        /// <summary>
        /// Writes the file to the response.
        /// </summary>
        /// <param name="response">The response object.</param>
        protected override void WriteFile(System.Web.HttpResponseBase response)
        {
            this.Content(response.OutputStream);
        }
    }
}
