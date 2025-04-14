using AkanjiApp.Models;

namespace AkanjiApp.Repository
{
    public interface IDocumentRepository: IRepository<Documento>
    {

        Task<Documento> GetByDoiAsync(string doi);
    }
}
