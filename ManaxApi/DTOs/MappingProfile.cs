using AutoMapper;

namespace ManaxApi.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mappings entre modèles et DTOs seront configurés ici
            // Exemple : CreateMap<ModeleSource, DTODestination>();
            
            // Mappings des User
            CreateMap<Models.User.User, UserDTO>();
            CreateMap<UserCreateDTO, Models.User.User>();
            CreateMap<UserUpdateDTO, Models.User.User>();
            
            // Mappings des Serie
            CreateMap<Models.Serie.Serie, SerieDTO>();
            CreateMap<SerieCreateDTO, Models.Serie.Serie>();
            CreateMap<SerieUpdateDTO, Models.Serie.Serie>();
            
            // Mappings des Library
            CreateMap<Models.Library.Library, LibraryDTO>();
            CreateMap<LibraryCreateDTO, Models.Library.Library>();
            CreateMap<LibraryUpdateDTO, Models.Library.Library>();
            
            // Mappings des Issue
            CreateMap<Models.Issue.Issue, IssueDTO>();
            CreateMap<IssueCreateDTO, Models.Issue.Issue>();
            
            // Mappings des Chapter
            CreateMap<Models.Chapter.Chapter, ChapterDTO>();
            
            // Mappings des Read
            CreateMap<Models.Read.Read, ReadDTO>();
            CreateMap<ReadCreateDTO, Models.Read.Read>();
        }
    }
}
