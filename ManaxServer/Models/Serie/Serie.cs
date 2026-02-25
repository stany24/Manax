// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using ManaxLibrary.DTO.Serie;
using ManaxServer.Settings;

namespace ManaxServer.Models.Serie;

public class Serie
{
    public const string PosterName = "poster";
    public const string BannerName = "banner";

    public Serie(SerieCreateDto serieCreate, SavePoint.SavePoint savePoint)
    {
        SavePoint = savePoint;
        Title = serieCreate.Title;
        FolderName = serieCreate.Title;
        Description = "";
        Status = Status.Ongoing;
        Creation = DateTime.UtcNow;
        LastModification = DateTime.UtcNow;
    }

    public Serie()
    {
    }

    public long Id { get; set; }
    public List<Person.Person> Persons { get; set; } = [];
    public List<Tag.Tag> Tags { get; set; } = [];
    public Library.Library? Library { get; set; }
    public long SavePointId { get; set; }
    [ForeignKey(nameof(SavePointId))] public SavePoint.SavePoint SavePoint { get; set; } = null!;

    [MaxLength(255)] public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }
    public string SavePath => Path.Combine(SavePoint.Path, FolderName);

    public string PosterPath => Path.Combine(SavePath, PosterName + "." +
                                                       SettingsManager.DataDto.PosterFormat.ToString()
                                                           .ToLower(CultureInfo.InvariantCulture));

    public string BannerPath => Path.Combine(SavePath + BannerName + "." +
                                             SettingsManager.DataDto.BannerFormat.ToString()
                                                 .ToLower(CultureInfo.InvariantCulture));

    public string ChapterPath(uint nb)
    {
        return Path.Combine(SavePath, nb.ToString(CultureInfo.InvariantCulture),
            SettingsManager.DataDto.ArchiveFormat.ToString().ToLower(CultureInfo.InvariantCulture));
    }

    public SerieDto ToDto()
    {
        return new SerieDto
        {
            Id = Id,
            PersonIds = Persons.Select(person => person.Id).ToList(),
            TagIds = Tags.Select(tag => tag.Id).ToList(),
            LibraryId = Library?.Id,
            Title = Title,
            Description = Description,
            Status = Status,
            Creation = Creation,
            LastModification = LastModification
        };
    }

    public void Update(SerieUpdateDto serieUpdate, ManaxContext context)
    {
        Title = serieUpdate.Title;
        Description = serieUpdate.Description;
        Library = context.Libraries.Find(serieUpdate.LibraryId);
        Status = serieUpdate.Status;
        LastModification = DateTime.UtcNow;

        Tags.Clear();
        foreach (Tag.Tag tag in serieUpdate.TagIds.Select(tagId => context.Tags.Find(tagId)).OfType<Tag.Tag>())
            Tags.Add(tag);

        Persons.Clear();
        foreach (Person.Person person in serieUpdate.PersonIds.Select(personId => context.Persons.Find(personId))
                     .OfType<Person.Person>()) Persons.Add(person);
    }
}