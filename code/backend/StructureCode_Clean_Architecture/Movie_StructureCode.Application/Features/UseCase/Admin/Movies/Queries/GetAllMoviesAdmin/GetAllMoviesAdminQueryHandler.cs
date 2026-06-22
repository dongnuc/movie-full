using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Movie_StructureCode.Application.Abstractions.Services.Storage;
using Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common;
using Movie_StructureCode.Application.Mappers;
using Movie_StructureCode.Contract.Abstractions.Message;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Movie_StructureCode.Domain.Repositories;

namespace Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Queries.GetAllMoviesAdmin
{
    internal sealed class GetAllMoviesAdminQueryHandler : IQueryHandler<GetAllMoviesAdminQuery, PagedResult<MovieResponse>>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IStorageService _storageService;

        public GetAllMoviesAdminQueryHandler(IMovieRepository movieRepository, IStorageService storageService)
        {
            _movieRepository = movieRepository;
            _storageService = storageService;
        }

        public async Task<Result<PagedResult<MovieResponse>>> Handle(
            GetAllMoviesAdminQuery request,
            CancellationToken cancellationToken)
        {
            var movies = await _movieRepository.GetMoviesPagedAsync(
                request.Search,
                request.CategoryId,
                request.IsActive,
                request.Page,
                request.PageSize,
                cancellationToken);

            var responseItems = movies.Items.Select(m => MovieMapper.ToResponse(m, _storageService)).ToList();
            var result = PagedResult<MovieResponse>.Create(
                responseItems,
                movies.PageIndex,
                movies.PageSize,
                movies.TotalCount);

            return Result.Success(result);
        }
    }
}
