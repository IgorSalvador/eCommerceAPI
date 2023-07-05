using eCommerceAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace eCommerceAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;

        public UsuarioRepository()
        {
            _connection = new SqlConnection("Data Source=DESKTOP-IU2EP5O;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public List<Usuario> Get()
        {
            return _db;
        }

        public Usuario Get(int id)
        {
            return _db.FirstOrDefault(u => u.Id == id);
        }

        public void Insert(Usuario usuario)
        {
            Usuario lastUser = _db.LastOrDefault();

            if(lastUser == null)
            {
                usuario.Id = 1;
            }
            else
            {
                usuario.Id = lastUser.Id + 1;
            }

            _db.Add(usuario);
        }

        public void Update(Usuario usuario)
        {
            _db.Remove(_db.FirstOrDefault(u => u.Id == usuario.Id));
            _db.Add(usuario);
        }

        public void Delete(int id)
        {
            _db.Remove(_db.FirstOrDefault(u => u.Id == id));
        }

        private static List<Usuario> _db = new List<Usuario>()
        {
            new Usuario(){ Id = 1, Nome = "Felipe Rodrigues", Email = "felipe.rodrigues@gmail.com"},
            new Usuario(){ Id = 2, Nome = "Marcelo Rodrigues", Email = "marcelo.rodrigues@gmail.com"},
            new Usuario(){ Id = 3, Nome = "Jessica Rodrigues", Email = "jessica.rodrigues@gmail.com"}
        };
    }
}
