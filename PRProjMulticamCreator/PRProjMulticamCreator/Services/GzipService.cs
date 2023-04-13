using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PRProjMulticamCreator.Services;

public class GzipService
{
    public async Task DecompressAndSave(string inputPath, string outputPath)
    {
        await using var inputFileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        await using var gzipStream = new GZipStream(inputFileStream, CompressionMode.Decompress);
        await using var outputFileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        await gzipStream.CopyToAsync(outputFileStream);
    }

    public async Task CompressAndSave(string compressedFilePath, string tempFilePath)
    {
        await using FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Create);
        await using GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionLevel.Optimal);
        await using FileStream tempFileStream = new FileStream(tempFilePath, FileMode.Open);
        await tempFileStream.CopyToAsync(gzipStream);
    }
}