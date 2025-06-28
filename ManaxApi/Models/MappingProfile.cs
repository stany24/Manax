using AutoMapper;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Rank;

namespace ManaxApi.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mappings entre modèles et DTOs seront configurés ici
        // Exemple : CreateMap<ModeleSource, DTODestination>();
            
        // Mappings des User
        CreateMap<User.User, UserDTO>();
        CreateMap<UserCreateDTO, User.User>();
        CreateMap<UserUpdateDTO, User.User>();
            
        // Mappings des Serie
        CreateMap<Serie.Serie, SerieDTO>();
        CreateMap<SerieCreateDTO, Serie.Serie>();
        CreateMap<SerieUpdateDTO, Serie.Serie>();
            
        // Mappings des Library
        CreateMap<Library.Library, LibraryDTO>();
        CreateMap<LibraryCreateDTO, Library.Library>();
        CreateMap<LibraryUpdateDTO, Library.Library>();
            
        // Mappings des Issue
        CreateMap<Issue.Issue, IssueDTO>();
        CreateMap<IssueCreateDTO, Issue.Issue>();
            
        // Mappings des Chapter
        CreateMap<Chapter.Chapter, ChapterDTO>();
            
        // Mappings des Read
        CreateMap<Read.Read, ReadDTO>();
        CreateMap<ReadCreateDTO, Read.Read>();
        
        // Mappings des Rank
        CreateMap<Rank.Rank, RankDTO>();
        CreateMap<Rank.Rank, RankCreateDTO>();
    }
}