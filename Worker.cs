using CRM.MQ.Consumer.Model;
using CRM.MQ.Consumer.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace CRM.MQ.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connSql = _configuration.GetConnectionString("ConnectionSqlServer");

            _logger.LogInformation("Importando dados de Clientes do BD MySQL para o SQL Server: {time}", DateTimeOffset.Now);

            //CONSUMINDO A FILA
            //IEnumerable<ClienteDTO> clientesDto = null;
            //SQL Server
            //ExcluirSql(connSql);
            

            while (!stoppingToken.IsCancellationRequested)
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                var factory2 = new ConnectionFactory() { HostName = "localhost" };
                //factory.AutomaticRecoveryEnabled = true;
                //factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
                //factory.TopologyRecoveryEnabled = true;

                //Fila Cliente
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "clientes",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                        var cli = default(ClienteDTO);
                        consumer.Received += (model, ea) =>
                        {
                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var cli = JsonConvert.DeserializeObject<ClienteDTO>(message);
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("*********************************************************************************************************************");
                                Console.WriteLine($"[x] CNPJ: {cli.CnpjNumInscricao} | Nome: {cli.NomeEmpresarial} ");
                                if (cli != null)
                                {
                                    InsereClienteSqlServer(cli, connSql);
                                }
                                // Thread.Sleep(10000);
                                //channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" Erro: {0}", ex.Message);
                                //channel.BasicNack(ea.DeliveryTag, false, true);
                            }
                        };
                        channel.BasicConsume(queue: "clientes",
                                             autoAck: true,
                                             consumer: consumer);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Aguardando...");
                    }
                }

                //Fila Cliente2
                using (var connection = factory2.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "clientes2",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                        var cli = default(ClienteDTO);
                        consumer.Received += (model, ea) =>
                        {
                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                var cli = JsonConvert.DeserializeObject<ClienteDTO>(message);
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine("*********************************************************************************************************************");
                                Console.WriteLine($"[x] CNPJ: { cli.CnpjNumInscricao } | Nome: { cli.NomeEmpresarial } ");
                                if (cli != null)
                                {
                                    InsereClienteSqlServer(cli, connSql);
                                }
                               // Thread.Sleep(10000);
                                //channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" Erro: {0}", ex.Message);
                                //channel.BasicNack(ea.DeliveryTag, false, true);
                            }
                        };
                        channel.BasicConsume(queue: "clientes2",
                                             autoAck: true,
                                             consumer: consumer);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Aguardando...");
                    }
                }
                await Task.Delay(5000, stoppingToken);
            }
        }

        public void NotifyUser(IEnumerable<ClienteDTO> message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                //var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                //notificationService.NotifyUser(message.NomeEmpresarial, message.CnpjNumInscricao, message.FatBrutoAnual);
            }
        }
        public void ExcluirSql(string ConnectionString)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("prc_cliente_del", connection);
                command.Connection.Open();
                command.CommandType = CommandType.StoredProcedure;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void InsereClienteSqlServer(ClienteDTO item, string ConnectionString)
        {
         
            int returnValue = 0;
            
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {

                    SqlCommand command = new SqlCommand("prc_cliente_ins", connection);
                    command.Connection.Open();
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdHtml", item.IdHtml);
                    command.Parameters["@IdHtml"].Direction = ParameterDirection.Input;
                    command.Parameters.AddWithValue("@CodInterno", item.CodInterno);
                    command.Parameters.AddWithValue("@CnpjParametro", item.CnpjParametro);
                    command.Parameters.AddWithValue("@CnpjConsultado", item.CnpjConsultado);
                    command.Parameters.AddWithValue("@CnpjNumInscricao", item.CnpjNumInscricao);
                    command.Parameters.AddWithValue("@NomeEmpresarial", item.NomeEmpresarial);
                    command.Parameters.AddWithValue("@NomeFantasia", item.NomeFantasia);
                    command.Parameters.AddWithValue("@PorteEmpresa", item.PorteEmpresa);
                    command.Parameters.AddWithValue("@FatBrutoAnual", item.FatBrutoAnual);
                    command.Parameters.AddWithValue("@Ativo", item.Ativo);
                    command.Parameters.AddWithValue("@Logradouro", item.Logradouro);
                    command.Parameters.AddWithValue("@Numero", item.Numero);
                    command.Parameters.AddWithValue("@Bairro", item.Bairro);
                    command.Parameters.AddWithValue("@Complemento", item.Complemento);
                    command.Parameters.AddWithValue("@CEP", item.CEP);
                    command.Parameters.Add("@Id", SqlDbType.Int);
                    command.Parameters["@Id"].Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    returnValue = (Int32)command.Parameters["@Id"].Value;
                    connection.Close();

                }
        }
    }
}