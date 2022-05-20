using Microsoft.AspNetCore.Mvc;
using ResumableUploder.Api.DTOS;
using ResumableUploder.Api.Utilities;

namespace ResumableUploder.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        object _obj = new object();
        private readonly IWebHostEnvironment _env;

        public FilesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public IActionResult Upload(FileInfoDto fileInfo)
        {
            var uploadToken = fileInfo.ResumableUploadToken;

            int resumableChunkNumber = int.Parse(fileInfo.ResumableChunkNumber);

            //var resumableFilename = Cryption.Decrypt(fileInfo.ResumableFilename);
            var resumableFilename = fileInfo.ResumableFilename;

            var resumableFileExtension = fileInfo.ResumableFileExtension;

            var resumableIdentifier = fileInfo.ResumableIdentifier;

            int resumableChunkSize = int.Parse(fileInfo.ResumableChunkSize);

            long resumableTotalSize = long.Parse(fileInfo.ResumableTotalSize);

            //var resumableFolderPath = Cryption.Decrypt(fileInfo.ResumableFolderPath);
            var resumableFolderPath = fileInfo.ResumableFolderPath;

            var resumableUid = Cryption.Decrypt(fileInfo.ResumableUid);

            int resumableTotalChunkCount = int.Parse(fileInfo.ResumableTotalChunks);

            var localFilePath = string.Format("{0}/{1}/{2}-{3}/{2}-{3}.part{4:0000}", _env.ContentRootPath.Replace("\\", "/").Replace(@"\", "/"),
                resumableFolderPath, resumableUid, resumableIdentifier, resumableChunkNumber);

            var directory = Path.GetDirectoryName(localFilePath).Replace("\\", "/").Replace(@"\", "/");

            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            if (Request.Form.Files.Count == 1)
            {
                // save chunk
                if (!System.IO.File.Exists(localFilePath))
                {
                    using (var stream = new FileStream(localFilePath, FileMode.Create))
                    {
                        Request.Form.Files[0].CopyTo(stream);
                    }
                }

                // Check if all chunks are ready and save file
                var files = System.IO.Directory.GetFiles(directory);
                var partCount = Directory.EnumerateFiles(directory, "*.part*", SearchOption.AllDirectories)
                    .Count();

                if (partCount == resumableTotalChunkCount)
                {
                    Monitor.Enter(_obj);

                    var otherMainFilePath = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                                            .Where(d => !d.Contains(".part")).ToArray();
                    foreach (var file in otherMainFilePath)
                    {
                        System.IO.File.Delete(file);
                    }

                    var filePath = string.Format("{0}/{1}", directory, resumableFilename + "." + resumableFileExtension);
                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        foreach (string file in files.OrderBy(x => x))
                        {
                            var buffer = System.IO.File.ReadAllBytes(file);

                            fs.Write(buffer, 0, buffer.Length);

                            System.IO.File.Delete(file);
                        }

                        fs.Close();
                    }

                    var TempDirectory = _env.ContentRootPath.Replace("\\", "/").Replace(@"\", "/") + "/" + "temp/" + resumableFilename;
                    if (Directory.Exists(TempDirectory))
                    {
                        if (System.IO.File.Exists(TempDirectory + "/" + resumableFilename + "." + resumableFileExtension))
                        {
                            System.IO.File.Delete(TempDirectory + "/" + resumableFilename + "." + resumableFileExtension);
                        }
                        System.IO.File.Move(filePath, Path.Combine(TempDirectory, Path.GetFileName(filePath)));
                    }
                    else
                    {
                        Directory.CreateDirectory(TempDirectory);
                        System.IO.File.Move(filePath, Path.Combine(TempDirectory, Path.GetFileName(filePath)));
                    }
                    //Delete Directory in Video
                    int subDirectoryVideo = filePath.Split('/').Last().Length;
                    string ParentSubDirectoryVideo = filePath.Substring(0, filePath.Length - subDirectoryVideo);
                    System.IO.Directory.Delete(ParentSubDirectoryVideo, true);

                    Monitor.Exit(_obj);

                    return Ok(true);
                }
            }
            return Ok(true);
        }

        [HttpPost]
        public IActionResult CompleteAction([FromBody] CompleteActionInfoDto completeActionInfo)
        {
            string uniqueId = Cryption.Decrypt(completeActionInfo.UniqueId);
            var tempFolder = _env.ContentRootPath.Replace("\\", "/").Replace(@"\", "/") + "/Temp/" + uniqueId;
            var tempVideoDirectory = _env.ContentRootPath.Replace("\\", "/").Replace(@"\", "/") + "/VideoTemp/" + uniqueId;
            if (Directory.Exists(tempFolder))
            {
                var contentDirectory = Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories).OrderByDescending(f => f.Length).ToArray();
                if (Directory.Exists(tempVideoDirectory))
                {
                    if (contentDirectory.Count() != 1)
                    {
                        foreach (var item in contentDirectory)
                        {
                            System.IO.File.Move(item, Path.Combine(tempVideoDirectory, Path.GetFileName(item)));
                        }
                    }
                    else
                    {
                        System.IO.File.Move(contentDirectory[0], Path.Combine(tempVideoDirectory, Path.GetFileName(contentDirectory[0])));
                    }
                }
                else
                {
                    Directory.CreateDirectory(tempVideoDirectory);
                    if (contentDirectory.Count() != 1)
                    {
                        foreach (var item in contentDirectory)
                        {
                            System.IO.File.Move(item, Path.Combine(tempVideoDirectory, Path.GetFileName(item)));
                        }
                    }
                    else
                    {
                        System.IO.File.Move(contentDirectory[0], Path.Combine(tempVideoDirectory, Path.GetFileName(contentDirectory[0])));
                    }
                }

                Directory.Delete(tempFolder, true);
            }

            return Ok();
        }
    }
}
