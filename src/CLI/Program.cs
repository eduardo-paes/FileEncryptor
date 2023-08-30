using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using CLI.Factories;
using System.Text.Json;
using CLI.Models;

namespace CLI
{
    public class Program
    {
        #region Global Scope
        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        private static ILogger _logger = Log.Logger;
        private static int _exitCode = 0;
        #endregion

        /// <summary>
        /// Método principal da aplicação.
        /// Inicializa serviço de criptografia e descriptografia de arquivos.
        /// Parâmetros de configuração devem ser fornecidos através de linha de comando.
        /// </summary>
        /// <param name="args">Parâmetros de configuração: arquivo de configuração e id para gravação de logs no Interop.</param>
        public static void Main(string[] args)
        {
            try
            {
                // Inicializa o Logger
                InitializeLogger();

                // Inicializa o programa
                _logger.Information("Iniciando o programa.");

                // Verifica se os argumentos foram informados
                if (args.Length < 2)
                {
                    _exitCode = 1;
                    throw new Exception("Argumentos não informados. Sintaxe: ArqConfiguracao, CodInterop.");
                }

                // Obtém parâmetros de configuração
                string fileConfig = args[0];

                // Deserializa o arquivo de configuração e obtém as configurações
                var settings = GetConfig(fileConfig);

                // Inicializa o serviço de criptografia
                var cryptoService = CryptoServiceFactory.Create(Encoding.UTF8.GetBytes(settings.Key!));

                // Verifica se a operação é de criptografia ou descriptografia
                if (settings.Operation == "ENC")
                {
                    // Informa o início da criptografia
                    _logger.Information("Iniciando a criptografia dos arquivos do diretório: {Source}", settings.Source);

                    // Obtem a lista de arquivos do diretório
                    var files = GetFileListFromDirectory(settings.Source!);

                    // Criptografa cada arquivo da lista
                    foreach (var file in files)
                    {
                        // Obtém nome do arquivo
                        string fileName = Path.GetFileName(file);

                        // Gera nome do arquivo de destino
                        string destinationFile = Path.Combine(settings.Destination!, fileName);

                        // Adiciona extensão .enc ao arquivo de destino
                        destinationFile += ".enc";

                        // Verifica se o arquivo de destino já existe
                        if (File.Exists(destinationFile))
                        {
                            _logger.Warning("Sobrescrevendo arquivo já existente no destino: {Destination}", destinationFile);
                            File.Delete(destinationFile);
                        }

                        // Informa o início da criptografia do arquivo
                        _logger.Information("Criptografando arquivo: {File}", file);

                        // Criptografa o arquivo
                        cryptoService.EncryptFile(file, destinationFile);
                    }

                    // Informa o fim da criptografia
                    _logger.Information("Arquivos criptografados com sucesso no diretório: {Destination}", settings.Destination);
                }
                else if (settings.Operation == "DEC")
                {
                    // Informa o início da descriptografia
                    _logger.Information("Iniciando a descriptografia dos arquivos do diretório: {Source}", settings.Source);

                    // Obtem a lista de arquivos do diretório
                    var files = GetFileListFromDirectory(settings.Source!);

                    // Criptografa cada arquivo da lista
                    foreach (var file in files)
                    {
                        // Obtém nome do arquivo
                        string fileName = Path.GetFileName(file);

                        // Gera nome do arquivo de destino
                        string destinationFile = Path.Combine(settings.Destination!, fileName);

                        // Remove extensão .enc ao arquivo de destino
                        destinationFile = destinationFile.Replace(".enc", "");

                        // Verifica se o arquivo de destino já existe
                        if (File.Exists(destinationFile))
                        {
                            _logger.Warning("Sobrescrevendo arquivo já existente no destino: {Destination}", destinationFile);
                            File.Delete(destinationFile);
                        }

                        // Informa o início da descriptografia do arquivo
                        _logger.Information("Descriptografando arquivo: {File}", file);

                        // Criptografa o arquivo
                        cryptoService.DecryptFile(file, destinationFile);
                    }

                    // Informa o fim da descriptografia
                    _logger.Information("Arquivos descriptografados com sucesso no diretório: {Destination}", settings.Destination);
                }
                else
                {
                    _exitCode = 5;
                    throw new Exception("Operação inválida. Operações válidas: ENC, DEC.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Erro ao executar o programa: {ErrorDetails}", ex);
            }
            finally
            {
                _logger.Information("Finalizando o programa.");
                Log.CloseAndFlush();
                Environment.Exit(_exitCode);
            }
        }

        /// <summary>
        /// Inicializa o Logger.
        /// </summary>
        private static void InitializeLogger()
        {
            #region Configurações Iniciais
            // Obtem o nome do arquivo de log
            string? fileName = _configuration.GetSection("Settings").GetSection("FileName").Value;
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("Nome do arquivo não informado.");

            // Obtem o caminho para salvamento de log
            string? logDirPath = _configuration.GetSection("Settings").GetSection("LogPath").Value;
            if (string.IsNullOrEmpty(logDirPath))
                throw new Exception("Caminho para salvamento de log não informado.");
            #endregion

            #region Inicialização do Logger
            // Configura o Serilog
            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    Path.Combine(logDirPath!, fileName + ".log"),
                    outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss}] FormataDivida.Program.Main [{Level}] {Message}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Month,
                    retainedFileTimeLimit: TimeSpan.FromDays(90))
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss}] FormataDivida.Program.Main [{Level}] {Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .CreateLogger();
            #endregion
        }

        /// <summary>
        /// Deserializa o arquivo de configuração e obtém as configurações.
        /// </summary>
        /// <param name="fileConfig">Nome do arquivo de configuração.</param>
        /// <returns>Configurações obtidas.</returns>
        private static Settings GetConfig(string fileConfig)
        {
            // Obtem o caminho para o arquivo de configuração
            string? configDirPath = _configuration.GetSection("Settings").GetSection("ConfigPath").Value;

            // Verifica se o caminho para o arquivo de configuração foi informado
            if (string.IsNullOrEmpty(configDirPath))
            {
                _exitCode = 2;
                throw new Exception("Caminho para o arquivo de configuração não informado.");
            }

            // Verifica se o arquivo de configuração existe
            if (!File.Exists(Path.Combine(configDirPath!, fileConfig)))
            {
                _exitCode = 3;
                throw new Exception("Arquivo de configuração não encontrado.");
            }

            try
            {
                // Obtem o conteúdo do arquivo de configuração
                string configContent = File.ReadAllText(Path.Combine(configDirPath!, fileConfig));

                // Deserializa o arquivo de configuração
                var settings = JsonSerializer.Deserialize<Settings>(configContent);

                // Verifica se as configurações foram obtidas
                if (settings == null)
                    throw new Exception($"Não foi possível obter as configurações do arquivo: {fileConfig}");
                if (string.IsNullOrEmpty(settings.Source))
                    throw new Exception("Caminho de origem não informado.");
                if (string.IsNullOrEmpty(settings.Destination))
                    throw new Exception("Caminho de destino não informado.");
                if (string.IsNullOrEmpty(settings.Key))
                    throw new Exception("Chave de criptografia não informada.");
                if (string.IsNullOrEmpty(settings.Operation))
                    throw new Exception("Operação não informada.");

                return settings!;
            }
            catch (Exception ex)
            {
                _exitCode = 4;
                throw new Exception("Erro ao deserializar o arquivo de configuração.", ex);
            }
        }

        /// <summary>
        /// Obtém a lista de arquivos do diretório.
        /// </summary>
        /// <param name="path">Caminho do diretório.</param>
        /// <returns>Lista de arquivos do diretório.</returns>
        private static string[] GetFileListFromDirectory(string path)
        {
            // Verifica se o diretório existe
            if (!Directory.Exists(path))
            {
                _exitCode = 6;
                throw new Exception($"Diretório não encontrado: {path}");
            }

            // Obtem a lista de arquivos do diretório
            string[] files = Directory.GetFiles(path);

            // Verifica se o diretório está vazio
            if (files.Length == 0)
            {
                _exitCode = 6;
                throw new Exception($"Diretório vazio: {path}");
            }

            // Retorna a lista de arquivos
            return files;
        }
    }
}