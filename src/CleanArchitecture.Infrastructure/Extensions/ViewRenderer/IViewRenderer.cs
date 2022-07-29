using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Extensions.ViewRenderer
{
    public interface IViewRenderer
    {
        Task<string> RenderToStringAsync<TModel>(string viewName, TModel model, CancellationToken cancellationToken = default);
    }
}
