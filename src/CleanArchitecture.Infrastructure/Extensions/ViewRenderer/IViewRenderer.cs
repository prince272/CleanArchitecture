namespace CleanArchitecture.Infrastructure.Extensions.ViewRenderer
{
    public interface IViewRenderer
    {
        Task<string> RenderToStringAsync<TModel>(string viewName, TModel model, CancellationToken cancellationToken = default);
    }
}
