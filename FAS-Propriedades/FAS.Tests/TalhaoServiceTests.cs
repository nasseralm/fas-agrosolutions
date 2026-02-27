using AutoMapper;
using FAS.Application.DTOs;
using FAS.Application.Mappings;
using FAS.Application.Services;
using FAS.Domain.Entities;
using FAS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace FAS.Tests
{
    public class TalhaoServiceTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new EntitiesToDTOMappingProfile()));
            return config.CreateMapper();
        }

        [Fact]
        public async Task Incluir_DeveRetornarErro_QuandoPropriedadeNaoExiste()
        {
            var mockTalhaoRepo = new Mock<ITalhaoRepository>();
            var mockPropRepo = new Mock<IPropriedadeRepository>();
            mockPropRepo.Setup(r => r.Selecionar(It.IsAny<int>())).ReturnsAsync((Propriedade)null);

            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<TalhaoService>>();

            var service = new TalhaoService(mockTalhaoRepo.Object, mockPropRepo.Object, mockUow.Object, CreateMapper(), mockLogger.Object);

            var dto = new TalhaoDTO
            {
                PropriedadeId = 999,
                Nome = "T1",
                Cultura = "Soja",
                Delimitacao = JsonDocument.Parse("{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}").RootElement
            };

            var result = await service.Incluir(producerId: 1, isAdmin: false, dto);

            Assert.True(result.HasNotifications);
            Assert.Contains("Propriedade não encontrada.", result.Notifications);
            mockUow.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public async Task Incluir_DeveRetornarErro_QuandoProdutorNaoEhDono()
        {
            var propriedade = new Propriedade(
                producerId: 2,
                nome: "Fazenda",
                codigo: null,
                descricaoLocalizacao: "X",
                municipio: null,
                uf: null,
                areaTotalHectares: null,
                localizacao: null,
                localizacaoGeoJson: null);

            var mockTalhaoRepo = new Mock<ITalhaoRepository>();
            var mockPropRepo = new Mock<IPropriedadeRepository>();
            mockPropRepo.Setup(r => r.Selecionar(10)).ReturnsAsync(propriedade);

            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<TalhaoService>>();

            var service = new TalhaoService(mockTalhaoRepo.Object, mockPropRepo.Object, mockUow.Object, CreateMapper(), mockLogger.Object);

            var dto = new TalhaoDTO
            {
                PropriedadeId = 10,
                Nome = "T1",
                Cultura = "Soja",
                Delimitacao = JsonDocument.Parse("{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}").RootElement
            };

            var result = await service.Incluir(producerId: 1, isAdmin: false, dto);

            Assert.True(result.HasNotifications);
            Assert.Contains("Acesso negado.", result.Notifications);
            mockUow.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public async Task Incluir_DeveRetornarErro_QuandoDelimitacaoAusente()
        {
            var propriedade = new Propriedade(
                producerId: 1,
                nome: "Fazenda",
                codigo: null,
                descricaoLocalizacao: "X",
                municipio: null,
                uf: null,
                areaTotalHectares: null,
                localizacao: null,
                localizacaoGeoJson: null);

            var mockTalhaoRepo = new Mock<ITalhaoRepository>();
            var mockPropRepo = new Mock<IPropriedadeRepository>();
            mockPropRepo.Setup(r => r.Selecionar(10)).ReturnsAsync(propriedade);

            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<TalhaoService>>();

            var service = new TalhaoService(mockTalhaoRepo.Object, mockPropRepo.Object, mockUow.Object, CreateMapper(), mockLogger.Object);

            var dto = new TalhaoDTO
            {
                PropriedadeId = 10,
                Nome = "T1",
                Cultura = "Soja",
                Delimitacao = null
            };

            var result = await service.Incluir(producerId: 1, isAdmin: false, dto);

            Assert.True(result.HasNotifications);
            Assert.Contains("Delimitação (GeoJSON) é obrigatória.", result.Notifications);
        }
    }
}
