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
            List<Usuario> usuarios = new List<Usuario>();
            string query = "SELECT U.*, C.*, EE.*, D.* FROM Usuarios as U LEFT JOIN Contatos as C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega as EE ON EE.UsuarioId = U.Id LEFT JOIN UsuariosDepartamentos as UD ON UD.UsuarioId = U.Id LEFT JOIN Departamentos as D ON UD.DepartamentoId = D.Id";

            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(query,
                (usuario, contato, enderecoEntrega, departamento) =>
                {
                    //Verificacao do usuario
                    if (usuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                    {
                        usuario.Departamentos = new List<Departamento>();
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                    }

                    //Verificacao do endereco de entrega
                    if (usuario.EnderecosEntrega.SingleOrDefault(e => e.Id == enderecoEntrega.Id) == null)
                    {
                        usuario.EnderecosEntrega.Add(enderecoEntrega);
                    }

                    //Verificacao do departamento
                    if (usuario.Departamentos.SingleOrDefault(e => e.Id == departamento.Id) == null)
                    {
                        usuario.Departamentos.Add(departamento);
                    }


                    return usuario;
                });

            return usuarios;
        }

        public Usuario Get(int id)
        {
            List<Usuario> usuarios = new List<Usuario>();
            string query = "SELECT U.*, C.*, EE.*, D.* FROM Usuarios as U LEFT JOIN Contatos as C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega as EE ON EE.UsuarioId = U.Id LEFT JOIN UsuariosDepartamentos as UD ON UD.UsuarioId = U.Id LEFT JOIN Departamentos as D ON UD.DepartamentoId = D.Id WHERE U.Id = @Id";

            _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(query,
                (usuario, contato, enderecoEntrega, departamento) =>
                {
                    if (usuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                    {
                        usuario.Departamentos = new List<Departamento>();
                        usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                        usuario.Contato = contato;
                        usuarios.Add(usuario);
                    }
                    else
                    {
                        usuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                    }

                    //Verificacao do endereco de entrega
                    if (usuario.EnderecosEntrega.SingleOrDefault(e => e.Id == enderecoEntrega.Id) == null)
                    {
                        usuario.EnderecosEntrega.Add(enderecoEntrega);
                    }

                    //Verificacao do departamento
                    if (usuario.Departamentos.SingleOrDefault(e => e.Id == departamento.Id) == null)
                    {
                        usuario.Departamentos.Add(departamento);
                    }

                    return usuario;
                },
                new { Id = id });

            return usuarios.SingleOrDefault();
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

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Any())
                {
                    foreach (EnderecoEntrega enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string queryEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(queryEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                if (usuario.Departamentos != null && usuario.Departamentos.Any())
                {
                    foreach (Departamento departamento in usuario.Departamentos)
                    {
                        string queryUsuariosDepartamento = "INSERT INTO UsuariosDepartamentos(UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId)";
                        _connection.Execute(queryUsuariosDepartamento, new { UsuarioId = usuario.Id, DepartamentoId = departamento.Id }, transaction);
                    }
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

                if (usuario.Contato != null)
                {
                    string queryContato = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id";
                    _connection.Execute(queryContato, usuario.Contato, transaction);
                }

                string queryDeleteEnderecos = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @Id";
                _connection.Execute(queryDeleteEnderecos, usuario, transaction);

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Any())
                {
                    foreach (EnderecoEntrega enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string queryEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(queryEndereco, enderecoEntrega, transaction).Single();
                    }
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
