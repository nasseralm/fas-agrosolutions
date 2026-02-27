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
    public class TalhaoService : ITalhaoService
    {
        private const int DefaultSrid = 4326;

        private readonly ITalhaoRepository _talhaoRepository;
        private readonly IPropriedadeRepository _propriedadeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TalhaoService> _logger;

        public TalhaoService(
            ITalhaoRepository talhaoRepository,
            IPropriedadeRepository propriedadeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TalhaoService> logger)
        {
            _talhaoRepository = talhaoRepository;
            _propriedadeRepository = propriedadeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DomainNotificationsResult<TalhaoViewModel>> Incluir(int producerId, bool isAdmin, TalhaoDTO dto)
        {
            var result = new DomainNotificationsResult<TalhaoViewModel>();
            _logger.LogInformation("Iniciando inclusão de talhão. ProducerId: {ProducerId} PropriedadeId: {PropriedadeId}", producerId, dto?.PropriedadeId);

            try
            {
                if (dto == null)
                {
                    result.Add("Payload inválido.");
                    return result;
                }

                var propriedade = await _propriedadeRepository.Selecionar(dto.PropriedadeId);
                if (propriedade == null)
                {
                    result.Add("Propriedade não encontrada.");
                    return result;
                }

                if (!isAdmin && propriedade.ProducerId != producerId)
                {
                    result.Add("Acesso negado.");
                    return result;
                }

                var (geometry, geoJson, geoError) = ParseDelimitacao(dto.Delimitacao);
                if (geoError != null)
                {
                    result.Add(geoError);
                    return result;
                }

                var talhao = new Talhao(
                    propriedadeId: dto.PropriedadeId,
                    producerId: propriedade.ProducerId,
                    nome: dto.Nome,
                    codigo: dto.Codigo,
                    cultura: dto.Cultura,
                    variedade: dto.Variedade,
                    safra: dto.Safra,
                    areaHectares: dto.AreaHectares,
                    delimitacao: geometry,
                    delimitacaoGeoJson: geoJson);

                await _talhaoRepository.Incluir(talhao);

                var commitResult = await _unitOfWork.Commit();
                if (!commitResult)
                {
                    result.Add("Não foi possível salvar o talhão.");
                    return result;
                }

                result.Result = await ToViewModel(talhao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incluir talhão. ProducerId: {ProducerId}", producerId);
                result.Add("Erro ao incluir talhão.");
            }

            return result;
        }

        public async Task<DomainNotificationsResult<TalhaoViewModel>> Alterar(int producerId, bool isAdmin, TalhaoDTO dto)
        {
            var result = new DomainNotificationsResult<TalhaoViewModel>();

            try
            {
                if (dto == null || dto.Id <= 0)
                {
                    result.Add("Id inválido.");
                    return result;
                }

                var existente = await _talhaoRepository.Selecionar(dto.Id);
                if (existente == null)
                {
                    result.Add("Talhão não encontrado.");
                    return result;
                }

                if (!isAdmin && existente.ProducerId != producerId)
                {
                    result.Add("Acesso negado.");
                    return result;
                }

                var (geometry, geoJson, geoError) = ParseDelimitacao(dto.Delimitacao);
                if (geoError != null)
                {
                    result.Add(geoError);
                    return result;
                }

                existente.Atualizar(
                    nome: dto.Nome,
                    codigo: dto.Codigo,
                    cultura: dto.Cultura,
                    variedade: dto.Variedade,
                    safra: dto.Safra,
                    areaHectares: dto.AreaHectares,
                    delimitacao: geometry,
                    delimitacaoGeoJson: geoJson);
                _talhaoRepository.Alterar(existente);

                var commitResult = await _unitOfWork.Commit();
                if (!commitResult)
                {
                    result.Add("Não foi possível salvar o talhão.");
                    return result;
                }

                result.Result = await ToViewModel(existente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar talhão. Id: {Id} ProducerId: {ProducerId}", dto?.Id, producerId);
                result.Add("Erro ao alterar talhão.");
            }

            return result;
        }

        public async Task<DomainNotificationsResult<TalhaoViewModel>> Excluir(int producerId, bool isAdmin, int id)
        {
            var result = new DomainNotificationsResult<TalhaoViewModel>();

            try
            {
                var existente = await _talhaoRepository.Selecionar(id);
                if (existente == null)
                {
                    result.Add("Talhão não encontrado.");
                    return result;
                }

                if (!isAdmin && existente.ProducerId != producerId)
                {
                    result.Add("Acesso negado.");
                    return result;
                }

                var removido = await _talhaoRepository.Excluir(id);
                if (removido == null)
                {
                    result.Add("Talhão não encontrado.");
                    return result;
                }

                var commitResult = await _unitOfWork.Commit();
                if (!commitResult)
                {
                    result.Add("Não foi possível excluir o talhão.");
                    return result;
                }

                result.Result = await ToViewModel(removido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir talhão. Id: {Id} ProducerId: {ProducerId}", id, producerId);
                result.Add("Erro ao excluir talhão.");
            }

            return result;
        }

        public async Task<DomainNotificationsResult<TalhaoViewModel>> Selecionar(int id)
        {
            var result = new DomainNotificationsResult<TalhaoViewModel>();
            var talhao = await _talhaoRepository.Selecionar(id);

            if (talhao == null)
            {
                result.Add("Talhão não encontrado.");
                return result;
            }

            result.Result = await ToViewModel(talhao);
            return result;
        }

        public async Task<DomainNotificationsResult<PagedList<TalhaoViewModel>>> ListarPorPropriedade(int producerId, bool isAdmin, int propriedadeId, int pageNumber, int pageSize)
        {
            var result = new DomainNotificationsResult<PagedList<TalhaoViewModel>>();

            var propriedade = await _propriedadeRepository.Selecionar(propriedadeId);
            if (propriedade == null)
            {
                result.Add("Propriedade não encontrada.");
                return result;
            }

            if (!isAdmin && propriedade.ProducerId != producerId)
            {
                result.Add("Acesso negado.");
                return result;
            }

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var paged = await _talhaoRepository.ListarPorPropriedade(propriedadeId, pageNumber, pageSize);
            var viewModels = new List<TalhaoViewModel>(paged.Count);
            foreach (var item in paged) viewModels.Add(await ToViewModel(item));

            result.Result = new PagedList<TalhaoViewModel>(
                items: viewModels,
                currentPage: paged.CurrentPage,
                totalPages: paged.TotalPages,
                pageSize: paged.PageSize,
                totalCount: paged.TotalCount);
            return result;
        }

        private (Geometry geometry, string geoJson, string error) ParseDelimitacao(JsonElement? element)
        {
            if (!element.HasValue || element.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return (null, null, "Delimitação (GeoJSON) é obrigatória.");
            }

            try
            {
                var geoJson = element.Value.GetRawText();
                var reader = new GeoJsonReader();
                var geometry = reader.Read<Geometry>(geoJson);

                if (geometry == null)
                    return (null, null, "GeoJSON inválido.");

                if (geometry.SRID <= 0)
                    geometry.SRID = DefaultSrid;

                if (geometry.SRID != DefaultSrid)
                    return (null, null, $"SRID inválido. Use EPSG:{DefaultSrid}.");

                var type = geometry.GeometryType;
                if (!new[] { "Polygon", "MultiPolygon" }.Contains(type, StringComparer.OrdinalIgnoreCase))
                    return (null, null, "Delimitação deve ser Polygon ou MultiPolygon.");

                var isValid = new IsValidOp(geometry).IsValid;
                if (!isValid)
                    return (null, null, "Geometria inválida (topologia).");

                if (geometry.Area <= 0)
                    return (null, null, "A delimitação deve possuir área > 0.");

                return (geometry, geoJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao parsear GeoJSON.");
                return (null, null, "GeoJSON inválido.");
            }
        }

        private Task<TalhaoViewModel> ToViewModel(Talhao talhao)
        {
            var vm = _mapper.Map<TalhaoViewModel>(talhao);

            var geoJson = talhao.DelimitacaoGeoJson;
            if (string.IsNullOrWhiteSpace(geoJson) && talhao.Delimitacao != null)
            {
                try
                {
                    var writer = new GeoJsonWriter();
                    geoJson = writer.Write(talhao.Delimitacao);
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
                    vm.Delimitacao = doc.RootElement.Clone();
                }
                catch
                {
                    vm.Delimitacao = null;
                }
            }

            return Task.FromResult(vm);
        }
    }
}
