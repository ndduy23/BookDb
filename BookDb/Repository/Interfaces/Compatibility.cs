using BookDb.Models;

namespace BookDb.Repository.Interfaces
{
 // Compatibility interfaces that extend the canonical ones in BookDb.Repositories.Interfaces
 public interface IGenericRepository<T> : global::BookDb.Repositories.Interfaces.IGenericRepository<T> where T : class { }

 public interface IDocumentRepository : global::BookDb.Repositories.Interfaces.IDocumentRepository { }

 public interface IDocumentPageRepository : global::BookDb.Repositories.Interfaces.IDocumentPageRepository { }

 public interface IRefreshTokenRepository : global::BookDb.Repositories.Interfaces.IRefreshTokenRepository { }
}
