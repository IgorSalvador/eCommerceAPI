using eCommerceAPI.Models;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace eCommerceAPI.Repositories
{
    public class ContribUsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;

        public ContribUsuarioRepository()
        {
            _connection = new SqlConnection("Data Source=DESKTOP-IU2EP5O;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public List<Usuario> Get()
        {
            return _connection.GetAll<Usuario>().ToList(); 
        }

        public Usuario Get(int id)
        {
            return _connection.Get<Usuario>(id);
        }

        public void Insert(Usuario usuario)
        {
            usuario.Id = Convert.ToInt32(_connection.Insert(usuario));
        }

        public void Update(Usuario usuario)
        {
            _connection.Update(usuario);
        }
        public void Delete(int id)
        {
            _connection.Delete(Get(id));
        }
    }
}
