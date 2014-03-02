using System.Windows.Controls;
using Procurement.ViewModel;
using Procurement.Controls;

namespace Procurement.View
{
    public partial class InventoryView : IView
    {
        public InventoryView()
        {
            InitializeComponent();
            this.DataContext = new InventoryViewModel(this);
        }

        public new Grid Content
        {
            get { return this.ViewContent; }
        }
    }
}
