using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Jwtauth.Helpers
{
    public class ControlBundle
    {
        public List<VisualElement> Items;

        public ControlBundle(List<VisualElement> items)
        {
            Items = items;
        }

        public void Enable(bool affirmative = true)
        {
            Items.ForEach(o => o.IsEnabled = affirmative);
        }
    }
}
