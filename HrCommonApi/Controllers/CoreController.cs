using AutoMapper;
using HrCommonApi.Controllers.Requests;
using HrCommonApi.Controllers.Responses;
using HrCommonApi.Database.Models.Base;
using HrCommonApi.Enums;
using HrCommonApi.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HrCommonApi.Controllers;

/// <summary>
/// The behemoth of all controllers. 
/// Contains all the plumbing for CRUD operations.
/// Also contains a base implementation for all CRUD operations.
/// </summary>
/// <typeparam name="TController">The type of the controller that inherits from the CoreController.</typeparam>
/// <typeparam name="TService">The type of the service interface that this controller uses.</typeparam>
/// <typeparam name="TEntity">The type of the entity that this controller, and service use.</typeparam>
/// <typeparam name="TSimpleResponse">The default response type for the core endpoint responses.</typeparam>
/// <typeparam name="TCreateRequest">The request type that gets used for the creation of a new object on this controller.</typeparam>
/// <typeparam name="TUpdateRequest">The request type that gets used for the updating of an existing object on this controller. (Patch and Put)</typeparam>
/// <param name="logger">The instance of a logger for this controller.</param>
/// <param name="mapper">The instance of IMapper used to map these the request > entity > response.</param>
/// <param name="coreService">The instance of the service, that implements CoreService with TEntity as type.</param>
[Route("api/v1/[controller]")]
public abstract class CoreController<TController, TService, TEntity, TSimpleResponse, TCreateRequest, TUpdateRequest>(ILogger<TController> logger, IMapper mapper, TService coreService) : ControllerBase
    where TController : ControllerBase
    where TService : ICoreService<TEntity>
    where TEntity : DbEntity
    where TSimpleResponse : IResponse
    where TCreateRequest : IRequest
    where TUpdateRequest : IRequest
{
    /// <summary>
    /// This is the core service that this controller uses. In the more specific implementation inheriting from the core controller, this service will be of the interface type provided.
    /// </summary>
    protected TService CoreService { get; } = coreService;
    /// <summary>
    /// This is the logger provided by the creation of the controller. It contains the type of the controller using it.
    /// </summary>
    protected ILogger<TController> Logger { get; } = logger;
    /// <summary>
    /// This is the mapper provided by the creation of the controller.
    /// </summary>
    protected IMapper Mapper { get; } = mapper;

    protected string GetFromClaim(string key, string defaultValue = "")
    {
        if (HttpContext.User.Identity == null || !HttpContext.User.Identity.IsAuthenticated)
            return defaultValue;

        return HttpContext.User.FindFirst(key)?.Value ?? defaultValue;
    }

    // Standard CRUD operations

    /// <summary>
    /// Returns all items with optional filtering by a list of Guids.
    /// </summary>
    /// <param name="ids">Optional parameter. Provide a list of Guids of items to retrieve.</param>
    /// <returns>A list of simple responses.</returns>
    [HttpGet]
    public virtual async Task<IActionResult> All([FromQuery] Guid[]? ids = null)
        => await AllToResponseModel<TSimpleResponse>(ids);

    /// <summary>
    /// Returns a single item that matched the Guid provided.
    /// </summary>
    /// <param name="id">Provide the Guid to use for the item retrieval.</param>
    /// <returns>A single simple response.</returns>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(Guid id)
        => await GetByIdToResponseModel<TSimpleResponse>(id);

    /// <summary>
    /// Returns the item that was created, if successfully.
    /// </summary>
    /// <param name="createRequest">The creation request related to this entity.</param>
    /// <returns>A single simple response.</returns>
    [HttpPost, Authorize(Policy = "Admin")]
    public virtual async Task<IActionResult> Create(TCreateRequest createRequest)
        => await CreateToResponseModel<TSimpleResponse>(createRequest);

    /// <summary>
    /// Update all fields on the object that has the provided Id. Returns the entity that was put, after update.
    /// </summary>
    /// <param name="id">The entity id to put the update on.</param>
    /// <param name="putRequest">The put update request related to this entity.</param>
    /// <returns>A single simple response.</returns>
    [HttpPut("{id}"), Authorize(Policy = "Admin")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] TUpdateRequest putRequest)
        => await UpdateToResponseModel<TSimpleResponse>(id, putRequest);

    /// <summary>
    /// Update all non null fields on the object that has the provided Id. Returns the entity that was patched, after update.
    /// </summary>
    /// <param name="id">The entity id to patch the update on.</param>
    /// <param name="patchRequest">The patch update request related to this entity.</param>
    /// <returns>A single simple response.</returns>
    [HttpPatch("{id}"), Authorize(Policy = "Admin")]
    public virtual async Task<IActionResult> Patch(Guid id, [FromBody] TUpdateRequest patchRequest)
        => await UpdateToResponseModel<TSimpleResponse>(id, patchRequest, true);

    /// <summary>
    /// Returns the entity that was deleted if successfully.
    /// </summary>
    /// <param name="id">The Guid for the item to be deleted.</param>
    /// <returns>A single simple response.</returns>
    [HttpDelete("{id}"), Authorize(Policy = "Admin")]
    public virtual async Task<IActionResult> Delete(Guid id)
        => await DeleteToResponseModel<TSimpleResponse>(id);

    // Default plumbing for handling requests and responses

    /// <summary>
    /// Handles the most core flow for every request. This handles logging. It is not recommended to use this function outside of the CoreController.
    /// </summary>
    /// <typeparam name="TResponseModel">The type of ResponseModel used.</typeparam>
    /// <typeparam name="TResultType">The result type used by the ServiceResult.</typeparam>
    /// <param name="ServiceAction">The action to be executed. This usually involves an call to the Service.</param>
    private async Task<IActionResult> HandleRequestFlow<TResponseModel>(
        Func<Task<ServiceResult<TResponseModel>>> serviceAction,
        Func<TResponseModel, Task<OkObjectResult>> resultAction
    ) where TResponseModel : class
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var serviceResponse = await serviceAction();

        Logger.Log((LogLevel)(int)serviceResponse.Response, message: $"{serviceResponse.Response} | {serviceResponse.Message}", exception: serviceResponse.Exception);

        return serviceResponse.Response switch
        {
            ServiceResponse.NotFound => NotFound(serviceResponse.Message ?? string.Empty),
            ServiceResponse.BadRequest => BadRequest(serviceResponse.Message ?? string.Empty),
            ServiceResponse.Exception => StatusCode(500, serviceResponse.Exception?.Message ?? string.Empty),
            ServiceResponse.NotImplemented => StatusCode(501, serviceResponse.Message ?? string.Empty),
            _ => await resultAction(serviceResponse.Result!),
        };
    }

    /// <summary>
    /// Handles the request flow for a service action that needs the response to be mapped.
    /// </summary>
    /// <typeparam name="TResponseModel">The Response model that the TResultType will be mapped to.</typeparam>
    /// <typeparam name="TResultType">The type of result the Service action responds with.</typeparam>
    /// <param name="serviceAction">The action that will result in the request response.</param>
    /// <returns>The IActionResult that will be returned to the client. Task is awaited with Task.FromResult to force it as async.</returns>
    protected async Task<IActionResult> HandleRequestFlow<TResponseModel, TResultType>(Func<Task<ServiceResult<TResultType>>> serviceAction) where TResultType : class
        => await HandleRequestFlow(serviceAction, async (result) => await Task.FromResult(Ok(Mapper.Map<TResponseModel>(result))));

    /// <summary>
    /// Handles the request flow for a service action that doesn't need the response to be mapped.
    /// </summary>
    /// <typeparam name="TResponseModel">The Response model that will be returned.</typeparam>
    /// <param name="serviceAction">The action that will result in a pre-mapped response.</param>
    /// <returns>The IActionResult that will be returned to the client. Task is awaited with Task.FromResult to force it as async.</returns>
    protected async Task<IActionResult> HandleRequestFlow<TResponseModel>(Func<Task<ServiceResult<TResponseModel>>> serviceAction) where TResponseModel : class
        => await HandleRequestFlow(serviceAction, async (result) => await Task.FromResult(Ok(result)));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponseModel"></typeparam>
    /// <param name="ids"></param>
    /// <returns></returns>
    protected async Task<IActionResult> AllToResponseModel<TResponseModel>(Guid[]? ids) where TResponseModel : IResponse
        => await HandleRequestFlow<List<TResponseModel>, List<TEntity>>(() => CoreService.Get(ids));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponseModel"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    protected async Task<IActionResult> GetByIdToResponseModel<TResponseModel>(Guid id) where TResponseModel : IResponse
        => await HandleRequestFlow<TResponseModel, TEntity>(() => CoreService.Get(id));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponseModel"></typeparam>
    /// <param name="createRequest"></param>
    /// <returns></returns>
    protected async Task<IActionResult> CreateToResponseModel<TResponseModel>(IRequest createRequest) where TResponseModel : IResponse
        => await HandleRequestFlow<TResponseModel, TEntity>(() => CoreService.Create(Mapper.Map<TEntity>(createRequest)));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponseModel"></typeparam>
    /// <param name="id"></param>
    /// <param name="updateRequest"></param>
    /// <param name="isPatch"></param>
    /// <returns></returns>
    protected async Task<IActionResult> UpdateToResponseModel<TResponseModel>(Guid id, IRequest updateRequest, bool isPatch = false) where TResponseModel : IResponse
        => await HandleRequestFlow<TResponseModel, TEntity>(() => CoreService.Update(id, updateRequest, isPatch));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponseModel"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    protected async Task<IActionResult> DeleteToResponseModel<TResponseModel>(Guid id) where TResponseModel : IResponse
        => await HandleRequestFlow<TResponseModel, TEntity>(() => CoreService.Delete(id));
}
