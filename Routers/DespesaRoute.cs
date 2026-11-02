using Crud.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

/// <summary>Resposta paginada de despesas.</summary>
public sealed record DespesaCollectionResponse(
    IReadOnlyList<DespesaResponse> Data,
    PaginationMetadata Pagination,
    IReadOnlyList<HyperLink> Links);

/// <summary>Resposta de uma despesa e seus links de navegação.</summary>
public sealed record DespesaResourceResponse(DespesaResponse Data, IReadOnlyList<HyperLink> Links);

public static class DespesaRoute
{
    public static void MapDespesaRoutes(this WebApplication app)
    {
        const string prefixo = "despesas";
        
        var route = app.MapGroup($"/{prefixo}");
        
        route.MapGet("", async Task<IResult> (
            int? page,
            int? pageSize,
            int? year,
            int? month,
            CrudContext db,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (year.HasValue != month.HasValue ||
                month is < 1 or > 12 ||
                year is < 1 or > 9998)
            {
                return TypedResults.BadRequest(new
                {
                    message = "Informe ano e mês válidos para filtrar as despesas."
                });
            }

            var pageNumber = page is null or < 1 ? 1 : page.Value;
            var requestedPageSize = pageSize is null or < 1 ? 10 : pageSize.Value;
            var query = db.Despesas.AsNoTracking();

            if (year.HasValue && month.HasValue)
            {
                var startDate = new DateTimeOffset(
                    year.Value,
                    month.Value,
                    1,
                    0,
                    0,
                    0,
                    TimeSpan.FromHours(-3)).ToUniversalTime();
                var endDate = startDate.AddMonths(1);
                query = query.Where(d => d.DataDespesa >= startDate && d.DataDespesa < endDate);
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var totalPages = totalItems == 0
                ? 0
                : (int)Math.Ceiling(totalItems / (double)requestedPageSize);

            var despesas = await query
                .OrderByDescending(d => d.DataDespesa)
                .ThenBy(d => d.Id)
                .Skip((pageNumber - 1) * requestedPageSize)
                .Take(requestedPageSize)
                .Select(d => new DespesaResponse(
                    d.Id,
                    d.DataDespesa,
                    d.DescricaoDespesa,
                    d.ValorDespesa,
                    d.ModalidadePagamento,
                    d.CategoriaDespesa,
                    d.Ativo,
                    d.Created))
                .ToListAsync(cancellationToken);

            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            var monthQuery = year.HasValue
                ? $"&year={year.Value}&month={month.Value}"
                : string.Empty;
            var pageQuery = $"?page={pageNumber}&pageSize={requestedPageSize}{monthQuery}";
            var links = new List<HyperLink>
            {
                new("self", $"{baseUrl}/{prefixo}{pageQuery}", "GET"),
                new("collection", $"{baseUrl}/{prefixo}", "GET"),
                new("create", $"{baseUrl}/{prefixo}", "POST")
            };

            if (pageNumber > 1)
            {
                links.Add(new HyperLink("prev", $"{baseUrl}/{prefixo}?page={pageNumber - 1}&pageSize={requestedPageSize}{monthQuery}", "GET"));
            }

            if (pageNumber < totalPages)
            {
                links.Add(new HyperLink("next", $"{baseUrl}/{prefixo}?page={pageNumber + 1}&pageSize={requestedPageSize}{monthQuery}", "GET"));
            }

            if (totalPages > 0)
            {
                links.Add(new HyperLink("first", $"{baseUrl}/{prefixo}?page=1&pageSize={requestedPageSize}{monthQuery}", "GET"));
                links.Add(new HyperLink("last", $"{baseUrl}/{prefixo}?page={totalPages}&pageSize={requestedPageSize}{monthQuery}", "GET"));
            }

            return TypedResults.Ok(new DespesaCollectionResponse(
                despesas,
                new PaginationMetadata(pageNumber, requestedPageSize, totalItems, totalPages),
                links));
        })
        .WithName("GetDespesas")
        .WithSummary("Lista despesas")
        .WithDescription("Retorna despesas registradas em ordem decrescente de data.")
        .Produces<DespesaCollectionResponse>()
        .RequireAuthorization("ValidarRequisitosPerfil");

        route.MapPost("", async Task<IResult> (
            CriarDespesaRequest input,
            CrudContext db,
            HttpContext httpContext,
            IValidator<CriarDespesaRequest> validator,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(input, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(new
                {
                    message = "Erros de validação encontrados.",
                    errors = validation.ToDictionary()
                });
            }

            var despesa = new DespesaModel
            {
                Id = Guid.NewGuid(),
                DataDespesa = input.DataDespesa.ToUniversalTime(),
                DescricaoDespesa = input.DescricaoDespesa.Trim(),
                ValorDespesa = input.ValorDespesa,
                ModalidadePagamento = input.ModalidadePagamento.Trim(),
                CategoriaDespesa = input.CategoriaDespesa.Trim()
            };

            db.Despesas.Add(despesa);
            await db.SaveChangesAsync(cancellationToken);

            var response = ToResponse(despesa);
            var links = Links.GenerateLinks(httpContext, despesa.Id, prefixo);
            return TypedResults.Created(
                $"{Links.BaseUrl(httpContext)}/{prefixo}/{despesa.Id}",
                new DespesaResourceResponse(response, links));
        })
        .WithName("PostDespesas")
        .WithSummary("Registra uma despesa")
        .WithDescription("Persiste a data, descrição, valor, modalidade de pagamento e categoria da despesa.")
        .Produces<DespesaResourceResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("ValidarRequisitosPerfil");

        route.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            CrudContext db,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var despesa = await db.Despesas
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (despesa is null)
            {
                return TypedResults.NotFound();
            }

            var links = Links.GenerateLinks(httpContext, despesa.Id, prefixo);
            return TypedResults.Ok(new DespesaResourceResponse(ToResponse(despesa), links));
        })
        .WithName("GetDespesasById")
        .WithSummary("Busca uma despesa")
        .WithDescription("Retorna uma despesa pelo identificador.")
        .Produces<DespesaResourceResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization("ValidarRequisitosPerfil");

        route.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            AtualizarDespesaRequest input,
            CrudContext db,
            HttpContext httpContext,
            IValidator<(Guid Id, AtualizarDespesaRequest Input)> validator,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync((id, input), cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(new
                {
                    message = "Erros de validação encontrados.",
                    errors = validation.ToDictionary()
                });
            }

            var despesa = await db.Despesas.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (despesa is null)
            {
                return TypedResults.NotFound();
            }

            Apply(despesa, input);
            await db.SaveChangesAsync(cancellationToken);

            var links = Links.GenerateLinks(httpContext, despesa.Id, prefixo);
            return TypedResults.Ok(new DespesaResourceResponse(ToResponse(despesa), links));
        })
        .WithName("PutDespesasById")
        .WithSummary("Atualiza uma despesa")
        .WithDescription("Substitui todos os dados de uma despesa existente.")
        .Produces<DespesaResourceResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization("ValidarRequisitosPerfil");

        route.MapPatch("/{id:guid}", async Task<IResult> (
            Guid id,
            AtualizarParcialDespesaRequest input,
            CrudContext db,
            HttpContext httpContext,
            IValidator<AtualizarParcialDespesaRequest> validator,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(input, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(new
                {
                    message = "Erros de validação encontrados.",
                    errors = validation.ToDictionary()
                });
            }

            var despesa = await db.Despesas.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (despesa is null)
            {
                return TypedResults.NotFound();
            }

            if (input.DataDespesa.HasValue) despesa.DataDespesa = input.DataDespesa.Value.ToUniversalTime();
            if (input.DescricaoDespesa is not null) despesa.DescricaoDespesa = input.DescricaoDespesa.Trim();
            if (input.ValorDespesa.HasValue) despesa.ValorDespesa = input.ValorDespesa.Value;
            if (input.ModalidadePagamento is not null) despesa.ModalidadePagamento = input.ModalidadePagamento.Trim();
            if (input.CategoriaDespesa is not null) despesa.CategoriaDespesa = input.CategoriaDespesa.Trim();

            await db.SaveChangesAsync(cancellationToken);

            var links = Links.GenerateLinks(httpContext, despesa.Id, prefixo);
            return TypedResults.Ok(new DespesaResourceResponse(ToResponse(despesa), links));
        })
        .WithName("PatchDespesasById")
        .WithSummary("Atualiza parcialmente uma despesa")
        .WithDescription("Altera somente os campos enviados para a despesa.")
        .Produces<DespesaResourceResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization("ValidarRequisitosPerfil");

        route.MapDelete("/{id:guid}", async Task<IResult> (
            Guid id,
            CrudContext db,
            CancellationToken cancellationToken) =>
        {
            var despesa = await db.Despesas.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (despesa is null)
            {
                return TypedResults.NotFound();
            }

            db.Despesas.Remove(despesa);
            await db.SaveChangesAsync(cancellationToken);
            return TypedResults.NoContent();
        })
        .WithName("DeleteDespesasById")
        .WithSummary("Exclui uma despesa")
        .WithDescription("Remove uma despesa pelo identificador.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization("ValidarRequisitosPerfil");


        route.MapPost("/analisar-comprovante", async Task<IResult> (
            HttpRequest request,
            IReceiptAnalysisService receiptAnalysisService,
            CancellationToken cancellationToken) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { message = "Envie o comprovante como multipart/form-data." });
            }

            var form = await request.ReadFormAsync(cancellationToken);
            var arquivo = form.Files.GetFile("receipt");

            if (arquivo is null || arquivo.Length == 0)
            {
                return Results.BadRequest(new { message = "Imagem do comprovante não fornecida." });
            }

            if (arquivo.Length > 10 * 1024 * 1024)
            {
                return Results.Problem(
                    "O comprovante deve ter no máximo 10 MB.",
                    statusCode: StatusCodes.Status413PayloadTooLarge);
            }

            var contentType = arquivo.ContentType.ToLowerInvariant();
            if (contentType is not ("image/jpeg" or "image/png" or "image/webp"))
            {
                return Results.BadRequest(new
                {
                    message = "Formato de imagem não suportado. Envie JPEG, PNG ou WEBP."
                });
            }

            await using var stream = arquivo.OpenReadStream();
            var result = await receiptAnalysisService.AnalyzeAsync(
                stream,
                contentType,
                cancellationToken);

            return Results.Ok(result);
        })
        .WithName("AnalyzeReceipt")
        .WithSummary("Analisa um comprovante de despesa")
        .WithDescription("Extrai os dados da imagem e retorna um rascunho de despesa.")
        .Produces<ReceiptAnalysisResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status413PayloadTooLarge);
    }

    private static DespesaResponse ToResponse(DespesaModel despesa) =>
        new(
            despesa.Id,
            despesa.DataDespesa,
            despesa.DescricaoDespesa,
            despesa.ValorDespesa,
            despesa.ModalidadePagamento,
            despesa.CategoriaDespesa,
            despesa.Ativo,
            despesa.Created);

    private static void Apply(DespesaModel despesa, AtualizarDespesaRequest input)
    {
        despesa.DataDespesa = input.DataDespesa.ToUniversalTime();
        despesa.DescricaoDespesa = input.DescricaoDespesa.Trim();
        despesa.ValorDespesa = input.ValorDespesa;
        despesa.ModalidadePagamento = input.ModalidadePagamento.Trim();
        despesa.CategoriaDespesa = input.CategoriaDespesa.Trim();
    }
}
