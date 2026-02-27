using AutoMapper;
using FAS.Application.DTOs;
using FAS.Application.Mappings;
using FAS.Application.Services;
using FAS.Domain.Entities;
using FAS.Domain.Interfaces;
using FAS.Domain.Pagination;
using Microsoft.Extensions.Logging;
using Moq;

namespace FAS.Tests
{
    public class PropriedadeServiceTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new EntitiesToDTOMappingProfile()));
            return config.CreateMapper();
        }

        [Fact]
        public async Task Incluir_DeveRetornarErro_QuandoSemDescricaoELocalizacao()
        {
            var mockRepo = new Mock<IPropriedadeRepository>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<PropriedadeService>>();

            var service = new PropriedadeService(mockRepo.Object, mockUow.Object, CreateMapper(), mockLogger.Object);

            var dto = new PropriedadeDTO { Nome = "Fazenda A", DescricaoLocalizacao = null, Localizacao = null };

            var result = await service.Incluir(producerId: 1, dto);

            Assert.True(result.HasNotifications);
            Assert.Contains("Informe ao menos DescricaoLocalizacao ou Localizacao (GeoJSON).", result.Notifications);
            mockUow.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public async Task Incluir_DeveSalvar_QuandoDescricaoInformada()
        {
            var mockRepo = new Mock<IPropriedadeRepository>();
            mockRepo.Setup(r => r.Incluir(It.IsAny<Propriedade>()))
                .ReturnsAsync((Propriedade p) => p);

            var mockUow = new Mock<IUnitOfWork>();
            mockUow.Setup(u => u.Commit()).ReturnsAsync(true);

            var mockLogger = new Mock<ILogger<PropriedadeService>>();

            var service = new PropriedadeService(mockRepo.Object, mockUow.Object, CreateMapper(), mockLogger.Object);

            var dto = new PropriedadeDTO { Nome = "Fazenda A", DescricaoLocalizacao = "SP", Localizacao = null };

            var result = await service.Incluir(producerId: 1, dto);

            Assert.False(result.HasNotifications);
            Assert.NotNull(result.Result);
            mockRepo.Verify(r => r.Incluir(It.IsAny<Propriedade>()), Times.Once);
            mockUow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task Listar_DeveRetornarPagedList()
        {
            var props = new List<Propriedade>
            {
                new Propriedade(1, "F1", null, "X", null, null, null, null, null),
                new Propriedade(1, "F2", null, "Y", null, null, null, null, null)
            };

            var mockRepo = new Mock<IPropriedadeRepository>();
            mockRepo.Setup(r => r.ListarPorProducer(1, 1, 10))
                .ReturnsAsync(new PagedList<Propriedade>(props, 1, 10, 2));

            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<PropriedadeService>>();

            var service = new PropriedadeService(mockRepo.Object, mockUow.Object, CreateMapper(), mockLogger.Object);

            var result = await service.Listar(1, 1, 10);

            Assert.False(result.HasNotifications);
            Assert.NotNull(result.Result);
            Assert.Equal(2, result.Result.TotalCount);
        }
    }
}
