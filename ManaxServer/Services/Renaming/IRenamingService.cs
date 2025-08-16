// filepath: /media/Pgm3/Git/Manax/ManaxServer/Services/IRenamingService.cs

using ManaxLibrary.DTO.Setting;

namespace ManaxServer.Services.Renaming;

public interface IRenamingService
{
    void RenameChapters();
    void RenamePosters(string oldName, string newName, ImageFormat oldFormat, ImageFormat newFormat);
}