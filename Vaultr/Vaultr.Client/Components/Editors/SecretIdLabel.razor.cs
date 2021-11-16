using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Components.Panes;

namespace Vaultr.Client.Components.Editors
{
    public partial class SecretIdLabel
    {
        [Inject]
        private IMediator Mediator { get; set; } = null!;

        private async Task CopyAsync(MouseEventArgs e)
        {
            if (await Mediator.NotifyEventAsync(this, new PaneRequestEventArgs(typeof(ClonePane), EditContext, new ButtonContext(default, default))) == CrudType.Refresh)
            {
                Mediator.NotifyEvent(this, new CollectionRepositoryEventArgs(EditContext.CollectionAlias, EditContext.RepositoryAlias, default, default(string?), CrudType.Insert));
            }
        }
    }
}
