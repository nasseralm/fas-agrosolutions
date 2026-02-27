using System.Reflection;
using System.Security.Claims;
using FAS.API.Controllers;
using FAS.Application.Interfaces;
using FAS.Domain.Notifications;
using FAS.Infra.Ioc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FAS.Tests
{
    /// <summary>
    /// T-010: Testes unitários — "usuário A não acessa dados do usuário B".
    /// </summary>
    public class UsuarioAutorizacaoTests
    {
        private static FAS.API.Models.UsuarioViewModel CriarUsuarioViewModel(int id, string nome, string email, bool isAdmin)
        {
            var vm = new FAS.API.Models.UsuarioViewModel();
            foreach (var prop in new (string Name, object Value)[] { ("Id", id), ("Nome", nome), ("Email", email), ("IsAdmin", isAdmin) })
            {
                var p = typeof(FAS.API.Models.UsuarioViewModel).GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                p?.SetValue(vm, prop.Value);
            }
            return vm;
        }

        private static ClaimsPrincipal CriarUsuario(int producerId, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim("id", producerId.ToString()),
                new Claim("ProducerId", producerId.ToString()),
                new Claim("email", $"user{producerId}@teste.com"),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "Produtor")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void GetProducerId_RetornaIdDoToken()
        {
            var user = CriarUsuario(42);
            Assert.Equal(42, user.GetProducerId());
            Assert.Equal(42, user.GetId());
        }

        [Fact]
        public void GetProducerId_FallbackParaClaimId_QuandoProducerIdAusente()
        {
            var claims = new List<Claim> { new Claim("id", "99") };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            Assert.Equal(99, user.GetProducerId());
        }

        [Fact]
        public async Task Selecionar_UsuarioATentaAcessarDadosDoUsuarioB_RetornaForbid()
        {
            var producerIdA = 1;
            var idUsuarioB = 2;

            var mockUsuarioService = new Mock<IUsuarioService>();
            var mockLogger = new Mock<ILogger<UsuarioController>>();

            var controller = new UsuarioController(mockUsuarioService.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = CriarUsuario(producerIdA, isAdmin: false) }
                }
            };

            var result = await controller.Selecionar(idUsuarioB);

            var forbidResult = result as ForbidResult;
            Assert.NotNull(forbidResult);
            mockUsuarioService.Verify(s => s.Selecionar(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Selecionar_UsuarioAcessaPropriosDados_ChamaServico()
        {
            var producerId = 1;
            var mockUsuarioService = new Mock<IUsuarioService>();
            var viewModel = CriarUsuarioViewModel(producerId, "A", "a@teste.com", false);
            mockUsuarioService.Setup(s => s.Selecionar(producerId))
                .ReturnsAsync(new DomainNotificationsResult<FAS.API.Models.UsuarioViewModel> { Result = viewModel });

            var mockLogger = new Mock<ILogger<UsuarioController>>();
            var controller = new UsuarioController(mockUsuarioService.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = CriarUsuario(producerId, isAdmin: false) }
                }
            };

            var result = await controller.Selecionar(producerId);

            mockUsuarioService.Verify(s => s.Selecionar(producerId), Times.Once);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task Excluir_UsuarioATentaExcluirUsuarioB_RetornaForbid()
        {
            var producerIdA = 1;
            var idUsuarioB = 2;

            var mockUsuarioService = new Mock<IUsuarioService>();
            var mockLogger = new Mock<ILogger<UsuarioController>>();
            var controller = new UsuarioController(mockUsuarioService.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = CriarUsuario(producerIdA, isAdmin: false) }
                }
            };

            var result = await controller.Excluir(idUsuarioB);

            var forbidResult = result as ForbidResult;
            Assert.NotNull(forbidResult);
            mockUsuarioService.Verify(s => s.Excluir(It.IsAny<int>()), Times.Never);
        }
    }
}
