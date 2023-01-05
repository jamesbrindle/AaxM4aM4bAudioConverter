using audiamus.aaxconv.lib;
using System.ComponentModel;
using System.Windows.Forms;

namespace audiamus.aaxconv
{
    interface ISet
    {
        void Set(AaxFileItem fileItem);
    }

    class FileItemForm : Form, ISet
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IPreviewTitle Previewer { get; set; }
        public virtual void Set(AaxFileItem fileItem) { }
    }
}
