#if TEXT_MESH_PRO

using TMPro;

namespace Silphid.Showzup.TextMeshPro
{
    public static class IViewTextMeshProExtensions
    {
        public static void Bind(this IView This, TextMeshProUGUI text, string value)
        {
            if (text != null)
                text.text = value;
        }
    }
}

#endif