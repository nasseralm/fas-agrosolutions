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
    /// US-002/T-010: "usuário A não acessa dados do usuário B" aplicado a Propriedades.
    /// </summary>
    public class PropriedadeAutorizacaoTests
    {
        private static ClaimsPrincipal CriarUsuario(int producerId, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim("id", producerId.ToString()),
                new Claim("ProducerId", producerId.ToString()),
                new Claim("email", $"user{producerId}@teste.com"),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "Produtor")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static FAS.API.Models.PropriedadeViewModel CriarViewModel(int id, int producerIdDono)
        {
            var vm = (FAS.API.Models.PropriedadeViewModel)Activator.CreateInstance(typeof(FAS.API.Models.PropriedadeViewModel), nonPublic: true);
            Set(vm, "Id", id);
            Set(vm, "ProducerId", producerIdDono);
            Set(vm, "Nome", "Fazenda A");
            return vm;
        }

        private static void Set(object instance, string propertyName, object value)
        {
            var p = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            p?.SetValue(instance, value);
        }

        [Fact]
        public void GetProducerId_RetornaProducerIdDoToken()
        {
            var user = CriarUsuario(42);
            Assert.Equal(42, user.GetProducerId());
        }

        [Fact]
        public async Task Selecionar_ProdutorTentaAcessarPropriedadeDeOutroProdutor_RetornaForbid()
        {
            var producerA = 1;
            var producerB = 2;

            var mockService = new Mock<IPropriedadeService>();
            mockService.Setup(s => s.Selecionar(10))
                .ReturnsAsync(new DomainNotificationsResult<FAS.API.Models.PropriedadeViewModel>
                {
                    Result = CriarViewModel(id: 10, producerIdDono: producerB)
                });

            var mockLogger = new Mock<ILogger<PropriedadeController>>();
            var controller = new PropriedadeController(mockService.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = CriarUsuario(producerA, isAdmin: false) }
                }
            };

            var result = await controller.Selecionar(10);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Selecionar_AdminPodeAcessarQualquerPropriedade_RetornaOk()
        {
            var producerA = 1;
            var producerB = 2;

            var mockService = new Mock<IPropriedadeService>();
            mockService.Setup(s => s.Selecionar(10))
                .ReturnsAsync(new DomainNotificationsResult<FAS.API.Models.PropriedadeViewModel>
                {
                    Result = CriarViewModel(id: 10, producerIdDono: producerB)
                });

            var mockLogger = new Mock<ILogger<PropriedadeController>>();
            var controller = new PropriedadeController(mockService.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = CriarUsuario(producerA, isAdmin: true) }
                }
            };

            var result = await controller.Selecionar(10);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}

