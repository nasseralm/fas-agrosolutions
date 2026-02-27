using AutoMapper;
using FAS.API.Models;
using FAS.Application.DTOs;
using FAS.Application.Interfaces;
using FAS.Domain.Entities;
using FAS.Domain.Interfaces;
using FAS.Domain.Pagination;
using FAS.Domain.Notifications;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using System.Text.Json;

namespace FAS.Application.Services
{
    public class PropriedadeService : IPropriedadeService
    {
        private const int DefaultSrid = 4326;

        private readonly IPropriedadeRepository _propriedadeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PropriedadeService> _logger;

        public PropriedadeService(
            IPropriedadeRepository propriedadeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PropriedadeService> logger)
        {
            _propriedadeRepository = propriedadeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DomainNotificationsResult<PropriedadeViewModel>> Incluir(int producerId, PropriedadeDTO dto)
        {
            var result = new DomainNotificationsResult<PropriedadeViewModel>();
            _logger.LogInformation("Iniciando inclusão de propriedade. ProducerId: {ProducerId} Nome: {Nome}", producerId, dto?.Nome);

            try
            {
                if (dto == null)
                {
                    result.Add("Payload inválido.");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(dto.DescricaoLocalizacao) && (!dto.Localizacao.HasValue || dto.Localizacao.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined))
                {
                    result.Add("Informe ao menos DescricaoLocalizacao ou Localizacao (GeoJSON).");
                    return result;
                }

                var (geometry, geoJson, geoError) = TryParseGeometry(dto.Localizacao, allowedTypes: new[] { "Point", "Polygon", "MultiPolygon" });
                if (geoError != null)
                {
                    result.Add(geoError);
                    return result;
                }

                var propriedade = new Propriedade(
                    producerId: producerId,
                    nome: dto.Nome,
                    codigo: dto.Codigo,
                    descricaoLocalizacao: dto.DescricaoLocalizacao,
                    municipio: dto.Municipio,
                    uf: dto.Uf,
                    areaTotalHectares: dto.AreaTotalHectares,
                    localizacao: geometry,
                    localizacaoGeoJson: geoJson);

                await _propriedadeRepository.Incluir(propriedade);

                var commitResult = await _unitOfWork.Commit();
                if (!commitResult)
                {
                    result.Add("Não foi possível salvar a propriedade.");
                    return result;
                }

                result.Result = await ToViewModel(propriedade);
                _logger.LogInformation("Propriedade incluída com sucesso. Id: {Id} ProducerId: {ProducerId}", result.Result?.Id, producerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incluir propriedade. ProducerId: {ProducerId}", producerId);
                result.Add("Erro ao incluir propriedade.");
            }

            return result;
        }

        public async Task<DomainNotificationsResult<PropriedadeViewModel>> Alterar(int producerId, bool isAdmin, PropriedadeDTO dto)
        {
            var result = new DomainNotificationsResult<PropriedadeViewModel>();
            _logger.LogInformation("Iniciando alteração de propriedade. Id: {Id} ProducerId: {ProducerId}", dto?.Id, producerId);

            try
            {
                if (dto == null || dto.Id <= 0)
                {
                    result.Add("Id inválido.");
                    return result;
                }

                var existente = await _propriedadeRepository.Selecionar(dto.Id);
                if (existente == null)
                {
                    result.Add("Propriedade não encontrada.");
                    return result;
                }

                if (!isAdmin && existente.ProducerId != producerId)
                {
                    result.Add("Acesso negado.");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(dto.DescricaoLocalizacao) && (!dto.Localizacao.HasValue || dto.Localizacao.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined))
                {
                    result.Add("Informe ao menos DescricaoLocalizacao ou Localizacao (GeoJSON).");
                    return result;
                }

                var (geometry, geoJson, geoError) = TryParseGeometry(dto.Localizacao, allowedTypes: new[] { "Point", "Polygon", "MultiPolygon" });
                if (geoError != null)
                {
                    result.Add(geoError);
                    return result;
                }

                existente.Atualizar(
                    nome: dto.Nome,
                    codigo: dto.Codigo,
                    descricaoLocalizacao: dto.DescricaoLocalizacao,
                    municipio: dto.Municipio,
                    uf: dto.Uf,
                    areaTotalHectares: dto.AreaTotalHectares,
                    localizacao: geometry,
                    localizacaoGeoJson: geoJson);
                _propriedadeRepository.Alterar(existente);

                var commitResult = await _unitOfWork.Commit();
                if (!commitResult)
                {
                    result.Add("Não foi possível salvar a propriedade.");
                    return result;
                }

                result.Result = await ToViewModel(existente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar propriedade. Id: {Id} ProducerId: {ProducerId}", dto?.Id, producerId);
                result.Add("Erro ao alterar propriedade.");
            }

            return result;
        }

        public async Task<DomainNotificationsResult<PropriedadeViewModel>> Excluir(int producerId, bool isAdmin, int id)
        {
            var result = new DomainNotificationsResult<PropriedadeViewModel>();
            _logger.LogInformation("Iniciando exclusão de propriedade. Id: {Id} ProducerId: {ProducerId}", id, producerId);

            try
            {
                var existente = await _propriedadeRepository.Selecionar(id);
                if (existente == null)
                {
                    result.Add("Propriedade não encontrada.");
                    return result;
                }

                if (!isAdmin && existente.ProducerId != producerId)
                {
                    result.Add("Acesso negado.");
                    return result;
                }

                var removida = await _propriedadeRepository.Excluir(id);
                if (removida == null)
                {
                    result.Add("Propriedade não encontrada.");
                    return result;
                }

                var commitResult = await _unitOfWork.Commit();
                if (!commitResult)
                {
                    result.Add("Não foi possível excluir a propriedade.");
                    return result;
                }

                result.Result = await ToViewModel(removida);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir propriedade. Id: {Id} ProducerId: {ProducerId}", id, producerId);
                result.Add("Erro ao excluir propriedade.");
            }

            return result;
        }

        public async Task<DomainNotificationsResult<PropriedadeViewModel>> Selecionar(int id)
        {
            var result = new DomainNotificationsResult<PropriedadeViewModel>();

            var propriedade = await _propriedadeRepository.Selecionar(id);
            if (propriedade == null)
            {
                result.Add("Propriedade não encontrada.");
                return result;
            }

            result.Result = await ToViewModel(propriedade);
            return result;
        }

        public async Task<DomainNotificationsResult<PagedList<PropriedadeViewModel>>> Listar(int producerId, int pageNumber, int pageSize)
        {
            var result = new DomainNotificationsResult<PagedList<PropriedadeViewModel>>();

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var paged = await _propriedadeRepository.ListarPorProducer(producerId, pageNumber, pageSize);
            var viewModels = new List<PropriedadeViewModel>(paged.Count);
            foreach (var item in paged) viewModels.Add(await ToViewModel(item));

            result.Result = new PagedList<PropriedadeViewModel>(
                items: viewModels,
                currentPage: paged.CurrentPage,
                totalPages: paged.TotalPages,
                pageSize: paged.PageSize,
                totalCount: paged.TotalCount);
            return result;
        }

        private (Geometry geometry, string geoJson, string error) TryParseGeometry(JsonElement? element, string[] allowedTypes)
        {
            if (!element.HasValue || element.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return (null, null, null);
            }

            try
            {
                var geoJson = element.Value.GetRawText();
                var reader = new GeoJsonReader();
                var geometry = reader.Read<Geometry>(geoJson);

                if (geometry == null)
                {
                    return (null, null, "GeoJSON inválido.");
                }

                if (geometry.SRID <= 0)
                    geometry.SRID = DefaultSrid;

                if (geometry.SRID != DefaultSrid)
                    return (null, null, $"SRID inválido. Use EPSG:{DefaultSrid}.");

                var type = geometry.GeometryType;
                if (allowedTypes != null && allowedTypes.Length > 0 && !allowedTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                    return (null, null, $"Tipo de geometria inválido. Permitido: {string.Join(", ", allowedTypes)}.");

                var isValid = new IsValidOp(geometry).IsValid;
                if (!isValid)
                    return (null, null, "Geometria inválida (topologia).");

                return (geometry, geoJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao parsear GeoJSON.");
                return (null, null, "GeoJSON inválido.");
            }
        }

        private Task<PropriedadeViewModel> ToViewModel(Propriedade propriedade)
        {
            var vm = _mapper.Map<PropriedadeViewModel>(propriedade);

            var geoJson = propriedade.LocalizacaoGeoJson;
            if (string.IsNullOrWhiteSpace(geoJson) && propriedade.Localizacao != null)
            {
                try
                {
                    var writer = new GeoJsonWriter();
                    geoJson = writer.Write(propriedade.Localizacao);
                }
                catch
                {
                    geoJson = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(geoJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(geoJson);
                    vm.Localizacao = doc.RootElement.Clone();
                }
                catch
                {
                    vm.Localizacao = null;
                }
            }

            return Task.FromResult(vm);
        }
    }
}
