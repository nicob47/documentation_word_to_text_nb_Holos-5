using System.Collections.Generic;
using H.Core.Models;

namespace H.Avalonia.Models;

public class Farm
{
    #region Fields


    #endregion

    #region Constructors

    public Farm()
    {
        this.Components = new List<ComponentBase>();
    }

    #endregion

    #region Properties

    public IList<ComponentBase> Components { get; set; }
    public ComponentBase SelectedComponent { get; set; } = null!;

    #endregion
}