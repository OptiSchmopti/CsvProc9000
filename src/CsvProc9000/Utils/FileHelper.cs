using CsvProc9000.Utils.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using Ude;

namespace CsvProc9000.Utils
{
    [ExcludeFromCodeCoverage] // mostly untestable because of file-magic
    internal sealed class FileHelper : IFileHelper
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<FileHelper> _logger;

        public FileHelper(
            [NotNull] IFileSystem fileSystem,
            [NotNull] ILogger<FileHelper> logger)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public Encoding DetectEncodingOfFile(string fileName)
        {
            using var fileStream = _fileSystem.FileStream.Create(fileName, FileMode.Open, FileAccess.Read);
            
            _logger.LogTrace("Trying to detect charset of '{File}'", fileName);

            var detector = new CharsetDetector();
            detector.Feed(fileStream);
            detector.DataEnd();
            var charset = detector.Charset;
            
            _logger.LogTrace("Found charset '{Charset}' with a confidence of {Confidence}", 
                charset, detector.Confidence);

            var encodingFromCharset = GetEncodingFromCharsetString(charset);
            return encodingFromCharset;
        }

        public Encoding GetEncodingFromCharsetString(string charset)
        {
            var encoding = Encoding.GetEncoding(charset);
            if (encoding == null)
                throw new ArgumentException($"Could not find encoding for charset '{charset}'", nameof(charset));
            
            return encoding;
        }
    }
}
