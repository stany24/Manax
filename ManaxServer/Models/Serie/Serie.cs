// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;

namespace ManaxServer.Models.Serie;

public class Serie
{
    public long Id { get; set; }
    public List<Person.Person> Persons { get; set; } = [];
    public List<Tag.Tag> Tags { get; set; } = [];
    public Library.Library? Library { get; set; }
    public SavePoint.SavePoint SavePoint { get; set; } = null!;

    public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime Creation { get; set; }
    public DateTime LastModification { get; set; }

    public string SavePath => SavePoint.Path + Path.DirectorySeparatorChar + FolderName;
    
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
        foreach (TagDto tagDto in serieUpdate.Tags)
        {
            Tag.Tag? tag = context.Tags.Find(tagDto.Id);
            if (tag != null)
            {
                Tags.Add(tag);
            }
        }
        
        Persons.Clear();
        foreach (PersonDto personDto in serieUpdate.Persons)
        {
            Person.Person? person = context.People.Find(personDto.Id);
            if (person != null)
            {
                Persons.Add(person);
            }
        }
    }
}