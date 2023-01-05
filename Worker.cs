using CRM.MQ.Consumer.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CRM.MQ.Consumer
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;

		public Worker(ILogger<Worker> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Consumindo dados de Clientes Importados do Excel para o MySQL: {time}", DateTimeOffset.Now);
				//
				var factory = new ConnectionFactory() { HostName = "localhost" };
				using (var connection = factory.CreateConnection())
				using (var channel = connection.CreateModel())
				{
					channel.QueueDeclare(queue: "Clientes Importados para BD MySQL",
										 durable: false,
										 exclusive: false,
										 autoDelete: false,
										 arguments: null);

					var consumer = new EventingBasicConsumer(channel);

					consumer.Received += (model, ea) =>
					{
						var body = ea.Body.ToArray();
						var message = Encoding.UTF8.GetString(body);

						IEnumerable<ClienteDTO> clientesDto = JsonSerializer.Deserialize<IEnumerable<ClienteDTO>>(message);

						Console.WriteLine($"Model:, {message}");
					};
					channel.BasicConsume(queue: "Clientes Importados para BD MySQL",
										 autoAck: true,
										 consumer: consumer);

					Console.WriteLine("aguarde...");
					
				}
					await Task.Delay(30000, stoppingToken);
			}
		}
	}
}