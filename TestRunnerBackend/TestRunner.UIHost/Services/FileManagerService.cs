using Grpc.Core;
using TestRunner.Contract.Grpc.V1;

namespace UIHost.Services;

internal class FileManagerService : FileManager.FileManagerBase
{
    public FileManagerService()
    {
    }

    public override Task<FileSystemTreeResponse> ChangeDirectory(ChangeDirectoryRequest request,
        ServerCallContext context)
    {
        return base.ChangeDirectory(request, context);
    }

    public override Task<ChooseTargetDirectoryResponse> ChooseTargetDirectory(ChooseTargetDirectoryRequest request,
        ServerCallContext context)
    {
        return base.ChooseTargetDirectory(request, context);
    }

    public override Task<FileSystemTreeResponse> OpenFileSystem(OpenFileSystemRequest request,
        ServerCallContext context)
    {
        return base.OpenFileSystem(request, context);
    }
}