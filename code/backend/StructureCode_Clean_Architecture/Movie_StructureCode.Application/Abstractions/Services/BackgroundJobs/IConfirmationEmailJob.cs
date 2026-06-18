using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.BackgroundJobs
{
    public interface IConfirmationEmailJob
    {
        Task SendConfirmationEmailAsync(string email, string confirmationLink, CancellationToken cancellationToken);
    }
}
