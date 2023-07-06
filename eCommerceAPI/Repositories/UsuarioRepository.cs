using eCommerceAPI.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;

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
            return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();
        }

        public Usuario Get(int id)
        {
            return _connection.Query<Usuario, Contato, Usuario>("SELECT * FROM Usuarios as U LEFT JOIN Contatos as C ON C.UsuarioId = U.Id WHERE U.Id = @Id", 
                (usuario, contato) =>
                {
                    usuario.Contato = contato;
                    return usuario;
                },
                new {Id = id}).SingleOrDefault();
        }

        public void Insert(Usuario usuario)
        {
            _connection.Open();

            //Cria transacao para caso qualquer operacao falhe, elas sao canceladas.
            IDbTransaction transaction = _connection.BeginTransaction();

            try
            {
                string query = "INSERT INTO Usuarios (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                usuario.Id = _connection.Query<int>(query, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string queryContato = "INSERT INTO Contatos(UsuarioId, Telefone, Celular) VALUES (@UsuarioId, @Telefone, @Celular);SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(queryContato, usuario.Contato, transaction).Single();
                }

                transaction.Commit();
            }
            catch(Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    throw new Exception("Ocorreu um erro na inserção do usuário, favor verificar!");
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Update(Usuario usuario)
        {
            _connection.Open();

            //Cria transacao para caso qualquer operacao falhe, elas sao canceladas.
            IDbTransaction transaction = _connection.BeginTransaction();

            try
            {
                string query = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id";
                _connection.Execute(query, usuario, transaction);

                if(usuario.Contato != null)
                {
                    string queryContato = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id";
                    _connection.Execute(queryContato, usuario.Contato, transaction);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    throw new Exception("Ocorreu um erro na atualização do usuário, favor verificar!");
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }
    }
}
