using AutoMapper;

namespace ManaxLibrary.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mappings entre modèles et DTOs seront configurés ici
            // Exemple : CreateMap<ModeleSource, DTODestination>();
            
            // Mappings des User
            CreateMap<ManaxApi.Models.User.User, UserDTO>();
            CreateMap<UserCreateDTO, ManaxApi.Models.User.User>();
            CreateMap<UserUpdateDTO, ManaxApi.Models.User.User>();
            
            // Mappings des Serie
            CreateMap<ManaxApi.Models.Serie.Serie, SerieDTO>();
            CreateMap<SerieCreateDTO, ManaxApi.Models.Serie.Serie>();
            CreateMap<SerieUpdateDTO, ManaxApi.Models.Serie.Serie>();
            
            // Mappings des Library
            CreateMap<ManaxApi.Models.Library.Library, LibraryDTO>();
            CreateMap<LibraryCreateDTO, ManaxApi.Models.Library.Library>();
            CreateMap<LibraryUpdateDTO, ManaxApi.Models.Library.Library>();
            
            // Mappings des Issue
            CreateMap<ManaxApi.Models.Issue.Issue, IssueDTO>();
            CreateMap<IssueCreateDTO, ManaxApi.Models.Issue.Issue>();
            
            // Mappings des Chapter
            CreateMap<ManaxApi.Models.Chapter.Chapter, ChapterDTO>();
            
            // Mappings des Read
            CreateMap<ManaxApi.Models.Read.Read, ReadDTO>();
            CreateMap<ReadCreateDTO, ManaxApi.Models.Read.Read>();
        }
    }
}
