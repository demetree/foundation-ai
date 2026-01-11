using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Foundation.CommonConstants;



namespace Foundation.Controllers
{
    /// <summary>
    /// 
    /// This is the base class for any Dashboard controller on a field gear app.
    /// 
    /// It provides common services like log file listing, deleting, and downloading.
    ///
    /// </summary>
    [ApiController]
    public abstract class DashboardControllerBase : ControllerBase
    {
        private const int LOG_FILE_PREVIEW_ROW_COUNT = 100;

        private readonly ILogger<DashboardControllerBase> _logger;

        private DateTime _timeOfLastLogFileDownload = DateTime.MinValue;

        private DateTime _timeOfLastLogFilePreview = DateTime.MinValue;

        public DashboardControllerBase(ILogger<DashboardControllerBase> logger)
        {
            this._logger = logger;
        }

        #region Log File Access

        /// <summary>
        /// 
        /// This gets a list of file names in the log directory.
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LogFiles")]
        public IActionResult GetLogFiles()
        {
            var data = Logger.GetCommonLogger().GetLogFileDetails();

            //
            // Sort be last modified time descending
            //
            data.Sort((x, y) => DateTime.Compare(y.lastModified, x.lastModified));

            return Ok(data);
        }



        //
        /// <summary>
        /// 
        /// This version zips and streams at the same time to dramatically increase the performance
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpGet("LogFiles/{fileName}")]
        public async Task<IActionResult> DownloadLogFile(string fileName, bool zipContents = true)
        {
            if (string.IsNullOrEmpty(fileName) == true)
            {
                return BadRequest();
            }


            //
            //  Only allow log file download requests once every second
            //
            if (DateTime.Now.Subtract(_timeOfLastLogFileDownload).TotalSeconds < 1)
            {
                // 429 is the code for too many requests.
                return StatusCode(429);
            }

            string filePath = Path.Combine(Logger.GetCommonLogger().GetDirectory(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }


            if (zipContents == true)
            {
                _logger.LogInformation("About to process request to zip and download log file: " + filePath);

                // Set the response headers for the file download
                Response.ContentType = MIME_TYPE_ZIP;
                Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}.zip";

                try
                {
                    // Use a response stream to zip and stream at the same time
                    using (var zipStream = new ZipArchive(Response.Body, ZipArchiveMode.Create, leaveOpen: false))
                    {
                        // Add a new entry in the zip archive
                        var zipEntry = zipStream.CreateEntry(fileName);

                        // Write the file content to the zip entry
                        using (var entryStream = zipEntry.Open())
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not zip and stream log file.  Error is: " + ex.Message);
                    return Problem("Unable to zip log file.");
                }

                _logger.LogInformation("Completed zipping and sending data for log data folder: " + filePath);
            }
            else
            {
                _logger.LogInformation("About to process request to download log file: " + filePath);

                // Set the response headers for the file download
                Response.ContentType = MIME_TYPE_TXT;
                Response.Headers["Content-Disposition"] = $"inline; filename={fileName}";
                Response.StatusCode = 200; // Explicitly set 200 OK

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        await fileStream.CopyToAsync(Response.Body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Could not stream log file.  Error is: " + ex.Message);
                    return Problem("Unable to stream log file.");
                }


                _logger.LogInformation("Completed sending data for log file: " + filePath);

            }

            _timeOfLastLogFileDownload = DateTime.Now;

            return new EmptyResult();  // Response is already streamed
        }


        [HttpGet("LogFilePreview/{fileName}")]
        public async Task<IActionResult> PreviewLogFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) == true)
            {
                return BadRequest();
            }


            //
            //  Only allow log file preview requests once every 10th of a second.
            //
            if (DateTime.Now.Subtract(_timeOfLastLogFilePreview).TotalMilliseconds < 100)
            {
                // 429 is the code for too many requests.
                return StatusCode(429);
            }

            string filePath = Path.Combine(Logger.GetCommonLogger().GetDirectory(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            _logger.LogInformation("About to process request to preview log file: " + filePath);

            // Set the response headers for the file download
            Response.ContentType = MIME_TYPE_TXT;
            Response.Headers["Content-Disposition"] = $"inline; filename=Preview_{fileName}";
            Response.StatusCode = 200; // Explicitly set 200 OK

            try
            {
                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    await fileStream.CopyToAsync(memoryStream);

                    memoryStream.Position = 0;

                    using (StreamReader reader = new StreamReader(memoryStream))
                    {
                        string content = reader.ReadToEnd();

                        // Split the string by new lines to create a string array
                        string[] data = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        //
                        // Write up to 100 of the most recent log messages to the response body
                        //
                        int loopLimit = 0;

                        if (data.Count() > LOG_FILE_PREVIEW_ROW_COUNT)
                        {
                            loopLimit = data.Count() - LOG_FILE_PREVIEW_ROW_COUNT;
                        }

                        for (int i = data.Count() - 1; i >= loopLimit; i--)
                        {
                            await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(data[i]));
                            await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not stream log file.  Error is: " + ex.Message);
                return Problem("Unable to stream log file.");
            }


            _logger.LogInformation("Completed sending preview data for log file: " + filePath);

            _timeOfLastLogFilePreview = DateTime.Now;

            return new EmptyResult();  // Response is already streamed
        }



        [HttpDelete]
        [Route("LogFiles/{fileName}")]
        public IActionResult DeleteLogFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) == true)
            {
                return BadRequest();
            }

            try
            {

                //
                // Get the actual folder path
                // 
                string realFile = System.IO.Path.Combine(Logger.GetCommonLogger().GetDirectory(), fileName);

                if (System.IO.File.Exists(realFile) == false)
                {
                    return BadRequest();
                }

                _logger.LogInformation("About to process request to delete file: " + realFile);

                //
                // Delete the log file
                //
                System.IO.File.Delete(realFile);

                _logger.LogInformation("Deleted log file: " + realFile);

                return Ok();
            }
            catch (Exception)
            {
                return Problem("Unable delete log file.");
            }
        }

        #endregion
    }
}
