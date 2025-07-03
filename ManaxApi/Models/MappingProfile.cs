using AutoMapper;
using ManaxApi.Models.Issue;
using ManaxApi.Models.Rank;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Issue;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.DTOs.Read;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;

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
        CreateMap<ChapterIssue, IssueDTO>();
        CreateMap<IssueCreateDTO, ChapterIssue>();

        // Mappings des Chapter
        CreateMap<Chapter.Chapter, ChapterDTO>();

        // Mappings des Read
        CreateMap<Read.Read, ReadDTO>();
        CreateMap<ReadCreateDTO, Read.Read>();

        // Mappings des Rank
        CreateMap<Rank.Rank, RankDTO>();
        CreateMap<Rank.Rank, RankCreateDTO>();
        CreateMap<UserRank, UserRankDTO>();
    }
}