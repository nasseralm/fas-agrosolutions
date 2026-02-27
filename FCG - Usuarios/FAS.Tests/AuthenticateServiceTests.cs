using System.Reflection;
using System.Text;
using FAS.API.Models;
using FAS.Application.Services;
using FAS.Domain.Entities;
using FAS.Domain.EventSourcing;
using FAS.Domain.Interfaces;
using FAS.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;

namespace FAS.Tests
{
    /// <summary>
    /// T-007: Testes unitários do fluxo de login (credenciais válidas/inválidas).
    /// </summary>
    public class AuthenticateServiceTests
    {
        private static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
        }

        private static UsuarioViewModel CriarUsuarioViewModel(int id, string nome, string email, bool isAdmin)
        {
            var vm = new UsuarioViewModel();
            foreach (var (name, value) in new[] { ("Id", (object)id), ("Nome", nome), ("Email", email), ("IsAdmin", isAdmin) })
            {
                var p = typeof(UsuarioViewModel).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                p?.SetValue(vm, value);
            }
            return vm;
        }

        [Fact]
        public async Task Login_CredenciaisValidas_RetornaToken()
        {
            var email = "produtor@teste.com";
            var senha = "Senha123!";
            var (hash, salt) = HashPassword(senha);
            var usuario = new Usuario(1, "Produtor Teste", new Email(email), StatusProdutor.Ativo);
            usuario.AlterarSenha(hash, salt);

            var mockRepo = new Mock<IUsuarioRepository>();
            mockRepo.Setup(r => r.SelecionarPorEmail(email)).ReturnsAsync(usuario);

            var usuarioViewModel = CriarUsuarioViewModel(1, usuario.Nome, email, false);
            var mockUsuarioService = new Mock<FAS.Application.Interfaces.IUsuarioService>();
            mockUsuarioService.Setup(s => s.SelecionarPorEmail(email)).ReturnsAsync(usuarioViewModel);

            var configDict = new Dictionary<string, string>
            {
                ["jwt:secretKey"] = "ChaveSecretaMinima32Caracteres!!",
                ["jwt:issuer"] = "test",
                ["jwt:audience"] = "test"
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();

            var mockLogger = new Mock<ILogger<AuthenticateService>>();
            var mockUow = new Mock<IUnitOfWork>();
            mockUow.Setup(u => u.Commit()).ReturnsAsync(true);
            var mockEventPublisher = new Mock<IEventPublisher>();

            var service = new AuthenticateService(
                mockLogger.Object, config, mockUsuarioService.Object,
                mockRepo.Object, mockUow.Object, mockEventPublisher.Object);

            var loginDto = new LoginDTO { EmailUsuario = email, Password = senha };
            var result = await service.Login(loginDto);

            Assert.False(result.HasNotifications);
            Assert.True(result.HasResult);
            Assert.NotNull(result.Result?.Token);
            Assert.NotEmpty(result.Result.Token);
        }

        [Fact]
        public async Task Login_CredenciaisInvalidas_SenhaErrada_RetornaNotificacao()
        {
            var email = "produtor@teste.com";
            var (hash, salt) = HashPassword("Senha123!");
            var usuario = new Usuario(1, "Produtor", new Email(email), StatusProdutor.Ativo);
            usuario.AlterarSenha(hash, salt);

            var mockRepo = new Mock<IUsuarioRepository>();
            mockRepo.Setup(r => r.SelecionarPorEmail(email)).ReturnsAsync(usuario);

            var mockUsuarioService = new Mock<FAS.Application.Interfaces.IUsuarioService>();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
            var mockLogger = new Mock<ILogger<AuthenticateService>>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockEventPublisher = new Mock<IEventPublisher>();

            var service = new AuthenticateService(
                mockLogger.Object, config, mockUsuarioService.Object,
                mockRepo.Object, mockUow.Object, mockEventPublisher.Object);

            var loginDto = new LoginDTO { EmailUsuario = email, Password = "SenhaErrada!" };
            var result = await service.Login(loginDto);

            Assert.True(result.HasNotifications);
            Assert.Contains(result.Notifications, n => n.Contains("credenciais inválidas") || n.Contains("não autenticado"));
        }

        [Fact]
        public async Task Login_UsuarioNaoEncontrado_RetornaNotificacao()
        {
            var email = "naoexiste@teste.com";
            var mockRepo = new Mock<IUsuarioRepository>();
            mockRepo.Setup(r => r.SelecionarPorEmail(email)).ReturnsAsync((Usuario)null);

            var mockUsuarioService = new Mock<FAS.Application.Interfaces.IUsuarioService>();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
            var mockLogger = new Mock<ILogger<AuthenticateService>>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockEventPublisher = new Mock<IEventPublisher>();

            var service = new AuthenticateService(
                mockLogger.Object, config, mockUsuarioService.Object,
                mockRepo.Object, mockUow.Object, mockEventPublisher.Object);

            var loginDto = new LoginDTO { EmailUsuario = email, Password = "Senha123!" };
            var result = await service.Login(loginDto);

            Assert.True(result.HasNotifications);
            Assert.Contains(result.Notifications, n => n.Contains("não localizado") || n.Contains("não encontrado"));
        }

        [Fact]
        public async Task Login_ProdutorInativo_RetornaNotificacao()
        {
            var email = "inativo@teste.com";
            var (hash, salt) = HashPassword("Senha123!");
            var usuario = new Usuario(1, "Inativo", new Email(email), StatusProdutor.Inativo);
            usuario.AlterarSenha(hash, salt);

            var mockRepo = new Mock<IUsuarioRepository>();
            mockRepo.Setup(r => r.SelecionarPorEmail(email)).ReturnsAsync(usuario);

            var mockUsuarioService = new Mock<FAS.Application.Interfaces.IUsuarioService>();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
            var mockLogger = new Mock<ILogger<AuthenticateService>>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockEventPublisher = new Mock<IEventPublisher>();

            var service = new AuthenticateService(
                mockLogger.Object, config, mockUsuarioService.Object,
                mockRepo.Object, mockUow.Object, mockEventPublisher.Object);

            var loginDto = new LoginDTO { EmailUsuario = email, Password = "Senha123!" };
            var result = await service.Login(loginDto);

            Assert.True(result.HasNotifications);
            Assert.Contains(result.Notifications, n => n.Contains("inativa") || n.Contains("Inativo"));
        }
    }
}
